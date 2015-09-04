﻿using System;
using System.Collections;
using System.Text;
using Infovision.Data;
using Infovision.Datamining;
using Infovision.Datamining.Roughset;
using Infovision.Test;
using Infovision.Utils;

namespace Infovision.RunTest
{
    [Serializable]
    public class RoughMeasureTest : ITestRunable, IEnumerator
    {
        #region Globals

        private ParameterList parameterList;
        private string[] parmNames;
        private ParameterVectorEnumerator i_parm;

        private DataStoreSplitter dataSplitter = null;
        private RoughClassifier roughClassifier = null;
        private ClassificationResult result = null;
        
        private PermutationList permutationList = null;

        private DataStore dataStoreTrain = null;
        private DataStore dataStoreTest = null;
        private int numberOfReducts = -1;
        private int numberOfPermutations = -1;
        private int foldNumber = -1;
        private int nfold = -1;
        private int testNumber = -1;
        private int epsilon = -1;
        private string reductFactoryKey = String.Empty;
        private string reductMeasureKey = String.Empty;
        
        private IdentificationType identificationType;
        private VoteType voteType;

        private DataStore lastDataStoreTrain = null;
        private DataStore lastDataStoreTest = null;
        private int lastNumberOfReducts = -1;
        private int lastNumberOfPermutations = -1;
        private int lastFoldNumber = -1;
        private int lastTestNumber = -1;
        private int lastEpsilon = -1;
        private string lastReductMeasureKey = String.Empty;
        private string lastReductFactoryKey = String.Empty;
        
        private bool isTrained = false;

        private DataStore localDataStoreTest = null;
        private DataStore localDataStoreTrain = null;
        private IReductStore localReductStore = null;

        #endregion

        #region Constructor

        public RoughMeasureTest(ParameterList parameterList)
        {
            this.parameterList = parameterList;
            this.parmNames = parameterList.GetParameterNames();
            
            this.i_parm = (ParameterVectorEnumerator)parameterList.ParmValueEnumerator;
        }

        public RoughMeasureTest(ITestParameter[] parameters)
            : this(new ParameterList(parameters))
        {
        }

        #endregion

        #region Properties

        public DataStore DataStoreTest
        {
            get { return this.DataStoreTest; }
        }

        public DataStore DataStoreTrain
        {
            get { return this.dataStoreTrain; }
        }

        public IReductStore ReductStore
        {
            get { return this.localReductStore; }
        }

        #endregion

        #region Methods

        private bool CheckRetrain()
        {
            if (this.isTrained == false
                || this.lastEpsilon != this.epsilon
                || this.lastNumberOfPermutations != this.numberOfPermutations
                || String.IsNullOrEmpty(this.lastReductFactoryKey)
                || this.lastReductFactoryKey != this.reductFactoryKey
                || this.lastFoldNumber != this.foldNumber)
            {
                return true;
            }

            return false;
        }

        private bool CheckReclassify()
        {
            if (this.CheckRetrain()
                || String.IsNullOrEmpty(this.lastReductMeasureKey)
                || this.lastReductMeasureKey != this.reductMeasureKey
                || this.lastNumberOfReducts != this.numberOfReducts)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// checks if the cache entries should be removed
        /// </summary>
        /// <returns></returns>
        private bool CheckTrimCache()
        {
            if (this.lastFoldNumber != this.foldNumber
                || this.lastTestNumber != this.testNumber
                || this.lastNumberOfPermutations != this.numberOfPermutations
                || String.IsNullOrEmpty(this.lastReductFactoryKey)
                || this.lastReductFactoryKey != this.reductFactoryKey)
            {
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Checks if a new permutation list should be generated
        /// </summary>
        /// <returns></returns>
        private bool CheckRegeneratePermutation()
        {
            //TODO if epsilon does not change but reductType changes, in case of bireducts we can use the same permutation for Bireduct and Gammabireduct            
            if (this.lastTestNumber != this.testNumber
                || ((this.reductFactoryKey == "Bireduct" 
                    || this.reductFactoryKey == "GammaBireduct" 
                    || this.reductFactoryKey == "BireductRelative")
                        && (this.lastEpsilon != this.epsilon 
                            || this.lastFoldNumber != this.foldNumber))
                || this.lastNumberOfPermutations != this.numberOfPermutations)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// checks if we need to generate data split for cross validation
        /// </summary>
        /// <returns></returns>
        private bool CheckGenerateSplit()
        {
            if (this.dataSplitter == null
                || this.lastTestNumber != this.testNumber)
                return true;
            return false;
        }
        
        private void ForceTraining(bool forceTraining)
        {
            if (forceTraining)
            {
                this.isTrained = false;            
            }
        }

        public void Run()
        {
            ParameterValueVector parameterVector = (ParameterValueVector)this.Current;
            Args args = new Args(parmNames, parameterVector.GetArray());
            this.TestRun(args);
        }
        
        private void TestRun(Args args)
        {
            this.SetParameters(args);
            
            if (nfold > 1)
            {
                if (this.CheckGenerateSplit())
                {
                    this.dataSplitter = new DataStoreSplitter(dataStoreTrain, nfold);
                }

                if (this.lastFoldNumber != this.foldNumber)
                {
                    dataSplitter.ActiveFold = foldNumber;
                    dataSplitter.Split(ref localDataStoreTrain, ref localDataStoreTest);
                }
                else
                {
                    localDataStoreTrain = lastDataStoreTrain;                    
                    localDataStoreTest = lastDataStoreTest;
                }
            }
            else
            {
                localDataStoreTrain = dataStoreTrain;
                localDataStoreTest = dataStoreTest;
            }

            if (this.CheckRegeneratePermutation())
            {
                Args parms = new Args(new string[] { "DataStore", "ApproximationRatio" }, new object[] { localDataStoreTrain, epsilon });
                this.permutationList = ReductFactory.GetPermutationGenerator(reductFactoryKey, parms).Generate(numberOfPermutations);
                this.ForceTraining(true);
            }

            if (this.CheckTrimCache())
            {
                ReductCache.Instance.Trim(100);
                
                foreach (var element in ReductCache.Instance)
                {
                    ReductCache.Instance.Remove(element.Key);
                }
            }

            if (this.CheckRetrain())
            {
                roughClassifier = new RoughClassifier();
                roughClassifier.Train(localDataStoreTrain, reductFactoryKey, epsilon, permutationList);
            }

            if (this.CheckReclassify())
            {
                localReductStore = roughClassifier.Classify(localDataStoreTest, reductMeasureKey, numberOfReducts);
            }

            result = roughClassifier.Vote(localDataStoreTest, identificationType, voteType);
            result.QualityRatio = localReductStore.GetAvgMeasure(ReductFactory.GetReductMeasure(reductMeasureKey));

            this.SaveLast();
        }

        private void SaveLast()
        {
            this.isTrained = true;
            
            this.lastTestNumber = testNumber;
            this.lastFoldNumber = foldNumber;
            this.lastEpsilon = epsilon;
            this.lastNumberOfReducts = numberOfReducts;
            this.lastReductFactoryKey = reductFactoryKey;
            this.lastReductMeasureKey = reductMeasureKey;
            this.lastNumberOfPermutations = numberOfPermutations;
            this.lastDataStoreTrain = localDataStoreTrain;
            this.lastDataStoreTest = localDataStoreTest;
        }

        public object GetResult()
        {
            return result;
        }

        //TODO write parms in this order
        /*
        parmDataStoreTraining,//0
        parmDataStoreTest,//1
        parmNumberOfReducts,//2
        parmNumberOfPermutations,//3
        parmNFold,//4
        parmFoldNumber,//5
        parmTestNumber,//6
        parmReductType,//7
        parmEpsilon,//8
        parmReductMeasure,//9
        parmIdentification,//10
        parmVote//11
        */

        public string GetResultHeader()
        {            
            StringBuilder parms = new StringBuilder();

            parms.Append("DateTime").Append('\t');
            parms.Append("TrainName").Append('\t');
            parms.Append("TrainInfo").Append('\t');
            parms.Append("TestName").Append('\t');
            parms.Append("TestInfo").Append('\t');
            parms.Append("NumberOfReducts").Append('\t');
            parms.Append("NumberOfPermutations").Append('\t');
            parms.Append("ReductMeasureType").Append('\t');
            parms.Append("NumberOfFolds").Append('\t');
            
            parms.Append("TestNumber").Append('\t');
            parms.Append("FoldNumber").Append('\t');
            
            parms.Append("Epsilon").Append('\t');
            parms.Append("InformationMeasure").Append('\t');

            parms.Append("IdentificationType").Append('\t');
            parms.Append("VoteType").Append('\t');

            parms.Append("RuleVoteCenseqentRating").Append('\t');
            parms.Append("RuleVoteAntecedentRating").Append('\t');

            parms.Append(result.ResultHeader());

            return parms.ToString();
        }
        
        public string GetResultInfo()
        {
            StringBuilder parms = new StringBuilder();

            parms.AppendFormat("{0:s}", System.DateTime.Now).Append('\t');
            parms.Append(lastDataStoreTrain.ToString()).Append('\t');
            parms.Append(lastDataStoreTrain.DataStoreInfo.ToString()).Append('\t');
            parms.Append(lastDataStoreTest.ToString()).Append('\t');
            parms.Append(lastDataStoreTest.DataStoreInfo.ToString()).Append('\t');
            parms.Digits(numberOfReducts).Append('\t');
            parms.Digits(numberOfPermutations).Append('\t');
            parms.Append(reductMeasureKey).Append('\t');
            parms.Digits(nfold).Append('\t');

            parms.Digits(testNumber).Append('\t');
            parms.Digits(foldNumber).Append('\t');
            
            parms.Digits(epsilon).Append('\t');
            parms.Append(reductFactoryKey).Append('\t');
            parms.Append(identificationType).Append('\t');
            parms.Append(voteType).Append('\t');

            parms.Append(Infovision.Datamining.Roughset.EnumHelper.VoteType2RuleVoteConseqentRating(voteType)).Append('\t');
            parms.Append(Infovision.Datamining.Roughset.EnumHelper.VoteType2RuleVoteAntecedentRating(voteType)).Append('\t');
            
            parms.Append(result.ToString()).Append('\t');

            return parms.ToString();
        }

        protected void SetParameters(Args args)
        {
            dataStoreTrain = (DataStore)args.GetParameter("DataStoreTraining");
            dataStoreTest = (DataStore)args.GetParameter("DataStoreTest");
            numberOfReducts = (int)args.GetParameter("NumberOfReducts");
            numberOfPermutations = (int)args.GetParameter("NumberOfPermutations");
            nfold = (int)args.GetParameter("NumberOfFolds");
            foldNumber = (int)args.GetParameter("FoldNumber");
            testNumber = (int)args.GetParameter("NumberOfTests");
            reductFactoryKey = (string)args.GetParameter("ReductType");
            epsilon = (int)args.GetParameter("ApproximationDegree");
            reductMeasureKey = (string)args.GetParameter("ReductMeasure");
            identificationType = (IdentificationType)args.GetParameter("IdentificationType");
            voteType = (VoteType)args.GetParameter("VoteType");
        }

        public bool MoveNext()
        {
            return i_parm.MoveNext();
        }

        public void Reset()
        {
            i_parm.Reset();
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public ParameterValueVector Current
        {
            get
            {
                try
                {
                    return (ParameterValueVector) i_parm.Current;
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        #endregion
    }
}
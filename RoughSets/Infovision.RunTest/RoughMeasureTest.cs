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
        #region Members

        private ParameterCollection parameterList;
        private string[] parmNames;
        private ParameterVectorEnumerator i_parm;

        private DataStoreSplitter dataSplitter = null;
        private RoughClassifier roughClassifier = null;
        private ClassificationResult result = null;
        
        private PermutationCollection permutationList = null;

        private DataStore dataStoreTrain = null;
        private DataStore dataStoreTest = null;
        private int numberOfReducts = -1;
        private int numberOfPermutations = -1;
        private int foldNumber = -1;
        private int nfold = -1;
        private int testNumber = -1;
        private decimal epsilon = -1;
        private string reductFactoryKey = String.Empty;
        private string reductMeasureKey = String.Empty;
        
        private RuleQualityFunction identificationType;
        private RuleQualityFunction voteType;

        private DataStore lastDataStoreTrain = null;
        private DataStore lastDataStoreTest = null;
        private int lastNumberOfReducts = -1;
        private int lastNumberOfPermutations = -1;
        private int lastFoldNumber = -1;
        private int lastTestNumber = -1;
        private decimal lastEpsilon = -1;
        private string lastReductMeasureKey = String.Empty;
        private string lastReductFactoryKey = String.Empty;
        
        private bool isTrained = false;

        private DataStore localDataStoreTest = null;
        private DataStore localDataStoreTrain = null;
        //private IReductStore exceptionStore = null;
        private IReductStoreCollection localReductStoreCollection = null;

        #endregion

        #region Constructor

        public RoughMeasureTest(ParameterCollection parameterList)
        {
            this.parameterList = parameterList;
            this.parmNames = parameterList.GetParameterNames();
            
            this.i_parm = (ParameterVectorEnumerator)parameterList.ParmValueEnumerator;
        }

        public RoughMeasureTest(ITestParameter[] parameters)
            : this(new ParameterCollection(parameters))
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

        public IReductStoreCollection ReductStoreCollection
        {
            get { return this.localReductStoreCollection; }
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
                || ((this.reductFactoryKey == ReductFactoryKeyHelper.Bireduct
                    || this.reductFactoryKey == ReductFactoryKeyHelper.GammaBireduct 
                    || this.reductFactoryKey == ReductFactoryKeyHelper.BireductRelative)
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
            
            if (nfold > 0)
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
                Args parms = new Args(
                    new string[] { 
                        ReductGeneratorParamHelper.FactoryKey, 
                        ReductGeneratorParamHelper.DataStore, 
                        ReductGeneratorParamHelper.Epsilon }, 
                    new object[] { 
                        reductFactoryKey, 
                        localDataStoreTrain, 
                        epsilon});

                this.permutationList = ReductFactory.GetPermutationGenerator(parms).Generate(numberOfPermutations);
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
                Args localArgs = new Args();
                localArgs.SetParameter(ReductGeneratorParamHelper.DataStore, localDataStoreTrain);
                localArgs.SetParameter(ReductGeneratorParamHelper.Epsilon, epsilon);
                localArgs.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permutationList);
                localArgs.SetParameter(ReductGeneratorParamHelper.FactoryKey, reductFactoryKey);
                localArgs.SetParameter(ReductGeneratorParamHelper.DataStore, localDataStoreTrain);

                IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
                reductGenerator.Generate();

                localReductStoreCollection = reductGenerator.GetReductStoreCollection();

                //TODO reductMeasureKey, numberOfReducts
            }

            roughClassifier = new RoughClassifier(
                    localReductStoreCollection,
                    identificationType,
                    voteType,
                    localDataStoreTrain.DataStoreInfo.GetDecisionValues());
            result = roughClassifier.Classify(localDataStoreTest, null);            

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
            parms.Append(ReductGeneratorParamHelper.NumberOfReducts).Append('\t');
            parms.Append(ReductGeneratorParamHelper.NumberOfPermutations).Append('\t');
            parms.Append("ReductMeasureType").Append('\t');
            parms.Append("NumberOfFolds").Append('\t');
            
            parms.Append("TestNumber").Append('\t');
            parms.Append("FoldNumber").Append('\t');

            parms.Append(ReductGeneratorParamHelper.Epsilon).Append('\t');
            parms.Append("InformationMeasure").Append('\t');

            parms.Append(ReductGeneratorParamHelper.IdentificationType).Append('\t');
            parms.Append(ReductGeneratorParamHelper.VoteType).Append('\t');            

            parms.Append(ClassificationResult.ResultHeader());

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
            
            parms.Append(epsilon).Append('\t');
            parms.Append(reductFactoryKey).Append('\t');
            parms.Append(identificationType.Method.Name).Append('\t');
            parms.Append(voteType.Method.Name).Append('\t');            
            
            parms.Append(result.ToString()).Append('\t');

            return parms.ToString();
        }

        protected void SetParameters(Args args)
        {
            dataStoreTrain = (DataStore)args.GetParameter(ReductGeneratorParamHelper.DataStore);
            dataStoreTest = (DataStore)args.GetParameter("DataStoreTest");
            numberOfReducts = (int)args.GetParameter(ReductGeneratorParamHelper.NumberOfReducts);
            numberOfPermutations = (int)args.GetParameter(ReductGeneratorParamHelper.NumberOfPermutations);
            nfold = (int)args.GetParameter("NumberOfFolds");
            foldNumber = (int)args.GetParameter("FoldNumber");
            testNumber = (int)args.GetParameter("NumberOfTests");
            reductFactoryKey = (string)args.GetParameter(ReductGeneratorParamHelper.FactoryKey);
            epsilon = (decimal)args.GetParameter(ReductGeneratorParamHelper.Epsilon);
            reductMeasureKey = (string)args.GetParameter("ReductMeasure");
            identificationType = (RuleQualityFunction)args.GetParameter(ReductGeneratorParamHelper.IdentificationType);
            voteType = (RuleQualityFunction)args.GetParameter(ReductGeneratorParamHelper.VoteType);
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

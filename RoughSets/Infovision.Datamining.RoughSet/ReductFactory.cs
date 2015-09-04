using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    public interface IFactoryProduct
    {
        String FactoryKey { get; }
    }
    
    public interface IReductFactory : IFactoryProduct
    {
        IReductGenerator GetReductGenerator(Args args);
        IPermutationGenerator GetPermutationGenerator(Args args);
    }

    public class ReductFactory
    {
        private static volatile ReductFactory reductFactoryInstance = null;
        private static object syncRoot = new object();        

        ListDictionary registeredReductFactories = new ListDictionary();
        ListDictionary registeredReductMeasures = new ListDictionary();

        private ReductFactory()
        {
            this.Initialize();
        }

        private void Initialize()
        {
            Assembly callingAssembly = Assembly.GetCallingAssembly();

            Type[] assemblyTypes = callingAssembly.GetTypes();
            foreach (Type type in assemblyTypes)
            {
                if (type.IsClass && !type.IsAbstract)
                {
                    this.RegisterFactory(type);
                }
            }            
        }

        private void RegisterFactory(Type type)
        {
            Type iReductFactory = type.GetInterface("IReductFactory");
            if (iReductFactory == null)
            {
                iReductFactory = type.GetInterface("IReductMeasure");
            }

            if (iReductFactory != null)
            {
                this.RegisterFactoryFromType(type, iReductFactory.Name);
            }
        }

        public void RegisterFactory(string className)
        {            
            StringBuilder assemblyName = new StringBuilder();
            string [] assemblyNameParts = className.Split(new Char[] {'.'});
            for (int i = 0; i < assemblyNameParts.Length - 1; i++)
            {
                assemblyName.Append(assemblyNameParts[i]);
                if (i < assemblyNameParts.Length - 2)
                {
                    assemblyName.Append('.');
                }
            }

            Assembly assembly = null;
            try
            {
                assembly = Assembly.Load(assemblyName.ToString());
            }
            catch (FileNotFoundException e)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Cannot find assembly {0} ({1})", assemblyName.ToString(), e.Message), "className");
            }

            Type type = assembly.GetType(className);
            if (type == null)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Class {0} does not exist", className), "className");
            }

            this.RegisterFactory(type);
        }

        private void RegisterFactoryFromType(Type type, string interfaceName)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type", "Type is null");
            }

            if (type.IsClass && !type.IsAbstract)
            {
                Type iReductFactory = type.GetInterface(interfaceName);
                if (iReductFactory != null)
                {
                    object factoryInstance = type.Assembly.CreateInstance(type.FullName, true,
                        BindingFlags.CreateInstance, null, null, null, null);

                    if (factoryInstance != null)
                    {
                        IFactoryProduct keyDesc = (IFactoryProduct)factoryInstance;
                        string key = keyDesc.FactoryKey;
                        factoryInstance = null;

                        if (interfaceName == "IReductFactory")
                        {
                            this.registeredReductFactories.Add(key, type);
                        }
                        else if (interfaceName == "IReductMeasure")
                        {
                            this.registeredReductMeasures.Add(key, type);
                        }
                    }
                    else
                    {
                        throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Class {0} should implement default empty contructor", type.Name), "type");
                    }
                }
                else
                {
                    throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Class {0} does not implement interface {1}", type.Name, "IReductFactory"), "type");
                }
            }            
        }

        public static string[] GetReductFactoryKeys()
        {
            string [] keys = new String[ReductFactory.Instance.registeredReductFactories.Keys.Count];
            ReductFactory.Instance.registeredReductFactories.Keys.CopyTo(keys, 0);
            return keys;
        }

        public static string[] GetReductMeasureKeys()
        {
            String[] keys = new String[ReductFactory.Instance.registeredReductMeasures.Keys.Count];
            ReductFactory.Instance.registeredReductMeasures.Keys.CopyTo(keys, 0);
            return keys;
        }

        public static IReductFactory GetReductFactory(string key)
        {
            if (String.IsNullOrEmpty(key))
                throw new ArgumentNullException("key", "Invalid key supplied, must be non-empty string.");

            Type type = (Type)ReductFactory.Instance.registeredReductFactories[key];
                        
            if (type != null)
            {
                object reductFactory = type.Assembly.CreateInstance(type.FullName, 
                                        true, 
                                        BindingFlags.CreateInstance, 
                                        null, 
                                        null, 
                                        null, 
                                        null);

                if (reductFactory == null)
                    throw new InvalidOperationException("Null factory instance. Unable to create necessary reduct factory class.");

                IReductFactory iReductFactory = (IReductFactory) reductFactory;
                return iReductFactory;
            }
            else
            {
                throw new InvalidOperationException("Factory was not registered. Unable to create necessary reduct factory class.");
            }
        }

        public static IReductGenerator GetReductGenerator(string factoryKey, Args args)
        {
            IReductGenerator reductGenerator = (IReductGenerator)ReductFactory.GetReductFactory(factoryKey).GetReductGenerator(args);
            return reductGenerator;
        }

        public static IPermutationGenerator GetPermutationGenerator(string factoryKey, Args args)
        {
            return (IPermutationGenerator)ReductFactory.GetReductFactory(factoryKey).GetPermutationGenerator(args);
        }

        public static IReductMeasure GetReductMeasure(string measureKey)
        {
            if (String.IsNullOrEmpty(measureKey))
                return null;

            Type type = (Type)ReductFactory.Instance.registeredReductMeasures[measureKey];

            if (type != null)
            {
                object reductMeasure = type.Assembly.CreateInstance(type.FullName,
                                        true,
                                        BindingFlags.CreateInstance,
                                        null,
                                        null,
                                        null,
                                        null);

                if (reductMeasure == null)
                    throw new InvalidOperationException("Null reduct measure instance. Unable to create necessary reduct measure class.");

                IReductMeasure iReductMeasure = (IReductMeasure)reductMeasure;
                return iReductMeasure;
            }
            else
            {
                throw new InvalidOperationException("Factory was not registered. Unable to create necessary reduct factory class.");
            }
        }

        //TODO use registered classes
        public static Comparer<IReduct> GetReductComparer(String measureKey)
        {
            Comparer<IReduct> comparer;

            switch(measureKey)
            {
                case "ReductMeasureLength":
                    comparer = new ReductLenghtComparer();
                    break;

                case "ReductMeasureNumberOfPartitions":
                    comparer = new ReductRuleNumberComparer();
                    break;

                case "BireductMeasureMajority":
                    comparer = new BireductSizeComparer();
                    break;

                case "BireductMeasureRelative":
                    comparer = new BireductRelativeComparer();
                    break;

                default:
                    comparer = new ReductLenghtComparer();
                    break;
            }

            return comparer;
        }

        public static ReductFactory Instance
        {
            get
            {
                if (reductFactoryInstance == null)
                {
                    lock (syncRoot)
                    {
                        if (reductFactoryInstance == null)
                        {
                            reductFactoryInstance = new ReductFactory();
                        }
                    }
                }

                return reductFactoryInstance;
            }
        }        
    }
    
    public abstract class ApproximateReductFactory : IReductFactory
    {
        public abstract string FactoryKey { get; }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter("DataStore");
            return new PermutationGeneratorReverse(dataStore);
        }

        public abstract IReductGenerator GetReductGenerator(Args args);
    }

    public class ApproximateReductMajorityFactory : ApproximateReductFactory
    {
        public override string FactoryKey
        {
            get { return "ApproximateReductMajority"; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter("DataStore");
            return new ReductGeneratorMajority(dataStore);
        }
    }

    public class ApproximateReductMajorityWeightsFactory : ApproximateReductMajorityFactory
    {
        public override string FactoryKey
        {
            get { return "ApproximateReductMajorityWeights"; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter("DataStore");
            return new ReductGeneratorWeightsMajority(dataStore);
        }
    }

    public class ApproximateReductMajorityWeightEnsembleFactory : ApproximateReductRelativeWeightsFactory
    {
        public override string FactoryKey
        {
            get { return "ApproximateReductMajorityWeightsEnsemble"; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter("DataStore");
            return new ReductGeneratorWeightsEnsembleMajority(dataStore);
        }
    }

    public class ApproximateReductRelativeFactory : ApproximateReductFactory
    {
        public override string FactoryKey
        {
            get { return "ApproximateReductRelative"; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter("DataStore");
            return new ReductGeneratorRelative(dataStore); 
        }
    }

    public class ApproximateReductRelativeWeightsFactory : ApproximateReductRelativeFactory
    {
        public override string FactoryKey
        {
            get { return "ApproximateReductRelativeWeights"; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter("DataStore");
            return new ReductGeneratorWeightsRelative(dataStore);
        }
    }

    public class ApproximateReductRelativeWeightEnsembleFactory : ApproximateReductRelativeWeightsFactory
    {
        public override string FactoryKey
        {
            get { return "ApproximateReductRelativeWeightsEnsemble"; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter("DataStore");
            return new ReductGeneratorWeightsEnsembleRelative(dataStore);
        }
    }

    public class ApproximateReductPositiveFactory : ApproximateReductFactory
    {
        public override string FactoryKey
        {
            get { return "ApproximateReductPositive"; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter("DataStore");
            return new ReductGeneratorPositive(dataStore);
        }
    }

    public class BireductFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return "Bireduct"; }
        }
        
        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter("DataStore");
            return new BireductGenerator(dataStore);
        }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter("DataStore");

            if (args.Exist("ApproximationRatio"))
            {
                int approximationRatio = (int)args.GetParameter("ApproximationRatio");
                double selectionRatio = (double)approximationRatio / (double)100;

                return new PermutationGeneratorFieldObject(dataStore, selectionRatio);
            }

            return new PermutationGeneratorFieldObject(dataStore);
        }
    }

    public class BireductRelativeFactory : BireductFactory
    {
        public override string FactoryKey
        {
            get { return "BireductRelative"; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter("DataStore");
            return new BireductRelativeGenerator(dataStore);
        }

        public override IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter("DataStore");

            if (args.Exist("ApproximationRatio"))
            {
                int approximationRatio = (int)args.GetParameter("ApproximationRatio");
                double selectionRatio = (double)approximationRatio / (double)100;

                return new PermutationGeneratorFieldObjectRelative(dataStore, selectionRatio);
            }

            return new PermutationGeneratorFieldObjectRelative(dataStore);
        }
    }

    public class GammaBireductFactory : BireductFactory
    {
        public override string FactoryKey
        {
            get { return "GammaBireduct"; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter("DataStore");
            return new GammaBireductGenerator(dataStore);
        }
    }   

}

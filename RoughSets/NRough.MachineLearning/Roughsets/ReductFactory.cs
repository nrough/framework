using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using NRough.Core;
using NRough.MachineLearning.Permutations;

namespace NRough.MachineLearning.Roughsets
{
    public interface IFactoryProduct
    {
        string FactoryKey { get; }
    }

    public interface IReductFactory : IFactoryProduct
    {
        IReductGenerator GetReductGenerator(Args args);

        IPermutationGenerator GetPermutationGenerator(Args args);
    }

    public class ReductFactory
    {
        private static volatile ReductFactory reductFactoryInstance = null;
        private static readonly object syncRoot = new object();

        private ListDictionary registeredReductFactories = new ListDictionary();
        private ListDictionary registeredReductMeasures = new ListDictionary();

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
            string[] assemblyNameParts = className.Split(new char[] { '.' });
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
            string[] keys = new string[ReductFactory.Instance.registeredReductFactories.Keys.Count];
            ReductFactory.Instance.registeredReductFactories.Keys.CopyTo(keys, 0);
            return keys;
        }

        public static string[] GetReductMeasureKeys()
        {
            string[] keys = new string[ReductFactory.Instance.registeredReductMeasures.Keys.Count];
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
                    throw new InvalidOperationException("Null factory newInstance. Unable to create necessary reduct factory class.");
                
                return (IReductFactory)reductFactory;
            }
            else
            {
                throw new InvalidOperationException("Factory was not registered. Unable to create necessary reduct factory class.");
            }
        }

        public static IReductGenerator GetReductGenerator(Args args)
        {
            if (!args.Exist(ReductFactoryOptions.ReductType))
                throw new ArgumentException("No FactoryKey parameter found!", "args");
            string factoryKey = (string)args.GetParameter(ReductFactoryOptions.ReductType);
            return ReductFactory.GetReductFactory(factoryKey).GetReductGenerator(args);
        }

        public static IPermutationGenerator GetPermutationGenerator(Args args)
        {
             if (!args.Exist(ReductFactoryOptions.ReductType))
                throw new ArgumentException("No FactoryKey parameter found!", "args");

            if (args.Exist(ReductFactoryOptions.PermuatationGenerator))
                return (IPermutationGenerator)args.GetParameter(ReductFactoryOptions.PermuatationGenerator);

            string factoryKey = (string)args.GetParameter(ReductFactoryOptions.ReductType);

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
                    throw new InvalidOperationException("Null reduct measure newInstance. Unable to create necessary reduct measure class.");

                IReductMeasure iReductMeasure = (IReductMeasure)reductMeasure;
                return iReductMeasure;
            }
            else
            {
                throw new InvalidOperationException("Factory was not registered. Unable to create necessary reduct factory class.");
            }
        }

        //TODO use registered classes
        public static Comparer<IReduct> GetReductComparer(string measureKey)
        {
            Comparer<IReduct> comparer;

            switch (measureKey)
            {
                //default case
                //case "ReductMeasureLength":
                //    comparer = new ReductLengthComparer();
                //    break;

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
                    comparer = new ReductLengthComparer();
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
}
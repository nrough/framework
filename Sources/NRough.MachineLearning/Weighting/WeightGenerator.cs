// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using System;
using NRough.Data;
using NRough.Core;
using NRough.Core.Random;

namespace NRough.MachineLearning.Weighting
{
    [Serializable]
    public class WeightGenerator
    {
        private DataStore dataStore = null;
        protected double[] w;
        protected readonly object syncRoot = new object();

        public WeightGenerator(DataStore dataStore)
        {
            this.dataStore = dataStore;
            this.w = new double[dataStore.NumberOfRecords];
            this.CalcFlag = false;
        }

        #region Properties

        protected DataStore DataStore
        {
            get { return this.dataStore; }
        }

        public virtual double[] Weights
        {
            get
            {
                if (!this.CalcFlag)
                {
                    lock (syncRoot)
                    {
                        if (!this.CalcFlag)
                        {
                            this.Generate();
                        }
                    }
                }
                return this.w;
            }

            set
            {
                if (value != null)
                {
                    this.CalcFlag = false;
                }
                this.w = value;
            }
        }

        protected bool CalcFlag { get; set; }

        #endregion Properties

        #region Methods

        public virtual void Generate()
        {
            this.CalcFlag = true;
        }

        public virtual void Reset()
        {
            this.CalcFlag = false;
            this.Generate();
        }

        #endregion Methods

        public static WeightGenerator Construct(WeightGeneratorType generatorType, DataStore dataStore)
        {
            switch (generatorType)
            {
                case WeightGeneratorType.Majority:
                    return new WeightGeneratorMajority(dataStore);

                case WeightGeneratorType.Relative:
                    return new WeightGeneratorRelative(dataStore);

                case WeightGeneratorType.Random:
                    return new WeightGeneratorRandom(dataStore);

                case WeightGeneratorType.Constant:
                    return new WeightGeneratorConstant(dataStore, 1.0 / dataStore.NumberOfRecords);

                case WeightGeneratorType.Boosting:
                    return new WeightBoostingGenerator(dataStore);
            }

            return null;
        }
    }

    [Serializable]
    public class WeightGeneratorRandom : WeightGenerator
    {
        public WeightGeneratorRandom(DataStore dataStore)
            : base(dataStore)
        {
        }

        public override void Generate()
        {
            if (this.CalcFlag == true)
                return;

            base.Generate();

            double sum = 0;
            for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
            {
                this.Weights[i] = 
                    RandomSingleton.Random.Next(0, this.DataStore.NumberOfRecords);
                sum += this.Weights[i];
            }

            if (sum != 0)
            {
                double allocated = 0;
                for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
                {
                    this.Weights[i] = this.Weights[i] / sum;

                    if (i == this.DataStore.NumberOfRecords - 1)
                        this.Weights[i] = 1.0 - allocated;

                    allocated += this.Weights[i];
                }
            }
            else
            {
                throw new InvalidOperationException("sum of weights cannot be zero.");
            }
        }
    }

    [Serializable]
    public class WeightGeneratorConstant : WeightGenerator
    {        
        public double Value { get; set; }

        public WeightGeneratorConstant(DataStore dataStore)
            : base(dataStore)
        {

            this.Value = dataStore.NumberOfRecords > 0 ? 1.0 / dataStore.NumberOfRecords : 1.0;
        }

        public WeightGeneratorConstant(DataStore dataStore, double value)
            : base(dataStore)
        {
            this.Value = value;
        }

        public override void Generate()
        {
            if (this.CalcFlag == true)
                return;
            base.Generate();
            for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
                this.Weights[i] = this.Value;
        }
    }

    [Serializable]
    public class WeightGeneratorMajority : WeightGeneratorConstant
    {
        public WeightGeneratorMajority(DataStore dataStore)
            : base(dataStore, 1.0 / dataStore.NumberOfRecords)
        {
        }
    }

    [Serializable]
    public class WeightGeneratorRelative : WeightGenerator
    {
        public WeightGeneratorRelative(DataStore dataStore)
            : base(dataStore)
        {
        }

        public override void Generate()
        {
            if (this.CalcFlag == true)
                return;

            base.Generate();
            for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
            {
                this.Weights[i] = 1.0
                    / (this.DataStore.DataStoreInfo.NumberOfObjectsWithDecision(this.DataStore.GetDecisionValue(i))
                        * this.DataStore.DataStoreInfo.NumberOfDecisionValues);
            }
        }
    }
}
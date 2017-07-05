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

using NRough.Core;
using NRough.Core.CollectionExtensions;
using NRough.Data;
using NRough.MachineLearning.Classification;
using NRough.MachineLearning.Roughsets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification.Ensembles
{
    public abstract class EnsembleBase 
        : ClassificationModelBase, ILearner, IClassificationModel, IEnumerable<IClassificationModel>
    {
        protected static int DefaultIterations = 100;

        public int Iterations { get; set; }
        public int Size { get; set; }
        public ModelConfidenceCalcMethod CalcModelConfidence { get; set; }

        protected class WeakClassifierInfo
        {
            public WeakClassifierInfo(IClassificationModel model, double weight)
            {
                this.Model = model;
                this.Weight = weight;
            }
            public IClassificationModel Model { get; set; }
            public double Weight { get; set; }
        }

        protected IList<WeakClassifierInfo> weakClassifiers;

        public EnsembleBase()
        {
            Iterations = -1;
            Size = -1;
            weakClassifiers = new List<WeakClassifierInfo>();
            CalcModelConfidence = ReductEnsembleBoostingGenerator.ModelConfidenceAdaBoostM1;
        }

        public EnsembleBase(string modelName)
            : base(modelName)
        {
            Iterations = -1;
            Size = -1;
            weakClassifiers = new List<WeakClassifierInfo>();
            CalcModelConfidence = ReductEnsembleBoostingGenerator.ModelConfidenceAdaBoostM1;
        }

        public abstract ClassificationResult Learn(DataStore data, int[] attributes);

        public virtual void AddClassfier(IClassificationModel model, double weight)
        {
            weakClassifiers.Add(new WeakClassifierInfo(model, weight));
        }

        public virtual void Reset()
        {
            weakClassifiers = Iterations > 0 ? new List<WeakClassifierInfo>(Iterations)
                : new List<WeakClassifierInfo>(Iterations);
        }

        public virtual long Compute(DataRecordInternal record)
        {
            var votes = new Dictionary<long, double>(weakClassifiers.Count);

            int i = 0;
            foreach (var weakClassifierInfo in weakClassifiers)
            {
                i++;
                long result = weakClassifierInfo.Model.Compute(record);

                if (votes.ContainsKey(result))
                    votes[result] += weakClassifierInfo.Weight;
                else
                    votes.Add(result, weakClassifierInfo.Weight);

                if (this.Size > 0 && i >= this.Size)
                    break;
            }

            return votes.Count > 0 ? votes.FindMaxValueKey() : Classifier.UnclassifiedOutput;
        }

        public IEnumerator<IClassificationModel> GetEnumerator()
        {
            return weakClassifiers.Select(c => c.Model).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

﻿// 
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
using NRough.MachineLearning.Permutations;
using NRough.Doc;

namespace NRough.MachineLearning.Roughsets
{
    [AssemblyTreeVisible(false)]
    public class ReductEnsembleBoostingWithAttributeDiversityGenerator : ReductEnsembleBoostingGenerator
    {
        public ReductEnsembleBoostingWithAttributeDiversityGenerator()
            : base()
        {
        }

        public ReductEnsembleBoostingWithAttributeDiversityGenerator(DataStore data)
            : base(data)
        {
        }

        public override void InitDefaultParameters()
        {
            base.InitDefaultParameters();
        }

        public override void InitFromArgs(Args args)
        {
            base.InitFromArgs(args);
        }

        public override IReduct GetNextReduct(double[] weights)
        {
            Permutation permutation = new PermutationGeneratorEnsemble(this.DecisionTable, this.GetReductGroups()).Generate(1)[0];
            return this.CreateReduct(permutation.ToArray(), this.Epsilon, weights);
        }
    }

    [AssemblyTreeVisible(false)]
    public class ReductEnsembleBoostingWithAttributeDiversityFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return ReductTypes.ReductEnsembleBoostingWithAttributeDiversity; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            ReductEnsembleBoostingWithAttributeDiversityGenerator rGen = new ReductEnsembleBoostingWithAttributeDiversityGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            throw new NotImplementedException("GetPermutationGenerator(Args args) method is not implemented");
        }
    }
}
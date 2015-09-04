using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{       
    public class ReductEnsembleGenerator : ReductGenerator
    {                        
        private int[] permEpsilon;
        
        public ReductEnsembleGenerator(DataStore data, int[] epsilon)
            : base(data)
        {
            permEpsilon = new int[epsilon.Length];
            Array.Copy(epsilon, permEpsilon, epsilon.Length);
        }

        public override IReductStore Generate(Args args)
        {
            throw new NotImplementedException();            
        }

        protected override IReductStore CreateReductStore(Args args)
        {
            throw new NotImplementedException();
        }

        protected override IReduct CreateReductObject(int[] fieldIds)
        {
            return new Reduct(this.DataStore, fieldIds);
        }
    }

    public class ReductEnsambleFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return "ReductEnsemble"; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter("DataStore");
            int[] epsilons = (int[])args.GetParameter("PermutationEpsilon");
            ReductEnsembleGenerator reductGenerator = new ReductEnsembleGenerator(dataStore, epsilons);
            return reductGenerator;
        }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter("DataStore");
            return new PermutationGenerator(dataStore);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoon.MachineLearning.Roughset
{
    public static class ReductGeneratorExtensions
    {
        public static List<IReduct> GetReducts(this IReductGenerator generator)
        {
            List<IReduct> result = new List<IReduct>();
            foreach (var store in generator.GetReductStoreCollection())
            {
                foreach(var reduct in store)
                {
                    result.Add(reduct);
                }
            }

            return result;
        }
    }
}

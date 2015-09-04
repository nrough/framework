using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    public interface IReductGenerator
    {                        
        double ApproximationDegree { get; set; }
        
        void InitFromArgs(Args args);
        IReductStore ReductPool { get; }
        void Generate();
        IReductStoreCollection GetReductGroups(int numberOfEnsembles);
    }
}

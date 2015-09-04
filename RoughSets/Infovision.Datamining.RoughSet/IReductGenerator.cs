using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    public interface IReductGenerator
    {                
        //IReductStoreCollection Generate(Args args);
        double ApproximationDegree { get; set; }
        
        void InitFromArgs(Args args);
        IReductStoreCollection ReductStoreCollection { get; }
        IReductStore ReductPool { get; }
        void Generate();
    }
}

using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    public interface IReductGenerator
    {        
        //TODO Run() or Execute() or Generate()
        //TODO Property for returning generated reduct store        
        
        IReductStoreCollection Generate(Args args);
        double ApproximationDegree { get; set; }
    }
}

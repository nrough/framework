using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    public interface IReductGenerator
    {
        //IReductStore Generate(Args args);
        IReductStoreCollection Generate(Args args);
        double ApproximationDegree { get; set; }
    }
}

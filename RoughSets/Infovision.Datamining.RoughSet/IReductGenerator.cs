using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    public interface IReductGenerator
    {
        IReductStore Generate(Args args);
        double ApproximationLevel { get; set; }
    }
}

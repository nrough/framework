
namespace Infovision.Datamining.Genetic
{
    public interface IGAEncodable
    {
        uint GetSize();
        Chromosome GetChromosome();
        Chromosome GetChromosome(uint index);
        double GetEvaluation(Chromosome chromosome);
    }

    public class SolutionEncoder : IGAEncodable
    {
        public uint GetSize()
        {
            return 1;
        }

        public Chromosome GetChromosome()
        {
            return null;
        }

        public Chromosome GetChromosome(uint index)
        {
            return GetChromosome();
        }

        public double GetEvaluation(Chromosome chromosome)
        {
            return 1.0;
        }
    }

    public class GASolutionEncoderBin : SolutionEncoder
    {
    }
}

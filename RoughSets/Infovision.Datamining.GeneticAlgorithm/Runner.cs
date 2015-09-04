using System;

namespace Infovision.Datamining.Genetic
{
    public class Runner
    {
        protected Random random;

        public Runner(IGAEncodable solutionEncoder, uint populationSize, uint iterations, int mutationRatio)
        {
            random = new Random();

            this.populationSize = populationSize;
            this.solutionEncoder = solutionEncoder;
            this.mutationRatio = mutationRatio;
            this.iterations = iterations;

            population = new Chromosome[populationSize];
            intermidiatePopulation = new Chromosome[populationSize];
            
            //init random population
            for (uint i = 0; i < populationSize; i++)
            {
                population[i] = solutionEncoder.GetChromosome(i);
            }
        }

        public void Run()
        {
            for (uint i = 0; i < iterations; i++)
            {
                CalcFitness();
               
                Reproduce();

            }
        }

        public uint PopulationSize
        {
            get
            {
                return populationSize;
            }
            set
            {
                populationSize = value;
            }
        }
        
        public Int32 ChromosomeSize
        {
            get { return population[0].Length; }
        }

        public int MutationRatio
        {
            get { return mutationRatio; }
            set { mutationRatio = value; }
        }
        public Chromosome this[int index]
        {
            get { return (Chromosome) population[index]; }
            set { population[index] = value; }
        }

        private void CalcFitness()
        {
            double fitnessSum = 0;
            foreach (Chromosome chromosome in population)
            {
                chromosome.Fitness = solutionEncoder.GetEvaluation(chromosome);
                fitnessSum += chromosome.Fitness;
            }

            foreach (Chromosome chromosome in population)
            {
                chromosome.Fitness = fitnessSum != 0 ? chromosome.Fitness / (fitnessSum / populationSize) : 0;
            }
        }
        
        private void Select()
        {
            int index = 0;
            bool selected = false;
            foreach (Chromosome chromosome in population)
            {
                if (selected)
                {
                }
                
                int copies = (int)chromosome.Fitness;
                double remainder = chromosome.Fitness - copies;

                for (int i = 0; i < copies; i++)
                {
                    intermidiatePopulation[index] = chromosome;
                    index++;
                }

                selected = false;

                if (remainder > random.NextDouble())
                {
                    intermidiatePopulation[index] = chromosome;
                    index++;
                }
            }
        }
        
        private void Reproduce()
        {
        }
        
        private void MutatePopulation()
        {
        }

        private uint populationSize;
        private int mutationRatio;
        private uint iterations;

        private Chromosome [] population;
        private Chromosome[] intermidiatePopulation;
        private IGAEncodable solutionEncoder;
    }
}

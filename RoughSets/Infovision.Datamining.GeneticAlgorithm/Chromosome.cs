using System;

namespace Infovision.Datamining.Genetic
{
    public class Chromosome
    {
        private Object[] chromosome;
        private double fitness;

        public Chromosome(Object [] values)
        {
            chromosome = new object[values.Length];
            values.CopyTo(chromosome, 0);
            fitness = 0;
        }

        public int Length
        {
            get { return chromosome.Length; }
        }

        public double Fitness
        {
            get { return fitness;}
            set { fitness = value; }
        }
        
        public Object this[int index]
        {
            get { return chromosome[index]; }
        }

        public Object[] GetChromosome()
        {
            return chromosome;
        }
    }
}

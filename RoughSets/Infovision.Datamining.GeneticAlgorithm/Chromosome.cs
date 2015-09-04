using System;

namespace Infovision.Datamining.Genetic
{
    public class Chromosome
    {
        private Object[] chromosome;
        private Double fitness;

        public Chromosome(Object [] values)
        {
            chromosome = new object[values.Length];
            values.CopyTo(chromosome, 0);
            fitness = 0;
        }

        public Int32 Length
        {
            get { return chromosome.Length; }
        }

        public Double Fitness
        {
            get { return fitness;}
            set { fitness = value; }
        }
        
        public Object this[Int32 index]
        {
            get { return chromosome[index]; }
        }

        public Object[] GetChromosome()
        {
            return chromosome;
        }
    }
}

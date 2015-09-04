using System;
using Infovision.Utils;

namespace Infovision.Datamining.NeuralNets
{
    [Serializable]
    public abstract class Neuron
    {
        #region Globals

        protected int numberOfInputs = 0;
        protected double[] weights = null;
        protected double output = 0;
        protected static Range weightRange = new Range(0, 1.0);

        #endregion

        #region Constructor
        
        public Neuron(int numberOfInputs)
        {
            this.numberOfInputs = numberOfInputs;
            this.weights = new double[numberOfInputs];
            InitWeights();
        }

        #endregion

        #region Properties

        public int NumberOfInputs
        {
            get { return this.numberOfInputs; }
        }

        public double Output
        {
            get { return this.output; }
        }

        public double this[int index]
        {
            get { return this.weights[index]; }
            set { this.weights[index] = value; }
        }

        #endregion

        #region Methods

        public virtual void InitWeights()
        {
            for (int i = 0; i < this.numberOfInputs; i++)
            {
                weights[i] = RandomSingleton.Random.NextDouble() * Neuron.weightRange.Length + Neuron.weightRange.LowerBound;
            }
        }

        public abstract double Calc(double[] input);

        #endregion
    }

}

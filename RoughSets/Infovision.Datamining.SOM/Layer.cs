using System;

namespace Infovision.Datamining.NeuralNets
{
    public abstract class Layer
    {
        #region Globals
        
        protected int numberOfInputs = 0;
        protected int numberOfNeurons = 0;
        protected Neuron[] neurons = null;
        protected double[] output = null;

        #endregion

        #region Constructors
        
        protected Layer( int numberOfNeurons, int numberOfInputs)
        {
            this.numberOfInputs = Math.Max( 1, numberOfInputs );
            this.numberOfNeurons = Math.Max( 1, numberOfNeurons );
            neurons = new Neuron[this.numberOfNeurons];
        }

        #endregion

        #region Properties
        
        public int NumberOfInputs
        {
            get { return this.numberOfInputs; }
        }

        public int NumberOfNeurons
        {
            get { return this.numberOfNeurons; }
        }

        public Neuron this[int index]
        {
            get { return this.neurons[index]; }
        }

        #endregion

        #region Methods

        public double[] GetOutput()
        {
            return this.output;
        }

        public double GetOutput(int index)
        {
            return this.output[index];
        }
         
        #endregion
        
        public virtual double[] Calc( double[] input )
        {
            double[] output = new double[this.numberOfNeurons];
            for ( int i = 0; i < this.numberOfNeurons; i++ )
            {
                output[i] = this.neurons[i].Calc( input );
            }
            this.output = output;
            return output;
        }

        public virtual void InitNeurons()
        {
            foreach (Neuron neuron in this.neurons)
            {
                neuron.InitWeights();
            }
        }
    }
}

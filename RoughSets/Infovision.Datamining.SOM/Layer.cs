using System;

namespace Infovision.Datamining.NeuralNets
{
    public abstract class Layer
    {
        #region Globals
        
        protected Int32 numberOfInputs = 0;
        protected Int32 numberOfNeurons = 0;
        protected Neuron[] neurons = null;
        protected Double[] output = null;

        #endregion

        #region Constructors
        
        protected Layer( Int32 numberOfNeurons, Int32 numberOfInputs)
        {
            this.numberOfInputs = Math.Max( 1, numberOfInputs );
            this.numberOfNeurons = Math.Max( 1, numberOfNeurons );
            neurons = new Neuron[this.numberOfNeurons];
        }

        #endregion

        #region Properties
        
        public Int32 NumberOfInputs
        {
            get { return this.numberOfInputs; }
        }

        public Int32 NumberOfNeurons
        {
            get { return this.numberOfNeurons; }
        }

        public Neuron this[int index]
        {
            get { return this.neurons[index]; }
        }

        #endregion

        #region Methods

        public Double[] GetOutput()
        {
            return this.output;
        }

        public Double GetOutput(Int32 index)
        {
            return this.output[index];
        }
         
        #endregion
        
        public virtual Double[] Calc( Double[] input )
        {
            Double[] output = new Double[this.numberOfNeurons];
            for ( Int32 i = 0; i < this.numberOfNeurons; i++ )
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

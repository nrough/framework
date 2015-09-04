using System;
using AForge;
using AForge.Neuro;
using AForge.Neuro.Learning;
using NUnit.Framework;


namespace Infovision.Datamining.NeuralNets.UnitTests
{
    [TestFixture]
    public class AForgeNeuralTest
    {

        [Test]
        public void KohonenSOMExample()
        {
            // set range for randomization neurons' weights
            Neuron.RandRange = new Range( 0, 255 );
            ThreadSafeRandom rand = Neuron.RandGenerator;
            // create network
            DistanceNetwork    network = new DistanceNetwork(
                    3, // thress inputs in the network
                    100 * 100 ); // 10000 neurons
            // create learning algorithm
            SOMLearning    trainer = new SOMLearning( network );
            // network's input
            double[] input = new double[3];

            Boolean needToStop = false;

            trainer.LearningRadius = 1;
            trainer.LearningRate = 2;
            
            Int32 i = 0;

            // loop
            while ( !needToStop )
            {
                i++;

                input[0] = rand.Next( 256 );
                input[1] = rand.Next( 256 );
                input[2] = rand.Next( 256 );

                trainer.Run( input );


                if (i > 20)
                {
                    needToStop = true;
                }
                
                
                // ...
                // update learning rate and radius continuously,
                // so networks may come steady state
            }

        }
        
        [Test]
        public void NeuralNetworkTest()
        {
            Neuron.RandRange = new Range(0, 255);

            AForge.Neuro.DistanceNetwork network = new DistanceNetwork(3, 3 * 3);
            network.Randomize();

            AForge.Neuro.Learning.SOMLearning somLearning = new AForge.Neuro.Learning.SOMLearning(network);

            Double [][] inputVectors = new Double[][] 
                                        {
                                            new Double[] {100, 100, 100},
                                            new Double[] {250, 0, 250},
                                            new Double[] {100, 100, 0},
                                            new Double[] {10, 10, 10},
                                            new Double[] {10, 10, 0}
                                        };

            Double error = somLearning.RunEpoch(inputVectors);
            Console.WriteLine("Error {0}", error);

            network.Compute(new Double[] { 100, 100, 100 });
            Console.WriteLine("Output {0}", network.GetWinner());

            network.Compute(new Double[] { 10, 10, 10 });
            Console.WriteLine("Output {0}", network.GetWinner());
        }


    }
}

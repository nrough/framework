﻿using System;
using System.IO;
using AForge;
using AForge.Neuro;
using AForge.Neuro.Learning;

namespace Infovision.MRI
{
    [Serializable]
    public class ImageSOMCluster
    {
        public ImageSOMCluster(int inputSize, int outputSize)
        {
            if (inputSize < 1)
            {
                throw new System.ArgumentOutOfRangeException();
            }

            this.Network = new DistanceNetwork(inputSize, outputSize);
        }

        public ImageSOMCluster(DistanceNetwork trainedNetwork)
        {
            this.Network = trainedNetwork;
        }

        public DistanceNetwork Network
        {
            get;
            private set;
        }

        private double[][] GetInputData(IImage[] images, int sliceId)
        {
            uint width = images[0].Width;
            uint height = images[0].Height;
            double[][] input = new double[width * height][];
            uint[] position = new uint[] { 0, 0, (uint)sliceId };
            
            for (uint y = 0; y < height; y++)
            {
                for (uint x = 0; x < width; x++)
                {
                    uint idx = y * width + x;
                    input[idx] = new double[images.Length];
                    
                    position[0] = x;
                    position[1] = y;

                    for (int j = 0; j < images.Length; j++)
                    {
                        input[idx][j] = images[j].GetPixel<double>(position);
                    }
                }
            }

            return input;
        }

        public void Train(IImage[] images, int iterations, double learningRate, int radius, int sliceId)
        {
            Neuron.RandRange = SimpleITKHelper.GetRandRange(images[0]);
            ThreadSafeRandom rand = Neuron.RandGenerator;

            this.Network.Randomize();
            SOMLearning trainer = new SOMLearning(this.Network);

            double fixedLearningRate = learningRate / 10;
            double driftingLearningRate = fixedLearningRate * 9;
            double[][] input = this.GetInputData(images, sliceId);
            
            for (int i = 0; i <= iterations; i++)
            {
                trainer.LearningRate = driftingLearningRate * (iterations - i) / iterations + fixedLearningRate;
                trainer.LearningRadius = (double)radius * (iterations - i) / iterations;

                //trainer.Run(input);
                trainer.RunEpoch(input);
            }

        }

        public void Train(IImage[] images, int iterations, double learningRate, int radius)
        {
            Train(images, iterations, learningRate, radius, 1);
        }

        public void Train(IImage image, int iterations, double learningRate, int radius, int sliceId)
        {
            Train(new IImage[] { image }, iterations, learningRate, radius, sliceId);
        }

        public void Train(IImage image, int iterations, double learningRate, int radius)
        {
            Train(new IImage[] { image }, iterations, learningRate, radius, 1);
        }

        public IImage Execute(IImage[] images)
        {
            if (images.Length != this.Network.InputsCount)
            {
                throw new System.ArgumentException();
            }

            uint width = images[0].Width;
            uint height = images[0].Height;
            uint depth = images[0].Depth;

            IImage result = new ImageITK
            {
                ItkImage = SimpleITKHelper.ConstructImage(width, height, depth, typeof(byte))
            };

            double[] input = new double[this.Network.InputsCount];
            uint[] position = new uint[] { 0, 0, 0 };

            for (uint z = 0; z < depth; z++)
            {
                for (uint y = 0; y < height; y++)
                {
                    for (uint x = 0; x < width; x++)
                    {
                        position[0] = x;
                        position[1] = y;
                        position[2] = z;

                        for (int i = 0; i < images.Length; i++)
                        {
                            input[i] = images[i].GetPixel<double>(position);
                        }

                        this.Network.Compute(input);                        
                        byte pixelValue = (byte) (this.Network.GetWinner() * 10);

                        result.SetPixel<byte>(position, pixelValue);
                    }
                }
            }

            return result;
        }

        public IImage Execute(IImage image)
        {
            return Execute(new IImage[] { image });
        }

        public void SaveNetwork(string fileName)
        {
            this.Network.Save(fileName);
        }

        public void SaveNetwork(Stream stream)
        {
            this.Network.Save(stream);
        }

        public void LoadNetwork(Stream stream)
        {
            this.Network = (DistanceNetwork) DistanceNetwork.Load(stream);
        }

        public void LoadNetwork(string fileName)
        {
            this.Network = (DistanceNetwork)DistanceNetwork.Load(fileName);
        }
    }
}
using System;
using LearningFoundation;

namespace KLRIntegrationWithLearningAPI
{
    /// <summary>
    /// Class for Kernel Logistic Regression Machine Learning Algorithm 
    /// </summary>
    public class KLRAlgorithm : IAlgorithm
    {
        private readonly double _learningRate;
        private readonly double _sigma;

        private static readonly Random Random = new Random(0);

        public double[] Alphas;
        public double[][] TrainingData;

        /// <summary>
        /// Initializes an instance of KLRAlgorithm class (Constructor).
        /// </summary>
        public KLRAlgorithm(double learningRate, double sigma)
        {
            _learningRate = learningRate;
            _sigma = sigma;
        }

        public IScore Run(double[][] data, IContext ctx)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            const int numberOfFeatures = 2;
            var iteration = 0;
            const int maxIteration = 1000;
            var trainingDataLength = data.Length;
            var kernelMatrix = new double[trainingDataLength][];
            var indices = new int[trainingDataLength];
            var kLrScore = new KLRScore();

            Alphas = new double[trainingDataLength + 1]; // alpha weight for each train item
            TrainingData = data;
            
            // initializing kernel matrix
            for (int index = 0; index < kernelMatrix.Length; ++index)
            {
                kernelMatrix[index] = new double[trainingDataLength];
            }

            // initiating alpha weights
            for (var counter = 0; counter < Alphas.Length; ++counter)
            {
                Alphas[counter] = 0.0;
            }

            // pre-computing all Kernel: item-item similarity
            for (var rowIndex = 0; rowIndex < trainingDataLength; ++rowIndex)
            {
                for (var columnIndex = 0; columnIndex < trainingDataLength; ++columnIndex)
                {
                    var k = Kernel(data[rowIndex], data[columnIndex], _sigma);
                    kernelMatrix[rowIndex][columnIndex] = kernelMatrix[columnIndex][rowIndex] = k;
                }
            }

            // initializing indices
            for (var index = 0; index < indices.Length; ++index)
                indices[index] = index;

            // training data: aj = aj + eta(t - y) * K(i,j)
            while (iteration < maxIteration)
            {
                Shuffle(indices); // visiting training data in random order

                for (var index = 0; index < indices.Length; ++index) // each 'from' training data
                {
                    var currentIndex = indices[index]; // current training data index

                    var sum = 0.0; // sum of alpha[i] * kernel[i]

                    for (var counter = 0; counter < Alphas.Length - 1; ++counter) // not the bias
                    {
                        sum += Alphas[counter] * kernelMatrix[currentIndex][counter];
                    }

                    sum += Alphas[Alphas.Length - 1]; // add bias (last cell of alphas array) 

                    var y = 1.0 / (1.0 + Math.Exp(-sum));

                    var t = data[currentIndex][numberOfFeatures]; // last column holds target value

                    // update each alpha value
                    for (var counter = 0; counter < Alphas.Length - 1; ++counter)
                    {
                        Alphas[counter] = Alphas[counter] +
                                          (_learningRate * (t - y) * kernelMatrix[currentIndex][counter]);
                    }

                    // update the bias value
                    Alphas[Alphas.Length - 1] = Alphas[Alphas.Length - 1] +
                                                (_learningRate * (t - y)) * 1; // dummy input
                }

                ++iteration;
            }

            kLrScore.Alphas = Alphas;
            kLrScore.Data = data;

            return kLrScore;
        }

        public IScore Train(double[][] data, IContext ctx)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return Run(data, ctx);
        }

        public IResult Predict(double[][] data, IContext ctx)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var predictedValues = new double[data.Length];
            var trainingData = TrainingData;
            var alphas = Alphas;
            
            for (var i = 0; i < data.Length; ++i) // i index into data to predict
            {
                // compare current test data against all training data
                double sum = 0.0;

                for (var j = 0; j < alphas.Length - 1; ++j)
                {
                    var k = Kernel(data[i], trainingData[j], _sigma);
                    sum += alphas[j] * k; // cannot be pre-computed
                }

                sum += alphas[alphas.Length - 1] * 1; // adding the bias

                var y = 1.0 / (1.0 + Math.Exp(-sum));

                predictedValues[i] = 0;

                if (y > 0.5)
                {
                    predictedValues[i] = 1;
                }
            }


            var result = new KLRResult {PredictedValues = predictedValues};

            return result;
        }

        // RBF kernel method.
        // vectors v1 & v2 have class label in last cell
        private static double Kernel(double[] v1, double[] v2, double sigma)
        {
            var numerator = 0.0;

            for (var counter = 0; counter < v1.Length - 1; ++counter)
            {
                numerator += (v1[counter] - v2[counter]) * (v1[counter] - v2[counter]);
            }


            var denominator = 2.0 * sigma * sigma;
            var z = numerator / denominator;

            return Math.Exp(-z);
        }

        private static void Shuffle(int[] indices)
        {
            // assumes class-scope Random object _random
            for (var counter = 0; counter < indices.Length; ++counter)
            {
                var ri = Random.Next(counter, indices.Length);
                var tmp = indices[counter];
                indices[counter] = indices[ri];
                indices[ri] = tmp;
            }
        }
    }
}
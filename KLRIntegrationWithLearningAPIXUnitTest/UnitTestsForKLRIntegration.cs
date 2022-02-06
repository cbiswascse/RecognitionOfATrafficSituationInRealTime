using System;
using System.Collections.Generic;
using System.IO;
using KLRIntegrationWithLearningAPI;
using KLRIntegrationWithLearningAPINUnitTests;
using LearningFoundation;
using LearningFoundation.DataProviders;
using Xunit;

namespace KLRIntegrationWithLearningAPIXUnitTest
{
    public class UnitTestsForKLRIntegration
    {
        const string TrainingDataPath = @"Data\TrainingData.csv";
        private LearningApi _api;
        private LearningApi _apiForGettingTestData;
        private double[][] testData;

        public UnitTestsForKLRIntegration()
        {
            // The arguments for constructor of KLRAlgorithm.
            const double learningRate = 0.001;
            const double sigma = 1.0;

            // The path for the training data csv file.
            var path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), TrainingDataPath);

            //***********************************************
            //* LearningApi used for getting training data. *
            //***********************************************

            // Initializing LearningApi.
            _api = new LearningApi(Helper.GetDescriptor());

            // Reading training data from the csv file.
            _api.UseCsvDataProvider(path, ',', true);

            // Deserializing and parsing of the csv file.
            _api.UseActionModule<object[][], double[][]>((object[][] data, IContext ctx) =>
            {
                List<double[]> newData = new List<double[]>();
                foreach (var item in data)
                {
                    List<double> row = new List<double>();
                    for (int i = 0; i < ctx.DataDescriptor.Features.Length; i++)
                    {
                        double converted;
                        if (double.TryParse((string)item[i], out converted))
                            row.Add(converted);
                        else
                            throw new System.Exception("Column is not convertable to double.");
                    }

                    switch (item[ctx.DataDescriptor.LabelIndex])
                    {
                        case "0":
                            row.Add(0);
                            break;
                        case "1":
                            row.Add(1);
                            break;
                    }

                    newData.Add(row.ToArray());
                }

                return newData.ToArray();
            });

            // Using the pipeline module of the KLR algorithm.
            _api.UseKLRPipelineModule();

            // Getting the training data.
            _api.UseActionModule<double[][], double[][]>((double[][] data, IContext ctx) =>
            {
                int trainingDataLength = (int)Math.Ceiling(data.Length * 0.8);
                var trainingData = new double[trainingDataLength][];

                for (var counter = 0; counter < trainingDataLength; counter++)
                {
                    trainingData[counter] = data[counter];
                }

                return trainingData;
            });

            // Integrating KLR algorithm with LearningApi.
            _api.UseKLRAlgorithm(learningRate, sigma);

            // Running the LearningApi to calculate alpha values and bias.
            var scoreForTrainingSet = _api.Run() as KLRScore;

            //*******************************************
            //* LearningApi used for getting test data. *
            //*******************************************

            // Initializing LearningApi which is used for getting last 20 data of the csv file for test data.
            _apiForGettingTestData = new LearningApi(Helper.GetDescriptor());

            // Reading data from the csv file.
            _apiForGettingTestData.UseCsvDataProvider(path, ',', true);

            // Deserializing and parsing of the csv file.
            _apiForGettingTestData.UseActionModule<object[][], double[][]>((object[][] data, IContext ctx) =>
            {
                List<double[]> newData = new List<double[]>();
                foreach (var item in data)
                {
                    List<double> row = new List<double>();
                    for (int i = 0; i < ctx.DataDescriptor.Features.Length; i++)
                    {
                        double converted;
                        if (double.TryParse((string)item[i], out converted))
                            row.Add(converted);
                        else
                            throw new System.Exception("Column is not convertable to double.");
                    }

                    switch (item[ctx.DataDescriptor.LabelIndex])
                    {
                        case "0":
                            row.Add(0);
                            break;
                        case "1":
                            row.Add(1);
                            break;
                    }

                    newData.Add(row.ToArray());
                }

                return newData.ToArray();
            });

            // Using the pipeline module of the KLR algorithm.
            _apiForGettingTestData.UseKLRPipelineModule();

            // Getting the test data.
            _apiForGettingTestData.UseActionModule<double[][], double[][]>((double[][] data, IContext ctx) =>
            {
                int totalDataLength = data.Length;
                int testDataLength = (int)Math.Ceiling(data.Length * 0.2);
                var testData = new double[testDataLength][];

                for (var counter = 0; counter < testDataLength; counter++)
                {
                    testData[counter] = data[totalDataLength - testDataLength + counter];
                }

                return testData;
            });

            // Integrating KLR algorithm with LearningApi.
            _apiForGettingTestData.UseKLRAlgorithm(learningRate, sigma);

            // Running the LearningApi to calculate alpha values and bias.
            var scoreForTestSet = _apiForGettingTestData.Run() as KLRScore;

            if (scoreForTestSet != null) testData = scoreForTestSet.Data;
        }

        /// <summary>
        /// Test case that tests if the calculated predictions are correct.
        /// </summary>
        [Fact]
        public void Predict_Calculates_ExpectedValue()
        {
            
            var predictedValue = _api.Algorithm.Predict(testData, _api.Context) as KLRResult;

            if (predictedValue == null) return;

            for(var index = 0; index < predictedValue.PredictedValues.Length; index++)
            {
                Assert.Equal(predictedValue.PredictedValues[index], testData[index][2]);
            }
        }

        /// <summary>
        /// Test case that tests if prediction value is calculated for each test data.
        /// </summary>
        [Fact]
        public void Predict_NumberOfPredictedValues_EqualsTo_NumberOfTestData()
        {
            var predictedValue = _api.Algorithm.Predict(testData, _api.Context) as KLRResult;
        
            if (predictedValue != null)
                Assert.Equal(testData.Length, predictedValue.PredictedValues.Length);
        }

        ///<summary>
        /// Test case that checks if ArgumentNullException is thrown in case of null argument for test data.
        /// </summary>
        [Fact]
        public void Predict_WithTestDataNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _api.Algorithm.Predict(null, _api.Context));
        }

        /// <summary>
        /// Test case that tests if alpha value is calculated for each training data.
        /// </summary>
        [Fact]
        public void Run_NumberOfAlphaValues_EqualsTo_NumberOfTrainingData()
        {
            var trainingData = new double[10][];
        
            trainingData[0] = new[] { 2.5, 8.5, 0 };
            trainingData[1] = new[] { -9.5, -0.5, 0 };
            trainingData[2] = new[] { 0.0, -2.0, 1 };
            trainingData[3] = new[] { 1.5, -2.0, 1 };
            trainingData[4] = new[] { 0.5, 2.0, 1 };
            trainingData[5] = new[] { 7.5, 5.5, 0 };
            trainingData[6] = new[] { 2.5, 1.0, 1 };
            trainingData[7] = new[] { 4.0, 0.0, 1 };
            trainingData[8] = new[] { -0.5, 2.5, 1 };
            trainingData[9] = new[] { 1.0, -2.5, 1 };
        
        
            var score = _api.Algorithm.Run(trainingData, _api.Context) as KLRScore;
        
            if (score != null)
                Assert.Equal(trainingData.Length, score.Alphas.Length - 1); //Bias value is stored in the last cell of the Alphas array.
        }

        /// <summary>
        /// Test case that checks if ArgumentNullException is thrown in case of null argument for training data.
        /// </summary>
        [Fact]
        public void Run_WithTrainingDataNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _api.Algorithm.Run(null, _api.Context));
        }

        /// <summary>
        /// Test case that tests if alpha value is calculated for each training data.
        /// </summary>
        [Fact]
        public void Train_NumberOfAlphaValues_EqualsTo_NumberOfTrainingData()
        {
            var trainingData = new double[10][];
        
            trainingData[0] = new[] { 2.5, 8.5, 0 };
            trainingData[1] = new[] { -9.5, -0.5, 0 };
            trainingData[2] = new[] { 0.0, -2.0, 1 };
            trainingData[3] = new[] { 1.5, -2.0, 1 };
            trainingData[4] = new[] { 0.5, 2.0, 1 };
            trainingData[5] = new[] { 7.5, 5.5, 0 };
            trainingData[6] = new[] { 2.5, 1.0, 1 };
            trainingData[7] = new[] { 4.0, 0.0, 1 };
            trainingData[8] = new[] { -0.5, 2.5, 1 };
            trainingData[9] = new[] { 1.0, -2.5, 1 };
        
            var score = _api.Algorithm.Train(trainingData, _api.Context) as KLRScore;
        
            if (score != null)
                Assert.Equal(trainingData.Length, score.Alphas.Length - 1); //Bias value is stored in the last cell of the Alphas array.
        }
        
        /// <summary>
        /// Test case that checks if ArgumentNullException is thrown in case of null argument for training data.
        /// </summary>
        [Fact]
        public void Train_WithTrainingDataNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _api.Algorithm.Train(null, _api.Context));
        }
    }
}

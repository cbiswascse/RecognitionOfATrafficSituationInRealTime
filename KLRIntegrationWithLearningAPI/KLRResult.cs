using LearningFoundation;

namespace KLRIntegrationWithLearningAPI
{
    /// <summary>
    /// Implements IResult interface. Keeps the predicted result from the algorithm.
    /// </summary>
    public class KLRResult : IResult
    {
        public double[] PredictedValues { get; set; }
    }
}
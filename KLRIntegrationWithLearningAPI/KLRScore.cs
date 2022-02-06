using LearningFoundation;

namespace KLRIntegrationWithLearningAPI
{
    /// <summary>
    /// Inherits IScore interface. Keeps related scores of KLR Algorithm.
    /// </summary>
    public class KLRScore : IScore
    {
        public double[][] Data { get; set; }

        public double[] Alphas { get; set; }
    }
}

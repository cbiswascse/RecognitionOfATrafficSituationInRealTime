using LearningFoundation;

namespace KLRIntegrationWithLearningAPI
{
    public static class KLRAlgorithmExtensions
    {
        public static LearningApi UseKLRAlgorithm(this LearningApi api, double learningRate, double sigma)
        {
            KLRAlgorithm klrAlgorithm = new KLRAlgorithm(learningRate, sigma);
            api.AddModule(klrAlgorithm, "KLRAlgorithm");
            return api;
        }
    }
}
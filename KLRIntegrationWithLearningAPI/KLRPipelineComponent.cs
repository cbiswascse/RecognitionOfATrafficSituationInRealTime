using LearningFoundation;

namespace KLRIntegrationWithLearningAPI
{
    public class KLRPipelineComponent : IPipelineModule<double[][], double[][]>
    {
        public double[][] Run(double[][] data, IContext ctx)
        {
            return data;
        }
    }
}
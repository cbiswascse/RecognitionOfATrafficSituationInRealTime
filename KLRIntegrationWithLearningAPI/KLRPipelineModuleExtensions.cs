using System;
using LearningFoundation;

namespace KLRIntegrationWithLearningAPI
{
    public static class KLRPipelineModuleExtensions
    {
        public static LearningApi UseKLRPipelineModule(this LearningApi api)
        {
            KLRPipelineComponent klrAlgorithm = new KLRPipelineComponent();
            api.AddModule(klrAlgorithm, $"KLRPipelineComponent-{Guid.NewGuid()}");
            return api;
        }
    }
}

using LearningFoundation;
using Newtonsoft.Json;

namespace KLRIntegrationWithLearningAPINUnitTests
{
    internal class Helper
    {
        private const string TrainingDataMapperFileName = "Data/TrainingDataMapper.json";

        public static DataDescriptor GetDescriptor(string mapperPath = null)
        {
            if (string.IsNullOrEmpty(mapperPath))
                mapperPath = TrainingDataMapperFileName;

            string strContent = System.IO.File.ReadAllText(mapperPath);

            var dm = JsonConvert.DeserializeObject(strContent, typeof(DataDescriptor));

            if (dm is DataDescriptor)
                return (DataDescriptor)dm;
            else
                return null;
        }
    }
}

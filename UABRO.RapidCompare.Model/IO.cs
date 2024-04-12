using System.IO;
using Newtonsoft.Json;

namespace UABRO.RapidCompare.Model
{
    public static class IO
    {
        public static RapidPlanModelDvhSet JsonToRapidPlanModelDvhSet(string json)
        {
            return JsonConvert.DeserializeObject<RapidPlanModelDvhSet>(json);
        }

        public static string RapidPlanModelDvhSetToJson(RapidPlanModelDvhSet rapidPlanComparison)
        {
            return JsonConvert.SerializeObject(rapidPlanComparison, Formatting.Indented);
        }

        public static RapidPlanModelDvhSet ReadRapidPlanModelDvhSet(string path)
        {
            string json = File.ReadAllText(path);
            return JsonToRapidPlanModelDvhSet(json);
        }

        public static void WriteRapidPlanModelDvhSet(RapidPlanModelDvhSet rapidPlanComparison, string path)
        {
            File.WriteAllText(path, RapidPlanModelDvhSetToJson(rapidPlanComparison));
        }
    }
}

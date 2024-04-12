using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace UABRO.RapidCompare.Model
{
    public class RapidPlanModelDvhSet
    {
        public RapidPlanModelDescription RapidPlanModel;
        public List<RapidPlanDoseVolumeHistograms> RapidPlanDvhs;

        public RapidPlanModelDvhSet()
        {
            RapidPlanModel = new RapidPlanModelDescription();
            RapidPlanDvhs = new List<RapidPlanDoseVolumeHistograms>();
        }

        [JsonIgnore]
        public Lookup<string, RapidPlanDoseVolumeHistograms> RapidPlanDvhsByStructure
        {
            get => (Lookup<string, RapidPlanDoseVolumeHistograms>)RapidPlanDvhs.ToLookup(d => d.ModelStructureId);
        }
        [JsonIgnore]
        public Lookup<string, RapidPlanDoseVolumeHistograms> RapidPlanDvhsByPlanPairId
        {
            get => (Lookup<string, RapidPlanDoseVolumeHistograms>)RapidPlanDvhs.ToLookup(d => d.PlanPairId);
        }
        [JsonIgnore]
        public List<string> StructureIds { get => RapidPlanDvhs.Select(r => r.ModelStructureId).Distinct().ToList(); }
        
    }
}

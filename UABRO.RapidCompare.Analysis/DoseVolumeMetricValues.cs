using System.Collections.Generic;
using System.Linq;
using UABRO.RapidCompare.Model;

namespace UABRO.RapidCompare.Analysis
{
    public class DoseVolumeMetricValues
    {
        public string Label;
        public string Units;
        public string StructureId;
        public ObjectiveOperator ObjectiveOperator;
        public double[] RapidPlan;
        public double[] ReferencePlan;
        public double[] DifferenceValues { get => RapidPlan.Zip(ReferencePlan, (a, b) => a - b).ToArray(); }

        public static List<DoseVolumeMetricValues> AnalyzePointObjectives(List<RapidPlanDoseVolumeHistograms> dvhs, List<RapidPlanObjective> objectives)
        {
            string structureId = dvhs.Select(d => d.ModelStructureId).Distinct().Single();
            List<DoseVolumeMetricValues> result = new List<DoseVolumeMetricValues>();
            foreach (var obj in objectives)
            {
                DoseVolumeMetricValues toadd;
                if (obj.Type == ObjectiveType.Point && obj.Volume.HasValue)
                {
                    toadd = new DoseVolumeMetricValues();
                    toadd.StructureId = structureId;
                    toadd.ObjectiveOperator = obj.Operator;
                    double volume = obj.Volume.Value;
                    if (volume == 0)
                    {
                        toadd.Label = $"DMax[{obj.DoseUnit}]";
                    }
                    else if (volume == 100)
                    {
                        toadd.Label = $"DMin[{obj.DoseUnit}]";
                    }
                    else
                    {
                        toadd.Label = $"D{volume}%[{obj.DoseUnit}]";
                    }
                    toadd.RapidPlan = dvhs.Select(dv => dv.GetDoseAt(new double[] { volume }, obj.DoseUnit, DvhType.RapidPlan).Single()).ToArray();
                    toadd.ReferencePlan = dvhs.Select(dv => dv.GetDoseAt(new double[] { volume }, obj.DoseUnit, DvhType.ReferencePlan).Single()).ToArray();
                    toadd.Units = obj.DoseUnit == DoseUnit.Percent ? "%" : obj.DoseUnit.ToString();
                    result.Add(toadd);
                }

                if (obj.Type == ObjectiveType.Point && obj.Dose.HasValue)
                {
                    toadd = new DoseVolumeMetricValues();
                    toadd.StructureId = structureId;
                    toadd.ObjectiveOperator = obj.Operator;
                    double dose = obj.Dose.Value;
                    toadd.Label = $"V{dose}{obj.DoseUnit}[%]";

                    toadd.RapidPlan = dvhs.Select(dv => dv.GetVolumeAt(new double[] { dose }, obj.DoseUnit, DvhType.RapidPlan).Single()).ToArray();
                    toadd.ReferencePlan = dvhs.Select(dv => dv.GetVolumeAt(new double[] { dose }, obj.DoseUnit, DvhType.ReferencePlan).Single()).ToArray();
                    toadd.Units = "%";
                    result.Add(toadd);
                }

                if (obj.Type == ObjectiveType.Mean)
                {
                    toadd = new DoseVolumeMetricValues();
                    toadd.StructureId = structureId;
                    toadd.ObjectiveOperator = obj.Operator;
                    toadd.Label = $"DMean[{obj.DoseUnit}]";

                    toadd.RapidPlan = dvhs.Select(dv => dv.GetMeanDose(obj.DoseUnit, DvhType.RapidPlan)).ToArray();
                    toadd.ReferencePlan = dvhs.Select(dv => dv.GetMeanDose(obj.DoseUnit, DvhType.ReferencePlan)).ToArray();
                    toadd.Units = obj.DoseUnit == DoseUnit.Percent ? "%" : obj.DoseUnit.ToString();
                    result.Add(toadd);
                }
            }
            return result.GroupBy(r => r.StructureId + r.Label).Select(g => g.First()).ToList();
        }
    }
}

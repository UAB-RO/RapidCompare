using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UABRO.RapidCompare.Model;

namespace UABRO.RapidCompare.Analysis
{
    public static class DoseVolumeMetrics
    {
        static public List<double[]> GetDvhDifferences(List<RapidPlanDoseVolumeHistograms> dvhs, double[] dose, DoseUnit doseUnit, DvhType first, DvhType second)
        {
            var result = new List<double[]>();
            for (int i = 0; i < dose.Length; i++)
                result.Add(new double[dvhs.Count()]);

            for (int j = 0; j < dvhs.Count; j++)
            {
                var volumedifference = dvhs[j].GetVolumeDifferenceAt(dose, doseUnit, first, second);
                for (int i = 0; i < dose.Length; i++)
                    result[i][j] = volumedifference[i];
            }
            return result;
        }
        static public double[] GetDvhDifferences(List<RapidPlanDoseVolumeHistograms> dvhs, double dose, DoseUnit doseUnit, DvhType first, DvhType second)
        {
            return GetDvhDifferences(dvhs,new double[] { dose },doseUnit,first,second).Single();
        }
        static public List<BoxWhiskerComponents> GetVolumeDifferenceBoxWhiskerStatistics(List<RapidPlanDoseVolumeHistograms> dvhs, double[] dose, DoseUnit doseUnit, DvhType first, DvhType second)
        {
            var delta = GetDvhDifferences(dvhs, dose, doseUnit, first, second);
            var result = new List<BoxWhiskerComponents>();
            foreach (var value in delta)
                result.Add(new BoxWhiskerComponents(value));
            return result;
        }
        static public BoxWhiskerComponents GetVolumeDifferenceBoxWhiskerStatistics(List<RapidPlanDoseVolumeHistograms> dvhs, double dose, DoseUnit doseUnit, DvhType first, DvhType second)
        {
            double[] delta = GetDvhDifferences(dvhs, dose, doseUnit, first, second);
            return new BoxWhiskerComponents(delta);
        }
        static public List<BoxWhiskerComponents> GetVolumeBoxWhiskerStatistics(List<RapidPlanDoseVolumeHistograms> dvhs, double[] dose, DoseUnit doseUnit, DvhType dvhType)
        {
            var result = new List<BoxWhiskerComponents>();
            foreach (var d in dose)
            {
                result.Add(GetVolumeBoxWhiskerStatistics(dvhs,d,doseUnit,dvhType));
            }
            return result;
        }
        static public BoxWhiskerComponents GetVolumeBoxWhiskerStatistics(List<RapidPlanDoseVolumeHistograms> dvhs, double dose, DoseUnit doseUnit, DvhType dvhType)
        {
            double[] value = dvhs.Select(dv => dv.GetVolumeAt(dose, doseUnit, dvhType)).ToArray();
            return new BoxWhiskerComponents(value);
        }

        static public double[] GetVolumeDifferenceOneSidedSignTestPValue(List<RapidPlanDoseVolumeHistograms> dvhs, double[] dose, DoseUnit doseUnit, DvhType first, DvhType second)
        {
            var delta = GetDvhDifferences(dvhs.ToList(), dose, doseUnit, first, second);
            var result = new double[dose.Length];
            for (int i = 0; i < dose.Length; i++)
            {
                result[i] = Statistics.SignTestPValue(delta[i]).pRight;
            }
            return result;
        }
        static public double GetVolumeDifferenceOneSidedSignTestPValue(List<RapidPlanDoseVolumeHistograms> dvhs, double dose, DoseUnit doseUnit, DvhType first, DvhType second)
        {
            var delta = GetDvhDifferences(dvhs.ToList(), dose, doseUnit, first, second);
            return Statistics.SignTestPValue(delta).pRight;
        }
        static public double GetMaximumDose(List<RapidPlanDoseVolumeHistograms> dvhs)
        {
            double result = dvhs.Select(d => d.ReferencePlanDvh.Select(dd => dd.DoseGy).Max()).Max();
            result = Math.Max(result, dvhs.Select(d => d.RapidPlanDvh.Select(dd => dd.DoseGy).Max()).Max());
            result = Math.Max(result, dvhs.Where(d => d.LowerDvhEstimate != null).Select(d => d.LowerDvhEstimate.Select(dd => dd.DoseGy).Max()).Max());
            result = Math.Max(result, dvhs.Where(d => d.UpperDvhEstimate != null).Select(d => d.UpperDvhEstimate.Select(dd => dd.DoseGy).Max()).Max());
            return result;
        }
        static public BoxWhiskerComponents GetDoseBoxWhiskerStatistics(List<RapidPlanDoseVolumeHistograms> dvhs, double volume, DoseUnit doseUnit, DvhType dvhType)
        {
            double[] value = dvhs.Select(dv => dv.GetDoseAt(new double[] { volume }, doseUnit, dvhType).Single()).ToArray();
            return new BoxWhiskerComponents(value);
        }
        static public double[] GetDoseDifference(List<RapidPlanDoseVolumeHistograms> dvhs, double volume, DoseUnit doseUnit, DvhType first, DvhType second)
        {
            double[] firstdose = dvhs.Select(dv => dv.GetDoseAt(new double[] { volume }, doseUnit, first).Single()).ToArray();
            double[] seconddose = dvhs.Select(dv => dv.GetDoseAt(new double[] { volume }, doseUnit, second).Single()).ToArray();
            double[] delta = firstdose.Zip(seconddose, (a, b) => a - b).ToArray();
            return delta;
        }
        static public BoxWhiskerComponents GetDoseDifferenceBoxWhiskerStatistics(List<RapidPlanDoseVolumeHistograms> dvhs, double volume, DoseUnit doseUnit, DvhType first, DvhType second)
        {
            double[] delta = GetDoseDifference(dvhs, volume, doseUnit, first, second);
            return new BoxWhiskerComponents(delta);
        }
        static public double GetDoseDifferenceOneSidedSignTestPValue(List<RapidPlanDoseVolumeHistograms> dvhs, double volume, DoseUnit doseUnit, DvhType first, DvhType second)
        {
            double[] delta = GetDoseDifference(dvhs, volume, doseUnit, first, second);
            return Statistics.SignTestPValue(delta).pRight;
        }
        static public BoxWhiskerComponents GetMeanDoseBoxWhiskerStatistics(List<RapidPlanDoseVolumeHistograms> dvhs, DoseUnit doseUnit, DvhType dvhType)
        {
            double[] value = dvhs.Select(dv => dv.GetMeanDose(doseUnit, dvhType)).ToArray();
            return new BoxWhiskerComponents(value);
        }
        static public double[] GetMeanDoseDifference(List<RapidPlanDoseVolumeHistograms> dvhs, DoseUnit doseUnit, DvhType first, DvhType second)
        {
            double[] firstmndose = dvhs.Select(dv => dv.GetMeanDose(doseUnit, first)).ToArray();
            double[] secondmndose = dvhs.Select(dv => dv.GetMeanDose(doseUnit, second)).ToArray();
            double[] delta = firstmndose.Zip(secondmndose, (a, b) => a - b).ToArray();
            return delta;
        }
        static public BoxWhiskerComponents GetMeanDoseDifferenceBoxWhiskerStatistics(List<RapidPlanDoseVolumeHistograms> dvhs, DoseUnit doseUnit, DvhType first, DvhType second)
        {
            return new BoxWhiskerComponents(GetMeanDoseDifference(dvhs,doseUnit,first,second));
        }
        static public double GetMeanDoseDifferenceOneSidedSignTestPValue(List<RapidPlanDoseVolumeHistograms> dvhs, DoseUnit doseUnit, DvhType first, DvhType second)
        {
            double[] delta = GetMeanDoseDifference(dvhs, doseUnit, first, second);
            return Statistics.SignTestPValue(delta).pRight;
        }
    }
}

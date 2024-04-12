using System;
using System.Linq;
using UABRO.RapidCompare.Model.Helpers;

namespace UABRO.RapidCompare.Model
{
    public class RapidPlanDoseVolumeHistograms
    {
        public string PlanPairId;
        public double PrescriptionDoseGy;
        public string ModelStructureId;
        public DoseVolumePoint[] LowerDvhEstimate;
        public DoseVolumePoint[] UpperDvhEstimate;
        public DoseVolumePoint[] ReferencePlanDvh;
        public DoseVolumePoint[] RapidPlanDvh;

        public bool HasDvhEstimates()
        {
            return LowerDvhEstimate != null && LowerDvhEstimate.Length > 1 && UpperDvhEstimate != null && UpperDvhEstimate.Length > 1;
        }
        public DoseVolumePoint[] GetDoseVolumeHistogram(DvhType dvhType)
        {
            switch (dvhType)
            {
                case DvhType.ReferencePlan:
                    return ReferencePlanDvh;
                case DvhType.RapidPlan:
                    return RapidPlanDvh;
                case DvhType.LowerEstimate:
                    return LowerDvhEstimate;
                case DvhType.UpperEstimate:
                    return UpperDvhEstimate;
                default:
                    throw new ArgumentException($"Invalid volume difference type {dvhType}");
            }
        }
        public double[] GetDose(DvhType dvhType, DoseUnit doseUnit)
        {
            var dv = GetDoseVolumeHistogram(dvhType);
            double gyToUnit;
            switch (doseUnit)
            {
                case DoseUnit.Gy:
                    gyToUnit = 1;
                    break;
                case DoseUnit.Percent:
                    gyToUnit = 100 / PrescriptionDoseGy;
                    break;
                default:
                    throw new ArgumentException($"Unrecognized dose unit {doseUnit}");
            }
            return dv.Select(h => h.DoseGy * gyToUnit).ToArray();
        }
        public double[] GetVolume(DvhType dvhType)
        {
            var dv = GetDoseVolumeHistogram(dvhType);
            return dv.Select(h => h.VolumePercent).ToArray();
        }
        public double[] GetVolumeAt(double[] dose, DoseUnit doseUnit, DvhType dvhType)
        {
            return DvhInterpolation.InterpolateVolume(GetDose(dvhType, doseUnit), GetVolume(dvhType), dose);
        }
        public double GetVolumeAt(double dose, DoseUnit doseUnit, DvhType dvhType)
        {
            return GetVolumeAt(new double[1] { dose }, doseUnit, dvhType)[0];
        }
        public double[] GetVolumeDifferenceAt(double[] dose, DoseUnit doseUnit, DvhType firstDvhType, DvhType secondDvhType)
        {
            var firstVolume = GetVolumeAt(dose, doseUnit, firstDvhType);
            var secondVolume = GetVolumeAt(dose, doseUnit, secondDvhType);

            double[] deltavol = new double[dose.Length];
            for (int i = 0; i < deltavol.Length; i++)
            {
                deltavol[i] = firstVolume[i] - secondVolume[i];
            }
            return deltavol;
        }
        public double GetVolumeDifferenceAt(double dose, DoseUnit doseUnit, DvhType firstDvhType, DvhType secondDvhType)
        {
            return GetVolumeDifferenceAt(new double[1] { dose }, doseUnit, firstDvhType, secondDvhType)[0];
        }

        public double[] GetDoseAt(double[] volume, DoseUnit doseUnit, DvhType dvhType)
        {
            return DvhInterpolation.InterpolateDose(GetDose(dvhType, doseUnit), GetVolume(dvhType), volume);
        }
        public double GetDoseAt(double volume, DoseUnit doseUnit, DvhType dvhType)
        {
            return DvhInterpolation.InterpolateDose(GetDose(dvhType, doseUnit), GetVolume(dvhType), new double[1] { volume })[0];
        }
        public double GetMeanDose(DoseUnit doseUnit, DvhType dvhType)
        {
            var vol = GetVolume(dvhType);
            var dose = GetDose(dvhType, doseUnit);
            double mndose = 0;
            for(int i=0;i<vol.Length-1;i++)
            {
                double deltavol = vol[i] - vol[i + 1];
                mndose += deltavol * (dose[i] + dose[i+1]) / 2;
            }
            if (vol.Last() > 0)
            {
                double deltavol = vol.Last();
                mndose += deltavol * (dose.Last() + DvhInterpolation.InterpolateDose(dose,vol, new double[1] { 0 })[0]) / 2;
            }
            return mndose / vol[0];
        }
    }
}
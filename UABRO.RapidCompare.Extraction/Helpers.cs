using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UABRO.RapidCompare.Model;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace UABRO.RapidCompare.Extraction
{
    public static class Helpers
    {
        public static RapidPlanModelDescription GetModelDescriptionFromRapidPlanDatabase(string modelId)
        {
            var planningModelDataBase = new PlanningModelQ.PM(RapidPlanHelper.PlanningDataBaseServer);
            var ids = planningModelDataBase.PlanningModels.Select(pm => pm.DisplayName).ToList();
            var modelQ = planningModelDataBase.PlanningModels.Where(pm => pm.DisplayName == modelId);
            if (modelQ.Count() == 0)
                throw new ArgumentException($"Model {modelId} does not exist in the RapidPlan database.");

            var planningModel = modelQ.Single();

            var model = new RapidPlanModelDescription();

            model.Id = planningModel.DisplayName;
            model.Version = planningModel.ModelVersion;
            model.Description = planningModel.Description;
            model.NumberOfTrainingCases = planningModel.TrainingCases.Where(t => t.IsActive).Count();
            model.ModelStructures = new List<ModelStructure>();
            foreach (var ms in planningModel.ModelStructures.Where(ms => !String.IsNullOrWhiteSpace(ms.PlanningObjective)))
            {
                var modelStructure = new ModelStructure();
                modelStructure.Name = ms.StructureName;
                modelStructure.Codes = ms.StructureCodes.Select(c => $"{c.Code} ({c.CodingScheme})").ToList();
                modelStructure.IsTarget = ms.StructureType == 1;

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(ms.PlanningObjective);

                modelStructure.PlanningObjectives = new List<RapidPlanObjective>();
                XmlNodeList elemList = doc.GetElementsByTagName("Objective");
                IEnumerator ienum = elemList.GetEnumerator();
                while (ienum.MoveNext())
                {
                    var node = (XmlNode)ienum.Current;
                    modelStructure.PlanningObjectives.Add(new RapidPlanObjective(node));
                }
                model.ModelStructures.Add(modelStructure);
            }

            var xdoc = RapidPlanHelper.GetObjectiveTemplate(modelId);
            model.NormalTissueObjective = RapidPlanHelper.GetNormalTissueObjective(xdoc);
            model.ImrtSmoothing = RapidPlanHelper.GetImrtSmoothing(xdoc);

            return model;
        }

        public static RapidPlanDoseVolumeHistograms GetRapidPlanDoseVolumeHistograms(string modelStructureId, string planStructureId, ExternalPlanSetup referenceplan, ExternalPlanSetup rapidplan)
        {
            RapidPlanDoseVolumeHistograms dvhs = new RapidPlanDoseVolumeHistograms();

            dvhs.ModelStructureId = modelStructureId;

            double dvhBinSize = 0.001; // Default bin size for DVH calculation in Gy - only used if DVH estimates are missing
            dvhs.PrescriptionDoseGy = referenceplan.TotalDose.Dose;
            if (referenceplan.TotalDose.Unit == DoseValue.DoseUnit.cGy)
            {
                dvhs.PrescriptionDoseGy /= 100;
                dvhBinSize *= 100;
            }
            else if (referenceplan.TotalDose.Unit != DoseValue.DoseUnit.Gy)
            {
                throw new ArgumentException("Invalid dose unit.");
            }

            var ests = rapidplan.DVHEstimates.Where(e => e.StructureId == planStructureId && (e.Type == DVHEstimateType.Lower || e.Type == DVHEstimateType.Upper));
            if (ests.Count() > 0)
            {

                var est = ests.Where(e => e.Type == DVHEstimateType.Lower && e.CurveData.Length > 1);
                if (est.Count() > 0)
                    dvhs.LowerDvhEstimate = CopyCurveData(est.Single().CurveData);

                est = ests.Where(e => e.Type == DVHEstimateType.Upper && e.CurveData.Length > 1);
                if (est.Count() > 0)
                    dvhs.UpperDvhEstimate = CopyCurveData(est.Single().CurveData);

                if (ests.Any(e => e.CurveData.Length > 1))
                    dvhBinSize = ests.Where(e => e.CurveData.Length > 1).Min(e => e.CurveData[1].DoseValue.Dose - e.CurveData[0].DoseValue.Dose);
                else
                    dvhBinSize = rapidplan.DVHEstimates.Where(e => e.CurveData.Length > 1).Min(e => e.CurveData[1].DoseValue.Dose - e.CurveData[0].DoseValue.Dose);
            }

            var roi = referenceplan.StructureSet.Structures.Where(s => s.Id == planStructureId).SingleOrDefault();
            if (roi == null)
                throw new ArgumentException($"Structure ID {planStructureId} is missing from reference plan structure set");
            var dvh = referenceplan.GetDVHCumulativeData(roi, DoseValuePresentation.Absolute, VolumePresentation.Relative, dvhBinSize);
            dvhs.ReferencePlanDvh = CopyCurveData(dvh.CurveData);

            roi = rapidplan.StructureSet.Structures.Where(s => s.Id == planStructureId).SingleOrDefault();
            if (roi == null)
                throw new ArgumentException($"Structure ID {planStructureId} is missing from RapidPlan structure set");
            dvh = rapidplan.GetDVHCumulativeData(roi, DoseValuePresentation.Absolute, VolumePresentation.Relative, dvhBinSize);
            dvhs.RapidPlanDvh = CopyCurveData(dvh.CurveData);

            return dvhs;
        }

        private static DoseVolumePoint[] CopyCurveData(DVHPoint[] curvedata)
        {
            double dosePerGy = 1;
            if (curvedata[0].DoseValue.Unit == DoseValue.DoseUnit.cGy)
                dosePerGy = 100;
            else if (curvedata[0].DoseValue.Unit != DoseValue.DoseUnit.Gy)
                throw new ArgumentException("Invalid dose unit.");

            if (curvedata[0].VolumeUnit != "%")
                throw new ArgumentException("Invalid volume unit.");

            var result = new DoseVolumePoint[curvedata.Count()];
            for (int i = 0; i < curvedata.Count(); i++)
            {
                result[i].DoseGy = curvedata[i].DoseValue.Dose / dosePerGy;
                result[i].VolumePercent = curvedata[i].Volume;
            }

            // Volume at zero dose should be 100%. However, there can be
            // round off error in the Eclipse calculated DVHs. Check that
            // round off error is within expected range and then correct
            // volume to 100%.
            if (Math.Abs(100 - curvedata[0].Volume) > 0.001)
                throw new Exception("DVH volume at zero dose is less than 100%");
            int j = 0;
            while(result[j].VolumePercent == curvedata[0].Volume)
            {
                result[j].VolumePercent = 100;
                j++;
            }

            return result;
        }
    }
}

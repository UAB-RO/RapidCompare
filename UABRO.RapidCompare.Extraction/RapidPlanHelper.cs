using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace UABRO.RapidCompare.Extraction
{
    class RapidPlanHelper
    {
        public static string PlanningDataBaseServer;
        /// <summary>
        /// Get the normal tissue objective parameters from the smoothing template
        /// </summary>
        /// <param name="xdoc"></param>
        /// <returns></returns>
        public static (string Mode, double Priority, double DistanceFromTargetBorder, double StartDose, double EndDose, double Falloff) GetNormalTissueObjective(XmlDocument xdoc)
        {
            XmlNodeList elemList = xdoc.GetElementsByTagName("NormalTissueObjective");
            if (elemList.Count == 0)
            {
                throw new ArgumentException($"Normal tissue objective is missing.");
            }
            else if (elemList.Count > 1)
            {
                throw new ArgumentException($"Multiple normal tissue objectives found.");
            }
            var node = (XmlNode)elemList.Item(0);

            string mode = node.SelectSingleNode("child::Mode").InnerText;
            double priority = double.Parse(node.SelectSingleNode("child::Priority").InnerText);
            double distanceFromTargetBorder = double.Parse(node.SelectSingleNode("child::DistanceFromTargetBorder").InnerText);
            double startDose = double.Parse(node.SelectSingleNode("child::StartDose").InnerText);
            double endDose = double.Parse(node.SelectSingleNode("child::EndDose").InnerText);
            double falloff = double.Parse(node.SelectSingleNode("child::FallOff").InnerText);

            return (Mode: mode, Priority: priority, DistanceFromTargetBorder: distanceFromTargetBorder, StartDose: startDose, EndDose: endDose, Falloff: falloff);
        }
        /// <summary>
        /// Get smoothing parameters from objective template
        /// </summary>
        /// <param name="xdoc"></param>
        /// <returns></returns>
        public static (double SmoothingX, double SmoothingY) GetImrtSmoothing(XmlDocument xdoc)
        {
            double smoothingX = double.Parse(xdoc.DocumentElement.SelectSingleNode("/ObjectiveTemplate/Helios/DefaultSmoothingX").InnerText);
            double smoothingY = double.Parse(xdoc.DocumentElement.SelectSingleNode("/ObjectiveTemplate/Helios/DefaultSmoothingY").InnerText);

            return (SmoothingX: smoothingX, SmoothingY: smoothingY);
        }
        /// <summary>
        /// Query the planning model database for the objective template.
        /// </summary>
        /// <param name="modelId"></param>
        /// <returns></returns>
        public static XmlDocument GetObjectiveTemplate(string modelId)
        {
            var planningModelDataBase = new PlanningModelQ.PM(server: PlanningDataBaseServer);
            var modelQ = planningModelDataBase.PlanningModels.Where(pm => pm.DisplayName == modelId);
            if (modelQ.Count() == 0)
                throw new ArgumentException($"Model {modelId} does not exist in the RapidPlan database.");

            var planningModel = modelQ.Single();
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(planningModel.PlanningObjective);
            return xdoc;
        }
        /// <summary>
        /// The ESAPI method CalculateDVHEstimates does not create the NTO or IMRT smoothing objectives. This helper gets the objectives from
        /// the planning model database and creates the DVH estimates, normal tissue objective, and smoothing objective.
        /// </summary>
        /// <param name="plan"></param>
        /// <param name="modelId"></param>
        /// <param name="targetDoseLevels"></param>
        /// <param name="structureMatches"></param>
        /// <returns></returns>
        public static bool ApplyRapidPlanModel(ExternalPlanSetup plan, string modelId, Dictionary<string, DoseValue> targetDoseLevels, Dictionary<string, string> structureMatches)
        {
            var result = plan.CalculateDVHEstimates(modelId, targetDoseLevels, structureMatches);
            if (!result.Success)
            {
                return false;
            }

            XmlDocument xdoc = GetObjectiveTemplate(modelId);

            var nto = GetNormalTissueObjective(xdoc);
            if (nto.Mode == "Auto")
            {
                plan.OptimizationSetup.AddAutomaticNormalTissueObjective(nto.Priority);
            }
            else if (nto.Mode == "Manual")
            {
                plan.OptimizationSetup.AddNormalTissueObjective(nto.Priority, nto.DistanceFromTargetBorder, nto.StartDose, nto.EndDose, nto.Falloff);
            }
            else if (nto.Mode != "Off")
            {
                throw new NotImplementedException($"Normal tissue objective mode {nto.Mode} is not implemented.");
            }

            var smoothing = GetImrtSmoothing(xdoc);
            foreach (var beam in plan.Beams.Where(b => !b.IsSetupField).Where(b => b.GantryDirection == GantryDirection.None))
            {
                plan.OptimizationSetup.AddBeamSpecificParameter(beam, smoothing.SmoothingX, smoothing.SmoothingY, false);
            }

            return true;
        }
    }
}

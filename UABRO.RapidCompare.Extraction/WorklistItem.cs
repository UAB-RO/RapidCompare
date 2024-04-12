using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using UABRO.RapidCompare.Model;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace UABRO.RapidCompare.Extraction
{
    public class WorklistItem
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static Application App;
        public static bool SaveModifications = false;
        public static bool RecalculateExistingPlan = false;
        public string RapidPlanModelId;
        public Dictionary<string, Dictionary<string, string>> CalculationModelOptions;
        public Dictionary<CalculationType, string> CalculationModels;
        public string PatientID;
        public string ReferenceCourseID;
        public string ReferencePlanID;
        public string RapidPlanCourseID;
        public string RapidPlanID;
        public string ItemId;
        public Dictionary<string, string> ModelStructureMatches;
        public Dictionary<string, DoseValue> TargetDoseLevels;
        private Patient _patient;
        public void OpenPatient()
        {
            _patient = App.OpenPatientById(PatientID);
            _patient.BeginModifications();
            _logger.Info($"Opened patient {_patient.Id}");
        }
        public ExternalPlanSetup GetReferencePlan()
        {
            return _patient.Courses.Where(c => c.Id == ReferenceCourseID).Single().ExternalPlanSetups.Where(p => p.Id == ReferencePlanID).Single();
        }

        public ExternalPlanSetup GetRapidPlan()
        {
            var plan = GetReferencePlan();

            // Open the course. Create it if it does not exist
            var course = _patient.Courses.Where(c => c.Id == RapidPlanCourseID).SingleOrDefault();
            if (course == null)
            {
                course = _patient.AddCourse();
                course.Id = RapidPlanCourseID;
                _logger.Info($"Created course {RapidPlanCourseID}");
            }

            // Get the RapidPlan
            ExternalPlanSetup rapidplan;
            rapidplan = course.ExternalPlanSetups.Where(p => p.Id == RapidPlanID).SingleOrDefault();
            if (rapidplan != null && !RecalculateExistingPlan && rapidplan.IsDoseValid && rapidplan.DVHEstimates.Where(e => e.Type == DVHEstimateType.Lower || e.Type == DVHEstimateType.Upper).Any())
                return rapidplan;

            if (rapidplan == null)
            {
                // RapidPlan does not exist - create it
                List<Structure> targets = new List<Structure>();
                foreach(var key in TargetDoseLevels.Keys)
                {
                    var tv = plan.StructureSet.Structures.Where(s => s.Id == key).Single();
                    targets.Add(tv);
                }
                rapidplan = PlanCopyHelper.CopyPlanSetup(plan, course, targets);
                rapidplan.Id = RapidPlanID;

                _logger.Info($"Created RapidPlan {rapidplan.Id}");
                if (SaveModifications)
                {
                    App.SaveModifications();
                    _logger.Info("New RapidPlan saved.");
                }
            }

            // Set calculation options
            foreach (CalculationType calculationType in CalculationModels.Keys)
            {
                string model = CalculationModels[calculationType];
                rapidplan.SetCalculationModel(calculationType, model);
                foreach (KeyValuePair<string, string> option in CalculationModelOptions[model])
                {
                    rapidplan.SetCalculationOption(model, option.Key, option.Value);
                }
            }

            // Get MLC ID. If the RapidPlan existed (rather than having been copied),
            // fixed gantry fields intended to be IMRT might not have an MLC,
            // so we need to infer it from the reference plan

            string mlcId;
            var beams = rapidplan.Beams.Where(b => !b.IsSetupField && b.MLC != null);
            if (beams.Count() > 0)
                mlcId = beams.First().MLC.Id;
            else
            {
                // Create a beam to get the default MLC, then delete it.
                var beam = rapidplan.Beams.Where(b => !b.IsSetupField).First();
                var energyMode = beam.EnergyModeDisplayName.Split('-');
                var machineParameters = new ExternalBeamMachineParameters(beam.TreatmentUnit.Id, energyMode[0], beam.DoseRate, "STATIC", energyMode.Length > 1 ? energyMode[1] : null);
                beam = rapidplan.AddMLCBeam(machineParameters, null, new VRect<double>(-5, -5, 5, 5), 0, 0, 0, beam.IsocenterPosition);
                mlcId = beam.MLC.Id;
                rapidplan.RemoveBeam(beam);
            }

            // Ensure that target dose levels are in system units
            DoseValue.DoseUnit systemUnit = plan.TotalDose.Unit;
            var targetDoseLevels = new Dictionary<string, DoseValue>(TargetDoseLevels);
            foreach (var key in TargetDoseLevels.Keys)
            {
                targetDoseLevels[key] = ConvertDoseUnits(TargetDoseLevels[key], systemUnit);
            }

            _logger.Info("Calculating DVH estimates for model {ModelId}", RapidPlanModelId);
            var success = RapidPlanHelper.ApplyRapidPlanModel(rapidplan, RapidPlanModelId, targetDoseLevels, ModelStructureMatches);
            if (!success)
            {
                _logger.Warn("Calculating DVH estimates failed for model {ModelId}", RapidPlanModelId);
                return null;
            }
            _logger.Info("DVH estimates created for model {ModelId}", RapidPlanModelId);
            if (SaveModifications)
            {
                App.SaveModifications();
                _logger.Info("DVH estimates saved.");
            }

            bool isIMRT = rapidplan.Beams.Where(b => !b.IsSetupField).All(b => b.GantryDirection == GantryDirection.None);

            _logger.Info("Starting optimizer");
            OptimizerResult optResult;

            if (isIMRT)
            {
                var optoptions = new OptimizationOptionsIMRT(1000, OptimizationOption.RestartOptimization, OptimizationConvergenceOption.TerminateIfConverged, OptimizationIntermediateDoseOption.UseIntermediateDose, mlcId);
                optResult = rapidplan.Optimize(optoptions);
            }
            else
            {
                var optoptions = new OptimizationOptionsVMAT(OptimizationIntermediateDoseOption.UseIntermediateDose, mlcId);
                optResult = rapidplan.OptimizeVMAT(optoptions);
            }

            if (!optResult.Success)
            {
                _logger.Warn("Optimization not successful.");
                return null;
            }

            _logger.Info("Optimization result total objective function value = {TotalObjectiveFunctionValue}", optResult.TotalObjectiveFunctionValue);
            foreach (var sov in optResult.StructureObjectiveValues.Where(o => o.Value > 0))
                _logger.Info("Optimization result {StructureId} objective function value = {TotalObjectiveFunctionValue}", sov.Structure.Id, sov.Value);

            if (SaveModifications)
            {
                App.SaveModifications();
                _logger.Info("Optimization results saved.");
            }

            _logger.Info("Starting dose calculation");
            CalculationResult calcResult;

            if (isIMRT)
                calcResult = rapidplan.CalculateLeafMotionsAndDose();
            else
                calcResult = rapidplan.CalculateDose();

            if (!calcResult.Success)
            {
                _logger.Warn("Dose calculation not successful.");
                return null;
            }

            _logger.Info("Dose calculation complete");

            var normalizationID = targetDoseLevels.First().Key;
            var normalizationDose = targetDoseLevels.First().Value;
            var ptv = plan.StructureSet.Structures.Where(s => s.Id == normalizationID).Single();
            var normvol = plan.GetVolumeAtDose(ptv, normalizationDose, VolumePresentation.Relative);
            ptv = rapidplan.StructureSet.Structures.Where(s => s.Id == normalizationID).Single();
            var rpdose = rapidplan.GetDoseAtVolume(ptv, normvol, VolumePresentation.Relative, DoseValuePresentation.Absolute);
            rapidplan.PlanNormalizationValue = 100 * rpdose.Dose / normalizationDose.Dose;

            if (SaveModifications)
            {
                App.SaveModifications();
                _logger.Info("Calculated plan saved.");
            }

            return rapidplan;
        }

        public List<RapidPlanDoseVolumeHistograms> GetRapidPlanDoseVolumeHistograms()
        {
            var rapidPlanDvhs = new List<RapidPlanDoseVolumeHistograms>();
            var plan = GetReferencePlan();
            var rapidplan = GetRapidPlan();
            if (rapidplan != null)
            {
                foreach (var planStructureId in ModelStructureMatches.Keys)
                {
                    RapidPlanDoseVolumeHistograms rp = Helpers.GetRapidPlanDoseVolumeHistograms(ModelStructureMatches[planStructureId], planStructureId, plan, rapidplan);
                    rp.PlanPairId = ItemId;
                    rapidPlanDvhs.Add(rp);
                }
            }
            return rapidPlanDvhs;
        }

        public void ClosePatient()
        {
            App.ClosePatient();
            _logger.Info($"Closed patient {PatientID}");
        }
        public void PruneModelStructures(List<string> modelStructuresToKeep)
        {
            foreach (var key in ModelStructureMatches.Keys.ToList())
            {
                if (!modelStructuresToKeep.Contains(ModelStructureMatches[key]))
                {
                    string modelStructure = ModelStructureMatches[key];
                    ModelStructureMatches.Remove(key);
                    _logger.Info($"Removed {key} corresponding to model structure {modelStructure} from model structure matches.");
                    if (TargetDoseLevels.ContainsKey(key))
                    {
                        TargetDoseLevels.Remove(key);
                        _logger.Info($"Removed {key} from target dose levels.");
                    }
                }
            }
        }
        static internal DoseValue ConvertDoseUnits(DoseValue dose, DoseValue.DoseUnit unit)
        {
            double gyToUnit = 1;
            if (unit == DoseValue.DoseUnit.cGy)
            {
                gyToUnit = 100;
            }
            else if (unit != DoseValue.DoseUnit.Gy)
            {
                throw new ArgumentException($"Target dose unit {unit.ToString()} is invalid.");
            }

            double doseGy = dose.Dose;
            if (dose.Unit == DoseValue.DoseUnit.cGy)
            {
                doseGy /= 100;
            }
            else if (dose.Unit != DoseValue.DoseUnit.Gy)
            {
                throw new ArgumentException($"Dose value unit {dose.Unit.ToString()} is invalid.");
            }

            return new DoseValue(gyToUnit * doseGy, unit);
        }
    }

}
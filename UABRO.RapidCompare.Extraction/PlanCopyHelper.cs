using System;
using System.Collections.Generic;
using System.Linq;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace UABRO.RapidCompare.Extraction
{
    public static class PlanCopyHelper
    {
        private static double jawMargin = 3;

        public static ExternalPlanSetup CopyPlanSetup(ExternalPlanSetup sourcePlan, Course destCourse, List<Structure> targets)
        {
            ExternalPlanSetup copyPlan = destCourse.AddExternalPlanSetup(sourcePlan.StructureSet, null, sourcePlan.PrimaryReferencePoint);
            copyPlan.SetPrescription(sourcePlan.NumberOfFractions.Value, sourcePlan.DosePerFraction, sourcePlan.TreatmentPercentage);

            CopyArcBeams(copyPlan, sourcePlan.Beams.ToList());
            CopyStaticBeams(copyPlan, sourcePlan.Beams.ToList());
            SetJawPositions(copyPlan.Beams.ToList(), targets);

            return copyPlan;
        }

        public static void CopyStaticBeams(ExternalPlanSetup destPlan, List<Beam> sourceBeams)
        {
            var beams = sourceBeams.Where(b => !b.IsSetupField)
                .Where(b => b.GantryDirection == GantryDirection.None)
                .GroupBy(b => new
                {
                    b.ControlPoints.First().GantryAngle,
                    b.ControlPoints.First().CollimatorAngle,
                    b.ControlPoints.First().PatientSupportAngle
                })
                .Select(g => g.First())
                .ToList();

            foreach (var sourceBeam in beams)
            {
                var machineParameters = CopyMachineParameters(sourceBeam);

                var jawPositions = new VRect<double>(-50, -50, 50, 50);

                double gantryAngle = sourceBeam.ControlPoints.First().GantryAngle;
                double gantryStop = sourceBeam.ControlPoints.Last().GantryAngle;
                double collimatorAngle = sourceBeam.ControlPoints.First().CollimatorAngle;
                double patientSupportAngle = sourceBeam.ControlPoints.First().PatientSupportAngle;

                destPlan.AddStaticBeam(machineParameters, jawPositions, collimatorAngle, gantryAngle, patientSupportAngle, sourceBeam.IsocenterPosition);
            }
        }
        public static void CopyArcBeams(ExternalPlanSetup destPlan, List<Beam> sourceBeams)
        {
            var beams = sourceBeams.Where(b => !b.IsSetupField)
               .Where(b => b.GantryDirection != GantryDirection.None)
               .ToList();

            foreach (var sourceBeam in beams)
            {
                var machineParameters = CopyMachineParameters(sourceBeam);

                var jawPositions = new VRect<double>(-50, -50, 50, 50);

                double gantryAngle = sourceBeam.ControlPoints.First().GantryAngle;
                double gantryStop = sourceBeam.ControlPoints.Last().GantryAngle;
                double collimatorAngle = sourceBeam.ControlPoints.First().CollimatorAngle;
                double patientSupportAngle = sourceBeam.ControlPoints.First().PatientSupportAngle;

                destPlan.AddArcBeam(machineParameters, jawPositions, collimatorAngle, gantryAngle, gantryStop, sourceBeam.GantryDirection, patientSupportAngle, sourceBeam.IsocenterPosition);
            }
        }
        public static ExternalBeamMachineParameters CopyMachineParameters(Beam beam)
        {
            var energyMode = beam.EnergyModeDisplayName.Split('-');
            return new ExternalBeamMachineParameters(beam.TreatmentUnit.Id, energyMode[0], beam.DoseRate, beam.Technique.Id,
                energyMode.Length > 1 ? energyMode[1] : null);
        }
        public static void SetJawPositions(List<Beam> beams, List<Structure> targets)
        {
            foreach (var beam in beams)
            {
                
                var thisjaw = BeamGeometry.GetJawPositions(
                    targets, beam.IsocenterPosition, 
                    beam.ControlPoints.First().GantryAngle, 
                    beam.ControlPoints.First().CollimatorAngle, 
                    beam.ControlPoints.First().PatientSupportAngle);
                double x1 = thisjaw.X1;
                double y1 = thisjaw.Y1;
                double x2 = thisjaw.X2;
                double y2 = thisjaw.Y2;
                if (beam.GantryDirection != GantryDirection.None)
                {
                    double gantryFirst = beam.ControlPoints.First().GantryAngle;
                    double gantryLast = beam.ControlPoints.Last().GantryAngle;

                    double vGantryFirst = (540 - gantryFirst) % 360;
                    double vGantryLast = (540 - gantryLast) % 360;
                    int controlPointCount = (int)Math.Ceiling(Math.Abs(vGantryLast - vGantryFirst)) + 1;

                    double dgantry = (vGantryLast - vGantryFirst) / (controlPointCount - 1);
                    double[] gantryAngles = new double[controlPointCount];
                    for (int i = 0; i < gantryAngles.Length; i++)
                        gantryAngles[i] = (540 - vGantryFirst - i * dgantry) % 360;

                    foreach (var gantry in gantryAngles)
                    {
                        thisjaw = BeamGeometry.GetJawPositions(targets, beam.IsocenterPosition, gantry, beam.ControlPoints.First().CollimatorAngle, beam.ControlPoints.First().PatientSupportAngle);

                        x1 = Math.Min(x1, thisjaw.X1);
                        y1 = Math.Min(y1, thisjaw.Y1);
                        x2 = Math.Max(x2, thisjaw.X2);
                        y2 = Math.Max(y2, thisjaw.Y2);
                    }
                }
                var jawPositions = new VRect<double>(x1-jawMargin, y1-jawMargin, x2+jawMargin, y2+jawMargin);

                var beamParams = beam.GetEditableParameters();
                beamParams.SetJawPositions(jawPositions);
                beam.ApplyParameters(beamParams);
            }
        }
    }
}

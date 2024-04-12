using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text;
using System.Threading.Tasks;

namespace UABRO.RapidCompare.Model
{
    public class ModelStructure
    {
        public string Name;
        public List<string> Codes;
        public bool IsTarget;
        public List<RapidPlanObjective> PlanningObjectives;
    }
    public class RapidPlanModelDescription
    {
        public string Id;
        public string Description;
        public string Version;
        public int NumberOfTrainingCases;
        public List<ModelStructure> ModelStructures;
        public (string Mode, double Priority, double DistanceFromTargetBorder, double StartDose, double EndDose, double Falloff) NormalTissueObjective;
        public (double SmoothingX, double SmoothingY) ImrtSmoothing;
    }
}

using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UABRO.RapidCompare.Model
{
    public enum ObjectiveType
    {
        Point = 0,
        Mean = 2,
        gEUD = 3,
        LinePreferringOar = 11,
        LinePreferringTarget = 1
    }

    public enum ObjectiveOperator
    {
        LessThan = 0,
        GreaterThan = 1,
        Equal = 2
    }
    public enum DoseUnit
    {
        Gy = 0,
        Percent = 1
    }

    public class RapidPlanObjective
    {
        public ObjectiveType Type;
        public ObjectiveOperator Operator;
        public double? Volume;
        public double? Dose;
        public DoseUnit DoseUnit;
        public int? Priority;
        public double? ParameterA;

        public RapidPlanObjective() { }
        public RapidPlanObjective(XmlNode node)
        {
            var val = node.SelectSingleNode("child::Type");
            Type = (ObjectiveType)int.Parse(val.InnerText);
            val = node.SelectSingleNode("child::Operator");
            Operator = (ObjectiveOperator)int.Parse(val.InnerText);

            val = node.SelectSingleNode("child::Volume");
            Volume = null;
            if (!String.IsNullOrWhiteSpace(val.InnerText))
                Volume = double.Parse(val.InnerText);

            val = node.SelectSingleNode("child::Dose");
            Dose = null;
            if (!String.IsNullOrWhiteSpace(val.InnerText))
                Dose = double.Parse(val.InnerText);

            DoseUnit = DoseUnit.Gy;
            if (Dose.HasValue)
            {
                val = node.SelectSingleNode("child::DoseUnit");
                DoseUnit = (DoseUnit)int.Parse(val.InnerText);
            }

            val = node.SelectSingleNode("child::Priority");
            Priority = null;
            if (!String.IsNullOrWhiteSpace(val.InnerText))
                Priority = int.Parse(val.InnerText);

            val = node.SelectSingleNode("child::ParameterA");
            ParameterA = null;
            if (!String.IsNullOrWhiteSpace(val.InnerText))
                ParameterA = double.Parse(val.InnerText);
        }
    }
}

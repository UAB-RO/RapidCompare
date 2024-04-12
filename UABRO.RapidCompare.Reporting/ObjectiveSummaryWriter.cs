using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using MigraDoc.DocumentObjectModel;
using System.Xml.XPath;
using PdfSharp.Pdf.Content.Objects;
using System.ComponentModel;
using UABRO.RapidCompare.Model;
using UABRO.RapidCompare.Analysis;

namespace UABRO.RapidCompare.Reporting
{
    static class ObjectiveSummaryWriter
    {
        public static void WriteOptimizationObjectivesTable(Section section, RapidPlanModelDvhSet rapidPlanModelDvhSet, string modelStructureId)
        {
            var table = section.AddTable();
            table.Style = "Table";
            table.Borders.Width = 0.25;
            table.Borders.Left.Width = 0.5;
            table.Borders.Right.Width = 0.5;
            table.Rows.LeftIndent = 0;

            var column = table.AddColumn("2.5cm");
            column.Format.Alignment = ParagraphAlignment.Left;


            column = table.AddColumn("4cm");
            column.Format.Alignment = ParagraphAlignment.Center;

            column = table.AddColumn("4cm");
            column.Format.Alignment = ParagraphAlignment.Center;

            column = table.AddColumn("6cm");
            column.Format.Alignment = ParagraphAlignment.Center;

            var row = table.AddRow();
            row.Cells[0].AddParagraph("Metric");
            row.Cells[1].AddParagraph("Reference Plan");
            row.Cells[2].AddParagraph("RapidPlan");
            row.Cells[3].AddParagraph("Difference");

            var s = rapidPlanModelDvhSet.RapidPlanModel.ModelStructures.Where(ms => ms.Name == modelStructureId).Single();

            List<RapidPlanObjective> objs = s.PlanningObjectives;
            var dvhs = rapidPlanModelDvhSet.RapidPlanDvhsByStructure[s.Name].ToList();
            foreach (var obj in objs.Where(o => o.Type == ObjectiveType.Point))
            {
                if (obj.Dose != null)
                {
                    double dose = obj.Dose.Value;

                    row = table.AddRow();
                    row.Cells[0].AddParagraph($"V{dose}{obj.DoseUnit}[%]");

                    var bwc = DoseVolumeMetrics.GetVolumeBoxWhiskerStatistics(dvhs, dose, obj.DoseUnit, DvhType.ReferencePlan);
                    row.Cells[1].AddParagraph($"{bwc.Median:0.0} [{bwc.LowerQuartile:0.0},{bwc.UpperQuartile:0.0}]");

                    bwc = DoseVolumeMetrics.GetVolumeBoxWhiskerStatistics(dvhs, dose, obj.DoseUnit, DvhType.RapidPlan);
                    row.Cells[2].AddParagraph($"{bwc.Median:0.0} [{bwc.LowerQuartile:0.0},{bwc.UpperQuartile:0.0}]");

                    bwc = DoseVolumeMetrics.GetVolumeDifferenceBoxWhiskerStatistics(dvhs, dose, obj.DoseUnit, DvhType.RapidPlan, DvhType.ReferencePlan);
                    double p = DoseVolumeMetrics.GetVolumeDifferenceOneSidedSignTestPValue(dvhs, dose, obj.DoseUnit, DvhType.RapidPlan, DvhType.ReferencePlan);
                    row.Cells[3].AddParagraph($"{bwc.Median:0.0} [{bwc.LowerQuartile:0.0},{bwc.UpperQuartile:0.0}] (p = {p:0.000}) ({bwc.MildOutliers.Count()})");

                }

                if (obj.Volume != null)
                {
                    double volume = obj.Volume.Value;

                    row = table.AddRow();
                    string volstring = $"D{volume}%";
                    if (volume == 100)
                    {
                        volstring = "DMin";
                    }
                    else if (volume == 0)
                    {
                        volstring = "DMax";
                    }
                    row.Cells[0].AddParagraph($"{volstring}[{obj.DoseUnit}]");

                    var bwc = DoseVolumeMetrics.GetDoseBoxWhiskerStatistics(dvhs, volume, obj.DoseUnit, DvhType.ReferencePlan);
                    row.Cells[1].AddParagraph($"{bwc.Median:0.0} [{bwc.LowerQuartile:0.0},{bwc.UpperQuartile:0.0}]");

                    bwc = DoseVolumeMetrics.GetDoseBoxWhiskerStatistics(dvhs, volume, obj.DoseUnit, DvhType.RapidPlan);
                    row.Cells[2].AddParagraph($"{bwc.Median:0.0} [{bwc.LowerQuartile:0.0},{bwc.UpperQuartile:0.0}]");

                    bwc = DoseVolumeMetrics.GetDoseDifferenceBoxWhiskerStatistics(dvhs, volume, obj.DoseUnit, DvhType.RapidPlan, DvhType.ReferencePlan);
                    double p = DoseVolumeMetrics.GetDoseDifferenceOneSidedSignTestPValue(dvhs, volume, obj.DoseUnit, DvhType.RapidPlan, DvhType.ReferencePlan);
                    row.Cells[3].AddParagraph($"{bwc.Median:0.0} [{bwc.LowerQuartile:0.0},{bwc.UpperQuartile:0.0}] (p = {p:0.000}) ({bwc.ExtremeOutliers.Count()})");
                }
            }

            if(objs.Any(o => o.Type == ObjectiveType.Mean))
            {
                var obj = objs.Where(o => o.Type == ObjectiveType.Mean).First();

                row = table.AddRow();
                row.Cells[0].AddParagraph($"DMean[{obj.DoseUnit}]");

                var bwc = DoseVolumeMetrics.GetMeanDoseBoxWhiskerStatistics(dvhs, obj.DoseUnit, DvhType.ReferencePlan);
                row.Cells[1].AddParagraph($"{bwc.Median:0.0} [{bwc.LowerQuartile:0.0},{bwc.UpperQuartile:0.0}]");

                bwc = DoseVolumeMetrics.GetMeanDoseBoxWhiskerStatistics(dvhs, obj.DoseUnit, DvhType.RapidPlan);
                row.Cells[2].AddParagraph($"{bwc.Median:0.0} [{bwc.LowerQuartile:0.0},{bwc.UpperQuartile:0.0}]");

                bwc = DoseVolumeMetrics.GetMeanDoseDifferenceBoxWhiskerStatistics(dvhs, obj.DoseUnit, DvhType.RapidPlan, DvhType.ReferencePlan);
                double p = DoseVolumeMetrics.GetMeanDoseDifferenceOneSidedSignTestPValue(dvhs, obj.DoseUnit, DvhType.RapidPlan, DvhType.ReferencePlan);
                row.Cells[3].AddParagraph($"{bwc.Median:0.0} [{bwc.LowerQuartile:0.0},{bwc.UpperQuartile:0.0}] (p = {p:0.000})");

            }
        }

    }
}

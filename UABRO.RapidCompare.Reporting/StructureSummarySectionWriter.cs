using System;
using System.Collections.Generic;
using Drawing = System.Drawing;
using System.Linq;
using MigraDoc.DocumentObjectModel;
using ScottPlot;
using UABRO.RapidCompare.Model;
using UABRO.RapidCompare.Analysis;

namespace UABRO.RapidCompare.Reporting
{
    static class StructureSummarySectionWriter
    {
        public static void WriteDoseVolumeMetricDifferencePlot(Section section, RapidPlanModelDvhSet rapidPlanModelDvhSet)
        {
            var paragraph = section.AddParagraph("Dose-volume metric summary");
            paragraph.Style = StyleNames.Heading1.ToString();
            paragraph = section.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            List<DoseVolumeMetricValues> results = new List<DoseVolumeMetricValues>();
            List<string> missingStructures = new List<string>();
            foreach (var id in rapidPlanModelDvhSet.StructureIds)
            {
                var s = rapidPlanModelDvhSet.RapidPlanModel.ModelStructures.Where(ms => ms.Name == id);
                if (s.Count() > 0)
                {
                    results.AddRange(DoseVolumeMetricValues.AnalyzePointObjectives(rapidPlanModelDvhSet.RapidPlanDvhsByStructure[id].ToList(), s.Single().PlanningObjectives));
                }
                else
                {
                    missingStructures.Add(id);
                }
            }

            double printWidth = section.PageSetup.PageWidth.Inch - section.PageSetup.RightMargin.Inch - section.PageSetup.LeftMargin.Inch;
            printWidth *= 0.97;

            var plt = new ReportPlot(printWidth, results.Count / 4);

            results.Reverse();
            for(int i=0;i<results.Count;i++)
            {
                var bwc = new BoxWhiskerComponents(results[i].DifferenceValues);
                plt.PlotPoint(bwc.Median, i, color: Drawing.Color.Black);
                plt.PlotLine(bwc.LowerQuartile, i, bwc.UpperQuartile, i, color: Drawing.Color.Black);
            }
            int n = results.Select(r => r.StructureId.Length + r.Label.Length).Max();
            plt.YTicks(DataGen.Consecutive(results.Count), results.Select(r=>r.StructureId + new string('\x00B7', n+3-r.StructureId.Length-r.Label.Length) + r.Label).ToArray());
            plt.Layout(yScaleWidthInches: 2.5);

            plt.AddPlotToParagraph(paragraph);

            if (missingStructures.Count > 0)
            {
                paragraph = section.AddParagraph($"No matches in any plans for structures {string.Join(", ",missingStructures)}.");
                paragraph.Format.Font.Italic = true;
                paragraph.Format.Font.Color = Color.FromRgb(1, 0, 0);
            }
        }
        public static void WriteVolumeDifferencePlot(Section section, RapidPlanModelDvhSet rapidPlanModelDvhSet, string modelStructureId)
        {
            var maxdose = Math.Ceiling(DoseVolumeMetrics.GetMaximumDose(rapidPlanModelDvhSet.RapidPlanDvhs));
            int n = (int)maxdose + 1;
            var dose = new double[n];
            for (int i = 0; i < n; i++)
                dose[i] = i;

            var dvhs = rapidPlanModelDvhSet.RapidPlanDvhsByStructure[modelStructureId].ToList();
            var boxWhiskerComponents = DoseVolumeMetrics.GetVolumeDifferenceBoxWhiskerStatistics(dvhs, dose, DoseUnit.Gy, DvhType.RapidPlan, DvhType.ReferencePlan);
            var pValue = DoseVolumeMetrics.GetVolumeDifferenceOneSidedSignTestPValue(dvhs, dose, DoseUnit.Gy, DvhType.RapidPlan, DvhType.ReferencePlan);

            var paragraph = section.AddParagraph("DVH Volume difference");
            paragraph.Style = StyleNames.Heading2.ToString();
            paragraph = section.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Left;

            double printWidth = section.PageSetup.PageWidth.Inch - section.PageSetup.RightMargin.Inch - section.PageSetup.LeftMargin.Inch;
            printWidth *= 0.97;

            var plt = new ReportPlot(printWidth, 0.75 * printWidth);

            plt.PlotScatter(dose, boxWhiskerComponents.Select(s => s.Median).ToArray(), markerShape: MarkerShape.none, lineWidth: 2.0, color: Drawing.Color.Blue, label: "Median");
            plt.PlotScatter(dose, boxWhiskerComponents.Select(s => s.UpperQuartile).ToArray(), markerShape: MarkerShape.none, lineWidth: 1.0, color: Drawing.Color.Black, label: "Upper Quartile");
            plt.PlotScatter(dose, boxWhiskerComponents.Select(s => s.LowerQuartile).ToArray(), markerShape: MarkerShape.none, lineWidth: 1.0, color: Drawing.Color.Black, label: "Lower Quartile");

            for (int i = 0; i < dose.Length; i++)
            {
                foreach (var y in boxWhiskerComponents[i].ExtremeOutliers)
                    plt.PlotPoint(dose[i], y, color: Drawing.Color.Black);
                foreach (var y in boxWhiskerComponents[i].MildOutliers)
                    plt.PlotPoint(dose[i], y, color: Drawing.Color.Black);
            }

            var xx = new List<double>();
            var yy = new List<double>();
            for (int i = 0; i < dose.Length; i++)
            {
                if (pValue[i] < 0.05)
                {
                    xx.Add(dose[i]);
                    yy.Add(boxWhiskerComponents[i].Median);
                }
            }
            if (xx.Count() > 0)
                plt.PlotScatter(xx.ToArray(), yy.ToArray(), markerShape: MarkerShape.openCircle, lineWidth: 0, color: Drawing.Color.Red);

            plt.XLabel("Dose [Gy]");
            plt.YLabel("Volume Difference [%]");
            plt.Title("RapidPlan - Reference plan volume difference for " + modelStructureId, fontSize: 16);

            plt.AddPlotToParagraph(paragraph);
        }

        public static void WriteOptimizationObjectivesTable(Section section, RapidPlanModelDvhSet rapidPlanModelDvhSet, string modelStructureId)
        {
            var paragraph = section.AddParagraph("Dose-volume metric summary table");
            paragraph.Style = StyleNames.Heading2.ToString();

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

            var results = DoseVolumeMetricValues.AnalyzePointObjectives(rapidPlanModelDvhSet.RapidPlanDvhsByStructure[modelStructureId].ToList(), s.PlanningObjectives);

            foreach (var result in results)
            {

                row = table.AddRow();
                row.Cells[0].AddParagraph(result.Label);

                var bwc = new BoxWhiskerComponents(result.ReferencePlan);
                row.Cells[1].AddParagraph($"{bwc.Median:0.0} [{bwc.LowerQuartile:0.0},{bwc.UpperQuartile:0.0}]");

                bwc = new BoxWhiskerComponents(result.RapidPlan);
                row.Cells[2].AddParagraph($"{bwc.Median:0.0} [{bwc.LowerQuartile:0.0},{bwc.UpperQuartile:0.0}]");

                bwc = new BoxWhiskerComponents(result.DifferenceValues);
                double p = Statistics.SignTestPValue(result.DifferenceValues).pRight;
                row.Cells[3].AddParagraph($"{bwc.Median:0.0} [{bwc.LowerQuartile:0.0},{bwc.UpperQuartile:0.0}] (p = {p:0.000}) ({bwc.MildOutliers.Count()})");

            }
        }
        public static void WriteOptimizationObjectivesBoxWhiskerPlots(Section section, RapidPlanModelDvhSet rapidPlanModelDvhSet, string modelStructureId)
        {
            section.AddPageBreak();
            var paragraph = section.AddParagraph("Dose-volume metric box whisker plots");
            paragraph.Style = StyleNames.Heading2.ToString();

            paragraph = section.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Left;

            double printWidth = section.PageSetup.PageWidth.Inch - section.PageSetup.RightMargin.Inch - section.PageSetup.LeftMargin.Inch;
            printWidth /= 3;
            printWidth *= 0.97;

            var s = rapidPlanModelDvhSet.RapidPlanModel.ModelStructures.Where(ms => ms.Name == modelStructureId).Single();
            var results = DoseVolumeMetricValues.AnalyzePointObjectives(rapidPlanModelDvhSet.RapidPlanDvhsByStructure[modelStructureId].ToList(), s.PlanningObjectives);

            foreach (var result in results)
            {
                var plt = new ReportPlot(printWidth, printWidth);
                
                var bwc = new BoxWhiskerComponents(result.ReferencePlan);
                plt.PlotBoxWhisker(1, bwc, color: Drawing.Color.Black);

                bwc = new BoxWhiskerComponents(result.RapidPlan);
                plt.PlotBoxWhisker(2, bwc, color: Drawing.Color.Black);

                plt.Ticks(displayTicksXminor: false);
                plt.XTicks(new double[] { 1, 2 }, new string[] { "Reference", "RapidPlan" });
                plt.YLabel(result.Label);
                plt.Title(result.Label);

                plt.AddPlotToParagraph(paragraph);
            }
        }

        public static void WriteDVHDifferenceBarCharts(Section section, RapidPlanModelDvhSet rapidPlanModelDvhSet, string modelStructureId)
        {
            section.AddPageBreak();
            var paragraph = section.AddParagraph("Dose-volume metric differences by plan");
            paragraph.Style = StyleNames.Heading2.ToString();

            paragraph = section.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            var s = rapidPlanModelDvhSet.RapidPlanModel.ModelStructures.Where(ms => ms.Name == modelStructureId).Single();

            var results = DoseVolumeMetricValues.AnalyzePointObjectives(rapidPlanModelDvhSet.RapidPlanDvhsByStructure[modelStructureId].ToList(), s.PlanningObjectives);

            double printWidth = section.PageSetup.PageWidth.Inch - section.PageSetup.RightMargin.Inch - section.PageSetup.LeftMargin.Inch;
            printWidth *= 0.97;
            foreach (var result in results)
            {
                double dvhPanelAspectRatio = 3;

                var plt = new ReportPlot(printWidth, printWidth / dvhPanelAspectRatio);

                double[] xs = DataGen.Consecutive(result.DifferenceValues.Length);

                Drawing.Color fillColor;
                Drawing.Color negativeColor;
                switch (result.ObjectiveOperator)
                {
                    case ObjectiveOperator.GreaterThan:
                        fillColor = Drawing.Color.Green;
                        negativeColor = Drawing.Color.Red;
                        break;
                    case ObjectiveOperator.LessThan:
                        fillColor = Drawing.Color.Red;
                        negativeColor = Drawing.Color.Green;
                        break;
                    case ObjectiveOperator.Equal:
                        fillColor = Drawing.Color.CornflowerBlue;
                        negativeColor = Drawing.Color.CornflowerBlue;
                        break;
                    default:
                        fillColor = Drawing.Color.Black;
                        negativeColor = Drawing.Color.Black;
                        break;
                }
                plt.PlotBar(xs, result.DifferenceValues, fillColor: fillColor, negativeColor: negativeColor);

                plt.XTicks(rapidPlanModelDvhSet.RapidPlanDvhsByStructure[modelStructureId].ToList().Select(dvh => dvh.PlanPairId).ToArray());
                plt.YLabel($"Difference ({result.Units})");
                plt.Title(result.Label);

                plt.AddPlotToParagraph(paragraph);
            }
        }
        public static void WriteAllDVHs(Section section, RapidPlanModelDvhSet rapidPlanModelDvhSet, string modelStructureId)
        {
            section.AddPageBreak();
            var paragraph = section.AddParagraph("Dose-volume histograms");
            paragraph.Style = StyleNames.Heading2.ToString();

            var dvhs = rapidPlanModelDvhSet.RapidPlanDvhsByStructure[modelStructureId].ToList();

            paragraph = section.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            foreach (var dvh in dvhs)
            {
                var plt = PlotDVHPanel(section, dvh);

                if (dvh == dvhs.First())
                    plt.Legend(fontSize: 10, location: legendLocation.upperRight);

                plt.AddPlotToParagraph(paragraph);
                plt = PlotDVHDifferencePanel(section, dvh);

                if (dvh == dvhs.First())
                    plt.Legend(fontSize: 10, location: legendLocation.upperRight);

                plt.AddPlotToParagraph(paragraph);
            }
        }
        private static ReportPlot PlotDVHPanel(Section section, RapidPlanDoseVolumeHistograms dvh)
        {
            double printWidth = section.PageSetup.PageWidth.Inch - section.PageSetup.RightMargin.Inch - section.PageSetup.LeftMargin.Inch;
            printWidth *= 0.97 / 2;

            var plt = new ReportPlot(printWidth, printWidth);

            var xs = dvh.ReferencePlanDvh.Select(d => d.DoseGy).ToArray();
            var ys = dvh.ReferencePlanDvh.Select(d => d.VolumePercent).ToArray();
            plt.PlotScatter(xs, ys, label: "Reference", markerShape: MarkerShape.none, lineWidth: 2.0);

            xs = dvh.RapidPlanDvh.Select(d => d.DoseGy).ToArray();
            ys = dvh.RapidPlanDvh.Select(d => d.VolumePercent).ToArray();
            plt.PlotScatter(xs, ys, label: "RapidPlan", markerShape: MarkerShape.none, lineWidth: 2.0);
            
            if (dvh.LowerDvhEstimate != null && dvh.UpperDvhEstimate != null)
            {
                var xd = dvh.LowerDvhEstimate.Select(d => d.DoseGy).ToList();
                xd.AddRange(dvh.UpperDvhEstimate.Select(d => d.DoseGy).Reverse().ToList());
                var yd = dvh.LowerDvhEstimate.Select(d => d.VolumePercent).ToList();
                yd.AddRange(dvh.UpperDvhEstimate.Select(d => d.VolumePercent).Reverse().ToList());
                plt.PlotPolygon(xd.ToArray(), yd.ToArray(), fillColor: Drawing.Color.LightSteelBlue, fillAlpha: 0.2, label: "DVH estimate");
            }

            plt.Ticks(fontSize: 10);
            plt.XLabel("Dose [Gy]", fontSize: 12);
            plt.YLabel("Volume [%]", fontSize: 12);
            plt.Title(dvh.PlanPairId, fontSize: 12);

            return plt;
        }
        private static ReportPlot PlotDVHDifferencePanel(Section section, RapidPlanDoseVolumeHistograms dvh)
        {
            double printWidth = section.PageSetup.PageWidth.Inch - section.PageSetup.RightMargin.Inch - section.PageSetup.LeftMargin.Inch;
            printWidth *= 0.97 / 2;

            var plt = new ReportPlot(printWidth, printWidth);

            var xs = dvh.ReferencePlanDvh.Select(d => d.DoseGy).ToArray();

            var ys = dvh.GetVolumeDifferenceAt(xs, DoseUnit.Gy, DvhType.RapidPlan, DvhType.ReferencePlan);
            plt.PlotScatter(xs, ys, label: "RapidPlan", markerShape: MarkerShape.none, lineWidth: 2.0);

            if (dvh.LowerDvhEstimate != null && dvh.UpperDvhEstimate != null)
            {
                var xd = xs.ToList();
                xd.AddRange(xs.Reverse().ToList());
                var yd = dvh.GetVolumeDifferenceAt(xs, DoseUnit.Gy, DvhType.LowerEstimate, DvhType.ReferencePlan).ToList();
                yd.AddRange(dvh.GetVolumeDifferenceAt(xs, DoseUnit.Gy, DvhType.UpperEstimate, DvhType.ReferencePlan).Reverse().ToList());
                plt.PlotPolygon(xd.ToArray(), yd.ToArray(), fillColor: Drawing.Color.LightSteelBlue, fillAlpha: 0.5, label: "DVH estimate");
            }

            plt.Ticks(fontSize: 10);
            plt.XLabel("Dose [Gy]", fontSize: 12);
            plt.YLabel("Volume difference [%]", fontSize: 12);
            plt.Title(dvh.PlanPairId, fontSize: 12);

            return plt;
        }
    }
}

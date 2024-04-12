using System;
using System.Collections.Generic;
using System.Linq;
using MigraDoc.DocumentObjectModel;
using UABRO.RapidCompare.Model;

namespace UABRO.RapidCompare.Reporting
{
    class DVHWriter
    {
        public static void WriteMultipleDVHPanels(Section section, RapidPlanModelDvhSet rapidPlanModelDvhSet)
        {
            string targetColor = "#e6194b";
            var colorList = new List<string> { "#3cb44b", "#ffe119", "#4363d8", "#f58231", "#911eb4", "#46f0f0", "#f032e6", "#bcf60c", "#fabebe", "#008080", "#e6beff", "#9a6324", "#800000", "#aaffc3", "#808000", "#ffd8b1", "#000075", "#808080", "#000000" };
            
            Dictionary<string, System.Drawing.Color> plotColors = new Dictionary<string, System.Drawing.Color>();
            int i = 0;
            foreach (var ms in rapidPlanModelDvhSet.RapidPlanModel.ModelStructures)
            {
                if (ms.IsTarget)
                {
                    plotColors.Add(ms.Name, System.Drawing.ColorTranslator.FromHtml(targetColor));
                }
                else
                {
                    plotColors.Add(ms.Name, System.Drawing.ColorTranslator.FromHtml(colorList[i]));
                    i++;
                    i = i % colorList.Count;
                }
            }

            var paragraph = section.AddParagraph("DVHs");
            paragraph.Style = StyleNames.Heading1.ToString();
            paragraph = section.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            foreach (var group in rapidPlanModelDvhSet.RapidPlanDvhsByPlanPairId)
            {
                var plt = PlotMultipleDVHPanel(section, group.ToList(),plotColors);
                plt.Title(group.Key, fontSize: 12);
                plt.AddPlotToParagraph(paragraph);
            }
        }
        private static ReportPlot PlotMultipleDVHPanel(Section section, List<RapidPlanDoseVolumeHistograms> dvhs, Dictionary<string, System.Drawing.Color> plotColors)
        {
            double printWidth = section.PageSetup.PageWidth.Inch - section.PageSetup.RightMargin.Inch - section.PageSetup.LeftMargin.Inch;
            printWidth *= 0.97 / 2;

            var plt = new ReportPlot(printWidth, printWidth);

            foreach (var dvh in dvhs)
            {
                var xs = dvh.ReferencePlanDvh.Select(d => d.DoseGy).ToArray();
                var ys = dvh.ReferencePlanDvh.Select(d => d.VolumePercent).ToArray();
                plt.PlotScatter(xs, ys, label: "Reference", markerShape: ScottPlot.MarkerShape.none, lineWidth: 1.0, color: plotColors[dvh.ModelStructureId]);

                xs = dvh.RapidPlanDvh.Select(d => d.DoseGy).ToArray();
                ys = dvh.RapidPlanDvh.Select(d => d.VolumePercent).ToArray();
                plt.PlotScatter(xs, ys, label: "RapidPlan", markerShape: ScottPlot.MarkerShape.none, lineWidth: 1.5, lineStyle: ScottPlot.LineStyle.Dash,color: plotColors[dvh.ModelStructureId]);
            }
            plt.Ticks(fontSize: 10);
            plt.XLabel("Dose [Gy]", fontSize: 12);
            plt.YLabel("Volume [%]", fontSize: 12);

            return plt;
        }
    }
}

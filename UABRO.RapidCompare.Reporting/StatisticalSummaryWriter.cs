using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigraDoc.DocumentObjectModel;
using ScottPlot;
using UABRO.RapidCompare.Analysis;
using UABRO.RapidCompare.Model;

namespace UABRO.RapidCompare.Reporting
{
    static class StatisticalSummaryWriter
    {
        public static void WriteVolumeDifferencePlot(Section section, double[] dose, List<BoxWhiskerComponents> boxWhiskerComponents, double[] pValue, string structureId)
        {
            var paragraph = section.AddParagraph(structureId + " DVHs");
            paragraph.Style = StyleNames.Heading1.ToString();
            paragraph = section.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Left;

            double printWidth = section.PageSetup.PageWidth.Inch - section.PageSetup.RightMargin.Inch - section.PageSetup.LeftMargin.Inch;
            printWidth *= 0.97;

            double plotDpi = 96;
            var w = (int)Math.Floor(printWidth * plotDpi);
            var h = (int)(0.75 * w);
            var plt = new Plot(w, h);

            plt.PlotScatter(dose, boxWhiskerComponents.Select(s => s.Median).ToArray(), markerShape: MarkerShape.none, lineWidth: 2.0, color: System.Drawing.Color.Blue, label: "Median");
            plt.PlotScatter(dose, boxWhiskerComponents.Select(s => s.UpperQuartile).ToArray(), markerShape: MarkerShape.none, lineWidth: 1.0, color: System.Drawing.Color.Black, label: "Upper Quartile") ;
            plt.PlotScatter(dose, boxWhiskerComponents.Select(s => s.LowerQuartile).ToArray(), markerShape: MarkerShape.none, lineWidth: 1.0, color: System.Drawing.Color.Black, label: "Lower Quartile");

            for (int i = 0; i < dose.Length; i++)
            {
                foreach (var y in boxWhiskerComponents[i].ExtremeOutliers)
                    plt.PlotPoint(dose[i], y, color: System.Drawing.Color.Black);
                foreach (var y in boxWhiskerComponents[i].MildOutliers)
                    plt.PlotPoint(dose[i], y, color: System.Drawing.Color.Black);
            }

            var xx = new List<double>();
            var yy = new List<double>();
            for (int i=0;i<dose.Length;i++)
            {
                if (pValue[i] < 0.05)
                {
                    xx.Add(dose[i]);
                    yy.Add(boxWhiskerComponents[i].Median);
                }
            }
            if (xx.Count() > 0)
                plt.PlotScatter(xx.ToArray(), yy.ToArray(), markerShape: MarkerShape.openCircle, lineWidth: 0, color: System.Drawing.Color.Red);

            plt.Ticks(fontSize: 12);
            plt.XLabel("Dose [Gy]", fontSize: 14);
            plt.YLabel("Volume Difference [%]", fontSize: 14);
            plt.Title("RapidPlan - Reference plan volume difference for " + structureId, fontSize: 16);

            PlotWriter.AddPlotToParagraph(plt, paragraph);
        }
    }
}

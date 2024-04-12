using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigraDoc.DocumentObjectModel;
using ScottPlot;
using UABRO.RapidCompare.Model;
using UABRO.RapidCompare.Analysis;

namespace UABRO.RapidCompare.Reporting
{
    class ReportPlot
    {
        private static int _plotDpi = 96;

        private static string _fontName = "Consolas";
        private Plot _plot;
        public static int MagnificationFactor = 6;
        public static double PlotDpi { get => MagnificationFactor * _plotDpi; }
        public ReportPlot(double widthInches,double heightInches)
        {
            var w = (int)Math.Floor(widthInches * PlotDpi);
            var h = (int)(heightInches * PlotDpi);

            _plot = new Plot(w, h);

            _plot.Ticks(fontName: "Consolas", fontSize: 10 * MagnificationFactor);
        }
        public void PlotPoint(double x, double y, System.Drawing.Color? color = null, double markerSize = 5)
        {
            _plot.PlotPoint(x, y, color: color, markerSize: MagnificationFactor * markerSize);
        }
        public void PlotLine(double x1, double y1, double x2, double y2, System.Drawing.Color? color = null, double lineWidth = 1)
        {
            _plot.PlotLine(x1, y1, x2, y2, color: color, MagnificationFactor * lineWidth);
        }
        public void PlotScatter(double[] x, double[] y, MarkerShape markerShape = MarkerShape.filledCircle, double lineWidth = 1, System.Drawing.Color? color = null, string label = null, LineStyle lineStyle = LineStyle.Solid)
        {
            _plot.PlotScatter(x,y, markerShape: markerShape, lineWidth: MagnificationFactor * lineWidth, color: color, label: label,lineStyle: lineStyle);

        }
        public void PlotBar(double[] xs, double[] ys, System.Drawing.Color? fillColor = null, System.Drawing.Color? negativeColor = null)
        {
            _plot.PlotBar(xs, ys, fillColor: fillColor, negativeColor: negativeColor);
        }
        public void PlotPolygon(double[] xs, double[] ys, string label = null, System.Drawing.Color? fillColor = null, double fillAlpha = 1)
        {
            _plot.PlotPolygon(xs, ys, fillColor: fillColor, fillAlpha: fillAlpha, label: label);
        }
        public void PlotBoxWhisker(double x, BoxWhiskerComponents box, double boxWidth = 0.2, 
            MarkerShape markerShape = MarkerShape.eks, double markerSize = 5, double lineWidth = 1, System.Drawing.Color? color = null)
        {
            var xunit = new double[] { -1, 0, 0, -1, 1, 0, 0, 1, 1, 0, 0, -1, 1, 0, 0, -1, -1, 1, -1 };
            var xbox = xunit.Select(xn => xn * boxWidth + x).ToArray();

            var ybox = new double[] { box.LowerQuartile, box.LowerQuartile, box.Minimum,
                box.Minimum, box.Minimum, box.Minimum, box.LowerQuartile,
                box.LowerQuartile, box.UpperQuartile, box.UpperQuartile, box.Maximum,
                box.Maximum, box.Maximum, box.Maximum, box.UpperQuartile,
                box.UpperQuartile, box.Median, box.Median, box.Median };

            _plot.PlotPolygon(xbox, ybox, lineWidth: lineWidth * MagnificationFactor, fill: false, lineColor: color);
        }
       
        public void XLabel(string xLabel, float fontSize = 12)
        {
            _plot.XLabel(xLabel, fontName: _fontName, fontSize: fontSize * MagnificationFactor);
        }
        public void YLabel(string yLabel, float fontSize = 12)
        {
            _plot.YLabel( yLabel, fontName: _fontName, fontSize: fontSize * MagnificationFactor);
        }
        public void Title(string title, float fontSize = 14)
        {
            _plot.Title(title, fontName: _fontName, fontSize: fontSize * MagnificationFactor);
        }
        public void Grid(double? lineWidth = null)
        {
            _plot.Grid(lineWidth: lineWidth * MagnificationFactor);
        }

        public void Ticks(bool? displayTicksXminor = null, float fontSize = 10)
        {
            _plot.Ticks(displayTicksXminor: displayTicksXminor, fontName: _fontName, fontSize: fontSize * MagnificationFactor);
        }
        public void XTicks(string[] labels)
        {
            _plot.XTicks(labels);
        }
        public void XTicks(double[] positions, string[] labels)
        {
            _plot.XTicks(positions, labels);
        }
        public void YTicks(double[] positions, string[] labels)
        {
            _plot.YTicks(positions, labels);
        }
        public void Layout(double? yScaleWidthInches = null)
        {
            _plot.Layout(yScaleWidth: yScaleWidthInches * PlotDpi);
        }
        public void Legend(float fontSize = 10,legendLocation location = legendLocation.lowerRight)
        {
            _plot.Legend(fontSize: fontSize * MagnificationFactor, location: location);
        }
        public void AddPlotToParagraph(Paragraph paragraph)
        {
            paragraph.AddImage(ExportPlotToMigradocFileString(_plot));
        }
        private static string ExportPlotToMigradocFileString(Plot plot)
        {
            byte[] bytes;
            using (var stream = new System.IO.MemoryStream())
            {
                var bmp = plot.GetBitmap();
                bmp.SetResolution(MagnificationFactor * bmp.HorizontalResolution, MagnificationFactor * bmp.VerticalResolution);
                bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                bytes = stream.ToArray();
            }

            return "base64:" + Convert.ToBase64String(bytes);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScottPlot;
using MigraDoc.DocumentObjectModel;

namespace UABRO.RapidCompare.Reporting
{
    static class PlotWriter
    {
        internal static void AddPlotToParagraph(Plot plot, Paragraph paragraph)
        {
            paragraph.AddImage(ExportPlotToMigradocFileString(plot));
        }
        private static string ExportPlotToMigradocFileString(Plot plot)
        {
            byte[] bytes;
            using (var stream = new System.IO.MemoryStream())
            {
                plot.GetBitmap().Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                bytes = stream.ToArray();
            }

            return "base64:" + Convert.ToBase64String(bytes);
        }
    }
}

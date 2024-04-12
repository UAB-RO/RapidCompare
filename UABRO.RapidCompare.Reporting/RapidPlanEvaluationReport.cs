using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using MigraDoc.DocumentObjectModel.Tables;
using S = ScottPlot;
using UABRO.RapidCompare.Model;
using UABRO.RapidCompare.Analysis;

namespace UABRO.RapidCompare.Reporting
{
    public class RapidPlanEvaluationReport
    {
        private Document report;
        private RapidPlanModelDvhSet rapidPlanModelDvhSet;

        public string Author = string.Empty;
        public DateTime Date = DateTime.Now;
        public RapidPlanEvaluationReport(RapidPlanModelDvhSet comparison)
        {
            rapidPlanModelDvhSet = comparison;

            report = new Document();

            // Get the predefined style Normal.
            Style style = report.Styles[StyleNames.Normal];
            // Because all styles are derived from Normal, the next line changes the 
            // font of the whole document. Or, more exactly, it changes the font of
            // all styles and paragraphs that do not redefine the font.
            style.Font.Name = "Times New Roman";
            style.Font.Size = 11;

            // Heading1 to Heading9 are predefined styles with an outline level. An outline level
            // other than OutlineLevel.BodyText automatically creates the outline (or bookmarks) 
            // in PDF.

            style = report.Styles[StyleNames.Heading1];
            style.Font.Name = "Arial";
            style.Font.Size = 14;
            style.Font.Bold = true;
            style.Font.Color = Colors.DarkBlue;
            style.ParagraphFormat.PageBreakBefore = true;
            style.ParagraphFormat.SpaceAfter = 6;

            style = report.Styles[StyleNames.Heading2];
            style.Font.Name = "Arial";
            style.Font.Size = 11;
            style.Font.Bold = true;
            style.Font.Color = Colors.Black;
            style.ParagraphFormat.PageBreakBefore = false;
            style.ParagraphFormat.SpaceBefore = 6;
            style.ParagraphFormat.SpaceAfter = 6;

            style = report.Styles[StyleNames.Heading3];
            style.Font.Name = "Arial";
            style.Font.Size = 11;
            style.Font.Bold = true;
            style.Font.Italic = true;
            style.ParagraphFormat.SpaceBefore = 6;
            style.ParagraphFormat.SpaceAfter = 3;

            style = report.Styles[StyleNames.Header];
            style.ParagraphFormat.AddTabStop("3.5in", TabAlignment.Center);

            style = report.Styles[StyleNames.Footer];
            style.ParagraphFormat.AddTabStop("3.5in", TabAlignment.Center);

            // Create a new style called TextBox based on style Normal
            style = report.Styles.AddStyle("TextBox", "Normal");
            style.ParagraphFormat.Alignment = ParagraphAlignment.Justify;
            style.ParagraphFormat.Borders.Width = 2.5;
            style.ParagraphFormat.Borders.Distance = "3pt";
            style.ParagraphFormat.Shading.Color = Colors.SkyBlue;

            // Create a new style called TOC based on style Normal
            style = report.Styles.AddStyle("TOC", "Normal");
            style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right, TabLeader.Dots);
            style.ParagraphFormat.Font.Color = Colors.Blue;

            ReportPlot.MagnificationFactor = 2;
        }

        private Section AddStandardSection()
        {
            Section section = report.AddSection();
            section.PageSetup = report.DefaultPageSetup.Clone();
            section.PageSetup.PageFormat = PageFormat.Letter;
            section.PageSetup.LeftMargin = "0.75in";
            section.PageSetup.RightMargin = "0.75in";
            section.PageSetup.TopMargin = "0.75in";
            section.PageSetup.BottomMargin = "0.75in";
            section.PageSetup.HeaderDistance = "0.375in";
            section.PageSetup.FooterDistance = "0.5 in";

            return section;
        }
        private void AddHeaderFooter(Section section,string label)
        {
            Paragraph paragraph = section.Headers.Primary.AddParagraph($"{rapidPlanModelDvhSet.RapidPlanModel.Id}\t{label}");
            paragraph.Format.Borders.Bottom = new Border() { Width = "0.5pt", Color = Colors.Black };

            // Create a paragraph with centered page number. See definition of style "Footer".
            paragraph = new Paragraph();

            paragraph.AddTab();
            paragraph.AddPageField();
            paragraph.Format.Borders.Top = new Border() { Width = "0.5pt", Color = Colors.Black };
            section.Footers.Primary.Add(paragraph);
        }
        public void RenderCoverPage()
        {
            Section section = AddStandardSection();
            
            Paragraph paragraph = section.AddParagraph();
            paragraph.Format.SpaceAfter = "3cm";

            paragraph = section.AddParagraph($"RapidPlan evaluation\n{rapidPlanModelDvhSet.RapidPlanModel.Id}");
            paragraph.Format.Font.Size = 16;
            paragraph.Format.Font.Color = Colors.DarkBlue;
            paragraph.Format.SpaceBefore = "5cm";
            paragraph.Format.SpaceAfter = "1cm";

            paragraph = section.AddParagraph($"{Author}\n{Date:MMMM d, yyyy}");
            paragraph.Format.Font.Size = 14;
            paragraph.Format.SpaceAfter = "2cm";
        }
        public void RenderModelSummary()
        {
            var section = AddStandardSection();
            AddHeaderFooter(section, "Model summary");
            RapidPlanModelWriter.WriteModelSummary(section, rapidPlanModelDvhSet.RapidPlanModel);
        }
        public void RenderPlanDVHs()
        {
            var section = AddStandardSection();
            AddHeaderFooter(section, "All DVHs");
            DVHWriter.WriteMultipleDVHPanels(section, rapidPlanModelDvhSet);
        }
        public void RenderStructureSummaries()
        {
            var section = AddStandardSection();
            AddHeaderFooter(section, "Model reference plan comparison summary");
            StructureSummarySectionWriter.WriteDoseVolumeMetricDifferencePlot(section, rapidPlanModelDvhSet);
            foreach (var id in rapidPlanModelDvhSet.StructureIds)
            {
                section = AddStandardSection();
                AddHeaderFooter(section, id);
                var paragraph = section.AddParagraph(id);
                paragraph.Style = StyleNames.Heading1.ToString();

                if (rapidPlanModelDvhSet.RapidPlanModel.ModelStructures.Where(ms => ms.Name == id).Count() > 0)
                {
                    StructureSummarySectionWriter.WriteVolumeDifferencePlot(section, rapidPlanModelDvhSet, id);

                    StructureSummarySectionWriter.WriteOptimizationObjectivesTable(section, rapidPlanModelDvhSet, id);
                    StructureSummarySectionWriter.WriteOptimizationObjectivesBoxWhiskerPlots(section, rapidPlanModelDvhSet, id);

                    StructureSummarySectionWriter.WriteDVHDifferenceBarCharts(section, rapidPlanModelDvhSet, id);
                    StructureSummarySectionWriter.WriteAllDVHs(section, rapidPlanModelDvhSet, id);
                }
                else
                {
                    paragraph = section.AddParagraph($"No plans had matches for {id}.");
                    paragraph.Format.Font.Italic = true;
                }
            }
        }

       
        public void Save(string filename)
        {
            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true);
            
            renderer.Document = report;
            renderer.RenderDocument();
            renderer.PdfDocument.Save(filename);
        }

    }
}

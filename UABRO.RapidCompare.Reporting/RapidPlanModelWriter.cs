using System;
using MigraDoc.DocumentObjectModel;
using UABRO.RapidCompare.Model;

namespace UABRO.RapidCompare.Reporting
{
    class RapidPlanModelWriter
    {
        public static void WriteModelSummary(Section section, RapidPlanModelDescription model)
        {
            var paragraph = section.AddParagraph("Model Summary");
            paragraph.Style = StyleNames.Heading1.ToString();
            paragraph = section.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Left;


            paragraph = section.AddParagraph($"Model ID: {model.Id}");
            paragraph = section.AddParagraph($"Model Version: {model.Version}");
            paragraph = section.AddParagraph($"Description: {model.Description}");

            paragraph = section.AddParagraph($"Number of training cases: {model.NumberOfTrainingCases}");

            paragraph = section.AddParagraph("Optimization objectives and optimization settings");
            paragraph.Style = StyleNames.Heading2.ToString();
            WriteOptimizationObjectivesTable(section, model);

            paragraph = section.AddParagraph("Structure codes");
            paragraph.Style = StyleNames.Heading2.ToString();
            WriteStructureCodesTable(section, model);

        }

        private static void WriteOptimizationObjectivesTable(Section section, RapidPlanModelDescription model)
        {
            var table = section.AddTable();
            table.Style = "Table";
            table.Borders.Width = 0.25;
            table.Borders.Left.Width = 0.5;
            table.Borders.Right.Width = 0.5;
            table.Rows.LeftIndent = 0;

            var column = table.AddColumn("3cm");
            column.Format.Alignment = ParagraphAlignment.Left;

            column = table.AddColumn("2.5cm");
            column.Format.Alignment = ParagraphAlignment.Left;

            column = table.AddColumn("3cm");
            column.Format.Alignment = ParagraphAlignment.Center;

            column = table.AddColumn("3.5cm");
            column.Format.Alignment = ParagraphAlignment.Center;

            column = table.AddColumn("2cm");
            column.Format.Alignment = ParagraphAlignment.Center;

            var row = table.AddRow();
            row.Cells[0].AddParagraph("Structure");
            row.Cells[1].AddParagraph("Objective type");
            row.Cells[2].AddParagraph("Relative volume (%)");
            row.Cells[3].AddParagraph("Dose");
            row.Cells[4].AddParagraph("Priority");

            bool isShaded = true;
            var shadingColor = new Color(242, 242, 242);
            foreach (var s in model.ModelStructures)
            {
                row = table.AddRow();
                row.Cells[0].AddParagraph($"{s.Name}");

                bool addRow = false;
               foreach(var obj in s.PlanningObjectives)
                {
                    var volume = obj.Volume == null ? "Generated" : obj.Volume.ToString() + "%";
                    var dose = obj.Dose == null ? "Generated" : obj.Dose.ToString() + " " + obj.DoseUnit;
                    var priority = obj.Priority == null ? "Generated" : obj.Priority.ToString();

                    string objective;
                    if (obj.Type == ObjectiveType.Point && obj.Operator == ObjectiveOperator.GreaterThan)
                        objective = "Lower";
                    else if (obj.Type == ObjectiveType.Point && obj.Operator == ObjectiveOperator.LessThan)
                        objective = "Upper";
                    else if (obj.Type == ObjectiveType.Mean && obj.Operator == ObjectiveOperator.LessThan)
                        objective = "Mean";
                    else if (obj.Type == ObjectiveType.LinePreferringOar && obj.Operator == ObjectiveOperator.LessThan)
                        objective = "Line (preferring OAR)";
                    else if (obj.Type == ObjectiveType.LinePreferringTarget && obj.Operator == ObjectiveOperator.LessThan)
                        objective = "Line (preferring target)";
                    else
                        objective = obj.Type.ToString() + " " + obj.Operator.ToString();

                    if (addRow)
                        row = table.AddRow();
                    addRow = true;
                    if (isShaded)
                        row.Shading.Color = shadingColor;
                    row.Borders.Visible = true;
                    row.Borders.Top.Visible = true;

                    row.Cells[1].AddParagraph(objective);
                    row.Cells[2].AddParagraph(volume);
                    row.Cells[3].AddParagraph(dose);
                    row.Cells[4].AddParagraph(priority);
                }
                isShaded = !isShaded;
            }
        }

        private static void WriteStructureCodesTable(Section section, RapidPlanModelDescription model)
        {
            var table = section.AddTable();
            table.Style = "Table";
            table.Borders.Width = 0.25;
            table.Borders.Left.Width = 0.5;
            table.Borders.Right.Width = 0.5;
            table.Rows.LeftIndent = 0;

            var column = table.AddColumn("3cm");
            column.Format.Alignment = ParagraphAlignment.Left;

            column = table.AddColumn("11cm");
            column.Format.Alignment = ParagraphAlignment.Left;

            var row = table.AddRow();
            row.Cells[0].AddParagraph("Structure");
            row.Cells[1].AddParagraph("Codes");

            bool isShaded = true;
            var shadingColor = new Color(242, 242, 242);
            foreach (var s in model.ModelStructures)
            {
                row = table.AddRow();
                if (isShaded)
                    row.Shading.Color = shadingColor;
                row.Borders.Visible = true;
                row.Borders.Top.Visible = true;

                row.Cells[0].AddParagraph($"{s.Name}");
                row.Cells[1].AddParagraph(String.Join(", ", s.Codes));

                isShaded = !isShaded;
            }
        }
    }
}

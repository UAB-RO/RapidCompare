using System;
using System.IO;
using ConsoleX;
using UABRO.RapidCompare.Model;
using UABRO.RapidCompare.Reporting;

namespace RapidPlanEvaluationReportingConsole
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var ui = new ConsoleUI();
            var path = ui.GetOpenFilePath(fileExtension: "*.json");
            ui.Write("Reading model comparison...");

            string json = File.ReadAllText(path);
            var rapidPlanModelDvhSet = IO.ReadRapidPlanModelDvhSet(path);

            ui.Write(rapidPlanModelDvhSet.RapidPlanModel.Id);

            ui.Write("Rendering report...");
            var report = new RapidPlanEvaluationReport(rapidPlanModelDvhSet);

            report.RenderCoverPage();
            report.RenderModelSummary();
            report.RenderStructureSummaries();
            report.RenderPlanDVHs();

            var folder = Path.GetDirectoryName(path);
            var rootname = Path.GetFileNameWithoutExtension(path);
            path = Path.Combine(folder, rootname + ".pdf");
            report.Save(path);

            ui.Write("Done");
            ui.Write("Starting PDF viewer...");

            System.Diagnostics.Process.Start(path);
            ui.Write("Done");

            Console.ReadLine();
        }
    }
}

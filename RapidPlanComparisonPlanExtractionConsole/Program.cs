// <copyright file="Program.cs" company="UAB Medicine">
// Copyright (c) UAB Medicine. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ConsoleX;
using NLog;
using UABRO.RapidCompare.Extraction;
using UABRO.RapidCompare.Model;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

// Uncomment the following line if the script requires write access.
[assembly: ESAPIScript(IsWriteable = true)]

namespace RapidPlanComaprisonPlanExtractionConsole
{
    class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        [STAThread]
        static void Main(string[] args)
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = "${basedir}/logs/logfile_${cached:inner=${date:format=yyyy-MM-dd HH-mm}}.txt",
                Layout = "${longdate} ${qpc:normalize=true} ${qpc:normalize=true:difference=true} ${logger} ${level} ${message}  ${exception} ${event-properties:myProperty}",
            };
            logfile.AutoFlush = true;

            var logconsole = new NLog.Targets.ColoredConsoleTarget("logconsole");
            logconsole.Layout = "${time} ${message} ${exception} ${event-properties:myProperty}";

            // Rules for mapping loggers to targets
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);

            // Apply config
            LogManager.Configuration = config;

            Trace.Listeners.Add(new NLogTraceListener());

            try
            {
                using (Application app = Application.CreateApplication())
                {
                    Execute(app);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }

            LogManager.Shutdown();
            Console.WriteLine("Done");
            Console.ReadLine();
        }

        static void Execute(Application app)
        {
            var ui = new ConsoleUI();
            Console.SetWindowSize(200, 50);

            string path = ui.GetOpenFilePath(fileExtension: "*.txt");
            ui.Write(path);
            var folder = Path.GetDirectoryName(path);
            var rootname = Path.GetFileNameWithoutExtension(path);

            var logfilefolder = Path.Combine(folder, rootname + $" Logs {DateTime.Now:yyyy-MM-dd HH-mm}");

            ui.WritePrompt("Recalculate existing plans?");
            WorklistItem.RecalculateExistingPlan = ui.GetYesNoResponse();

            var worklist = WorklistBuilder.ParseText(File.ReadAllLines(path));
            if (worklist.Select(w => w.PatientID+w.ReferenceCourseID+w.ReferencePlanID).Distinct().Count() != worklist.Count())
            {
                _logger.Warn("Worklist contains duplicate reference plan patient-course-plans.");
                return;
            }
            if (worklist.Select(w => w.PatientID + w.RapidPlanCourseID + w.RapidPlanID).Distinct().Count() != worklist.Count())
            {
                _logger.Warn("Worklist contains duplicate RapidPlan patient-course-plans.");
                return;
            }

            WorklistItem.App = app;
            WorklistItem.SaveModifications = true;

            // Add case specific log file
            var caseSpecificLogFile = new NLog.Targets.FileTarget("casespecificlogfile");
            caseSpecificLogFile.AutoFlush = true;
            caseSpecificLogFile.Layout = "${longdate} ${logger} ${level} ${message}  ${exception}";
            LogManager.Configuration.AddTarget("casespecificlogfile", caseSpecificLogFile);
            LogManager.Configuration.AddRule(LogLevel.Debug, LogLevel.Fatal, caseSpecificLogFile);

            // Get model information
            var rapidPlanModelDvhSet = new RapidPlanModelDvhSet();
            string rapidPlanDbServer = "ariaprsql1"; // ariaprsql1 for production, ECS848 for test
            rapidPlanModelDvhSet.RapidPlanModel = Helpers.GetModelDescriptionFromRapidPlanDatabase(worklist.Select(w => w.RapidPlanModelId).Distinct().Single(),rapidPlanDbServer);
            var rapidPlanModelStructureNames = rapidPlanModelDvhSet.RapidPlanModel.ModelStructures.Select(m => m.Name).ToList();
            
            var modelStructureMatchModelNames = worklist.SelectMany(w => w.ModelStructureMatches.Values).Distinct().ToList();
            if (modelStructureMatchModelNames.Any(m=>!rapidPlanModelStructureNames.Contains(m)))
            {
                var structuresNotInModel = string.Join(", ", modelStructureMatchModelNames.Where(m => !rapidPlanModelStructureNames.Contains(m)));
                _logger.Warn($"Worklist contains model structures that either aren't in the model {rapidPlanModelDvhSet.RapidPlanModel.Id} or don't have objectives: {structuresNotInModel}");
            }
            if (rapidPlanModelStructureNames.Any(m=>!modelStructureMatchModelNames.Contains(m)))
            {
                var structuresNotInWorklist = string.Join(", ", rapidPlanModelStructureNames.Where(m => !modelStructureMatchModelNames.Contains(m)));
                _logger.Warn($"Model {rapidPlanModelDvhSet.RapidPlanModel.Id} contains model structures that do not have matches in the worklist: {structuresNotInWorklist}");
            }

            // Get comparison data
            var success = new List<bool>();
            var popupListener = new PopupListener();
            popupListener.Start();
            foreach (var item in worklist)
            {
                item.ItemId = (worklist.IndexOf(item) + 1).ToString();

                ui.WriteSectionHeader($"{item.ItemId}-{item.PatientID}");

                caseSpecificLogFile.FileName = Path.Combine(logfilefolder, $"{item.ItemId}-{item.PatientID}.txt");
                LogManager.ReconfigExistingLoggers();

                // Prune worklist to remove structures without objectives in the RapidPlan model
                item.PruneModelStructures(rapidPlanModelDvhSet.RapidPlanModel.ModelStructures.Select(m => m.Name).ToList());

                try
                {
                    item.OpenPatient();
                    List<RapidPlanDoseVolumeHistograms> dvhs = item.GetRapidPlanDoseVolumeHistograms();
                    rapidPlanModelDvhSet.RapidPlanDvhs.AddRange(dvhs);
                    success.Add(dvhs.Count > 0);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    success.Add(false);
                }
                finally
                {
                    item.ClosePatient();
                }
            }
            popupListener.Stop();

            if (!success.All(b => b))
            {
                return;
            }

            path = Path.Combine(folder, rootname + ".json");
            IO.WriteRapidPlanModelDvhSet(rapidPlanModelDvhSet, path);
        }
    }
}
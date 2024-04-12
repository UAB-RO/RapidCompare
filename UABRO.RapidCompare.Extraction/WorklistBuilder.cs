using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace UABRO.RapidCompare.Extraction
{
    public static class WorklistBuilder
    {
        static int columnOfFirstModelStructure = 5;
        static public List<WorklistItem> ParseText(string[] lines)
        {
            string[] row;
            int iRow;
            int jCol;

            // Parse header

            string rapidPlanModelId = string.Empty;

            Dictionary<string, Dictionary<string, string>> calculationModelOptions = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<CalculationType, string> calculationModels = new Dictionary<CalculationType, string>();
            string[] calculationTypes = Enum.GetNames(typeof(CalculationType));

            string beamGeometry = string.Empty;
            string[] modelStructures = new string[0];
            bool isEndOfHeader = false;
            iRow = 0;
            do
            {
                row = lines[iRow].Split('\t').Select(s => s.Trim()).ToArray();
                if (row[0] == "RapidPlanModelID")
                {
                    rapidPlanModelId = row[1];
                }
                else if (row[0] == "PatientID")
                {
                    modelStructures = row.Skip(columnOfFirstModelStructure).TakeWhile(s => !String.IsNullOrWhiteSpace(s)).ToArray();
                    isEndOfHeader = true;
                }
                else if (calculationTypes.Contains(row[0]))
                {
                    calculationModels.Add((CalculationType)Enum.Parse(typeof(CalculationType), row[0]), row[1]);
                    jCol = 2;
                    
                    if (!calculationModelOptions.ContainsKey(row[1]))
                    {
                        calculationModelOptions.Add(row[1], new Dictionary<string, string>());
                    }
                    Dictionary<string, string> calculationOptions = calculationModelOptions[row[1]];
                    while (row.Length > jCol + 1 && !String.IsNullOrWhiteSpace(row[jCol]))
                    {
                        calculationOptions.Add(row[jCol], row[jCol + 1]);
                        jCol += 2;
                    }
                }
                else if (!String.IsNullOrWhiteSpace(row[0]))
                {
                    throw new ArgumentException("Invalid keyword in header");
                }
                iRow++;
            } while (!isEndOfHeader);

            // Parse rows
            var regex = new Regex(@"^Dose\[(?<DoseUnit>cGy|Gy)\]_(?<TargetName>.+)$");
            var worklist = new List<WorklistItem>();
            for (int i = iRow; i < lines.Length; i++)
            {
                row = lines[i].Split('\t').Select(s => s.Trim()).ToArray();
                var modmatches = new Dictionary<string, string>();
                var doses = new Dictionary<string, DoseValue>();
                for (int j = 0; j < modelStructures.Length; j++)
                {
                    var match = regex.Match(modelStructures[j]);
                    if (match.Success)
                    {
                        string roi = row[j + columnOfFirstModelStructure - 1];
                        if (!String.IsNullOrWhiteSpace(roi))
                        {
                            doses.Add(roi, new DoseValue(double.Parse(row[j + columnOfFirstModelStructure]), (DoseValue.DoseUnit)Enum.Parse(typeof(DoseValue.DoseUnit), match.Groups["DoseUnit"].Value)));
                        }
                    }
                    else
                    {
                        string roi = row[j + columnOfFirstModelStructure];
                        if (!String.IsNullOrWhiteSpace(roi))
                        {
                            modmatches.Add(roi, modelStructures[j]);
                        }
                    }
                }

                var item = new WorklistItem();
                item.RapidPlanModelId = rapidPlanModelId;
                item.CalculationModels = calculationModels;
                item.CalculationModelOptions = calculationModelOptions;
                item.PatientID = row[0];
                item.ReferenceCourseID = row[1];
                item.ReferencePlanID = row[2];
                item.RapidPlanCourseID = row[3];
                item.RapidPlanID = row[4];
                item.ModelStructureMatches = modmatches;
                item.TargetDoseLevels = doses;
                worklist.Add(item);
            }

            return worklist;
        }
        public static string ParseQuotedString(string str)
        {
            var result = str;
            // If the text being parsed was created in Excel and exported as tab-delimited text, the exported text will be
            // enclosed in double quotes if it contains any commas. Furthermore, all double quotes in the text will be escaped
            // by replacing all double quotes (") with double double quotes (""). Inspect the string and fix it.
            if (str.StartsWith("\"") && str.EndsWith("\""))
            {
                result = str.Substring(1, str.Length - 2);
                result = result.Replace("\"\"", "\"");
            }
            return result;
        }
    }
}

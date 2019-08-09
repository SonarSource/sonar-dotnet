using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;

namespace SonarAnalyzer
{
    internal class MlirExporterMetrics
    {
        public static void AddExportedFunction(bool exported)
        {
            path = Path.Combine(Path.GetTempPath(), $"exportedFunctions.txt");

            if (!File.Exists(path))
            {
                CreateMetricsFile(path);
            }

            string[] text = File.ReadAllLines(path);
            if (text.Length < MLIRExporter.unsupportedSyntaxes.Count + nbLines)
            {
                File.Delete(path);
                CreateMetricsFile(path);
            }

            var idx = Convert.ToInt32(exported);
            var value = int.Parse(Regex.Match(text[idx], @"\d+").Value);
            text[idx] = text[idx].Replace(value.ToString(), (value + 1).ToString());
            File.WriteAllLines(path, text);
        }

        public static void AddUnsupportedSyntax(SyntaxKind syntaxKind)
        {
            string[] text = File.ReadAllLines(path);
            var syntaxIdx = nbLines + MLIRExporter.unsupportedSyntaxes.IndexOf(syntaxKind);
            var value = int.Parse(Regex.Match(text[syntaxIdx], @"\d+").Value);
            text[syntaxIdx] = text[syntaxIdx].Replace(value.ToString(), (value + 1).ToString());
            File.WriteAllLines(path, text);
        }

        private static void CreateMetricsFile(string path)
        {
            StringBuilder myStringBuilder = new StringBuilder("0 functions skipped" + Environment.NewLine);
            myStringBuilder.Append("0 functions exported" + Environment.NewLine);
            myStringBuilder.Append(Environment.NewLine);
            myStringBuilder.Append("Unsupported operations:" + Environment.NewLine);
            foreach (var kind in MLIRExporter.unsupportedSyntaxes)
            {
                myStringBuilder.Append("0 " + kind.ToString() + Environment.NewLine);
            }
            File.WriteAllText(path, myStringBuilder.ToString());
        }

        private const int nbLines = 4; // number of existing lines before the list of unsupported syntaxes
        private static string path;
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace SonarAnalyzer
{
    public class MlirExporterMetrics
    {
        private int supportedFunctionsCount;
        private int unsupportedFunctionsCount;
        private Dictionary<SyntaxKind, int> unsupportedSyntaxes;

        public MlirExporterMetrics()
        {
            supportedFunctionsCount = 0;
            unsupportedFunctionsCount = 0;
            unsupportedSyntaxes = new Dictionary<SyntaxKind, int>();
        }

        public void AddSupportedFunction()
        {
            ++supportedFunctionsCount;
        }
        public void AddUnsupportedFunction(SyntaxKind unsupportedSyntax)
        {
            ++unsupportedFunctionsCount;
            unsupportedSyntaxes[unsupportedSyntax] = unsupportedSyntaxes.TryGetValue(unsupportedSyntax, out int currentVal) ? ++currentVal : 1;
        }

        public String Dump()
        {
            if (supportedFunctionsCount == 0 && unsupportedFunctionsCount == 0)
            {
                return "";
            }
            var metricsStr = new StringBuilder($"exported {supportedFunctionsCount} \n" +
                $"unsupported {unsupportedFunctionsCount} \n");
            foreach (KeyValuePair<SyntaxKind, int> entry in unsupportedSyntaxes)
            {
                metricsStr.AppendLine($"{entry.Key.ToString()} {entry.Value}");
            }
            metricsStr.AppendLine();
            return metricsStr.ToString();
        }
    }
}

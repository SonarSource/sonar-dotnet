using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;

namespace SonarAnalyzer
{
    public class MlirExporterMetrics
    {
        private int nbSupportedFunctions;
        private int nbUnsupportedFunctions;
        private Dictionary<SyntaxKind, int> unsupportedSyntaxes;

        public MlirExporterMetrics()
        {
            nbSupportedFunctions = 0;
            nbUnsupportedFunctions = 0;
            unsupportedSyntaxes = new Dictionary<SyntaxKind, int>();
        }

        public void AddSupportedFunction()
        {
            ++nbSupportedFunctions;
        }
        public void AddUnsupportedFunction(SyntaxKind unsupportedSyntax)
        {
            ++nbUnsupportedFunctions;
            unsupportedSyntaxes[unsupportedSyntax] = unsupportedSyntaxes.TryGetValue(unsupportedSyntax, out int currentVal) ? ++currentVal : 1;
        }

        public String Dump()
        {
            if (nbSupportedFunctions == 0 && nbUnsupportedFunctions == 0)
            {
                return "";
            }
            var metricsStr = $"exported {nbSupportedFunctions} \n" +
                $"unsupported {nbUnsupportedFunctions} \n";
            foreach (KeyValuePair<SyntaxKind, int> entry in unsupportedSyntaxes)
            {
                metricsStr += $"{entry.Key.ToString()} {entry.Value} \n";
            }
            return metricsStr + "\n";
        }
    }
}

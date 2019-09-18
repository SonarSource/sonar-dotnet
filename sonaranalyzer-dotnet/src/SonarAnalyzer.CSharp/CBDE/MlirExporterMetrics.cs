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
            if (!unsupportedSyntaxes.ContainsKey(unsupportedSyntax))
            {
                unsupportedSyntaxes.Add(unsupportedSyntax, 1);
                return;
            }
            ++unsupportedSyntaxes[unsupportedSyntax];
        }

        public String Dump()
        {
            if (nbSupportedFunctions == 0 && nbUnsupportedFunctions == 0)
            {
                return "";
            }
            var metricsStr = $"{nbSupportedFunctions} exported functions \n" +
                $"{nbUnsupportedFunctions} unsupported functions \n" +
                $" \t => {(float)nbSupportedFunctions / (float)(nbSupportedFunctions + nbUnsupportedFunctions) * 100}% rate \n";
            foreach (KeyValuePair<SyntaxKind, int> entry in unsupportedSyntaxes)
            {
                metricsStr += $"{entry.Key.ToString()} : {entry.Value} \n";
            }
            return metricsStr + "\n";
        }
    }
}

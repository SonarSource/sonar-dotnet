using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties;

namespace Tests
{
    [TestClass]
    public class RuleTest
    {
        [TestMethod]
        public void RuleHasResourceHtml()
        {
            var assembly = typeof(AssignmentInsideSubExpression).Assembly;

            var analyzers = assembly
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(DiagnosticAnalyzer)))
                .ToList();

            var resources = assembly.GetManifestResourceNames();


            foreach (var analyzer in analyzers)
            {
                var ruleDescriptor = analyzer.GetCustomAttributes<RuleAttribute>().First();
                var resource = resources.SingleOrDefault(r => r.EndsWith(string.Format("Diagnostics.RuleDescriptions.{0}.html", ruleDescriptor.Key)));
                if (resource != null)
                {
                    using (var stream = assembly.GetManifestResourceStream(resource))
                    using (var reader = new StreamReader(stream))
                    {
                        var content = reader.ReadToEnd();
                    }
                }
                else
                {
                    throw new Exception(string.Format("Missing HTML description for rule '{0}'", analyzer.Name));
                }
            }
        }

        [TestMethod]
        public void DiagnosticAnalyzerHasRuleAttribute()
        {
            var assembly = typeof(AssignmentInsideSubExpression).Assembly;

            var analyzers = assembly
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(DiagnosticAnalyzer)))
                .ToList();
            
            foreach (var analyzer in analyzers)
            {
                var ruleDescriptor = analyzer.GetCustomAttributes<RuleAttribute>().SingleOrDefault();
                if (ruleDescriptor == null)
                {
                    throw new Exception(string.Format("RuleAttribute is missing from DiagnosticAnalyzer '{0}'", analyzer.Name));
                }
            }
        }
    }
}
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;
using SonarQube.Analyzers.SonarQube.Settings;
using SonarQube.RuleDescriptor;

namespace SonarQube.Rules.Test
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
                var resource = resources.SingleOrDefault(r => r.EndsWith(
                    string.Format(CultureInfo.InvariantCulture, RuleFinder.RuleDescriptionPathPattern,
                        ruleDescriptor.Key), StringComparison.OrdinalIgnoreCase));
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
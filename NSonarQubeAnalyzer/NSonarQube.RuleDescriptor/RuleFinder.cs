using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.Diagnostics;
using NSonarQube.RuleDescriptor.RuleDescriptors;
using NSonarQubeAnalyzer.Diagnostics.Helpers;
using NSonarQubeAnalyzer.Diagnostics.Rules;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties.Sqale;

namespace NSonarQube.RuleDescriptor
{
    public class RuleFinder
    {
        private readonly List<Type> diagnosticAnalyzers;

        public RuleFinder(Assembly assembly)
        {
            diagnosticAnalyzers = assembly
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof (DiagnosticAnalyzer)))
                .Where(t => t.GetCustomAttributes<RuleAttribute>().Any())
                .ToList();
        }

        public IEnumerable<FullRuleDescriptor> GetRuleDescriptors()
        {
            return diagnosticAnalyzers.Select(GetRuleDescriptor);
        }

        private static FullRuleDescriptor GetRuleDescriptor(Type analyzerType)
        {
            var rule = analyzerType.GetCustomAttributes<RuleAttribute>().FirstOrDefault();

            if (rule == null)
            {
                return null;
            }


            var ruleDescriptor = new RuleDescriptors.RuleDescriptor
            {
                Key = rule.Key,
                Title = rule.Description,
                Severity = rule.Severity.ToString().ToUpper(),
                IsActivatedByDefault = rule.IsActivatedByDefault,
                Cardinality = rule.Template ? "MULTIPLE" : "SINGLE",
            };

            var resources = analyzerType.Assembly.GetManifestResourceNames();
            var resource = resources.SingleOrDefault(r => r.EndsWith(string.Format("Diagnostics.RuleDescriptions.{0}.html", rule.Key)));
            if (resource != null)
            {
                using (var stream = analyzerType.Assembly.GetManifestResourceStream(resource))
                using (var reader = new StreamReader(stream))
                {
                    ruleDescriptor.Description = reader.ReadToEnd();
                }
            }
            var parameters =
                analyzerType.GetProperties().Where(p => p.GetCustomAttributes<RuleParameterAttribute>().Any());

            foreach (var propertyInfo in parameters)
            {
                var ruleParameter = propertyInfo.GetCustomAttributes<RuleParameterAttribute>().First();
                ruleDescriptor.Parameters.Add(
                    new RuleParameter()
                    {
                        DefaultValue = ruleParameter.DefaultValue, 
                        Description = ruleParameter.Description, 
                        Key = ruleParameter.Key,
                        Type = ruleParameter.Type.ToSonarQubeString()
                    });
            
            }

            var tags = analyzerType.GetCustomAttributes<TagsAttribute>().FirstOrDefault();

            if (tags != null)
            {
                ruleDescriptor.Tags.AddRange(tags.Tags);
            }

            var sqaleRemediation = analyzerType.GetCustomAttributes<SqaleRemediationAttribute>().FirstOrDefault();

            SqaleDescriptor sqale = null;

            if (sqaleRemediation != null && 
                !(sqaleRemediation is NoSqaleRemediationAttribute))
            {
                var sqaleSubCharacteristic = analyzerType.GetCustomAttributes<SqaleSubCharacteristicAttribute>().First();
                sqale = new SqaleDescriptor { SubCharacteristic = sqaleSubCharacteristic.SubCharacteristic.ToSonarQubeString() };
                var constant = sqaleRemediation as SqaleConstantRemediationAttribute;
                if (constant != null)
                {
                    sqale.Remediation.Properties.Add(new SqaleRemediationProperty()
                    {
                        Key = "remediationFunction",
                        Text = "CONSTANT_ISSUE"
                    });

                    sqale.Remediation.Properties.Add(new SqaleRemediationProperty()
                    {
                        Key = "offset",
                        Value = constant.Value
                    });

                    sqale.Remediation.RuleKey = rule.Key;
                }
            }

            return new FullRuleDescriptor()
            {
                RuleDescriptor = ruleDescriptor,
                SqaleDescriptor = sqale
            };
        }
    }
}
using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Helpers;
using NSonarQubeAnalyzer.Diagnostics.Rules;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties.Sqale;

namespace Tests.Diagnostics.Attributes
{
    [TestClass]
    public class SqaleTest
    {
        [TestMethod]
        public void SingleSqaleRemediationAttribute()
        {
            var analyzers = typeof(AssignmentInsideSubExpression).Assembly
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof (DiagnosticAnalyzer)))
                .ToList();

            foreach (var analyzer in analyzers)
            {
                var count = analyzer.GetCustomAttributes<SqaleRemediationAttribute>().Count();
                if (count != 1)
                {
                    throw new Exception(
                        string.Format("Only one SqaleRemediationAttribute can be assigned to DiagnosticAnalyzers, '{0}' has {1}",
                        analyzer.Name, count));
                }
            }
        }

        [TestMethod]
        public void SqaleSubCharacteristicAttribute()
        {
            var analyzers = typeof(AssignmentInsideSubExpression).Assembly
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(DiagnosticAnalyzer)))
                .ToList();

            foreach (var analyzer in analyzers)
            {
                var noSqaleCount = analyzer.GetCustomAttributes<NoSqaleRemediationAttribute>().Count();

                var subCharacteristicCount = analyzer.GetCustomAttributes<SqaleSubCharacteristicAttribute>().Count();

                if (noSqaleCount > 0)
                {
                    if (subCharacteristicCount > 0)
                    {
                        throw new Exception(
                            string.Format("SqaleSubCharacteristicAttribute can only be assigned to DiagnosticAnalyzers that have a SQALE remediation function, '{0}' has NoSqaleRemediationAttribute",
                            analyzer.Name));
                    }
                }
                else
                {
                    if (subCharacteristicCount != 1)
                    {
                        throw new Exception(
                            string.Format("Only one SqaleSubCharacteristicAttribute can be assigned to DiagnosticAnalyzers, '{0}' has {1}",
                            analyzer.Name, subCharacteristicCount));
                    }
                }
            }
        }

        [TestMethod]
        public void SqaleSubCharacteristic()
        {
            var stringsFromJava = new []
            {
                "MODULARITY",
                "TRANSPORTABILITY",
                "COMPILER_RELATED_PORTABILITY",
                "HARDWARE_RELATED_PORTABILITY",
                "LANGUAGE_RELATED_PORTABILITY",
                "OS_RELATED_PORTABILITY",
                "SOFTWARE_RELATED_PORTABILITY",
                "TIME_ZONE_RELATED_PORTABILITY",
                "READABILITY",
                "UNDERSTANDABILITY",
                "API_ABUSE",
                "ERRORS",
                "INPUT_VALIDATION_AND_REPRESENTATION",
                "SECURITY_FEATURES",
                "CPU_EFFICIENCY",
                "MEMORY_EFFICIENCY",
                "NETWORK_USE",
                "ARCHITECTURE_CHANGEABILITY",
                "DATA_CHANGEABILITY",
                "LOGIC_CHANGEABILITY",
                "ARCHITECTURE_RELIABILITY",
                "DATA_RELIABILITY",
                "EXCEPTION_HANDLING",
                "FAULT_TOLERANCE",
                "INSTRUCTION_RELIABILITY",
                "LOGIC_RELIABILITY",
                "RESOURCE_RELIABILITY",
                "SYNCHRONIZATION_RELIABILITY",
                "UNIT_TESTS",
                "INTEGRATION_TESTABILITY",
                "UNIT_TESTABILITY",
                "USABILITY_ACCESSIBILITY",
                "USABILITY_COMPLIANCE",
                "USABILITY_EASE_OF_USE",
                "REUSABILITY_COMPLIANCE",
                "PORTABILITY_COMPLIANCE",
                "MAINTAINABILITY_COMPLIANCE",
                "SECURITY_COMPLIANCE",
                "EFFICIENCY_COMPLIANCE",
                "CHANGEABILITY_COMPLIANCE",
                "RELIABILITY_COMPLIANCE",
                "TESTABILITY_COMPLIANCE",
            };

            var enumValues = Enum.GetValues(typeof (SqaleSubCharacteristic)).Cast<SqaleSubCharacteristic>();
            var enumStrings = enumValues.Select(v => v.ToSonarQubeString()).ToList();

            var matchingEnumStrings = enumStrings.Intersect(stringsFromJava);

            enumStrings.Should().HaveCount(matchingEnumStrings.Count());
        }
    }
}

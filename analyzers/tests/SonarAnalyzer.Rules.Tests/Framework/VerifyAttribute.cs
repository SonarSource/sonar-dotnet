using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

namespace SonarAnalyzer.Rules.Tests.Framework
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class VerifyAttribute : Attribute, ITestBuilder, IImplyFixture
    {
        public VerifyAttribute(Type analyzer, string scenario = null, LanguageVersions versions = LanguageVersions.None)
        {
            Analyzer = new AnalyzerInfo(analyzer);
            Scenario = scenario;
        }

        public AnalyzerInfo Analyzer { get; }
        public string Scenario { get; }
        private static readonly NUnitTestCaseBuilder Builder = new NUnitTestCaseBuilder();

        public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test suite)
        {
            var test = Builder.BuildTestMethod(method, suite, new TestCaseParameters(
                new object[]
                {
                    Analyzer.Instance(),
                }))
                .AddCategories("Rule");

            test.FullName = Analyzer.DiagnosticIds.Any()
                ? $"Rules.{Analyzer.DiagnosticIds.First()}: {Analyzer.Name}"
                : $"Rules.?: {Analyzer.Name}";
            test.Name = string.IsNullOrEmpty(Scenario)
                ? $"[generic] {Analyzer.Language}"
                : $"[{Scenario}] {Analyzer.Language}";
            test.FullName += $".{test.Name}";

            yield return test;
        }
    }
}

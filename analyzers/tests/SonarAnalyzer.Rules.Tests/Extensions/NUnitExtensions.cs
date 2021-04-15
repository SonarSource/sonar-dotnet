using System.Linq;
using NUnit.Framework.Interfaces;
using SonarAnalyzer.Rules.Tests.Framework;

namespace NUnit.Framework.Internal.Builders
{
    internal static class NUnitExtensions
    {
        public static TestMethod AnalyzerTestCase(
            this NUnitTestCaseBuilder builder,
            IMethodInfo method,
            Test suite,
            LanguageVersionInfo language,
            AnalyzerInfo analyzer,
            string scenario) =>

            builder.BuildTestMethod(method, suite, new TestCaseParameters(
                new object[]
                {
                    analyzer.Instance(),
                    new[] { language.Options },
                }))
                .AddCategories(
                    "Rule",
                    language.ToString())
                .WithName(analyzer, language, scenario);

        public static TestMethod AnalyzerTestCase(
            this NUnitTestCaseBuilder builder,
            IMethodInfo method,
            Test suite,
            LanguageVersionInfo language,
            TestCaseInfo testCase,
            AnalyzerInfo analyzer) =>

            builder.BuildTestMethod(method, suite, new TestCaseParameters(
                new object[]
                {
                    testCase.Location.FullName,
                    analyzer.Instance(),
                    new[] { language.Options },
                    testCase.Output,
                    testCase.AdditionalReferences,
                }))
                .AddCategories(
                    "Rule",
                    language.ToString())
                .WithName(analyzer, language, testCase.Scenario);

        private static TestMethod WithName(
            this TestMethod test,
            AnalyzerInfo analyzer,
            LanguageVersionInfo language,
            string scenario)
        {

            test.FullName = analyzer.DiagnosticIds.Any()
                ? $"Rules.{analyzer.DiagnosticIds.First()}: {analyzer.Name}"
                : $"Rules.?: {analyzer.Name}";
            test.Name = string.IsNullOrEmpty(scenario)
                ? $"[generic] {language.DisplayName}"
                : $"[{scenario}] {language.DisplayName}";
            test.FullName += $".{test.Name}";
            return test;
        }

        internal static TTest AddCategories<TTest>(this TTest test, params string[] categories)
            where TTest : Test
        {
            foreach (var category in categories)
            {
                new CategoryAttribute(category).ApplyToTest(test);
            }
            return test;
        }
    }
}

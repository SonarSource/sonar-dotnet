using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
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
            SupportedLanguageVersions = LanguageVersionInfo.Select(versions)
                ?? (Analyzer.Language == LanguageNames.CSharp
                ? LanguageVersionInfo.CSharp()
                : LanguageVersionInfo.VisualBasic());
        }

        public AnalyzerInfo Analyzer { get; }
        public string Scenario { get; }
        public IEnumerable<LanguageVersionInfo> SupportedLanguageVersions { get; }

        private static readonly NUnitTestCaseBuilder Builder = new NUnitTestCaseBuilder();

        public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test suite) =>
            SupportedLanguageVersions
            .Select(languageVersion => Builder.AnalyzerTestCase(method, suite, languageVersion, Analyzer, Scenario));
    }
}

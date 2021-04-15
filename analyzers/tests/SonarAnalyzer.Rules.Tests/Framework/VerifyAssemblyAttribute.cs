using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

namespace SonarAnalyzer.Rules.Tests.Framework
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class VerifyAssemblyAttribute : Attribute, ITestBuilder, IImplyFixture
    {
        public DirectoryInfo Root { get; }
        public IReadOnlyCollection<AnalyzerInfo> Analyzers { get; }
        private static readonly NUnitTestCaseBuilder Builder = new NUnitTestCaseBuilder();

        public VerifyAssemblyAttribute(Type assemblyType)
        {
            Root = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Cases"));
            Analyzers = AnalyzerInfo.FromAssembly(assemblyType.Assembly).ToArray();
        }
        private VerifyAssemblyAttribute() { }

        public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test suite)
        {
            var testCases = TestCaseInfo.FromDirectory(Root).Where(test => !test.IsFix).ToArray();

            foreach (var analyzer in Analyzers)
            {
                foreach (var testCase in testCases.Where(test => test.IsApplicable(analyzer)))
                {
                    foreach (var languageVersion in testCase.SupportedLanguageVersions)
                    {
                        yield return Builder.AnalyzerTestCase(method, suite, languageVersion, testCase, analyzer);
                    }
                }
            }
        }
    }
}

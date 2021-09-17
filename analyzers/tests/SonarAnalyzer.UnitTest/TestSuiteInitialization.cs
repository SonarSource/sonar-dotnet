using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.Helpers;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest
{
    [TestClass]
    public class TestSuiteInitialization
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Console.WriteLine(@"Running tests initialization...");
            Console.WriteLine(@$"Build reason: {Environment.GetEnvironmentVariable(TestContextHelper.BuildReason) ?? "Local build"}");

            var parseOptions = ParseOptionsHelper.GetParseOptionsOrDefault(null).ToList();

            var csVersions = parseOptions.OfType<CSharpParseOptions>().Select(GetVersion);
            Console.WriteLine(@"C# versions used for analysis: " + string.Join(", ", csVersions));

            var vbVersions = parseOptions.OfType<VisualBasicParseOptions>().Select(GetVersion);
            Console.WriteLine(@"VB.Net versions used for analysis: " + string.Join(", ", vbVersions));
        }

        private static string GetVersion(CSharpParseOptions options) =>
            options.LanguageVersion.ToDisplayString();

        private static string GetVersion(VisualBasicParseOptions options) =>
            options.LanguageVersion.ToDisplayString();
    }
}

/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

extern alias csharp;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using csharp::SonarAnalyzer.Rules.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Performance
{
    [TestClass]
    public class PerformanceTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        // This test takes around 40s-60s to run, which is as long as all of the other tests.
        // This category allows us to exclude it from the set of tests to run.
        // In the IDE: set the test filter in the Test Explorer to the following
        //      -Trait:"Slow"
        // dotnet test or vstest.console.exe: pass the following command line argument
        //       --TestCaseFilter:"TestCategory!=Slow"
        [TestCategory("Slow")]
        [Ignore("Skip this one as it adds significant delays. It will be moved to the integration tests.")]
        public void Perf_EntityFrameworkMigration()
        {
            // Repro for https://github.com/SonarSource/sonar-dotnet/issues/2474
            // See notes in the test case file for more info.

            // Analyzers known to timeout against this test code
            // (note: this test doesn't cover the utility analyzers):
            var knownSlowAnalyzers = new[] { "SonarAnalyzer.Rules.CSharp.UnusedPrivateMember" };

            int analysisTimeoutInMs = 700;
            var allAnalyzerTypes = GetSonarAnalyzerTypes(typeof(MetricsAnalyzer).Assembly);
            var compilation = GetEntityFrameworkMigrationCompilation();

            var executionResults = ExecuteAllAnalyzers(compilation, allAnalyzerTypes, analysisTimeoutInMs);

            AssertExpectedAnalyzerPerformance(executionResults, knownSlowAnalyzers);
        }

        /// <summary>
        /// This method runs each of the supplied C# analyzers in series against the compilation.
        /// Any analyzer that takes longer than a specifed timeout is reported and that analysis is stopped.
        /// NOTE: the utility analyzers won't run because the required analysis settings are not set
        /// by the test.
        /// </summary>
        private IEnumerable<ExecutionResult> ExecuteAllAnalyzers(Compilation compilation, IEnumerable<Type> analyzerTypes, int analysisTimeoutInMs)
        {
            // Run an initial analysis so any startup costs aren't attributed to the first analyzer to be tested
            ExecuteAnalyzerWithTimeout(compilation, CreateAnalyzer(analyzerTypes.First()), 5000);

            var executionResults = new List<ExecutionResult>();
            var counter = 0;
            var timer = new Stopwatch();

            // Executing one analyzer at a time (running them in parallel can skew the results)
            foreach (var analyzerType in analyzerTypes)
            {
                var analyzer = CreateAnalyzer(analyzerType);

                // Write to the DEBUG window so we can see what's happening in the output window in the IDE
                counter++;
                Debug.WriteLine($"{counter}: {analyzerType.FullName}");

                timer.Restart();
                var succeeded = ExecuteAnalyzerWithTimeout(compilation, analyzer, analysisTimeoutInMs);
                timer.Stop();

                var newExecutionResult = new ExecutionResult(analyzerType, timer.ElapsedMilliseconds, !succeeded);
                executionResults.Add(newExecutionResult);

                if (!succeeded)
                {
                    Debug.WriteLine($"   -> TIMED OUT");
                }
            }

            DumpExecutionResults(executionResults);
            return executionResults;
        }

        private Compilation GetEntityFrameworkMigrationCompilation()
        {
            var solutionBuilder = SolutionBuilder.CreateSolutionFromPaths(
                new string[] { @"TestCases\Performance\Bug2474_EntityFrameworkMigration.cs" },
                additionalReferences: GetEntityFrameworkReferencesNetCore("2.0.0"));

            var compilation = solutionBuilder.Compile(new CSharpParseOptions(LanguageVersion.Latest)).Single();
            return compilation;
        }

        private static IEnumerable<MetadataReference> GetEntityFrameworkReferencesNetCore(string entityFrameworkVersion) =>
            Enumerable.Empty<MetadataReference>()
                .Concat(FrameworkMetadataReference.Netstandard)
                .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCoreSqlServer(entityFrameworkVersion))
                .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCoreRelational(entityFrameworkVersion));

        private Type[] GetSonarAnalyzerTypes(Assembly assembly)
        {
            var allAnalyzerTypes = assembly.GetTypes()
                .Where(IsSonarAnalyzer).ToArray();

            Log($"Analyzer count: {allAnalyzerTypes.Length}");

            return allAnalyzerTypes;
        }

        private static readonly Type SonarDiagnosticAnalyzerType = typeof(SonarDiagnosticAnalyzer);

        private bool IsSonarAnalyzer(Type type)
        {
            return (!type.IsAbstract && IsDerivedFromSonarDiagnosticAnalyzer(type));

            bool IsDerivedFromSonarDiagnosticAnalyzer(Type typeToCheck)
            {
                var current = typeToCheck.BaseType;

                while (current != null)
                {
                    if (current == SonarDiagnosticAnalyzerType)
                    {
                        return true;
                    }
                    current = current.BaseType;
                }
                return false;
            }
        }

        private SonarDiagnosticAnalyzer CreateAnalyzer(Type analyzerType) =>
            Activator.CreateInstance(analyzerType) as SonarDiagnosticAnalyzer;

        private bool ExecuteAnalyzerWithTimeout(Compilation compilation, SonarDiagnosticAnalyzer analyzer, int timeoutMs)
        {
            var cancellationSource = new CancellationTokenSource(timeoutMs);

            try
            {
                DiagnosticVerifier.GetAllDiagnostics(compilation, new[] { analyzer }, CompilationErrorBehavior.FailTest, cancellationSource.Token);
            }
            catch(AggregateException ex)
            {
                if (ex.InnerExceptions.Any(x => x is OperationCanceledException))
                {
                    return false;
                }
                throw ex;
            }
            return true;
        }

        private void AssertExpectedAnalyzerPerformance(IEnumerable<ExecutionResult> executionResults, IEnumerable<string> expectedSlowAnalyzers)
        {
            // Anything in expectedSlowAnalyzers should be slow;
            // everything else should not time out.

            var actualSlowAnalyzers = executionResults.Where(x => x.TimedOut).Select(x => x.AnalyzerType.FullName);
            AssertNoUnexpectedSlowAnalyzers(actualSlowAnalyzers.Except(expectedSlowAnalyzers, StringComparer.OrdinalIgnoreCase));

            var unexpectedlyFastAnalyzers = expectedSlowAnalyzers.Except(actualSlowAnalyzers, StringComparer.OrdinalIgnoreCase);

            if (unexpectedlyFastAnalyzers.Any())
            {
                string unexpectedlyFastMessage = FormatListToMessage(unexpectedlyFastAnalyzers,
                    "The following analyzers were expected to be slow but performed better than expected - the baseline may need to be updated:");

                Log(unexpectedlyFastMessage);
                Assert.Fail(unexpectedlyFastMessage);
            }

            Log(FormatListToMessage(expectedSlowAnalyzers, "The following analyzers were slow (as expected):"));
        }

        private void AssertNoUnexpectedSlowAnalyzers(IEnumerable<string> slowAnalyzers)
        {
            if (!slowAnalyzers.Any())
            {
                Log("No unexpectedly slow analyzers");
                return;
            }

            string slowAnalyzersMessage = FormatListToMessage(slowAnalyzers, "Unexpectedly slow analyzers:");

            Log(slowAnalyzersMessage);
            Assert.Fail(slowAnalyzersMessage);
        }

        private void DumpExecutionResults(IEnumerable<ExecutionResult> executionResults)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Execution results:");
            foreach (var result in executionResults.OrderByDescending(x => x.ElapsedTimeInMs))
            {
                sb.AppendLine($"  {result.AnalyzerType.FullName}, {result.ElapsedTimeInMs}ms  {(result.TimedOut ? "TIMED OUT" : "")}");
            }
            var message = sb.ToString();

            Log(message);

            // Also dump the output to disk and attach it to the test result.
            // Note that VS will automatically delete the test results if the
            // test passed.
            var filePath = Path.Combine(TestContext.TestRunDirectory,
                $"ExecutionTime_{TestContext.TestName}.log");
            File.WriteAllText(filePath, message);
            TestContext.AddResultFile(filePath);
        }

        private static string FormatListToMessage(IEnumerable<string> list, string messagePrefix = null) =>
            messagePrefix + Environment.NewLine
                + string.Join(", " + Environment.NewLine, list);

        private void Log(string message) =>
            TestContext.WriteLine(message);

        private struct ExecutionResult
        {
            public ExecutionResult(Type analyzerType, long elapsedTimeInMs, bool timedOut)
            {
                AnalyzerType = analyzerType;
                ElapsedTimeInMs = elapsedTimeInMs;
                TimedOut = timedOut;
            }

            public Type AnalyzerType { get; }
            public long ElapsedTimeInMs { get; }
            public bool TimedOut { get; }
        }
    }
}

/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.Helpers;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class CbdeHandlerTest
    {
        [TestMethod]
        public void CbdeHandler_CS()
        {
            using var scope = new EnvironmentVariableScope(false) { InternalLogCBDE = true };
            Verifier.VerifyNonConcurrentAnalyzer(@"TestCases\CbdeHandler.cs", CbdeHandlerRule.MakeUnitTestInstance(null, null));
        }
#if NET
        [TestMethod]
        public void CbdeHandler_CS_FromCSharp9()
        {
            using var scope = new EnvironmentVariableScope(false) { InternalLogCBDE = true };
            Verifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\CbdeHandler.CSharp9.cs",
                                                      CbdeHandlerRule.MakeUnitTestInstance(null, null));
        }
#endif
        [TestMethod]
        public void CbdeHandlerWait()
        {
            var cbdeExecuted = false;
            RunAnalysisWithoutVerification(@"TestCases\CbdeHandlerDummy.cs",
                CbdeHandlerRule.MakeUnitTestInstance(CreateMockPath("CBDEWaitAndSucceeds"),
                s =>
                {
                    cbdeExecuted = true;
                    var logContent = File.ReadAllText(s);
                    Assert.IsTrue(logContent.Contains("Running CBDE: Success"));
                    var workingSet = Regex.Match(logContent, "peak_working_set: ([0-9]*) MB");
                    Assert.IsTrue(workingSet.Success);
                    var peak = workingSet.Groups[1].Value;
                    Assert.IsTrue(int.TryParse(peak, out int peakValue));
                    Assert.AreNotEqual(peakValue, 0); // We had enough time to at least use some memory
                }));
            Assert.IsTrue(cbdeExecuted);
        }

        [TestMethod]
        public void CbdeHandlerExecutableNotFound()
        {
            var cbdeExecuted = false;
            RunAnalysisWithoutVerification(@"TestCases\CbdeHandlerDummy.cs",
                CbdeHandlerRule.MakeUnitTestInstance(CreateMockPath("NonExistingExecutable"),
                s =>
                {
                    cbdeExecuted = true;
                    var logContent = File.ReadAllText(s);
                    Assert.IsTrue(logContent.Contains("Running CBDE: Cannot start process"));
                }));
            Assert.IsTrue(cbdeExecuted);
        }

        [TestMethod]
        public void CbdeHandlerFailed()
        {
            var cbdeExecuted = false;
            RunAnalysisWithoutVerification(@"TestCases\CbdeHandlerDummy.cs",
                CbdeHandlerRule.MakeUnitTestInstance(CreateMockPath("CBDEFails"),
                s =>
                {
                    cbdeExecuted = true;
                    var logContent = File.ReadAllText(s);
                    Assert.IsTrue(logContent.Contains("Running CBDE: Failure"));
                }));
            Assert.IsTrue(cbdeExecuted);
        }

        [TestMethod]
        public void CbdeHandlerIncorrectOutput()
        {
            var cbdeExecuted = false;
            RunAnalysisWithoutVerification(@"TestCases\CbdeHandlerDummy.cs",
                CbdeHandlerRule.MakeUnitTestInstance(CreateMockPath("CBDESucceedsWithIncorrectResults"),
                s =>
                {
                    cbdeExecuted = true;
                    var logContent = File.ReadAllText(s);
                    Assert.IsTrue(logContent.Contains("error parsing result file"));
                }));
            Assert.IsTrue(cbdeExecuted);
        }

        private static void RunAnalysisWithoutVerification(string path, SonarDiagnosticAnalyzer diagnosticAnalyzer,
            IEnumerable<MetadataReference> additionalReferences = null)
        {
            var compilation = SolutionBuilder.Create()
                .AddProject(AnalyzerLanguage.FromPath(path))
                .AddTestReferences()
                .AddReferences(additionalReferences)
                .AddDocument(path)
                .GetCompilation();
            DiagnosticVerifier.GetDiagnostics(compilation, diagnosticAnalyzer,
                CompilationErrorBehavior.FailTest,
                verifyNoExceptionIsThrown: false);
        }

        private static string CreateMockPath(string mockName)
        {
            var assembly = typeof(CbdeHandlerTest).Assembly;
            return Path.Combine(Path.GetDirectoryName(assembly.Location), "CBDEMocks", mockName + ".exe");
        }
    }
}

/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using SonarAnalyzer.SymbolicExecution.Sonar.Analyzers;

using ChecksCS = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;
using ChecksVB = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.VisualBasic;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class InitializationVectorShouldBeRandomTest
    {
        private readonly VerifierBuilder sonar = new VerifierBuilder()
            .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabledWithSonarCfg))
            .WithBasePath(@"SymbolicExecution\Sonar")
            .AddReferences(MetadataReferenceFacade.SystemSecurityCryptography)
            .WithOnlyDiagnostics(InitializationVectorShouldBeRandom.S3329);

        private readonly VerifierBuilder roslynCS = new VerifierBuilder()
            .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabled))
            .WithBasePath(@"SymbolicExecution\Roslyn")
            .AddReferences(MetadataReferenceFacade.SystemSecurityCryptography)
            .WithOnlyDiagnostics(ChecksCS.InitializationVectorShouldBeRandom.S3329);

        private readonly VerifierBuilder roslynVB = new VerifierBuilder<VB.SymbolicExecutionRunner>()
            .WithBasePath(@"SymbolicExecution\Roslyn")
            .WithOnlyDiagnostics(ChecksVB.InitializationVectorShouldBeRandom.S3329)
            .AddReferences(MetadataReferenceFacade.SystemSecurityCryptography);

        [TestMethod]
        public void InitializationVectorShouldBeRandom_Sonar_CS() =>
            sonar.AddPaths("InitializationVectorShouldBeRandom.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

        [TestMethod]
        public void InitializationVectorShouldBeRandom_Roslyn_CS() =>
            roslynCS.AddPaths("InitializationVectorShouldBeRandom.cs").Verify();

        [TestMethod]
        public void InitializationVectorShouldBeRandom_Roslyn_VB() =>
            roslynVB.AddPaths("InitializationVectorShouldBeRandom.vb").Verify();

        [TestMethod]
        public void InitializationVectorShouldBeRandom_DoesNotRaiseIssuesForTestProject_Sonar() =>
            sonar.AddPaths("InitializationVectorShouldBeRandom.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .AddTestReference()
                .VerifyNoIssuesIgnoreErrors();

        [TestMethod]
        public void InitializationVectorShouldBeRandom_DoesNotRaiseIssuesForTestProject_Roslyn_CS() =>
            roslynCS.AddPaths("InitializationVectorShouldBeRandom.cs")
                .AddTestReference()
                .VerifyNoIssuesIgnoreErrors();

        [TestMethod]
        public void InitializationVectorShouldBeRandom_Roslyn_CSharp8() =>
            roslynCS.AddPaths("InitializationVectorShouldBeRandom.CSharp8.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp8)
            .Verify();

#if NET

        [TestMethod]
        public void InitializationVectorShouldBeRandom_Sonar_CSharp9() =>
            sonar.AddPaths("InitializationVectorShouldBeRandom.CSharp9.cs")
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void InitializationVectorShouldBeRandom_Roslyn_CSharp9() =>
            roslynCS.AddPaths("InitializationVectorShouldBeRandom.CSharp9.cs")
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void InitializationVectorShouldBeRandom_Sonar_CSharp10() =>
            sonar.AddPaths("InitializationVectorShouldBeRandom.CSharp10.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .VerifyNoIssues();  // FN, mixed declarations and expressions in deconstruction are not supported

        [TestMethod]
        public void InitializationVectorShouldBeRandom_Roslyn_CSharp10() =>
            roslynCS.AddPaths("InitializationVectorShouldBeRandom.CSharp10.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .Verify();

        [TestMethod]
        public void InitializationVectorShouldBeRandom_Sonar_CSharp11() =>
            sonar.AddPaths("InitializationVectorShouldBeRandom.CSharp11.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .VerifyNoIssues();  // FN, UTF-8 string literals are not supported

        [TestMethod]
        public void InitializationVectorShouldBeRandom_Roslyn_CSharp11() =>
            roslynCS.AddPaths("InitializationVectorShouldBeRandom.CSharp11.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .VerifyNoIssues();  // FN, UTF-8 string literals are not supported

#endif

    }
}

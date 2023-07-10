/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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
using CS = SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
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

        [TestMethod]
        public void InitializationVectorShouldBeRandom_Sonar_CSharp8() =>
            sonar.AddPaths("InitializationVectorShouldBeRandom.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

        [Ignore] // ToDo: Remove after S3329 implementation
        [TestMethod]
        public void InitializationVectorShouldBeRandom_Roslyn_CS() =>
            roslynCS.AddPaths("InitializationVectorShouldBeRandom.cs")
            .Verify();

        [Ignore] // ToDo: Remove after S3329 implementation
        [TestMethod]
        public void InitializationVectorShouldBeRandom_Roslyn_CSharp8() =>
            roslynCS.AddPaths("InitializationVectorShouldBeRandom.CSharp8.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

        [TestMethod]
        public void InitializationVectorShouldBeRandom_Sonar_DoesNotRaiseIssuesForTestProject() =>
            sonar.AddPaths("InitializationVectorShouldBeRandom.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .AddTestReference()
                .VerifyNoIssueReported();

        [Ignore] // ToDo: Remove after S3329 implementation
        [TestMethod]
        public void InitializationVectorShouldBeRandom_DoesNotRaiseIssuesForTestProject_Roslyn_CS() =>
            roslynCS.AddPaths("InitializationVectorShouldBeRandom.cs")
                .AddTestReference()
                .VerifyNoIssueReported();

#if NET

        [TestMethod]
        public void InitializationVectorShouldBeRandom_Sonar_CSharp9() =>
            sonar.AddPaths("InitializationVectorShouldBeRandom.CSharp9.cs")
                .WithTopLevelStatements()
                .Verify();

        [Ignore] // ToDo: Remove after S3329 implementation
        [TestMethod]
        public void InitializationVectorShouldBeRandom_Roslyn_CSharp9() =>
            roslynCS.AddPaths("InitializationVectorShouldBeRandom.CSharp9.cs")
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void InitializationVectorShouldBeRandom_Sonar_CSharp10() =>
            sonar.AddPaths("InitializationVectorShouldBeRandom.CSharp10.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .Verify();

        [Ignore] // ToDo: Remove after S3329 implementation
        [TestMethod]
        public void InitializationVectorShouldBeRandom_Roslyn_CSharp10() =>
            roslynCS.AddPaths("InitializationVectorShouldBeRandom.CSharp10.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .Verify();

        [TestMethod]
        public void InitializationVectorShouldBeRandom_Sonar_CSharp11() =>
            sonar.AddPaths("InitializationVectorShouldBeRandom.CSharp11.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .Verify();

        [Ignore] // ToDo: Remove after S3329 implementation
        [TestMethod]
        public void InitializationVectorShouldBeRandom_Roslyn_CSharp11() =>
            roslynCS.AddPaths("InitializationVectorShouldBeRandom.CSharp11.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .Verify();

#endif

    }
}

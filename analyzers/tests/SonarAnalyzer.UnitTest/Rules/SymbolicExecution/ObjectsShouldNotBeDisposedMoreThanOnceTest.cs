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

using SonarAnalyzer.Common;
using SonarAnalyzer.SymbolicExecution.Sonar.Analyzers;
using ChecksCS = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;
using CS = SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class ObjectsShouldNotBeDisposedMoreThanOnceTest
    {
        private readonly VerifierBuilder sonar = new VerifierBuilder()
            .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabledWithSonarCfg))
            .WithBasePath(@"SymbolicExecution\Sonar")
            .WithOnlyDiagnostics(ObjectsShouldNotBeDisposedMoreThanOnce.S3966);

        private readonly VerifierBuilder roslynCS = new VerifierBuilder()
            .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabled))
            .WithBasePath(@"SymbolicExecution\Roslyn")
            .WithOnlyDiagnostics(ChecksCS.ObjectsShouldNotBeDisposedMoreThanOnce.S3966);

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void ObjectsShouldNotBeDisposedMoreThanOnce_Sonar_CSharp8(ProjectType projectType) =>
            sonar.AddPaths("ObjectsShouldNotBeDisposedMoreThanOnce.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .AddReferences(TestHelper.ProjectTypeReference(projectType))
                .AddReferences(MetadataReferenceFacade.NETStandard21)
                .Verify();

        [Ignore] // ToDo: Remove after S3966 implementation
        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void ObjectsShouldNotBeDisposedMoreThanOnce_Roslyn_CS(ProjectType projectType) =>
            roslynCS.AddPaths("ObjectsShouldNotBeDisposedMoreThanOnce.cs")
                .AddReferences(TestHelper.ProjectTypeReference(projectType))
                .Verify();

#if NET

        [Ignore] // ToDo: Remove after S3966 implementation
        [TestMethod]
        public void ObjectsShouldNotBeDisposedMoreThanOnce_Roslyn_CSharp8() =>
            roslynCS.AddPaths("ObjectsShouldNotBeDisposedMoreThanOnce.CSharp8.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

        [TestMethod]
        public void ObjectsShouldNotBeDisposedMoreThanOnce_Sonar_CSharp9() =>
            sonar.AddPaths("ObjectsShouldNotBeDisposedMoreThanOnce.CSharp9.cs")
            .WithTopLevelStatements()
            .Verify();

        [Ignore] // ToDo: Remove after S3966 implementation
        [TestMethod]
        public void ObjectsShouldNotBeDisposedMoreThanOnce_Roslyn_CSharp9() =>
            roslynCS.AddPaths("ObjectsShouldNotBeDisposedMoreThanOnce.CSharp9.cs")
                .WithTopLevelStatements()
                .Verify();

#endif

    }
}

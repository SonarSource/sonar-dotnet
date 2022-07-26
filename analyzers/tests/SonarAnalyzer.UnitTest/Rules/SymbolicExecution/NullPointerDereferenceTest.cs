/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;

namespace SonarAnalyzer.UnitTest.Rules.SymbolicExecution
{
    [TestClass]
    public class NullPointerDereferenceTest
    {
        private readonly VerifierBuilder sonar = new VerifierBuilder()
            .AddAnalyzer(() => new SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabledWithSonarCfg))
            .WithBasePath(@"SymbolicExecution\Sonar")
            .WithOnlyDiagnostics(new[] { NullPointerDereference.S2259 });

        private readonly VerifierBuilder roslyn = new VerifierBuilder()
            .AddAnalyzer(() => new SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabled))
            .WithBasePath(@"SymbolicExecution\Roslyn")
            .WithOnlyDiagnostics(new[] { NullPointerDereference.S2259 });

        [TestMethod]
        public void NullPointerDereference_Sonar_CS() =>
            sonar.AddPaths("NullPointerDereference.cs").WithConcurrentAnalysis(false).Verify();

        [TestMethod]
        public void NullPointerDereference_Roslyn_CS() =>
            roslyn.AddPaths("NullPointerDereference.cs").WithConcurrentAnalysis(false).Verify();

        [TestMethod]
        public void NullPointerDereference_Sonar_DoesNotRaiseIssuesForTestProject() =>
            sonar.AddTestReference().AddPaths("NullPointerDereference.cs").WithConcurrentAnalysis(false).VerifyNoIssueReported();

        [TestMethod]
        public void NullPointerDereference_Roslyn_DoesNotRaiseIssuesForTestProject() =>
            roslyn.AddTestReference().AddPaths("NullPointerDereference.cs").WithConcurrentAnalysis(false).VerifyNoIssueReported();

        [TestMethod]
        public void NullPointerDereference_Sonar_CSharp6() =>
            sonar.AddPaths("NullPointerDereference.CSharp6.cs").WithOptions(ParseOptionsHelper.FromCSharp6).Verify();

        [TestMethod]
        public void NullPointerDereference_Roslyn_CSharp6() =>
            roslyn.AddPaths("NullPointerDereference.CSharp6.cs").WithOptions(ParseOptionsHelper.FromCSharp6).Verify();

        [TestMethod]
        public void NullPointerDereference_Sonar_CSharp7() =>
            sonar.AddPaths("NullPointerDereference.CSharp7.cs").WithOptions(ParseOptionsHelper.FromCSharp7).Verify();

        [TestMethod]
        public void NullPointerDereference_Roslyn_CSharp7() =>
            roslyn.AddPaths("NullPointerDereference.CSharp7.cs").WithOptions(ParseOptionsHelper.FromCSharp7).Verify();

        [TestMethod]
        public void NullPointerDereference_Sonar_CSharp8() =>
            sonar.AddPaths("NullPointerDereference.CSharp8.cs").AddReferences(MetadataReferenceFacade.NETStandard21).WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

        [TestMethod]
        public void NullPointerDereference_Roslyn_CSharp8() =>
            roslyn.AddPaths("NullPointerDereference.CSharp8.cs").AddReferences(MetadataReferenceFacade.NETStandard21).WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

#if NET

        [TestMethod]
        public void NullPointerDereference_Sonar_CSharp9() =>
            sonar.AddPaths("NullPointerDereference.CSharp9.cs").WithTopLevelStatements().Verify();

        [TestMethod]
        public void NullPointerDereference_Roslyn_CSharp9() =>
            roslyn.AddPaths("NullPointerDereference.CSharp9.cs").WithTopLevelStatements().Verify();

        [TestMethod]
        public void NullPointerDereference_Sonar_CSharp10() =>
            sonar.AddPaths("NullPointerDereference.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

        [TestMethod]
        public void NullPointerDereference_Roslyn_CSharp10() =>
            roslyn.AddPaths("NullPointerDereference.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

#endif

    }
}

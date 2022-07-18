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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules.SymbolicExecution
{
    [TestClass]
    public class NullPointerDereferenceTest
    {
        private readonly VerifierBuilder verifierSonar = new VerifierBuilder<SymbolicExecutionRunner>()
            .WithBasePath(@"SymbolicExecution\Sonar")
            .WithOnlyDiagnostics(new[] { NullPointerDereference.S2259 });

        [TestMethod]
        public void NullPointerDereference_CS() =>
            verifierSonar.AddPaths("NullPointerDereference.cs").WithConcurrentAnalysis(false).Verify();

        [TestMethod]
        public void NullPointerDereference_DoesNotRaiseIssuesForTestProject() =>
            verifierSonar.AddTestReference().AddPaths("NullPointerDereference.cs").WithConcurrentAnalysis(false).VerifyNoIssueReported();

        [TestMethod]
        public void NullPointerDereference_CSharp6() =>
            verifierSonar.AddPaths("NullPointerDereference.CSharp6.cs").WithOptions(ParseOptionsHelper.FromCSharp6).Verify();

        [TestMethod]
        public void NullPointerDereference_CSharp7() =>
            verifierSonar.AddPaths("NullPointerDereference.CSharp7.cs").WithOptions(ParseOptionsHelper.FromCSharp7).Verify();

        [TestMethod]
        public void NullPointerDereference_CSharp8() =>
            verifierSonar.AddPaths("NullPointerDereference.CSharp8.cs").AddReferences(MetadataReferenceFacade.NETStandard21).WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

#if NET

        [TestMethod]
        public void NullPointerDereference_CSharp9() =>
            verifierSonar.AddPaths("NullPointerDereference.CSharp9.cs").WithTopLevelStatements().Verify();

        [TestMethod]
        public void NullPointerDereference_CSharp10() =>
            verifierSonar.AddPaths("NullPointerDereference.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

#endif

    }
}

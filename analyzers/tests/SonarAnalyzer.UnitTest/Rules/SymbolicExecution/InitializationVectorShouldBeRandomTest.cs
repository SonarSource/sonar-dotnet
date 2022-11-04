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
using SonarAnalyzer.SymbolicExecution.Sonar.Analyzers;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class InitializationVectorShouldBeRandomTest
    {
        private readonly VerifierBuilder sonarVerifier = new VerifierBuilder<SymbolicExecutionRunner>().WithBasePath(@"SymbolicExecution\Sonar")
            .WithOnlyDiagnostics(InitializationVectorShouldBeRandom.S3329)
            .AddReferences(MetadataReferenceFacade.SystemSecurityCryptography);

        [TestMethod]
        public void InitializationVectorShouldBeRandom_CSharp8() =>
            sonarVerifier.AddPaths("InitializationVectorShouldBeRandom.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

        [TestMethod]
        public void InitializationVectorShouldBeRandom_DoesNotRaiseIssuesForTestProject() =>
            sonarVerifier.AddPaths("InitializationVectorShouldBeRandom.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .AddTestReference()
                .VerifyNoIssueReported();

#if NET

        [TestMethod]
        public void InitializationVectorShouldBeRandom_CSharp9() =>
            sonarVerifier.AddPaths("InitializationVectorShouldBeRandom.CSharp9.cs")
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void InitializationVectorShouldBeRandom_CSharp10() =>
            sonarVerifier.AddPaths("InitializationVectorShouldBeRandom.CSharp10.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .Verify();

        [TestMethod]
        public void InitializationVectorShouldBeRandom_CSharp11() =>
            sonarVerifier.AddPaths("InitializationVectorShouldBeRandom.CSharp11.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .Verify();

#endif

    }
}

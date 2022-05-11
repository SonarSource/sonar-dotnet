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
using SonarAnalyzer.Rules.SymbolicExecution;
using SonarAnalyzer.UnitTest.MetadataReferences;

namespace SonarAnalyzer.UnitTest.Rules.SymbolicExecution
{
    [TestClass]
    public class InitializationVectorShouldBeRandomTest
    {
        private static readonly DiagnosticDescriptor[] OnlyDiagnostics = new[] { InitializationVectorShouldBeRandom.S3329 };

        [TestMethod]
        public void InitializationVectorShouldBeRandom_CS() =>
            OldVerifier.VerifyAnalyzer(
                @"TestCases\SymbolicExecution\Sonar\InitializationVectorShouldBeRandom.cs",
                new SymbolicExecutionRunner(),
                ParseOptionsHelper.FromCSharp8,
                MetadataReferenceFacade.SystemSecurityCryptography,
                onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        public void InitializationVectorShouldBeRandom_DoesNotRaiseIssuesForTestProject() =>
            OldVerifier.VerifyNoIssueReportedInTest(
                @"TestCases\SymbolicExecution\Sonar\InitializationVectorShouldBeRandom.cs",
                new SymbolicExecutionRunner(),
                ParseOptionsHelper.FromCSharp8,
                MetadataReferenceFacade.SystemSecurityCryptography,
                onlyDiagnostics: OnlyDiagnostics);

#if NET

        [TestMethod]
        public void InitializationVectorShouldBeRandom_CSharp9() =>
            OldVerifier.VerifyAnalyzerFromCSharp9Console(
                @"TestCases\SymbolicExecution\Sonar\InitializationVectorShouldBeRandom.CSharp9.cs",
                new SymbolicExecutionRunner(),
                MetadataReferenceFacade.SystemSecurityCryptography,
                onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        public void InitializationVectorShouldBeRandom_CSharp10() =>
            OldVerifier.VerifyAnalyzerFromCSharp10Library(
                @"TestCases\SymbolicExecution\Sonar\InitializationVectorShouldBeRandom.CSharp10.cs",
                new SymbolicExecutionRunner(),
                MetadataReferenceFacade.SystemSecurityCryptography,
                onlyDiagnostics: OnlyDiagnostics);

#endif
    }
}

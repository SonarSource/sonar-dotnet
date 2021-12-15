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

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.Rules.SymbolicExecution;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class ConditionEvaluatesToConstantTest
    {
        private static readonly DiagnosticDescriptor[] OnlyDiagnostics = new[] { ConditionEvaluatesToConstant.S2583, ConditionEvaluatesToConstant.S2589 };

        [TestMethod]
        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void ConditionEvaluatesToConstant_Scope(ProjectType projectType) =>
            Verifier.VerifyAnalyzer(
                @"TestCases\SymbolicExecution\Sonar\ConditionEvaluatesToConstant.cs",
                new SymbolicExecutionRunner(),
                NuGetMetadataReference.MicrosoftExtensionsPrimitives("3.1.7").Concat(TestHelper.ProjectTypeReference(projectType)),
                onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        public void ConditionEvaluatesToConstant_FromCSharp7() =>
            Verifier.VerifyAnalyzer(
                @"TestCases\SymbolicExecution\Sonar\ConditionEvaluatesToConstant.CSharp7.cs",
                new SymbolicExecutionRunner(),
                ParseOptionsHelper.FromCSharp7,
                onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        public void ConditionEvaluatesToConstant_FromCSharp8() =>
            Verifier.VerifyAnalyzer(
                @"TestCases\SymbolicExecution\Sonar\ConditionEvaluatesToConstant.CSharp8.cs",
                new SymbolicExecutionRunner(),
                ParseOptionsHelper.FromCSharp8,
                MetadataReferenceFacade.NETStandard21,
                onlyDiagnostics: OnlyDiagnostics);

#if NET
        [TestMethod]
        public void ConditionEvaluatesToConstant_FromCSharp9() =>
            Verifier.VerifyAnalyzerFromCSharp9Library(
                @"TestCases\SymbolicExecution\Sonar\ConditionEvaluatesToConstant.CSharp9.cs",
                new SymbolicExecutionRunner(),
                onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        public void ConditionEvaluatesToConstant_FromCSharp9_TopLevelStatements() =>
            Verifier.VerifyAnalyzerFromCSharp9Console(
                @"TestCases\SymbolicExecution\Sonar\ConditionEvaluatesToConstant.CSharp9.TopLevelStatements.cs",
                new SymbolicExecutionRunner(),
                onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        public void ConditionEvaluatesToConstant_FromCSharp10() =>
            Verifier.VerifyAnalyzerFromCSharp10Library(
                @"TestCases\SymbolicExecution\Sonar\ConditionEvaluatesToConstant.CSharp10.cs",
                new SymbolicExecutionRunner(),
                onlyDiagnostics: OnlyDiagnostics);
#endif
    }
}

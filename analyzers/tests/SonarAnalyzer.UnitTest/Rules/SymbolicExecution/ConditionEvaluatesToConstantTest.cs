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
    public class ConditionEvaluatesToConstantTest
    {
        private readonly VerifierBuilder sonar = new VerifierBuilder<SymbolicExecutionRunner>().WithBasePath(@"SymbolicExecution\Sonar")
            .WithOnlyDiagnostics(new[] { ConditionEvaluatesToConstant.S2583, ConditionEvaluatesToConstant.S2589 });

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void ConditionEvaluatesToConstant_CS(ProjectType projectType) =>
            sonar.AddPaths("ConditionEvaluatesToConstant.cs")
                .AddReferences(NuGetMetadataReference.MicrosoftExtensionsPrimitives("3.1.7").Concat(TestHelper.ProjectTypeReference(projectType)))
                .Verify();

        [TestMethod]
        public void ConditionEvaluatesToConstant_FromCSharp7() =>
            sonar.AddPaths("ConditionEvaluatesToConstant.CSharp7.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp7)
                .Verify();

        [TestMethod]
        public void ConditionEvaluatesToConstant_FromCSharp8() =>
            sonar.AddPaths("ConditionEvaluatesToConstant.CSharp8.cs")
                .AddReferences(MetadataReferenceFacade.NETStandard21)
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

#if NET

        [TestMethod]
        public void ConditionEvaluatesToConstant_FromCSharp9() =>
            sonar.AddPaths("ConditionEvaluatesToConstant.CSharp9.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .Verify();

        [TestMethod]
        public void ConditionEvaluatesToConstant_FromCSharp9_TopLevelStatements() =>
            sonar.AddPaths("ConditionEvaluatesToConstant.CSharp9.TopLevelStatements.cs")
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void ConditionEvaluatesToConstant_FromCSharp10() =>
            sonar.AddPaths("ConditionEvaluatesToConstant.CSharp10.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .WithConcurrentAnalysis(false)
                .Verify();

#endif

    }
}

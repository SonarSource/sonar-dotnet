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

using System;
using System.Linq;
using FluentAssertions;
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
        [TestMethod]
        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        [TestCategory("Rule")]
        public void ConditionEvaluatesToConstant(ProjectType projectType) =>
            Verifier.VerifyAnalyzer(@"TestCases\ConditionEvaluatesToConstant.cs",
                                    GetAnalyzer(),
                                    NuGetMetadataReference.MicrosoftExtensionsPrimitives("3.1.7").Concat(TestHelper.ProjectTypeReference(projectType)));

        [TestMethod]
        [TestCategory("Rule")]
        public void ConditionEvaluatesToConstant_FromCSharp7() =>
            Verifier.VerifyAnalyzer(@"TestCases\ConditionEvaluatesToConstant.CSharp7.cs",
                                    GetAnalyzer(),
                                    ParseOptionsHelper.FromCSharp7);

        [TestMethod]
        [TestCategory("Rule")]
        public void ConditionEvaluatesToConstant_FromCSharp8() =>
            Verifier.VerifyAnalyzer(@"TestCases\ConditionEvaluatesToConstant.CSharp8.cs",
                                    GetAnalyzer(),
#if NETFRAMEWORK
                                    ParseOptionsHelper.FromCSharp8,
                                    NuGetMetadataReference.NETStandardV2_1_0);
#else
                                    ParseOptionsHelper.FromCSharp8);
#endif

#if NET
        [TestMethod]
        [TestCategory("Rule")]
        public void ConditionEvaluatesToConstant_FromCSharp9() =>
            Verifier.VerifyAnalyzerFromCSharp9Library(@"TestCases\ConditionEvaluatesToConstant.CSharp9.cs", GetAnalyzer());

        [TestMethod]
        [TestCategory("Rule")]
        public void ConditionEvaluatesToConstant_FromCSharp9_TopLevelStatements() =>
            Verifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\ConditionEvaluatesToConstant.CSharp9.TopLevelStatements.cs", GetAnalyzer());
#endif

        [TestMethod]
        [TestCategory("Rule")]
        public void ConditionEvaluatesToConstant_UncaughtException()
        {
            Action action = () => Verifier.VerifyCSharpAnalyzer(@"
using System;
public class Reproducer
{
    private DateTime? foo;

    public virtual DateTime? Foo
    {
        get { return foo; }

        set
        {
            if (value.HasValue)
            {
                value = DateTime.SpecifyKind(value.Value, DateTimeKind.Unspecified);
            }

            if (foo != value || (foo.HasValue && value.HasValue && foo.Value.Kind != value.Value.Kind))
            {
                foo = value;
                Bar(""x"");
            }
        }
    }
    public void Bar(string x) { }
}", GetAnalyzer(), CompilationErrorBehavior.Ignore);

            // https://github.com/SonarSource/sonar-dotnet/issues/4573
            action.Should().Throw<AssertFailedException>().Where(e =>
                e.Message.Contains("Expected diagnostics {error AD0001: Analyzer 'SonarAnalyzer.Rules.SymbolicExecution.SymbolicExecutionRunner' threw an exception of type " +
                "'SonarAnalyzer.SymbolicExecution.SymbolicExecutionException' with message 'Error processing method: set_Foo ## Method file: snippet1.cs " +
                "## Method line: 11,8 ## Inner exception: System.NotSupportedException: Unexpected constraint type: ObjectConstraint.Old constraints: NoValue " +
                "##    at SonarAnalyzer.SymbolicExecution.SymbolicValue.TrySetObjectConstraint(ObjectConstraint constraint, SymbolicValueConstraints oldConstraints, ProgramState programState)"));
        }

        private static SonarDiagnosticAnalyzer GetAnalyzer() =>
            new SymbolicExecutionRunner(new ConditionEvaluatesToConstant());
    }
}

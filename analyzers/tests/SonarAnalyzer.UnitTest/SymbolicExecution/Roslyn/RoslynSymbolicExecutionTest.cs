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
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    [TestClass]
    public class RoslynSymbolicExecutionTest
    {
        [TestMethod]
        public void Constructor_Throws()
        {
            var cfg = TestHelper.CompileCfg("public class Sample { public void Main() { } }");
            var check = new Mock<SymbolicExecutionCheck>().Object;
            ((Action)(() => new RoslynSymbolicExecution(null, new[] { check }))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("cfg");
            ((Action)(() => new RoslynSymbolicExecution(cfg, null))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("checks");
            ((Action)(() => new RoslynSymbolicExecution(cfg, Array.Empty<SymbolicExecutionCheck>()))).Should().Throw<ArgumentException>().WithMessage("At least one check is expected*");
        }

        [TestMethod]
        public void SequentialInput_CS()
        {
            var context = CreateContextCS("var a = true; var b = false; b = !b; a = (b);");
            context.Collector.ValidateOrder(
                "Literal: true",
                "Literal: false",
                "LocalReference: b",
                "LocalReference: b",
                "UnaryOperator: !b",
                "SimpleAssignment: b = !b",
                "ExpressionStatement: b = !b;",
                "LocalReference: a",
                "LocalReference: b",
                "SimpleAssignment: a = (b)",
                "ExpressionStatement: a = (b);");
        }

        [TestMethod]
        public void SequentialInput_VB()
        {
            var context = CreateContextVB("Dim A As Boolean = True, B As Boolean = False : B = Not B : A = (B)");
            context.Collector.ValidateOrder(
                "Literal: True",
                "Literal: False",
                "LocalReference: B",
                "LocalReference: B",
                "UnaryOperator: Not B",
                "ExpressionStatement: B = Not B",
                "LocalReference: A",
                "LocalReference: B",
                "Parenthesized: (B)",
                "ExpressionStatement: A = (B)");
        }

        private Context CreateContextCS(string methodBody, string additionalParameters = null)
        {
            var code = $@"
public class Sample
{{
    public void Main(bool boolParameter{additionalParameters})
    {{
        {methodBody}
    }}

    private string Method(params string[] args) => null;
    private bool IsMethod(params bool[] args) => true;
}}";
            return new Context(code, true);
        }

        private Context CreateContextVB(string methodBody, string additionalParameters = null)
        {
            var code = $@"
Public Class Sample

    Public Sub Main(BoolParameter As Boolean{additionalParameters})
        {methodBody}
    End Sub

    Private Function Method(ParamArray Args() As String) As String
    End Function

    Private Function IsMethod(ParamArray Args() As Boolean) As Boolean
    End Function

End Class";
            return new Context(code, false);
        }

        private class Context
        {
            public readonly CollectorCheck Collector = new();
            private readonly RoslynSymbolicExecution se;

            public Context(string code, bool isCSharp)
            {
                var cfg = TestHelper.CompileCfg(code, isCSharp);
                se = new RoslynSymbolicExecution(cfg, new[] { Collector });
                se.Execute();
            }
        }

        private class CollectorCheck : SymbolicExecutionCheck
        {
            // ToDo: Simplified version for now, we'll need ProgramState & Operation. Or even better, the whole exploded Node
            private readonly List<IOperationWrapperSonar> preProcessedOperations = new();

            public override ProgramState PreProcess(ProgramState state, IOperationWrapperSonar operation)
            {
                preProcessedOperations.Add(operation);
                return state;
            }

            public void ValidateOrder(params string[] expected) =>
                preProcessedOperations.Where(x => !x.IsImplicit).Select(TestHelper.Serialize).Should().OnlyContainInOrder(expected);
        }
    }
}

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
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.CFG.Sonar;
using SonarAnalyzer.LiveVariableAnalysis;
using SonarAnalyzer.LiveVariableAnalysis.CSharp;
using SonarAnalyzer.UnitTest.CFG.Sonar;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.LiveVariableAnalysis
{
    [TestClass]
    public class LiveVariableAnalysisTest
    {
        [TestMethod]
        public void StaticLocalFunction_ExpressionLiveIn()
        {
            var code = @"
outParameter = LocalFunction(inParameter);
static int LocalFunction(int a) => a + 1;";
            var context = new LiveVariableAnalysisContext(code, "LocalFunction");
            context.Validate(context.Cfg.EntryBlock, new LiveIn("a"));
        }

        [TestMethod]
        public void StaticLocalFunction_ExpressionNotLiveIn()
        {
            var code = @"
outParameter = LocalFunction(0);
static int LocalFunction(int a) => 42;";
            var context = new LiveVariableAnalysisContext(code, "LocalFunction");
            context.Validate(context.Cfg.EntryBlock);
        }

        [TestMethod]
        public void StaticLocalFunction_LiveIn()
        {
            var code = @"
outParameter = LocalFunction(inParameter);
static int LocalFunction(int a)
{
    return a + 1
};";
            var context = new LiveVariableAnalysisContext(code, "LocalFunction");
            context.Validate(context.Cfg.EntryBlock, new LiveIn("a"));
        }

        [TestMethod]
        public void StaticLocalFunction_NotLiveIn()
        {
            var code = @"
outParameter = LocalFunction(0);
static int LocalFunction(int a)
{
    return 42
};";
            var context = new LiveVariableAnalysisContext(code, "LocalFunction");
            context.Validate(context.Cfg.EntryBlock);
        }

        [TestMethod]
        public void StaticLocalFunction_Recursive()
        {
            var code = @"
outParameter = LocalFunction(inParameter);
static int LocalFunction(int a)
{
    if(a <= 0)
        return 0;
    else
        return LocalFunction(a - 1);
};";
            var context = new LiveVariableAnalysisContext(code, "LocalFunction");
            context.Validate(context.Cfg.EntryBlock, new LiveIn("a"), new LiveOut("a"));
        }

        private class LiveVariableAnalysisContext
        {
            public readonly AbstractLiveVariableAnalysis Lva;
            public readonly IControlFlowGraph Cfg;

            public LiveVariableAnalysisContext(string methodBody, string localFunctionName = null)
            {
                var code = @$"
public class Sample
{{
    public void Main(bool inParameter, out bool outParameter)
    {{
        {methodBody}
    }}
}}";
                var method = SonarControlFlowGraphTest.CompileWithMethodBody(code, "Main", out var semanticModel);
                IMethodSymbol symbol;
                CSharpSyntaxNode body;
                if (localFunctionName == null)
                {
                    symbol = semanticModel.GetDeclaredSymbol(method);
                    body = method.Body;
                }
                else
                {
                    var function = (LocalFunctionStatementSyntaxWrapper)method.DescendantNodes()
                        .Single(x => x.Kind() == SyntaxKindEx.LocalFunctionStatement && ((LocalFunctionStatementSyntaxWrapper)x).Identifier.Text == localFunctionName);
                    symbol = semanticModel.GetDeclaredSymbol(function) as IMethodSymbol;
                    body = (CSharpSyntaxNode)function.Body ?? function.ExpressionBody;
                }
                Cfg = CSharpControlFlowGraph.Create(body, semanticModel);
                Lva = CSharpLiveVariableAnalysis.Analyze(Cfg, symbol, semanticModel);
            }

            public void Validate(Block block, params Expected[] expected)
            {
                // This is not very nice from OOP perspective, but it makes UTs above easy to read.
                var expectedLiveIn = expected.OfType<LiveIn>().SingleOrDefault() ?? new LiveIn();
                var expectedLiveOut = expected.OfType<LiveOut>().SingleOrDefault() ?? new LiveOut();
                var expectedCaptured = expected.OfType<Captured>().SingleOrDefault() ?? new Captured();
                Lva.GetLiveIn(block).Select(x => x.Name).Should().BeEquivalentTo(expectedLiveIn.Names);
                Lva.GetLiveOut(block).Select(x => x.Name).Should().BeEquivalentTo(expectedLiveOut.Names);
                Lva.CapturedVariables.Select(x => x.Name).Should().BeEquivalentTo(expectedCaptured.Names);
            }
        }

        private abstract class Expected
        {
            public readonly string[] Names;

            protected Expected(string[] names) =>
                Names = names;
        }

        private class LiveIn : Expected
        {
            public LiveIn(params string[] names) : base(names) { }
        }

        private class LiveOut : Expected
        {
            public LiveOut(params string[] names) : base(names) { }
        }

        private class Captured : Expected
        {
            public Captured(params string[] names) : base(names) { }
        }
    }
}

/*
* SonarAnalyzer for .NET
* Copyright (C) 2015-2019 SonarSource SA
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

extern alias csharp;
using System.Linq;
using csharp::SonarAnalyzer.LiveVariableAnalysis.CSharp;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.ControlFlowGraph;
using SonarAnalyzer.ControlFlowGraph.CSharp;
using SonarAnalyzer.ShimLayer.CSharp;
using SonarAnalyzer.LiveVariableAnalysis;
using SonarAnalyzer.UnitTest.Helpers;

namespace SonarAnalyzer.UnitTest.LiveVariableAnalysis
{
    [TestClass]
    public class LiveVariableAnalysisTests
    {
        private const string TestInput = @"
namespace NS
{{
  public class Foo
  {{
    public void Main(bool inParameter, out bool outParameter)
    {{
      {0}
    }}
  }}
}}";

        [TestMethod]
        public void LiveVariableAnalysis_StaticLocalFunction_ExpressionLiveIn()
        {
            var context = new LiveVariableAnalysisContext(
@"static int LocalFunction(int a) => a + 1;
outParameter = LocalFunction(inParameter);"
            , "LocalFunction");
            var liveIn = context.LVA.GetLiveIn(context.CFG.EntryBlock).OfType<IParameterSymbol>().ToArray();
            liveIn.Should().HaveCount(1);
            liveIn.Single().Name.Should().Be("a");
        }

        [TestMethod]
        public void LiveVariableAnalysis_StaticLocalFunction_ExpressionNotLiveIn()
        {
            var context = new LiveVariableAnalysisContext(
@"static int LocalFunction(int a) => 42;
outParameter = LocalFunction(0);"
            , "LocalFunction");
            var liveIn = context.LVA.GetLiveIn(context.CFG.EntryBlock).OfType<IParameterSymbol>().ToArray();
            liveIn.Should().BeEmpty();
        }

        [TestMethod]
        public void LiveVariableAnalysis_StaticLocalFunction_LiveIn()
        {
            var context = new LiveVariableAnalysisContext(
@"static int LocalFunction(int a)
{
    return a + 1
};
outParameter = LocalFunction(inParameter);"
            , "LocalFunction");
            var liveIn = context.LVA.GetLiveIn(context.CFG.EntryBlock).OfType<IParameterSymbol>().ToArray();
            liveIn.Should().HaveCount(1);
            liveIn.Single().Name.Should().Be("a");
        }

        [TestMethod]
        public void LiveVariableAnalysis_StaticLocalFunction_NotLiveIn()
        {
            var context = new LiveVariableAnalysisContext(
@"static int LocalFunction(int a)
{
    return 42
};
outParameter = LocalFunction(0);"
            , "LocalFunction");
            var liveIn = context.LVA.GetLiveIn(context.CFG.EntryBlock).OfType<IParameterSymbol>().ToArray();
            liveIn.Should().BeEmpty();
        }

        [TestMethod]
        public void LiveVariableAnalysis_StaticLocalFunction_Recursive()
        {
            var context = new LiveVariableAnalysisContext(
@"static int LocalFunction(int a)
{
    if(a <= 0)
        return 0;
    else
        return LocalFunction(a - 1);
};
outParameter = LocalFunction(inParameter);"
            , "LocalFunction");
            var liveIn = context.LVA.GetLiveIn(context.CFG.EntryBlock).OfType<IParameterSymbol>().ToArray();
            liveIn.Should().HaveCount(1);
            liveIn.Single().Name.Should().Be("a");
        }

        private class LiveVariableAnalysisContext
        {
            public readonly AbstractLiveVariableAnalysis LVA;
            public readonly IControlFlowGraph CFG;

            public LiveVariableAnalysisContext(string methodBody, string localFunctionName = null)
            {
                var method = ControlFlowGraphTest.CompileWithMethodBody(string.Format(TestInput, methodBody), "Main", out var semanticModel);
                IMethodSymbol symbol;
                CSharpSyntaxNode body;
                if (localFunctionName == null)
                {
                    symbol = semanticModel.GetDeclaredSymbol(method);
                    body = method.Body;
                }
                else
                {
                    var function = (LocalFunctionStatementSyntaxWrapper)method.DescendantNodes().Single(x => x.Kind() == SyntaxKindEx.LocalFunctionStatement && ((LocalFunctionStatementSyntaxWrapper)x).Identifier.Text == localFunctionName);
                    symbol = semanticModel.GetDeclaredSymbol(function) as IMethodSymbol;
                    body = (CSharpSyntaxNode)function.Body ?? function.ExpressionBody;
                }
                this.CFG = CSharpControlFlowGraph.Create(body, semanticModel);
                this.LVA = CSharpLiveVariableAnalysis.Analyze(this.CFG, symbol, semanticModel);
            }
        }

    }
}

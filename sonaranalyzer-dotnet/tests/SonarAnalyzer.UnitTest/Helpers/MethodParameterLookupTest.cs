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

using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.TestFramework;
using CSharpSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;
using VBSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class MethodParameterLookupTest
    {


        [TestMethod]
        public void TestMethodParameterLookup_CS()
        {
            const string Source = @"
namespace Test
{
    class TestClass
    {
        void Main()
        {
            DoNothing();
            DoSomething(1, true);
            DoSomething(b: true, a: 1);
            WithOptional(1);
            WithOptional(1, ""Ipsum"");
            WithOptional(opt: ""Ipsum"", a: 1);
            WithParams();
            WithParams(1, 2, 3);
        }

        void ThisShouldNotBeFoundInMain()
        {
            SpecialMethod(65535);
        }

        void DoNothing()
        {
        }

        void DoSomething(int a, bool b)
        {
        }

        void WithOptional(int a, string opt = ""Lorem"")
        {
        }

        void WithParams(params int[] arr)
        {
        }

        void SpecialMethod(int specialParameter)
        {
        }
    }
}";
            var c = new CSharpInspection(Source);
            c.Inspect(0, "DoNothing", new { });
            c.Inspect(1, "DoSomething", new { a = 1, b = true });
            c.Inspect(2, "DoSomething", new { a = 1, b = true });
            c.Inspect(3, "WithOptional", new { a = 1 });
            c.Inspect(4, "WithOptional", new { a = 1, opt = "Ipsum" });
            c.Inspect(5, "WithOptional", new { a = 1, opt = "Ipsum" });
            c.Inspect(6, "WithParams", new { });

            //ToDo: TryGetSymbolParameter doesn't work for Params. Test look like this
            //c.Inspect(7, "WithParams", new { arr = new[] { 1, 2, 3 } });

            c.MainInvocations.Length.Should().Be(8); //Self-Test of this test. If new Invocation is added to the Main(), this number has to be updated and test should be written for that case.
        }
        
        [TestMethod]
        public void TestMethodParameterLookup_VB()
        {
            const string Source = @"
Module MainModule

    Sub Main()
        DoNothing()
        DoSomething(1, True)
        WithOptional(1)
        WithOptional(1, ""Ipsum"")
        WithParams()
        WithParams(1, 2, 3)
    End Sub

    Sub ThisShouldNotBeFoundInMain()
        SpecialMethod(65535)
    End Sub

    Sub DoNothing()
    End Sub

    Sub DoSomething(a As Integer, b As Boolean)
    End Sub

    Sub WithOptional(a As Integer, Optional opt As String = ""Lorem"")
    End Sub

    Sub WithParams(ParamArray arr() As Integer)
    End Sub

    Sub SpecialMethod(SpecialParameter As Integer)
    End Sub
End Module
";
            var c = new VisualBasicInspection(Source);
            c.Inspect(0, "DoNothing", new { });
            c.Inspect(1, "DoSomething", new { a = 1, b = true });
            c.Inspect(2, "WithOptional", new { a = 1 });
            c.Inspect(3, "WithOptional", new { a = 1, opt = "Ipsum" });
            c.Inspect(4, "WithParams", new { });
            //ToDo: TryGetSymbolParameter doesn't work for ParamArray. Test look like this
            //c.Inspect(5, "WithParams", new { arr = new[] { 1, 2, 3 } });

            c.MainInvocations.Length.Should().Be(6); //Self-Test of this test. If new Invocation is added to the Main(), this number has to be updated and test should be written for that case.
        }

        private abstract class InspectionBase<TArgumentSyntax, TInvocationSyntax>
            where TArgumentSyntax : SyntaxNode
            where TInvocationSyntax : SyntaxNode
        {
            public SnippetCompiler Compiler { get; protected set; }
            public TInvocationSyntax[] MainInvocations { get; protected set; }
            public TArgumentSyntax SpecialArgument { get; private set; }
            public IParameterSymbol SpecialParameter { get; private set; }

            public abstract TInvocationSyntax[] FindInvocationsIn(string name);
            public abstract object ExtractArgumentValue(TArgumentSyntax argumentSyntax);
            public abstract TArgumentSyntax[] GetArguments(TInvocationSyntax invocation);
            public abstract AbstractMethodParameterLookup<TArgumentSyntax> CreateLookup(TInvocationSyntax invocation, IMethodSymbol method);

            protected InspectionBase(string source, AnalyzerLanguage language)
            {
                this.Compiler = new SnippetCompiler(source, false, language);
                this.MainInvocations = FindInvocationsIn("Main");
            }

            protected void InitSpecial(TInvocationSyntax specialInvocation)
            {
                this.SpecialArgument = GetArguments(specialInvocation).Single();
                this.SpecialParameter = (Compiler.SemanticModel.GetSymbolInfo(specialInvocation).Symbol as IMethodSymbol).Parameters.Single();
            }

            public void Inspect(int invocationIndex, string expectedMethod, object expectedArguments)
            {
                var invocation = MainInvocations[invocationIndex];
                var method = Compiler.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                method.Name.Should().Be(expectedMethod);
                var lookup = CreateLookup(invocation, method);
                InspectTryGetSymbolParametr(lookup, expectedArguments, method);
                InspectTryGetParameterSymbol(lookup, expectedArguments, GetArguments(invocation));
            }

            private void InspectTryGetSymbolParametr(AbstractMethodParameterLookup<TArgumentSyntax> lookup, object expectedArguments, IMethodSymbol method)
            {
                lookup.TryGetSymbolParameter(SpecialParameter, out var symbol).Should().Be(false);

                foreach (var parameter in method.Parameters)
                {
                    if (lookup.TryGetSymbolParameter(parameter, out var argument))
                    {
                        ExtractArgumentValue(argument).Should().Be(ExtractExpectedValue(expectedArguments, parameter.Name));
                    }
                    else if (!parameter.IsOptional && !parameter.IsParams)
                    {
                        Assert.Fail($"TryGetSymbolParametr missing {parameter.Name}");
                    } //Else it's OK
                }
            }

            private void InspectTryGetParameterSymbol(AbstractMethodParameterLookup<TArgumentSyntax> lookup, object expectedArguments, TArgumentSyntax[] arguments)
            {
                lookup.TryGetParameterSymbol(SpecialArgument, out var parameter).Should().Be(false);

                foreach (var argument in arguments)
                {
                    if (lookup.TryGetParameterSymbol(argument, out var symbol))
                    {
                        ExtractArgumentValue(argument).Should().Be(ExtractExpectedValue(expectedArguments, symbol.Name));
                    }
                    else
                    {
                        Assert.Fail($"TryGetParameterSymbol missing {argument.ToString()}");
                    }
                }
            }

            private object ExtractExpectedValue(object expected, string name)
            {
                var pi = expected.GetType().GetProperty(name);
                if (pi == null)
                {
                    Assert.Fail($"Parameter name {name} was not expected.");
                }
                return pi.GetValue(expected, null);
            }

        }

        private class CSharpInspection : InspectionBase<CSharpSyntax.ArgumentSyntax, CSharpSyntax.InvocationExpressionSyntax>
        {

            public CSharpInspection(string source) : base(source, AnalyzerLanguage.CSharp)
            {
                InitSpecial(Compiler.GetNodes<CSharpSyntax.InvocationExpressionSyntax>().Single(x => x.Expression is CSharpSyntax.IdentifierNameSyntax identifier && identifier.Identifier.ValueText == "SpecialMethod"));
            }

            public override CSharpSyntax.InvocationExpressionSyntax[] FindInvocationsIn(string name)
            {
                return Compiler.GetNodes<CSharpSyntax.MethodDeclarationSyntax>().Single(x => x.Identifier.ValueText == name).DescendantNodes().OfType<CSharpSyntax.InvocationExpressionSyntax>().ToArray();
            }

            public override CSharpSyntax.ArgumentSyntax[] GetArguments(CSharpSyntax.InvocationExpressionSyntax invocation)
            {
                return invocation.ArgumentList.Arguments.ToArray();
            }

            public override object ExtractArgumentValue(CSharpSyntax.ArgumentSyntax argumentSyntax)
            {
                return Compiler.SemanticModel.GetConstantValue(argumentSyntax.Expression).Value;
            }

            public override AbstractMethodParameterLookup<CSharpSyntax.ArgumentSyntax> CreateLookup(CSharpSyntax.InvocationExpressionSyntax invocation, IMethodSymbol method)
            {
                return new CSharpMethodParameterLookup(invocation.ArgumentList, method);
            }

        }

        private class VisualBasicInspection : InspectionBase<VBSyntax.ArgumentSyntax, VBSyntax.InvocationExpressionSyntax>
        {

            public VisualBasicInspection(string source) : base(source, AnalyzerLanguage.VisualBasic)
            {
                InitSpecial(Compiler.GetNodes<VBSyntax.InvocationExpressionSyntax>().Single(x => x.Expression is VBSyntax.IdentifierNameSyntax identifier && identifier.Identifier.ValueText == "SpecialMethod"));
            }

            public override VBSyntax.InvocationExpressionSyntax[] FindInvocationsIn(string name)
            {
                return Compiler.GetNodes<VBSyntax.MethodBlockSyntax>().Single(x => x.SubOrFunctionStatement.Identifier.ValueText == "Main").DescendantNodes().OfType<VBSyntax.InvocationExpressionSyntax>().ToArray();
            }

            public override VBSyntax.ArgumentSyntax[] GetArguments(VBSyntax.InvocationExpressionSyntax invocation)
            {
                return invocation.ArgumentList.Arguments.ToArray();
            }

            public override AbstractMethodParameterLookup<VBSyntax.ArgumentSyntax> CreateLookup(VBSyntax.InvocationExpressionSyntax invocation, IMethodSymbol method)
            {
                return new VisualBasicMethodParameterLookup(invocation.ArgumentList, Compiler.SemanticModel);
            }

            public override object ExtractArgumentValue(VBSyntax.ArgumentSyntax argumentSyntax)
            {
                return Compiler.SemanticModel.GetConstantValue(argumentSyntax.GetExpression()).Value;
            }

        }
    }
}

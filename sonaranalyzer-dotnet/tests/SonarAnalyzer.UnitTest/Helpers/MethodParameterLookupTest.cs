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

        private const string Source_CS = @"
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

        [TestMethod]
        public void TestMethodParameterLookup_CS()
        {
            var c = new CSharpInspection(Source_CS);
            c.Inspect(0, "DoNothing", new { });
            c.Inspect(1, "DoSomething", new { a = 1, b = true });
            c.Inspect(2, "DoSomething", new { a = 1, b = true });
            c.Inspect(3, "WithOptional", new { a = 1 });
            c.Inspect(4, "WithOptional", new { a = 1, b = "Ipsum" });
            c.Inspect(5, "WithOptional", new { a = 1, opt = "Ipsum" });
            c.Inspect(6, "WithParams", new { });
            //False-Postive, AbstractMethodParameterLookup doesn't work for Params
            c.Inspect(7, "WithParams", new { arr = 1 });
            //It should be like this:
            //c.Inspect(7, "WithParams", new { arr = new[] { 1, 2, 3 } });
            c.MainInvocations.Length.Should().Be(8); //Self-Test of this test. If new Invocation is added to the Main(), this number has to be updated and test should be written for that case.
        }



        //        [TestMethod]
        //        public void TestMethodParameterLookup_VB()
        //        {
        //            const string Source = @"
        //Module MainModule

        //    Sub Main()
        //        Special(65535)
        //        'DoNothing()
        //        'DoSomething(42, True, Nothing)
        //        'DoSomething(42, True, Nothing, ""Ipsum"")
        //        'WithParamArray(42)
        //        WithParamArray(1, 2, 3)
        //    End Sub

        //    Sub DoSomething(a As Integer, b As Boolean, c As Object, Optional d As String = ""Lorem"")
        //    End Sub

        //    Sub WithParamArray(ParamArray arr() As Integer)
        //    End Sub

        //    'Sub WithParamArray(a As Integer, ParamArray arr() As Integer)
        //    'End Sub

        //    Sub DoNothing()
        //    End Sub

        //    Sub Special(SpecialArg As Integer)
        //    End Sub

        //End Module
        //";
        //            var compiler = new SnippetCompiler(Source, false, AnalyzerLanguage.VisualBasic);
        //            var mainMethod = compiler.GetNodes<VBSyntax.MethodBlockSyntax>().Single(x => x.SubOrFunctionStatement.Identifier.ValueText == "Main");
        //            var specialInvocation = compiler.GetNodes<VBSyntax.InvocationExpressionSyntax>().Single(x => x.Expression is VBSyntax.IdentifierNameSyntax identifier && identifier.Identifier.ValueText == "Special");
        //            var specialArgument = specialInvocation.ArgumentList.Arguments.Single();
        //            var specialParameter = (compiler.SemanticModel.GetSymbolInfo(specialInvocation).Symbol as IMethodSymbol).Parameters.Single();
        //            Assert.IsNotNull(mainMethod);

        //            foreach (var invocation in mainMethod.DescendantNodes().OfType<VBSyntax.InvocationExpressionSyntax>())
        //            {
        //                var method = compiler.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        //                var lookup = new VisualBasicMethodParameterLookup(invocation.ArgumentList, compiler.SemanticModel);
        //                var context = new VisualBasicInspectionContext();
        //                context.Init(compiler, lookup, method, invocation.ArgumentList.Arguments.ToArray(), specialArgument, specialParameter);
        //                InspectLookupValues(context);
        //            }

        //        }




        private abstract class InspectionBase<TArgumentSyntax, TInvocationSyntax>
            where TArgumentSyntax : SyntaxNode
            where TInvocationSyntax : SyntaxNode
        {
            public SnippetCompiler Compiler { get; protected set; }
            public TInvocationSyntax[] MainInvocations;
            public TArgumentSyntax SpecialArgument { get; private set; }
            public IParameterSymbol SpecialParameter { get; private set; }

            public abstract object ExtractArgumentValue(TArgumentSyntax argumentSyntax);
            public abstract TArgumentSyntax[] GetArguments(TInvocationSyntax invocation);
            public abstract AbstractMethodParameterLookup<TArgumentSyntax> CreateLookup(TInvocationSyntax invocation, IMethodSymbol method);
            
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

            public CSharpInspection(string source)
            {
                this.Compiler = new SnippetCompiler(source, false, AnalyzerLanguage.CSharp);
                this.MainInvocations = Compiler.GetNodes<CSharpSyntax.MethodDeclarationSyntax>().Single(x => x.Identifier.ValueText == "Main").DescendantNodes().OfType<CSharpSyntax.InvocationExpressionSyntax>().ToArray();
                InitSpecial(Compiler.GetNodes<CSharpSyntax.InvocationExpressionSyntax>().Single(x => x.Expression is CSharpSyntax.IdentifierNameSyntax identifier && identifier.Identifier.ValueText == "SpecialMethod"));
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

        //private class VisualBasicInspectionContext : InspectionContextBase<VBSyntax.ArgumentSyntax>
        //{

        //    public override object ExtractArgumentValue(VBSyntax.ArgumentSyntax argumentSyntax)
        //    {
        //        return Compiler.SemanticModel.GetConstantValue(argumentSyntax.GetExpression()).Value;
        //    }
        //}
    }
}

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
            Special(65535);
            DoNothing();
            DoSomething(42, true, null);
            DoSomething(42, true, null, ""Ipsum"");
            DoSomething(b: true, a: 42, c: null);
            DoSomething(b: true, a: 42, c: null, d: ""Ipsum"");
        }

        void DoSomething(int a, bool b, object c, string d = ""Lorem"")
        {
            if (c != null)
            {
                Console.WriteLine($""{a}#: {b}: {d}"");
            }
        }

        void DoNothing() 
        {
            Console.WriteLine(""Nothing"");
        }

        void Special(int special){
        }

    }
}";
            var compiler = new SnippetCompiler(Source, ignoreErrors: true, language: AnalyzerLanguage.CSharp);
            var mainMethod = compiler.GetNodes<CSharpSyntax.MethodDeclarationSyntax>().Single(x => x.Identifier.ValueText == "Main");
            var specialInvocation = compiler.GetNodes<CSharpSyntax.InvocationExpressionSyntax>().Single(x => x.Expression is CSharpSyntax.IdentifierNameSyntax identifier && identifier.Identifier.ValueText == "Special");
            var specialArgument = specialInvocation.ArgumentList.Arguments.Single();
            var specialParameter = (compiler.SemanticModel.GetSymbolInfo(specialInvocation).Symbol as IMethodSymbol).Parameters.Single();
            Assert.IsNotNull(mainMethod);   

            foreach (var invocation in mainMethod.DescendantNodes().OfType<CSharpSyntax.InvocationExpressionSyntax>())
            {
                var method = compiler.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                var lookup = new CSharpMethodParameterLookup(invocation.ArgumentList, method);
                var context = new CSharpInspectionContext();
                context.Init(compiler, lookup, method, invocation.ArgumentList.Arguments.ToArray(), specialArgument, specialParameter);
                InspectLookupValues(context);
            }
        }

        [TestMethod]
        public void TestMethodParameterLookup_VB()
        {
            const string Source = @"
Module MainModule

    Sub Main()
        Special(65535)
        DoNothing()
        DoSomething(42, True, Nothing)
        DoSomething(42, True, Nothing, ""Ipsum"")
    End Sub

    Sub DoSomething(a As Integer, b As Boolean, c As Object, Optional d As String = ""Lorem"")
        If c IsNot Nothing Then
            Console.WriteLine($""{a}#: {b}: {d}"")
        End If
    End Sub

    Sub DoNothing()
        Console.WriteLine(""Nothing"")
    End Sub

    Sub Special(SpecialArg As Integer)
    End Sub

End Module
";
            var compiler = new SnippetCompiler(Source, ignoreErrors: true, language: AnalyzerLanguage.VisualBasic);
            var mainMethod = compiler.GetNodes<VBSyntax.MethodStatementSyntax>().Single(x => x.Identifier.ValueText == "Main");
            var specialInvocation = compiler.GetNodes<VBSyntax.InvocationExpressionSyntax>().Single(x => x.Expression is VBSyntax.IdentifierNameSyntax identifier && identifier.Identifier.ValueText == "Special");
            var specialArgument = specialInvocation.ArgumentList.Arguments.Single();
            var specialParameter = (compiler.SemanticModel.GetSymbolInfo(specialInvocation).Symbol as IMethodSymbol).Parameters.Single();
            Assert.IsNotNull(mainMethod);

            foreach (var invocation in mainMethod.DescendantNodes().OfType<VBSyntax.InvocationExpressionSyntax>())
            {
                var method = compiler.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                var lookup = new VisualBasicMethodParameterLookup(invocation.ArgumentList, compiler.SemanticModel);
                var context = new VisualBasicInspectionContext();
                context.Init(compiler, lookup, method, invocation.ArgumentList.Arguments.ToArray(), specialArgument, specialParameter);
                InspectLookupValues(context);
            }

        }


        private void InspectLookupValues<TArgumentSyntax>(InspectionContextBase<TArgumentSyntax> context) where TArgumentSyntax : SyntaxNode
        {
            if (context.Method.Name == "Special")
            {
                //Do not inspect this one. It's only for providing nonexisting parameters for DoSomething
            }
            else if (context.Method.Name == "DoNothing")
            {
                context.Lookup.GetAllArgumentParameterMappings().Count().Should().Be(0);
            }
            else    //DoSomething
            {
                context.Lookup.GetAllArgumentParameterMappings().Count().Should().BeGreaterOrEqualTo(3);
                TryGetSymbolParametrTests(context);
                TryGetParameterSymbolTests(context);
            }
        }

        private void TryGetSymbolParametrTests<TArgumentSyntax>(InspectionContextBase<TArgumentSyntax> context) where TArgumentSyntax : SyntaxNode
        {
            context.Lookup.TryGetSymbolParameter(context.SpecialParameter, out var symbol).Should().Be(false);
            foreach (var parameter in context.Method.Parameters)
            {
                if (parameter.IsOptional)
                {
                    parameter.Name.Should().Be("d");
                    if (context.Lookup.TryGetSymbolParameter(parameter, out var argument))
                    {
                        context.ExtractArgumentValue(argument).Should().Be(ExpectedValue(parameter.Name));
                    }
                    else
                    {
                        context.Arguments.Length.Should().Be(3);    //Ommited optional argument should not be found.
                    }
                }
                else
                {
                    if (context.Lookup.TryGetSymbolParameter(parameter, out var argument))
                    {
                        context.ExtractArgumentValue(argument).Should().Be(ExpectedValue(parameter.Name));
                    }
                    else
                    {
                        Assert.Fail($"TryGetSymbolParametr missing {parameter.Name}");
                    }
                }
            }
        }

        private void TryGetParameterSymbolTests<TArgumentSyntax>(InspectionContextBase<TArgumentSyntax> context) where TArgumentSyntax : SyntaxNode
        {
            context.Lookup.TryGetParameterSymbol(context.SpecialArgument, out var parameter).Should().Be(false);

            foreach (var argument in context.Arguments)
            {
                if (context.Lookup.TryGetParameterSymbol(argument, out var symbol))
                {
                    context.ExtractArgumentValue(argument).Should().Be(ExpectedValue(symbol.Name));
                }
                else
                {
                    Assert.Fail($"TryGetParameterSymbol missing {argument.ToString()}");
                }
            }
        }

        private object ExpectedValue(string parameterName)
        {
            switch (parameterName)
            {
                case "a":
                    return 42;
                case "b":
                    return true;
                case "c":
                    return null;
                case "d":
                    return "Ipsum";
                default:
                    throw new System.Exception("Unexpected parameter in test method: " + parameterName);
            }
        }

        private abstract class InspectionContextBase<TArgumentSyntax> where TArgumentSyntax : SyntaxNode
        {
            public SnippetCompiler Compiler { get; private set; }
            public AbstractMethodParameterLookup<TArgumentSyntax> Lookup { get; private set; }
            public IMethodSymbol Method { get; private set; }
            public TArgumentSyntax[] Arguments { get; private set; }
            public TArgumentSyntax SpecialArgument { get; private set; }
            public IParameterSymbol SpecialParameter { get; private set; }

            public void Init(SnippetCompiler compiler, AbstractMethodParameterLookup<TArgumentSyntax> lookup, IMethodSymbol method, TArgumentSyntax[] arguments, TArgumentSyntax specialArgument, IParameterSymbol specialParameter)
            {
                this.Compiler = compiler;
                this.Lookup = lookup;
                this.Method = method;
                this.Arguments = arguments;
                this.SpecialArgument = specialArgument;
                this.SpecialParameter = specialParameter;
            }

            public abstract object ExtractArgumentValue(TArgumentSyntax argumentSyntax);
        }

        private class CSharpInspectionContext : InspectionContextBase<CSharpSyntax.ArgumentSyntax>
        {

            public override object ExtractArgumentValue(CSharpSyntax.ArgumentSyntax argumentSyntax)
            {
                return Compiler.SemanticModel.GetConstantValue(argumentSyntax.Expression).Value;
            }

        }

        private class VisualBasicInspectionContext : InspectionContextBase<VBSyntax.ArgumentSyntax>
        {

            public override object ExtractArgumentValue(VBSyntax.ArgumentSyntax argumentSyntax)
            {
                return Compiler.SemanticModel.GetConstantValue(argumentSyntax).Value;
            }
        }
    }
}

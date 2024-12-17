/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Collections;
using SonarAnalyzer.Core.Syntax.Utilities;
using SonarAnalyzer.CSharp.Core.Syntax.Utilities;
using SonarAnalyzer.VisualBasic.Core.Syntax.Utilities;
using CSharpCodeAnalysis = Microsoft.CodeAnalysis.CSharp;
using CSharpSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;
using VBCodeAnalysis = Microsoft.CodeAnalysis.VisualBasic;
using VBSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.Test.Syntax.Utilities;

[TestClass]
public class MethodParameterLookupTest
{
    private const string SourceCS = """
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
                        WithOptional(1, "Ipsum");
                        WithOptional(opt: "Ipsum", a: 1);
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

                    void WithOptional(int a, string opt = "Lorem")
                    {
                    }

                    void WithParams(params int[] arr)
                    {
                    }

                    void SpecialMethod(int specialParameter)
                    {
                    }
                }
            }
            """;

    private const string SourceVB = """
            Module MainModule

                Sub Main()
                    DoNothing()
                    DoSomething(1, True)
                    WithOptional(1)
                    WithOptional(1, "Ipsum")
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

                Sub WithOptional(a As Integer, Optional opt As String = "Lorem")
                End Sub

                Sub WithParams(ParamArray arr() As Integer)
                End Sub

                Sub SpecialMethod(SpecialParameter As Integer)
                End Sub
            End Module
            """;

    [TestMethod]
    public void TestMethodParameterLookup_CS()
    {
        var c = new CSharpInspection(SourceCS);
        c.CheckExpectedParameterMappings(0, "DoNothing", new { });
        c.CheckExpectedParameterMappings(1, "DoSomething", new { a = 1, b = true });
        c.CheckExpectedParameterMappings(2, "DoSomething", new { a = 1, b = true });
        c.CheckExpectedParameterMappings(3, "WithOptional", new { a = 1 });
        c.CheckExpectedParameterMappings(4, "WithOptional", new { a = 1, opt = "Ipsum" });
        c.CheckExpectedParameterMappings(5, "WithOptional", new { a = 1, opt = "Ipsum" });
        c.CheckExpectedParameterMappings(6, "WithParams", new { });
        c.CheckExpectedParameterMappings(7, "WithParams", new { arr = new[] { 1, 2, 3 } });

        c.MainInvocations.Length.Should().Be(8); // Self-Test of this test. If new Invocation is added to the Main(), this number has to be updated and test should be written for that case.
    }

    [TestMethod]
    public void TestMethodParameterLookup_VB()
    {
        var c = new VisualBasicInspection(SourceVB);
        c.CheckExpectedParameterMappings(0, "DoNothing", new { });
        c.CheckExpectedParameterMappings(1, "DoSomething", new { a = 1, b = true });
        c.CheckExpectedParameterMappings(2, "WithOptional", new { a = 1 });
        c.CheckExpectedParameterMappings(3, "WithOptional", new { a = 1, opt = "Ipsum" });
        c.CheckExpectedParameterMappings(4, "WithParams", new { });
        c.CheckExpectedParameterMappings(5, "WithParams", new { arr = new[] { 1, 2, 3 } });

        c.MainInvocations.Length.Should().Be(6); // Self-Test of this test. If new Invocation is added to the Main(), this number has to be updated and test should be written for that case.
    }

    [TestMethod]
    public void TestMethodParameterLookup_CS_ThrowsException()
    {
        var c = new CSharpInspection(SourceCS);
        var lookupThrow = c.CreateLookup(1, "DoSomething");

        var invalidOperationEx = Assert.ThrowsException<InvalidOperationException>(() => lookupThrow.TryGetNonParamsSyntax(lookupThrow.MethodSymbol.Parameters.Single(), out var argument));
        invalidOperationEx.Message.Should().Be("Sequence contains more than one element");

        var argumentEx = Assert.ThrowsException<ArgumentException>(() =>
            lookupThrow.TryGetSymbol(CSharpCodeAnalysis.SyntaxFactory.LiteralExpression(CSharpCodeAnalysis.SyntaxKind.StringLiteralExpression), out var parameter));
        argumentEx.Message.Should().StartWith("argument must be of type Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax");
    }

    [TestMethod]
    public void TestMethodParameterLookup_VB_ThrowsException()
    {
        var c = new VisualBasicInspection(SourceVB);
        var lookupThrow = c.CreateLookup(1, "DoSomething");

        var invalidOperationEx = Assert.ThrowsException<InvalidOperationException>(() => lookupThrow.TryGetNonParamsSyntax(lookupThrow.MethodSymbol.Parameters.Single(), out var argument));
        invalidOperationEx.Message.Should().Be("Sequence contains more than one element");

        var argumentEx = Assert.ThrowsException<ArgumentException>(() =>
            lookupThrow.TryGetSymbol(VBCodeAnalysis.SyntaxFactory.StringLiteralExpression(VBCodeAnalysis.SyntaxFactory.StringLiteralToken(string.Empty, string.Empty)), out var parameter));
        argumentEx.Message.Should().StartWith("argument must be of type Microsoft.CodeAnalysis.VisualBasic.Syntax.ArgumentSyntax");
    }

    [TestMethod]
    public void TestMethodParameterLookup_CS_ThrowsException_NonParams()
    {
        var c = new CSharpInspection(SourceCS);
        var lookupThrow = c.CreateLookup(7, "WithParams");

        var invalidOperationEx = Assert.ThrowsException<InvalidOperationException>(() => lookupThrow.TryGetNonParamsSyntax(lookupThrow.MethodSymbol.Parameters.Single(), out var argument));
        invalidOperationEx.Message.Should().Be("Cannot call TryGetNonParamsSyntax on ParamArray/params parameters.");
    }

    [TestMethod]
    public void TestMethodParameterLookup_VB_ThrowsException_NonParams()
    {
        var c = new VisualBasicInspection(SourceVB);
        var lookupThrow = c.CreateLookup(5, "WithParams");

        var invalidOperationEx = Assert.ThrowsException<InvalidOperationException>(() => lookupThrow.TryGetNonParamsSyntax(lookupThrow.MethodSymbol.Parameters.Single(), out var argument));
        invalidOperationEx.Message.Should().Be("Cannot call TryGetNonParamsSyntax on ParamArray/params parameters.");
    }

    [TestMethod]
    public void TestMethodParameterLookup_CS_MultipleCandidates()
    {
        var source = """
            namespace Test
            {
                class TestClass
                {
                    void Main(dynamic d) =>
                        AmbiguousCall(d);

                    void AmbiguousCall(int p) { }

                    void AmbiguousCall(string p) { }
                }
            }
            """;
        var (tree, model) = TestCompiler.CompileCS(source);
        var lookup = new CSharpMethodParameterLookup(tree.GetRoot().DescendantNodes().OfType<CSharpSyntax.InvocationExpressionSyntax>().Single(), model);

        lookup.TryGetSyntax("p", out var expressions).Should().BeTrue();
        expressions.Should().BeEquivalentTo<object>(new[] { new { Identifier = new { ValueText = "d" } } });
    }

    [TestMethod]
    public void TestMethodParameterLookup_VB_MultipleCandidates()
    {
        var source = """
            Module Test
                Sub Main()
                    Overloaded(42, "")
                End Sub

                Sub Overloaded(a As Integer, b As Boolean)
                End Sub

                Sub Overloaded(a As Integer, b As Integer)
                End Sub
            End Module
            """;
        var (tree, model) = TestCompiler.CompileIgnoreErrorsVB(source);
        var lookup = new VisualBasicMethodParameterLookup(tree.GetRoot().DescendantNodes().OfType<VBSyntax.InvocationExpressionSyntax>().Single(), model);

        lookup.TryGetSyntax("a", out var expressions).Should().BeTrue();
        expressions.Should().BeEquivalentTo<object>(new[] { new { Token = new { ValueText = "42" } } });
    }

    [TestMethod]
    public void TestMethodParameterLookup_CS_MultipleCandidatesWithDifferentParameters()
    {
        var source = """
            namespace Test
            {
                class TestClass
                {
                    void Main(dynamic d) =>
                        AmbiguousCall(d, d);

                    void AmbiguousCall(int a, int b) { }

                    void AmbiguousCall(string b, string a) { }
                }
            }
            """;
        var (tree, model) = TestCompiler.CompileCS(source);
        var lookup = new CSharpMethodParameterLookup(tree.GetRoot().DescendantNodes().OfType<CSharpSyntax.InvocationExpressionSyntax>().Single(), model);

        lookup.TryGetSyntax("a", out var expressions).Should().BeFalse();
        expressions.Should().BeEmpty();
    }

    [TestMethod]
    public void TestMethodParameterLookup_VB_MultipleCandidatesWithDifferentParameters()
    {
        var source = """
            Module Test
                Sub Main()
                    Overloaded(42, "")
                End Sub

                Sub Overloaded(a As Integer, b As Integer)
                End Sub

                Sub Overloaded(b As Boolean, a As Boolean)
                End Sub
            End Module
            """;
        var (tree, model) = TestCompiler.CompileIgnoreErrorsVB(source);
        var lookup = new VisualBasicMethodParameterLookup(tree.GetRoot().DescendantNodes().OfType<VBSyntax.InvocationExpressionSyntax>().Single(), model);

        lookup.TryGetSyntax("a", out var expressions).Should().BeFalse();
        expressions.Should().BeEmpty();
    }

    [TestMethod]
    public void TestMethodParameterLookup_CS_UnknownMethod()
    {
        var source = """
            namespace Test
            {
                class TestClass
                {
                    void Main()
                    {
                        UnknownMethod(42);
                    }
                }
            }
            """;
        var (tree, model) = TestCompiler.CompileIgnoreErrorsCS(source);
        var lookup = new CSharpMethodParameterLookup(tree.GetRoot().DescendantNodes().OfType<CSharpSyntax.InvocationExpressionSyntax>().Single(), model);

        lookup.TryGetSyntax("p", out var expressions).Should().BeFalse();
        expressions.Should().BeEmpty();
    }

    [TestMethod]
    public void TestMethodParameterLookup_VB_UnknownMethod()
    {
        var source = """
            Module Test
                Sub Main()
                    UnknownMethod(42)
                End Sub
            End Module
            """;
        var (tree, model) = TestCompiler.CompileIgnoreErrorsVB(source);
        var lookup = new VisualBasicMethodParameterLookup(tree.GetRoot().DescendantNodes().OfType<VBSyntax.InvocationExpressionSyntax>().Single(), model);

        lookup.TryGetSyntax("a", out var expressions).Should().BeFalse();
        expressions.Should().BeEmpty();
    }

    private abstract class InspectionBase<TArgumentSyntax, TInvocationSyntax>
        where TArgumentSyntax : SyntaxNode
        where TInvocationSyntax : SyntaxNode
    {
        public abstract TInvocationSyntax[] FindInvocationsIn(string name);
        public abstract object ExtractArgumentValue(TArgumentSyntax argumentSyntax);
        public abstract TArgumentSyntax[] GetArguments(TInvocationSyntax invocation);
        public abstract MethodParameterLookupBase<TArgumentSyntax> CreateLookup(TInvocationSyntax invocation, IMethodSymbol method);

        public SnippetCompiler Compiler { get; protected set; }
        public TInvocationSyntax[] MainInvocations { get; protected set; }
        public TArgumentSyntax SpecialArgument { get; private set; }
        public IParameterSymbol SpecialParameter { get; private set; }

        protected InspectionBase(string source, AnalyzerLanguage language)
        {
            Compiler = new SnippetCompiler(source, false, language);
            MainInvocations = FindInvocationsIn("Main");
        }

        public MethodParameterLookupBase<TArgumentSyntax> CreateLookup(int invocationIndex, string expectedMethod)
        {
            var invocation = MainInvocations[invocationIndex];
            var method = Compiler.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            method.Name.Should().Be(expectedMethod);
            return CreateLookup(invocation, method);
        }

        public void CheckExpectedParameterMappings(int invocationIndex, string expectedMethod, object expectedArguments)
        {
            var lookup = CreateLookup(invocationIndex, expectedMethod);
            InspectTryGetSyntax(lookup, expectedArguments, lookup.MethodSymbol);
            InspectTryGetSymbol(lookup, expectedArguments, GetArguments(MainInvocations[invocationIndex]));
        }

        protected void InitSpecial(TInvocationSyntax specialInvocation)
        {
            SpecialArgument = GetArguments(specialInvocation).Single();
            SpecialParameter = (Compiler.SemanticModel.GetSymbolInfo(specialInvocation).Symbol as IMethodSymbol).Parameters.Single();
        }

        private void InspectTryGetSyntax(MethodParameterLookupBase<TArgumentSyntax> lookup, object expectedArguments, IMethodSymbol method)
        {
            lookup.TryGetSyntax(SpecialParameter, out var symbol).Should().Be(false);

            foreach (var parameter in method.Parameters)
            {
                if (parameter.IsParams && lookup.TryGetSyntax(parameter, out var expressions))
                {
                    var expected = ExtractExpectedValue(expectedArguments, parameter.Name).Should().BeAssignableTo<IEnumerable>().Subject.Cast<object>();
                    expressions.Select(x => ConstantValue(x)).Should().Equal(expected);
                }
                else if (!parameter.IsParams && lookup.TryGetNonParamsSyntax(parameter, out var expression))
                {
                    ConstantValue(expression).Should().Be(ExtractExpectedValue(expectedArguments, parameter.Name));
                }
                else if (!parameter.IsOptional && !parameter.IsParams)
                {
                    Assert.Fail($"TryGetSyntax missing {parameter.Name}");
                } // Else it's OK
            }
        }

        private void InspectTryGetSymbol(MethodParameterLookupBase<TArgumentSyntax> lookup, object expectedArguments, TArgumentSyntax[] arguments)
        {
            lookup.TryGetSymbol(SpecialArgument, out var parameter).Should().Be(false);

            foreach (var argument in arguments)
            {
                if (lookup.TryGetSymbol(argument, out var symbol))
                {
                    var value = ExtractArgumentValue(argument);
                    var expected = ExtractExpectedValue(expectedArguments, symbol.Name);
                    if (symbol.IsParams)
                    {
                        // Expected contains all values {1, 2, 3} for ParamArray/params, but foreach is probing one at a time
                        expected.Should().BeAssignableTo<IEnumerable>().Which.Cast<object>().Should().Contain(value);
                    }
                    else
                    {
                        value.Should().Be(expected);
                    }
                }
                else
                {
                    Assert.Fail($"TryGetParameterSymbol missing {argument.ToString()}");
                }
            }
        }

        private static object ExtractExpectedValue(object expected, string name)
        {
            var pi = expected.GetType().GetProperty(name);
            if (pi is null)
            {
                Assert.Fail($"Parameter name {name} was not expected.");
            }
            return pi.GetValue(expected, null);
        }

        private object ConstantValue(SyntaxNode node) =>
            Compiler.SemanticModel.GetConstantValue(node).Value;
    }

    private class CSharpInspection : InspectionBase<CSharpSyntax.ArgumentSyntax, CSharpSyntax.InvocationExpressionSyntax>
    {
        public CSharpInspection(string source) : base(source, AnalyzerLanguage.CSharp) =>
            InitSpecial(Compiler.GetNodes<CSharpSyntax.InvocationExpressionSyntax>()
                                .Single(x => x.Expression is CSharpSyntax.IdentifierNameSyntax identifier
                                             && identifier.Identifier.ValueText == "SpecialMethod"));

        public override CSharpSyntax.InvocationExpressionSyntax[] FindInvocationsIn(string name) =>
            Compiler.GetNodes<CSharpSyntax.MethodDeclarationSyntax>().Single(x => x.Identifier.ValueText == name).DescendantNodes().OfType<CSharpSyntax.InvocationExpressionSyntax>().ToArray();

        public override CSharpSyntax.ArgumentSyntax[] GetArguments(CSharpSyntax.InvocationExpressionSyntax invocation) =>
            invocation.ArgumentList.Arguments.ToArray();

        public override object ExtractArgumentValue(CSharpSyntax.ArgumentSyntax argumentSyntax) =>
            Compiler.SemanticModel.GetConstantValue(argumentSyntax.Expression).Value;

        public override MethodParameterLookupBase<CSharpSyntax.ArgumentSyntax> CreateLookup(CSharpSyntax.InvocationExpressionSyntax invocation, IMethodSymbol method) =>
            new CSharpMethodParameterLookup(invocation.ArgumentList, method);
    }

    private class VisualBasicInspection : InspectionBase<VBSyntax.ArgumentSyntax, VBSyntax.InvocationExpressionSyntax>
    {
        public VisualBasicInspection(string source) : base(source, AnalyzerLanguage.VisualBasic) =>
            InitSpecial(Compiler.GetNodes<VBSyntax.InvocationExpressionSyntax>()
                                .Single(x => x.Expression is VBSyntax.IdentifierNameSyntax identifier
                                             && identifier.Identifier.ValueText == "SpecialMethod"));

        public override VBSyntax.InvocationExpressionSyntax[] FindInvocationsIn(string name) =>
            Compiler.GetNodes<VBSyntax.MethodBlockSyntax>().Single(x => x.SubOrFunctionStatement.Identifier.ValueText == "Main")
                                                           .DescendantNodes().OfType<VBSyntax.InvocationExpressionSyntax>().ToArray();

        public override VBSyntax.ArgumentSyntax[] GetArguments(VBSyntax.InvocationExpressionSyntax invocation) =>
            invocation.ArgumentList.Arguments.ToArray();

        public override MethodParameterLookupBase<VBSyntax.ArgumentSyntax> CreateLookup(VBSyntax.InvocationExpressionSyntax invocation, IMethodSymbol method) =>
            new VisualBasicMethodParameterLookup(invocation.ArgumentList, Compiler.SemanticModel);

        public override object ExtractArgumentValue(VBSyntax.ArgumentSyntax argumentSyntax) =>
            Compiler.SemanticModel.GetConstantValue(argumentSyntax.GetExpression()).Value;
    }
}

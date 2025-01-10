/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.Core.Test.Semantics.Extensions;

[TestClass]
public class IMethodSymbolExtensionsTest
{
    internal const string TestInput = """
        using System.Linq;

        namespace NS
        {
          public static class Helper
          {
            public static void ToVoid(this int self){}
          }
          public class Class
          {
            public static void TestMethod()
            {
              new int[] { 0, 1, 2 }.Any();
              Enumerable.Any(new int[] { 0, 1, 2 });

              new int[] { 0, 1, 2 }.Clone();

              new int[] { 0, 1, 2 }.Cast<object>();

              1.ToVoid();
            }
          }
        }
        """;

    private SemanticModel model;
    private List<StatementSyntax> statements;

    [TestInitialize]
    public void Compile()
    {
        var compilation = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).AddSnippet(TestInput).GetCompilation();

        var tree = compilation.SyntaxTrees.First();
        model = compilation.GetSemanticModel(tree);
        statements = tree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(x => x.Identifier.ValueText == "TestMethod").Body
            .DescendantNodes()
            .OfType<StatementSyntax>().ToList();
    }

    [TestMethod]
    public void Symbol_IsExtensionOnIEnumerable()
    {
        GetMethodSymbolForIndex(3).IsExtensionOn(KnownType.System_Collections_IEnumerable)
            .Should().BeTrue();

        GetMethodSymbolForIndex(2).IsExtensionOn(KnownType.System_Collections_IEnumerable)
            .Should().BeFalse();
        GetMethodSymbolForIndex(1).IsExtensionOn(KnownType.System_Collections_IEnumerable)
            .Should().BeFalse();
    }

    [TestMethod]
    public void Symbol_IsExtensionOnGenericIEnumerable()
    {
        GetMethodSymbolForIndex(0).IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T)
            .Should().BeTrue();
        GetMethodSymbolForIndex(1).IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T)
            .Should().BeTrue();

        GetMethodSymbolForIndex(2).IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T)
            .Should().BeFalse();
        GetMethodSymbolForIndex(3).IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T)
            .Should().BeFalse();
    }

    [TestMethod]
    public void Symbol_IsExtensionOnInt()
    {
        GetMethodSymbolForIndex(4).IsExtensionOn(KnownType.System_Int32)
            .Should().BeTrue();
        GetMethodSymbolForIndex(2).IsExtensionOn(KnownType.System_Int32)
            .Should().BeFalse();
    }

    [TestMethod]
    public void IsAnyAttributeInOverridingChain_WhenMethodSymbolIsNull_ReturnsFalse() =>
        ((IMethodSymbol)null).IsAnyAttributeInOverridingChain().Should().BeFalse();

    [DataTestMethod]
    [DataRow(MethodKind.AnonymousFunction, "method")]
    [DataRow(MethodKind.BuiltinOperator, "operator")]
    [DataRow(MethodKind.Constructor, "constructor")]
    [DataRow(MethodKind.Conversion, "operator")]
    [DataRow(MethodKind.DeclareMethod, "method")]
    [DataRow(MethodKind.DelegateInvoke, "method")]
    [DataRow(MethodKind.Destructor, "destructor")]
    [DataRow(MethodKind.EventAdd, "method")]
    [DataRow(MethodKind.EventRaise, "method")]
    [DataRow(MethodKind.EventRemove, "method")]
    [DataRow(MethodKind.ExplicitInterfaceImplementation, "method")]
    [DataRow(MethodKind.FunctionPointerSignature, "method")]
    [DataRow(MethodKind.LambdaMethod, "method")]
    [DataRow(MethodKind.LocalFunction, "local function")]
    [DataRow(MethodKind.Ordinary, "method")]
    [DataRow(MethodKind.PropertyGet, "getter")]
    [DataRow(MethodKind.PropertySet, "setter")]
    [DataRow(MethodKind.ReducedExtension, "method")]
    [DataRow(MethodKind.SharedConstructor, "constructor")]
    [DataRow(MethodKind.StaticConstructor, "constructor")]
    [DataRow(MethodKind.UserDefinedOperator, "operator")]
    public void GetClassification_Method(MethodKind methodKind, string expected)
    {
        var symbol = Substitute.For<IMethodSymbol>();
        symbol.Kind.Returns(SymbolKind.Method);
        symbol.MethodKind.Returns(methodKind);

        symbol.GetClassification().Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("BaseClass<int>         ", "VirtualMethod               ", "InheritedAttribute", "DerivedInheritedAttribute", "DerivedNotInheritedAttribute", "UnannotatedAttribute", "NotInheritedAttribute")]
    [DataRow("DerivedOpenGeneric<int>", "VirtualMethod               ", "InheritedAttribute", "DerivedInheritedAttribute", "DerivedNotInheritedAttribute", "UnannotatedAttribute")]
    [DataRow("DerivedClosedGeneric   ", "VirtualMethod               ", "InheritedAttribute", "DerivedInheritedAttribute", "DerivedNotInheritedAttribute", "UnannotatedAttribute")]
    [DataRow("DerivedNoOverrides<int>", "VirtualMethod               ", "InheritedAttribute", "DerivedInheritedAttribute", "DerivedNotInheritedAttribute", "UnannotatedAttribute", "NotInheritedAttribute")]
    [DataRow("DerivedOpenGeneric<int>", "GenericVirtualMethod<int>   ", "InheritedAttribute", "DerivedInheritedAttribute", "DerivedNotInheritedAttribute", "UnannotatedAttribute")]
    [DataRow("DerivedClosedGeneric   ", "GenericVirtualMethod<int>   ", "InheritedAttribute", "DerivedInheritedAttribute", "DerivedNotInheritedAttribute", "UnannotatedAttribute")]
    [DataRow("DerivedNoOverrides<int>", "GenericVirtualMethod<int>   ", "InheritedAttribute", "DerivedInheritedAttribute", "DerivedNotInheritedAttribute", "UnannotatedAttribute", "NotInheritedAttribute")]
    [DataRow("DerivedOpenGeneric<int>", "NonVirtualMethod            ")]
    [DataRow("DerivedClosedGeneric   ", "NonVirtualMethod            ")]
    [DataRow("DerivedNoOverrides<int>", "NonVirtualMethod            ", "InheritedAttribute", "DerivedInheritedAttribute", "DerivedNotInheritedAttribute", "UnannotatedAttribute", "NotInheritedAttribute")]
    [DataRow("DerivedOpenGeneric<int>", "GenericNonVirtualMethod<int>")]
    [DataRow("DerivedClosedGeneric   ", "GenericNonVirtualMethod<int>")]
    [DataRow("DerivedNoOverrides<int>", "GenericNonVirtualMethod<int>", "InheritedAttribute", "DerivedInheritedAttribute", "DerivedNotInheritedAttribute", "UnannotatedAttribute", "NotInheritedAttribute")]
    public void GetAttributesWithInherited_MethodSymbol(string className, string methodName, params string[] expectedAttributes)
    {
        className = className.TrimEnd();
        methodName = methodName.TrimEnd();
        var code = $$"""
            using System;

            [AttributeUsage(AttributeTargets.All, Inherited = true)]
            public class InheritedAttribute : Attribute { }

            [AttributeUsage(AttributeTargets.All, Inherited = false)]
            public class NotInheritedAttribute : Attribute { }

            public class DerivedInheritedAttribute: InheritedAttribute { }

            public class DerivedNotInheritedAttribute: NotInheritedAttribute { }

            public class UnannotatedAttribute : Attribute { }

            public class BaseClass<T1>
            {
                [Inherited]
                [DerivedInherited]
                [NotInherited]
                [DerivedNotInherited]
                [Unannotated]
                public virtual void VirtualMethod() { }

                [Inherited]
                [DerivedInherited]
                [NotInherited]
                [DerivedNotInherited]
                [Unannotated]
                public void NonVirtualMethod() { }

                [Inherited]
                [DerivedInherited]
                [NotInherited]
                [DerivedNotInherited]
                [Unannotated]
                public void GenericNonVirtualMethod<T2>() { }

                [Inherited]
                [DerivedInherited]
                [NotInherited]
                [DerivedNotInherited]
                [Unannotated]
                public virtual void GenericVirtualMethod<T2>() { }
            }

            public class DerivedOpenGeneric<T1>: BaseClass<T1>
            {
                public override void VirtualMethod() { }
                public new void NonVirtualMethod() { }
                public new void GenericNonVirtualMethod<T2>() { }
                public override void GenericVirtualMethod<T2>() { }
            }

            public class DerivedClosedGeneric: BaseClass<int>
            {
                public override void VirtualMethod() { }
                public new void NonVirtualMethod() { }
                public new void GenericNonVirtualMethod<T2>() { }
                public override void GenericVirtualMethod<T2>() { }
            }

            public class DerivedNoOverrides<T>: BaseClass<T> { }

            public class Program
            {
                public static void Main()
                {
                    new {{className}}().{{methodName}}();
                }
            }
            """;
        var compiler = new SnippetCompiler(code);
        var invocationExpression = compiler.GetNodes<InvocationExpressionSyntax>().Should().ContainSingle().Subject;
        var method = compiler.GetSymbol<IMethodSymbol>(invocationExpression);
        var actual = method.GetAttributesWithInherited().Select(x => x.AttributeClass.Name).ToList();
        actual.Should().BeEquivalentTo(expectedAttributes);

        // GetAttributesWithInherited should behave like MemberInfo.GetCustomAttributes from runtime reflection:
        var type = compiler.EmitAssembly().GetType(className.Replace("<int>", "`1"), throwOnError: true);
        var methodInfo = type.GetMethod(methodName.Replace("<int>", string.Empty));
        methodInfo.GetCustomAttributes(inherit: true).Select(x => x.GetType().Name).Should().BeEquivalentTo(expectedAttributes);
    }

    private IMethodSymbol GetMethodSymbolForIndex(int index)
    {
        var statement = (ExpressionStatementSyntax)statements[index];
        var methodSymbol = model.GetSymbolInfo(statement.Expression).Symbol as IMethodSymbol;
        return methodSymbol;
    }
}

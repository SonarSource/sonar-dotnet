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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
    [DataRow("3.0.20105.1")]
    [DataRow(TestConstants.NuGetLatestVersion)]
    public void IsControllerActionMethod_PublicControllerMethods_AreEntryPoints(string aspNetMvcVersion)
    {
        const string code = """
            public abstract class Foo : System.Web.Mvc.Controller
            {
                public Foo() { }
                public void PublicFoo() { }
                protected void ProtectedFoo() { }
                internal void InternalFoo() { }
                private void PrivateFoo() { }
                public static void StaticFoo() { }
                public virtual void VirtualFoo() { }
                public abstract void AbstractFoo();
                public void InFoo(in string arg) { }
                public void OutFoo(out string arg) { arg = null; }
                public void RefFoo(ref string arg) { }
                public void ReadonlyRefFoo(ref readonly string arg) { }
                public void GenericFoo<T>(T arg) { }
                private class Bar : System.Web.Mvc.Controller
                {
                    public void InnerFoo() { }
                }
                [System.Web.Mvc.NonActionAttribute]
                public void PublicNonAction() { }
            }
            """;
        var compilation = new SnippetCompiler(code, NuGetMetadataReference.MicrosoftAspNetMvc(aspNetMvcVersion));
        compilation.GetTypeByMetadataName("Foo").Constructors[0].IsControllerActionMethod().Should().BeFalse();
        compilation.GetMethodSymbol("Foo.PublicFoo").IsControllerActionMethod().Should().BeTrue();
        compilation.GetMethodSymbol("Foo.ProtectedFoo").IsControllerActionMethod().Should().BeFalse();
        compilation.GetMethodSymbol("Foo.InternalFoo").IsControllerActionMethod().Should().BeFalse();
        compilation.GetMethodSymbol("Foo.PrivateFoo").IsControllerActionMethod().Should().BeFalse();
        compilation.GetMethodSymbol("Foo.StaticFoo").IsControllerActionMethod().Should().BeFalse();
        compilation.GetMethodSymbol("Foo.VirtualFoo").IsControllerActionMethod().Should().BeTrue();
        compilation.GetMethodSymbol("Foo.AbstractFoo").IsControllerActionMethod().Should().BeTrue();
        compilation.GetMethodSymbol("Foo.InFoo").IsControllerActionMethod().Should().BeFalse();
        compilation.GetMethodSymbol("Foo.OutFoo").IsControllerActionMethod().Should().BeFalse();
        compilation.GetMethodSymbol("Foo.ReadonlyRefFoo").IsControllerActionMethod().Should().BeFalse();
        compilation.GetMethodSymbol("Foo.RefFoo").IsControllerActionMethod().Should().BeFalse();
        compilation.GetMethodSymbol("Foo.GenericFoo").IsControllerActionMethod().Should().BeFalse();
        compilation.GetMethodSymbol("Foo.InnerFoo").IsControllerActionMethod().Should().BeFalse();
        compilation.GetMethodSymbol("Foo.PublicNonAction").IsControllerActionMethod().Should().BeFalse();
    }

    [TestMethod]
    [DataRow("3.0.20105.1")]
    [DataRow(TestConstants.NuGetLatestVersion)]
    public void IsControllerActionMethod_ControllerMethods_AreEntryPoints(string aspNetMvcVersion)
    {
        const string code = """
            public class Foo : System.Web.Mvc.Controller
            {
                public void PublicFoo() { }
                [System.Web.Mvc.NonActionAttribute]
                public void PublicNonAction() { }
            }
            public class Controller
            {
                public void PublicBar() { }
            }
            public class MyController : Controller
            {
                public void PublicDiz() { }
            }
            """;
        var compilation = new SnippetCompiler(code, NuGetMetadataReference.MicrosoftAspNetMvc(aspNetMvcVersion));
        compilation.GetMethodSymbol("Foo.PublicFoo").IsControllerActionMethod().Should().BeTrue();
        compilation.GetMethodSymbol("Controller.PublicBar").IsControllerActionMethod().Should().BeFalse();
        compilation.GetMethodSymbol("MyController.PublicDiz").IsControllerActionMethod().Should().BeFalse();
        compilation.GetMethodSymbol("Foo.PublicNonAction").IsControllerActionMethod().Should().BeFalse();
    }

    [TestMethod]
    [DataRow("2.1.3")]
    [DataRow(TestConstants.NuGetLatestVersion)]
    public void IsControllerActionMethod_MethodsInClassesWithControllerAttribute_AreEntryPoints(string aspNetMvcVersion)
    {
        const string code = """
            [Microsoft.AspNetCore.Mvc.ControllerAttribute]
            public class Foo
            {
                public void PublicFoo() { }
                [Microsoft.AspNetCore.Mvc.NonActionAttribute]
                public void PublicNonAction() { }
            }
            """;
        var compilation = new SnippetCompiler(code, MetadataReferenceFacade.NetStandard.Union(NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(aspNetMvcVersion)));
        compilation.GetMethodSymbol("Foo.PublicFoo").IsControllerActionMethod().Should().BeTrue();
        compilation.GetMethodSymbol("Foo.PublicNonAction").IsControllerActionMethod().Should().BeFalse();
    }

    [TestMethod]
    [DataRow("2.1.3")]
    [DataRow(TestConstants.NuGetLatestVersion)]
    public void IsControllerActionMethod_MethodsInClassesWithNonControllerAttribute_AreNotEntryPoints(string aspNetMvcVersion)
    {
        const string code = """
            [Microsoft.AspNetCore.Mvc.NonControllerAttribute]
            public class Foo : Microsoft.AspNetCore.Mvc.ControllerBase
            {
                public void PublicFoo() { }
            }
            """;
        var compilation = new SnippetCompiler(code, MetadataReferenceFacade.NetStandard.Union(NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(aspNetMvcVersion)));
        compilation.GetMethodSymbol("Foo.PublicFoo").IsControllerActionMethod().Should().BeFalse();
    }

    [TestMethod]
    [DataRow("2.1.3")]
    [DataRow(TestConstants.NuGetLatestVersion)]
    public void IsControllerActionMethod_ConstructorsInClasses_AreNotEntryPoints(string aspNetMvcVersion)
    {
        const string code = """
            [Microsoft.AspNetCore.Mvc.ControllerAttribute]
            public class Foo : Microsoft.AspNetCore.Mvc.ControllerBase
            {
                public Foo() { }
            }
            """;
        var compilation = new SnippetCompiler(code, MetadataReferenceFacade.NetStandard.Union(NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(aspNetMvcVersion)));
        compilation.GetTypeByMetadataName("Foo").Constructors[0].IsControllerActionMethod().Should().BeFalse();
    }

    [TestMethod]
    [DataRow("List<int>", "Clear()", "Clear", "System.Collections.Generic.ICollection", "T")]
    [DataRow("List<int>", "Clear()", "Clear", "System.Collections.IList")]
    [DataRow("List<int>", "RemoveAt(1)", "RemoveAt", "System.Collections.Generic.IList", "T")]
    [DataRow("List<int>", "RemoveAt(1)", "RemoveAt", "System.Collections.IList")]
    [DataRow("ObservableCollection<int>", "RemoveAt(1)", "RemoveAt", "System.Collections.Generic.IList", "T")]
    [DataRow("ObservableCollection<int>", "RemoveAt(1)", "RemoveAt", "System.Collections.IList")]
    [DataRow("Derived1", "ToString(string.Empty, null)", "ToString", "System.IFormattable")]
    [DataRow("Derived2", "ToString(string.Empty, null)", "ToString", "System.IFormattable")]
    [DataRow("Derived3", "ToString(string.Empty, null)", "ToString", "System.IFormattable")]
    [DataRow("Derived4", "ToString(string.Empty, null)", "ToString", "System.IFormattable")]
    public void IsImplementingInterfaceMember_Methods(string declaration, string invocation, string methodName, string interfaceType, params string[] genericParameter)
    {
        var code = $$"""
            using System;
            using System.Collections.Generic;
            using System.Collections.ObjectModel;

            public class Test
            {
                public void Method({{declaration}} instance)
                {
                    instance.{{invocation}};
                }
            }

            public class Base
            {
                public virtual string ToString(string format, IFormatProvider formatProvider) => "";
            }

            public class Derived1: Base, IFormattable
            {
                public override string ToString(string format, IFormatProvider formatProvider) => "";
            }

            public class Derived2: Derived1
            {
                public override string ToString(string format, IFormatProvider formatProvider) => "";
            }

            public class Derived3: Derived2
            { }

            public class Derived4: Derived3
            {
                public override string ToString(string format, IFormatProvider formatProvider) => "";
            }
            """;
        var compilation = new SnippetCompiler(code);
        var invocationSyntax = compilation.SyntaxTree.GetRoot().DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().First();
        var methodSymbol = compilation.SemanticModel.GetSymbolInfo(invocationSyntax).Symbol as IMethodSymbol;
        methodSymbol.IsImplementingInterfaceMember(new KnownType(interfaceType, genericParameter), methodName).Should().BeTrue();
    }

    private IMethodSymbol GetMethodSymbolForIndex(int index)
    {
        var statement = (ExpressionStatementSyntax)statements[index];
        var methodSymbol = model.GetSymbolInfo(statement.Expression).Symbol as IMethodSymbol;
        return methodSymbol;
    }
}

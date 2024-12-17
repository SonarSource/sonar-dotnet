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

using Microsoft.CodeAnalysis.CSharp.Syntax;
using ISymbolExtensionsCommon = SonarAnalyzer.Core.Semantics.Extensions.ISymbolExtensions;

namespace SonarAnalyzer.Core.Test.Semantics.Extensions;

[TestClass]
public class ISymbolExtensionsTest
{
    private const string TestInput = """
        public interface IInterface
        {
            int Property2 { get; set; }
            void Method3();
        }

        public abstract class Base
        {
            public virtual void Method1() { }
            protected virtual void Method2() { }
            public abstract int Property { get; set; }

            public void Method4(){}
        }

        public class Derived1 : Base
        {
            public override int Property { get; set; }
            private int PrivateProperty { get; set; }
            private protected int PrivateProtectedProperty { get; set; }
            protected int ProtectedProperty { get; set; }
            protected internal int ProtectedInternalProperty { get; set; }
            internal int InternalProperty { get; set; }
        }

        public abstract class Derived2 : Base, IInterface
        {
            public override int Property { get; set; }
            public int Property2 { get; set; }
            public void Method3(){}

            public abstract void Method5();
        }
        """;

    private SnippetCompiler testSnippet;

    [TestInitialize]
    public void Compile() =>
        testSnippet = new SnippetCompiler(TestInput);

    [TestMethod]
    public void IsInType_Null_KnownType() =>
        ISymbolExtensionsCommon.IsInType(null, KnownType.System_Boolean).Should().BeFalse();

    [TestMethod]
    public void IsInType_Null_TypeSymbol() =>
        ISymbolExtensionsCommon.IsInType(null, (ITypeSymbol)null).Should().BeFalse();

    [TestMethod]
    public void IsInType_Null_ArrayOfTypeSymbols() =>
        ISymbolExtensionsCommon.IsInType(null, []).Should().BeFalse();

    [DataTestMethod]
    [DataRow("{ get; set; }")]
    [DataRow("{ get; }")]
    [DataRow("{ get; } = string.Empty;")]
    [DataRow("{ get; set; } = string.Empty;")]
#if NET
    [DataRow("{ get; init; }")]
#endif
    public void IsAutoProperty_AutoProperty_CS(string getterSetter)
    {
        var code = $$"""
            public class Sample
            {
                public string SymbolMember {{getterSetter}}
            }
            """;
        CreateSymbol(code, AnalyzerLanguage.CSharp).IsAutoProperty().Should().BeTrue();
    }

    [TestMethod]
    public void IsAutoProperty_AutoProperty_VB()
    {
        const string code = """
            Public Class Sample

                Public Property SymbolMember As String

            End Class
            """;
        CreateSymbol(code, AnalyzerLanguage.VisualBasic).IsAutoProperty().Should().BeTrue();
    }

    [TestMethod]
    public void IsAutoProperty_ExplicitProperty_CS()
    {
        const string code = """
            public class Sample
            {
                private string _SymbolMember; // Try to confuse the method with auto-like implementation

                public string SymbolMember
                {
                    get => _SymbolMember;
                    set { _SymbolMember = value; }
                }
            }
            """;
        CreateSymbol(code, AnalyzerLanguage.CSharp).IsAutoProperty().Should().BeFalse();
    }

    [TestMethod]
    public void IsAutoProperty_ExplicitProperty_VB()
    {
        const string code = """
            Public Class Sample

                Private _SymbolMember As String ' Try to confuse the method with auto-like implementation

                Public Property SymbolMember As String
                    Get
                        Return _SymbolMember
                    End Get
                    Set(value As String)
                        _SymbolMember = value
                    End Set
                End Property

            End Class
            """;
        CreateSymbol(code, AnalyzerLanguage.VisualBasic).IsAutoProperty().Should().BeFalse();
    }

    [TestMethod]
    public void IsAutoProperty_NonpropertySymbol_CS()
    {
        const string code = """
            public class Sample
            {
                public void SymbolMember() { }
            }
            """;
        CreateSymbol(code, AnalyzerLanguage.CSharp).IsAutoProperty().Should().BeFalse();
    }

    [TestMethod]
    public void IsAutoProperty_NonpropertySymbol_VB()
    {
        const string code = """
            Public Class Sample

                Public Sub SymbolMember()
                End Sub

            End Class
            """;
        CreateSymbol(code, AnalyzerLanguage.VisualBasic).IsAutoProperty().Should().BeFalse();
    }

    [TestMethod]
    public void Symbol_IsPublicApi()
    {
        testSnippet.GetMethodSymbol("Base.Method1").IsPubliclyAccessible().Should().BeTrue();
        testSnippet.GetMethodSymbol("Base.Method2").IsPubliclyAccessible().Should().BeTrue();
        testSnippet.GetPropertySymbol("Base.Property").IsPubliclyAccessible().Should().BeTrue();
        testSnippet.GetPropertySymbol("IInterface.Property2").IsPubliclyAccessible().Should().BeTrue();
        testSnippet.GetPropertySymbol("Derived1.PrivateProperty").IsPubliclyAccessible().Should().BeFalse();
        testSnippet.GetPropertySymbol("Derived1.PrivateProtectedProperty").IsPubliclyAccessible().Should().BeFalse();
        testSnippet.GetPropertySymbol("Derived1.ProtectedProperty").IsPubliclyAccessible().Should().BeTrue();
        testSnippet.GetPropertySymbol("Derived1.ProtectedInternalProperty").IsPubliclyAccessible().Should().BeTrue();
        testSnippet.GetPropertySymbol("Derived1.InternalProperty").IsPubliclyAccessible().Should().BeFalse();
    }

    [TestMethod]
    public void Symbol_IsInterfaceImplementationOrMemberOverride()
    {
        testSnippet.GetMethodSymbol("Base.Method1").GetInterfaceMember().Should().BeNull();
        testSnippet.GetMethodSymbol("Base.Method1").GetOverriddenMember().Should().BeNull();
        testSnippet.GetPropertySymbol("Derived2.Property").GetOverriddenMember().Should().NotBeNull();
        testSnippet.GetPropertySymbol("Derived2.Property2").GetInterfaceMember().Should().NotBeNull();
        testSnippet.GetMethodSymbol("Derived2.Method3").GetInterfaceMember().Should().NotBeNull();
    }

    [TestMethod]
    public void Symbol_TryGetOverriddenOrInterfaceMember()
    {
        var actualOverriddenMethod = testSnippet.GetMethodSymbol("Base.Method1").GetOverriddenMember();
        actualOverriddenMethod.Should().BeNull();

        var expectedOverriddenProperty = testSnippet.GetPropertySymbol("Base.Property");
        var propertySymbol = testSnippet.GetPropertySymbol("Derived2.Property");

        var actualOverriddenProperty = propertySymbol.GetOverriddenMember();
        actualOverriddenProperty.Should().NotBeNull();
        actualOverriddenProperty.Should().Be(expectedOverriddenProperty);

        var expectedOverriddenMethod = testSnippet.GetMethodSymbol("IInterface.Method3");
        actualOverriddenMethod = testSnippet.GetMethodSymbol("Derived2.Method3").GetInterfaceMember();
        actualOverriddenMethod.Should().NotBeNull();
        actualOverriddenMethod.Should().Be(expectedOverriddenMethod);
    }

    [TestMethod]
    public void Symbol_IsChangeable()
    {
        testSnippet.GetMethodSymbol("Base.Method1").IsChangeable().Should().BeFalse();
        testSnippet.GetMethodSymbol("Base.Method4").IsChangeable().Should().BeTrue();
        testSnippet.GetMethodSymbol("Derived2.Method5").IsChangeable().Should().BeFalse();
        testSnippet.GetMethodSymbol("Derived2.Method3").IsChangeable().Should().BeFalse();
    }

    [TestMethod]
    public void AnyAttributeDerivesFrom_WhenSymbolIsNull_ReturnsFalse() =>
        ISymbolExtensionsCommon.AnyAttributeDerivesFrom(null, KnownType.Void).Should().BeFalse();

    [TestMethod]
    public void AnyAttributeDerivesFromAny_WhenSymbolIsNull_ReturnsFalse() =>
        ISymbolExtensionsCommon.AnyAttributeDerivesFromAny(null, [KnownType.Void]).Should().BeFalse();

    [TestMethod]
    public void GetAttributesForKnownType_WhenSymbolIsNull_ReturnsEmpty() =>
        ISymbolExtensionsCommon.GetAttributes(null, KnownType.Void).Should().BeEmpty();

    [TestMethod]
    public void GetAttributesForKnownTypes_WhenSymbolIsNull_ReturnsEmpty() =>
        ISymbolExtensionsCommon.GetAttributes(null, [KnownType.Void]).Should().BeEmpty();

    [TestMethod]
    public void GetParameters_WhenSymbolIsNotMethodOrProperty_ReturnsEmpty()
    {
        var symbol = Substitute.For<ISymbol>();
        symbol.Kind.Returns(SymbolKind.Alias);
        symbol.GetParameters().Should().BeEmpty();
    }

    [TestMethod]
    public void GetInterfaceMember_WhenSymbolIsNull_ReturnsEmpty() =>
        ((ISymbol)null).GetInterfaceMember().Should().BeNull();

    [TestMethod]
    public void GetOverriddenMember_WhenSymbolIsNull_ReturnsEmpty() =>
        ((ISymbol)null).GetOverriddenMember().Should().BeNull();

    [TestMethod]
    public void GetEffectiveAccessibility_WhenSymbolIsNull_ReturnsNotApplicable() =>
        ISymbolExtensionsCommon.GetEffectiveAccessibility(null).Should().Be(Accessibility.NotApplicable);

    [DataTestMethod]
    [DataRow(SymbolKind.Alias, "alias")]
    [DataRow(SymbolKind.ArrayType, "array")]
    [DataRow(SymbolKind.Assembly, "assembly")]
    [DataRow(SymbolKind.Discard, "discard")]
    [DataRow(SymbolKind.DynamicType, "dynamic")]
    [DataRow(SymbolKind.ErrorType, "error")]
    [DataRow(SymbolKind.Event, "event")]
    [DataRow(SymbolKind.Field, "field")]
    [DataRow(SymbolKind.FunctionPointerType, "function pointer")]
    [DataRow(SymbolKind.Label, "label")]
    [DataRow(SymbolKind.Local, "local")]
    [DataRow(SymbolKind.Namespace, "namespace")]
    [DataRow(SymbolKind.NetModule, "netmodule")]
    [DataRow(SymbolKind.Parameter, "parameter")]
    [DataRow(SymbolKind.PointerType, "pointer")]
    [DataRow(SymbolKind.Preprocessing, "preprocessing")]
    [DataRow(SymbolKind.Property, "property")]
    [DataRow(SymbolKind.RangeVariable, "range variable")]
    [DataRow(SymbolKind.TypeParameter, "type parameter")]
    public void GetClassification_SimpleKinds(SymbolKind symbolKind, string expected)
    {
        var symbol = Substitute.For<ISymbol>();
        symbol.Kind.Returns(symbolKind);
        symbol.GetClassification().Should().Be(expected);
    }

    [TestMethod]
    public void GetClassification_UnknowKind()
    {
        var symbol = Substitute.For<ISymbol>();
        symbol.Kind.Returns((SymbolKind)999);
#if DEBUG
        new Action(() => symbol.GetClassification()).Should().Throw<NotSupportedException>();
#else
        ISymbolExtensionsCommon.GetClassification(symbol).Should().Be("symbol");
#endif
    }

    [TestMethod]
    public void AllPartialParts_MethodSymbol_NonPartialMethod()
    {
        const string code = """
            public partial class Sample
            {
                partial void SymbolMember();
            }
            """;
        var symbol = CreateSymbol(code, AnalyzerLanguage.CSharp);
        var methodSymbol = symbol as IMethodSymbol;

        var result = symbol.AllPartialParts().ToList();

        result.Should().ContainSingle().And.Subject.Should().Contain(methodSymbol);
    }

    [TestMethod]
    public void AllPartialParts_MethodSymbol_PartialMethodSameClass()
    {
        const string code = """
            public partial class Sample
            {
                partial void SymbolMember();
                partial void SymbolMember() { }
            }
            """;
        var symbols = CreateSymbols(code, AnalyzerLanguage.CSharp, x => x is MethodDeclarationSyntax);

        var declarationSymbol = symbols[0] as IMethodSymbol;
        var declarationResult = declarationSymbol.AllPartialParts().ToList();
        declarationResult.Should().HaveCount(2).And.Contain([declarationSymbol, declarationSymbol.PartialImplementationPart]);

        var implementationSymbol = symbols[1] as IMethodSymbol;
        var implementationResult = implementationSymbol.AllPartialParts().ToList();
        implementationResult.Should().HaveCount(2).And.Contain([implementationSymbol, implementationSymbol.PartialDefinitionPart]);
    }

    [TestMethod]
    public void AllPartialParts_MethodSymbol_PartialMethodDifferentClass()
    {
        const string code = """
            public partial class Sample
            {
                partial void SymbolMember();
            }
            public partial class Sample
            {
                partial void SymbolMember() { }
            }
            """;
        var symbols = CreateSymbols(code, AnalyzerLanguage.CSharp, x => x is MethodDeclarationSyntax);

        var declarationSymbol = symbols[0] as IMethodSymbol;
        var declarationResult = declarationSymbol.AllPartialParts().ToList();
        declarationResult.Should().HaveCount(2).And.Contain([declarationSymbol, declarationSymbol.PartialImplementationPart]);

        var implementationSymbol = symbols[1] as IMethodSymbol;
        var implementationResult = implementationSymbol.AllPartialParts().ToList();
        implementationResult.Should().HaveCount(2).And.Contain([implementationSymbol, implementationSymbol.PartialDefinitionPart]);
    }

    [TestMethod]
    public void AllPartialParts_PropertySymbol_PartialPropertySameClass()
    {
        const string code = """
            public partial class Sample
            {
                public partial int SymbolMember { get; set; }
                public partial int SymbolMember
                {
                    get => 0;
                    set { }
                }
            }
            """;
        var symbols = CreateSymbols(code, AnalyzerLanguage.CSharp, x => x is PropertyDeclarationSyntax);

        var declarationSymbol = symbols[0] as IPropertySymbol;
        var declarationResult = declarationSymbol.AllPartialParts().ToList();
        declarationResult.Should().HaveCount(2).And.Contain([declarationSymbol, declarationSymbol.PartialImplementationPart]);

        var implementationSymbol = symbols[1] as IPropertySymbol;
        var implementationResult = implementationSymbol.AllPartialParts().ToList();
        implementationResult.Should().HaveCount(2).And.Contain([implementationSymbol, implementationSymbol.PartialDefinitionPart]);
    }

    [TestMethod]
    public void AllPartialParts_PropertySymbol_PartialPropertyDifferentClass()
    {
        const string code = """
            public partial class Sample
            {
                public partial int SymbolMember { get; set; }
            }
            public partial class Sample
            {
                public partial int SymbolMember
                {
                    get => 0;
                    set { }
                }
            }
            """;
        var symbols = CreateSymbols(code, AnalyzerLanguage.CSharp, x => x is PropertyDeclarationSyntax);

        var declarationSymbol = symbols[0] as IPropertySymbol;
        var declarationResult = declarationSymbol.AllPartialParts().ToList();
        declarationResult.Should().HaveCount(2).And.Contain([declarationSymbol, declarationSymbol.PartialImplementationPart]);

        var implementationSymbol = symbols[1] as IPropertySymbol;
        var implementationResult = implementationSymbol.AllPartialParts().ToList();
        implementationResult.Should().HaveCount(2).And.Contain([implementationSymbol, implementationSymbol.PartialDefinitionPart]);
    }

    [TestMethod]
    public void AllPartialParts_OtherSymbol()
    {
        var result = Substitute.For<ISymbol>().AllPartialParts().ToList();
        result.Should().ContainSingle();
    }

    private static ISymbol CreateSymbol(string snippet, AnalyzerLanguage language, ParseOptions parseOptions = null)
    {
        var (tree, semanticModel) = TestCompiler.Compile(snippet, false, language, parseOptions: parseOptions);
        var node = tree.GetRoot().DescendantNodes().Last(x => x.ToString().Contains(" SymbolMember"));
        return semanticModel.GetDeclaredSymbol(node);
    }

    private static List<ISymbol> CreateSymbols(string snippet, AnalyzerLanguage language, Func<SyntaxNode, bool> additionalFilter = null)
    {
        var (tree, semanticModel) = TestCompiler.Compile(snippet, false, language);
        var nodes = tree.GetRoot().DescendantNodes().Where(x => x.ToString().Contains("SymbolMember") && (additionalFilter?.Invoke(x) ?? true)).ToList();
        return nodes.Select(x => semanticModel.GetDeclaredSymbol(x)).ToList();
    }
}

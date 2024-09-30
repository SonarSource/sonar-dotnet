/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using ISymbolExtensionsCommon = SonarAnalyzer.Core.Extensions.ISymbolExtensions;

namespace SonarAnalyzer.Core.Test.Extensions;

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
        ISymbolExtensionsCommon.IsAutoProperty(CreateSymbol(code, AnalyzerLanguage.CSharp)).Should().BeTrue();
    }

    [TestMethod]
    public void IsAutoProperty_AutoProperty_VB()
    {
        const string code = """
            Public Class Sample

                Public Property SymbolMember As String

            End Class
            """;
        ISymbolExtensionsCommon.IsAutoProperty(CreateSymbol(code, AnalyzerLanguage.VisualBasic)).Should().BeTrue();
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
        ISymbolExtensionsCommon.IsAutoProperty(CreateSymbol(code, AnalyzerLanguage.CSharp)).Should().BeFalse();
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
        ISymbolExtensionsCommon.IsAutoProperty(CreateSymbol(code, AnalyzerLanguage.VisualBasic)).Should().BeFalse();
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
        ISymbolExtensionsCommon.IsAutoProperty(CreateSymbol(code, AnalyzerLanguage.CSharp)).Should().BeFalse();
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
        ISymbolExtensionsCommon.IsAutoProperty(CreateSymbol(code, AnalyzerLanguage.VisualBasic)).Should().BeFalse();
    }

    [TestMethod]
    public void Symbol_IsPublicApi()
    {
        ISymbolExtensionsCommon.IsPubliclyAccessible(testSnippet.GetMethodSymbol("Base.Method1")).Should().BeTrue();
        ISymbolExtensionsCommon.IsPubliclyAccessible(testSnippet.GetMethodSymbol("Base.Method2")).Should().BeTrue();
        ISymbolExtensionsCommon.IsPubliclyAccessible(testSnippet.GetPropertySymbol("Base.Property")).Should().BeTrue();
        ISymbolExtensionsCommon.IsPubliclyAccessible(testSnippet.GetPropertySymbol("IInterface.Property2")).Should().BeTrue();
        ISymbolExtensionsCommon.IsPubliclyAccessible(testSnippet.GetPropertySymbol("Derived1.PrivateProperty")).Should().BeFalse();
        ISymbolExtensionsCommon.IsPubliclyAccessible(testSnippet.GetPropertySymbol("Derived1.PrivateProtectedProperty")).Should().BeFalse();
        ISymbolExtensionsCommon.IsPubliclyAccessible(testSnippet.GetPropertySymbol("Derived1.ProtectedProperty")).Should().BeTrue();
        ISymbolExtensionsCommon.IsPubliclyAccessible(testSnippet.GetPropertySymbol("Derived1.ProtectedInternalProperty")).Should().BeTrue();
        ISymbolExtensionsCommon.IsPubliclyAccessible(testSnippet.GetPropertySymbol("Derived1.InternalProperty")).Should().BeFalse();
    }

    [TestMethod]
    public void Symbol_IsInterfaceImplementationOrMemberOverride()
    {
        ISymbolExtensionsCommon.GetInterfaceMember(testSnippet.GetMethodSymbol("Base.Method1")).Should().BeNull();
        ISymbolExtensionsCommon.GetOverriddenMember(testSnippet.GetMethodSymbol("Base.Method1")).Should().BeNull();
        ISymbolExtensionsCommon.GetOverriddenMember(testSnippet.GetPropertySymbol("Derived2.Property")).Should().NotBeNull();
        ISymbolExtensionsCommon.GetInterfaceMember(testSnippet.GetPropertySymbol("Derived2.Property2")).Should().NotBeNull();
        ISymbolExtensionsCommon.GetInterfaceMember(testSnippet.GetMethodSymbol("Derived2.Method3")).Should().NotBeNull();
    }

    [TestMethod]
    public void Symbol_TryGetOverriddenOrInterfaceMember()
    {
        var actualOverriddenMethod = ISymbolExtensionsCommon.GetOverriddenMember(testSnippet.GetMethodSymbol("Base.Method1"));
        actualOverriddenMethod.Should().BeNull();

        var expectedOverriddenProperty = testSnippet.GetPropertySymbol("Base.Property");
        var propertySymbol = testSnippet.GetPropertySymbol("Derived2.Property");

        var actualOverriddenProperty = ISymbolExtensionsCommon.GetOverriddenMember(propertySymbol);
        actualOverriddenProperty.Should().NotBeNull();
        actualOverriddenProperty.Should().Be(expectedOverriddenProperty);

        var expectedOverriddenMethod = testSnippet.GetMethodSymbol("IInterface.Method3");
        actualOverriddenMethod = ISymbolExtensionsCommon.GetInterfaceMember(testSnippet.GetMethodSymbol("Derived2.Method3"));
        actualOverriddenMethod.Should().NotBeNull();
        actualOverriddenMethod.Should().Be(expectedOverriddenMethod);
    }

    [TestMethod]
    public void Symbol_IsChangeable()
    {
        ISymbolExtensionsCommon.IsChangeable(testSnippet.GetMethodSymbol("Base.Method1")).Should().BeFalse();
        ISymbolExtensionsCommon.IsChangeable(testSnippet.GetMethodSymbol("Base.Method4")).Should().BeTrue();
        ISymbolExtensionsCommon.IsChangeable(testSnippet.GetMethodSymbol("Derived2.Method5")).Should().BeFalse();
        ISymbolExtensionsCommon.IsChangeable(testSnippet.GetMethodSymbol("Derived2.Method3")).Should().BeFalse();
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
        ISymbolExtensionsCommon.GetParameters(symbol).Should().BeEmpty();
    }

    [TestMethod]
    public void GetInterfaceMember_WhenSymbolIsNull_ReturnsEmpty() =>
        ISymbolExtensionsCommon.GetInterfaceMember((ISymbol)null).Should().BeNull();

    [TestMethod]
    public void GetOverriddenMember_WhenSymbolIsNull_ReturnsEmpty() =>
        ISymbolExtensionsCommon.GetOverriddenMember((ISymbol)null).Should().BeNull();

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
        ISymbolExtensionsCommon.GetClassification(symbol).Should().Be(expected);
    }

    [TestMethod]
    public void GetClassification_UnknowKind()
    {
        var symbol = Substitute.For<ISymbol>();
        symbol.Kind.Returns((SymbolKind)999);
#if DEBUG
        new Action(() => ISymbolExtensionsCommon.GetClassification(symbol)).Should().Throw<NotSupportedException>();
#else
        ISymbolExtensionsCommon.GetClassification(symbol).Should().Be("symbol");
#endif
    }

    private static ISymbol CreateSymbol(string snippet, AnalyzerLanguage language, ParseOptions parseOptions = null)
    {
        var (tree, semanticModel) = TestHelper.Compile(snippet, false, language, parseOptions: parseOptions);
        var node = tree.GetRoot().DescendantNodes().Last(x => x.ToString().Contains(" SymbolMember"));
        return semanticModel.GetDeclaredSymbol(node);
    }
}

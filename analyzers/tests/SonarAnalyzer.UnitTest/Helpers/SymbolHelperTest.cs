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

using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.TestFramework;
using CodeAnalysisAccessibility = Microsoft.CodeAnalysis.Accessibility; // This is needed because there is an Accessibility namespace in the windows forms binaries.

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class SymbolHelperTest
    {
        internal const string TestInput = @"
namespace NS
{
  using System.Collections.Generic;

  public class Base
  {
    public class Nested
    {
      public class NestedMore
      {}
    }

    public virtual void Method1() { }
    protected virtual void Method2() { }
    public abstract int Property { get; set; }

    public void Method4(){}
  }
  private class Derived1 : Base
  {
    public override int Property { get; set; }
  }
  public class Derived2 : Base, IInterface
  {
    public override int Property { get; set; }
    public int Property2 { get; set; }
    public void Method3(){}

    public abstract void Method5();
    public void EventHandler(object o, System.EventArgs args){}
  }
  public interface IInterface
  {
    int Property2 { get; set; }
    void Method3();
    void Method4<T, V>(List<T> param1, List<int> param2, List<V> param3, IList<int> param4);
  }
}
";

        private SnippetCompiler testCode;

        [TestInitialize]
        public void Compile()
        {
            testCode = new SnippetCompiler(TestInput, ignoreErrors: true, language: AnalyzerLanguage.CSharp);
        }

        [TestMethod]
        public void Symbol_IsPublicApi()
        {
            ISymbol symbol = testCode.GetMethodSymbol("Base.Method1");
            SymbolHelper.IsPubliclyAccessible(symbol).Should().BeTrue();

            symbol = testCode.GetMethodSymbol("Base.Method2");
            symbol.IsPubliclyAccessible().Should().BeTrue();

            symbol = testCode.GetPropertySymbol("Base.Property");
            symbol.IsPubliclyAccessible().Should().BeTrue();

            symbol = testCode.GetPropertySymbol("IInterface.Property2");
            symbol.IsPubliclyAccessible().Should().BeTrue();

            symbol = testCode.GetPropertySymbol("Derived1.Property");
            symbol.IsPubliclyAccessible().Should().BeFalse();
        }

        [TestMethod]
        public void Symbol_IsInterfaceImplementationOrMemberOverride()
        {
            ISymbol symbol = testCode.GetMethodSymbol("Base.Method1");
            symbol.GetInterfaceMember().Should().BeNull();
            symbol.GetOverriddenMember().Should().BeNull();

            symbol = testCode.GetPropertySymbol("Derived2.Property");
            symbol.GetOverriddenMember().Should().NotBeNull();

            symbol = testCode.GetPropertySymbol("Derived2.Property2");
            symbol.GetInterfaceMember().Should().NotBeNull();

            symbol = testCode.GetMethodSymbol("Derived2.Method3");
            symbol.GetInterfaceMember().Should().NotBeNull();
        }

        [TestMethod]
        public void Symbol_TryGetOverriddenOrInterfaceMember()
        {
            var methodSymbol = testCode.GetMethodSymbol("Base.Method1");
            var actualOverriddenMethod = methodSymbol.GetOverriddenMember();
            actualOverriddenMethod.Should().BeNull();

            var expectedOverriddenProperty = testCode.GetPropertySymbol("Base.Property");
            var propertySymbol = testCode.GetPropertySymbol("Derived2.Property");

            var actualOverriddenProperty = propertySymbol.GetOverriddenMember();
            actualOverriddenProperty.Should().NotBeNull();
            actualOverriddenProperty.Should().Be(expectedOverriddenProperty);

            var expectedOverriddenMethod = testCode.GetMethodSymbol("IInterface.Method3");
            methodSymbol = testCode.GetMethodSymbol("Derived2.Method3");

            actualOverriddenMethod = methodSymbol.GetInterfaceMember();
            actualOverriddenMethod.Should().NotBeNull();
            actualOverriddenMethod.Should().Be(expectedOverriddenMethod);
        }

        [TestMethod]
        public void Symbol_IsChangeable()
        {
            var symbol = testCode.GetMethodSymbol("Base.Method1");
            symbol.IsChangeable().Should().BeFalse();

            symbol = testCode.GetMethodSymbol("Base.Method4");
            symbol.IsChangeable().Should().BeTrue();

            symbol = testCode.GetMethodSymbol("Derived2.Method5");
            symbol.IsChangeable().Should().BeFalse();

            symbol = testCode.GetMethodSymbol("Derived2.Method3");
            symbol.IsChangeable().Should().BeFalse();
        }

        [TestMethod]
        public void Symbol_IsProbablyEventHandler()
        {
            var symbol = testCode.GetMethodSymbol("Derived2.Method3");
            symbol.IsEventHandler().Should().BeFalse();

            symbol = testCode.GetMethodSymbol("Derived2.EventHandler");
            symbol.IsEventHandler().Should().BeTrue();
        }

        [TestMethod]
        public void Symbol_GetSelfAndBaseTypes()
        {
            var objectType = testCode.GetTypeByMetadataName("System.Object");
            var baseTypes = objectType.GetSelfAndBaseTypes().ToList();
            baseTypes.Should().ContainSingle();
            baseTypes.First().Should().Be(objectType);

            var derived1Type = testCode.GetTypeSymbol("Derived1") as INamedTypeSymbol;
            baseTypes = derived1Type.GetSelfAndBaseTypes().ToList();
            baseTypes.Should().HaveCount(3);
            baseTypes.Should().HaveElementAt(0, derived1Type);
            baseTypes.Should().HaveElementAt(1, testCode.GetTypeSymbol("Base"));
            baseTypes.Should().HaveElementAt(2, objectType);
        }

        [TestMethod]
        public void Symbol_GetAllNamedTypes_Namespace()
        {
            var nsSymbol = testCode.GetNamespaceSymbol("NS");

            var typeSymbols = nsSymbol.GetAllNamedTypes();
            typeSymbols.Should().HaveCount(6);
        }

        [TestMethod]
        public void Symbol_GetAllNamedTypes_Type()
        {
            var typeSymbol = testCode.GetTypeSymbol("Base") as INamedTypeSymbol;
            var typeSymbols = typeSymbol.GetAllNamedTypes();
            typeSymbols.Should().HaveCount(3);
        }

        [TestMethod]
        public void Symbol_IsKnownType()
        {
            var method4 = (MethodDeclarationSyntax)testCode.GetMethodDeclaration("IInterface.Method4");

            method4.ParameterList
                .Parameters[0]
                .Type
                .IsKnownType(KnownType.System_Collections_Generic_List_T, testCode.SemanticModel)
                .Should().BeTrue();

            method4.ParameterList
                .Parameters[1]
                .Type
                .IsKnownType(KnownType.System_Collections_Generic_List_T, testCode.SemanticModel)
                .Should().BeTrue();

            method4.ParameterList
                .Parameters[2]
                .Type
                .IsKnownType(KnownType.System_Collections_Generic_List_T, testCode.SemanticModel)
                .Should().BeTrue();

            method4.ParameterList
                .Parameters[3]
                .Type
                .IsKnownType(KnownType.System_Collections_Generic_List_T, testCode.SemanticModel)
                .Should().BeFalse();
        }

        [TestMethod]
        public void IsAnyAttributeInOverridingChain_WhenMethodSymbolIsNull_ReturnsFalse() =>
            SymbolHelper.IsAnyAttributeInOverridingChain((IMethodSymbol)null).Should().BeFalse();

        [TestMethod]
        public void IsAnyAttributeInOverridingChain_WhenPropertySymbolIsNull_ReturnsFalse() =>
            SymbolHelper.IsAnyAttributeInOverridingChain((IPropertySymbol)null).Should().BeFalse();

        [TestMethod]
        public void AnyAttributeDerivesFrom_WhenSymbolIsNull_ReturnsFalse() =>
            SymbolHelper.AnyAttributeDerivesFrom(null, KnownType.Void).Should().BeFalse();

        [TestMethod]
        public void AnyAttributeDerivesFromAny_WhenSymbolIsNull_ReturnsFalse() =>
            SymbolHelper.AnyAttributeDerivesFromAny(null, ImmutableArray.Create(KnownType.Void)).Should().BeFalse();

        [TestMethod]
        public void GetAttributesForKnownType_WhenSymbolIsNull_ReturnsEmpty() =>
            SymbolHelper.GetAttributes(null, KnownType.Void).Should().BeEmpty();

        [TestMethod]
        public void GetAttributesForKnownTypes_WhenSymbolIsNull_ReturnsEmpty() =>
            SymbolHelper.GetAttributes(null, ImmutableArray.Create(KnownType.Void)).Should().BeEmpty();

        [TestMethod]
        public void GetParameters_WhenSymbolIsNotMethodOrProperty_ReturnsEmpty() =>
            Mock.Of<ISymbol>(x => x.Kind == SymbolKind.Alias).GetParameters().Should().BeEmpty();

        [TestMethod]
        public void GetInterfaceMember_WhenSymbolIsNull_ReturnsEmpty() =>
            ((ISymbol)null).GetInterfaceMember().Should().BeNull();

        [TestMethod]
        public void GetOverriddenMember_WhenSymbolIsNull_ReturnsEmpty() =>
            ((ISymbol)null).GetOverriddenMember().Should().BeNull();

        [TestMethod]
        public void GetAllNamedTypesForNamespace_WhenSymbolIsNull_ReturnsEmpty() =>
            ((INamespaceSymbol)null).GetAllNamedTypes().Should().BeEmpty();

        [TestMethod]
        public void GetAllNamedTypesForNamedType_WhenSymbolIsNull_ReturnsEmpty() =>
            ((INamedTypeSymbol)null).GetAllNamedTypes().Should().BeEmpty();

        [TestMethod]
        public void GetSelfAndBaseTypes_WhenSymbolIsNull_ReturnsEmpty() =>
            ((ITypeSymbol)null).GetSelfAndBaseTypes().Should().BeEmpty();

        [TestMethod]
        public void GetEffectiveAccessibility_WhenSymbolIsNull_ReturnsNotApplicable() =>
            ((ISymbol)null).GetEffectiveAccessibility().Should().Be(CodeAnalysisAccessibility.NotApplicable);
    }
}

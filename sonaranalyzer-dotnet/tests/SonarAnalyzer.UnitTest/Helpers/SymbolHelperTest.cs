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

using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.TestFramework;

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
            this.testCode = new SnippetCompiler(TestInput, ignoreErrors: true, language: AnalyzerLanguage.CSharp);
        }

        [TestMethod]
        public void Symbol_IsPublicApi()
        {
            ISymbol symbol = this.testCode.GetMethodSymbol("Base.Method1");
            SymbolHelper.IsPubliclyAccessible(symbol).Should().BeTrue();

            symbol = this.testCode.GetMethodSymbol("Base.Method2");
            symbol.IsPubliclyAccessible().Should().BeTrue();

            symbol = this.testCode.GetPropertySymbol("Base.Property");
            symbol.IsPubliclyAccessible().Should().BeTrue();

            symbol = this.testCode.GetPropertySymbol("IInterface.Property2");
            symbol.IsPubliclyAccessible().Should().BeTrue();

            symbol = this.testCode.GetPropertySymbol("Derived1.Property");
            symbol.IsPubliclyAccessible().Should().BeFalse();
        }

        [TestMethod]
        public void Symbol_IsInterfaceImplementationOrMemberOverride()
        {
            ISymbol symbol = this.testCode.GetMethodSymbol("Base.Method1");
            symbol.GetInterfaceMember().Should().BeNull();
            symbol.GetOverriddenMember().Should().BeNull();

            symbol = this.testCode.GetPropertySymbol("Derived2.Property");
            symbol.GetOverriddenMember().Should().NotBeNull();

            symbol = this.testCode.GetPropertySymbol("Derived2.Property2");
            symbol.GetInterfaceMember().Should().NotBeNull();

            symbol = this.testCode.GetMethodSymbol("Derived2.Method3");
            symbol.GetInterfaceMember().Should().NotBeNull();
        }

        [TestMethod]
        public void Symbol_TryGetOverriddenOrInterfaceMember()
        {
            var methodSymbol = this.testCode.GetMethodSymbol("Base.Method1");
            var actualOverriddenMethod = methodSymbol.GetOverriddenMember();
            actualOverriddenMethod.Should().BeNull();


            var expectedOverriddenProperty = this.testCode.GetPropertySymbol("Base.Property");
            var propertySymbol = this.testCode.GetPropertySymbol("Derived2.Property");

            var actualOverriddenProperty = propertySymbol.GetOverriddenMember();
            actualOverriddenProperty.Should().NotBeNull();
            actualOverriddenProperty.Should().Be(expectedOverriddenProperty);


            var expectedOverriddenMethod = this.testCode.GetMethodSymbol("IInterface.Method3");
            methodSymbol = this.testCode.GetMethodSymbol("Derived2.Method3");

            actualOverriddenMethod = methodSymbol.GetInterfaceMember();
            actualOverriddenMethod.Should().NotBeNull();
            actualOverriddenMethod.Should().Be(expectedOverriddenMethod);
        }

        [TestMethod]
        public void Symbol_IsChangeable()
        {
            var symbol = this.testCode.GetMethodSymbol("Base.Method1");
            symbol.IsChangeable().Should().BeFalse();

            symbol = this.testCode.GetMethodSymbol("Base.Method4");
            symbol.IsChangeable().Should().BeTrue();

            symbol = this.testCode.GetMethodSymbol("Derived2.Method5");
            symbol.IsChangeable().Should().BeFalse();

            symbol = this.testCode.GetMethodSymbol("Derived2.Method3");
            symbol.IsChangeable().Should().BeFalse();
        }

        [TestMethod]
        public void Symbol_IsProbablyEventHandler()
        {
            var symbol = this.testCode.GetMethodSymbol("Derived2.Method3");
            symbol.IsEventHandler().Should().BeFalse();

            symbol = this.testCode.GetMethodSymbol("Derived2.EventHandler");
            symbol.IsEventHandler().Should().BeTrue();
        }

        [TestMethod]
        public void Symbol_GetSelfAndBaseTypes()
        {
            var objectType = this.testCode.GetTypeByMetadataName("System.Object");
            var baseTypes = objectType.GetSelfAndBaseTypes().ToList();
            baseTypes.Should().ContainSingle();
            baseTypes.First().Should().Be(objectType);

            var derived1Type = this.testCode.GetTypeSymbol("Derived1") as INamedTypeSymbol;
            baseTypes = derived1Type.GetSelfAndBaseTypes().ToList();
            baseTypes.Should().HaveCount(3);
            baseTypes.Should().HaveElementAt(0, derived1Type);
            baseTypes.Should().HaveElementAt(1, this.testCode.GetTypeSymbol("Base"));
            baseTypes.Should().HaveElementAt(2, objectType);
        }

        [TestMethod]
        public void Symbol_GetAllNamedTypes_Namespace()
        {
            var nsSymbol = this.testCode.GetNamespaceSymbol("NS");

            var typeSymbols = nsSymbol.GetAllNamedTypes();
            typeSymbols.Should().HaveCount(6);
        }

        [TestMethod]
        public void Symbol_GetAllNamedTypes_Type()
        {
            var typeSymbol = this.testCode.GetTypeSymbol("Base") as INamedTypeSymbol;
            var typeSymbols = typeSymbol.GetAllNamedTypes();
            typeSymbols.Should().HaveCount(3);
        }

        [TestMethod]
        public void Symbol_IsKnownType()
        {
            var method4 = (MethodDeclarationSyntax)this.testCode.GetMethodDeclaration("IInterface.Method4");

            method4.ParameterList
                .Parameters[0]
                .Type
                .IsKnownType(KnownType.System_Collections_Generic_List_T, this.testCode.SemanticModel)
                .Should().BeTrue();

            method4.ParameterList
                .Parameters[1]
                .Type
                .IsKnownType(KnownType.System_Collections_Generic_List_T, this.testCode.SemanticModel)
                .Should().BeTrue();

            method4.ParameterList
                .Parameters[2]
                .Type
                .IsKnownType(KnownType.System_Collections_Generic_List_T, this.testCode.SemanticModel)
                .Should().BeTrue();

            method4.ParameterList
                .Parameters[3]
                .Type
                .IsKnownType(KnownType.System_Collections_Generic_List_T, this.testCode.SemanticModel)
                .Should().BeFalse();
        }
    }
}

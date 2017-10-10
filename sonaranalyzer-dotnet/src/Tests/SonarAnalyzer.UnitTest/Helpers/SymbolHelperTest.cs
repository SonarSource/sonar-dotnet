/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using SonarAnalyzer.Helpers;

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

        private ClassDeclarationSyntax baseClassDeclaration;
        private ClassDeclarationSyntax derivedClassDeclaration1;
        private ClassDeclarationSyntax derivedClassDeclaration2;
        private InterfaceDeclarationSyntax interfaceDeclaration;
        private SemanticModel semanticModel;
        private SyntaxTree tree;

        [TestInitialize]
        public void Compile()
        {
            using (var workspace = new AdhocWorkspace())
            {
                var document = workspace.CurrentSolution.AddProject("foo", "foo.dll", LanguageNames.CSharp)
                    .AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                    .AddMetadataReference(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location))
                    .AddDocument("test", TestInput);
                var compilation = document.Project.GetCompilationAsync().Result;
                tree = compilation.SyntaxTrees.First();
                semanticModel = compilation.GetSemanticModel(tree);

                baseClassDeclaration = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .First(m => m.Identifier.ValueText == "Base");
                derivedClassDeclaration1 = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .First(m => m.Identifier.ValueText == "Derived1");
                derivedClassDeclaration2 = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .First(m => m.Identifier.ValueText == "Derived2");
                interfaceDeclaration = tree.GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>()
                    .First(m => m.Identifier.ValueText == "IInterface");
            }
        }

        [TestMethod]
        public void Symbol_IsPublicApi()
        {
            var method = baseClassDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == "Method1");
            var symbol = semanticModel.GetDeclaredSymbol(method);

            SymbolHelper.IsPubliclyAccessible(symbol).Should().BeTrue();

            method = baseClassDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == "Method2");
            symbol = semanticModel.GetDeclaredSymbol(method);

            symbol.IsPubliclyAccessible().Should().BeTrue();

            var property = baseClassDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == "Property");
            symbol = semanticModel.GetDeclaredSymbol(property);

            symbol.IsPubliclyAccessible().Should().BeTrue();

            property = interfaceDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == "Property2");
            symbol = semanticModel.GetDeclaredSymbol(property);

            symbol.IsPubliclyAccessible().Should().BeTrue();

            property = derivedClassDeclaration1.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == "Property");
            symbol = semanticModel.GetDeclaredSymbol(property);

            symbol.IsPubliclyAccessible().Should().BeFalse();
        }

        [TestMethod]
        public void Symbol_IsInterfaceImplementationOrMemberOverride()
        {
            var method = baseClassDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == "Method1");
            var symbol = semanticModel.GetDeclaredSymbol(method);
            symbol.GetInterfaceMember().Should().BeNull();
            symbol.GetOverriddenMember().Should().BeNull();

            var property = derivedClassDeclaration2.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == "Property");
            symbol = semanticModel.GetDeclaredSymbol(property);

            symbol.GetOverriddenMember().Should().NotBeNull();

            property = derivedClassDeclaration2.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == "Property2");
            symbol = semanticModel.GetDeclaredSymbol(property);

            symbol.GetInterfaceMember().Should().NotBeNull();

            method = derivedClassDeclaration2.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == "Method3");
            symbol = semanticModel.GetDeclaredSymbol(method);

            symbol.GetInterfaceMember().Should().NotBeNull();
        }

        [TestMethod]
        public void Symbol_TryGetOverriddenOrInterfaceMember()
        {
            var method = baseClassDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == "Method1");
            var methodSymbol = (IMethodSymbol)semanticModel.GetDeclaredSymbol(method);
            var overriddenMethod = methodSymbol.GetOverriddenMember();
            overriddenMethod.Should().BeNull();

            var property = derivedClassDeclaration2.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                .First(p => p.Identifier.ValueText == "Property");
            var propertySymbol = (IPropertySymbol)semanticModel.GetDeclaredSymbol(property);

            var overriddenProperty = propertySymbol.GetOverriddenMember();
            overriddenProperty.Should().NotBeNull();

            property = baseClassDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                .First(p => p.Identifier.ValueText == "Property");
            overriddenProperty.Should().Be((IPropertySymbol)semanticModel.GetDeclaredSymbol(property));

            method = derivedClassDeclaration2.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == "Method3");
            methodSymbol = (IMethodSymbol)semanticModel.GetDeclaredSymbol(method);

            overriddenMethod = methodSymbol.GetInterfaceMember();
            overriddenMethod.Should().NotBeNull();

            method = interfaceDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == "Method3");
            overriddenMethod.Should().Be((IMethodSymbol)semanticModel.GetDeclaredSymbol(method));
        }

        [TestMethod]
        public void Symbol_IsChangeable()
        {
            var method = baseClassDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == "Method1");
            var symbol = semanticModel.GetDeclaredSymbol(method) as IMethodSymbol;

            symbol.IsChangeable().Should().BeFalse();

            method = baseClassDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == "Method4");
            symbol = semanticModel.GetDeclaredSymbol(method) as IMethodSymbol;

            symbol.IsChangeable().Should().BeTrue();

            method = derivedClassDeclaration2.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == "Method5");
            symbol = semanticModel.GetDeclaredSymbol(method) as IMethodSymbol;

            symbol.IsChangeable().Should().BeFalse();

            method = derivedClassDeclaration2.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == "Method3");
            symbol = semanticModel.GetDeclaredSymbol(method) as IMethodSymbol;

            symbol.IsChangeable().Should().BeFalse();
        }

        [TestMethod]
        public void Symbol_IsProbablyEventHandler()
        {
            var method = derivedClassDeclaration2.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == "Method3");
            var symbol = semanticModel.GetDeclaredSymbol(method) as IMethodSymbol;

            symbol.IsProbablyEventHandler().Should().BeFalse();

            method = derivedClassDeclaration2.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == "EventHandler");
            symbol = semanticModel.GetDeclaredSymbol(method) as IMethodSymbol;

            symbol.IsProbablyEventHandler().Should().BeTrue();
        }

        [TestMethod]
        public void Symbol_GetSelfAndBaseTypes()
        {
            var objectType = semanticModel.Compilation.GetTypeByMetadataName("System.Object");
            var baseTypes = objectType.GetSelfAndBaseTypes().ToList();
            baseTypes.Should().HaveCount(1);
            baseTypes.First().Should().Be(objectType);

            var derived1Type = semanticModel.GetDeclaredSymbol(derivedClassDeclaration1) as INamedTypeSymbol;
            baseTypes = derived1Type.GetSelfAndBaseTypes().ToList();
            baseTypes.Should().HaveCount(3);
            baseTypes[0].Should().Be(derived1Type);
            baseTypes[1].Should().Be(semanticModel.GetDeclaredSymbol(baseClassDeclaration) as INamedTypeSymbol);
            baseTypes[2].Should().Be(objectType);
        }

        [TestMethod]
        public void Symbol_GetAllNamedTypes_Namespace()
        {
            var ns = (NamespaceDeclarationSyntax)tree.GetRoot().ChildNodes().First();
            var nsSymbol = semanticModel.GetDeclaredSymbol(ns) as INamespaceSymbol;

            var typeSymbols = nsSymbol.GetAllNamedTypes();
            typeSymbols.Should().HaveCount(6);
        }

        [TestMethod]
        public void Symbol_GetAllNamedTypes_Type()
        {
            var typeSymbol = semanticModel.GetDeclaredSymbol(baseClassDeclaration) as INamedTypeSymbol;
            var typeSymbols = typeSymbol.GetAllNamedTypes();
            typeSymbols.Should().HaveCount(3);
        }

        [TestMethod]
        public void Symbol_IsKnownType()
        {
            var method4 = interfaceDeclaration
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == "Method4");

            method4.ParameterList
                .Parameters[0]
                .Type
                .IsKnownType(KnownType.System_Collections_Generic_List_T, semanticModel)
                .Should().BeTrue();

            method4.ParameterList
                .Parameters[1]
                .Type
                .IsKnownType(KnownType.System_Collections_Generic_List_T, semanticModel)
                .Should().BeTrue();

            method4.ParameterList
                .Parameters[2]
                .Type
                .IsKnownType(KnownType.System_Collections_Generic_List_T, semanticModel)
                .Should().BeTrue();

            method4.ParameterList
                .Parameters[3]
                .Type
                .IsKnownType(KnownType.System_Collections_Generic_List_T, semanticModel)
                .Should().BeFalse();
        }
    }
}

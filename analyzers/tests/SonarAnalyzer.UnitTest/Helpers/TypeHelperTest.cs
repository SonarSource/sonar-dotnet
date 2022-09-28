/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class TypeHelperTest
    {
        private ClassDeclarationSyntax baseClassDeclaration;
        private ClassDeclarationSyntax derivedClassDeclaration1;
        private ClassDeclarationSyntax derivedClassDeclaration2;
        private SyntaxNode root;
        private SemanticModel semanticModel;

        [TestInitialize]
        public void Compile()
        {
            using (var workspace = new AdhocWorkspace())
            {
                var document = workspace.CurrentSolution.AddProject("foo", "foo.dll", LanguageNames.CSharp)
                    .AddDocument("test", SymbolHelperTest.TestInput);
                var compilation = document.Project.GetCompilationAsync().Result;
                var tree = compilation.SyntaxTrees.First();
                semanticModel = compilation.GetSemanticModel(tree);

                root = tree.GetRoot();
                baseClassDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .First(m => m.Identifier.ValueText == "Base");
                derivedClassDeclaration1 = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .First(m => m.Identifier.ValueText == "Derived1");
                derivedClassDeclaration2 = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .First(m => m.Identifier.ValueText == "Derived2");
            }
        }

        [TestMethod]
        public void Type_DerivesOrImplementsAny()
        {
            var baseType = new KnownType("NS.Base");
            var interfaceType = new KnownType("NS.IInterface");

            var derived1Type = semanticModel.GetDeclaredSymbol(derivedClassDeclaration1) as INamedTypeSymbol;
            var derived2Type = semanticModel.GetDeclaredSymbol(derivedClassDeclaration2) as INamedTypeSymbol;

            derived2Type.DerivesOrImplements(interfaceType).Should().BeTrue();
            derived1Type.DerivesOrImplements(interfaceType).Should().BeFalse();

            var baseTypes = ImmutableArray.Create(new[] { interfaceType, baseType });
            derived1Type.DerivesOrImplementsAny(baseTypes).Should().BeTrue();
        }

        [TestMethod]
        public void Type_DerivesOrImplements()
        {
            var snippet = new SnippetCompiler(@"
using System.Collections.Generic;

public interface IBase { }
public interface IDerived: IBase { }
public interface IOther { }
public class C<T>: List<T>, IOther { }

public class D<T> where T: C<T>, IDerived { }
");
            var typeParameter = snippet.SyntaxTree.GetRoot().ChildNodes().OfType<ClassDeclarationSyntax>().Last().TypeParameterList.Parameters[0];
            var symbol = (ITypeSymbol)snippet.SemanticModel.GetDeclaredSymbol(typeParameter); // T in D<T>
            var allInterfacesNames = symbol.AllInterfaces().Select(x => x.ToDisplayString()).ToList();
            allInterfacesNames.Should().Equal(
                "IDerived",
                "System.Collections.Generic.IList<T>",
                "System.Collections.Generic.ICollection<T>",
                "System.Collections.IList",
                "System.Collections.ICollection",
                "System.Collections.Generic.IReadOnlyList<T>",
                "System.Collections.Generic.IReadOnlyCollection<T>",
                "System.Collections.Generic.IEnumerable<T>",
                "System.Collections.IEnumerable",
                "IOther",
                "IBase");
        }

        [TestMethod]
        public void Type_DerivesOrImplements_RecursiveDefinition()
        {
            var snippet = new SnippetCompiler(@"
public interface I<T> where T: I<T> { }
");
            var typeParameter = snippet.SyntaxTree.GetRoot().ChildNodes().OfType<InterfaceDeclarationSyntax>().Last().TypeParameterList.Parameters[0];
            var symbol = (ITypeSymbol)snippet.SemanticModel.GetDeclaredSymbol(typeParameter); // T in I<T>
            var allInterfacesNames = symbol.AllInterfaces().Select(x => x.ToDisplayString()).ToList();
            allInterfacesNames.Should().Equal("I<T>");
        }

        [TestMethod]
        public void Type_Is()
        {
            var baseKnownType = new KnownType("NS.Base");
            var baseKnownTypes = ImmutableArray.Create(new[] { baseKnownType });

            var baseType = semanticModel.GetDeclaredSymbol(baseClassDeclaration) as INamedTypeSymbol;

            baseType.Is(baseKnownType).Should().BeTrue();
            baseType.IsAny(baseKnownTypes).Should().BeTrue();
        }

        [TestMethod]
        public void Type_GetSymbolType_Alias()
        {
            var aliasUsing = root.DescendantNodesAndSelf().OfType<UsingDirectiveSyntax>().FirstOrDefault(x => x.Alias is not null);
            var symbol = semanticModel.GetDeclaredSymbol(aliasUsing);
            var type = symbol.GetSymbolType();
            symbol.ToString().Should().Be("PropertyBag");
            type.ToString().Should().Be("System.Collections.Generic.Dictionary<string, object>");
        }
    }
}

/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using SonarAnalyzer.Common;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    /// <summary>
    /// Helper class compiles snippets of code on the fly
    /// </summary>
    internal class SnippetCompiler
    {
        private readonly SyntaxTree syntaxTree;
        public SemanticModel SemanticModel { get; private set; }

        public SnippetCompiler(string code)
        {
            var compilation = SolutionBuilder
            .Create()
            .AddProject(AnalyzerLanguage.CSharp, createExtraEmptyFile: false)
            .AddSnippet(code)
            .GetCompilation();
            this.syntaxTree = compilation.SyntaxTrees.First();
            this.SemanticModel = compilation.GetSemanticModel(this.syntaxTree);
        }

        public MethodDeclarationSyntax GetMethodDeclaration(string typeDotMethodName)
        {
            var nameParts = typeDotMethodName.Split('.');

            var type = this.syntaxTree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == nameParts[0]);

            var method = type.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == nameParts[1]);

            method.Should().NotBeNull("Test setup error: could not find method declaration in code snippet: " +
                $" Type: {nameParts[0]}, Method: {nameParts[1]}");

            return method;
        }

        public INamespaceSymbol GetNamespaceSymbol(string name)
        {
            var symbol = this.syntaxTree.GetRoot().DescendantNodes().OfType<NamespaceDeclarationSyntax>()
                .Select(s => this.SemanticModel.GetDeclaredSymbol(s))
                .First( s=> s.Name == name) as INamespaceSymbol;

            symbol.Should().NotBeNull($"Test setup error: could not find namespace in code snippet: {name}");

            return symbol;
        }

        public ITypeSymbol GetTypeSymbol(string typeName)
        {
            var type = this.syntaxTree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == typeName);

            var symbol = this.SemanticModel.GetDeclaredSymbol(type) as ITypeSymbol;
            symbol.Should().NotBeNull($"Test setup error: could not find type in code snippet: {type}");
            
            return symbol;
        }

        public IMethodSymbol GetMethodSymbol(string typeDotMethodName)
        {
            var method = GetMethodDeclaration(typeDotMethodName);
            var symbol = this.SemanticModel.GetDeclaredSymbol(method) as IMethodSymbol;
  
            return symbol;
        }

        public IPropertySymbol GetPropertySymbol(string typeDotMethodName)
        {
            var nameParts = typeDotMethodName.Split('.');
            var type = this.syntaxTree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == nameParts[0]);

            var method = type.DescendantNodes().OfType<PropertyDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == nameParts[1]);

            var symbol = this.SemanticModel.GetDeclaredSymbol(method) as IPropertySymbol;
            symbol.Should().NotBeNull("Test setup error: could not find property in code snippet: " +
                $" Type: {nameParts[0]}, Method: {nameParts[1]}");

            return symbol;
        }

        public INamedTypeSymbol GetTypeByMetadataName(string metadataName)
        {
            return this.SemanticModel.Compilation.GetTypeByMetadataName(metadataName);
        }
    }
}

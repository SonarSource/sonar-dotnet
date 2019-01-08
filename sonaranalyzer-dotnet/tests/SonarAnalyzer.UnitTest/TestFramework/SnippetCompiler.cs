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

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using CSharpSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;
using VBSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    /// <summary>
    /// Helper class compiles snippets of code on the fly
    /// </summary>
    internal class SnippetCompiler
    {
        private readonly Compilation compilation;

        public SyntaxTree SyntaxTree { get; private set; }
        public SemanticModel SemanticModel { get; private set; }

        public SnippetCompiler(string code, params MetadataReference[] additionalReferences)
            : this(code, false, AnalyzerLanguage.CSharp, (IEnumerable<MetadataReference>)additionalReferences)
        {
        }

        public SnippetCompiler(string code, IEnumerable<MetadataReference> additionalReferences)
            : this(code, false, AnalyzerLanguage.CSharp, additionalReferences)
        {
        }

        public SnippetCompiler(string code, bool ignoreErrors, AnalyzerLanguage language, IEnumerable<MetadataReference> additionalReferences = null)
        {
            CompilationOptions compilationOptions = language == AnalyzerLanguage.CSharp
                ? (CompilationOptions) new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                : (CompilationOptions) new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

            this.compilation = SolutionBuilder
            .Create()
            .AddProject(language, createExtraEmptyFile: false)
            .AddSnippet(code)
            .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
            .GetCompilation(compilationOptions: compilationOptions);

            if (!ignoreErrors && HasCompilationErrors(compilation))
            {
                DumpCompilationErrors(compilation);
                throw new System.InvalidOperationException("Test setup error: test code snippet did not compile. See output window for details.");
            }

            this.SyntaxTree = compilation.SyntaxTrees.First();
            this.SemanticModel = compilation.GetSemanticModel(this.SyntaxTree);
        }

        public bool IsCSharp() =>
            this.compilation.Language == LanguageNames.CSharp;

        public IEnumerable<TSyntaxNodeType> GetNodes<TSyntaxNodeType>()
            where TSyntaxNodeType : SyntaxNode =>
            this.SyntaxTree.GetRoot().DescendantNodes().OfType<TSyntaxNodeType>();
 
        public TSymbolType GetSymbol<TSymbolType>(SyntaxNode node)
            where TSymbolType : class, ISymbol =>
            this.SemanticModel.GetSymbolInfo(node).Symbol as TSymbolType;

        public SyntaxNode GetMethodDeclaration(string typeDotMethodName)
        {
            var nameParts = typeDotMethodName.Split('.');

            SyntaxNode method = null;

            if (IsCSharp())
            {
                var type = this.GetNodes<CSharpSyntax.TypeDeclarationSyntax>()
                    .First(m => m.Identifier.ValueText == nameParts[0]);
                method = type.DescendantNodes().OfType<CSharpSyntax.MethodDeclarationSyntax>()
                    .First(m => m.Identifier.ValueText == nameParts[1]);
            }
            else
            {
                var type = this.GetNodes<VBSyntax.TypeStatementSyntax>()
                    .First(m => m.Identifier.ValueText == nameParts[0]);
                method = type.DescendantNodes().OfType<VBSyntax.MethodStatementSyntax>()
                    .First(m => m.Identifier.ValueText == nameParts[1]);
            }

            method.Should().NotBeNull("Test setup error: could not find method declaration in code snippet: " +
                $" Type: {nameParts[0]}, Method: {nameParts[1]}");

            return method;
        }

        public INamespaceSymbol GetNamespaceSymbol(string name)
        {
            var symbol = (this.GetNodes<CSharpSyntax.NamespaceDeclarationSyntax>()
                .Concat<SyntaxNode>(this.GetNodes<VBSyntax.NamespaceStatementSyntax>()))
                .Select(s => this.SemanticModel.GetDeclaredSymbol(s))
                .First( s=> s.Name == name) as INamespaceSymbol;

            symbol.Should().NotBeNull($"Test setup error: could not find namespace in code snippet: {name}");

            return symbol;
        }

        public ITypeSymbol GetTypeSymbol(string typeName)
        {
            var type = (SyntaxNode)this.GetNodes<CSharpSyntax.TypeDeclarationSyntax>().FirstOrDefault(m => m.Identifier.ValueText == typeName)
                ?? (this.GetNodes<VBSyntax.TypeStatementSyntax>().FirstOrDefault(m => m.Identifier.ValueText == typeName));

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

            SyntaxNode property = null;

            if (IsCSharp())
            { 
                var type = this.SyntaxTree.GetRoot().DescendantNodes().OfType<CSharpSyntax.TypeDeclarationSyntax>()
                    .First(m => m.Identifier.ValueText == nameParts[0]);

                property = type.DescendantNodes().OfType<CSharpSyntax.PropertyDeclarationSyntax>()
                    .First(m => m.Identifier.ValueText == nameParts[1]);
            }
            else
            {
                var type = this.SyntaxTree.GetRoot().DescendantNodes().OfType<VBSyntax.TypeStatementSyntax>()
                    .First(m => m.Identifier.ValueText == nameParts[0]);

                property = type.DescendantNodes().OfType<VBSyntax.PropertyStatementSyntax>()
                    .First(m => m.Identifier.ValueText == nameParts[1]);
            }

            var symbol = this.SemanticModel.GetDeclaredSymbol(property) as IPropertySymbol;
            symbol.Should().NotBeNull("Test setup error: could not find property in code snippet: " +
                $" Type: {nameParts[0]}, Method: {nameParts[1]}");

            return symbol;
        }

        public INamedTypeSymbol GetTypeByMetadataName(string metadataName)
        {
            return this.SemanticModel.Compilation.GetTypeByMetadataName(metadataName);
        }


        #region Private methods
        
        private static bool HasCompilationErrors(Compilation compilation) =>
            compilation.GetDiagnostics().Any(d => d.Id.StartsWith("CS") || d.Id.StartsWith("BC"));

        private static void DumpCompilationErrors(Compilation compilation)
        {
            var diagnostics = compilation.GetDiagnostics();

            Console.WriteLine("Diagnostic errors:");
            foreach (var d in diagnostics)
            {
                Console.WriteLine($"  {d.Id} Line: {d.Location.GetMappedLineSpan().StartLinePosition.Line}: {d.GetMessage()}");
            }
        }

        #endregion
    }
}

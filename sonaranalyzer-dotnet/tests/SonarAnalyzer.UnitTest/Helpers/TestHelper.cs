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

using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.UnitTest.TestFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SonarAnalyzer.UnitTest
{
    internal static class TestHelper
    {
        public static (SyntaxTree, SemanticModel) Compile(string classDeclaration, bool isCSharp = true,
            params MetadataReference[] additionalReferences)
        {
            var language = isCSharp ? AnalyzerLanguage.CSharp : AnalyzerLanguage.VisualBasic;
            var ext = isCSharp ? ".cs" : ".vb";

            var compilation = SolutionBuilder
                .Create()
                .AddProject(language, createExtraEmptyFile: false)
                .AddSnippet(classDeclaration)
                .AddDocument(Path.Combine("TestCases", "Helpers", "Microsoft_AspNetCore_Mvc_ControllerAttribute" + ext))
                .AddDocument(Path.Combine("TestCases", "Helpers", "Microsoft_AspNetCore_Mvc_ControllerBase" + ext))
                .AddDocument(Path.Combine("TestCases", "Helpers", "Microsoft_AspNetCore_Mvc_NonControllerAttribute" + ext))
                .AddDocument(Path.Combine("TestCases", "Helpers", "System_Web_MVC_Controller" + ext))
                .AddReferences(additionalReferences)
                .GetCompilation();
            var tree = compilation.SyntaxTrees.First();
            return (tree, compilation.GetSemanticModel(tree));
        }

        public static MethodDeclarationSyntax GetMethod(this SyntaxTree syntaxTree, string name, int skip = 0) =>
            syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.Identifier.ValueText == name)
                .Skip(skip)
                .First();

        public static (MethodDeclarationSyntax, SemanticModel) GetMethod(this (SyntaxTree, SemanticModel) tuple, string name)
        {
            var (syntaxTree, semanticModel) = tuple;
            return (syntaxTree.GetMethod(name), semanticModel);
        }

        public static PropertyDeclarationSyntax GetProperty(this SyntaxTree syntaxTree, string name, int skip = 0) =>
            syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .Where(m => m.Identifier.ValueText == name)
                .Skip(skip)
                .First();
        public static (PropertyDeclarationSyntax, SemanticModel) GetProperty(this (SyntaxTree, SemanticModel) tuple, string name)
        {
            var (syntaxTree, semanticModel) = tuple;
            return (syntaxTree.GetProperty(name), semanticModel);
        }

        public static ConstructorDeclarationSyntax GetConstructor(this SyntaxTree syntaxTree, string name, int skip = 0) =>
            syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .Where(m => m.Identifier.ValueText == name)
                .Skip(skip)
                .First();

        public static (ConstructorDeclarationSyntax, SemanticModel) GetConstructor(this (SyntaxTree, SemanticModel) tuple, string name)
        {
            var (syntaxTree, semanticModel) = tuple;
            return (syntaxTree.GetConstructor(name), semanticModel);
        }

        public static IndexerDeclarationSyntax GetIndexer(this SyntaxTree syntaxTree) =>
            syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<IndexerDeclarationSyntax>()
                .First();

        public static (IndexerDeclarationSyntax, SemanticModel) GetIndexer(this (SyntaxTree, SemanticModel) tuple)
        {
            var (syntaxTree, semanticModel) = tuple;
            return (syntaxTree.GetIndexer(), semanticModel);
        }

        public static AccessorDeclarationSyntax GetAccessor(this SyntaxTree syntaxTree, string accessorKeyword) =>
           syntaxTree.GetRoot()
               .DescendantNodes()
               .OfType<AccessorDeclarationSyntax>()
               .First(m => m.Keyword.ValueText == accessorKeyword);

        public static (AccessorDeclarationSyntax, SemanticModel) GetAccessor(this (SyntaxTree, SemanticModel) tuple, string keyword)
        {
            var (syntaxTree, semanticModel) = tuple;
            return (syntaxTree.GetAccessor(keyword), semanticModel);
        }

        public static OperatorDeclarationSyntax GetOperator(this SyntaxTree syntaxTree) =>
           syntaxTree.GetRoot()
               .DescendantNodes()
               .OfType<OperatorDeclarationSyntax>()
               .First();

        public static (OperatorDeclarationSyntax, SemanticModel) GetOperator(this (SyntaxTree, SemanticModel) tuple)
        {
            var (syntaxTree, semanticModel) = tuple;
            return (syntaxTree.GetOperator(), semanticModel);
        }

        public static ConversionOperatorDeclarationSyntax GetConversionOperator(this SyntaxTree syntaxTree) =>
           syntaxTree.GetRoot()
               .DescendantNodes()
               .OfType<ConversionOperatorDeclarationSyntax>()
               .First();

        public static (ConversionOperatorDeclarationSyntax, SemanticModel) GetConversionOperator(this (SyntaxTree, SemanticModel) tuple)
        {
            var (syntaxTree, semanticModel) = tuple;
            return (syntaxTree.GetConversionOperator(), semanticModel);
        }

        public static DestructorDeclarationSyntax GetDestructor(this SyntaxTree syntaxTree) =>
            syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<DestructorDeclarationSyntax>()
                .First();

        public static (DestructorDeclarationSyntax, SemanticModel) GetDestructor(this (SyntaxTree, SemanticModel) tuple)
        {
            var (syntaxTree, semanticModel) = tuple;
            return (syntaxTree.GetDestructor(), semanticModel);
        }

        public static BaseTypeDeclarationSyntax GetType(this SyntaxTree syntaxTree, string name, int skip = 0) =>
            syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<BaseTypeDeclarationSyntax>()
                .Where(m => m.Identifier.ValueText == name)
                .Skip(skip)
                .First();

        public static IMethodSymbol GetMethodSymbol(this (SyntaxTree, SemanticModel) tuple, string name, int skip = 0)
        {
            var (syntaxTree, semanticModel) = tuple;
            return semanticModel.GetDeclaredSymbol(syntaxTree.GetMethod(name, skip));
        }

        public static INamedTypeSymbol GetNamedTypeSymbol(this (SyntaxTree, SemanticModel) tuple, string name, int skip = 0)
        {
            var (syntaxTree, semanticModel) = tuple;
            return semanticModel.GetDeclaredSymbol(syntaxTree.GetType(name, skip));
        }

        public static void AssertCollection<T>(IList<T> items, params Action<T>[] asserts)
        {
            items.Should().HaveSameCount(asserts);
            for (var i = 0; i < items.Count; i++)
            {
                asserts[i](items[i]);
            }
        }
    }
}

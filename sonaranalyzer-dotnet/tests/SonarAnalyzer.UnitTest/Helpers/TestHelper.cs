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

extern alias csharp;

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest
{
    internal static class TestHelper
    {
        public static (SyntaxTree, SemanticModel) Compile(string classDeclaration, bool isCSharp = true,
            params MetadataReference[] additionalReferences)
        {
            var language = isCSharp ? AnalyzerLanguage.CSharp : AnalyzerLanguage.VisualBasic;

            var compilation = SolutionBuilder
                .Create()
                .AddProject(language, createExtraEmptyFile: false)
                .AddSnippet(classDeclaration)
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

        public static PropertyDeclarationSyntax GetProperty(this SyntaxTree syntaxTree, string name, int skip = 0) =>
            syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .Where(m => m.Identifier.ValueText == name)
                .Skip(skip)
                .First();

        public static ConstructorDeclarationSyntax GetConstructor(this SyntaxTree syntaxTree, string name, int skip = 0) =>
            syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .Where(m => m.Identifier.ValueText == name)
                .Skip(skip)
                .First();

        public static IMethodSymbol GetMethodSymbol(this (SyntaxTree, SemanticModel) tuple, string name)
        {
            var (syntaxTree, semantcModel) = tuple;

            return semantcModel.GetDeclaredSymbol(syntaxTree.GetMethod(name));
        }

        public static (MethodDeclarationSyntax, SemanticModel) GetMethod(this (SyntaxTree, SemanticModel) tuple, string name)
        {
            var (syntaxTree, semantcModel) = tuple;

            return (syntaxTree.GetMethod(name), semantcModel);
        }

        public static (PropertyDeclarationSyntax, SemanticModel) GetProperty(this (SyntaxTree, SemanticModel) tuple, string name)
        {
            var (syntaxTree, semantcModel) = tuple;

            return (syntaxTree.GetProperty(name), semantcModel);
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

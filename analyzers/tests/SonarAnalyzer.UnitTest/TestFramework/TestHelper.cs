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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Moq;
using SonarAnalyzer.Common;
using SonarAnalyzer.UnitTest.PackagingTests;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest
{
    internal static class TestHelper
    {
        private const string ProjectConfigTemplate = @"
<SonarProjectConfig xmlns=""http://www.sonarsource.com/msbuild/analyzer/2021/1"">
    <FilesToAnalyzePath>{0}\FilesToAnalyze.txt</FilesToAnalyzePath>
</SonarProjectConfig>";

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

        public static bool IsSecurityHotspot(DiagnosticDescriptor diagnostic)
        {
            var key = diagnostic.Id.Substring(1);
            var type = CsRuleTypeMapping.RuleTypesCs.GetValueOrDefault(key) ?? VbRuleTypeMapping.RuleTypesVb.GetValueOrDefault(key);
            return type == "SECURITY_HOTSPOT";
        }

        public static AnalyzerOptions CreateOptions(string relativePath)
        {
            var text = File.Exists(relativePath) ? SourceText.From(File.ReadAllText(relativePath)) : null;
            var additionalText = new Mock<AdditionalText>();
            additionalText.Setup(x => x.Path).Returns(relativePath);
            additionalText.Setup(x => x.GetText(default)).Returns(text);
            return new AnalyzerOptions(ImmutableArray.Create(additionalText.Object));
        }

        public static string CreateSonarProjectConfig(string filesToAnalyzeDirectory)
        {
            var sonarProjectConfigPath = Path.Combine(filesToAnalyzeDirectory, "SonarProjectConfig.xml");
            File.WriteAllText(sonarProjectConfigPath, string.Format(ProjectConfigTemplate, filesToAnalyzeDirectory));
            return sonarProjectConfigPath;
        }
    }
}

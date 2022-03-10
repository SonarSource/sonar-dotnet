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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.PackagingTests;
using SonarAnalyzer.UnitTest.TestFramework;
using StyleCop.Analyzers.Lightup;
using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.UnitTest
{
    internal static class TestHelper
    {
        private const string ProjectConfigTemplate = @"
<SonarProjectConfig xmlns=""http://www.sonarsource.com/msbuild/analyzer/2021/1"">
    <{0}>{1}</{0}>
    <OutPath>{2}</OutPath>
</SonarProjectConfig>";

        public static (SyntaxTree Tree, SemanticModel Model) CompileIgnoreErrorsCS(string snippet, params MetadataReference[] additionalReferences) =>
            Compile(snippet, true, AnalyzerLanguage.CSharp, additionalReferences);

        public static (SyntaxTree Tree, SemanticModel Model) CompileIgnoreErrorsVB(string snippet, params MetadataReference[] additionalReferences) =>
            Compile(snippet, true, AnalyzerLanguage.VisualBasic, additionalReferences);

        public static (SyntaxTree Tree, SemanticModel Model) CompileCS(string snippet, params MetadataReference[] additionalReferences) =>
            Compile(snippet, false, AnalyzerLanguage.CSharp, additionalReferences);

        public static (SyntaxTree Tree, SemanticModel Model) CompileVB(string snippet, params MetadataReference[] additionalReferences) =>
            Compile(snippet, false, AnalyzerLanguage.VisualBasic, additionalReferences);

        public static (SyntaxTree Tree, SemanticModel Model) Compile(string snippet, bool ignoreErrors, AnalyzerLanguage language, params MetadataReference[] additionalReferences)
        {
            var compiled = new SnippetCompiler(snippet, ignoreErrors, language, additionalReferences);
            return (compiled.SyntaxTree, compiled.SemanticModel);
        }

        public static ControlFlowGraph CompileCfgBodyCS(string body = null, string additionalParameters = null) =>
            CompileCfgBodyCS(out _, body, additionalParameters);

        public static ControlFlowGraph CompileCfgBodyCS(out ISymbol methodSymbol, string body = null, string additionalParameters = null) =>
            CompileCfg($"public class Sample {{ public void Main({additionalParameters}) {{ {body} }} }}", AnalyzerLanguage.CSharp, out methodSymbol);

        public static ControlFlowGraph CompileCfgBodyVB(string body = null) =>
            CompileCfg(
$@"Public Class Sample
    Public Sub Main()
        {body}
    End Sub
End Class", AnalyzerLanguage.VisualBasic);

        public static ControlFlowGraph CompileCfgCS(out ISymbol methodSymbol, string snippet, bool ignoreErrors = false) =>
            CompileCfg(snippet, AnalyzerLanguage.CSharp, out methodSymbol, ignoreErrors);

        public static ControlFlowGraph CompileCfgCS(string snippet, bool ignoreErrors = false) =>
            CompileCfgCS(out _, snippet, ignoreErrors);

        public static ControlFlowGraph CompileCfg(string snippet, AnalyzerLanguage language, bool ignoreErrors = false) =>
            CompileCfg(snippet, language, out _, ignoreErrors);

        public static ControlFlowGraph CompileCfg(string snippet, AnalyzerLanguage language, out ISymbol methodsymbol, bool ignoreErrors = false)
        {
            var (tree, semanticModel) = Compile(snippet, ignoreErrors, language);
            var method = tree.GetRoot().DescendantNodes().First(IsMethod);
            methodsymbol = semanticModel.GetDeclaredSymbol(method);

            return ControlFlowGraph.Create(method, semanticModel);

            bool IsMethod(SyntaxNode node) =>
                language == AnalyzerLanguage.CSharp
                    ? node.RawKind == (int)CS.SyntaxKind.MethodDeclaration
                    : node.RawKind == (int)VB.SyntaxKind.FunctionBlock || node.RawKind == (int)VB.SyntaxKind.SubBlock;
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

        public static AccessorDeclarationSyntax GetAccessor(this SyntaxTree syntaxTree, string accessorKeyword) =>
           syntaxTree.GetRoot()
               .DescendantNodes()
               .OfType<AccessorDeclarationSyntax>()
               .First(m => m.Keyword.ValueText == accessorKeyword);

        public static ConversionOperatorDeclarationSyntax GetConversionOperator(this SyntaxTree syntaxTree) =>
           syntaxTree.GetRoot()
               .DescendantNodes()
               .OfType<ConversionOperatorDeclarationSyntax>()
               .First();

        public static DestructorDeclarationSyntax GetDestructor(this SyntaxTree syntaxTree) =>
            syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<DestructorDeclarationSyntax>()
                .First();

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

        public static bool IsSecurityHotspot(DiagnosticDescriptor diagnostic)
        {
            var key = diagnostic.Id.Substring(1);
            var type = CsRuleTypeMapping.RuleTypesCs.GetValueOrDefault(key) ?? VbRuleTypeMapping.RuleTypesVb.GetValueOrDefault(key);
            return type == "SECURITY_HOTSPOT";
        }

        public static IEnumerable<MetadataReference> ProjectTypeReference(ProjectType projectType) =>
            projectType == ProjectType.Test
                ? NuGetMetadataReference.MSTestTestFrameworkV1  // Any reference to detect a test project
                : Enumerable.Empty<MetadataReference>();

        public static string Serialize(IOperationWrapperSonar operation)
        {
            _ = operation ?? throw new ArgumentNullException(nameof(operation));
            return operation.Instance.Kind + ": " + operation.Instance.Syntax + (operation.IsImplicit ? " (Implicit)" : null);
        }

        public static AnalyzerOptions CreateOptions(string relativePath)
        {
            var text = File.Exists(relativePath) ? SourceText.From(File.ReadAllText(relativePath)) : null;

            return CreateOptions(relativePath, text);
        }

        public static AnalyzerOptions CreateOptions(string relativePath, SourceText text)
        {
            var additionalText = new Mock<AdditionalText>();
            additionalText.Setup(x => x.Path).Returns(relativePath);
            additionalText.Setup(x => x.GetText(default)).Returns(text);

            return new AnalyzerOptions(ImmutableArray.Create(additionalText.Object));
        }

        public static DiagnosticDescriptor CreateDescriptor(string id, params string[] customTags) =>
            new(id, "Title", "Message for " + id, "Category", DiagnosticSeverity.Warning, true, customTags: customTags);

        public static string ToUnixLineEndings(this string value) =>
            value.Replace(Constants.WindowsLineEnding, Constants.UnixLineEnding);

        public static string CreateFilesToAnalyze(string filesToAnalyzeDirectory, params string[] filesToAnalyze)
        {
            var filestoAnalyzePath = Path.Combine(filesToAnalyzeDirectory, "FilesToAnalyze.txt");
            File.WriteAllLines(filestoAnalyzePath, filesToAnalyze);
            return filestoAnalyzePath;
        }

        public static string TestPath(TestContext context, string fileName)
        {
            var path = Path.Combine(context.TestDir, context.FullyQualifiedTestClassName.Replace("SonarAnalyzer.UnitTest.", null), context.TestName, fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            return path;
        }

        public static string WriteFile(TestContext context, string fileName, string content = null)
        {
            var path = TestPath(context, fileName);
            File.WriteAllText(path, content);
            return path;
        }

        public static string CreateSonarProjectConfig(string sonarProjectConfigDirectory, string filesToAnalyzePath) =>
            CreateSonarProjectConfig(sonarProjectConfigDirectory, "FilesToAnalyzePath", filesToAnalyzePath, true);

        public static string CreateSonarProjectConfig(string testMethodName, ProjectType projectType, bool isScannerRun = true) =>
            CreateSonarProjectConfig(@"TestCases\" + testMethodName, "ProjectType", projectType.ToString(), isScannerRun);

        private static string CreateSonarProjectConfig(string directoryName, string element, string value, bool isScannerRun)
        {
            var directory = Directory.CreateDirectory(directoryName).FullName;
            var sonarProjectConfigPath = Path.Combine(directory, "SonarProjectConfig.xml");
            var projectConfigContent = string.Format(ProjectConfigTemplate, element, value, isScannerRun ? directory : null);
            File.WriteAllText(sonarProjectConfigPath, projectConfigContent);
            return sonarProjectConfigPath;
        }
    }
}

/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.CFG;
using SonarAnalyzer.CFG.Extensions;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.TestFramework.Build;
using StyleCop.Analyzers.Lightup;
using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.TestFramework.Common;

public static class TestCompiler
{
    public static (SyntaxTree Tree, SemanticModel Model) CompileIgnoreErrorsCS(string snippet, params MetadataReference[] additionalReferences) =>
        Compile(snippet, true, AnalyzerLanguage.CSharp, additionalReferences);

    public static (SyntaxTree Tree, SemanticModel Model) CompileIgnoreErrorsVB(string snippet, params MetadataReference[] additionalReferences) =>
        Compile(snippet, true, AnalyzerLanguage.VisualBasic, additionalReferences);

    public static (SyntaxTree Tree, SemanticModel Model) CompileCS(string snippet, params MetadataReference[] additionalReferences) =>
        Compile(snippet, false, AnalyzerLanguage.CSharp, additionalReferences);

    public static (SyntaxTree Tree, SemanticModel Model) CompileVB(string snippet, params MetadataReference[] additionalReferences) =>
        Compile(snippet, false, AnalyzerLanguage.VisualBasic, additionalReferences);

    public static (SyntaxTree Tree, SemanticModel Model) Compile(string snippet,
                                                                 bool ignoreErrors,
                                                                 AnalyzerLanguage language,
                                                                 MetadataReference[] additionalReferences = null,
                                                                 OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
                                                                 ParseOptions parseOptions = null)
    {
        var compiled = new SnippetCompiler(snippet, ignoreErrors, language, additionalReferences, outputKind, parseOptions);
        return (compiled.SyntaxTree, compiled.SemanticModel);
    }

    // Get the SyntaxNode between the markers $$ in the snippet
    public static SyntaxNode NodeBetweenMarkersCS(string snippet, bool getInnermostNodeForTie = false, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary) =>
        NodeBetweenMarkers(snippet, AnalyzerLanguage.CSharp, getInnermostNodeForTie, outputKind);

    // Get the SyntaxNode between the markers $$ in the snippet
    public static SyntaxNode NodeBetweenMarkersVB(string snippet, bool getInnermostNodeForTie = false, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary) =>
        NodeBetweenMarkers(snippet, AnalyzerLanguage.VisualBasic, getInnermostNodeForTie, outputKind);

    // Get the SyntaxToken between the markers $$ in the snippet
    public static SyntaxToken TokenBetweenMarkersCS(string snippet, bool getInnermostNodeForTie = false, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary) =>
        TokenBetweenMarkers(snippet, AnalyzerLanguage.CSharp, getInnermostNodeForTie, outputKind);

    // Get the SyntaxToken between the markers $$ in the snippet
    public static SyntaxToken TokenBetweenMarkersVB(string snippet, bool getInnermostNodeForTie = false, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary) =>
        TokenBetweenMarkers(snippet, AnalyzerLanguage.VisualBasic, getInnermostNodeForTie, outputKind);

    public static ControlFlowGraph CompileCfgBodyCS(string body = null, string additionalParameters = null) =>
        CompileCfg($$"""
            public class Sample
            {
                public void Main({{additionalParameters}})
                {
                    {{body}}
                }
            }
            """, AnalyzerLanguage.CSharp);

    public static ControlFlowGraph CompileCfgBodyVB(string body = null) =>
        CompileCfg($"""
            Public Class Sample
                Public Sub Main()
                    {body}
                End Sub
            End Class
            """, AnalyzerLanguage.VisualBasic);

    public static ControlFlowGraph CompileCfgCS(string snippet, bool ignoreErrors = false) =>
        CompileCfg(snippet, AnalyzerLanguage.CSharp, ignoreErrors);

    public static ControlFlowGraph CompileCfg(string snippet,
                                              AnalyzerLanguage language,
                                              bool ignoreErrors = false,
                                              string localFunctionName = null,
                                              string anonymousFunctionFragment = null,
                                              OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary)
    {
        var (tree, semanticModel) = Compile(snippet, ignoreErrors, language, outputKind: outputKind);
        var root = tree.GetRoot();
        var method = outputKind == OutputKind.ConsoleApplication && root.ChildNodes().OfType<GlobalStatementSyntax>().Any()
            ? root                                      // Top level statements
            : root.DescendantNodes().First(IsMethod);
        var cfg = ControlFlowGraph.Create(method, semanticModel, default);
        if (localFunctionName is not null && anonymousFunctionFragment is not null)
        {
            throw new InvalidOperationException($"Specify {nameof(localFunctionName)} or {nameof(anonymousFunctionFragment)}.");
        }
        if (localFunctionName is not null)
        {
            cfg = cfg.GetLocalFunctionControlFlowGraph(cfg.LocalFunctions.Single(x => x.Name == localFunctionName), default);
        }
        else if (anonymousFunctionFragment is not null)
        {
            var anonymousFunction = cfg.FlowAnonymousFunctionOperations().SingleOrDefault(x => x.WrappedOperation.Syntax.ToString().Contains(anonymousFunctionFragment));
            if (anonymousFunction.WrappedOperation is null)
            {
                throw new ArgumentException($"Anonymous function with '{anonymousFunctionFragment}' fragment was not found.");
            }
            cfg = cfg.GetAnonymousFunctionControlFlowGraph(anonymousFunction, default);
        }

        const string Separator = "----------";
        Console.WriteLine(Separator);
        Console.Write(CfgSerializer.Serialize(cfg));
        Console.WriteLine(Separator);

        return cfg;

        bool IsMethod(SyntaxNode node) =>
            language == AnalyzerLanguage.CSharp
                ? node.RawKind == (int)CS.SyntaxKind.MethodDeclaration
                : node.RawKind == (int)VB.SyntaxKind.FunctionBlock || node.RawKind == (int)VB.SyntaxKind.SubBlock;
    }

    public static IEnumerable<MetadataReference> ProjectTypeReference(ProjectType projectType) =>
        projectType == ProjectType.Test
            ? NuGetMetadataReference.MSTestTestFrameworkV1  // Any reference to detect a test project
            : Enumerable.Empty<MetadataReference>();

    public static string Serialize(IOperationWrapperSonar operation)
    {
        _ = operation.Instance ?? throw new ArgumentNullException(nameof(operation));
        return operation.Instance.Kind + ": " + operation.Instance.Syntax + (operation.IsImplicit ? " (Implicit)" : null);
    }

    private static SyntaxNode NodeBetweenMarkers(string snippet, AnalyzerLanguage language, bool getInnermostNodeForTie = false, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary)
    {
        var position = snippet.IndexOf("$$");
        var lastPosition = snippet.LastIndexOf("$$");
        var length = lastPosition == position ? 0 : lastPosition - position - "$$".Length;
        snippet = snippet.Replace("$$", string.Empty);
        var (tree, _) = Compile(snippet, ignoreErrors: false, language, outputKind: outputKind);
        var node = tree.GetRoot().FindNode(new TextSpan(position, length), getInnermostNodeForTie: getInnermostNodeForTie);
        return node;
    }

    private static SyntaxToken TokenBetweenMarkers(string snippet, AnalyzerLanguage language, bool findInsideTrivia = false, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary)
    {
        var position = snippet.IndexOf("$$");
        snippet = snippet.Replace("$$", string.Empty);
        var (tree, _) = TestCompiler.Compile(snippet, ignoreErrors: false, language, outputKind: outputKind);
        return tree.GetRoot().FindToken(position, findInsideTrivia);
    }
}

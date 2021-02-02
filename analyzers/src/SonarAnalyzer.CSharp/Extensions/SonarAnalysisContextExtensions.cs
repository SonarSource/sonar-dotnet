using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.ControlFlowGraph.CSharp;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.LiveVariableAnalysis.CSharp;
using SonarAnalyzer.ShimLayer.CSharp;
using SonarAnalyzer.SymbolicExecution;

namespace SonarAnalyzer.Extensions
{
    internal static class SonarAnalysisContextExtensions
    {
        public static void RegisterExplodedGraphBasedAnalysis(this SonarAnalysisContext context, Action<CSharpExplodedGraph, SyntaxNodeAnalysisContext> analyze)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var declaration = (BaseMethodDeclarationSyntax)c.Node;
                    var symbol = c.SemanticModel.GetDeclaredSymbol(declaration);
                    if (symbol == null)
                    {
                        return;
                    }

                    Analyze(declaration.Body, symbol, analyze, c);
                    Analyze(declaration.ExpressionBody(), symbol, analyze, c);
                },
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.DestructorDeclaration,
                SyntaxKind.ConversionOperatorDeclaration,
                SyntaxKind.OperatorDeclaration,
                SyntaxKind.MethodDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var declaration = (PropertyDeclarationSyntax)c.Node;
                    var symbol = c.SemanticModel.GetDeclaredSymbol(declaration);
                    if (symbol == null)
                    {
                        return;
                    }

                    Analyze(declaration.ExpressionBody, symbol, analyze, c);
                },
                SyntaxKind.PropertyDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var declaration = (AccessorDeclarationSyntax)c.Node;
                    var symbol = c.SemanticModel.GetDeclaredSymbol(declaration);
                    if (symbol == null)
                    {
                        return;
                    }

                    Analyze(declaration.Body, symbol, analyze, c);
                    Analyze(declaration.ExpressionBody(), symbol, analyze, c);
                },
                SyntaxKind.GetAccessorDeclaration,
                SyntaxKind.SetAccessorDeclaration,
                SyntaxKind.AddAccessorDeclaration,
                SyntaxKind.RemoveAccessorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var declaration = (AnonymousFunctionExpressionSyntax)c.Node;
                    var symbol = c.SemanticModel.GetSymbolInfo(declaration).Symbol;
                    if (symbol == null)
                    {
                        return;
                    }

                    Analyze(declaration.Body, symbol, analyze, c);
                },
                SyntaxKind.AnonymousMethodExpression,
                SyntaxKind.SimpleLambdaExpression,
                SyntaxKind.ParenthesizedLambdaExpression);
        }

        private static void Analyze(CSharpSyntaxNode declarationBody, ISymbol symbol, Action<CSharpExplodedGraph, SyntaxNodeAnalysisContext> analyze, SyntaxNodeAnalysisContext context)
        {
            if (declarationBody == null ||
                declarationBody.ContainsDiagnostics)
            {
                return;
            }

            if (!CSharpControlFlowGraph.TryGet(declarationBody, context.SemanticModel, out var cfg))
            {
                return;
            }

            var lva = CSharpLiveVariableAnalysis.Analyze(cfg, symbol, context.SemanticModel);

            try
            {
                var explodedGraph = new CSharpExplodedGraph(cfg, symbol, context.SemanticModel, lva);
                analyze(explodedGraph, context);
            }
            catch (Exception e)
            {
                // Roslyn/MSBuild is currently cutting exception message at the end of the line instead
                // of displaying the full message. As a workaround, we replace the line ending with ' ## '.
                // See https://github.com/dotnet/roslyn/issues/1455 and https://github.com/dotnet/roslyn/issues/24346
                var sb = new StringBuilder();
                sb.AppendLine($"Error processing method: {symbol?.Name ?? "{unknown}"}");
                sb.AppendLine($"Method file: {declarationBody.GetLocation()?.GetLineSpan().Path ?? "{unknown}"}");
                sb.AppendLine($"Method line: {declarationBody.GetLocation()?.GetLineSpan().StartLinePosition.ToString() ?? "{unknown}"}");
                sb.AppendLine($"Inner exception: {e}");

                throw new SymbolicExecutionException(sb.ToString().Replace(Environment.NewLine, " ## "), e);
            }
        }
    }
}

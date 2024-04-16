/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using Microsoft.CodeAnalysis.Shared.Extensions;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseAwaitableMethod : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6966";
    private const string MessageFormat = "FIXME";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCodeBlockStartAction<SyntaxKind>(CSharpGeneratedCodeRecognizer.Instance, codeBlockStart =>
            {
                if (IsAsyncCodeBlock(codeBlockStart.CodeBlock))
                {
                    var codeBlock = codeBlockStart.CodeBlock;
                    codeBlockStart.RegisterNodeAction(nodeContext =>
                    {
                        var (invocationExpression, semanticModel) = ((InvocationExpressionSyntax)nodeContext.Node, nodeContext.SemanticModel);

                        var awaitableRoot = GetAwaitableRootOfInvocation(invocationExpression);
                        if (awaitableRoot is AwaitExpressionSyntax)
                        {
                            return; // Invocation result is already awaited.
                        }
                        if (nodeContext.SemanticModel.GetSymbolInfo(nodeContext.Node, nodeContext.Cancel).Symbol is IMethodSymbol methodSymbol)
                        {
                            if (methodSymbol.IsAsync || methodSymbol.ReturnType.OriginalDefinition.IsAny(KnownType.SystemTasks))
                            {
                                return; // The invoked method returns something awaitable (but it isn't awaited).
                            }
                            if (methodSymbol.ContainingSymbol is ITypeSymbol type
                                && type.GetMembers() is { } members)
                            {
                                var asyncCandidates = members.OfType<IMethodSymbol>().Where(x =>
                                    x.Name.Equals($"{methodSymbol.Name}Async", StringComparison.OrdinalIgnoreCase)
                                    && x.IsAwaitableNonDynamic(semanticModel, invocationExpression.SpanStart));
                                var awaitableAlternatives = FindAwaitableAlternatives(nodeContext.SemanticModel, methodSymbol, codeBlock, awaitableRoot, invocationExpression, asyncCandidates);
                                if (awaitableAlternatives.Any())
                                {
                                    nodeContext.ReportIssue(Rule, invocationExpression);
                                }
                            }
                        }
                    }, SyntaxKind.InvocationExpression);
                }
            });
    private IEnumerable<ISymbol> FindAwaitableAlternatives(SemanticModel semanticModel, IMethodSymbol methodSymbol, SyntaxNode codeBlock, ExpressionSyntax awaitableRoot, InvocationExpressionSyntax invocationExpression, IEnumerable<IMethodSymbol> members) =>
        members.Where(x => IsAwaitableAlternative(semanticModel, x, codeBlock, awaitableRoot, invocationExpression));

    private bool IsAwaitableAlternative(SemanticModel semanticModel, IMethodSymbol candidate, SyntaxNode codeBlock, ExpressionSyntax awaitableRoot, InvocationExpressionSyntax invocationExpression)
    {
        var root = codeBlock.SyntaxTree.GetRoot();
        var invocationExpressionNew = SyntaxFactory.IdentifierName(candidate.Name);
        var invocationAnnotation = new SyntaxAnnotation();
        var replace = root.ReplaceNodes([codeBlock, awaitableRoot, invocationExpression], (original, newNode) =>
        {
            if (original == codeBlock)
            {
                newNode = newNode;
            }
            if (original == invocationExpression)
            {
                newNode = SyntaxFactory.InvocationExpression(invocationExpressionNew, invocationExpression.ArgumentList).WithAdditionalAnnotations(invocationAnnotation);
            }
            if (original == awaitableRoot)
            {
                newNode = SyntaxFactory.AwaitExpression(newNode as ExpressionSyntax);
            }
            return newNode;
        });
        var invocationReplaced = replace.GetAnnotatedNodes(invocationAnnotation).First();
        var speculativeSymbol = semanticModel.GetSpeculativeSymbolInfo(invocationReplaced.SpanStart, invocationReplaced, SpeculativeBindingOption.BindAsExpression);
        return candidate.Equals(speculativeSymbol.Symbol);
    }

    private static ExpressionSyntax GetAwaitableRootOfInvocation(ExpressionSyntax expression) =>
        expression?.Parent switch
        {
            ConditionalAccessExpressionSyntax conditional => conditional.GetRootConditionalAccessExpression(),
            ExpressionSyntax parent => parent,
            ExpressionStatementSyntax expressionStatement => expressionStatement.Expression,
            _ => null,
        };

    private static bool IsAsyncCodeBlock(SyntaxNode codeBlock)
    =>
        codeBlock switch
        {
            MethodDeclarationSyntax { Modifiers: { } modifiers } => modifiers.AnyOfKind(SyntaxKind.AsyncKeyword),
            AnonymousMethodExpressionSyntax { AsyncKeyword.RawKind: (int)SyntaxKind.AsyncKeyword } => true,
            LambdaExpressionSyntax { AsyncKeyword.RawKind: (int)SyntaxKind.AsyncKeyword } => true,
            { } localFunction when LocalFunctionStatementSyntaxWrapper.IsInstance(localFunction) => ((LocalFunctionStatementSyntaxWrapper)localFunction).Modifiers.AnyOfKind(SyntaxKind.AsyncKeyword),
            _ => false,
        };
}

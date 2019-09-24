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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ConsumeValueTaskCorrectly : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S5034";

        // 'await', 'AsTask', 'Result' and '.GetAwaiter().GetResult()' should be called only once on a ValueTask
        private const string MessageFormat = "Refactor this 'ValueTask' usage to consume it only once.";

        // This should be called only when 'readTask.IsCompletedSuccessfully' is not called before
        private const string MessageFormatResult = "Refactor this 'ValueTask' usage to consume the result only if the operation has completed successfully.";

        private static readonly DiagnosticDescriptor messageOnlyOnce =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        private static readonly DiagnosticDescriptor messageOnlyIfCompletedSuccessfully =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        private static readonly KnownType[] ValueTaskTypes =
            new[] { KnownType.System_Threading_Tasks_ValueTask, KnownType.System_Threading_Tasks_ValueTask_TResult };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(messageOnlyOnce, messageOnlyIfCompletedSuccessfully);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var walker = new ConsumeValueTaskWalker(c.SemanticModel);
                    walker.Visit(c.Node);

                    foreach (var syntaxNodes in walker.SymbolUsages.Values)
                    {
                        if (syntaxNodes.Count() > 1)
                        {
                            c.ReportDiagnosticWhenActive(Diagnostic.Create(messageOnlyOnce, syntaxNodes.First().GetLocation(),
                                additionalLocations: syntaxNodes.Skip(1).Select(node => node.GetLocation()).ToArray()));
                        }
                    }

                    foreach (var node in walker.ConsumedButNotCompleted)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(messageOnlyIfCompletedSuccessfully, node.GetLocation()));
                    }
                },
                // should also check in properties?
                SyntaxKind.MethodDeclaration);
        }

        private class ConsumeValueTaskWalker : CSharpSyntaxWalker
        {
            private readonly SemanticModel semanticModel;

            // The key is the 'ValueTask' variable symbol, the value is a list of nodes where it is consumed
            public Dictionary<ISymbol, IList<SyntaxNode>> SymbolUsages { get; }

            // A list of 'ValueTask' nodes on which '.Result' or '.GetAwaiter().GetResult()' has been invoked when the operation has not yet completed
            public List<SyntaxNode> ConsumedButNotCompleted { get; }

            public ConsumeValueTaskWalker(SemanticModel semanticModel)
            {
                this.semanticModel = semanticModel;
                SymbolUsages = new Dictionary<ISymbol, IList<SyntaxNode>>();
                ConsumedButNotCompleted = new List<SyntaxNode>();
            }

            /**
             * Check if 'await' is done on a 'ValueTask'
             */
            public override void VisitAwaitExpression(AwaitExpressionSyntax node)
            {
                if (node.Expression is IdentifierNameSyntax identifierName &&
                    this.semanticModel.GetSymbolInfo(identifierName).Symbol is ISymbol symbol &&
                    symbol.GetSymbolType().OriginalDefinition.IsAny(ValueTaskTypes))
                {
                    AddToSymbolUsages(symbol, identifierName);
                }

                base.VisitAwaitExpression(node);
            }

            /**
             * Check if it's the wanted method on a ValueTask
             * - we treat AsTask() like await - always add it to the list
             * - for GetAwaiter().GetResult() - ignore the call if it's called inside an 'if (valueTask.IsCompletedSuccessfully)'
             */
            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (node.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    if (memberAccess.NameIs("AsTask") &&
                        memberAccess.Expression is IdentifierNameSyntax identifierName &&
                        this.semanticModel.GetSymbolInfo(identifierName).Symbol is ISymbol symbol &&
                        symbol.GetSymbolType().OriginalDefinition.IsAny(ValueTaskTypes))
                    {
                        AddToSymbolUsages(symbol, identifierName);
                    }

                    if (memberAccess.NameIs("GetResult") &&
                        memberAccess.Expression is InvocationExpressionSyntax invocation &&
                        invocation.Expression is MemberAccessExpressionSyntax innerMemberAccess &&
                        innerMemberAccess.Expression is IdentifierNameSyntax leftMostIdentifier &&
                        this.semanticModel.GetSymbolInfo(leftMostIdentifier).Symbol is ISymbol leftMostSymbol &&
                        leftMostSymbol.GetSymbolType().OriginalDefinition.IsAny(ValueTaskTypes) &&
                        !IsCompletedSuccessfullyHasBeenCheckedInParent(leftMostSymbol, leftMostIdentifier))
                    {
                        AddToSymbolUsages(leftMostSymbol, leftMostIdentifier);
                        ConsumedButNotCompleted.Add(leftMostIdentifier);
                    }
                }

                base.VisitInvocationExpression(node);
            }

            /**
             * Check if ".Result" is accessed on a 'ValueTask'
             * - ignore the call if it's called inside an 'if (valueTask.IsCompletedSuccessfully)'
             */
            public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
            {
                if (node.NameIs("Result") &&
                    node.Expression is IdentifierNameSyntax identifierName &&
                    this.semanticModel.GetSymbolInfo(identifierName).Symbol is ISymbol symbol &&
                    symbol.GetSymbolType().OriginalDefinition.IsAny(ValueTaskTypes) &&
                    !IsCompletedSuccessfullyHasBeenCheckedInParent(symbol, identifierName))
                {
                    AddToSymbolUsages(symbol, identifierName);
                    ConsumedButNotCompleted.Add(identifierName);
                }

                base.VisitMemberAccessExpression(node);
            }

            private bool IsCompletedSuccessfullyHasBeenCheckedInParent(ISymbol symbol, SyntaxNode node)
            {
                var ancestor = node.Parent;
                var hasCheck = false;
                while (ancestor != null)
                {
                    if (ancestor is IfStatementSyntax statementSyntax)
                    {
                        hasCheck = statementSyntax.Condition.DescendantNodesAndSelf().Any(n =>
                             n is MemberAccessExpressionSyntax maes &&
                             maes.Name.Identifier.ValueText == "IsCompletedSuccessfully" &&
                             maes.Expression is IdentifierNameSyntax maesId &&
                             this.semanticModel.GetSymbolInfo(maesId).Symbol == symbol);
                        break;
                    }
                    ancestor = ancestor.Parent;
                }

                return hasCheck;
            }

            private void AddToSymbolUsages(ISymbol symbol, SyntaxNode syntaxNode)
            {
                if (SymbolUsages.TryGetValue(symbol, out var syntaxNodes))
                {
                    syntaxNodes.Add(syntaxNode);
                }
                else
                {
                    SymbolUsages.Add(symbol, new List<SyntaxNode>() { syntaxNode });
                }
            }

        }
    }
}


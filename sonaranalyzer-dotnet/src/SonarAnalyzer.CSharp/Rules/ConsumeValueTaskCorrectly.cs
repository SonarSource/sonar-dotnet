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
        private const string MessageFormatResult = " Refactor this 'ValueTask' usage to consume the result only if the operation has completed successfully.";

        private static readonly DiagnosticDescriptor messageOnlyOnce =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        private static readonly DiagnosticDescriptor messageOnlyIfCompletedSuccessfully =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        private static readonly KnownType[] ValueTaskTypes =
            new[] { KnownType.System_Threading_Tasks_ValueTask, KnownType.System_Threading_Tasks_ValueTask_TResult };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(messageOnlyIfCompletedSuccessfully);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var node = c.Node;
                    var walker = new ConsumeValueTaskWalker(c.SemanticModel);
                    walker.Visit(node);
                    foreach (var keyValue in walker.SymbolUsages)
                    {
                        var syntaxNodes = keyValue.Value;
                        if (syntaxNodes.Count() > 1)
                        {
                            c.ReportDiagnosticWhenActive(Diagnostic.Create(messageOnlyOnce, syntaxNodes.First().GetLocation(),
                                additionalLocations: syntaxNodes.Skip(1).Select(sn => sn.GetLocation()).ToArray()));
                        }
                    }
                },
                // should also check in properties?
                SyntaxKind.MethodDeclaration);
        }

        private class ConsumeValueTaskWalker : CSharpSyntaxWalker
        {
            private readonly SemanticModel semanticModel;

            public Dictionary<ISymbol, IList<SyntaxNode>> SymbolUsages { get; }

            public ConsumeValueTaskWalker(SemanticModel semanticModel)
            {
                this.semanticModel = semanticModel;
                this.SymbolUsages = new Dictionary<ISymbol, IList<SyntaxNode>>();
            }

            public override void VisitAwaitExpression(AwaitExpressionSyntax node)
            {
                if (node.Expression is IdentifierNameSyntax identifierName &&
                    semanticModel.GetSymbolInfo(identifierName).Symbol is ISymbol symbol &&
                    symbol.GetSymbolType().OriginalDefinition.IsAny(ValueTaskTypes))
                {
                    if (SymbolUsages.TryGetValue(symbol, out var syntaxNodes))
                    {
                        syntaxNodes.Add(identifierName);
                    }
                    else
                    {
                        SymbolUsages.Add(symbol, new List<SyntaxNode>() { identifierName });
                    }
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
                    if (memberAccess.Name.Identifier.ValueText == "AsTask" &&
                        memberAccess.Expression is IdentifierNameSyntax identifierName &&
                        semanticModel.GetSymbolInfo(identifierName).Symbol is ISymbol symbol &&
                        symbol.GetSymbolType().OriginalDefinition.IsAny(ValueTaskTypes))
                    {
                        if (SymbolUsages.TryGetValue(symbol, out var syntaxNodes))
                        {
                            syntaxNodes.Add(identifierName);
                        }
                        else
                        {
                            SymbolUsages.Add(symbol, new List<SyntaxNode>() { identifierName });
                        }
                    }

                    if (memberAccess.Name.Identifier.ValueText == "GetResult" &&
                        memberAccess.Expression is InvocationExpressionSyntax invocation &&
                        invocation.Expression is MemberAccessExpressionSyntax innerMemberAccess &&
                        innerMemberAccess.Expression is IdentifierNameSyntax leftMostIdentifier &&
                        semanticModel.GetSymbolInfo(leftMostIdentifier).Symbol is ISymbol leftMostSymbol &&
                        leftMostSymbol.GetSymbolType().OriginalDefinition.IsAny(ValueTaskTypes))
                    {
                        var ancestor = memberAccess.Parent;
                        bool hasCheck = false;
                        while (ancestor != null)
                        {
                            if (ancestor is IfStatementSyntax statementSyntax)
                            {
                                hasCheck = statementSyntax.Condition.DescendantNodesAndSelf().Any(n =>
                                     n is MemberAccessExpressionSyntax maes &&
                                     maes.Name.Identifier.ValueText == "IsCompletedSuccessfully" &&
                                     maes.Expression is IdentifierNameSyntax maesId &&
                                     semanticModel.GetSymbolInfo(maesId).Symbol == leftMostSymbol);
                                break;
                            }
                            ancestor = ancestor.Parent;
                        }

                        if (!hasCheck)
                        {
                            if (SymbolUsages.TryGetValue(leftMostSymbol, out var syntaxNodes))
                            {
                                syntaxNodes.Add(leftMostIdentifier);
                            }
                            else
                            {
                                SymbolUsages.Add(leftMostSymbol, new List<SyntaxNode>() { leftMostIdentifier });
                            }
                        }
                    }
                }


                base.VisitInvocationExpression(node);
            }

            public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
            {
                // check if it's the wanted method on a ValueTask
                // do stuff
                // - Result -> also check if inside an if (readTask.IsCompletedSuccessfully) and ignore if so

                base.VisitMemberAccessExpression(node);
            }

        }
}
}


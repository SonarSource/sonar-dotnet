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
    public sealed class UninvokedEventDeclaration : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3264";
        private const string MessageFormat = "Remove the unused event '{0}' or invoke it.";

        private static readonly Accessibility maxAccessibility = Accessibility.Public;

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ISet<SyntaxKind> eventSyntax = new HashSet<SyntaxKind>
        {
            SyntaxKind.EventFieldDeclaration,
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSymbolAction(RaiseOnUninvokedEventDeclaration, SymbolKind.NamedType);
        }

        private void RaiseOnUninvokedEventDeclaration(SymbolAnalysisContext context)
        {
            var namedType = (INamedTypeSymbol)context.Symbol;
            if (!namedType.IsClassOrStruct() ||
                namedType.ContainingType != null)
            {
                return;
            }

            var removableDeclarationCollector = new CSharpRemovableDeclarationCollector(namedType, context.Compilation);

            var removableEventFields = removableDeclarationCollector
                .GetRemovableFieldLikeDeclarations(eventSyntax, maxAccessibility)
                .ToList();

            if (!removableEventFields.Any())
            {
                return;
            }

            var invokedSymbols = GetInvokedEventSymbols(removableDeclarationCollector);
            var possiblyCopiedSymbols = GetPossiblyCopiedSymbols(removableDeclarationCollector);

            removableEventFields
                .Where(IsNotInvoked)
                .Where(IsNotCopied)
                .ToList()
                .ForEach(x => context.ReportDiagnosticIfNonGenerated(
                    Diagnostic.Create(rule, GetLocation(x.SyntaxNode), x.Symbol.Name)));

            Location GetLocation(SyntaxNode node) =>
                node is VariableDeclaratorSyntax variableDeclarator
                    ? variableDeclarator.Identifier.GetLocation()
                    : ((EventDeclarationSyntax)node).Identifier.GetLocation();

            bool IsNotInvoked(SyntaxNodeSymbolSemanticModelTuple<SyntaxNode, ISymbol> tuple) =>
                !invokedSymbols.Contains(tuple.Symbol);

            bool IsNotCopied(SyntaxNodeSymbolSemanticModelTuple<SyntaxNode, ISymbol> tuple) =>
                !possiblyCopiedSymbols.Contains(tuple.Symbol);
        }

        private static ISet<ISymbol> GetInvokedEventSymbols(CSharpRemovableDeclarationCollector removableDeclarationCollector)
        {
            var delegateInvocations = removableDeclarationCollector.TypeDeclarations
                .SelectMany(container => container.SyntaxNode.DescendantNodes()
                    .Where(node => node.IsKind(SyntaxKind.InvocationExpression))
                    .Cast<InvocationExpressionSyntax>()
                    .Select(node =>
                        new SyntaxNodeSymbolSemanticModelTuple<InvocationExpressionSyntax, IMethodSymbol>
                        {
                            SyntaxNode = node,
                            SemanticModel = container.SemanticModel,
                            Symbol = container.SemanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol
                        }))
                 .Where(tuple =>
                    tuple.Symbol != null &&
                    IsDelegateInvocation(tuple.Symbol));

            var invokedEventSymbols = delegateInvocations
                .Select(tuple =>
                    new SyntaxNodeAndSemanticModel<ExpressionSyntax>
                    {
                        SyntaxNode = GetEventExpressionFromInvocation(tuple.SyntaxNode, tuple.Symbol),
                        SemanticModel = tuple.SemanticModel
                    })
                .Select(tuple =>
                    new SyntaxNodeSymbolSemanticModelTuple<ExpressionSyntax, IEventSymbol>
                    {
                        SyntaxNode = tuple.SyntaxNode,
                        SemanticModel = tuple.SemanticModel,
                        Symbol = tuple.SemanticModel.GetSymbolInfo(tuple.SyntaxNode).Symbol as IEventSymbol
                    })
                .Where(tuple => tuple.Symbol != null)
                .Select(tuple => tuple.Symbol.OriginalDefinition);

            return new HashSet<ISymbol>(invokedEventSymbols);
        }

        private static ISet<ISymbol> GetPossiblyCopiedSymbols(CSharpRemovableDeclarationCollector removableDeclarationCollector)
        {
            var usedSymbols = new HashSet<ISymbol>();

            var arguments = removableDeclarationCollector.TypeDeclarations
                .SelectMany(container => container.SyntaxNode.DescendantNodes()
                    .Where(node =>
                        node.IsKind(SyntaxKind.Argument))
                    .Cast<ArgumentSyntax>()
                    .Select(node =>
                        new SyntaxNodeAndSemanticModel<SyntaxNode>
                        {
                            SyntaxNode = node.Expression,
                            SemanticModel = container.SemanticModel
                        }));

            var equalsValue = removableDeclarationCollector.TypeDeclarations
                .SelectMany(container => container.SyntaxNode.DescendantNodes()
                    .OfType<EqualsValueClauseSyntax>()
                    .Select(node =>
                        new SyntaxNodeAndSemanticModel<SyntaxNode>
                        {
                            SyntaxNode = node.Value,
                            SemanticModel = container.SemanticModel
                        }));

            var assignment = removableDeclarationCollector.TypeDeclarations
                .SelectMany(container => container.SyntaxNode.DescendantNodes()
                    .Where(node =>
                        node.IsKind(SyntaxKind.SimpleAssignmentExpression))
                    .Cast<AssignmentExpressionSyntax>()
                    .Select(node =>
                        new SyntaxNodeAndSemanticModel<SyntaxNode>
                        {
                            SyntaxNode = node.Right,
                            SemanticModel = container.SemanticModel
                        }));

            var allNodes = arguments
                .Concat(equalsValue)
                .Concat(assignment);

            foreach (var node in allNodes)
            {

                if (node.SemanticModel.GetSymbolInfo(node.SyntaxNode).Symbol is IEventSymbol symbol)
                {
                    usedSymbols.Add(symbol.OriginalDefinition);
                }
            }

            return usedSymbols;
        }

        private static ExpressionSyntax GetEventExpressionFromInvocation(InvocationExpressionSyntax invocation, IMethodSymbol symbol)
        {
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            var memberBinding = invocation.Expression as MemberBindingExpressionSyntax;

            var expression = memberAccess?.Expression;
            var invokedMethodName = memberAccess?.Name;
            if (memberBinding != null)
            {
                expression = (invocation.Parent as ConditionalAccessExpressionSyntax)?.Expression;
                invokedMethodName = memberBinding.Name;
            }

            if ((memberAccess != null || memberBinding != null) &&
                expression != null &&
                IsExplicitDelegateInvocation(symbol, invokedMethodName))
            {
                return expression;
            }

            return invocation.Expression;
        }

        private static bool IsExplicitDelegateInvocation(IMethodSymbol symbol, SimpleNameSyntax invokedMethodName)
        {
            if (IsDynamicInvoke(symbol) ||
                IsBeginInvoke(symbol))
            {
                return true;
            }

            return symbol.MethodKind == MethodKind.DelegateInvoke && invokedMethodName.Identifier.ValueText == "Invoke";
        }

        private static bool IsDelegateInvocation(IMethodSymbol symbol) =>
            symbol.MethodKind == MethodKind.DelegateInvoke ||
            IsInvoke(symbol) ||
            IsDynamicInvoke(symbol) ||
            IsBeginInvoke(symbol);

        private static bool IsInvoke(IMethodSymbol symbol) =>
            symbol.MethodKind == MethodKind.Ordinary &&
            symbol.Name == nameof(EventHandler.Invoke);

        private static bool IsDynamicInvoke(IMethodSymbol symbol) =>
            symbol.MethodKind == MethodKind.Ordinary &&
            symbol.Name == nameof(Delegate.DynamicInvoke) &&
            symbol.ReceiverType.OriginalDefinition.Is(KnownType.System_Delegate);

        private static bool IsBeginInvoke(IMethodSymbol symbol) =>
            symbol.MethodKind == MethodKind.Ordinary &&
            symbol.Name == nameof(EventHandler.BeginInvoke);
    }
}

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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class RedundantModifier : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2333";
        private const string MessageFormat = "'{0}' is {1} in this context.";

        private static readonly DiagnosticDescriptor Rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        private static readonly ISet<SyntaxKind> UnsafeConstructKinds = new HashSet<SyntaxKind>
        {
            SyntaxKind.AddressOfExpression,
            SyntaxKind.PointerIndirectionExpression,
            SyntaxKind.SizeOfExpression,
            SyntaxKind.PointerType,
            SyntaxKind.FixedStatement,
            SyntaxKind.StackAllocArrayCreationExpression
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckSealedMemberInSealedClass,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.PropertyDeclaration,
                SyntaxKind.IndexerDeclaration,
                SyntaxKind.EventDeclaration,
                SyntaxKind.EventFieldDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckTypeDeclarationForRedundantPartial,
                SyntaxKind.ClassDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKindEx.RecordDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckForUnnecessaryUnsafeBlocks,
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKindEx.RecordDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    if (CheckedWalker.IsTopLevel(c.Node))
                    {
                        new CheckedWalker(c).SafeVisit(c.Node);
                    }
                },
                SyntaxKind.CheckedStatement,
                SyntaxKind.UncheckedStatement,
                SyntaxKind.CheckedExpression,
                SyntaxKind.UncheckedExpression);
        }

        private static void CheckForUnnecessaryUnsafeBlocks(SyntaxNodeAnalysisContext context)
        {
            var typeDeclaration = (TypeDeclarationSyntax)context.Node;
            if (typeDeclaration.Parent is TypeDeclarationSyntax || context.ContainingSymbol.Kind != SymbolKind.NamedType)
            {
                // only process top level type declarations
                return;
            }

            CheckForUnnecessaryUnsafeBlocksBelow(typeDeclaration, context);
        }

        private static void CheckForUnnecessaryUnsafeBlocksBelow(TypeDeclarationSyntax typeDeclaration, SyntaxNodeAnalysisContext context)
        {
            if (TryGetUnsafeKeyword(typeDeclaration, out var unsafeKeyword))
            {
                MarkAllUnsafeBlockInside(typeDeclaration, context);
                if (!HasUnsafeConstructInside(typeDeclaration, context.SemanticModel))
                {
                    ReportOnUnsafeBlock(context, unsafeKeyword.GetLocation());
                }
                return;
            }

            foreach (var member in typeDeclaration.Members)
            {
                if (TryGetUnsafeKeyword(member, out unsafeKeyword))
                {
                    MarkAllUnsafeBlockInside(member, context);
                    if (!HasUnsafeConstructInside(member, context.SemanticModel))
                    {
                        ReportOnUnsafeBlock(context, unsafeKeyword.GetLocation());
                    }
                    continue;
                }

                if (member is TypeDeclarationSyntax nestedTypeDeclaration)
                {
                    CheckForUnnecessaryUnsafeBlocksBelow(nestedTypeDeclaration, context);
                    continue;
                }

                var topLevelUnsafeBlocks = member.DescendantNodes(n => !n.IsKind(SyntaxKind.UnsafeStatement)).OfType<UnsafeStatementSyntax>();
                foreach (var topLevelUnsafeBlock in topLevelUnsafeBlocks)
                {
                    MarkAllUnsafeBlockInside(topLevelUnsafeBlock, context);
                    if (!HasUnsafeConstructInside(member, context.SemanticModel))
                    {
                        ReportOnUnsafeBlock(context, topLevelUnsafeBlock.UnsafeKeyword.GetLocation());
                    }
                }
            }
        }

        private static bool HasUnsafeConstructInside(SyntaxNode container, SemanticModel semanticModel) =>
            ContainsUnsafeConstruct(container)
                || ContainsFixedDeclaration(container)
                || ContainsUnsafeTypedIdentifier(container, semanticModel)
                || ContainsUnsafeInvocationReturnValue(container, semanticModel)
                || ContainsUnsafeParameter(container, semanticModel);

        private static bool ContainsUnsafeParameter(SyntaxNode container, SemanticModel semanticModel) =>
            container.DescendantNodes()
                .OfType<ParameterSyntax>()
                .Select(p => semanticModel.GetDeclaredSymbol(p))
                .Any(p => IsUnsafe(p?.Type));

        private static bool ContainsUnsafeInvocationReturnValue(SyntaxNode container, SemanticModel semanticModel) =>
            container.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Select(i => semanticModel.GetSymbolInfo(i).Symbol as IMethodSymbol)
                .Any(m => IsUnsafe(m?.ReturnType));

        private static bool ContainsUnsafeTypedIdentifier(SyntaxNode container, SemanticModel semanticModel) =>
            container.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Select(i => semanticModel.GetTypeInfo(i).Type)
                .Any(t => IsUnsafe(t));

        private static bool ContainsFixedDeclaration(SyntaxNode container) =>
            container.DescendantNodes()
                .OfType<FieldDeclarationSyntax>()
                .Any(fd => fd.Modifiers.Any(SyntaxKind.FixedKeyword));

        private static bool ContainsUnsafeConstruct(SyntaxNode container) =>
            container.DescendantNodes().Any(node => UnsafeConstructKinds.Contains(node.Kind()));

        private static bool IsUnsafe(ITypeSymbol type)
        {
            if (type == null)
            {
                return false;
            }

            if (type.TypeKind == TypeKind.Pointer)
            {
                return true;
            }

            return type.TypeKind == TypeKind.Array &&
                IsUnsafe(((IArrayTypeSymbol)type).ElementType);
        }

        private static void MarkAllUnsafeBlockInside(SyntaxNode container, SyntaxNodeAnalysisContext context)
        {
            foreach (var @unsafe in container.DescendantNodes()
                .SelectMany(node => node.ChildTokens())
                .Where(token => token.IsKind(SyntaxKind.UnsafeKeyword)))
            {
                ReportOnUnsafeBlock(context, @unsafe.GetLocation());
            }
        }

        private static void ReportOnUnsafeBlock(SyntaxNodeAnalysisContext context, Location issueLocation) =>
            context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, issueLocation, "unsafe", "redundant"));

        private static bool TryGetUnsafeKeyword(MemberDeclarationSyntax memberDeclaration, out SyntaxToken unsafeKeyword)
        {
            if (memberDeclaration is DelegateDeclarationSyntax delegateDeclaration)
            {
                unsafeKeyword = delegateDeclaration.Modifiers.FirstOrDefault(m => m.IsKind(SyntaxKind.UnsafeKeyword));
                return unsafeKeyword != default;
            }
            else if (memberDeclaration is BasePropertyDeclarationSyntax propertyDeclaration)
            {
                unsafeKeyword = propertyDeclaration.Modifiers.FirstOrDefault(m => m.IsKind(SyntaxKind.UnsafeKeyword));
                return unsafeKeyword != default;
            }
            else if (memberDeclaration is BaseMethodDeclarationSyntax methodDeclaration)
            {
                unsafeKeyword = methodDeclaration.Modifiers.FirstOrDefault(m => m.IsKind(SyntaxKind.UnsafeKeyword));
                return unsafeKeyword != default;
            }
            else if (memberDeclaration is BaseFieldDeclarationSyntax fieldDeclaration)
            {
                unsafeKeyword = fieldDeclaration.Modifiers.FirstOrDefault(m => m.IsKind(SyntaxKind.UnsafeKeyword));
                return unsafeKeyword != default;
            }
            else if (memberDeclaration is TypeDeclarationSyntax typeDeclaration)
            {
                unsafeKeyword = typeDeclaration.Modifiers.FirstOrDefault(m => m.IsKind(SyntaxKind.UnsafeKeyword));
                return unsafeKeyword != default;
            }
            else
            {
                unsafeKeyword = default;
                return false;
            }
        }

        private static void CheckTypeDeclarationForRedundantPartial(SyntaxNodeAnalysisContext context)
        {
            var typeDeclaration = (TypeDeclarationSyntax)context.Node;
            var classSymbol = context.SemanticModel.GetDeclaredSymbol(typeDeclaration);

            if (classSymbol == null
                || !typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)
                || classSymbol.DeclaringSyntaxReferences.Length > 1
                || context.ContainingSymbol.Kind != SymbolKind.NamedType)
            {
                return;
            }

            var keyword = typeDeclaration.Modifiers.First(m => m.IsKind(SyntaxKind.PartialKeyword));
            context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, keyword.GetLocation(), "partial", "gratuitous"));
        }

        private static SyntaxTokenList GetModifiers(MemberDeclarationSyntax memberDeclaration)
        {
            if (memberDeclaration is MethodDeclarationSyntax methodDeclaration)
            {
                return methodDeclaration.Modifiers;
            }
            else if (memberDeclaration is PropertyDeclarationSyntax propertyDeclaration)
            {
                return propertyDeclaration.Modifiers;
            }
            else if (memberDeclaration is IndexerDeclarationSyntax indexerDeclaration)
            {
                return indexerDeclaration.Modifiers;
            }
            else if (memberDeclaration is EventDeclarationSyntax eventDeclaration)
            {
                return eventDeclaration.Modifiers;
            }
            else if (memberDeclaration is EventFieldDeclarationSyntax eventFieldDeclaration)
            {
                return eventFieldDeclaration.Modifiers;
            }
            else
            {
                return default;
            }
        }

        private static void CheckSealedMemberInSealedClass(SyntaxNodeAnalysisContext context)
        {
            var memberDeclaration = (MemberDeclarationSyntax)context.Node;
            if (GetModifiers(memberDeclaration) is { } modifiers
                && modifiers.Any(SyntaxKind.SealedKeyword)
                && context.ContainingSymbol != null
                && context.ContainingSymbol.IsSealed
                && context.ContainingSymbol.ContainingType.IsSealed)
            {
                var keyword = modifiers.First(m => m.IsKind(SyntaxKind.SealedKeyword));
                context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, keyword.GetLocation(), "sealed", "redundant"));
            }
        }

        private class CheckedWalker : CSharpSyntaxWalker
        {
            private readonly SyntaxNodeAnalysisContext context;

            private static readonly ISet<SyntaxKind> BinaryOperationsForChecked = new HashSet<SyntaxKind>
            {
                SyntaxKind.AddExpression,
                SyntaxKind.SubtractExpression,
                SyntaxKind.MultiplyExpression,
                SyntaxKind.DivideExpression
            };

            private static readonly ISet<SyntaxKind> AssignmentsForChecked = new HashSet<SyntaxKind>
            {
                SyntaxKind.AddAssignmentExpression,
                SyntaxKind.SubtractAssignmentExpression,
                SyntaxKind.MultiplyAssignmentExpression,
                SyntaxKind.DivideAssignmentExpression
            };

            private static readonly ISet<SyntaxKind> UnaryOperationsForChecked = new HashSet<SyntaxKind>
            {
                SyntaxKind.UnaryMinusExpression,
                SyntaxKind.PostDecrementExpression,
                SyntaxKind.PostIncrementExpression,
                SyntaxKind.PreDecrementExpression,
                SyntaxKind.PreIncrementExpression
            };

            private bool isCurrentContextChecked;
            private bool currentContextHasIntegralOperation;

            public CheckedWalker(SyntaxNodeAnalysisContext context)
            {
                this.context = context;

                if (context.Node is CheckedStatementSyntax statement)
                {
                    isCurrentContextChecked = statement.IsKind(SyntaxKind.CheckedStatement);
                    return;
                }

                if (context.Node is CheckedExpressionSyntax expression)
                {
                    isCurrentContextChecked = expression.IsKind(SyntaxKind.CheckedExpression);
                    return;
                }
            }

            public override void VisitCheckedExpression(CheckedExpressionSyntax node) =>
                VisitChecked(node, SyntaxKind.CheckedExpression, node.Keyword, base.VisitCheckedExpression);

            public override void VisitCheckedStatement(CheckedStatementSyntax node) =>
                VisitChecked(node, SyntaxKind.CheckedStatement, node.Keyword, base.VisitCheckedStatement);

            public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
            {
                base.VisitAssignmentExpression(node);

                if (AssignmentsForChecked.Contains(node.Kind()))
                {
                    SetHasIntegralOperation(node);
                }
            }

            public override void VisitBinaryExpression(BinaryExpressionSyntax node)
            {
                base.VisitBinaryExpression(node);

                if (BinaryOperationsForChecked.Contains(node.Kind()))
                {
                    SetHasIntegralOperation(node);
                }
            }

            public override void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
            {
                base.VisitPrefixUnaryExpression(node);

                if (UnaryOperationsForChecked.Contains(node.Kind()))
                {
                    SetHasIntegralOperation(node);
                }
            }

            public override void VisitCastExpression(CastExpressionSyntax node)
            {
                base.VisitCastExpression(node);

                SetHasIntegralOperation(node);
            }

            public static bool IsTopLevel(SyntaxNode node) =>
                !node.Ancestors().Any(x => x is CheckedStatementSyntax || x is CheckedExpressionSyntax);

            private void VisitChecked<T>(T node, SyntaxKind checkedKind, SyntaxToken tokenToReport, Action<T> baseCall)
                where T : SyntaxNode
            {
                var isThisNodeChecked = node.IsKind(checkedKind);

                var originalIsCurrentContextChecked = isCurrentContextChecked;
                var originalContextHasIntegralOperation = currentContextHasIntegralOperation;

                isCurrentContextChecked = isThisNodeChecked;
                currentContextHasIntegralOperation = false;

                baseCall(node);

                var isSimplyRendundant = IsCurrentNodeEmbeddedInsideSameChecked(node, isThisNodeChecked, originalIsCurrentContextChecked);

                if (isSimplyRendundant || !currentContextHasIntegralOperation)
                {
                    var keywordToReport = isThisNodeChecked ? "checked" : "unchecked";
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, tokenToReport.GetLocation(), keywordToReport, "redundant"));
                }

                isCurrentContextChecked = originalIsCurrentContextChecked;
                currentContextHasIntegralOperation = originalContextHasIntegralOperation || (currentContextHasIntegralOperation && isSimplyRendundant);
            }

            private bool IsCurrentNodeEmbeddedInsideSameChecked(SyntaxNode node, bool isThisNodeChecked, bool isCurrentContextChecked) =>
                node != context.Node && isThisNodeChecked == isCurrentContextChecked;

            private void SetHasIntegralOperation(CastExpressionSyntax node)
            {
                var expressionType = context.SemanticModel.GetTypeInfo(node.Expression).Type;
                var castedToType = context.SemanticModel.GetTypeInfo(node.Type).Type;
                currentContextHasIntegralOperation |= castedToType != null && expressionType != null && castedToType.IsAny(KnownType.IntegralNumbers);
            }

            private void SetHasIntegralOperation(ExpressionSyntax node)
            {
                var methodSymbol = context.SemanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
                currentContextHasIntegralOperation |= methodSymbol != null && methodSymbol.ReceiverType.IsAny(KnownType.IntegralNumbers);
            }
        }
    }
}

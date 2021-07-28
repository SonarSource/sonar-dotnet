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
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ExceptionShouldNotBeThrownFromUnexpectedMethods : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3877";
        private const string MessageFormat = "Remove this 'throw' statement.";

        private static readonly DiagnosticDescriptor Rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<KnownType> DefaultAllowedExceptions = ImmutableArray.Create(KnownType.System_NotImplementedException);

        private static readonly ImmutableArray<KnownType> EventAccessorAllowedExceptions =
            ImmutableArray.Create(
                KnownType.System_NotImplementedException,
                KnownType.System_InvalidOperationException,
                KnownType.System_NotSupportedException,
                KnownType.System_ArgumentException
            );

        private static readonly ISet<SyntaxKind> TrackedOperators = new HashSet<SyntaxKind>
        {
            SyntaxKind.EqualsEqualsToken,
            SyntaxKind.ExclamationEqualsToken,
            SyntaxKind.LessThanToken,
            SyntaxKind.GreaterThanToken,
            SyntaxKind.LessThanEqualsToken,
            SyntaxKind.GreaterThanEqualsToken
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckForIssue<MethodDeclarationSyntax>(c, mds => IsTrackedMethod(mds, c.SemanticModel), DefaultAllowedExceptions),
                SyntaxKind.MethodDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckForIssue<ConstructorDeclarationSyntax>(c, cds => cds.Modifiers.Any(SyntaxKind.StaticKeyword), DefaultAllowedExceptions),
                SyntaxKind.ConstructorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckForIssue<OperatorDeclarationSyntax>(c, IsTrackedOperator, DefaultAllowedExceptions),
                SyntaxKind.OperatorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckForIssue<AccessorDeclarationSyntax>(c, x => true, EventAccessorAllowedExceptions),
                SyntaxKind.AddAccessorDeclaration,
                SyntaxKind.RemoveAccessorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckForIssue<ConversionOperatorDeclarationSyntax>(c, cods => cods.ImplicitOrExplicitKeyword.IsKind(SyntaxKind.ImplicitKeyword), DefaultAllowedExceptions),
                SyntaxKind.ConversionOperatorDeclaration);
        }

        private static void CheckForIssue<TSyntax>(SyntaxNodeAnalysisContext analysisContext, Func<TSyntax, bool> isTrackedSyntax, ImmutableArray<KnownType> allowedThrowTypes)
            where TSyntax : SyntaxNode
        {
            var syntax = (TSyntax)analysisContext.Node;
            if (isTrackedSyntax(syntax))
            {
                ReportOnInvalidThrowStatement(analysisContext, syntax, allowedThrowTypes);
            }
        }

        private static bool IsTrackedOperator(OperatorDeclarationSyntax declaration) =>
            TrackedOperators.Contains(declaration.OperatorToken.Kind());

        private static bool IsTrackedMethod(MethodDeclarationSyntax declaration, SemanticModel semanticModel) =>
            HasTrackedMethodOrAttributeName(declaration)
            && semanticModel.GetDeclaredSymbol(declaration) is { } methodSymbol
            && HasTrackedMethodOrAttributeType(methodSymbol);

        private static bool HasTrackedMethodOrAttributeName(MethodDeclarationSyntax declaration)
        {
            var name = declaration.Identifier.ValueText;
            return name == "Equals"
                || name == "GetHashCode"
                || name == "ToString"
                || name == "Dispose"
                || name == "Equals"
                || declaration.AttributeLists.SelectMany(list => list.Attributes).Any(x => x.ArgumentList == null && x.Name.ToStringContains("ModuleInitializer"));
        }

        private static bool HasTrackedMethodOrAttributeType(IMethodSymbol methodSymbol) =>
            methodSymbol.IsObjectEquals()
            || methodSymbol.IsObjectGetHashCode()
            || methodSymbol.IsObjectToString()
            || methodSymbol.IsIDisposableDispose()
            || methodSymbol.IsIEquatableEquals()
            || IsModuleInitializer(methodSymbol);

        private static bool IsModuleInitializer(IMethodSymbol methodSymbol) =>
            methodSymbol.AnyAttributeDerivesFrom(KnownType.System_Runtime_CompilerServices_ModuleInitializerAttribute);

        private static void ReportOnInvalidThrowStatement(SyntaxNodeAnalysisContext analysisContext,
            SyntaxNode node, ImmutableArray<KnownType> allowedTypes)
        {
            if (ExpressionBody(node) is { } expressionBody
                && ThrowExpressionSyntaxWrapper.IsInstance(expressionBody.Expression)
                && (ThrowExpressionSyntaxWrapper)expressionBody.Expression is { } throwExpression
                && analysisContext.SemanticModel.GetSymbolInfo(throwExpression.Expression).Symbol is { } symbol
                && ShouldReport(symbol.ContainingType, allowedTypes))
            {
                analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, throwExpression.Expression.GetLocation()));
            }
            else if (node.DescendantNodes()
                    .OfType<ThrowStatementSyntax>()
                    .Where(x => x.Expression != null)
                    .Select(x => new NodeAndSymbol(x, analysisContext.SemanticModel.GetSymbolInfo(x.Expression).Symbol))
                    .FirstOrDefault(nodeAndSymbol => nodeAndSymbol.Symbol != null && ShouldReport(nodeAndSymbol.Symbol.ContainingType, allowedTypes)) is { } throwStatement)
            {
                analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, throwStatement.Node.GetLocation()));
            }
        }

        private static ArrowExpressionClauseSyntax ExpressionBody(SyntaxNode node) =>
            node switch
            {
                MethodDeclarationSyntax a => a.ExpressionBody,
                ConstructorDeclarationSyntax b => b.ExpressionBody(),
                OperatorDeclarationSyntax c => c.ExpressionBody,
                AccessorDeclarationSyntax d => d.ExpressionBody(),
                ConversionOperatorDeclarationSyntax e => e.ExpressionBody,
                _ => null
            };

        private static bool ShouldReport(INamedTypeSymbol exceptionType, ImmutableArray<KnownType> allowedTypes) =>
            !exceptionType.IsAny(allowedTypes) && !exceptionType.DerivesFromAny(allowedTypes);
    }
}

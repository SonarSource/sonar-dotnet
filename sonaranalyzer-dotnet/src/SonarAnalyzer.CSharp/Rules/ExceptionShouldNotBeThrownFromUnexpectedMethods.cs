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
    public sealed class ExceptionShouldNotBeThrownFromUnexpectedMethods : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3877";
        private const string MessageFormat = "Remove this 'throw' statement.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ImmutableArray<KnownType> DefaultAllowedExceptions =
            ImmutableArray.Create(KnownType.System_NotImplementedException);

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
                c => CheckForIssue<MethodDeclarationSyntax>(c, mds => IsTrackedMethod(mds, c.SemanticModel),
                    DefaultAllowedExceptions),
                SyntaxKind.MethodDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckForIssue<ConstructorDeclarationSyntax>(c,
                    cds => cds.Modifiers.Any(SyntaxKind.StaticKeyword), DefaultAllowedExceptions),
                SyntaxKind.ConstructorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckForIssue<OperatorDeclarationSyntax>(c, IsTrackedOperator, DefaultAllowedExceptions),
                SyntaxKind.OperatorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckForIssue<AccessorDeclarationSyntax>(c, x => true, EventAccessorAllowedExceptions),
                SyntaxKind.AddAccessorDeclaration,
                SyntaxKind.RemoveAccessorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckForIssue<ConversionOperatorDeclarationSyntax>(c,
                    cods => cods.ImplicitOrExplicitKeyword.IsKind(SyntaxKind.ImplicitKeyword),
                    DefaultAllowedExceptions),
                SyntaxKind.ConversionOperatorDeclaration);
        }

        private void CheckForIssue<TSyntax>(SyntaxNodeAnalysisContext analysisContext,
            Func<TSyntax, bool> isTrackedSyntax, ImmutableArray<KnownType> allowedThrowTypes)
            where TSyntax : SyntaxNode
        {
            var syntax = (TSyntax)analysisContext.Node;
            if (!isTrackedSyntax(syntax))
            {
                return;
            }

            ReportOnInvalidThrowStatement(analysisContext, syntax, allowedThrowTypes);
        }

        private void ReportOnInvalidThrowStatement(SyntaxNodeAnalysisContext analysisContext,
            SyntaxNode node, ImmutableArray<KnownType> allowedTypes)
        {
            var throwToReportOn = node.DescendantNodes()
                .OfType<ThrowStatementSyntax>()
                .Where(tss => tss.Expression != null)
                .Select(tss => tss.ToSyntaxWithSymbol(analysisContext.SemanticModel.GetSymbolInfo(tss.Expression).Symbol))
                .FirstOrDefault(tuple => tuple.Symbol != null &&
                    !tuple.Symbol.ContainingType.IsAny(allowedTypes) &&
                    !tuple.Symbol.ContainingType.DerivesFromAny(allowedTypes))
                ?.Syntax;

            if (throwToReportOn != null)
            {
                analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(rule, throwToReportOn.GetLocation()));
            }
        }

        private static bool IsTrackedMethod(MethodDeclarationSyntax declaration, SemanticModel semanticModel)
        {
            var methodSymbol = semanticModel.GetDeclaredSymbol(declaration);
            if (methodSymbol == null)
            {
                return false;
            }

            return methodSymbol.IsObjectEquals() ||
                methodSymbol.IsObjectGetHashCode() ||
                methodSymbol.IsObjectToString() ||
                methodSymbol.IsIDisposableDispose() ||
                methodSymbol.IsIEquatableEquals();
        }

        private static bool IsTrackedOperator(OperatorDeclarationSyntax declaration)
        {
            return TrackedOperators.Contains(declaration.OperatorToken.Kind());
        }
    }
}

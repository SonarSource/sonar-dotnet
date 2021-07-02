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
using System.Collections.Immutable;
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
    public sealed class GetTypeWithIsAssignableFrom : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2219";
        internal const string UseIsOperatorKey = "UseIsOperator";
        internal const string ShouldRemoveGetTypeKey = "ShouldRemoveGetType";
        private const string MessageFormat = "Use {0} instead.";
        private const string MessageIsOperator = "the 'is' operator";
        private const string MessageIsInstanceOfType = "the 'IsInstanceOfType()' method";
        private const string MessageNullCheck = "a 'null' check";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    if (invocation.Expression is MemberAccessExpressionSyntax memberAccess
                        && invocation.HasExactlyNArguments(1)
                        && memberAccess.Name.Identifier.ValueText is var methodName
                        && (methodName == "IsInstanceOfType" || methodName == "IsAssignableFrom")
                        && c.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol
                        && methodSymbol.IsInType(KnownType.System_Type))
                    {
                        CheckForIsAssignableFrom(c, memberAccess, methodSymbol, invocation.ArgumentList.Arguments.First().Expression);
                        CheckForIsInstanceOfType(c, memberAccess, methodSymbol);
                    }
                },
                SyntaxKind.InvocationExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var binary = (BinaryExpressionSyntax)c.Node;
                    CheckGetTypeAndTypeOfEquality(c, binary.Left, binary.Right);
                    CheckGetTypeAndTypeOfEquality(c, binary.Right, binary.Left);

                    CheckAsOperatorComparedToNull(c, binary.Left, binary.Right);
                    CheckAsOperatorComparedToNull(c, binary.Right, binary.Left);
                },
                SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var isExpression = (BinaryExpressionSyntax)c.Node;
                    if (c.SemanticModel.GetTypeInfo(isExpression.Left).Type is { } objectToCast
                        && objectToCast.IsClass()
                        && c.SemanticModel.GetTypeInfo(isExpression.Right).Type is { } typeCastTo
                        && typeCastTo.IsClass()
                        && !typeCastTo.Is(KnownType.System_Object)
                        && objectToCast.DerivesOrImplements(typeCastTo))
                    {
                        ReportDiagnostic(c, MessageNullCheck);
                    }
                },
                SyntaxKind.IsExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var isPattern = (IsPatternExpressionSyntaxWrapper)c.Node;
                if (ConstantPatternExpression(isPattern.Pattern) is { } constantExpression)
                {
                    CheckAsOperatorComparedToNull(c, isPattern.Expression, constantExpression);
                }
            },
            SyntaxKindEx.IsPatternExpression);
        }

        private static void CheckAsOperatorComparedToNull(SyntaxNodeAnalysisContext context, ExpressionSyntax sideA, ExpressionSyntax sideB)
        {
            if (sideA.RemoveParentheses().IsKind(SyntaxKind.AsExpression) && sideB.RemoveParentheses().IsKind(SyntaxKind.NullLiteralExpression))
            {
                ReportDiagnostic(context, MessageIsOperator);
            }
        }

        private static void CheckGetTypeAndTypeOfEquality(SyntaxNodeAnalysisContext context, ExpressionSyntax sideA, ExpressionSyntax sideB)
        {
            if (sideA.ToStringContains("GetType")
                && sideB is TypeOfExpressionSyntax sideBeTypeOf
                && sideBeTypeOf.Type is { } typeSyntax
                && (sideA as InvocationExpressionSyntax).IsGetTypeCall(context.SemanticModel)
                && context.SemanticModel.GetTypeInfo(typeSyntax).Type is { } typeSymbol
                && typeSymbol.IsSealed
                && !typeSymbol.OriginalDefinition.Is(KnownType.System_Nullable_T))
            {
                ReportDiagnostic(context, MessageIsOperator);
            }
        }

        private static void CheckForIsInstanceOfType(SyntaxNodeAnalysisContext context, MemberAccessExpressionSyntax memberAccess, IMethodSymbol methodSymbol)
        {
            if (methodSymbol.Name == "IsInstanceOfType" && memberAccess.Expression is TypeOfExpressionSyntax)
            {
                ReportDiagnostic(context, MessageIsOperator, true);
            }
        }

        private static void CheckForIsAssignableFrom(SyntaxNodeAnalysisContext context, MemberAccessExpressionSyntax memberAccess, IMethodSymbol methodSymbol, ExpressionSyntax argument)
        {
            if (methodSymbol.Name == nameof(Type.IsAssignableFrom) && (argument as InvocationExpressionSyntax).IsGetTypeCall(context.SemanticModel))
            {
                if (memberAccess.Expression is TypeOfExpressionSyntax)
                {
                    ReportDiagnostic(context, MessageIsOperator, true, true);
                }
                else
                {
                    ReportDiagnostic(context, MessageIsInstanceOfType, false, true);
                }
            }
        }

        private static ExpressionSyntax ConstantPatternExpression(SyntaxNode node) =>
            node.Kind() switch
            {
                SyntaxKindEx.ConstantPattern => ((ConstantPatternSyntaxWrapper)node).Expression,
                SyntaxKindEx.NotPattern => ConstantPatternExpression(((UnaryPatternSyntaxWrapper)node).Pattern),
                _ => null
            };

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, string messageArg, bool useIsOperator = false, bool shouldRemoveGetType = false)
        {
            var properties = ImmutableDictionary<string, string>.Empty
            .Add(UseIsOperatorKey, useIsOperator.ToString())
            .Add(ShouldRemoveGetTypeKey, shouldRemoveGetType.ToString());
            context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, context.Node.GetLocation(), properties, messageArg));
        }
    }
}

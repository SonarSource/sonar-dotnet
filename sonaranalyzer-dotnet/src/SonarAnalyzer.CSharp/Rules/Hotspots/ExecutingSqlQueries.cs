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

using System.Collections.Immutable;
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
    public sealed class ExecutingSqlQueries : ExecutingSqlQueriesBase<SyntaxKind>
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager)
                .WithNotConfigurable();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(rule);

        public ExecutingSqlQueries()
            : this(AnalyzerConfiguration.Hotspot)
        {
        }

        internal /*for testing*/ ExecutingSqlQueries(IAnalyzerConfiguration analyzerConfiguration)
        {
            InvocationTracker = new CSharpInvocationTracker(analyzerConfiguration, rule);
            PropertyAccessTracker = new CSharpPropertyAccessTracker(analyzerConfiguration, rule);
            ObjectCreationTracker = new CSharpObjectCreationTracker(analyzerConfiguration, rule);
        }

        protected override InvocationCondition OnlyParameterIsConstantOrInterpolatedString() =>
            (context) =>
            {
                var argumentList = ((InvocationExpressionSyntax)context.Invocation).ArgumentList;
                if (argumentList == null ||
                    argumentList.Arguments.Count != 1)
                {
                    return false;
                }

                var onlyArgument = argumentList.Arguments[0].Expression.RemoveParentheses();

                return onlyArgument.IsAnyKind(SyntaxKind.InterpolatedStringExpression) ||
                    onlyArgument.IsConstant(context.SemanticModel);
            };

        protected override InvocationCondition ArgumentAtIndexIsConcat(int index) =>
            (context) =>
            {
                var argumentList = ((InvocationExpressionSyntax)context.Invocation).ArgumentList;
                if (argumentList == null ||
                    argumentList.Arguments.Count <= index)
                {
                    return false;
                }

                var argument = argumentList.Arguments[index].Expression.RemoveParentheses();
                return IsConcat(argument, context.SemanticModel);
            };

        protected override InvocationCondition ArgumentAtIndexIsFormat(int index) =>
            (context) =>
            {
                var argumentList = ((InvocationExpressionSyntax)context.Invocation).ArgumentList;
                if (argumentList == null ||
                    argumentList.Arguments.Count <= index)
                {
                    return false;
                }

                var argument = argumentList.Arguments[index].Expression.RemoveParentheses();
                return IsFormat(argument, context.SemanticModel);
            };

        protected override PropertyAccessCondition SetterIsConcat() =>
            (context) =>
            {
                // FIXME defensive coding make sure it's valid cast
                var setter = ((MemberAccessExpressionSyntax)context.Expression);
                if (!setter.IsLeftSideOfAssignment())
                {
                    return false;
                }

                var argument = ((AssignmentExpressionSyntax)setter.GetSelfOrTopParenthesizedExpression().Parent).Right.RemoveParentheses();
                return IsConcat(argument, context.SemanticModel);
            };

        protected override PropertyAccessCondition SetterIsFormat() =>
            (context) =>
            {
                // FIXME defensive coding make sure it's valid cast
                var setter = ((MemberAccessExpressionSyntax)context.Expression);
                if (!setter.IsLeftSideOfAssignment())
                {
                    return false;
                }

                var argument = ((AssignmentExpressionSyntax)setter.GetSelfOrTopParenthesizedExpression().Parent).Right.RemoveParentheses();
                return IsFormat(argument, context.SemanticModel);
            };

        protected override PropertyAccessCondition SetterIsInterpolation() =>
            (context) =>
            {
                // FIXME defensive coding make sure it's valid cast
                var setter = ((MemberAccessExpressionSyntax)context.Expression);
                if (!setter.IsLeftSideOfAssignment())
                {
                    return false;
                }

                var argument = ((AssignmentExpressionSyntax)setter.GetSelfOrTopParenthesizedExpression().Parent).Right.RemoveParentheses();
                return argument.IsAnyKind(SyntaxKind.InterpolatedStringExpression);
            };

        protected override ObjectCreationCondition FirstArgumentIsConcat() =>
            (context) =>
            {
                var argumentList = ((ObjectCreationExpressionSyntax)context.Expression).ArgumentList;
                if (argumentList == null || argumentList.Arguments.Count == 0)
                {
                    return false;
                }
                var argument = argumentList.Arguments[0].Expression.RemoveParentheses();
                return IsConcat(argument, context.SemanticModel);
            };

        protected override ObjectCreationCondition FirstArgumentIsFormat() =>
            (context) =>
            {
                var argumentList = ((ObjectCreationExpressionSyntax)context.Expression).ArgumentList;
                if (argumentList == null || argumentList.Arguments.Count == 0)
                {
                    return false;
                }
                var argument = argumentList.Arguments[0].Expression.RemoveParentheses();
                return IsFormat(argument, context.SemanticModel);
            };

        protected override ObjectCreationCondition FirstArgumentIsInterpolation() =>
            (context) =>
            {
                var argumentList = ((ObjectCreationExpressionSyntax)context.Expression).ArgumentList;
                if (argumentList == null || argumentList.Arguments.Count == 0)
                {
                    return false;
                }
                var argument = argumentList.Arguments[0].Expression.RemoveParentheses();
                return argument.IsAnyKind(SyntaxKind.InterpolatedStringExpression);
            };

        private static bool IsConcat(ExpressionSyntax argument, SemanticModel semanticModel)
        {
            if (argument.IsKind(SyntaxKind.AddExpression) && argument is BinaryExpressionSyntax concatenation)
            {
                // only say it's concatenation if it's not only constants
                return !AllConstants(concatenation, semanticModel);
            }

            if (argument.IsKind(SyntaxKind.InvocationExpression) && argument is InvocationExpressionSyntax invocation)
            {
                var methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol;
                return methodSymbol.IsInType(KnownType.System_String) &&
                    "Concat".Contains(methodSymbol.Name) &&
                    !AllConstants(invocation, semanticModel);
            }

            return false;
        }

        private static bool IsFormat(ExpressionSyntax argument, SemanticModel semanticModel)
        {
            if (argument.IsKind(SyntaxKind.InvocationExpression) && argument is InvocationExpressionSyntax invocation)
            {
                var methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol;
                return methodSymbol.IsInType(KnownType.System_String) &&
                    "Format".Contains(methodSymbol.Name) &&
                    !AllConstants(invocation, semanticModel);
            }

            return false;
        }

        private static bool AllConstants(BinaryExpressionSyntax binaryExpression, SemanticModel semanticModel)
        {
            System.Diagnostics.Debug.Assert(binaryExpression.IsKind(SyntaxKind.AddExpression));
            if (!(semanticModel.GetTypeInfo(binaryExpression).Type is ITypeSymbol concantenationType) ||
                !concantenationType.IsAny(KnownType.System_String))
            {
                return false;
            }
            if (!binaryExpression.Right.IsConstant(semanticModel))
            {
                return false;
            }
            var nestedLeft = binaryExpression.Left;
            var nestedBinary = nestedLeft as BinaryExpressionSyntax;
            while (nestedBinary != null)
            {
                if (!nestedBinary.IsKind(SyntaxKind.AddExpression) && !nestedBinary.IsConstant(semanticModel))
                {
                    return false;
                }

                nestedLeft = nestedBinary.Left;
                nestedBinary = nestedLeft as BinaryExpressionSyntax;
            }
            return true;
        }

        private static bool AllConstants(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            foreach (var argument in invocation.ArgumentList.Arguments)
            {
                if (!argument.Expression.IsConstant(semanticModel))
                {
                    return false;
                }
            }
            return true;
        }

    }
}

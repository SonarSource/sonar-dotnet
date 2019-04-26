﻿/*
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
                GetArgumentAtIndex(context, index) is ExpressionSyntax argument &&
                IsConcat(argument, context.SemanticModel);

        protected override InvocationCondition ArgumentAtIndexIsFormat(int index) =>
            (context) =>
                GetArgumentAtIndex(context, index) is ExpressionSyntax argument &&
                IsFormat(argument, context.SemanticModel);

        protected override PropertyAccessCondition SetterIsConcat() =>
            (context) =>
                GetSetValue(context) is ExpressionSyntax argument &&
                IsConcat(argument, context.SemanticModel);

        protected override PropertyAccessCondition SetterIsFormat() =>
            (context) =>
                GetSetValue(context) is ExpressionSyntax argument &&
                IsFormat(argument, context.SemanticModel);

        protected override PropertyAccessCondition SetterIsInterpolation() =>
            (context) =>
                GetSetValue(context) is ExpressionSyntax argument &&
                argument.IsAnyKind(SyntaxKind.InterpolatedStringExpression);

        protected override ObjectCreationCondition FirstArgumentIsConcat() =>
            (context) =>
                GetFirstArgument(context) is ExpressionSyntax firstArg &&
                IsConcat(firstArg, context.SemanticModel);

        protected override ObjectCreationCondition FirstArgumentIsFormat() =>
            (context) =>
                GetFirstArgument(context) is ExpressionSyntax firstArg &&
                IsFormat(firstArg, context.SemanticModel);

        protected override ObjectCreationCondition FirstArgumentIsInterpolation() =>
            (context) =>
                GetFirstArgument(context) is ExpressionSyntax firstArg &&
                firstArg.IsAnyKind(SyntaxKind.InterpolatedStringExpression);

        private static ExpressionSyntax GetArgumentAtIndex(InvocationContext context, int index) =>
            context.Invocation is InvocationExpressionSyntax invocation
                ? Get(invocation.ArgumentList, index)
                : null;

        private static ExpressionSyntax GetSetValue(PropertyAccessContext context) =>
            context.Expression is MemberAccessExpressionSyntax setter && setter.IsLeftSideOfAssignment()
                ? ((AssignmentExpressionSyntax)setter.GetSelfOrTopParenthesizedExpression().Parent).Right.RemoveParentheses()
                : null;

        private static ExpressionSyntax GetFirstArgument(ObjectCreationContext context) =>
            context.Expression is ObjectCreationExpressionSyntax objectCreation
                ? Get(objectCreation.ArgumentList, 0)
                : null;

        private static ExpressionSyntax Get(ArgumentListSyntax argumentList, int index) =>
            argumentList != null && argumentList.Arguments.Count > index
                ? argumentList.Arguments[index].Expression.RemoveParentheses()
                : null;

        private static bool IsConcat(ExpressionSyntax argument, SemanticModel semanticModel) =>
            IsStringMethodInvocation("Concat", argument, semanticModel) ||
            (
                argument.IsKind(SyntaxKind.AddExpression) &&
                argument is BinaryExpressionSyntax concatenation &&
                !IsConcatenationOfConstants(concatenation, semanticModel)
            );

        private static bool IsFormat(ExpressionSyntax argument, SemanticModel semanticModel) =>
            IsStringMethodInvocation("Format", argument, semanticModel);

        private static bool IsStringMethodInvocation(string methodName, ExpressionSyntax expression, SemanticModel semanticModel) =>
            expression is InvocationExpressionSyntax invocation &&
            semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol &&
            methodSymbol.IsInType(KnownType.System_String) &&
            methodName.Contains(methodSymbol.Name) &&
            !AllConstants(invocation, semanticModel);

        private static bool IsConcatenationOfConstants(BinaryExpressionSyntax binaryExpression, SemanticModel semanticModel)
        {
            System.Diagnostics.Debug.Assert(binaryExpression.IsKind(SyntaxKind.AddExpression));
            if ((semanticModel.GetTypeInfo(binaryExpression).Type is ITypeSymbol concantenationType) &&
                binaryExpression.Right.IsConstant(semanticModel))
            {
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
            return false;
        }

        private static bool AllConstants(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
            invocation.ArgumentList.Arguments.All(a=>a.Expression.IsConstant(semanticModel));

    }
}

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
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class ExecutingSqlQueries : ExecutingSqlQueriesBase<SyntaxKind, ExpressionSyntax>
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
            InvocationTracker = new VisualBasicInvocationTracker(analyzerConfiguration, rule);
            PropertyAccessTracker = new VisualBasicPropertyAccessTracker(analyzerConfiguration, rule);
            ObjectCreationTracker = new VisualBasicObjectCreationTracker(analyzerConfiguration, rule);
        }

        protected override ExpressionSyntax GetInvocationExpression(SyntaxNode expression) =>
            expression is InvocationExpressionSyntax invocation
                ? invocation.Expression
                : null;

        protected override ExpressionSyntax GetArgumentAtIndex(InvocationContext context, int index) =>
            context.Invocation is InvocationExpressionSyntax invocation
                ? invocation.ArgumentList.Get(index)
                : null;

        protected override ExpressionSyntax GetSetValue(PropertyAccessContext context) =>
            context.Expression is MemberAccessExpressionSyntax setter && setter.IsLeftSideOfAssignment()
                ? ((AssignmentStatementSyntax)setter.GetSelfOrTopParenthesizedExpression().Parent).Right.RemoveParentheses()
                : null;

        protected override ExpressionSyntax GetFirstArgument(ObjectCreationContext context) =>
            context.Expression is ObjectCreationExpressionSyntax objectCreation
                ? objectCreation.ArgumentList.Get(0)
                : null;

        protected override bool IsConcat(ExpressionSyntax argument, SemanticModel semanticModel) =>
            IsStringMethodInvocation("Concat", argument, semanticModel) ||
            (
                argument.IsKind(SyntaxKind.ConcatenateExpression) &&
                argument is BinaryExpressionSyntax concatenation &&
                !IsConcatenationOfConstants(concatenation, semanticModel)
            );

        protected override bool IsInterpolated(ExpressionSyntax argument) =>
            argument.IsAnyKind(SyntaxKind.InterpolatedStringExpression);

        protected override bool IsStringMethodInvocation(string methodName, ExpressionSyntax expression, SemanticModel semanticModel)
        {
            return expression is InvocationExpressionSyntax invocation &&
                invocation.IsMethodInvocation(KnownType.System_String, methodName, semanticModel) &&
                !AllConstants(invocation.ArgumentList.Arguments.ToList(), semanticModel);
        }

        private static bool AllConstants(List<ArgumentSyntax> arguments, SemanticModel semanticModel) =>
            arguments.All(a => a.GetExpression().IsConstant(semanticModel));

        private static bool IsConcatenationOfConstants(BinaryExpressionSyntax binaryExpression, SemanticModel semanticModel)
        {
            System.Diagnostics.Debug.Assert(binaryExpression.IsKind(SyntaxKind.ConcatenateExpression));
            if ((semanticModel.GetTypeInfo(binaryExpression).Type is ITypeSymbol concantenationType) &&
                binaryExpression.Right.IsConstant(semanticModel))
            {
                var nestedLeft = binaryExpression.Left;
                var nestedBinary = nestedLeft as BinaryExpressionSyntax;
                while (nestedBinary != null)
                {
                    if (!nestedBinary.IsKind(SyntaxKind.ConcatenateExpression) && !nestedBinary.IsConstant(semanticModel))
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
    }
}

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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class ExecutingSqlQueries : ExecutingSqlQueriesBase<SyntaxKind, ExpressionSyntax, IdentifierNameSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language { get; } = VisualBasicFacade.Instance;

        public ExecutingSqlQueries() : this(AnalyzerConfiguration.Hotspot) { }

        internal /*for testing*/ ExecutingSqlQueries(IAnalyzerConfiguration configuration) : base(configuration) { }

        protected override ExpressionSyntax GetArgumentAtIndex(InvocationContext context, int index) =>
            context.Node is InvocationExpressionSyntax invocation
                ? invocation.ArgumentList.Get(index)
                : null;

        protected override ExpressionSyntax GetArgumentAtIndex(ObjectCreationContext context, int index) =>
            context.Node is ObjectCreationExpressionSyntax objectCreation
                ? objectCreation.ArgumentList.Get(index)
                : null;

        protected override ExpressionSyntax GetSetValue(PropertyAccessContext context) =>
            context.Node is MemberAccessExpressionSyntax setter && setter.IsLeftSideOfAssignment()
                ? ((AssignmentStatementSyntax)setter.GetSelfOrTopParenthesizedExpression().Parent).Right.RemoveParentheses()
                : null;

        protected override bool IsTracked(ExpressionSyntax expression, SyntaxBaseContext context) =>
            expression != null && (IsSensitiveExpression(expression, context.SemanticModel) || IsTrackedVariableDeclaration(expression, context));

        protected override bool IsSensitiveExpression(ExpressionSyntax expression, SemanticModel semanticModel) =>
            IsConcatenation(expression, semanticModel)
            || expression.IsKind(SyntaxKind.InterpolatedStringExpression)
            || (expression is InvocationExpressionSyntax invocation && IsInvocationOfInterest(invocation, semanticModel));

        protected override Location SecondaryLocationForExpression(ExpressionSyntax node, string identifierNameToFind, out string identifierNameFound)
        {
            identifierNameFound = string.Empty;
            if (node == null)
            {
                return Location.None;
            }

            if (node.Parent is EqualsValueSyntax equalsValue
                && equalsValue.Parent is VariableDeclaratorSyntax declarationSyntax)
            {
                var identifier = declarationSyntax.Names.FirstOrDefault(name => name.Identifier.ValueText.Equals(identifierNameToFind, StringComparison.OrdinalIgnoreCase));

                if (identifier == null)
                {
                    return Location.None;
                }
                else
                {
                    identifierNameFound = identifier.Identifier.ValueText;
                    return identifier.GetLocation();
                }
            }

            if (node.Parent is AssignmentStatementSyntax assignment)
            {
                identifierNameFound = assignment.Left.GetName();
                return assignment.Left.GetLocation();
            }

            return Location.None;
        }

        private static bool IsInvocationOfInterest(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
            (invocation.IsMethodInvocation(KnownType.System_String, "Format", semanticModel) || invocation.IsMethodInvocation(KnownType.System_String, "Concat", semanticModel))
            && !AllConstants(invocation.ArgumentList.Arguments.ToList(), semanticModel);

        private static bool IsConcatenation(ExpressionSyntax expression, SemanticModel semanticModel) =>
            IsConcatenationOperator(expression)
            && expression is BinaryExpressionSyntax concatenation
            && !IsConcatenationOfConstants(concatenation, semanticModel);

        private static bool AllConstants(List<ArgumentSyntax> arguments, SemanticModel semanticModel) =>
            arguments.All(a => a.GetExpression().HasConstantValue(semanticModel));

        private static bool IsConcatenationOperator(SyntaxNode node) =>
            node.IsKind(SyntaxKind.ConcatenateExpression)
            || node.IsKind(SyntaxKind.AddExpression);

        private static bool IsConcatenationOfConstants(BinaryExpressionSyntax binaryExpression, SemanticModel semanticModel)
        {
            if ((semanticModel.GetTypeInfo(binaryExpression).Type is ITypeSymbol) && binaryExpression.Right.HasConstantValue(semanticModel))
            {
                var nestedLeft = binaryExpression.Left;
                var nestedBinary = nestedLeft as BinaryExpressionSyntax;
                while (nestedBinary != null)
                {
                    if (nestedBinary.Right.HasConstantValue(semanticModel)
                        && (IsConcatenationOperator(nestedBinary) || nestedBinary.HasConstantValue(semanticModel)))
                    {
                        nestedLeft = nestedBinary.Left;
                        nestedBinary = nestedLeft as BinaryExpressionSyntax;
                    }
                    else
                    {
                        return false;
                    }
                }
                return nestedLeft.HasConstantValue(semanticModel);
            }
            return false;
        }
    }
}

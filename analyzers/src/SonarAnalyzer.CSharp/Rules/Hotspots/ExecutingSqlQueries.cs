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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ExecutingSqlQueries : ExecutingSqlQueriesBase<SyntaxKind, ExpressionSyntax, IdentifierNameSyntax>
    {
        public ExecutingSqlQueries() : this(AnalyzerConfiguration.Hotspot) { }

        internal /*for testing*/ ExecutingSqlQueries(IAnalyzerConfiguration analyzerConfiguration) : base(RspecStrings.ResourceManager)
        {
            InvocationTracker = new CSharpInvocationTracker(analyzerConfiguration, Rule);
            PropertyAccessTracker = new CSharpPropertyAccessTracker(analyzerConfiguration, Rule);
            ObjectCreationTracker = new CSharpObjectCreationTracker(analyzerConfiguration, Rule);
            AssignmentFinder = new CSharpAssignmentFinder();
        }

        protected override string GetIdentifierName(IdentifierNameSyntax identifierNameSyntax) =>
            identifierNameSyntax.Identifier.ValueText;

        protected override ExpressionSyntax GetInvocationExpression(SyntaxNode expression) =>
            expression is InvocationExpressionSyntax invocation
                ? invocation.Expression
                : null;

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
                ? ((AssignmentExpressionSyntax)setter.GetSelfOrTopParenthesizedExpression().Parent).Right.RemoveParentheses()
                : null;

        protected override bool IsTracked(ExpressionSyntax expression, SyntaxBaseContext context) =>
            IsSensitiveExpression(expression, context.SemanticModel) || IsTrackedVariableDeclaration(expression, context);

        protected override bool IsSensitiveExpression(ExpressionSyntax expression, SemanticModel semanticModel) =>
            expression != null
            && (IsConcatenation(expression, semanticModel)
            || expression.IsKind(SyntaxKind.InterpolatedStringExpression)
            || (expression is InvocationExpressionSyntax invocation && IsInvocationOfInterest(invocation, semanticModel)));

        protected override Location SecondaryLocationForExpression(ExpressionSyntax node, string identifierNameToFind, out string identifierNameFound)
        {
            identifierNameFound = identifierNameToFind;

            if (node == null)
            {
                return Location.None;
            }

            if (node.Parent is EqualsValueClauseSyntax equalsValueClause
                && equalsValueClause.Parent is VariableDeclaratorSyntax declarationSyntax)
            {
                return declarationSyntax.Identifier.GetLocation();
            }

            return node.Parent is AssignmentExpressionSyntax assignment ? assignment.Left.GetLocation() : Location.None;
        }

        private static bool IsInvocationOfInterest(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
            (invocation.IsMethodInvocation(KnownType.System_String, "Format", semanticModel) || invocation.IsMethodInvocation(KnownType.System_String, "Concat", semanticModel))
            && !AllConstants(invocation.ArgumentList.Arguments.ToList(), semanticModel);

        private static bool IsConcatenation(ExpressionSyntax expression, SemanticModel semanticModel) =>
            expression.IsKind(SyntaxKind.AddExpression)
            && expression is BinaryExpressionSyntax concatenation
            && !IsConcatenationOfConstants(concatenation, semanticModel);

        private static bool AllConstants(IEnumerable<ArgumentSyntax> arguments, SemanticModel semanticModel) =>
            arguments.All(a => a.Expression.HasConstantValue(semanticModel));

        private static bool IsConcatenationOfConstants(BinaryExpressionSyntax binaryExpression, SemanticModel semanticModel)
        {
            System.Diagnostics.Debug.Assert(binaryExpression.IsKind(SyntaxKind.AddExpression), "Binary expression should be of syntax kind add expression.");
            if ((semanticModel.GetTypeInfo(binaryExpression).Type is ITypeSymbol) && binaryExpression.Right.HasConstantValue(semanticModel))
            {
                var nestedLeft = binaryExpression.Left;
                var nestedBinary = nestedLeft as BinaryExpressionSyntax;
                while (nestedBinary != null)
                {
                    if (nestedBinary.Right.HasConstantValue(semanticModel)
                        && (nestedBinary.IsKind(SyntaxKind.AddExpression) || nestedBinary.HasConstantValue(semanticModel)))
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

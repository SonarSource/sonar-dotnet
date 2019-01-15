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
    public sealed class DoNotOverwriteCollectionElements : DoNotOverwriteCollectionElementsBase<StatementSyntax>
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        protected override DiagnosticDescriptor Rule { get; } = rule;

        private static readonly HashSet<SyntaxKind> identifierOrLiteral =
            new HashSet<SyntaxKind>
            {
                SyntaxKind.IdentifierName,
                SyntaxKind.StringLiteralExpression,
                SyntaxKind.NumericLiteralExpression,
                SyntaxKind.CharacterLiteralExpression,
                SyntaxKind.NothingLiteralExpression,
                SyntaxKind.TrueLiteralExpression,
                SyntaxKind.FalseLiteralExpression,
            };

        private static readonly HashSet<string> collectionModifyingMethods =
            new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) // VB is case insensitive
            {
                "Item",
                "Add",
            };

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                AnalysisAction,
                SyntaxKind.ExpressionStatement,
                SyntaxKind.SimpleAssignmentStatement);

        protected override SyntaxNode GetCollectionIdentifier(StatementSyntax statement)
        {
            var invocation = GetInvocation(statement);
            return invocation != null
                ? GetInvokedMethodContainer(invocation).RemoveParentheses()
                : null;
        }

        protected override SyntaxNode GetIndexOrKey(StatementSyntax statement)
        {
            var invocation = GetInvocation(statement);
            return invocation != null
                ? GetFirstArgumentExpression(invocation).RemoveParentheses()
                : null;
        }

        protected override bool IsIdentifierOrLiteral(SyntaxNode syntaxNode) =>
            syntaxNode.IsAnyKind(identifierOrLiteral);

        // In Visual Basic all collection/dictionary item sets are made through invocations
        private static InvocationExpressionSyntax GetInvocation(StatementSyntax statement)
        {
            switch (statement.Kind())
            {
                case SyntaxKind.SimpleAssignmentStatement:
                    var assignmentStatement = (AssignmentStatementSyntax)statement;
                    return assignmentStatement.Left as InvocationExpressionSyntax;

                case SyntaxKind.ExpressionStatement:
                    var expression = ((ExpressionStatementSyntax)statement).Expression;

                    return expression.IsKind(SyntaxKind.ConditionalAccessExpression)
                        ? ((ConditionalAccessExpressionSyntax)expression).WhenNotNull as InvocationExpressionSyntax
                        : expression as InvocationExpressionSyntax;

                default:
                    return null;
            }
        }

        private static SyntaxNode GetInvokedMethodContainer(InvocationExpressionSyntax invocation)
        {
            //Supported syntax structures:
            // dictionary(key) = value
            // dictionary.Item(key) = value
            // dictionary.Add(key, value)
            // list(index) = value
            // list.Item(index) = value
            var expression = invocation.Expression.RemoveParentheses();
            switch (expression.Kind())
            {
                case SyntaxKind.SimpleMemberAccessExpression:
                    var memberAccess = (MemberAccessExpressionSyntax)expression;
                    if (!collectionModifyingMethods.Contains(memberAccess.Name.ToString()))
                    {
                        // Possibly an indexer syntax
                        return memberAccess;
                    }
                    if (memberAccess.Expression == null) // Possibly in a ConditionalAccess
                    {
                        var conditionalAccess = memberAccess.Parent.Parent as ConditionalAccessExpressionSyntax;
                        return conditionalAccess?.Expression;
                    }
                    return memberAccess.Expression; // Return the collection identifier containing the method

                case SyntaxKind.IdentifierName:
                    return expression;

                default:
                    return null;
            }
        }

        private static ExpressionSyntax GetFirstArgumentExpression(InvocationExpressionSyntax invocation) =>
            invocation.ArgumentList?.Arguments.ElementAtOrDefault(0)?.GetExpression().RemoveParentheses();
    }
}

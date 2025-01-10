/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class DoNotOverwriteCollectionElements : DoNotOverwriteCollectionElementsBase<SyntaxKind, StatementSyntax>
    {
        private static readonly HashSet<SyntaxKind> IdentifierOrLiteral =
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

        private static readonly HashSet<string> CollectionModifyingMethods =
            new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) // VB is case insensitive
            {
                        "Item",
                        "Add",
            };

        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
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
            syntaxNode.IsAnyKind(IdentifierOrLiteral);

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
            // Supported syntax structures:
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
                    if (!CollectionModifyingMethods.Contains(memberAccess.Name.ToString()))
                    {
                        // Possibly an indexer syntax
                        return memberAccess;
                    }
                    if (memberAccess.Expression == null) // Possibly in a ConditionalAccess
                    {
                        var conditionalAccess = memberAccess.Parent.Parent as ConditionalAccessExpressionSyntax;
                        return conditionalAccess?.Expression;
                    }

                    if (memberAccess.Name.ToString().Equals("Add", StringComparison.OrdinalIgnoreCase) && invocation.ArgumentList?.Arguments.Count == 1)
                    {
                        return null;    // #2674 Do not raise on ICollection.Add(item)
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

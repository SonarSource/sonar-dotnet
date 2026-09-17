/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.VisualBasic.Rules;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class DoNotOverwriteCollectionElements : DoNotOverwriteCollectionElementsBase<SyntaxKind, StatementSyntax>
{
    private static readonly HashSet<SyntaxKind> IdentifierOrLiteral =
    [
        SyntaxKind.IdentifierName,
        SyntaxKind.StringLiteralExpression,
        SyntaxKind.NumericLiteralExpression,
        SyntaxKind.CharacterLiteralExpression,
        SyntaxKind.NothingLiteralExpression,
        SyntaxKind.TrueLiteralExpression,
        SyntaxKind.FalseLiteralExpression
    ];

    private static readonly HashSet<string> CollectionModifyingMethods = new(StringComparer.InvariantCultureIgnoreCase) // VB is case-insensitive
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

    protected override SyntaxNode CollectionIdentifier(StatementSyntax statement, bool includeCompoundAssignments) =>
        Invocation(statement) is { } invocation ? InvokedMethodContainer(invocation).RemoveParentheses() : null;

    protected override SyntaxNode IndexOrKey(StatementSyntax statement) =>
        Invocation(statement) is { } invocation ? FirstArgumentExpression(invocation).RemoveParentheses() : null;

    protected override bool IsIdentifierOrLiteral(SyntaxNode node) =>
        node.IsAnyKind(IdentifierOrLiteral);

    // In Visual Basic all collection/dictionary item sets are made through invocations
    private static InvocationExpressionSyntax Invocation(StatementSyntax statement) =>
        statement switch
        {
            AssignmentStatementSyntax assignment => assignment.Left,
            ExpressionStatementSyntax { Expression: ConditionalAccessExpressionSyntax conditionalAccess } => conditionalAccess.WhenNotNull,
            ExpressionStatementSyntax expression => expression.Expression,
            _ => null
        } as InvocationExpressionSyntax;

    private static SyntaxNode InvokedMethodContainer(InvocationExpressionSyntax invocation) =>
        // Supported syntax structures:
        // dictionary(key) = value
        // dictionary.Item(key) = value
        // dictionary.Add(key, value)
        // list(index) = value
        // list.Item(index) = value
        invocation.Expression.RemoveParentheses() switch
        {
            MemberAccessExpressionSyntax memberAccess when !CollectionModifyingMethods.Contains(memberAccess.Name.ToString()) => memberAccess,  // Possibly an indexer syntax
            MemberAccessExpressionSyntax { Expression: null, Parent.Parent: ConditionalAccessExpressionSyntax conditionalAccess } => conditionalAccess.Expression,
            MemberAccessExpressionSyntax memberAccess when memberAccess.Name.ToString().Equals("Add", StringComparison.OrdinalIgnoreCase) && invocation.ArgumentList?.Arguments.Count == 1 => null,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Expression, // "a" from a.Add(item)
            IdentifierNameSyntax identifier => identifier,
            _ => null
        };

    private static ExpressionSyntax FirstArgumentExpression(InvocationExpressionSyntax invocation) =>
        invocation.ArgumentList?.Arguments.ElementAtOrDefault(0)?.GetExpression();
}

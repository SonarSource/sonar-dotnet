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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotOverwriteCollectionElements : DoNotOverwriteCollectionElementsBase<SyntaxKind, ExpressionStatementSyntax>
{
    private static readonly HashSet<SyntaxKind> IdentifierOrLiteral =
    [
        SyntaxKind.IdentifierName,
        SyntaxKind.StringLiteralExpression,
        SyntaxKind.NumericLiteralExpression,
        SyntaxKind.CharacterLiteralExpression,
        SyntaxKind.NullLiteralExpression,
        SyntaxKind.TrueLiteralExpression,
        SyntaxKind.FalseLiteralExpression
    ];

    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            AnalysisAction,
            SyntaxKind.ExpressionStatement);

    protected override SyntaxNode CollectionIdentifier(ExpressionStatementSyntax statement, bool includeCompoundAssignments) =>
        AssignmentOrInvocation(statement) switch
        {
            InvocationExpressionSyntax invocation => InvokedMethodContainer(invocation).RemoveParentheses(),
            AssignmentExpressionSyntax { Left: ElementAccessExpressionSyntax elementAccess } assignment =>
                includeCompoundAssignments || assignment.IsKind(SyntaxKind.SimpleAssignmentExpression)
                    ? Identifier(elementAccess.Expression.WithoutEnclosingParentheses).RemoveParentheses()
                    : null,
            _ => null
        };

    protected override SyntaxNode IndexOrKey(ExpressionStatementSyntax statement) =>
        IndexOrKeyArgument(statement)?.Expression.WithoutEnclosingParentheses;

    protected override bool IsIdentifierOrLiteral(SyntaxNode node) =>
        node.IsAnyKind(IdentifierOrLiteral);

    private static SyntaxNode AssignmentOrInvocation(ExpressionStatementSyntax statement) =>
        statement.Expression is ConditionalAccessExpressionSyntax conditionalAccess ? conditionalAccess.WhenNotNull : statement.Expression;

    private static ArgumentSyntax IndexOrKeyArgument(ExpressionStatementSyntax statement) =>
        AssignmentOrInvocation(statement) switch
        {
            InvocationExpressionSyntax invocation => invocation.ArgumentList.Arguments.ElementAtOrDefault(0),
            AssignmentExpressionSyntax { Left: ElementAccessExpressionSyntax elementAccess } =>
                elementAccess.ArgumentList.Arguments.ElementAtOrDefault(0),
            _ => null
        };

    private static SyntaxNode InvokedMethodContainer(InvocationExpressionSyntax invocation) =>
        invocation.Expression.WithoutEnclosingParentheses switch
        {
            MemberAccessExpressionSyntax memberAccess when memberAccess.Name.ToString() == "Add" && invocation.ArgumentList?.Arguments.Count != 1 => memberAccess.Expression,   // "a" from a.Add(item)
            MemberBindingExpressionSyntax { Parent.Parent: ConditionalAccessExpressionSyntax conditionalAccess } => conditionalAccess.Expression,                               // "a" from a?.Add(item)
            _ => null
        };

    private static SyntaxNode Identifier(ExpressionSyntax expression) =>
        expression?.Kind() is SyntaxKind.SimpleMemberAccessExpression or SyntaxKind.IdentifierName ? expression : null; // "a.b" from a.b[index], or "a" from a[index]
}

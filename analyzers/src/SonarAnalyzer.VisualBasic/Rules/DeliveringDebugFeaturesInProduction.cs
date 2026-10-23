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
public sealed class DeliveringDebugFeaturesInProduction : DeliveringDebugFeaturesInProductionBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override bool IsDevelopmentCheckInvoked(SyntaxNode node, SemanticModel model) =>
        node.FirstAncestorOrSelf<StatementSyntax>() is var invocationStatement
        && invocationStatement.Ancestors().Any(x => IsDevelopmentCheck(x, model) || IsInNonProductionBranch(x, node, model));

    protected override bool IsInDevelopmentContext(SyntaxNode node) =>
        node.Ancestors()
            .OfType<ClassBlockSyntax>()
            .Any(x => x.ClassStatement.Identifier.Text == StartupDevelopment);

    // Only one branch excludes production: If Not IsProduction() Then [here] or If IsProduction() Then ... ElseIf/Else [here]
    private bool IsInNonProductionBranch(SyntaxNode statement, SyntaxNode node, SemanticModel model) =>
        FindCondition(statement) is { } condition
        && (IsInElseBranch(statement, node)
            ? IsProductionCheck(condition, model)
            : FindThenStatements(statement).Span.Contains(node.Span) && IsNegatedProductionCheck(condition, model));

    private bool IsDevelopmentCheck(SyntaxNode node, SemanticModel model) =>
        FindCondition(node).RemoveParentheses() is InvocationExpressionSyntax condition
        && IsValidationMethod(model, condition, condition.Expression.GetIdentifier()?.ValueText);

    private bool IsProductionCheck(ExpressionSyntax condition, SemanticModel model) =>
        condition.RemoveParentheses() is { } check
        && IsProductionMethod(model, check, check.GetIdentifier()?.ValueText);

    private bool IsNegatedProductionCheck(ExpressionSyntax condition, SemanticModel model) =>
        condition.RemoveParentheses() is UnaryExpressionSyntax negation
        && negation.IsKind(SyntaxKind.NotExpression)
        && IsProductionCheck(negation.Operand, model);

    private static ExpressionSyntax FindCondition(SyntaxNode node) =>
        node switch
        {
            MultiLineIfBlockSyntax multiline => multiline.IfStatement.Condition,
            SingleLineIfStatementSyntax singleLine => singleLine.Condition,
            ElseIfBlockSyntax elseIf => elseIf.ElseIfStatement.Condition,
            _ => null
        };

    private static SyntaxList<StatementSyntax> FindThenStatements(SyntaxNode node) =>
        node switch
        {
            MultiLineIfBlockSyntax multiline => multiline.Statements,
            SingleLineIfStatementSyntax singleLine => singleLine.Statements,
            ElseIfBlockSyntax elseIf => elseIf.Statements,
            _ => default
        };

    private static bool IsInElseBranch(SyntaxNode statement, SyntaxNode node) =>
        statement switch
        {
            MultiLineIfBlockSyntax multiline => multiline.ElseIfBlocks.Any(x => x.Span.Contains(node.Span)) || multiline.ElseBlock?.Span.Contains(node.Span) is true,
            SingleLineIfStatementSyntax singleLine => singleLine.ElseClause?.Span.Contains(node.Span) is true,
            _ => false
        };
}

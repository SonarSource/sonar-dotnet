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
        && invocationStatement.Ancestors().Any(x => IsDevelopmentCheck(x, model));

    protected override bool IsInDevelopmentContext(SyntaxNode node) =>
        node.Ancestors()
            .OfType<ClassBlockSyntax>()
            .Any(x => x.ClassStatement.Identifier.Text == StartupDevelopment);

    private bool IsDevelopmentCheck(SyntaxNode node, SemanticModel model) =>
        FindCondition(node).RemoveParentheses() is InvocationExpressionSyntax condition
        && IsValidationMethod(model, condition, condition.Expression.GetIdentifier()?.ValueText);

    private static ExpressionSyntax FindCondition(SyntaxNode node) =>
        node switch
        {
            MultiLineIfBlockSyntax multiline => multiline.IfStatement.Condition,
            SingleLineIfStatementSyntax singleLine => singleLine.Condition,
            _ => null
        };
}

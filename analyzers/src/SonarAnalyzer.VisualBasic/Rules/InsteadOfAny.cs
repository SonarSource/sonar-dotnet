/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
public sealed class InsteadOfAny : InsteadOfAnyBase<SyntaxKind, InvocationExpressionSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override bool IsSimpleEqualityCheck(InvocationExpressionSyntax node, SemanticModel model) =>
        GetArgumentExpression(node, 0) is SingleLineLambdaExpressionSyntax lambda
        && lambda.SubOrFunctionHeader.ParameterList.Parameters is { Count: 1 } parameters
        && parameters[0].Identifier.GetName() is var lambdaVariableName
        && lambda.Body switch
        {
            BinaryExpressionSyntax binary when binary.OperatorToken.Kind() is SyntaxKind.EqualsToken or SyntaxKind.IsKeyword =>
                HasValidBinaryOperands(lambdaVariableName, binary.Left, binary.Right, model),
            InvocationExpressionSyntax invocation =>
                HasValidInvocationOperands(invocation, lambdaVariableName, model),
            _ => false
        };

    private bool HasValidBinaryOperands(string lambdaVariableName, SyntaxNode first, SyntaxNode second, SemanticModel model) =>
        (AreValidOperands(lambdaVariableName, first, second) && IsNullOrValueTypeOrString(second, model))
        || (AreValidOperands(lambdaVariableName, second, first) && IsNullOrValueTypeOrString(first, model));

    private static bool IsNullOrValueTypeOrString(SyntaxNode node, SemanticModel model) =>
        node.IsKind(SyntaxKind.NothingLiteralExpression) || IsValueTypeOrString(node, model);

    protected override bool AreValidOperands(string lambdaVariable, SyntaxNode first, SyntaxNode second) =>
        first is IdentifierNameSyntax && IsNameEqualTo(first, lambdaVariable)
        && second switch
        {
            LiteralExpressionSyntax => true,
            IdentifierNameSyntax => !IsNameEqualTo(first, second.GetName()),
            _ => false,
        };

    protected override SyntaxNode GetArgumentExpression(InvocationExpressionSyntax invocation, int index) =>
        invocation.ArgumentList.Arguments[index].GetExpression();
}

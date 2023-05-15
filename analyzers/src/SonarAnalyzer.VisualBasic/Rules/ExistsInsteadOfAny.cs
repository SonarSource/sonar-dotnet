/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules.VisualBasic;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class ExistsInsteadOfAny : InsteadOfAnyBase<SyntaxKind, InvocationExpressionSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override bool HasOneArgument(InvocationExpressionSyntax node) =>
        node.HasExactlyNArguments(1);

    protected override bool IsSimpleEqualityCheck(InvocationExpressionSyntax node, SemanticModel model) =>
        node.ArgumentList.Arguments[0].GetExpression() is SingleLineLambdaExpressionSyntax lambda
        && lambda.SubOrFunctionHeader.ParameterList.Parameters is { Count: 1 } parameters
        && parameters[0].Identifier.GetName() is var lambdaVariableName
        && lambda.Body switch
        {
            BinaryExpressionSyntax binary =>
                binary.OperatorToken.IsKind(SyntaxKind.EqualsToken)
                && HasBinaryValidOperands(lambdaVariableName, binary.Left, binary.Right, model),
            InvocationExpressionSyntax invocation =>
                IsNameEqual(invocation, nameof(Equals))
                && CheckInvocationArguments(invocation, lambdaVariableName),
            _ => false
        };

    private bool HasBinaryValidOperands(string lambdaVariableName, SyntaxNode first, SyntaxNode second, SemanticModel model) =>
        (AreValidOperands(lambdaVariableName, first, second) && IsValueTypeOrString(second, model))
        || (AreValidOperands(lambdaVariableName, second, first) && IsValueTypeOrString(first, model));

    private bool CheckInvocationArguments(InvocationExpressionSyntax invocation, string lambdaVariableName)
    {
        if (invocation.HasExactlyNArguments(1))
        {
            return Language.Syntax.TryGetOperands(invocation, out var left, out _)
                && HasInvocationValidOperands(left, invocation.ArgumentList.Arguments[0].GetExpression());
        }
        if (invocation.HasExactlyNArguments(2))
        {
            return HasInvocationValidOperands(invocation.ArgumentList.Arguments[0].GetExpression(), invocation.ArgumentList.Arguments[1].GetExpression());
        }
        return false;

        bool HasInvocationValidOperands(SyntaxNode first, SyntaxNode second) =>
            AreValidOperands(lambdaVariableName, first, second) || AreValidOperands(lambdaVariableName, second, first);
    }

    private bool AreValidOperands(string lambdaVariable, SyntaxNode first, SyntaxNode second) =>
        first is IdentifierNameSyntax && IsNameEqual(first, lambdaVariable)
        && second switch
        {
            LiteralExpressionSyntax => true,
            IdentifierNameSyntax => !IsNameEqual(first, second.GetName()),
            _ => false,
        };
}

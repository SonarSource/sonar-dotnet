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
public sealed class ExistsInsteadOfAny : ExistsInsteadOfAnyBase<SyntaxKind, InvocationExpressionSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override bool HasOneArgument(InvocationExpressionSyntax node) =>
        node.HasExactlyNArguments(1);

    protected override bool IsValueEquality(InvocationExpressionSyntax node, SemanticModel model) =>
        node.ArgumentList.Arguments[0].GetExpression() is SingleLineLambdaExpressionSyntax lambda
        && lambda.Body switch
        {
            BinaryExpressionSyntax binary =>
                binary.OperatorToken.IsKind(SyntaxKind.EqualsToken)
                && IsIdentifierOrLiteralValueType(binary.Left, model)
                && IsIdentifierOrLiteralValueType(binary.Right, model),
            InvocationExpressionSyntax invocation =>
                Language.GetName(invocation).Equals(nameof(Equals), Language.NameComparison),
            _ => false
        };

    private static bool IsIdentifierOrLiteralValueType(ExpressionSyntax expression, SemanticModel model) =>
        expression is IdentifierNameSyntax or LiteralExpressionSyntax
        && model.GetTypeInfo(expression).Type is { } type
        && (type.IsValueType || type.Is(KnownType.System_String));
}

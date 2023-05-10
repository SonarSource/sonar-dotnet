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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExistsInsteadOfAny : ExistsInsteadOfAnyBase<SyntaxKind, InvocationExpressionSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override bool TryGetOperands(InvocationExpressionSyntax invocation, out SyntaxNode left, out SyntaxNode right) =>
        invocation.TryGetOperands(out left, out right);

    protected override bool HasValidDelegate(InvocationExpressionSyntax node) =>
        node.ArgumentList.Arguments.First().Expression is SimpleLambdaExpressionSyntax { } lambda
        && CheckExpression(lambda.Body);

    protected override bool HasOneArgument(InvocationExpressionSyntax node) =>
        node.ArgumentList.Arguments.Count == 1;

    protected override SyntaxToken? GetIdentifier(InvocationExpressionSyntax invocation) =>
        invocation.GetIdentifier();

    private bool CheckExpression(SyntaxNode body) =>
        body switch
        {
            BinaryExpressionSyntax { } binary =>
                !(binary.OperatorToken.IsKind(SyntaxKind.EqualsEqualsToken)
                && ((binary.Left is IdentifierNameSyntax && binary.Right is LiteralExpressionSyntax)
                    || (binary.Right is IdentifierNameSyntax && binary.Left is LiteralExpressionSyntax)
                    || (binary.Left is IdentifierNameSyntax && binary.Right is IdentifierNameSyntax))),
            InvocationExpressionSyntax { } invocation =>
                !Language.GetName(invocation).Equals(nameof(Equals), Language.NameComparison),
            _ => true
        };
}

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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class UriShouldNotBeHardcoded : UriShouldNotBeHardcodedBase<SyntaxKind, ExpressionSyntax, LiteralExpressionSyntax, SyntaxKind, BinaryExpressionSyntax, ArgumentSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;
        protected override SyntaxKind StringLiteralSyntaxKind => SyntaxKind.StringLiteralExpression;

        protected override SyntaxKind[] StringConcatenateExpressions =>
            new[]
            {
                SyntaxKind.AddExpression,
                SyntaxKind.ConcatenateExpression
            };

        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer => VisualBasicGeneratedCodeRecognizer.Instance;

        protected override string GetLiteralText(LiteralExpressionSyntax literalExpression) => literalExpression?.Token.ValueText;

        protected override bool IsInvocationOrObjectCreation(SyntaxNode node) =>
            node.IsAnyKind(SyntaxKind.InvocationExpression, SyntaxKind.ObjectCreationExpression);

        protected override SyntaxNode GetRelevantAncestor(SyntaxNode node)
        {
            var parameter = node.FirstAncestorOrSelf<ParameterSyntax>();
            if (parameter != null)
            {
                return parameter;
            }
            var variableDeclarator = node.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
            if (variableDeclarator != null)
            {
                return variableDeclarator;
            }
            return null;
        }
    }
}

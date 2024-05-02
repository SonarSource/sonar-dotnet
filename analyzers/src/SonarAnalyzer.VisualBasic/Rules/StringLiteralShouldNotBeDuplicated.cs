/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
    public sealed class StringLiteralShouldNotBeDuplicated : StringLiteralShouldNotBeDuplicatedBase<SyntaxKind, LiteralExpressionSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        protected override SyntaxKind[] SyntaxKinds { get; } =
        {
            SyntaxKind.ClassBlock,
            SyntaxKind.StructureBlock
        };

        protected override bool IsMatchingMethodParameterName(LiteralExpressionSyntax literalExpression) =>
            literalExpression.FirstAncestorOrSelf<MethodBlockBaseSyntax>()
                ?.BlockStatement?.ParameterList
                ?.Parameters
                .Any(p => p.Identifier.Identifier.ValueText.Equals(literalExpression.Token.ValueText, StringComparison.OrdinalIgnoreCase))
                ?? false;

        protected override bool IsInnerInstance(SonarSyntaxNodeReportingContext context) =>
            context.Node.HasAncestor(SyntaxKind.ClassBlock, SyntaxKind.StructureBlock);

        protected override IEnumerable<LiteralExpressionSyntax> FindLiteralExpressions(SyntaxNode node) =>
            node.DescendantNodes(n => !n.IsKind(SyntaxKind.AttributeList))
                .Where(les => les.IsKind(SyntaxKind.StringLiteralExpression))
                .Cast<LiteralExpressionSyntax>();

        protected override SyntaxToken LiteralToken(LiteralExpressionSyntax literal) =>
            literal.Token;
    }
}

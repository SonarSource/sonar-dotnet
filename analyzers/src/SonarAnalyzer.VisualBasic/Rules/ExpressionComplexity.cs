/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
    public sealed class ExpressionComplexity : ExpressionComplexityBase<ExpressionSyntax, SyntaxKind>
    {
        protected override ILanguageFacade Language { get; } = VisualBasicFacade.Instance;

        protected override SyntaxKind[] TransparentKinds { get; } =
            {
                SyntaxKind.ParenthesizedExpression,
            };

        private static readonly ISet<SyntaxKind> CompoundExpressionKinds = new HashSet<SyntaxKind>
        {
            SyntaxKind.MultiLineFunctionLambdaExpression,
            SyntaxKind.MultiLineSubLambdaExpression,
            SyntaxKind.SingleLineFunctionLambdaExpression,
            SyntaxKind.SingleLineSubLambdaExpression,

            SyntaxKind.CollectionInitializer,
            SyntaxKind.ObjectMemberInitializer,

            SyntaxKind.InvocationExpression
        };

        protected override SyntaxKind[] ComplexityIncreasingKinds { get; } =
            {
                SyntaxKind.AndExpression,
                SyntaxKind.AndAlsoExpression,
                SyntaxKind.OrExpression,
                SyntaxKind.OrElseExpression,
                SyntaxKind.ExclusiveOrExpression
            };

        protected override bool IsComplexityIncreasingKind(SyntaxNode node) =>
            ComplexityIncreasingKinds.Contains(node.Kind());

        protected override bool IsCompoundExpression(SyntaxNode node) =>
            CompoundExpressionKinds.Contains(node.Kind());

        protected override bool IsPatternRoot(SyntaxNode node) =>
            false;
        protected override SyntaxNode[] ExpressionChildren(SyntaxNode node) => throw new NotImplementedException();
    }
}

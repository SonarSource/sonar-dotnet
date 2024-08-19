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
    public sealed class StringConcatenationInLoop : StringConcatenationInLoopBase<SyntaxKind, AssignmentStatementSyntax, BinaryExpressionSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        protected override SyntaxKind[] CompoundAssignmentKinds { get; } = new[] { SyntaxKind.AddAssignmentStatement, SyntaxKind.ConcatenateAssignmentStatement };

        protected override ISet<SyntaxKind> ExpressionConcatenationKinds { get; } = new HashSet<SyntaxKind>
        {
            SyntaxKind.AddExpression,
            SyntaxKind.ConcatenateExpression
        };

        protected override ISet<SyntaxKind> LoopKinds { get; } = new HashSet<SyntaxKind>
        {
            SyntaxKind.WhileBlock,
            SyntaxKind.SimpleDoLoopBlock,
            SyntaxKind.ForBlock,
            SyntaxKind.ForEachBlock,
            SyntaxKind.DoUntilLoopBlock,
            SyntaxKind.DoWhileLoopBlock,
            SyntaxKind.DoLoopUntilBlock,
            SyntaxKind.DoLoopWhileBlock
        };
    }
}

/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
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

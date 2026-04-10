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

namespace SonarAnalyzer.CFG.Syntax.Utilities;

/// <summary>
/// This class violates the basic principle that SE should only depend on IOperation.
///
/// Anything added here needs to have extremely rare reason why it exists.
/// </summary>
public abstract class SyntaxClassifierBase
{
    public abstract SyntaxNode MemberAccessExpression(SyntaxNode node);
    protected abstract bool IsCfgBoundary(SyntaxNode node);
    protected abstract bool IsStatement(SyntaxNode node);
    protected abstract SyntaxNode ParentLoopCondition(SyntaxNode node);

    // Detecting loops from CFG shape is not possible from the shape of CFG, because of nested loops.
    public bool IsInLoopCondition(SyntaxNode node)
    {
        while (node is not null)
        {
            if (ParentLoopCondition(node) == node)
            {
                return true;
            }
            if (IsStatement(node) || IsCfgBoundary(node))
            {
                return false;
            }
            else
            {
                node = node.Parent;
            }
        }
        return false;
    }
}

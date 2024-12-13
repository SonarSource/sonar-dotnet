/*
 * Copyright (C) 2015-2024 SonarSource SA
 * All rights reserved
 * mailto:info AT sonarsource DOT com
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

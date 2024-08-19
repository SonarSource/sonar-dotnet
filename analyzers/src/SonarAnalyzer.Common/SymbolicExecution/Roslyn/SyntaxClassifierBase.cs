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

namespace SonarAnalyzer.SymbolicExecution.Roslyn;

/// <summary>
/// This class violates the basic principle that SE should only depend on IOperation.
///
/// Anything added here needs to have extremely rare reason why it exists.
/// </summary>
public abstract class SyntaxClassifierBase
{
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

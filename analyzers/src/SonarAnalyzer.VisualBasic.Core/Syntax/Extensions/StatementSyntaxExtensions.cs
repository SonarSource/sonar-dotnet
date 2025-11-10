/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.VisualBasic.Core.Syntax.Extensions;

public static class StatementSyntaxExtensions
{
    public static StatementSyntax GetPrecedingStatement(this StatementSyntax currentStatement)
    {
        var children = currentStatement.Parent.ChildNodes().ToList();
        var index = children.IndexOf(currentStatement);
        return index == 0 ? null : children[index - 1] as StatementSyntax;
    }

    public static StatementSyntax GetSucceedingStatement(this StatementSyntax currentStatement)
    {
        var children = currentStatement.Parent.ChildNodes().ToList();
        var index = children.IndexOf(currentStatement);
        return index == children.Count - 1 ? null : children[index + 1] as StatementSyntax;
    }
}

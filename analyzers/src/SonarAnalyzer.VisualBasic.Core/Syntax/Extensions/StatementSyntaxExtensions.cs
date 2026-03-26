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
    extension(StatementSyntax currentStatement)
    {
        public StatementSyntax PrecedingStatement =>
            currentStatement.Parent.ChildNodes()
                .TakeWhile(x => x != currentStatement)
                .LastOrDefault() as StatementSyntax;

        public StatementSyntax SucceedingStatement =>
            currentStatement.Parent.ChildNodes()
                .SkipWhile(x => x != currentStatement)
                .Skip(1)
                .FirstOrDefault() as StatementSyntax;
    }
}

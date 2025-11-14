/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

public static class MethodBlockBaseSyntaxExtensions
{
    public static Location FindIdentifierLocation(this MethodBlockBaseSyntax methodBlockBase) =>
        GetIdentifierOrDefault(methodBlockBase)?.GetLocation();

    public static SyntaxToken? GetIdentifierOrDefault(this MethodBlockBaseSyntax methodBlockBase) =>
        methodBlockBase?.BlockStatement switch
        {
            SubNewStatementSyntax subNewStatement => subNewStatement.NewKeyword,
            MethodStatementSyntax methodStatement => methodStatement.Identifier,
            _ => null,
        };
}

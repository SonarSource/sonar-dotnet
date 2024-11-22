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

namespace SonarAnalyzer.VisualBasic.Core.Syntax.Extensions;

public static class MethodBlockSyntaxExtensions
{
    public static bool IsShared(this MethodBlockSyntax methodBlock) =>
        methodBlock.SubOrFunctionStatement.Modifiers.Any(SyntaxKind.SharedKeyword);

    public static string GetIdentifierText(this MethodBlockSyntax method) =>
        method.SubOrFunctionStatement.Identifier.ValueText;

    public static SeparatedSyntaxList<ParameterSyntax>? GetParameters(this MethodBlockSyntax method) =>
        method.BlockStatement?.ParameterList?.Parameters;
}

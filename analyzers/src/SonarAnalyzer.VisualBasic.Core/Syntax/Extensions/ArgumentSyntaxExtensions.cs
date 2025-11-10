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

public static class ArgumentSyntaxExtensions
{
    internal static int? GetArgumentIndex(this ArgumentSyntax argument) =>
        (argument.Parent as ArgumentListSyntax)?.Arguments.IndexOf(argument);

    internal static IEnumerable<ISymbol> GetSymbolsOfKnownType(this SeparatedSyntaxList<ArgumentSyntax> syntaxList, KnownType knownType, SemanticModel semanticModel) =>
        syntaxList.GetArgumentsOfKnownType(knownType, semanticModel)
                  .Select(argument => semanticModel.GetSymbolInfo(argument.GetExpression()).Symbol);

    private static IEnumerable<ArgumentSyntax> GetArgumentsOfKnownType(this SeparatedSyntaxList<ArgumentSyntax> syntaxList, KnownType knownType, SemanticModel semanticModel) =>
        syntaxList.Where(argument => semanticModel.GetTypeInfo(argument.GetExpression()).Type.Is(knownType));
}

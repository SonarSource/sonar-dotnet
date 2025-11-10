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

namespace SonarAnalyzer.Core.Semantics.Extensions;

public static class SymbolInfoExtensions
{
    /// <summary>
    /// Returns the <see cref="SymbolInfo.Symbol"/> or if no symbol could be found the <see cref="SymbolInfo.CandidateSymbols"/>.
    /// </summary>
    public static IEnumerable<ISymbol> AllSymbols(this SymbolInfo symbolInfo) =>
        symbolInfo.Symbol is null
            ? symbolInfo.CandidateSymbols
            : new[] { symbolInfo.Symbol };
}

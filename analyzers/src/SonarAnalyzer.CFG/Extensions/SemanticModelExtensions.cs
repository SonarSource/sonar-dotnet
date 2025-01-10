/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.CFG.Extensions;

public static class SemanticModelExtensions
{
    /// <summary>
    /// Starting .NET Framework 4.6.1, we've noticed that LINQ methods aren't resolved properly, so we need to use the CandidateSymbol.
    /// </summary>
    /// <param name="model">Semantic model</param>
    /// /// <param name="node">Node for which it gets the symbol</param>
    /// <returns>
    /// The symbol if resolved.
    /// The first candidate symbol if resolution failed.
    /// Null if no symbol was found.
    /// </returns>
    public static ISymbol GetSymbolOrCandidateSymbol(this SemanticModel model, SyntaxNode node)
    {
        var symbolInfo = model.GetSymbolInfo(node);
        if (symbolInfo.Symbol is not null)
        {
            return symbolInfo.Symbol;
        }
        return symbolInfo.CandidateSymbols.FirstOrDefault();
    }
}

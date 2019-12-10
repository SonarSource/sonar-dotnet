/*
 * Copyright (C) 2018-2019 SonarSource SA
 * All rights reserved
 * mailto:info AT sonarsource DOT com
 */

using System.Linq;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    internal static class SemanticModelHelper
    {
        public static SemanticModel GetSyntaxTreeSemanticModel(this SemanticModel model, SyntaxNode node)
        {
            // See https://github.com/dotnet/roslyn/issues/18730
            return model.SyntaxTree == node.SyntaxTree
                ? model
                : model.Compilation.GetSemanticModel(node.SyntaxTree);
        }

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
            if (symbolInfo.Symbol != null)
            {
                return symbolInfo.Symbol;
            }
            return symbolInfo.CandidateSymbols.FirstOrDefault();
        }
    }
}

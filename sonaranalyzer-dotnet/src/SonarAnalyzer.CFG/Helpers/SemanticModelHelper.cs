/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

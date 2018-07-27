/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using Microsoft.CodeAnalysis;
using UcfgLocation = SonarAnalyzer.Protobuf.Ucfg.Location;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    internal static class UcfgHelper
    {
        /// <summary>
        /// Returns UCFG Location that represents the location of the provided SyntaxNode
        /// in SonarQube coordinates - 1-based line numbers and 0-based columns (line offsets).
        /// Roslyn coordinates are 0-based.
        /// </summary>
        public static UcfgLocation GetUcfgLocation(this SyntaxNode syntaxNode)
        {
            if (syntaxNode == null)
            {
                return null;
            }

            return GetUcfgLocation(syntaxNode.GetLocation());
        }

        public static UcfgLocation GetUcfgLocation(this SyntaxToken syntaxToken)
        {
            if (syntaxToken == default(SyntaxToken))
            {
                return null;
            }

            return GetUcfgLocation(syntaxToken.GetLocation());
        }

        private static UcfgLocation GetUcfgLocation(Location location)
        {
            // The startLineOffset gives the first character that should be included (zero-based)
            // The endLineOffset gives the character *after* the last character that should be included (zero-based)
            // So the startLineOffset/endLineOffset for this method id ("GetUcfgLocation") is 36, 51.
            var lineSpan = location.GetLineSpan();
            return new UcfgLocation
            {
                FileId = location.SourceTree.FilePath,
                StartLine = lineSpan.StartLinePosition.Line + 1,
                StartLineOffset = lineSpan.StartLinePosition.Character,
                EndLine = lineSpan.EndLinePosition.Line + 1,
                EndLineOffset = lineSpan.EndLinePosition.Character,
            };
        }

        public static string ToUcfgMethodId(this IMethodSymbol methodSymbol)
        {
            switch (methodSymbol?.MethodKind)
            {
                case MethodKind.ExplicitInterfaceImplementation:
                    return methodSymbol.ConstructedFrom.ToDisplayString();

                case MethodKind.ReducedExtension:
                    return methodSymbol.ReducedFrom.ToDisplayString();

                default:
                    return methodSymbol?.OriginalDefinition?.ToDisplayString()
                        ?? UcfgBuiltInMethodId.Unknown;
            }
        }
    }
}

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

using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    public static class VbcHelper
    {
        public static readonly Regex VbNetErrorPattern = new Regex(@"\s+error\s*:", RegexOptions.Compiled |
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// VB.Net complier (VBC) post-process issues and will fail if the line contains the VbNetErrorPattern.
        /// </summary>
        /// <remarks>
        /// This helper method is intended to be used only while waiting for the bug to be fixed on Microsoft side.
        /// <see cref="https://github.com/dotnet/roslyn/issues/5724"/>.
        /// </remarks>
        /// <param name="diagnostic">
        /// The diagnostic to test.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> when reporting the diagnostic will trigger a VBC post-process error and <c>false</c> otherwise.
        /// </returns>
        public static bool IsTriggeringVbcError(Diagnostic diagnostic)
        {
            var rootNode = diagnostic.Location.SourceTree?.GetRoot();

            if (rootNode != null &&
                rootNode.Language == LanguageNames.VisualBasic)
            {
                var diagnosticNode = rootNode.FindNode(diagnostic.Location.SourceSpan);
                var lineContent = diagnosticNode?.ToString();

                if (lineContent != null &&
                    VbNetErrorPattern.IsMatch(lineContent))
                {
                    return true;
                }
            }

            return false;
        }
    }

}

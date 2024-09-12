/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Helpers;

public static class SyntaxNodeExtensions
{
    public static SemanticModel EnsureCorrectSemanticModelOrDefault(this SyntaxNode node, SemanticModel model) =>
        node.SyntaxTree.GetSemanticModelOrDefault(model);

    public static bool ToStringContains(this SyntaxNode node, string s) =>
        node.ToString().Contains(s);

    public static bool ToStringContains(this SyntaxNode node, string s, StringComparison comparison) =>
        node.ToString().IndexOf(s, comparison) != -1;

    public static bool ToStringContainsEitherOr(this SyntaxNode node, string a, string b)
    {
        var toString = node.ToString();
        return toString.Contains(a) || toString.Contains(b);
    }

    public static string GetMappedFilePathFromRoot(this SyntaxNode root)
    {
        if (root.DescendantTrivia().FirstOrDefault() is { RawKind: (ushort)SyntaxKindEx.PragmaChecksumDirectiveTrivia } pragmaChecksum)
        {
            // The format is: #pragma checksum "filename" "{guid}" "checksum bytes"
            // See https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/preprocessor-directives#pragma-checksum
            var content = pragmaChecksum.ToString();
            var firstIndex = content.IndexOf('"');
            var start = firstIndex + 1;
            var secondIndex = content.IndexOf('"', start);
            return content.Substring(start, secondIndex - start);
        }

        return root.SyntaxTree.FilePath;
    }

    public static TSyntaxKind Kind<TSyntaxKind>(this SyntaxNode node) where TSyntaxKind : struct, Enum =>
        node is null ? default : (TSyntaxKind)Enum.ToObject(typeof(TSyntaxKind), node.RawKind);

    public static SecondaryLocation ToSecondaryLocation(this SyntaxNode node, string message = null) =>
        new(node.GetLocation(), message);
}

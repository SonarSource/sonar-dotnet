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

using System.Runtime.CompilerServices;
using Roslyn.Utilities;

namespace SonarAnalyzer.Extensions;

internal static class SyntaxTreeExtensions
{
    private static readonly ConditionalWeakTable<SyntaxTree, object> GeneratedCodeCache = new();

    [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7439", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
    public static bool IsGenerated(this SyntaxTree tree, GeneratedCodeRecognizer generatedCodeRecognizer) =>
        tree switch
        {
            null => false,
            _ => GeneratedCodeCache.TryGetValue(tree, out var result)
                ? (bool)result
                : IsGeneratedGetOrAdd(tree, generatedCodeRecognizer)
        };

    private static bool IsGeneratedGetOrAdd(SyntaxTree tree, GeneratedCodeRecognizer generatedCodeRecognizer) =>
        (bool)GeneratedCodeCache.GetValue(tree, tree => generatedCodeRecognizer.IsGenerated(tree));

    public static bool IsConsideredGenerated(this SyntaxTree tree, GeneratedCodeRecognizer generatedCodeRecognizer, bool isRazorAnalysisEnabled) =>
        isRazorAnalysisEnabled
            ? IsGenerated(tree, generatedCodeRecognizer) && !GeneratedCodeRecognizer.IsRazorGeneratedFile(tree)
            : IsGenerated(tree, generatedCodeRecognizer);

    public static string GetOriginalFilePath(this SyntaxTree tree) =>
        // Currently we support only C# based generated files.
        tree.GetRoot().DescendantNodes(_ => true, true).OfType<Microsoft.CodeAnalysis.CSharp.Syntax.PragmaChecksumDirectiveTriviaSyntax>().FirstOrDefault() is { } pragmaChecksum
            ? pragmaChecksum.File.ValueText
            : tree.FilePath;
}

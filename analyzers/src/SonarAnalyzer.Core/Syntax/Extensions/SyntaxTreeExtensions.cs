/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Runtime.CompilerServices;
using Roslyn.Utilities;

namespace SonarAnalyzer.Core.Syntax.Extensions;

internal static class SyntaxTreeExtensions
{
    private static readonly ConditionalWeakTable<SyntaxTree, object> GeneratedCodeCache = new();

    [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7439", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
    public static bool IsGenerated(this SyntaxTree tree, GeneratedCodeRecognizer generatedCodeRecognizer)
    {
        if (tree is null)
        {
            return false;
        }
        else
        {
            return GeneratedCodeCache.TryGetValue(tree, out var result)
                ? (bool)result
                : IsGeneratedGetOrAdd(tree, generatedCodeRecognizer);
        }
    }

    public static bool IsConsideredGenerated(this SyntaxTree tree, GeneratedCodeRecognizer generatedCodeRecognizer, bool isRazorAnalysisEnabled) =>
        isRazorAnalysisEnabled
            ? tree.IsGenerated(generatedCodeRecognizer) && !GeneratedCodeRecognizer.IsRazorGeneratedFile(tree)
            : tree.IsGenerated(generatedCodeRecognizer);

    public static string GetOriginalFilePath(this SyntaxTree tree) =>
        // Currently we support only C# based generated files.
        tree.GetRoot().DescendantNodes(_ => true, true).OfType<Microsoft.CodeAnalysis.CSharp.Syntax.PragmaChecksumDirectiveTriviaSyntax>().FirstOrDefault() is { } pragmaChecksum
            ? pragmaChecksum.File.ValueText
            : tree.FilePath;

    public static bool EndsWith(this SyntaxTree tree, string suffix) =>
        tree.FilePath.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);

    private static bool IsGeneratedGetOrAdd(SyntaxTree tree, GeneratedCodeRecognizer generatedCodeRecognizer) =>
        (bool)GeneratedCodeCache.GetValue(tree, x => generatedCodeRecognizer.IsGenerated(x));
}

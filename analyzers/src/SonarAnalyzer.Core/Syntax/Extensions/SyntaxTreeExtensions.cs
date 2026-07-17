/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

    extension(SyntaxTree tree)
    {
        public string OriginalFilePath =>
            // Currently we support only C# based generated files.
            tree.GetRoot().DescendantNodes(_ => true, true).OfType<Microsoft.CodeAnalysis.CSharp.Syntax.PragmaChecksumDirectiveTriviaSyntax>().FirstOrDefault() is { } pragmaChecksum
                ? pragmaChecksum.File.ValueText
                : tree.FilePath;

        [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7439", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
        public bool IsGenerated(GeneratedCodeRecognizer generatedCodeRecognizer)
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

        public bool IsConsideredGenerated(GeneratedCodeRecognizer generatedCodeRecognizer, bool isRazorAnalysisEnabled) =>
            isRazorAnalysisEnabled
                ? tree.IsGenerated(generatedCodeRecognizer) && !GeneratedCodeRecognizer.IsRazorGeneratedFile(tree)
                : tree.IsGenerated(generatedCodeRecognizer);

        public bool EndsWith(string suffix) =>
            tree.FilePath.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsGeneratedGetOrAdd(SyntaxTree tree, GeneratedCodeRecognizer generatedCodeRecognizer) =>
        (bool)GeneratedCodeCache.GetValue(tree, x => generatedCodeRecognizer.IsGenerated(x));
}

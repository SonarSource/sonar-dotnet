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

using System.IO;
using Roslyn.Utilities;
using SonarAnalyzer.Core.Configuration;

namespace SonarAnalyzer.Core.Extensions;

public static class AnalyzerOptionsExtensions
{
    private static readonly SourceTextValueProvider<SonarLintXmlReader> SonarLintXmlProvider = new(x => new SonarLintXmlReader(x));

    public static SonarLintXmlReader SonarLintXml(this AnalyzerOptions options, SonarAnalysisContext context)
    {
        if (options.SonarLintXml() is { } sonarLintXml)
        {
            return sonarLintXml.GetText() is { } sourceText
                && context.TryGetValue(sourceText, SonarLintXmlProvider, out var sonarLintXmlReader)
                ? sonarLintXmlReader
                : throw new InvalidOperationException($"File '{Path.GetFileName(sonarLintXml.Path)}' has been added as an AdditionalFile but could not be read and parsed.");
        }
        else
        {
            return SonarLintXmlReader.Empty;
        }
    }

    public static AdditionalText SonarLintXml(this AnalyzerOptions options) =>
        options.AdditionalFile("SonarLint.xml");

    public static AdditionalText SonarProjectConfig(this AnalyzerOptions options) =>
        options.AdditionalFile("SonarProjectConfig.xml");

    public static AdditionalText ProjectOutFolderPath(this AnalyzerOptions options) =>
        options.AdditionalFile("ProjectOutFolderPath.txt");

    [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7440", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
    private static AdditionalText AdditionalFile(this AnalyzerOptions options, string fileName)
    {
        // HotPath: This code path needs to be allocation free. Don't use Linq.
        foreach (var additionalText in options.AdditionalFiles) // Uses the struct enumerator of ImmutableArray
        {
            // Don't use Path.GetFilename. It allocates a string.
            if (additionalText.Path is { } path
                && path.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
            {
                // The character before the filename (if there is a character) must be a directory separator
                var separatorPosition = path.Length - fileName.Length - 1;
                if (separatorPosition < 0 || IsDirectorySeparator(path[separatorPosition]))
                {
                    return additionalText;
                }
            }
        }
        return null;

        static bool IsDirectorySeparator(char c) =>
            c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;
    }
}

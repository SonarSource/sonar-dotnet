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

using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;
using SonarAnalyzer.Core.Configuration;

namespace SonarAnalyzer.Core.Extensions;

public static class AnalyzerOptionsExtensions
{
    private static readonly SourceTextValueProvider<SonarLintXmlReader> SonarLintXmlProvider = new(x => new SonarLintXmlReader(x));
    private static readonly SourceTextValueProvider<ImmutableHashSet<string>> CustomDictionaryAcronymsProvider = new(ParseCustomDictionaryAcronyms);

    extension(AnalyzerOptions options)
    {
        public SonarLintXmlReader SonarLintXml(SonarAnalysisContext context)
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

        public AdditionalText SonarLintXml() =>
            options.AdditionalFile("SonarLint.xml");

        public AdditionalText SonarProjectConfig() =>
            options.AdditionalFile("SonarProjectConfig.xml");

        public AdditionalText ProjectOutFolderPath() =>
            options.AdditionalFile("ProjectOutFolderPath.txt");

        public AdditionalText CustomDictionary() =>
            options.AdditionalFile("CustomDictionary.xml");

        // See https://learn.microsoft.com/en-us/visualstudio/code-quality/how-to-customize-the-code-analysis-dictionary for the expected XML format.
        public ImmutableHashSet<string> CustomDictionaryAcronyms(SonarCompilationStartAnalysisContext context) =>
            options.CustomDictionary()?.GetText() is { } text
            && context.TryGetValue(text, CustomDictionaryAcronymsProvider, out var acronyms)
                ? acronyms
                : ImmutableHashSet<string>.Empty;

        [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7440", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
        private AdditionalText AdditionalFile(string fileName)
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

    internal static ImmutableHashSet<string> ParseCustomDictionaryAcronyms(SourceText text)
    {
        try
        {
            var acronyms = XDocument.Parse(text.ToString()).Root is { Name.LocalName: "Dictionary" } root
                ? root.Element("Acronyms")?.Element("CasingExceptions")?.Elements("Acronym")
                : null;
            return (acronyms ?? [])
                .Select(x => x.Value.Trim().ToUpperInvariant())
                .Where(x => x.Length > 0)
                .ToImmutableHashSet();
        }
        catch (XmlException)
        {
            return ImmutableHashSet<string>.Empty;
        }
    }
}

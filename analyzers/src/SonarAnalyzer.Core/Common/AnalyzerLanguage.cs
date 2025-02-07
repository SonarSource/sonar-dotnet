/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Common;

public sealed class AnalyzerLanguage
{
    public static readonly AnalyzerLanguage CSharp = new(LanguageNames.CSharp, ".cs", "https://rules.sonarsource.com/csharp/RSPEC-{0}");
    public static readonly AnalyzerLanguage VisualBasic = new(LanguageNames.VisualBasic, ".vb", "https://rules.sonarsource.com/vbnet/RSPEC-{0}");

    private readonly string helpLinkFormat;

    public string LanguageName { get; }
    public string FileExtension { get; }

    private AnalyzerLanguage(string languageName, string fileExtension, string helpLinkFormat)
    {
        LanguageName = languageName;
        FileExtension = fileExtension;
        this.helpLinkFormat = helpLinkFormat;
    }

    public override string ToString() =>
        LanguageName;

    public static AnalyzerLanguage FromName(string name) =>
        name switch
        {
            LanguageNames.CSharp => CSharp,
            LanguageNames.VisualBasic => VisualBasic,
            _ => throw new NotSupportedException("Unsupported language name: " + name)
        };

    public static AnalyzerLanguage FromPath(string path)
    {
        var comparer = StringComparer.OrdinalIgnoreCase;
        return Path.GetExtension(path) switch
        {
            { } ext when comparer.Equals(ext, ".cs") || comparer.Equals(ext, ".razor") || comparer.Equals(ext, ".cshtml") => CSharp,
            { } ext when comparer.Equals(ext, ".vb") => VisualBasic,
            _ => throw new NotSupportedException("Unsupported file extension: " + Path.GetExtension(path))
        };
    }

    public string HelpLink(string id) =>
        id.StartsWith("S") ? string.Format(helpLinkFormat, id.Substring(1)) : null; // Not relevant for styling rules Txxxx
}

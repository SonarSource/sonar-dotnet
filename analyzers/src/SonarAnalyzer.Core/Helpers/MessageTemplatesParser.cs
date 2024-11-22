/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using System.Text.RegularExpressions;

namespace SonarAnalyzer.Helpers;

/// <summary>
/// Grammar can be found <a href="https://github.com/messagetemplates/grammar"> here</a>.
/// </summary>
public static class MessageTemplatesParser
{
    private const string NamePattern = "[0-9a-zA-Z_]+";
    private const string PlaceholderPattern = $"(?<Placeholder>{NamePattern})";
    private const string AlignmentPattern = "(,-?[0-9]+)?";
    private const string FormatPattern = @"(:[^\}]+)?";

    private const string HolePattern = "{" + "[@$]?" + PlaceholderPattern + AlignmentPattern + FormatPattern + "}";
    private const string TextPattern = @"([^\{]|\{\{|\}\})+";
    private const string TemplatePattern = $"^({TextPattern}|{HolePattern})*$";

    private static readonly Regex TemplateRegex = new(TemplatePattern, RegexOptions.Compiled, TimeSpan.FromMilliseconds(300));

    /// <summary>
    /// Matches and extracts placeholders from a template string.
    /// For more info, see <a href="https://messagetemplates.org/">Message Templates</a>.
    /// </summary>
    public static ParseResult Parse(string template) =>
        Parse(template, TemplateRegex);

    public static ParseResult Parse(string template, Regex regex)
    {
        if (regex.SafeMatch(template) is { Success: true } match)
        {
            var placeholders = match.Groups["Placeholder"].Captures
                 .OfType<Capture>()
                 .Select(x => new Placeholder(x.Value, x.Index, x.Length))
                 .ToArray();
            return new(true, placeholders);
        }
        else
        {
            return new(false);
        }
    }

    public record ParseResult(bool Success, Placeholder[] Placeholders = null);

    public record Placeholder(string Name, int Start, int Length);
}

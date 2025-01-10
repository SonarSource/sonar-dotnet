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

namespace SonarAnalyzer.Core.Analyzers;

public static class DiagnosticDescriptorFactory
{
    public static readonly string SonarWayTag = "SonarWay";
    public static readonly string UtilityTag = "Utility";
    public static readonly string MainSourceScopeTag = "MainSourceScope";
    public static readonly string TestSourceScopeTag = "TestSourceScope";

    public static DiagnosticDescriptor CreateUtility(string diagnosticId, string title) =>
        new(diagnosticId,
            title,
            string.Empty,
            string.Empty,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            customTags: BuildUtilityTags());

    public static DiagnosticDescriptor Create(AnalyzerLanguage language, RuleDescriptor rule, string messageFormat, bool? isEnabledByDefault, bool fadeOutCode) =>
        new(rule.Id,
            rule.Title,
            messageFormat,
            rule.Category,
            fadeOutCode ? DiagnosticSeverity.Info : DiagnosticSeverity.Warning,
            rule.IsHotspot || (isEnabledByDefault ?? rule.SonarWay),
            rule.Description,
            language.HelpLink(rule.Id),
            BuildTags(language, rule, fadeOutCode));

    private static string[] BuildTags(AnalyzerLanguage language, RuleDescriptor rule, bool fadeOutCode)
    {
        var tags = new List<string> { language.LanguageName };
        tags.AddRange(rule.Scope.ToTags());
        Add(rule.SonarWay, SonarWayTag);
        Add(fadeOutCode, WellKnownDiagnosticTags.Unnecessary);
        return tags.ToArray();

        void Add(bool condition, string tag)
        {
            if (condition)
            {
                tags.Add(tag);
            }
        }
    }

    private static IEnumerable<string> ToTags(this SourceScope sourceScope) =>
        sourceScope switch
        {
            SourceScope.Main => new[] { MainSourceScopeTag },
            SourceScope.Tests => new[] { TestSourceScopeTag },
            SourceScope.All => new[] { MainSourceScopeTag, TestSourceScopeTag },
            _ => throw new NotSupportedException($"{sourceScope} is not supported 'SourceScope' value."),
        };

    private static string[] BuildUtilityTags() =>
        SourceScope.All.ToTags().Concat(new[] { UtilityTag })
#if !DEBUG
            // Allow to configure the analyzers in debug mode only. This allows to run test selectively (for example to test only one rule)
            .Union(new[] { WellKnownDiagnosticTags.NotConfigurable })
#endif
            .ToArray();
}

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

namespace SonarAnalyzer.Core.Analyzers;

public static class DiagnosticDescriptorFactory
{
    public static readonly string SonarWayTag = "SonarWay";
    public static readonly string UtilityTag = "Utility";
    public static readonly string MainSourceScopeTag = "MainSourceScope";
    public static readonly string TestSourceScopeTag = "TestSourceScope";

    /// <summary>
    /// Indicates that the diagnostic is a compilation end diagnostic reported from a compilation end action.
    /// </summary>
    /// <remarks>
    /// See also:
    /// <list type="table">
    /// <item>
    /// <see href="https://github.com/dotnet/roslyn/blob/371953b0685063eae905ac3afa12028d73ecbfa8/src/Compilers/Core/Portable/Diagnostic/WellKnownDiagnosticTags.cs#L64-L68">WellKnownDiagnosticTags.cs</see>
    /// </item>
    /// <item>
    /// <see href="https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/Microsoft.CodeAnalysis.Analyzers.md#rs1037-add-compilationend-custom-tag-to-compilation-end-diagnostic-descriptor">RS1037</see>
    /// </item>
    /// </list>
    /// </remarks>
    public static readonly string CompilationEnd = nameof(CompilationEnd);

    public static DiagnosticDescriptor CreateUtility(string diagnosticId, string title) =>
        new(diagnosticId,
            title,
            string.Empty,
            string.Empty,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            customTags: BuildUtilityTags());

    public static DiagnosticDescriptor Create(AnalyzerLanguage language, RuleDescriptor rule, string messageFormat, bool? isEnabledByDefault, bool fadeOutCode, bool isCompilationEnd) =>
        new(rule.Id,
            rule.Title,
            messageFormat,
            rule.Category,
            fadeOutCode ? DiagnosticSeverity.Info : DiagnosticSeverity.Warning,
            rule.IsHotspot || (isEnabledByDefault ?? rule.SonarWay),
            rule.Description,
            null,
            BuildTags(language, rule, fadeOutCode, isCompilationEnd));

    private static string[] BuildTags(AnalyzerLanguage language, RuleDescriptor rule, bool fadeOutCode, bool isCompilationEnd)
    {
        var tags = new List<string> { language.LanguageName };
        tags.AddRange(rule.Scope.ToTags());
        Add(rule.SonarWay, SonarWayTag);
        Add(fadeOutCode, WellKnownDiagnosticTags.Unnecessary);
        Add(isCompilationEnd, CompilationEnd);
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

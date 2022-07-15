/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.Helpers
{
    public static class DiagnosticDescriptorBuilder
    {
        public static readonly string SonarWayTag = "SonarWay";
        public static readonly string UtilityTag = "Utility";
        public static readonly string MainSourceScopeTag = "MainSourceScope";
        public static readonly string TestSourceScopeTag = "TestSourceScope";

        public static DiagnosticDescriptor GetUtilityDescriptor(string diagnosticId, string title) =>
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
                isEnabledByDefault ?? rule.SonarWay,
                rule.Description,
                language.HelpLink(rule.Id),
                BuildTags(language.LanguageName, rule.SonarWay, rule.Scope, fadeOutCode));

        /// <summary>
        /// Indicates that the Roslyn diagnostic cannot be suppressed, filtered or have its severity changed.
        /// </summary>
        public static DiagnosticDescriptor WithNotConfigurable(this DiagnosticDescriptor dd) =>
            new(dd.Id,
                dd.Title,
                dd.MessageFormat,
                dd.Category,
                dd.DefaultSeverity,
                true,
                dd.Description,
                dd.HelpLinkUri,
                dd.CustomTags.Union(new[] { WellKnownDiagnosticTags.NotConfigurable }).ToArray());

        private static string[] BuildTags(string languageName, bool sonarWay, SourceScope scope, bool fadeOutCode)
        {
            var tags = new List<string> { languageName };
            tags.AddRange(scope.ToTags());
            if (sonarWay)
            {
                tags.Add(SonarWayTag);
            }
            if (fadeOutCode)
            {
                tags.Add(WellKnownDiagnosticTags.Unnecessary);
            }
            return tags.ToArray();
        }

        private static IEnumerable<string> ToTags(this SourceScope sourceScope) =>
            sourceScope switch
            {
                SourceScope.Main => new[] { MainSourceScopeTag },
                SourceScope.Tests => new[] { TestSourceScopeTag },
                SourceScope.All => new[] { MainSourceScopeTag, TestSourceScopeTag },
                _ => throw new NotSupportedException($"{sourceScope} is not supported 'SourceScope' value."),
            };

        private static string[] BuildUtilityTags()
        {
            return SourceScope.All.ToTags().Concat(new[] { UtilityTag })
#if !DEBUG
                // Allow to configure the analyzers in debug mode only.
                // This allows to run test selectively (for example to test only one rule)
                .Union(new[] { WellKnownDiagnosticTags.NotConfigurable })
#endif
                .ToArray();
        }
    }
}

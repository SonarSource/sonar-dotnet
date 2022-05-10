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
using System.Resources;
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
            new(
                diagnosticId,
                title,
                string.Empty,
                string.Empty,
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                customTags: BuildUtilityCustomTags());

        public static DiagnosticDescriptor Create(AnalyzerLanguage language, RuleDescriptor rule, string messageFormat, bool fadeOutCode) =>
            new(
                rule.Id,
                rule.Title,
                messageFormat,
                rule.Category,
                fadeOutCode ? DiagnosticSeverity.Info : DiagnosticSeverity.Warning,
                rule.SonarWay,
                rule.Description,
                language.HelpLink(rule.Id),
                BuildTags(language.LanguageName, rule.SonarWay, rule.Scope, fadeOutCode));

        [Obsolete("Use DescriptorFactory.Create()")]
        public static DiagnosticDescriptor GetDescriptor(string diagnosticId, string messageFormat,
            ResourceManager resourceManager, bool? isEnabledByDefault = null, bool fadeOutCode = false) =>
            new(
                diagnosticId,
                resourceManager.GetString($"{diagnosticId}_Title"),
                messageFormat,
                resourceManager.GetString($"{diagnosticId}_Category"),
                fadeOutCode ? DiagnosticSeverity.Info : DiagnosticSeverity.Warning,
                isEnabledByDefault ?? bool.Parse(resourceManager.GetString($"{diagnosticId}_IsActivatedByDefault")),
                helpLinkUri: GetHelpLink(resourceManager, diagnosticId),
                description: resourceManager.GetString($"{diagnosticId}_Description"),
                customTags: BuildTags(diagnosticId, resourceManager, fadeOutCode));

        [Obsolete]
        public static string GetHelpLink(ResourceManager resourceManager, string diagnosticId) =>
            string.Format(resourceManager.GetString("HelpLinkFormat"), diagnosticId.Substring(1));

        /// <summary>
        /// Indicates that the Roslyn diagnostic cannot be suppressed, filtered or have its severity changed.
        /// </summary>
        public static DiagnosticDescriptor WithNotConfigurable(this DiagnosticDescriptor dd) =>
            new(
                dd.Id,
                dd.Title,
                dd.MessageFormat,
                dd.Category,
                dd.DefaultSeverity,
                true,
                dd.Description,
                dd.HelpLinkUri,
                dd.CustomTags.Union(new[] { WellKnownDiagnosticTags.NotConfigurable }).ToArray());

        [Obsolete]
        private static string[] BuildTags(string diagnosticId, ResourceManager resourceManager, bool fadeOutCode) =>
            BuildTags(
                resourceManager.GetString("RoslynLanguage"),
                bool.Parse(resourceManager.GetString($"{diagnosticId}_IsActivatedByDefault")),
                (SourceScope)Enum.Parse(typeof(SourceScope), resourceManager.GetString($"{diagnosticId}_Scope")),
                fadeOutCode);

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

        private static string[] BuildUtilityCustomTags()
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

    public record RuleDescriptor(string Id, string Title, string Type, string DefaultSeverity, SourceScope Scope, bool SonarWay, string Description)
    {
        public string Category =>
            $"{DefaultSeverity} {ReadableType}";

        private string ReadableType =>
            Type switch
            {
                "BUG" => "Bug",
                "CODE_SMELL" => "Code Smell",
                "VULNERABILITY" => "Vulnerability",
                "SECURITY_HOTSPOT" => "Security Hotspot",
                _ => throw new UnexpectedValueException(nameof(Type), Type)
            };
    }
}

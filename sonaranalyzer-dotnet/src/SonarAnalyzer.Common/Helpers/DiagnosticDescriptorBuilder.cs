/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

namespace SonarAnalyzer.Helpers
{
    public static class DiagnosticDescriptorBuilder
    {
        public static readonly string SonarWayTag = "SonarWay";
        public static readonly string MainSourceScopeTag = "MainSourceScope";
        public static readonly string TestSourceScopeTag = "TestSourceScope";

        public static DiagnosticDescriptor GetUtilityDescriptor(string diagnosticId, string title,
            SourceScope sourceScope = SourceScope.All) =>
            new DiagnosticDescriptor(
                diagnosticId,
                title,
                string.Empty,
                string.Empty,
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                customTags: BuildUtilityCustomTags(sourceScope));

        public static DiagnosticDescriptor GetDescriptor(string diagnosticId, string messageFormat,
            ResourceManager resourceManager, bool? isEnabledByDefault = null, bool fadeOutCode = false) =>
            new DiagnosticDescriptor(
                diagnosticId,
                resourceManager.GetString($"{diagnosticId}_Title"),
                messageFormat,
                resourceManager.GetString($"{diagnosticId}_Category"),
                fadeOutCode ? DiagnosticSeverity.Info : DiagnosticSeverity.Warning,
                isEnabledByDefault ?? bool.Parse(resourceManager.GetString($"{diagnosticId}_IsActivatedByDefault")),
                helpLinkUri: GetHelpLink(resourceManager, diagnosticId),
                description: resourceManager.GetString($"{diagnosticId}_Description"),
                customTags: BuildCustomTags(diagnosticId, resourceManager, fadeOutCode));

        public static string GetHelpLink(ResourceManager resourceManager, string diagnosticId) =>
            string.Format(resourceManager.GetString("HelpLinkFormat"), diagnosticId.Substring(1));

        private static string[] BuildCustomTags(string diagnosticId, ResourceManager resourceManager, bool fadeOutCode)
        {
            var tags = new List<string> { resourceManager.GetString("RoslynLanguage") };

            if (bool.Parse(resourceManager.GetString($"{diagnosticId}_IsActivatedByDefault")))
            {
                tags.Add(SonarWayTag);
            }

            if (Enum.TryParse<SourceScope>(resourceManager.GetString($"{diagnosticId}_Scope"), out var sourceScope))
            {
                tags.AddRange(sourceScope.ToCustomTags());
            }

            if (fadeOutCode)
            {
                tags.Add(WellKnownDiagnosticTags.Unnecessary);
            }

            return tags.ToArray();
        }

        private static IEnumerable<string> ToCustomTags(this SourceScope sourceScope)
        {
            switch (sourceScope)
            {
                case SourceScope.Main:
                    return new[] { MainSourceScopeTag };
                case SourceScope.Tests:
                    return new[] { TestSourceScopeTag };
                case SourceScope.All:
                    return new[] { MainSourceScopeTag, TestSourceScopeTag };
                default:
                    throw new NotSupportedException($"{sourceScope} is not supported 'SourceScope' value.");
            }
        }

        private static string[] BuildUtilityCustomTags(SourceScope sourceScope)
        {
            return sourceScope.ToCustomTags()
#if !DEBUG
                // Allow to configure the analyzers in debug mode only.
                // This allows to run test selectively (for example to test only one rule)
                .Union(new[] { WellKnownDiagnosticTags.NotConfigurable })
#endif
                .ToArray();
        }

        /**
         * Indicates that the roslyn diagnostic cannot be suppressed, filtered or have its severity changed.
         */
        public static DiagnosticDescriptor WithNotConfigurable(this DiagnosticDescriptor dd) =>
            new DiagnosticDescriptor(
                dd.Id,
                dd.Title,
                dd.MessageFormat,
                dd.Category,
                dd.DefaultSeverity,
                true,
                dd.Description,
                dd.HelpLinkUri,
                dd.CustomTags.Union(new[] { WellKnownDiagnosticTags.NotConfigurable }).ToArray());
    }
}

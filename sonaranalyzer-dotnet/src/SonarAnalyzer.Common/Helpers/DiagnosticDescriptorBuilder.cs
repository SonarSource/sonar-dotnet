/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using System.Collections.Generic;
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
            string[] customTags = null) =>
            new DiagnosticDescriptor(
                diagnosticId,
                title,
                string.Empty,
                string.Empty,
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                customTags: customTags ?? BuildUtilityCustomTags());

        public static DiagnosticDescriptor GetDescriptor(string diagnosticId, string messageFormat,
            ResourceManager resourceManager, bool? isEnabledByDefault = null) =>
            new DiagnosticDescriptor(
                diagnosticId,
                resourceManager.GetString($"{diagnosticId}_Title"),
                messageFormat,
                resourceManager.GetString($"{diagnosticId}_Category"),
                DiagnosticSeverity.Warning, // We want all rules to be warning by default
                isEnabledByDefault ?? bool.Parse(resourceManager.GetString($"{diagnosticId}_IsActivatedByDefault")),
                helpLinkUri: GetHelpLink(resourceManager, diagnosticId),
                description: resourceManager.GetString($"{diagnosticId}_Description"),
                customTags: BuildCustomTags(diagnosticId, resourceManager));

        public static string GetHelpLink(ResourceManager resourceManager, string diagnosticId) =>
            string.Format(resourceManager.GetString("HelpLinkFormat"), diagnosticId.Substring(1));

        private static string[] BuildCustomTags(string diagnosticId, ResourceManager resourceManager)
        {
            var tags = new List<string> { resourceManager.GetString("RoslynLanguage") };

            if (bool.Parse(resourceManager.GetString($"{diagnosticId}_IsActivatedByDefault")))
            {
                tags.Add(SonarWayTag);
            }

            var scope = resourceManager.GetString($"{diagnosticId}_Scope");
            if (scope == "Main")
            {
                tags.Add(MainSourceScopeTag);
            }
            else if (scope == "Tests")
            {
                tags.Add(TestSourceScopeTag);
            }
            else if (scope == "All")
            {
                tags.Add(MainSourceScopeTag);
                tags.Add(TestSourceScopeTag);
            }
            else
            {
                // Do nothing
            }

            return tags.ToArray();
        }

        private static string[] BuildUtilityCustomTags()
        {
            return new[]
            {
#if !DEBUG
                // Allow to configure the analyzers in debug mode only.
                // This allows to run test selectively (for example to test only one rule)
                WellKnownDiagnosticTags.NotConfigurable,
#endif
                MainSourceScopeTag,
                TestSourceScopeTag
            };
        }
    }
}

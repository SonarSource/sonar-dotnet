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
        public static DiagnosticDescriptor GetDescriptor(string diagnosticId, string messageFormat,
            ResourceManager resourceManager)
        {
            return GetDescriptor(diagnosticId, messageFormat, IdeVisibility.Visible, resourceManager);
        }

        public static DiagnosticDescriptor GetDescriptor(string diagnosticId, string messageFormat,
            IdeVisibility ideVisibility, ResourceManager resourceManager)
        {
            return new DiagnosticDescriptor(
                diagnosticId,
                resourceManager.GetString($"{diagnosticId}_Title"),
                messageFormat,
                resourceManager.GetString($"{diagnosticId}_Category"),
                ParseSeverity(resourceManager.GetString($"{diagnosticId}_Severity")).ToDiagnosticSeverity(ideVisibility),
                isEnabledByDefault: true, // We want to have all rules enabled by default
                helpLinkUri: GetHelpLink(resourceManager, diagnosticId),
                description: resourceManager.GetString($"{diagnosticId}_Description"),
                customTags: BuildCustomTags(diagnosticId, ideVisibility, resourceManager));
        }

        public static string GetHelpLink(ResourceManager resourceManager, string diagnosticId)
        {
            var helpLinkFormat = resourceManager.GetString("HelpLinkFormat");
            return string.Format(helpLinkFormat, diagnosticId.Substring(1));
        }

        /// <summary>
        /// Creates a new DiagnosticDescriptor instance copying all the properties from the original, overriding the
        /// Severity.
        /// </summary>
        /// <param name="descriptor">DiagnosticDescriptor instance to copy.</param>
        /// <param name="severity">The new value of the Severity property.</param>
        /// <returns>A new DiagnosticDescriptor instance with overridden value of the Severity property.</returns>
        public static DiagnosticDescriptor WithSeverity(this DiagnosticDescriptor descriptor, Severity severity)
        {
            return new DiagnosticDescriptor(
                descriptor.Id,
                (string)descriptor.Title,
                (string)descriptor.MessageFormat,
                descriptor.Category,
                severity.ToDiagnosticSeverity(),
                descriptor.IsEnabledByDefault,
                (string)descriptor.Description,
                descriptor.HelpLinkUri,
                descriptor.CustomTags.ToArray());
        }

        private static string[] BuildCustomTags(string diagnosticId, IdeVisibility ideVisibility, ResourceManager resourceManager)
        {
            var tags = new List<string>();

            if (bool.Parse(resourceManager.GetString($"{diagnosticId}_IsActivatedByDefault")))
            {
                tags.Add(DiagnosticTagsHelper.SonarWayTag);
            }

            if (ideVisibility == IdeVisibility.Hidden)
            {
                tags.Add(WellKnownDiagnosticTags.Unnecessary);
            }

            return tags.ToArray();
        }

        private static Severity ParseSeverity(string severity)
        {
            if (Enum.TryParse(severity, out Severity result))
            {
                return result;
            }

            throw new NotSupportedException($"Not supported severity");
        }

        private static DiagnosticSeverity ToDiagnosticSeverity(this Severity severity,
            IdeVisibility ideVisibility = IdeVisibility.Visible)
        {
            switch (severity)
            {
                case Severity.Info:
                    return ideVisibility == IdeVisibility.Hidden ? DiagnosticSeverity.Hidden : DiagnosticSeverity.Info;

                case Severity.Minor:
                    return ideVisibility == IdeVisibility.Hidden ? DiagnosticSeverity.Hidden : DiagnosticSeverity.Warning;

                case Severity.Major:
                case Severity.Critical:
                case Severity.Blocker:
                    return DiagnosticSeverity.Warning;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}

/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.RuleDescriptors
{
    public class RuleDetail
    {
        private static readonly Assembly SonarAnalyzerUtilitiesAssembly = typeof(RuleDetail).Assembly;
        private static readonly HashSet<string> BackwardsCompatibleTypes = new()
        {
            "BUG",
            "CODE_SMELL",
            "VULNERABILITY",
        };

        public string Key { get; }
        public string Type { get; }
        public string Title { get; }
        public string Severity { get; }
        public string Status { get; }
        public string Description { get; }
        public bool IsActivatedByDefault { get; }
        public string Remediation { get; }
        public string RemediationCost { get; }
        public List<string> Tags { get; }
        public List<RuleParameter> Parameters { get; } = new();
        public List<string> CodeFixTitles { get; } = new();

        public RuleDetail(AnalyzerLanguage language, ResourceManager resources, string id)
        {
            Key = id;
            Type = BackwardsCompatibleType(resources.GetString($"{id}_Type"));
            Title = resources.GetString($"{id}_Title");
            Severity = resources.GetString($"{id}_Severity");
            Status = resources.GetString($"{id}_Status");
            IsActivatedByDefault = bool.Parse(resources.GetString($"{id}_IsActivatedByDefault"));
            Description = HtmlDescription(language, id);
            Remediation = SonarQubeRemediationFunction(resources.GetString($"{id}_Remediation"));
            RemediationCost = resources.GetString($"{id}_RemediationCost");
            Tags = resources.GetString($"{id}_Tags").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        // SonarQube before 7.3 supports only 3 types of issues: BUG, CODE_SMELL and VULNERABILITY.
        // This method returns backwards compatible issue type. The type should be adjusted in
        // AbstractRulesDefinition.
        private static string BackwardsCompatibleType(string type) =>
            BackwardsCompatibleTypes.Contains(type) ? type : "VULNERABILITY";

        private static string SonarQubeRemediationFunction(string remediation) =>
            remediation == "Constant/Issue" ? "CONSTANT_ISSUE" : null;

        private static string HtmlDescription(AnalyzerLanguage language, string id)
        {
            if (SonarAnalyzerUtilitiesAssembly.GetManifestResourceNames().FirstOrDefault(MatchesRule) is { } resourceName)
            {
                using var stream = SonarAnalyzerUtilitiesAssembly.GetManifestResourceStream(resourceName);
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            else
            {
                throw new InvalidDataException($"Could not locate resource for rule {id}");
            }

            bool MatchesRule(string resource) =>
                resource.EndsWith($"SonarAnalyzer.Rules.Description.{id}.html", StringComparison.OrdinalIgnoreCase)
                || resource.EndsWith($"SonarAnalyzer.Rules.Description.{id}{language.ResourceSuffix}.html", StringComparison.OrdinalIgnoreCase);
        }
    }
}

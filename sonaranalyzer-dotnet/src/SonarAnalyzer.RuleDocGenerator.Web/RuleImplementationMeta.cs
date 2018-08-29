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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SonarAnalyzer.Common;
using SonarAnalyzer.RuleDescriptors;

namespace SonarAnalyzer.RuleDocGenerator
{
    [ExcludeFromCodeCoverage]
    public class RuleImplementationMeta
    {
        internal const string CrosslinkPattern = "([rR]ule\\W+)?\\{rule:(?:csharpsquid|vbnet):(?<ruleId>S[0-9]+)\\}";
        internal const string HelpLinkPattern = "#version={0}&ruleId={1}";

        [JsonProperty("key")]
        public string Id { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("severity")]
        public string Severity { get; set; }

        [JsonProperty("tags")]
        public IEnumerable<string> Tags { get; set; }

        public static RuleImplementationMeta Convert(RuleDetail detail, string productVersion, AnalyzerLanguage language)
        {
            return new RuleImplementationMeta
            {
                Id = detail.Key,
                Language = language.FriendlyName,
                Severity = detail.Severity,
                Title = detail.Title,
                Description = GetParameterDescription(detail.Parameters) +
                    AddLinksBetweenRulesToDescription(detail.Description, productVersion) +
                    GetCodeFixDescription(detail),
                Tags = detail.Tags
            };
        }

        private static string AddLinksBetweenRulesToDescription(string description, string productVersion)
        {
            var urlRegexPattern = string.Format(HelpLinkPattern, productVersion, @"${ruleId}");
            var linkPattern = $"<a class=\"rule-link\" href=\"{urlRegexPattern}\">{"Rule ${ruleId}"}</a>";
            return Regex.Replace(description, CrosslinkPattern, linkPattern);
        }

        private static string GetParameterDescription(IList<RuleParameter> parameters)
        {
            if (!parameters.Any())
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.Append("<h2 class=\"param-header\">Parameters</h2>");
            sb.Append("<dl>");
            foreach (var parameter in parameters)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "<dt class=\"param-key\">{0}</dt>", parameter.Key);
                sb.AppendFormat(CultureInfo.InvariantCulture,
                    "<dd>" +
                    "<span class=\"param-description\">{0}</span>" +
                    "<span class=\"param-type\">{1}</span>" +
                    "<span class=\"param-default\">{2}</span>" +
                    "</dd>",
                    parameter.Description, parameter.Type, parameter.DefaultValue);
            }
            sb.Append("</dl>");
            return sb.ToString();
        }

        private static string GetCodeFixDescription(RuleDetail detail)
        {
            if (!detail.CodeFixTitles.Any())
            {
                return string.Empty;
            }

            const string listItemPattern = "<li>{0}</li>";
            const string codeFixPattern = "<h2>Code Fixes</h2><ul>{0}</ul>";

            return
                string.Format(codeFixPattern,
                    string.Join("", detail.CodeFixTitles.Select(title => string.Format(listItemPattern, title))));
        }
    }
}

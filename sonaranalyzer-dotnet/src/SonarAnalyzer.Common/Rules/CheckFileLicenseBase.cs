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

using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace SonarAnalyzer.Rules
{
    public abstract class CheckFileLicenseBase : ParameterLoadingDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1451";
        protected const string MessageFormat = "Add or update the header of this file.";

        internal const string HeaderFormatRuleParameterKey = "headerFormat";
        internal const string HeaderFormatPropertyKey = nameof(HeaderFormat);
        public abstract string HeaderFormat { get; set; }

        internal const string IsRegularExpressionRuleParameterKey = "isRegularExpression";
        internal const string IsRegularExpressionPropertyKey = nameof(IsRegularExpression);
        internal const string IsRegularExpressionDefaultValue = "false";
        [RuleParameter(IsRegularExpressionRuleParameterKey, PropertyType.Boolean,
            "Whether the headerFormat is a regular expression.", IsRegularExpressionDefaultValue)]
        public bool IsRegularExpression { get; set; } = bool.Parse(IsRegularExpressionDefaultValue);

        protected static bool IsRegexPatternValid(string pattern)
        {
            try
            {
                Regex.Match(string.Empty, pattern);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        protected bool HasValidLicenseHeader(SyntaxNode node)
        {
            if (node == null || !node.HasLeadingTrivia)
            {
                return false;
            }

            var trivias = node.GetLeadingTrivia();
            var header = trivias.ToString();
            return header != null && AreHeadersEqual(header);
        }

        protected bool AreHeadersEqual(string currentHeader)
        {
            var unixEndingHeader = currentHeader.Replace("\r\n", "\n");
            var unixEndingHeaderFormat = HeaderFormat.Replace("\r\n", "\n").Replace("\\r\\n", "\n");
            if (!IsRegularExpression && !unixEndingHeaderFormat.EndsWith("\n"))
            {
                // In standard text mode, we want to be sure that the matched header is on its own
                // line, with nothing else on the same line.
                unixEndingHeaderFormat += "\n";
            }
            return IsRegularExpression
                ? Regex.IsMatch(unixEndingHeader, unixEndingHeaderFormat)
                : unixEndingHeader.StartsWith(unixEndingHeaderFormat, StringComparison.Ordinal);
        }

        protected ImmutableDictionary<string, string> CreateDiagnosticProperties()
        {
            return ImmutableDictionary<string, string>.Empty
                .Add(HeaderFormatPropertyKey, HeaderFormat)
                .Add(IsRegularExpressionPropertyKey, IsRegularExpression.ToString());
        }
    }
}

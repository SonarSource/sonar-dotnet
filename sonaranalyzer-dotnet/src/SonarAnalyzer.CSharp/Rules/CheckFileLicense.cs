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
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class CheckFileLicense : ParameterLoadingDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1451";
        private const string MessageFormat = "Add or update the header of this file.";

        internal const string HeaderFormatRuleParameterKey = "headerFormat";
        internal const string HeaderFormatPropertyKey = nameof(HeaderFormat);
        internal const string HeaderFormatDefaultValue = @"/*
 * SonarQube, open source software quality management tool.
 * Copyright (C) 2008-2013 SonarSource
 * mailto:contact AT sonarsource DOT com
 *
 * SonarQube is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * SonarQube is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
";

        internal const string IsRegularExpressionRuleParameterKey = "isRegularExpression";
        internal const string IsRegularExpressionPropertyKey = nameof(IsRegularExpression);
        internal const string IsRegularExpressionDefaultValue = "false";

        [RuleParameter(HeaderFormatRuleParameterKey, PropertyType.String, "Expected copyright and license header.",
            HeaderFormatDefaultValue)]
        public string HeaderFormat { get; set; } = HeaderFormatDefaultValue;

        [RuleParameter(IsRegularExpressionRuleParameterKey, PropertyType.Boolean,
            "Whether the headerFormat is a regular expression.", IsRegularExpressionDefaultValue)]
        public bool IsRegularExpression { get; set; } = bool.Parse(IsRegularExpressionDefaultValue);

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager,
                isEnabledByDefault: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxTreeActionInNonGenerated(stac =>
            {
                if (HeaderFormat == null)
                {
                    return;
                }

                if (IsRegularExpression && !IsRegexPatternValid(HeaderFormat))
                {
                    throw new InvalidOperationException($"Invalid regular expression: {HeaderFormat}");
                }

                var firstNode = stac.Tree.GetRoot().ChildTokens().FirstOrDefault().Parent;
                if (!HasValidLicenseHeader(firstNode))
                {
                    var properties = CreateDiagnosticProperties();
                    stac.ReportDiagnosticWhenActive(Diagnostic.Create(rule, Location.Create(stac.Tree,
                        TextSpan.FromBounds(0, 0)), properties));
                }
            });
        }

        private static bool IsRegexPatternValid(string pattern)
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

        private bool HasValidLicenseHeader(SyntaxNode node)
        {
            if (node == null || !node.HasLeadingTrivia)
            {
                return false;
            }

            var trivias = node.GetLeadingTrivia();
            if (trivias.Last().IsKind(SyntaxKind.EndOfLineTrivia))
            {
                trivias = trivias.RemoveAt(trivias.Count - 1);
            }

            var header = trivias.ToString();
            return header != null && AreHeadersEqual(header);
        }

        private bool AreHeadersEqual(string currentHeader)
        {
            var unixEndingHeader = currentHeader.Replace("\r\n", "\n");
            var unixEndingHeaderFormat = HeaderFormat.Replace("\r\n", "\n").Replace("\\r\\n", "\n");

            return IsRegularExpression
                ? Regex.IsMatch(unixEndingHeader, unixEndingHeaderFormat)
                : unixEndingHeader.Equals(unixEndingHeaderFormat, StringComparison.Ordinal);
        }

        private ImmutableDictionary<string, string> CreateDiagnosticProperties()
        {
            return ImmutableDictionary<string, string>.Empty
                .Add(HeaderFormatPropertyKey, HeaderFormat)
                .Add(IsRegularExpressionPropertyKey, IsRegularExpression.ToString());
        }
    }
}

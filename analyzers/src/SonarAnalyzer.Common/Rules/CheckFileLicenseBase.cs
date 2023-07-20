/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Rules
{
    public abstract class CheckFileLicenseBase : ParametrizedDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1451";
        internal const string HeaderFormatPropertyKey = nameof(HeaderFormat);
        internal const string IsRegularExpressionPropertyKey = nameof(IsRegularExpression);
        protected const string HeaderFormatRuleParameterKey = "headerFormat";
        private const string IsRegularExpressionRuleParameterKey = "isRegularExpression";
        private const string IsRegularExpressionDefaultValue = "false";
        private const string MessageFormat = "Add or update the header of this file.";

        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade Language { get; }
        public abstract string HeaderFormat { get; set; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        [RuleParameter(IsRegularExpressionRuleParameterKey, PropertyType.Boolean, "Whether the headerFormat is a regular expression.", IsRegularExpressionDefaultValue)]
        public bool IsRegularExpression { get; set; } = bool.Parse(IsRegularExpressionDefaultValue);

        protected CheckFileLicenseBase() =>
            rule = Language.CreateDescriptor(DiagnosticId, MessageFormat, isEnabledByDefault: false);

        protected override void Initialize(SonarParametrizedAnalysisContext context) =>
            context.RegisterTreeAction(Language.GeneratedCodeRecognizer, c =>
                {
                    if (HeaderFormat == null)
                    {
                        return;
                    }

                    if (IsRegularExpression && !IsRegexPatternValid(HeaderFormat))
                    {
                        throw new InvalidOperationException($"Invalid regular expression: {HeaderFormat}");
                    }

                    var firstNode = c.Tree.GetRoot().ChildTokens().FirstOrDefault().Parent;
                    if (!HasValidLicenseHeader(firstNode))
                    {
                        var properties = CreateDiagnosticProperties();
                        c.ReportIssue(CreateDiagnostic(rule, Location.Create(c.Tree, TextSpan.FromBounds(0, 0)), properties));
                    }
                });

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
            var header = trivias.ToString();
            return header != null && AreHeadersEqual(header);
        }

        private bool AreHeadersEqual(string currentHeader)
        {
            var unixEndingHeader = currentHeader.Replace("\r\n", "\n");
            var unixEndingHeaderFormat = HeaderFormat.Replace("\r\n", "\n").Replace("\\r\\n", "\n");
            if (!IsRegularExpression && !unixEndingHeaderFormat.EndsWith("\n"))
            {
                // In standard text mode, we want to be sure that the matched header is on its own line, with nothing else on the same line.
                unixEndingHeaderFormat += "\n";
            }
            return IsRegularExpression
                ? Regex.IsMatch(unixEndingHeader, unixEndingHeaderFormat, RegexOptions.Singleline)
                : unixEndingHeader.StartsWith(unixEndingHeaderFormat, StringComparison.Ordinal);
        }

        private ImmutableDictionary<string, string> CreateDiagnosticProperties() =>
            ImmutableDictionary<string, string>.Empty
                .Add(HeaderFormatPropertyKey, HeaderFormat)
                .Add(IsRegularExpressionPropertyKey, IsRegularExpression.ToString());
    }
}

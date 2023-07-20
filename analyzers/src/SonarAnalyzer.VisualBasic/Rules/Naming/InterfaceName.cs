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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class InterfaceName : ParametrizedDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S114";
        private const string MessageFormat = "Rename this interface to match the regular expression: '{0}'.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat,
                isEnabledByDefault: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private const string DefaultPattern = "^I" + NamingHelper.PascalCasingInternalPattern + "$";

        [RuleParameter("format", PropertyType.String,
            "Regular expression used to check the interface names against.", DefaultPattern)]
        public string Pattern { get; set; } = DefaultPattern;

        protected override void Initialize(SonarParametrizedAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var declaration = (InterfaceStatementSyntax)c.Node;
                    if (!NamingHelper.IsRegexMatch(declaration.Identifier.ValueText, Pattern))
                    {
                        c.ReportIssue(CreateDiagnostic(rule, declaration.Identifier.GetLocation(), Pattern));
                    }
                },
                SyntaxKind.InterfaceStatement);
        }
    }
}

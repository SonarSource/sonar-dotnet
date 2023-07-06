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
    // This base class is only there to avoid duplication between the implementation of S119 and S2373
    public abstract class TypeParameterNameBase : ParametrizedDiagnosticAnalyzer
    {
        protected const string MessageFormat = "Rename '{0}' to match the regular expression: '{1}'.";

        private const string DefaultFormat = "^T(" + NamingHelper.PascalCasingInternalPattern + ")?";
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
        protected abstract DiagnosticDescriptor Rule { get; }

        [RuleParameter("format", PropertyType.String, "Regular expression used to check the generic type parameter names against.", DefaultFormat)]
        public string Pattern { get; set; } = DefaultFormat;

        protected override void Initialize(SonarParametrizedAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var typeParameter = (TypeParameterSyntax)c.Node;
                    if (!NamingHelper.IsRegexMatch(typeParameter.Identifier.ValueText, Pattern))
                    {
                        c.ReportIssue(Diagnostic.Create(Rule, typeParameter.Identifier.GetLocation(),
                            typeParameter.Identifier.ValueText, Pattern));
                    }
                },
                SyntaxKind.TypeParameter);
    }
}

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

namespace SonarAnalyzer.Rules
{
    public abstract class EnumNameShouldFollowRegexBase<TSyntaxKind> : ParametrizedDiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S2342";
        private const string MessageFormat = "Rename this enumeration to match the regular expression: '{0}'.";
        private const string DefaultEnumNamePattern = NamingHelper.PascalCasingPattern;
        private const string DefaultFlagsEnumNamePattern = "^" + NamingHelper.PascalCasingInternalPattern + "s$";

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        [RuleParameter("format", PropertyType.String, "Regular expression used to check the enumeration type names against.", DefaultEnumNamePattern)]
        public string EnumNamePattern { get; set; } = DefaultEnumNamePattern;

        [RuleParameter("flagsAttributeFormat", PropertyType.String, "Regular expression used to check the flags enumeration type names against.", DefaultFlagsEnumNamePattern)]
        public string FlagsEnumNamePattern { get; set; } = DefaultFlagsEnumNamePattern;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);
        private readonly DiagnosticDescriptor rule;

        protected EnumNameShouldFollowRegexBase() =>
            rule = Language.CreateDescriptor(DiagnosticId, MessageFormat, isEnabledByDefault: false);

        protected sealed override void Initialize(SonarParametrizedAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
                {
                    var pattern = c.Node.HasFlagsAttribute(c.SemanticModel) ? FlagsEnumNamePattern : EnumNamePattern;
                    if (Language.Syntax.NodeIdentifier(c.Node) is { } identifier && !NamingHelper.IsRegexMatch(identifier.ValueText, pattern))
                    {
                        c.ReportIssue(CreateDiagnostic(SupportedDiagnostics[0], identifier.GetLocation(), pattern));
                    }
                },
                Language.SyntaxKind.EnumDeclaration);
    }
}

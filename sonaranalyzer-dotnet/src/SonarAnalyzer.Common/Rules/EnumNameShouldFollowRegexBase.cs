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

namespace SonarAnalyzer.Rules
{
    public abstract class EnumNameShouldFollowRegexBase : ParameterLoadingDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S2342";
        protected const string MessageFormat = "Rename this enumeration to match the regular expression: '{0}'.";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }

        private const string DefaultEnumNamePattern = NamingHelper.PascalCasingPattern;
        private const string DefaultFlagsEnumNamePattern = "^" + NamingHelper.PascalCasingInternalPattern + "s$";

        [RuleParameter("format", PropertyType.String,
            "Regular expression used to check the enumeration type names against.", DefaultEnumNamePattern)]
        public string EnumNamePattern { get; set; } = DefaultEnumNamePattern;

        [RuleParameter("flagsAttributeFormat", PropertyType.String,
            "Regular expression used to check the flags enumeration type names against.", DefaultFlagsEnumNamePattern)]
        public string FlagsEnumNamePattern { get; set; } = DefaultFlagsEnumNamePattern;
    }

    public abstract class EnumNameShouldFollowRegexBase<TLanguageKindEnum, TEnumDeclarationSyntax> : EnumNameShouldFollowRegexBase
        where TLanguageKindEnum : struct
        where TEnumDeclarationSyntax : SyntaxNode
    {
        protected sealed override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                GeneratedCodeRecognizer,
                c =>
                {
                    var enumDeclaration = (TEnumDeclarationSyntax)c.Node;
                    var enumIdentifier = GetIdentifier(enumDeclaration);
                    var enumPattern = enumDeclaration.HasFlagsAttribute(c.SemanticModel)
                        ? FlagsEnumNamePattern
                        : EnumNamePattern;

                    if (!NamingHelper.IsRegexMatch(enumIdentifier.ValueText, enumPattern))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], enumIdentifier.GetLocation(),
                            enumPattern));
                    }
                },
                EnumStatementSyntaxKind);
        }

        protected abstract TLanguageKindEnum EnumStatementSyntaxKind { get; }

        protected abstract SyntaxToken GetIdentifier(TEnumDeclarationSyntax declaration);
    }
}

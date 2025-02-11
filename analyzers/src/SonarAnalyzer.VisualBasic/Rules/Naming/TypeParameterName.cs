/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.VisualBasic.Rules
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class TypeParameterName : ParametrizedDiagnosticAnalyzer
    {
        private const string S119DiagnosticId = "S119";

        [Obsolete("This rule is superseded by S119.")]
        private const string S2373DiagnosticId = "S2373";

        private const string MessageFormat = "Rename '{0}' to match the regular expression: '{1}'.";
        private const string DefaultFormat = "^T(" + NamingHelper.PascalCasingInternalPattern + ")?";

        internal static readonly DiagnosticDescriptor S119 = DescriptorFactory.Create(S119DiagnosticId, MessageFormat, isEnabledByDefault: false);
        internal static readonly DiagnosticDescriptor S2373 = DescriptorFactory.Create(S2373DiagnosticId, MessageFormat, isEnabledByDefault: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(S119, S2373);

        [RuleParameter("format", PropertyType.String, "Regular expression used to check the generic type parameter names against.", DefaultFormat)]
        public string Pattern { get; set; } = DefaultFormat;

        protected override void Initialize(SonarParametrizedAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var typeParameter = (TypeParameterSyntax)c.Node;
                    if (!NamingHelper.IsRegexMatch(typeParameter.Identifier.ValueText, Pattern))
                    {
                        var location = typeParameter.Identifier.GetLocation();
                        c.ReportIssue(S119, location, typeParameter.Identifier.ValueText, Pattern);
                        c.ReportIssue(S2373, location, typeParameter.Identifier.ValueText, Pattern);
                    }
                },
                SyntaxKind.TypeParameter);
    }
}

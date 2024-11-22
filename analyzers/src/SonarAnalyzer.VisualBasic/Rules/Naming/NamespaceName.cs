/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class NamespaceName : ParametrizedDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2304";
        private const string MessageFormat = "Rename this namespace to match the regular expression: '{0}'.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat,
                isEnabledByDefault: false);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private const string DefaultPattern =
            "^" + NamingHelper.PascalCasingInternalPattern + @"(\." + NamingHelper.PascalCasingInternalPattern + ")*$";

        [RuleParameter("format", PropertyType.String,
            "Regular expression used to check the namespace names against.", DefaultPattern)]
        public string Pattern { get; set; } = DefaultPattern;

        protected override void Initialize(SonarParametrizedAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var declaration = (NamespaceStatementSyntax)c.Node;
                    var declarationName = declaration.Name?.ToString();
                    if (declarationName != null &&
                        !NamingHelper.IsRegexMatch(declarationName, Pattern))
                    {
                        c.ReportIssue(rule, declaration.Name, Pattern);
                    }
                },
                SyntaxKind.NamespaceStatement);
        }
    }
}

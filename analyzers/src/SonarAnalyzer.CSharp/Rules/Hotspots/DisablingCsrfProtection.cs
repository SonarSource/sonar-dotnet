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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DisablingCsrfProtection : HotspotDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4502";
        private const string MessageFormat = "Make sure disabling CSRF protection is safe here.";
        private const SyntaxKind ImplicitObjectCreationExpression = (SyntaxKind)8659;

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public DisablingCsrfProtection() : base(AnalyzerConfiguration.Hotspot) { }

        public DisablingCsrfProtection(IAnalyzerConfiguration configuration) : base(configuration) { }

        protected override void Initialize(SonarAnalysisContext context) =>
             context.RegisterCompilationStartAction(
                c =>
                {
                    if (!IsEnabled(c.Options))
                    {
                        return;
                    }

                    c.RegisterNodeAction(
                        CheckIgnoreAntiforgeryTokenAttribute,
                        SyntaxKind.Attribute,
                        SyntaxKind.ObjectCreationExpression,
                        ImplicitObjectCreationExpression);
                });

        private static void CheckIgnoreAntiforgeryTokenAttribute(SonarSyntaxNodeReportingContext c)
        {
            var shouldReport = c.Node switch
            {
                AttributeSyntax attributeSyntax => attributeSyntax.IsKnownType(KnownType.Microsoft_AspNetCore_Mvc_IgnoreAntiforgeryTokenAttribute, c.SemanticModel),
                ObjectCreationExpressionSyntax objectCreation => objectCreation.IsKnownType(KnownType.Microsoft_AspNetCore_Mvc_IgnoreAntiforgeryTokenAttribute, c.SemanticModel),
                _ => c.Node.IsKnownType(KnownType.Microsoft_AspNetCore_Mvc_IgnoreAntiforgeryTokenAttribute, c.SemanticModel)
            };

            if (shouldReport)
            {
                ReportDiagnostic(c);
            }
        }

        private static void ReportDiagnostic(SonarSyntaxNodeReportingContext context) =>
            context.ReportIssue(Rule, context.Node);
    }
}

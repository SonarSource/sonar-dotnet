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
                ccc =>
                {
                    if (!IsEnabled(ccc.Options))
                    {
                        return;
                    }

                    context.RegisterNodeAction(
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
            context.ReportIssue(CreateDiagnostic(Rule, context.Node.GetLocation()));
    }
}

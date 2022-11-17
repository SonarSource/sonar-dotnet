/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

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

                    context.RegisterSyntaxNodeActionInNonGenerated(CheckIgnoreAntiforgeryTokenAttribute,
                                                                   SyntaxKind.Attribute,
                                                                   SyntaxKind.ObjectCreationExpression,
                                                                   ImplicitObjectCreationExpression);
                });

        private static void CheckIgnoreAntiforgeryTokenAttribute(SyntaxNodeAnalysisContext c)
        {
            if (c.SemanticModel.GetTypeInfo(c.Node).Type.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_IgnoreAntiforgeryTokenAttribute))
            {
                ReportDiagnostic(c);
            }
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context) =>
            context.ReportIssue(Diagnostic.Create(Rule, context.Node.GetLocation()));
    }
}

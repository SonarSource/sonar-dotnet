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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SupplyParameterFromQueryNeedsRoutableComponent : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6803";
    private const string MessageFormat = "Component parameters can only receive query parameter values in routable components.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(c =>
        {
            if (c.Compilation.GetTypeByMetadataName(KnownType.Microsoft_AspNetCore_Components_RouteAttribute) is not null)
            {
                context.RegisterSymbolAction(CheckMethod, SymbolKind.Property);
            }
        });

    private static void CheckMethod(SonarSymbolReportingContext context)
    {
        var property = (IPropertySymbol)context.Symbol;
        if (property.HasAttribute(KnownType.Microsoft_AspNetCore_Components_SupplyParameterFromQueryAttribute)
            && property.HasAttribute(KnownType.Microsoft_AspNetCore_Components_ParameterAttribute))
        {
            if (!property.ContainingType.HasAttribute(KnownType.Microsoft_AspNetCore_Components_RouteAttribute))
            {
                foreach (var location in property.Locations)
                {
                    context.ReportIssue(Diagnostic.Create(Rule, location));
                }
            }
        }
    }
}

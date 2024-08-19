/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using System.Collections.Concurrent;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SpecifyRouteAttribute() : SonarDiagnosticAnalyzer<SyntaxKind>(DiagnosticId)
{
    private const string DiagnosticId = "S6934";

    protected override string MessageFormat => "Specify the RouteAttribute when an HttpMethodAttribute or RouteAttribute is specified at an action level.";
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStart =>
        {
            if (!UsesAttributeRouting(compilationStart.Compilation))
            {
                return;
            }
            compilationStart.RegisterSymbolStartAction(symbolStart =>
            {
                if (symbolStart.Symbol.GetAttributesWithInherited().Any(x => x.AttributeClass.DerivesOrImplements(KnownType.Microsoft_AspNetCore_Mvc_Routing_IRouteTemplateProvider)))
                {
                    return;
                }
                var secondaryLocations = new ConcurrentStack<SecondaryLocation>();
                symbolStart.RegisterSyntaxNodeAction(nodeContext =>
                {
                    var methodDeclaration = (MethodDeclarationSyntax)nodeContext.Node;
                    if (nodeContext.SemanticModel.GetDeclaredSymbol(methodDeclaration, nodeContext.Cancel) is { } method
                        && !method.ContainingType.IsAbstract
                        && method.IsControllerActionMethod()
                        && method.GetAttributesWithInherited().Any(x => !CanBeIgnored(x.GetAttributeRouteTemplate())))
                    {
                        secondaryLocations.Push(methodDeclaration.Identifier.ToSecondaryLocation());
                    }
                }, SyntaxKind.MethodDeclaration);
                symbolStart.RegisterSymbolEndAction(symbolEnd => ReportIssues(symbolEnd, symbolStart.Symbol, secondaryLocations));
            }, SymbolKind.NamedType);
        });

    private void ReportIssues(SonarSymbolReportingContext context, ISymbol symbol, ConcurrentStack<SecondaryLocation> secondaryLocations)
    {
        if (secondaryLocations.IsEmpty)
        {
            return;
        }

        foreach (var declaration in symbol.DeclaringSyntaxReferences.Select(x => x.GetSyntax()))
        {
            if (declaration.GetIdentifier() is { } identifier)
            {
                context.ReportIssue(CSharpGeneratedCodeRecognizer.Instance, Rule, identifier.GetLocation(), secondaryLocations);
            }
        }
    }

    private static bool UsesAttributeRouting(Compilation compilation) =>
        compilation.GetTypeByMetadataName(KnownType.Microsoft_AspNetCore_Mvc_Routing_HttpMethodAttribute) is not null;

    private static bool CanBeIgnored(string template) =>
        string.IsNullOrEmpty(template)
        // See: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing#combining-attribute-routes
        // Route templates applied to an action that begin with / or ~/ don't get combined with route templates applied to the controller.
        || template.StartsWith("/")
        || template.StartsWith("~/");
}

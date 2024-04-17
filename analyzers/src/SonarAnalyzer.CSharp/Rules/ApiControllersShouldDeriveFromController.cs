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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ApiControllersShouldDeriveFromController : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6961";
    private const string MessageFormat = "Inherit from ControllerBase instead of Controller.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    private readonly HashSet<string> viewIdentifiers =
        [
           "View",
           "PartialView",
           "ViewComponent",
           "Json",
           "OnActionExecutionAsync",
           "OnActionExecuted",
           "ViewData",
           "ViewBag",
           "TempData",
           "ViewResult"
        ];

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStartContext =>
        {
            if (!compilationStartContext.Compilation.ReferencesControllers())
            {
                return;
            }

            compilationStartContext.RegisterSymbolStartAction(symbolStartContext =>
            {
                var symbol = (INamedTypeSymbol)symbolStartContext.Symbol;
                if (symbol.IsControllerType()
                    && symbol.IsPubliclyAccessible()
                    && symbol.AnyAttributeDerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ApiControllerAttribute)
                    && symbol.BaseType.Is(KnownType.Microsoft_AspNetCore_Mvc_Controller))
                {
                    var shouldReportController = true;
                    symbolStartContext.RegisterSyntaxNodeAction(nodeContext =>
                    {
                        if (viewIdentifiers.Contains(nodeContext.Node.GetName()))
                        {
                            shouldReportController = false;
                        }
                    }, SyntaxKind.IdentifierName);

                    symbolStartContext.RegisterSymbolEndAction(symbolEndContext =>
                    {
                        if (shouldReportController)
                        {
                            ReportIssue(symbolEndContext, symbol, shouldReportController);
                        }
                    });
                }
            }, SymbolKind.NamedType);
        });

    private static void ReportIssue(SonarSymbolReportingContext context, INamedTypeSymbol controllerSymbol, bool shouldReport)
    {
        if (shouldReport)
        {
            var reportLocations = controllerSymbol.DeclaringSyntaxReferences
                .Select(x => x.GetSyntax())
                .OfType<ClassDeclarationSyntax>()
                .Select(x => x.BaseList?.DescendantNodes().FirstOrDefault(x => x.GetName() is "Controller")?.GetLocation())
                .OfType<Location>();

            foreach (var location in reportLocations)
            {
                context.ReportIssue(CSharpFacade.Instance.GeneratedCodeRecognizer, Diagnostic.Create(Rule, location));
            }
        }
    }
}

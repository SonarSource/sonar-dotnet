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
public sealed class AnnotateApiActionsWithHttpVerb : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6965";
    private const string MessageFormat = "REST API controller actions should be annotated with the appropriate HTTP verb attribute.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    private static readonly ImmutableArray<KnownType> HttpMethodAttributes = ImmutableArray.Create(
        KnownType.Microsoft_AspNetCore_Mvc_HttpGetAttribute,
        KnownType.Microsoft_AspNetCore_Mvc_HttpPutAttribute,
        KnownType.Microsoft_AspNetCore_Mvc_HttpPostAttribute,
        KnownType.Microsoft_AspNetCore_Mvc_HttpDeleteAttribute,
        KnownType.Microsoft_AspNetCore_Mvc_HttpPatchAttribute,
        KnownType.Microsoft_AspNetCore_Mvc_HttpHeadAttribute,
        KnownType.Microsoft_AspNetCore_Mvc_HttpOptionsAttribute,
        KnownType.Microsoft_AspNetCore_Mvc_Routing_HttpMethodAttribute,
        KnownType.Microsoft_AspNetCore_Mvc_AcceptVerbsAttribute); // AcceptVerbs is treated as an exception

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStartContext =>
        {
            if (!compilationStartContext.Compilation.ReferencesControllers())
            {
                return;
            }

            compilationStartContext.RegisterSymbolStartAction(symbolStartContext =>
            {
                var controllerSymbol = (INamedTypeSymbol)symbolStartContext.Symbol;
                var controllerAttributes = controllerSymbol.GetAttributesWithInherited();

                if (controllerSymbol.IsControllerType()
                    && controllerAttributes.Any(x => x.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ApiControllerAttribute))
                    && !IgnoresApiExplorer(controllerAttributes))
                {
                    symbolStartContext.RegisterSyntaxNodeAction(c =>
                    {
                        var methodNode = (MethodDeclarationSyntax)c.Node;
                        var methodSymbol = c.SemanticModel.GetDeclaredSymbol(methodNode);
                        var methodAttributes = methodSymbol.GetAttributesWithInherited();

                        if (methodSymbol.IsControllerMethod()
                            && !methodSymbol.IsAbstract
                            && !methodAttributes.Any(x => x.AttributeClass.DerivesFromAny(HttpMethodAttributes))
                            && !IgnoresApiExplorer(methodAttributes))
                        {
                            c.ReportIssue(Rule, methodNode.Identifier.GetLocation());
                        }
                    },
                    SyntaxKind.MethodDeclaration);
                }
            },
            SymbolKind.NamedType);
        });

    // Tracks [ApiExplorerSettings(IgnoreApi = true)]
    private static bool IgnoresApiExplorer(IEnumerable<AttributeData> attributes) =>
        attributes.FirstOrDefault(x => x.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ApiExplorerSettingsAttribute)) is { } apiExplorerSettings
        && apiExplorerSettings.NamedArguments.FirstOrDefault(x => x.Key == "IgnoreApi").Value.Value is { } ignoreApi
        && ignoreApi.Equals(true);
}

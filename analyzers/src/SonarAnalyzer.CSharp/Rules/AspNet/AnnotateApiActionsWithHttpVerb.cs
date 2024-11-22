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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AnnotateApiActionsWithHttpVerb : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6965";
    private const string MessageFormat = "REST API controller actions should be annotated with the appropriate HTTP verb attribute.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
    private static readonly ImmutableArray<KnownType> HttpMethodAttributes = ImmutableArray.Create(
        KnownType.Microsoft_AspNetCore_Mvc_Routing_HttpMethodAttribute,
        KnownType.Microsoft_AspNetCore_Mvc_AcceptVerbsAttribute); // AcceptVerbs is treated as an exception

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStartContext =>
        {
            if (!compilationStartContext.Compilation.ReferencesNetCoreControllers())
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

                        if (methodSymbol.IsControllerActionMethod()
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
        && apiExplorerSettings.TryGetAttributeValue<bool>("IgnoreApi", out var ignoreApi)
        && ignoreApi;
}

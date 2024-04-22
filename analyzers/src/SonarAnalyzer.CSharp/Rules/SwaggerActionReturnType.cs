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
public sealed class SwaggerActionReturnType : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6968";
    private const string MessageFormat = "{0}";
    private const string NoAttributeMessageFormat = "Annotate this method with ProducesResponseType containing the return type for successful responses.";
    private const string NoTypeMessageFormat = "Use the ProducesResponseType overload containing the return type for successful responses.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
    private static readonly ImmutableArray<KnownType> ControllerActionAttributesNeedingTypeCheck = ImmutableArray.Create(
        KnownType.Microsoft_AspNetCore_Mvc_ProducesResponseTypeAttribute,
        KnownType.SwashbuckleAspNetCoreAnnotationsSwaggerResponseAttribute);
    private static readonly ImmutableArray<KnownType> ControllerActionReturnTypes = ImmutableArray.Create(
        KnownType.Microsoft_AspNetCore_Mvc_IActionResult,
        KnownType.Microsoft_AspNetCore_Http_IResult);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStart =>
        {
            if (compilationStart.Compilation.ReferencesAll(KnownAssembly.MicrosoftAspNetCoreMvcCore, KnownAssembly.SwashbuckleAspNetCoreSwagger))
            {
                compilationStart.RegisterSymbolStartAction(symbolStart =>
                {
                    if (IsControllerCandidate(symbolStart.Symbol))
                    {
                        symbolStart.RegisterSyntaxNodeAction(nodeContext =>
                        {
                            var methodDeclaration = (MethodDeclarationSyntax)nodeContext.Node;
                            if (InvalidMethod(methodDeclaration, nodeContext) is { } method)
                            {
                                nodeContext.ReportIssue(Rule, methodDeclaration.Identifier.GetLocation(), GetMessage(method));
                            }
                        }, SyntaxKind.MethodDeclaration);
                    }
                }, SymbolKind.NamedType);
            }
        });

    private static IMethodSymbol InvalidMethod(BaseMethodDeclarationSyntax methodDeclaration, SonarSyntaxNodeReportingContext nodeContext) =>
        nodeContext.SemanticModel.GetDeclaredSymbol(methodDeclaration, nodeContext.Cancel) is not { } method
        || !method.IsControllerMethod()
        || !method.ReturnType.DerivesOrImplementsAny(ControllerActionReturnTypes)
        || method.GetAttributesWithInherited().Any(x => x.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ApiConventionMethodAttribute)
                                                        || HasApiExplorerSettingsWithIgnoreApiTrue(x)
                                                        || HasProducesResponseTypeAttributeWithReturnType(x)
                                                        || HasSwaggerResponseAttributeWithReturnType(x))
            ? null
            : method;

    private static bool IsControllerCandidate(ISymbol symbol)
    {
        var hasApiControllerAttribute = false;
        foreach (var attribute in symbol.GetAttributesWithInherited())
        {
            hasApiControllerAttribute = hasApiControllerAttribute || attribute.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ApiControllerAttribute);

            if (attribute.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ApiConventionTypeAttribute)
                || HasProducesResponseTypeAttributeWithReturnType(attribute)
                || HasApiExplorerSettingsWithIgnoreApiTrue(attribute))
            {
                return false;
            }
        }
        return hasApiControllerAttribute;
    }

    private static string GetMessage(ISymbol symbol) =>
        symbol.GetAttributesWithInherited().Any(x => x.AttributeClass.DerivesFromAny(ControllerActionAttributesNeedingTypeCheck))
            ? NoTypeMessageFormat
            : NoAttributeMessageFormat;

    private static bool HasProducesResponseTypeAttributeWithReturnType(AttributeData attribute) =>
        attribute.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ProducesResponseTypeAttribute_T)
        || (attribute.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ProducesResponseTypeAttribute)
            && ContainsReturnType(attribute));

    private static bool HasSwaggerResponseAttributeWithReturnType(AttributeData attribute) =>
        attribute.AttributeClass.DerivesFrom(KnownType.SwashbuckleAspNetCoreAnnotationsSwaggerResponseAttribute)
        && ContainsReturnType(attribute);

    private static bool HasApiExplorerSettingsWithIgnoreApiTrue(AttributeData attribute) =>
        attribute.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ApiExplorerSettingsAttribute)
        && attribute.NamedArguments.FirstOrDefault(x => x.Key == "IgnoreApi").Value.Value is true;

    private static bool ContainsReturnType(AttributeData attribute) =>
        !attribute.ConstructorArguments.FirstOrDefault(x => x.Type.Is(KnownType.System_Type)).IsNull
        || attribute.NamedArguments.FirstOrDefault(x => x.Key == "Type").Value.Value is not null;
}

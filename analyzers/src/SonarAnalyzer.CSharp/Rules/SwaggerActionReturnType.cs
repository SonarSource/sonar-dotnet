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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SwaggerActionReturnType : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6968";
    private const string MessageFormat = "{0}";
    private const string NoAttributeMessageFormat = "Annotate this method with ProducesResponseType containing the return type for successful responses.";
    private const string NoTypeMessageFormat = "Use the ProducesResponseType overload containing the return type for successful responses.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
    private static readonly ImmutableArray<KnownType> ControllerActionReturnTypes = ImmutableArray.Create(
        KnownType.Microsoft_AspNetCore_Mvc_IActionResult,
        KnownType.Microsoft_AspNetCore_Http_IResult);
    private static readonly ImmutableArray<KnownType> ProducesAttributes = ImmutableArray.Create(
        KnownType.Microsoft_AspNetCore_Mvc_ProducesAttribute,
        KnownType.Microsoft_AspNetCore_Mvc_ProducesResponseTypeAttribute);
    private static HashSet<string> ActionResultMethods =>
    [
        "Ok",
        "Created",
        "CreatedAtAction",
        "CreatedAtRoute",
        "Accepted",
        "AcceptedAtAction",
        "AcceptedAtRoute"
    ];
    private static HashSet<string> ResultMethods =>
    [
        "Ok",
        "Created",
        "CreatedAtRoute",
        "Accepted",
        "AcceptedAtRoute"
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStart =>
        {
            if (!compilationStart.Compilation.Assembly.HasAttribute(KnownType.Microsoft_AspNetCore_Mvc_ApiConventionTypeAttribute)
                && compilationStart.Compilation.ReferencesAll(KnownAssembly.MicrosoftAspNetCoreMvcCore, KnownAssembly.SwashbuckleAspNetCoreSwagger))
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
                                nodeContext.ReportIssue(Rule, methodDeclaration.Identifier, method.ResponseInvocations.ToSecondaryLocations(), GetMessage(method.Symbol));
                            }
                        }, SyntaxKind.MethodDeclaration);
                    }
                }, SymbolKind.NamedType);
            }
        });

    private static InvalidMethodResult InvalidMethod(BaseMethodDeclarationSyntax methodDeclaration, SonarSyntaxNodeReportingContext nodeContext)
    {
        var responseInvocations = FindSuccessResponses(methodDeclaration, nodeContext.Model);
        return responseInvocations.Length == 0
               || nodeContext.Model.GetDeclaredSymbol(methodDeclaration, nodeContext.Cancel) is not { } method
               || !method.IsControllerActionMethod()
               || !method.ReturnType.DerivesOrImplementsAny(ControllerActionReturnTypes)
               || method.GetAttributesWithInherited().Any(x => x.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ApiConventionMethodAttribute)
                                                               || HasApiExplorerSettingsWithIgnoreApiTrue(x)
                                                               || HasProducesAttributesWithReturnType(x))
                   ? null
                   : new InvalidMethodResult(method, responseInvocations);
    }

    private static SyntaxNode[] FindSuccessResponses(SyntaxNode node, SemanticModel model)
    {
        return ActionResultInvocations().Concat(ObjectCreationInvocations()).Concat(ResultMethodsInvocations()).ToArray();

        IEnumerable<SyntaxNode> ActionResultInvocations() =>
            node.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(x => ActionResultMethods.Contains(x.GetName())
                            && x.ArgumentList.Arguments.Count > 0
                            && model.GetSymbolInfo(x.Expression).Symbol is { } symbol
                            && symbol.IsInType(KnownType.Microsoft_AspNetCore_Mvc_ControllerBase)
                            && symbol.GetParameters().Any(parameter => parameter.HasAttribute(KnownType.Microsoft_AspNetCore_Mvc_Infrastructure_ActionResultObjectValueAttribute)));

        IEnumerable<SyntaxNode> ObjectCreationInvocations() =>
            node.DescendantNodes()
                .OfType<ObjectCreationExpressionSyntax>()
                .Where(x => x.GetName() == "ObjectResult"
                            && x.ArgumentList?.Arguments.Count > 0
                            && model.GetSymbolInfo(x.Type).Symbol.GetSymbolType().Is(KnownType.Microsoft_AspNetCore_Mvc_ObjectResult));

        IEnumerable<SyntaxNode> ResultMethodsInvocations() =>
            node.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(x => ResultMethods.Contains(x.GetName())
                            && x.ArgumentList.Arguments.Count > 0
                            && model.GetSymbolInfo(x).Symbol.IsInType(KnownType.Microsoft_AspNetCore_Http_Results));
    }

    private static bool IsControllerCandidate(ISymbol symbol)
    {
        var hasApiControllerAttribute = false;
        foreach (var attribute in symbol.GetAttributesWithInherited())
        {
            if (attribute.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ApiConventionTypeAttribute)
                || HasProducesAttributesWithReturnType(attribute)
                || HasApiExplorerSettingsWithIgnoreApiTrue(attribute))
            {
                return false;
            }
            hasApiControllerAttribute = hasApiControllerAttribute || attribute.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ApiControllerAttribute);
        }
        return hasApiControllerAttribute;
    }

    private static string GetMessage(ISymbol symbol) =>
        symbol.GetAttributesWithInherited().Any(x => x.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ProducesResponseTypeAttribute))
            ? NoTypeMessageFormat
            : NoAttributeMessageFormat;

    private static bool HasProducesAttributesWithReturnType(AttributeData attribute) =>
        attribute.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ProducesResponseTypeAttribute_T)
        || attribute.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ProducesAttribute_T)
        || (attribute.AttributeClass.DerivesFromAny(ProducesAttributes) && ContainsReturnType(attribute));

    private static bool HasApiExplorerSettingsWithIgnoreApiTrue(AttributeData attribute) =>
        attribute.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ApiExplorerSettingsAttribute)
        && attribute.NamedArguments.FirstOrDefault(x => x.Key == "IgnoreApi").Value.Value is true;

    private static bool ContainsReturnType(AttributeData attribute) =>
        !attribute.ConstructorArguments.FirstOrDefault(x => x.Type.Is(KnownType.System_Type)).IsNull
        || attribute.NamedArguments.FirstOrDefault(x => x.Key == "Type").Value.Value is not null;

    private sealed record InvalidMethodResult(IMethodSymbol Symbol, SyntaxNode[] ResponseInvocations);
}

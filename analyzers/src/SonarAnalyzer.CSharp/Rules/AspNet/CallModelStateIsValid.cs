/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
public sealed class CallModelStateIsValid : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6967";
    private const string MessageFormat = "ModelState.IsValid should be checked in controller actions.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly SyntaxKind[] PropertyAccessSyntaxNodesToVisit = [
        SyntaxKind.ConditionalAccessExpression,
        SyntaxKind.SimpleMemberAccessExpression,
        SyntaxKindEx.Subpattern];

    private static readonly ImmutableArray<KnownType> IgnoredArgumentTypes = ImmutableArray.Create(
        KnownType.System_Object,
        KnownType.System_String,
        KnownType.System_Threading_CancellationToken);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStart =>
        {
            // The rule ignores the project completely if any of these conditions are met:
            // - the project doesn't reference ASP.NET MVC
            // - the project references the FluentValidation library:
            //      - as an alternative to using ModelState.IsValid
            //      - this can made more accurate: check if those validation methods are used in the controller actions rather than just checking whether the library is referenced
            // - the [ApiController] attribute is applied on the assembly level: this results in the attribute being applied to every Controller class in the project
            if (compilationStart.Compilation.ReferencesNetCoreControllers()
                && compilationStart.Compilation.GetTypeByMetadataName(KnownType.FluentValidation_IValidator) is null
                && !compilationStart.Compilation.Assembly.HasAttribute(KnownType.Microsoft_AspNetCore_Mvc_ApiControllerAttribute))
            {
                compilationStart.RegisterSymbolStartAction(symbolStart =>
                {
                    var type = (INamedTypeSymbol)symbolStart.Symbol;
                    if (type.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ControllerBase)
                        && !HasApiControllerAttribute(type)
                        && !HasActionFilterAttribute(type))
                    {
                        symbolStart.RegisterCodeBlockStartAction<SyntaxKind>(ProcessCodeBlock);
                    }
                }, SymbolKind.NamedType);
            }
        });

    private static void ProcessCodeBlock(SonarCodeBlockStartAnalysisContext<SyntaxKind> codeBlockContext)
    {
        if (codeBlockContext.CodeBlock is MethodDeclarationSyntax methodDeclaration
            && codeBlockContext.OwningSymbol is IMethodSymbol methodSymbol
            && !methodSymbol.Parameters.All(IgnoreParameter)
            && methodSymbol.IsControllerActionMethod()
            && !HasActionFilterAttribute(methodSymbol))
        {
            var isModelValidated = false;
            codeBlockContext.RegisterNodeAction(nodeContext =>
            {
                if (!isModelValidated)
                {
                    isModelValidated = IsCheckingValidityProperty(nodeContext.Node, nodeContext.Model);
                }
            }, PropertyAccessSyntaxNodesToVisit);
            codeBlockContext.RegisterNodeAction(nodeContext =>
            {
                if (!isModelValidated)
                {
                    isModelValidated = IsTryValidateInvocation(nodeContext.Node, nodeContext.Model);
                }
            }, SyntaxKind.InvocationExpression);
            codeBlockContext.RegisterCodeBlockEndAction(blockEnd =>
            {
                if (!isModelValidated)
                {
                    blockEnd.ReportIssue(Rule, methodDeclaration.Identifier);
                }
            });
        }
    }

    private static bool IgnoreParameter(IParameterSymbol parameter) =>
        !parameter.GetAttributes().Any(x => x.AttributeClass.DerivesFrom(KnownType.System_ComponentModel_DataAnnotations_ValidationAttribute))
        && (parameter.Type.TypeKind == TypeKind.Dynamic || parameter.Type.IsAny(IgnoredArgumentTypes));

    private static bool HasApiControllerAttribute(ITypeSymbol type) =>
        type.GetAttributesWithInherited().Any(x => x.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ApiControllerAttribute));

    private static bool HasActionFilterAttribute(ISymbol symbol) =>
        symbol.GetAttributesWithInherited().Any(x => x.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_Filters_ActionFilterAttribute));

    private static bool IsCheckingValidityProperty(SyntaxNode node, SemanticModel model) =>
        node.GetIdentifier() is { ValueText: "IsValid" or "ValidationState" } nodeIdentifier
        && model.GetSymbolInfo(nodeIdentifier.Parent).Symbol is IPropertySymbol propertySymbol
        && propertySymbol.ContainingType.Is(KnownType.Microsoft_AspNetCore_Mvc_ModelBinding_ModelStateDictionary);

    private static bool IsTryValidateInvocation(SyntaxNode node, SemanticModel model) =>
        node is InvocationExpressionSyntax invocation
        && invocation.GetName() == "TryValidateModel"
        && model.GetSymbolInfo(invocation.Expression).Symbol is IMethodSymbol method
        && method.ContainingType.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ControllerBase);
}

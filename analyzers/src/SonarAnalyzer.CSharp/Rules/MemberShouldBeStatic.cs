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
public sealed class MemberShouldBeStatic : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S2325";
    private const string MessageFormat = "Make '{0}' a static {1}.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly ImmutableHashSet<string> MethodNameWhitelist = ImmutableHashSet.Create(
        "Application_AuthenticateRequest",
        "Application_BeginRequest",
        "Application_End",
        "Application_EndRequest",
        "Application_Error",
        "Application_Init",
        "Application_Start",
        "Session_End",
        "Session_Start");

    private static readonly ImmutableHashSet<SymbolKind> InstanceSymbolKinds = ImmutableHashSet.Create(
        SymbolKind.Field,
        SymbolKind.Property,
        SymbolKind.Event,
        SymbolKind.Method);

    private static readonly ImmutableArray<KnownType> WebControllerTypes = ImmutableArray.Create(
        KnownType.System_Web_Mvc_Controller,
        KnownType.System_Web_Http_ApiController,
        KnownType.Microsoft_AspNetCore_Mvc_Controller,
        KnownType.System_Web_HttpApplication);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(
            c => CheckIssue<PropertyDeclarationSyntax>(c, GetPropertyDescendants, d => d.Identifier, "property"),
            SyntaxKind.PropertyDeclaration);

        context.RegisterNodeAction(
            c => CheckIssue<MethodDeclarationSyntax>(c, GetMethodDescendants, d => d.Identifier, "method"),
            SyntaxKind.MethodDeclaration);
    }

    private static IEnumerable<SyntaxNode> GetPropertyDescendants(PropertyDeclarationSyntax propertyDeclaration) =>
        propertyDeclaration.ExpressionBody is null
            ? propertyDeclaration.AccessorList.Accessors.SelectMany(x => x.DescendantNodes())
            : propertyDeclaration.ExpressionBody.DescendantNodes();

    private static IEnumerable<SyntaxNode> GetMethodDescendants(MethodDeclarationSyntax methodDeclaration) =>
        methodDeclaration.ExpressionBody is null
            ? methodDeclaration.Body?.DescendantNodes()
            : methodDeclaration.ExpressionBody.DescendantNodes();

    private static void CheckIssue<TDeclarationSyntax>(SonarSyntaxNodeReportingContext context,
        Func<TDeclarationSyntax, IEnumerable<SyntaxNode>> getDescendants,
        Func<TDeclarationSyntax, SyntaxToken> getIdentifier,
        string memberKind)
        where TDeclarationSyntax : MemberDeclarationSyntax
    {
        var declaration = (TDeclarationSyntax)context.Node;
        if (IsEmptyMethod(declaration)
            || CSharpFacade.Instance.Syntax.ModifierKinds(declaration).Contains(SyntaxKind.PartialKeyword))
        {
            return;
        }

        if (context.Model.GetDeclaredSymbol(declaration) is not { } methodOrPropertySymbol
            || IsStaticVirtualAbstractOrOverride()
            || MethodNameWhitelist.Contains(methodOrPropertySymbol.Name)
            || IsOverrideInterfaceOrNew()
            || IsExcludedByEnclosingType()
            || methodOrPropertySymbol.GetAttributes().Any(IsIgnoredAttribute)
            || IsAutoProperty(methodOrPropertySymbol)
            || IsPublicControllerMethod(methodOrPropertySymbol)
            || IsWindowsDesktopEventHandler(methodOrPropertySymbol))
        {
            return;
        }

        var descendants = getDescendants(declaration);
        if (descendants is null || HasInstanceReferences(descendants, context.Model))
        {
            return;
        }
        var identifier = getIdentifier(declaration);
        context.ReportIssue(Rule, identifier, identifier.Text, memberKind);

        bool IsStaticVirtualAbstractOrOverride() =>
            methodOrPropertySymbol.IsStatic || methodOrPropertySymbol.IsVirtual || methodOrPropertySymbol.IsAbstract || methodOrPropertySymbol.IsOverride;

        bool IsOverrideInterfaceOrNew() =>
            methodOrPropertySymbol.GetInterfaceMember() is not null
            || IsNewMethod(methodOrPropertySymbol)
            || IsNewProperty(methodOrPropertySymbol);

        bool IsExcludedByEnclosingType() =>
            methodOrPropertySymbol.ContainingType.IsInterface()
            // Any generic type in nesting chain with member accessible from outside (through the whole nesting chain) is excluded.
            || (methodOrPropertySymbol.ContainingType.IsGenericType && methodOrPropertySymbol.GetEffectiveAccessibility().IsAccessibleOutsideTheType())
            // Any nested private generic type with member accessible from outside that type (not the whole nesting chain) is also excluded.
            || (methodOrPropertySymbol.ContainingType.TypeArguments.Any() && methodOrPropertySymbol.DeclaredAccessibility.IsAccessibleOutsideTheType());
    }

    private static bool IsIgnoredAttribute(AttributeData attribute) =>
        !attribute.AttributeClass.Is(KnownType.System_Diagnostics_CodeAnalysis_SuppressMessageAttribute);

    private static bool IsEmptyMethod(MemberDeclarationSyntax node) =>
        node is MethodDeclarationSyntax { Body.Statements.Count: 0, ExpressionBody: null };

    private static bool IsNewMethod(ISymbol symbol) =>
        symbol.DeclaringSyntaxReferences
            .Select(x => x.GetSyntax())
            .OfType<MethodDeclarationSyntax>()
            .Any(x => x.Modifiers.Any(SyntaxKind.NewKeyword));

    private static bool IsNewProperty(ISymbol symbol) =>
        symbol.DeclaringSyntaxReferences
            .Select(x => x.GetSyntax())
            .OfType<PropertyDeclarationSyntax>()
            .Any(x => x.Modifiers.Any(SyntaxKind.NewKeyword));

    private static bool IsAutoProperty(ISymbol symbol) =>
        symbol.DeclaringSyntaxReferences
            .Select(x => x.GetSyntax())
            .OfType<PropertyDeclarationSyntax>()
            .Any(x => x.AccessorList is not null && x.AccessorList.Accessors.All(a => a.Body is null && a.ExpressionBody() is null));

    private static bool IsPublicControllerMethod(ISymbol symbol) =>
        symbol is IMethodSymbol methodSymbol
        && methodSymbol.GetEffectiveAccessibility() == Accessibility.Public
        && methodSymbol.ContainingType.DerivesFromAny(WebControllerTypes);

    private static bool IsWindowsDesktopEventHandler(ISymbol symbol) =>
        symbol is IMethodSymbol { Parameters.Length: 2 } methodSymbol
        && methodSymbol.Parameters[0].Type.Is(KnownType.System_Object)
        && methodSymbol.Parameters[1].Type.DerivesFrom(KnownType.System_EventArgs)
        && (IsContainingTypeWindowsForm(methodSymbol)
            || IsContainingTypeWpf(methodSymbol));

    private static bool IsContainingTypeWindowsForm(IMethodSymbol methodSymbol) =>
        methodSymbol.ContainingType.Implements(KnownType.System_Windows_Forms_IContainerControl);

    private static bool IsContainingTypeWpf(IMethodSymbol methodSymbol) =>
        methodSymbol.ContainingType.DerivesFrom(KnownType.System_Windows_FrameworkElement);

    private static bool HasInstanceReferences(IEnumerable<SyntaxNode> nodes, SemanticModel model) =>
        nodes.OfType<ExpressionSyntax>()
            .Where(IsLeftmostIdentifierName)
            .Where(x => !x.IsInNameOfArgument(model))
            .Any(x => IsInstanceMember(x, model));

    private static bool IsLeftmostIdentifierName(ExpressionSyntax node)
    {
        if (node is InstanceExpressionSyntax)
        {
            return true;
        }
        if (node is not SimpleNameSyntax)
        {
            return false;
        }

        var memberAccess = node.Parent as MemberAccessExpressionSyntax;
        var conditional = node.Parent as ConditionalAccessExpressionSyntax;
        var memberBinding = node.Parent as MemberBindingExpressionSyntax;

        return (memberAccess is null && conditional is null && memberBinding is null)
            || memberAccess?.Expression == node
            || conditional?.Expression == node;
    }

    private static bool IsInstanceMember(ExpressionSyntax node, SemanticModel model)
    {
        if (node is InstanceExpressionSyntax)
        {
            return true;
        }
        // For ctor(foo: bar), 'IsConstructorParameter(foo)' returns true. This check prevents that case.
        else if (node.Parent is NameColonSyntax)
        {
            return false;
        }
        return model.GetSymbolInfo(node).Symbol is { IsStatic: false } symbol
            && (InstanceSymbolKinds.Contains(symbol.Kind) || IsConstructorParameter(symbol));

        // Checking for primary constructor parameters
        static bool IsConstructorParameter(ISymbol symbol) =>
            symbol is IParameterSymbol { ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.Constructor } };
    }
}

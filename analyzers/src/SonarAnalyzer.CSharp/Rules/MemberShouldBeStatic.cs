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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MemberShouldBeStatic : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2325";
        private const string MessageFormat = "Make '{0}' a static {1}.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly ImmutableHashSet<string> MethodNameWhitelist =
            ImmutableHashSet.Create(
                "Application_AuthenticateRequest",
                "Application_BeginRequest",
                "Application_End",
                "Application_EndRequest",
                "Application_Error",
                "Application_Init",
                "Application_Start",
                "Session_End",
                "Session_Start"
            );

        private static readonly ImmutableHashSet<SymbolKind> InstanceSymbolKinds =
            ImmutableHashSet.Create(
                SymbolKind.Field,
                SymbolKind.Property,
                SymbolKind.Event,
                SymbolKind.Method);

        private static readonly ImmutableArray<KnownType> WebControllerTypes =
            ImmutableArray.Create(
                KnownType.System_Web_Mvc_Controller,
                KnownType.System_Web_Http_ApiController,
                KnownType.Microsoft_AspNetCore_Mvc_Controller,
                KnownType.System_Web_HttpApplication);

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
            propertyDeclaration.ExpressionBody == null
                ? propertyDeclaration.AccessorList.Accessors.SelectMany(a => a.DescendantNodes())
                : propertyDeclaration.ExpressionBody.DescendantNodes();

        private static IEnumerable<SyntaxNode> GetMethodDescendants(MethodDeclarationSyntax methodDeclaration) =>
            methodDeclaration.ExpressionBody == null
                ? methodDeclaration.Body?.DescendantNodes()
                : methodDeclaration.ExpressionBody.DescendantNodes();

        private static void CheckIssue<TDeclarationSyntax>(SonarSyntaxNodeReportingContext context,
                                                           Func<TDeclarationSyntax, IEnumerable<SyntaxNode>> getDescendants,
                                                           Func<TDeclarationSyntax, SyntaxToken> getIdentifier,
                                                           string memberKind)
            where TDeclarationSyntax : MemberDeclarationSyntax
        {
            var declaration = (TDeclarationSyntax)context.Node;
            if (IsEmptyMethod(declaration))
            {
                return;
            }

            if (context.SemanticModel.GetDeclaredSymbol(declaration) is not { } methodOrPropertySymbol
                || IsStaticVirtualAbstractOrOverride()
                || MethodNameWhitelist.Contains(methodOrPropertySymbol.Name)
                || IsOverrideInterfaceOrNew()
                || IsExcludedByEnclosingType()
                || methodOrPropertySymbol.GetAttributes().Any(IsIgnoredAttribute)
                || IsAutoProperty(methodOrPropertySymbol)
                || IsPublicControllerMethod(methodOrPropertySymbol)
                || IsWindowsFormsEventHandler(methodOrPropertySymbol))
            {
                return;
            }

            var descendants = getDescendants(declaration);
            if (descendants == null || HasInstanceReferences(descendants, context.SemanticModel))
            {
                return;
            }

            var identifier = getIdentifier(declaration);
            context.ReportIssue(CreateDiagnostic(Rule, identifier.GetLocation(), identifier.Text, memberKind));

            bool IsStaticVirtualAbstractOrOverride() =>
                methodOrPropertySymbol.IsStatic || methodOrPropertySymbol.IsVirtual || methodOrPropertySymbol.IsAbstract || methodOrPropertySymbol.IsOverride;

            bool IsOverrideInterfaceOrNew() =>
                methodOrPropertySymbol.GetInterfaceMember() != null
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
                  .Select(r => r.GetSyntax())
                  .OfType<MethodDeclarationSyntax>()
                  .Any(s => s.Modifiers.Any(SyntaxKind.NewKeyword));

        private static bool IsNewProperty(ISymbol symbol) =>
            symbol.DeclaringSyntaxReferences
                  .Select(r => r.GetSyntax())
                  .OfType<PropertyDeclarationSyntax>()
                  .Any(s => s.Modifiers.Any(SyntaxKind.NewKeyword));

        private static bool IsAutoProperty(ISymbol symbol) =>
            symbol.DeclaringSyntaxReferences
                  .Select(r => r.GetSyntax())
                  .OfType<PropertyDeclarationSyntax>()
                  .Any(s => s.AccessorList != null && s.AccessorList.Accessors.All(a => a.Body == null && a.ExpressionBody() == null));

        private static bool IsPublicControllerMethod(ISymbol symbol) =>
            symbol is IMethodSymbol methodSymbol
            && methodSymbol.GetEffectiveAccessibility() == Accessibility.Public
            && methodSymbol.ContainingType.DerivesFromAny(WebControllerTypes);

        private static bool IsWindowsFormsEventHandler(ISymbol symbol) =>
            symbol is IMethodSymbol { Parameters.Length: 2 } methodSymbol
            && methodSymbol.Parameters[0].Type.Is(KnownType.System_Object)
            && methodSymbol.Parameters[1].Type.DerivesFrom(KnownType.System_EventArgs)
            && methodSymbol.ContainingType.Implements(KnownType.System_Windows_Forms_IContainerControl);

        private static bool HasInstanceReferences(IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel) =>
            nodes.OfType<ExpressionSyntax>()
                 .Where(IsLeftmostIdentifierName)
                 .Where(n => !n.IsInNameOfArgument(semanticModel))
                 .Any(n => IsInstanceMember(n, semanticModel));

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

            return (memberAccess == null && conditional == null && memberBinding == null)
                || memberAccess?.Expression == node
                || conditional?.Expression == node;
        }

        private static bool IsInstanceMember(ExpressionSyntax node, SemanticModel semanticModel)
        {
            if (node is InstanceExpressionSyntax)
            {
                return true;
            }

            return semanticModel.GetSymbolInfo(node).Symbol is { IsStatic: false } symbol
                   && InstanceSymbolKinds.Contains(symbol.Kind);
        }
    }
}

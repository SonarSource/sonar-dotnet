/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class MemberShouldBeStatic : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2325";
        internal const string MessageFormat = "Make '{0}' a static {1}.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ISet<SymbolKind> InstanceSymbolKinds = new HashSet<SymbolKind>
        {
            SymbolKind.Field,
            SymbolKind.Property,
            SymbolKind.Event,
            SymbolKind.Method
        };

        private static readonly ISet<KnownType> WebControllerTypes = new HashSet<KnownType>
        {
            KnownType.System_Web_Mvc_Controller,
            KnownType.System_Web_Http_ApiController,
            KnownType.Microsoft_AspNetCore_Mvc_Controller
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckIssue<PropertyDeclarationSyntax>(c, GetPropertyDescendants, d => d.Identifier, "property"),
                SyntaxKind.PropertyDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckIssue<MethodDeclarationSyntax>(c, GetMethodDescendants, d => d.Identifier, "method"),
                SyntaxKind.MethodDeclaration);
        }

        private IEnumerable<SyntaxNode> GetPropertyDescendants(PropertyDeclarationSyntax propertyDeclaration)
        {
            if (propertyDeclaration.ExpressionBody != null)
            {
                return propertyDeclaration.ExpressionBody.DescendantNodes();
            }

            return propertyDeclaration.AccessorList.Accessors.SelectMany(a => a.DescendantNodes());
        }

        private IEnumerable<SyntaxNode> GetMethodDescendants(MethodDeclarationSyntax methodDeclaration)
        {
            if (methodDeclaration.ExpressionBody != null)
            {
                return methodDeclaration.ExpressionBody.DescendantNodes();
            }

            return methodDeclaration.Body?.DescendantNodes();
        }

        private static void CheckIssue<TDeclarationSyntax>(SyntaxNodeAnalysisContext context,
            Func<TDeclarationSyntax, IEnumerable<SyntaxNode>> getDescendants,
            Func<TDeclarationSyntax, SyntaxToken> getIdentifier,
            string memberKind)
            where TDeclarationSyntax : MemberDeclarationSyntax
        {
            var declaration = (TDeclarationSyntax)context.Node;

            var symbol = context.SemanticModel.GetDeclaredSymbol(declaration);

            if (symbol == null ||
                symbol.IsStatic ||
                symbol.IsVirtual ||
                symbol.IsAbstract ||
                symbol.IsOverride ||
                symbol.ContainingType.IsInterface() ||
                symbol.GetInterfaceMember() != null ||
                symbol.GetOverriddenMember() != null ||
                symbol.GetAttributes().Any(IsIgnoredAttribute) ||
                IsNewMethod(symbol) ||
                IsEmptyMethod(declaration) ||
                IsNewProperty(symbol) ||
                IsAutoProperty(symbol) ||
                IsPublicControllerMethod(symbol))
            {
                return;
            }

            var descendants = getDescendants(declaration);
            if (descendants == null || HasInstanceReferences(descendants, context.SemanticModel))
            {
                return;
            }

            var identifier = getIdentifier(declaration);
            context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, identifier.GetLocation(), identifier.Text, memberKind));
        }

        private static bool IsIgnoredAttribute(AttributeData attribute)
        {
            return !attribute.AttributeClass.Is(KnownType.System_Diagnostics_CodeAnalysis_SuppressMessageAttribute);
        }

        private static bool IsEmptyMethod(MemberDeclarationSyntax node)
        {
            return node is MethodDeclarationSyntax methodDeclarationSyntax &&
                methodDeclarationSyntax.Body?.Statements.Count == 0 &&
                methodDeclarationSyntax.ExpressionBody == null;
        }

        private static bool IsNewMethod(ISymbol symbol)
        {
            return symbol.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax())
                .OfType<MethodDeclarationSyntax>()
                .Any(s => s.Modifiers.Any(SyntaxKind.NewKeyword));
        }

        private static bool IsNewProperty(ISymbol symbol)
        {
            return symbol.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax())
                .OfType<PropertyDeclarationSyntax>()
                .Any(s => s.Modifiers.Any(SyntaxKind.NewKeyword));
        }

        private static bool IsAutoProperty(ISymbol symbol)
        {
            return symbol.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax())
                .OfType<PropertyDeclarationSyntax>()
                .Any(s => s.AccessorList != null && s.AccessorList.Accessors.All(a => a.Body == null && a.ExpressionBody() == null));
        }

        private static bool IsPublicControllerMethod(ISymbol symbol)
        {
            if (symbol is IMethodSymbol methodSymbol)
            {
                return methodSymbol.DeclaredAccessibility == Accessibility.Public &&
                    methodSymbol.ContainingType.DerivesFromAny(WebControllerTypes);
            }

            return false;
        }

        private static bool HasInstanceReferences(IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel)
        {
            return nodes.OfType<ExpressionSyntax>()
                .Where(IsLeftmostIdentifierName)
                .Where(n => !CSharpSyntaxHelper.IsInNameofCall(n, semanticModel))
                .Any(n => IsInstanceMember(n, semanticModel));
        }

        private static bool IsLeftmostIdentifierName(ExpressionSyntax node)
        {
            if (node is InstanceExpressionSyntax)
            {
                return true;
            }

            if (!(node is SimpleNameSyntax))
            {
                return false;
            }

            var memberAccess = node.Parent as MemberAccessExpressionSyntax;
            var conditional = node.Parent as ConditionalAccessExpressionSyntax;
            var memberBinding = node.Parent as MemberBindingExpressionSyntax;

            return
                memberAccess == null && conditional == null && memberBinding == null ||
                memberAccess?.Expression == node ||
                conditional?.Expression == node;
        }

        private static bool IsInstanceMember(ExpressionSyntax node, SemanticModel semanticModel)
        {
            if (node is InstanceExpressionSyntax)
            {
                return true;
            }

            var symbol = semanticModel.GetSymbolInfo(node).Symbol;

            return symbol != null &&
                !symbol.IsStatic &&
                InstanceSymbolKinds.Contains(symbol.Kind);
        }
    }
}

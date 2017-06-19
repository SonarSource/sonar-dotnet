/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class MemberShouldBeStatic : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2325";
        internal const string MessageFormat = "Make '{0}' a static {1}.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<SymbolKind> InstanceSymbolKinds = new HashSet<SymbolKind>
        {
            SymbolKind.Field,
            SymbolKind.Property,
            SymbolKind.Event,
            SymbolKind.Method
        };

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckIssue<PropertyDeclarationSyntax>(c, d => d.Identifier, "property"),
                SyntaxKind.PropertyDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckIssue<MethodDeclarationSyntax>(c, d => d.Identifier, "method"),
                SyntaxKind.MethodDeclaration);
        }

        private static void CheckIssue<TDeclarationSyntax>(SyntaxNodeAnalysisContext context, Func<TDeclarationSyntax, SyntaxToken> getIdentifier, string memberKind)
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
                IsNewMethod(symbol) ||
                IsEmptyMethod(declaration) ||
                IsNewProperty(symbol) ||
                IsAutoProperty(symbol) ||
                HasInstanceReferences(declaration, context.SemanticModel))
            {
                return;
            }

            var identifier = getIdentifier(declaration);
            context.ReportDiagnostic(Diagnostic.Create(rule, identifier.GetLocation(), identifier.Text, memberKind));
        }

        private static bool IsEmptyMethod(MemberDeclarationSyntax node)
        {
            var methodDeclarationSyntax = node as MethodDeclarationSyntax;
            return methodDeclarationSyntax != null &&
                methodDeclarationSyntax.Body != null &&
                !methodDeclarationSyntax.Body.Statements.Any();
        }

        private static bool IsNewMethod(ISymbol symbol)
        {
            return symbol.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax())
                .OfType<MethodDeclarationSyntax>()
                .Any(s => s.Modifiers.Any(m => m.IsKind(SyntaxKind.NewKeyword)));
        }

        private static bool IsNewProperty(ISymbol symbol)
        {
            return symbol.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax())
                .OfType<PropertyDeclarationSyntax>()
                .Any(s => s.Modifiers.Any(m => m.IsKind(SyntaxKind.NewKeyword)));
        }

        private static bool IsAutoProperty(ISymbol symbol)
        {
            return symbol.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax())
                .OfType<PropertyDeclarationSyntax>()
                .Any(s => s.AccessorList != null && s.AccessorList.Accessors.All(a => a.Body == null));
        }

        private static bool HasInstanceReferences(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel)
        {
            return memberDeclaration.DescendantNodes()
                .OfType<ExpressionSyntax>()
                .Where(IsLeftmostIdentifierName)
                .Where(n => !SyntaxHelper.IsInNameofCall(n, semanticModel))
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

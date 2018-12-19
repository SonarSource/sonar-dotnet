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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.Helpers
{
    /// <summary>
    /// Collects private or internal member symbols that could potentially be removed if they are not used.
    /// Members that are overriden, overridable, have specific use, etc. are not removable.
    /// </summary>
    internal class CSharpRemovableSymbolCollector : CSharpSyntaxWalker
    {
        private static readonly ISet<MethodKind> RemovableMethodKinds = new HashSet<MethodKind>
        {
            MethodKind.Ordinary,
            MethodKind.Constructor
        };

        private readonly Func<SyntaxNode, SemanticModel> getSemanticModel;

        public HashSet<ISymbol> PrivateSymbols { get; } =
            new HashSet<ISymbol>();

        public HashSet<ISymbol> InternalSymbols { get; } =
            new HashSet<ISymbol>();

        public BidirectionalDictionary<ISymbol, SyntaxNode> FieldLikeSymbols { get; } =
            new BidirectionalDictionary<ISymbol, SyntaxNode>();

        public CSharpRemovableSymbolCollector(Func<SyntaxTree, bool, SemanticModel> getSemanticModel)
        {
            this.getSemanticModel = node => getSemanticModel(node.SyntaxTree, false);
        }

        private ISymbol GetDeclaredSymbol(SyntaxNode syntaxNode) =>
            this.getSemanticModel(syntaxNode).GetDeclaredSymbol(syntaxNode);

        private void ConditionalStore<TSymbol>(TSymbol symbol, Func<TSymbol, bool> condition)
            where TSymbol : ISymbol
        {
            if (condition(symbol))
            {
                if (symbol.GetEffectiveAccessibility() == Accessibility.Private)
                {
                    PrivateSymbols.Add(symbol);
                }
                else
                {
                    InternalSymbols.Add(symbol);
                }
            }
        }

        private void StoreRemovableVariableDeclarations(BaseFieldDeclarationSyntax node)
        {
            foreach (var variable in node.Declaration.Variables)
            {
                var symbol = GetDeclaredSymbol(variable);
                if (IsRemovableMember(symbol))
                {
                    PrivateSymbols.Add(symbol);
                    FieldLikeSymbols.Add(symbol, variable);
                }
            }
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            ConditionalStore(GetDeclaredSymbol(node), IsRemovableType);
            base.VisitClassDeclaration(node);
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            ConditionalStore(GetDeclaredSymbol(node), IsRemovableType);
            base.VisitStructDeclaration(node);
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            ConditionalStore(GetDeclaredSymbol(node), IsRemovableType);
            base.VisitInterfaceDeclaration(node);
        }

        public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
        {
            ConditionalStore(GetDeclaredSymbol(node), IsRemovableType);
            base.VisitDelegateDeclaration(node);
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            StoreRemovableVariableDeclarations(node);
            base.VisitFieldDeclaration(node);
        }

        public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
        {
            StoreRemovableVariableDeclarations(node);
            base.VisitEventFieldDeclaration(node);
        }

        public override void VisitEventDeclaration(EventDeclarationSyntax node)
        {
            ConditionalStore(GetDeclaredSymbol(node), IsRemovableMember);
            base.VisitEventDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            ConditionalStore(GetDeclaredSymbol(node), IsRemovableMember);
            base.VisitPropertyDeclaration(node);
        }

        public override void VisitIndexerDeclaration(IndexerDeclarationSyntax node)
        {
            ConditionalStore(GetDeclaredSymbol(node), IsRemovableMember);
            base.VisitIndexerDeclaration(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var symbol = (IMethodSymbol)GetDeclaredSymbol(node);
            ConditionalStore(symbol.PartialDefinitionPart ?? symbol, IsRemovableMethod);
            base.VisitMethodDeclaration(node);
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            if (!IsEmptyConstructor(node))
            {
                ConditionalStore((IMethodSymbol)GetDeclaredSymbol(node), IsRemovableMethod);
            }
            base.VisitConstructorDeclaration(node);
        }

        private bool IsRemovableType(ISymbol typeSymbol)
        {
            var accessibility = typeSymbol.GetEffectiveAccessibility();
            return typeSymbol.ContainingType != null
                && (accessibility == Accessibility.Private || accessibility == Accessibility.Internal)
                && IsRemovable(typeSymbol);
        }

        private bool IsRemovableMember(ISymbol symbol) =>
            symbol.GetEffectiveAccessibility() == Accessibility.Private &&
            IsRemovable(symbol);

        private bool IsRemovableMethod(IMethodSymbol methodSymbol) =>
            IsRemovableMember(methodSymbol) &&
            RemovableMethodKinds.Contains(methodSymbol.MethodKind) &&
            !methodSymbol.IsMainMethod() &&
            !methodSymbol.IsEventHandler() && // Event handlers could be added in XAML and no method reference will be generated in the .g.cs file.
            !methodSymbol.IsSerializationConstructor();

        private bool IsRemovable(ISymbol symbol) =>
            symbol != null &&
            !symbol.IsImplicitlyDeclared &&
            !symbol.IsAbstract &&
            !symbol.IsVirtual &&
            !symbol.GetAttributes().Any() &&
            !symbol.ContainingType.IsInterface() &&
            symbol.GetInterfaceMember() == null &&
            symbol.GetOverriddenMember() == null;

        private static bool IsEmptyConstructor(ConstructorDeclarationSyntax constructorDeclaration) =>
            !constructorDeclaration.HasBodyOrExpressionBody() ||
            (constructorDeclaration.Body != null && constructorDeclaration.Body.Statements.Count == 0);
    }
}

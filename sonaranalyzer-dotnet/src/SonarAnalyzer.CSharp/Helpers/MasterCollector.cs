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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Helpers
{
    public class UsageCollector : CSharpSyntaxWalker
    {
        private readonly Func<SyntaxNode, SemanticModel> getSemanticModel;
        private readonly HashSet<string> symbolNames;

        private readonly HashSet<SyntaxNodeSemanticModelTuple<ExpressionSyntax>> usages =
            new HashSet<SyntaxNodeSemanticModelTuple<ExpressionSyntax>>();

        public UsageCollector(Func<SyntaxNode, SemanticModel> getSemanticModel, HashSet<string> symbolNames)
        {
            this.getSemanticModel = getSemanticModel;
            this.symbolNames = symbolNames;
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (symbolNames.Contains(node.Identifier.ValueText))
            {
                usages.Add(new SyntaxNodeSemanticModelTuple<ExpressionSyntax>
                {
                    SyntaxNode = node,
                    SemanticModel = getSemanticModel(node)
                });
            }

            base.VisitIdentifierName(node);
        }
    }

    // TODO: rename
    internal class DeclarationCollector : CSharpSyntaxWalker
    {
        private Func<SyntaxNode, SemanticModel> getSemanticModel;

        public readonly HashSet<ISymbol> removableSymbols =
            new HashSet<ISymbol>();

        public readonly BidirectionalDictionary<ISymbol, SyntaxNode> fieldLikeSymbols =
            new BidirectionalDictionary<ISymbol, SyntaxNode>();

        public DeclarationCollector(Func<SyntaxNode, SemanticModel> getSemanticModel)
        {
            this.getSemanticModel = getSemanticModel;
        }

        private ISymbol GetDeclaredSymbol(SyntaxNode syntaxNode) =>
            getSemanticModel(syntaxNode).GetDeclaredSymbol(syntaxNode);

        private void ConditionalStore<TSymbol>(TSymbol symbol, Func<TSymbol, bool> condition)
            where TSymbol : ISymbol
        {
            if (condition(symbol))
            {
                removableSymbols.Add(symbol);
            }
        }

        private void StoreRemovableVariableDeclarations(BaseFieldDeclarationSyntax node)
        {
            foreach (var variable in node.Declaration.Variables)
            {
                var symbol = GetDeclaredSymbol(variable);
                if (IsRemovableMember(symbol))
                {
                    removableSymbols.Add(symbol);
                    fieldLikeSymbols.Add(symbol, variable);
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
            ConditionalStore((IMethodSymbol)GetDeclaredSymbol(node), IsRemovableMethod);
            base.VisitConstructorDeclaration(node);
        }

        private bool IsRemovable(ISymbol symbol, Accessibility maxAccessibility) =>
            symbol != null &&
            symbol.GetEffectiveAccessibility() <= maxAccessibility &&
            !symbol.IsImplicitlyDeclared &&
            !symbol.IsAbstract &&
            !symbol.IsVirtual &&
            !symbol.GetAttributes().Any() &&
            !symbol.ContainingType.IsInterface() &&
            symbol.GetInterfaceMember() == null &&
            symbol.GetOverriddenMember() == null;

        private bool IsRemovableType(ISymbol typeSymbol) =>
            typeSymbol.ContainingType != null &&
            IsRemovable(typeSymbol, Accessibility.Internal);

        private bool IsRemovableMember(ISymbol typeSymbol) =>
            IsRemovable(typeSymbol, Accessibility.Private);

        private bool IsRemovableMethod(IMethodSymbol methodSymbol) =>
            IsRemovableMember(methodSymbol) &&
            RemovableMethodKinds.Contains(methodSymbol.MethodKind) &&
            !methodSymbol.IsMainMethod() &&
            !methodSymbol.IsEventHandler() &&
            !methodSymbol.IsSerializationConstructor();

        private static readonly ISet<MethodKind> RemovableMethodKinds = new HashSet<MethodKind>
        {
            MethodKind.Ordinary,
            MethodKind.Constructor
        };
    }
}

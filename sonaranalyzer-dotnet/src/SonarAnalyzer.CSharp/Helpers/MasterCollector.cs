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
    /*
        TODO:
        - element access
        - object creations
        - properties
    */
    public class UsageCollector : CSharpSyntaxWalker
    {
        private static readonly ISet<SyntaxKind> IncrementKinds = new HashSet<SyntaxKind>
        {
            SyntaxKind.PostIncrementExpression,
            SyntaxKind.PreIncrementExpression,
            SyntaxKind.PostDecrementExpression,
            SyntaxKind.PreDecrementExpression
        };

        private readonly Func<SyntaxNode, SemanticModel> getSemanticModel;
        private readonly HashSet<string> symbolNames;

        public readonly HashSet<ISymbol> usages =
            new HashSet<ISymbol>();

        public readonly HashSet<ISymbol> emptyConstructors =
            new HashSet<ISymbol>();

        public readonly Dictionary<IPropertySymbol, Rules.CSharp.UnusedPrivateMember.AccessorAccess> propertyAccess =
            new Dictionary<IPropertySymbol, Rules.CSharp.UnusedPrivateMember.AccessorAccess>();

        public UsageCollector(Func<SyntaxNode, SemanticModel> getSemanticModel, HashSet<string> symbolNames)
        {
            this.getSemanticModel = getSemanticModel;
            this.symbolNames = symbolNames;
        }

        private IEnumerable<ISymbol> GetSymbols<TSyntaxNode>(TSyntaxNode node, Func<TSyntaxNode, bool> condition)
            where TSyntaxNode : SyntaxNode
        {
            return condition(node)
                ? GetCandidateSymbols(getSemanticModel(node).GetSymbolInfo(node))
                : Enumerable.Empty<ISymbol>();

            IEnumerable<ISymbol> GetCandidateSymbols(SymbolInfo symbolInfo)
            {
                return new[] { symbolInfo.Symbol }
                    .Concat(symbolInfo.CandidateSymbols)
                    .Select(GetOriginalDefinition)
                    .WhereNotNull();

                ISymbol GetOriginalDefinition(ISymbol candidateSymbol)
                {
                    if (candidateSymbol is IMethodSymbol methodSymbol &&
                        methodSymbol.MethodKind == MethodKind.ReducedExtension)
                    {
                        return methodSymbol.ReducedFrom?.OriginalDefinition;
                    }
                    return candidateSymbol?.OriginalDefinition;
                }
            }
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            var identifierSymbols = GetSymbols(node, IsKnownIdentifier).ToList();
            usages.UnionWith(identifierSymbols);

            TryStorePropertyAccess(node, identifierSymbols);

            base.VisitIdentifierName(node);
        }

        private void TryStorePropertyAccess(IdentifierNameSyntax node, List<ISymbol> identifierSymbols)
        {
            var propertySymbols = identifierSymbols.OfType<IPropertySymbol>();
            if (propertySymbols.Any())
            {
                var access = EvaluatePropertyAccesses(node);
                foreach (var propertySymbol in propertySymbols)
                {
                    StorePropertyAccess(access, propertySymbol);
                }
            }
        }

        private void StorePropertyAccess(Rules.CSharp.UnusedPrivateMember.AccessorAccess access, IPropertySymbol propertySymbol)
        {
            if (propertyAccess.ContainsKey(propertySymbol))
            {
                propertyAccess[propertySymbol] |= access;
            }
            else
            {
                propertyAccess[propertySymbol] = access;
            }
        }

        private Rules.CSharp.UnusedPrivateMember.AccessorAccess EvaluatePropertyAccesses(IdentifierNameSyntax node)
        {
            var topmostSyntax = GetTopmostSyntaxWithTheSameSymbol(node);

            if (topmostSyntax.Parent is AssignmentExpressionSyntax assignmentExpression)
            {
                if (assignmentExpression.IsKind(SyntaxKind.SimpleAssignmentExpression))
                {
                    // Prop = value --> set
                    // value = Prop --> get
                    return assignmentExpression.Left == topmostSyntax
                        ? Rules.CSharp.UnusedPrivateMember.AccessorAccess.Set
                        : Rules.CSharp.UnusedPrivateMember.AccessorAccess.Get;
                }
                else
                {
                    // Prop += value --> get/set
                    return Rules.CSharp.UnusedPrivateMember.AccessorAccess.Both;
                }
            }
            // Prop++ --> get/set
            // TODO: nameof(Prop) --> get/set
            return topmostSyntax.Parent.IsAnyKind(IncrementKinds)
                ? Rules.CSharp.UnusedPrivateMember.AccessorAccess.Both
                : Rules.CSharp.UnusedPrivateMember.AccessorAccess.Get;
        }

        private SyntaxNode GetTopmostSyntaxWithTheSameSymbol(SyntaxNode identifier)
        {
            // All of the cases below could be parts of invocation or other expressions
            switch (identifier.Parent)
            {
                case MemberAccessExpressionSyntax memberAccess when memberAccess.Name == identifier:
                    // this.identifier or a.identifier or ((a)).identifier, but not identifier.other
                    return memberAccess.GetSelfOrTopParenthesizedExpression();
                case MemberBindingExpressionSyntax memberBinding when memberBinding.Name == identifier:
                    // this?.identifier or a?.identifier or ((a))?.identifier, but not identifier?.other
                    return memberBinding.Parent.GetSelfOrTopParenthesizedExpression();
                default:
                    // identifier or ((identifier))
                    return identifier.GetSelfOrTopParenthesizedExpression();
            }
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (node.Initializer != null)
            {
                var symbol = getSemanticModel(node).GetDeclaredSymbol(node);
                usages.Add(symbol);
                StorePropertyAccess(Rules.CSharp.UnusedPrivateMember.AccessorAccess.Set, symbol);
            }
            base.VisitPropertyDeclaration(node);
        }

        public override void VisitGenericName(GenericNameSyntax node)
        {
            usages.UnionWith(GetSymbols(node, IsKnownIdentifier));
            base.VisitGenericName(node);
        }

        public override void VisitConstructorInitializer(ConstructorInitializerSyntax node)
        {
            usages.UnionWith(GetSymbols(node, x => true));
            base.VisitConstructorInitializer(node);
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            emptyConstructors.UnionWith(GetSymbols(node, IsEmptyConstructor));
            base.VisitConstructorDeclaration(node);
        }

        private static bool IsEmptyConstructor(ConstructorDeclarationSyntax constructorDeclaration) =>
            constructorDeclaration.Body == null || constructorDeclaration.Body.Statements.Count == 0;

        private bool IsKnownIdentifier(SimpleNameSyntax nameSyntax) =>
            symbolNames.Contains(nameSyntax.Identifier.ValueText);
    }

    // TODO: rename
    internal class DeclarationCollector : CSharpSyntaxWalker
    {
        private static readonly ISet<MethodKind> RemovableMethodKinds = new HashSet<MethodKind>
        {
            MethodKind.Ordinary,
            MethodKind.Constructor
        };

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
    }
}

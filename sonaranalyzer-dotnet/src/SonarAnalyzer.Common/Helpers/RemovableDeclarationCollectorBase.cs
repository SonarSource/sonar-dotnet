/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    using SyntaxNodeSymbolSemanticModelTuple = SyntaxNodeSymbolSemanticModelTuple<SyntaxNode, ISymbol>;

    // For C#, TOwnerOfSubnodes == TDeclaration == BaseTypeDeclarationSyntax
    // For VB, TOwnerOfSubnodes == TypeBlockSyntax, TDeclaration = TypeStatementSyntax
    public abstract class RemovableDeclarationCollectorBase<TOwnerOfSubnodes, TDeclaration, TSyntaxKind>
        where TOwnerOfSubnodes : SyntaxNode
        where TDeclaration : SyntaxNode
    {
        private readonly Compilation compilation;
        private readonly INamedTypeSymbol namedType;

        private IEnumerable<SyntaxNodeAndSemanticModel<TOwnerOfSubnodes>> typeDeclarations;

        protected RemovableDeclarationCollectorBase(INamedTypeSymbol namedType, Compilation compilation)
        {
            this.namedType = namedType;
            this.compilation = compilation;
        }

        public IEnumerable<SyntaxNodeAndSemanticModel<TOwnerOfSubnodes>> TypeDeclarations
        {
            get
            {
                if (this.typeDeclarations == null)
                {
                    this.typeDeclarations = this.namedType.DeclaringSyntaxReferences
                        .Select(reference => reference.GetSyntax())
                        .OfType<TDeclaration>()
                        .Select(node =>
                            new SyntaxNodeAndSemanticModel<TOwnerOfSubnodes>
                            {
                                SyntaxNode = GetOwnerOfSubnodes(node),
                                SemanticModel = this.compilation.GetSemanticModel(node.SyntaxTree)
                            })
                        .Where(n => n.SemanticModel != null);
                }
                return this.typeDeclarations;
            }
        }

        internal abstract TOwnerOfSubnodes GetOwnerOfSubnodes(TDeclaration node);

        public IEnumerable<SyntaxNodeSymbolSemanticModelTuple> GetRemovableDeclarations(
            ISet<TSyntaxKind> kinds, Accessibility maxAcessibility)
        {
            return TypeDeclarations
                .SelectMany(container => SelectMatchingDeclarations(container, kinds)
                    .Select(node => SelectNodeTuple(node, container.SemanticModel)))
                    .Where(tuple => IsRemovable(tuple.Symbol, maxAcessibility));
        }

        public abstract IEnumerable<SyntaxNodeSymbolSemanticModelTuple> GetRemovableFieldLikeDeclarations(
            ISet<TSyntaxKind> kinds, Accessibility maxAcessibility);

        public static bool IsRemovable(IMethodSymbol methodSymbol, Accessibility maxAccessibility)
        {
            return IsRemovable((ISymbol)methodSymbol, maxAccessibility) &&
                (methodSymbol.MethodKind == MethodKind.Ordinary || methodSymbol.MethodKind == MethodKind.Constructor) &&
                !methodSymbol.IsMainMethod() &&
                !methodSymbol.IsEventHandler() &&
                !methodSymbol.IsSerializationConstructor();
        }

        public static bool IsRemovable(ISymbol symbol, Accessibility maxAccessibility)
        {
            return symbol != null &&
                symbol.GetEffectiveAccessibility() <= maxAccessibility &&
                !symbol.IsImplicitlyDeclared &&
                !symbol.IsAbstract &&
                !symbol.IsVirtual &&
                !symbol.GetAttributes().Any() &&
                !symbol.ContainingType.IsInterface() &&
                symbol.GetInterfaceMember() == null &&
                symbol.GetOverriddenMember() == null;
        }

        protected static SyntaxNodeSymbolSemanticModelTuple SelectNodeTuple(SyntaxNode node, SemanticModel semanticModel)
        {
            return new SyntaxNodeSymbolSemanticModelTuple
            {
                SyntaxNode = node,
                Symbol = semanticModel.GetDeclaredSymbol(node),
                SemanticModel = semanticModel
            };
        }

        protected abstract IEnumerable<SyntaxNode> SelectMatchingDeclarations(
            SyntaxNodeAndSemanticModel<TOwnerOfSubnodes> container,
            ISet<TSyntaxKind> kinds);
    }
}

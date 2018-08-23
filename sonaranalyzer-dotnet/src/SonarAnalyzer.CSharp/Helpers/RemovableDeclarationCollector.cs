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

namespace SonarAnalyzer.Helpers
{
    using SyntaxNodeSymbolSemanticModelTuple = SyntaxNodeSymbolSemanticModelTuple<SyntaxNode, ISymbol>;
    using TypeWithSemanticModel = SyntaxNodeSemanticModelTuple<BaseTypeDeclarationSyntax>;

    internal class RemovableDeclarationCollector
    {
        private readonly Compilation compilation;
        private readonly INamedTypeSymbol namedType;

        private IEnumerable<TypeWithSemanticModel> typeDeclarations;

        public static readonly Func<SyntaxNode, bool> IsNodeStructOrClassDeclaration =
            node => node.IsKind(SyntaxKind.ClassDeclaration) ||
                node.IsKind(SyntaxKind.StructDeclaration);

        public static readonly Func<SyntaxNode, bool> IsNodeContainerTypeDeclaration =
            node => IsNodeStructOrClassDeclaration(node) ||
                node.IsKind(SyntaxKind.InterfaceDeclaration);

        public RemovableDeclarationCollector(INamedTypeSymbol namedType, Compilation compilation)
        {
            this.namedType = namedType;
            this.compilation = compilation;
        }

        public IEnumerable<TypeWithSemanticModel> TypeDeclarations
        {
            get
            {
                if (this.typeDeclarations == null)
                {
                    this.typeDeclarations = this.namedType.DeclaringSyntaxReferences
                        .Select(reference => reference.GetSyntax())
                        .OfType<BaseTypeDeclarationSyntax>()
                        .Select(node =>
                            new TypeWithSemanticModel
                            {
                                SyntaxNode = node,
                                SemanticModel = this.compilation.GetSemanticModel(node.SyntaxTree)
                            })
                        .Where(n => n.SemanticModel != null);
                }
                return this.typeDeclarations;
            }
        }

        public IEnumerable<SyntaxNodeSymbolSemanticModelTuple> GetRemovableDeclarations(
            ISet<SyntaxKind> kinds, Accessibility maxAcessibility)
        {
            return TypeDeclarations
                .SelectMany(container => SelectMatchingDeclarations(container, kinds)
                    .Select(node => SelectNodeTuple(node, container.SemanticModel)))
                    .Where(tuple => IsRemovable(tuple.Symbol, maxAcessibility));
        }

        public IEnumerable<SyntaxNodeSymbolSemanticModelTuple> GetRemovableFieldLikeDeclarations(
            ISet<SyntaxKind> kinds, Accessibility maxAcessibility)
        {
            var fieldLikeNodes = TypeDeclarations
                .SelectMany(typeDeclaration => SelectMatchingDeclarations(typeDeclaration, kinds)
                    .Select(node =>
                        new SyntaxNodeSemanticModelTuple<BaseFieldDeclarationSyntax>
                        {
                            SyntaxNode = (BaseFieldDeclarationSyntax)node,
                            SemanticModel = typeDeclaration.SemanticModel
                        }));

            return fieldLikeNodes
                .SelectMany(fieldLikeNode => fieldLikeNode.SyntaxNode.Declaration.Variables
                    .Select(variable => SelectNodeTuple(variable, fieldLikeNode.SemanticModel))
                    .Where(tuple => IsRemovable(tuple.Symbol, maxAcessibility)));
        }

        private static readonly ISet<MethodKind> RemovableMethodKinds = new HashSet<MethodKind>
        {
            MethodKind.Ordinary,
            MethodKind.Constructor
        };

        public static bool IsRemovable(IMethodSymbol methodSymbol, Accessibility maxAccessibility)
        {
            return IsRemovable((ISymbol)methodSymbol, maxAccessibility) &&
                RemovableMethodKinds.Contains(methodSymbol.MethodKind) &&
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

        private static SyntaxNodeSymbolSemanticModelTuple SelectNodeTuple(SyntaxNode node, SemanticModel semanticModel)
        {
            return new SyntaxNodeSymbolSemanticModelTuple
            {
                SyntaxNode = node,
                Symbol = semanticModel.GetDeclaredSymbol(node),
                SemanticModel = semanticModel
            };
        }

        private static IEnumerable<SyntaxNode> SelectMatchingDeclarations(TypeWithSemanticModel container,
            ISet<SyntaxKind> kinds)
        {
            return container.SyntaxNode.DescendantNodes(IsNodeContainerTypeDeclaration)
                .Where(node => kinds.Contains(node.Kind()));
        }
    }
}

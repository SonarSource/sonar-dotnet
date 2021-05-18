/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Helpers
{
    using SyntaxNodeSymbolSemanticModelTuple = SyntaxNodeSymbolSemanticModelTuple<SyntaxNode, ISymbol>;

    internal class CSharpRemovableDeclarationCollector :
        RemovableDeclarationCollectorBase<BaseTypeDeclarationSyntax, BaseTypeDeclarationSyntax, SyntaxKind>
    {
        public CSharpRemovableDeclarationCollector(INamedTypeSymbol namedType, Compilation compilation)
            : base(namedType, compilation)
        {
        }

        protected override IEnumerable<SyntaxNode> SelectMatchingDeclarations(
            SyntaxNodeAndSemanticModel<BaseTypeDeclarationSyntax> container, ISet<SyntaxKind> kinds) =>
            container.SyntaxNode.DescendantNodes(IsNodeContainerTypeDeclaration)
                .Where(node => kinds.Contains(node.Kind()));

        public override IEnumerable<SyntaxNodeSymbolSemanticModelTuple> GetRemovableFieldLikeDeclarations(ISet<SyntaxKind> kinds, Accessibility maxAccessibility)
        {
            var fieldLikeNodes = TypeDeclarations
                .SelectMany(typeDeclaration => SelectMatchingDeclarations(typeDeclaration, kinds)
                    .Select(node =>
                        new SyntaxNodeAndSemanticModel<BaseFieldDeclarationSyntax>
                        {
                            SyntaxNode = (BaseFieldDeclarationSyntax)node,
                            SemanticModel = typeDeclaration.SemanticModel
                        }));

            return fieldLikeNodes
                .SelectMany(fieldLikeNode => fieldLikeNode.SyntaxNode.Declaration.Variables
                    .Select(variable => SelectNodeTuple(variable, fieldLikeNode.SemanticModel))
                    .Where(tuple => IsRemovable(tuple.Symbol, maxAccessibility)));
        }

        internal override BaseTypeDeclarationSyntax GetOwnerOfSubnodes(BaseTypeDeclarationSyntax node) =>
            node;

        public static bool IsNodeContainerTypeDeclaration(SyntaxNode node) =>
            IsNodeStructOrClassOrRecordDeclaration(node) || node.IsKind(SyntaxKind.InterfaceDeclaration);

        private static bool IsNodeStructOrClassOrRecordDeclaration(SyntaxNode node) =>
            node.IsAnyKind(SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKindEx.RecordDeclaration);
    }
}

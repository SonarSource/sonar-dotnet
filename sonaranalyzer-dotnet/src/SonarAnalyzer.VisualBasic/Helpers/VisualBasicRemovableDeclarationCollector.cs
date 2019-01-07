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
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.Helpers
{
    using SyntaxNodeSymbolSemanticModelTuple = SyntaxNodeSymbolSemanticModelTuple<SyntaxNode, ISymbol>;

    internal class VisualBasicRemovableDeclarationCollector :
        RemovableDeclarationCollectorBase<TypeBlockSyntax, TypeStatementSyntax, SyntaxKind>
    {
        public VisualBasicRemovableDeclarationCollector(INamedTypeSymbol namedType, Compilation compilation)
            : base(namedType, compilation)
        {
        }

        public static bool IsNodeStructOrClassDeclaration(SyntaxNode node) =>
            node.IsKind(SyntaxKind.ClassStatement) || node.IsKind(SyntaxKind.StructureStatement);

        public static bool IsNodeContainerTypeDeclaration(SyntaxNode node) =>
            IsNodeStructOrClassDeclaration(node) || node.IsKind(SyntaxKind.InterfaceStatement);

        protected override IEnumerable<SyntaxNode> SelectMatchingDeclarations(
            SyntaxNodeAndSemanticModel<TypeBlockSyntax> container, ISet<SyntaxKind> kinds) =>
            container.SyntaxNode.DescendantNodes(IsNodeContainerTypeDeclaration)
                .Where(node => kinds.Contains(node.Kind()));

        public override IEnumerable<SyntaxNodeSymbolSemanticModelTuple> GetRemovableFieldLikeDeclarations(
    ISet<SyntaxKind> kinds, Accessibility maxAcessibility)
        {
            var fieldLikeNodes = TypeDeclarations
                .SelectMany(typeDeclaration => SelectMatchingDeclarations(typeDeclaration, kinds)
                    .Select(node =>
                        new SyntaxNodeAndSemanticModel<FieldDeclarationSyntax>
                        {
                            SyntaxNode = (FieldDeclarationSyntax)node,
                            SemanticModel = typeDeclaration.SemanticModel
                        }));

            return fieldLikeNodes
                .SelectMany(fieldLikeNode => fieldLikeNode.SyntaxNode.Declarators
                    .Select(variable => SelectNodeTuple(variable, fieldLikeNode.SemanticModel))
                    .Where(tuple => IsRemovable(tuple.Symbol, maxAcessibility)));
        }

        internal override TypeBlockSyntax GetOwnerOfSubnodes(TypeStatementSyntax node) =>
            node.Parent as TypeBlockSyntax;
    }
}

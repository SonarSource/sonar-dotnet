﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using NodeSymbolAndModel = SonarAnalyzer.Common.NodeSymbolAndModel<Microsoft.CodeAnalysis.SyntaxNode, Microsoft.CodeAnalysis.ISymbol>;

namespace SonarAnalyzer.Helpers
{
    internal class VisualBasicRemovableDeclarationCollector : RemovableDeclarationCollectorBase<TypeBlockSyntax, TypeStatementSyntax, SyntaxKind>
    {
        public VisualBasicRemovableDeclarationCollector(INamedTypeSymbol namedType, Compilation compilation) : base(namedType, compilation) { }

        public static bool IsNodeStructOrClassDeclaration(SyntaxNode node) =>
            node.IsKind(SyntaxKind.ClassBlock) || node.IsKind(SyntaxKind.StructureBlock);

        public static bool IsNodeContainerTypeDeclaration(SyntaxNode node) =>
            IsNodeStructOrClassDeclaration(node) || node.IsKind(SyntaxKind.InterfaceBlock);

        protected override IEnumerable<SyntaxNode> SelectMatchingDeclarations(NodeAndModel<TypeBlockSyntax> container, ISet<SyntaxKind> kinds) =>
            container.Node.DescendantNodes(IsNodeContainerTypeDeclaration).Where(node => kinds.Contains(node.Kind()));

        public override IEnumerable<NodeSymbolAndModel> GetRemovableFieldLikeDeclarations(ISet<SyntaxKind> kinds, Accessibility maxAccessibility)
        {
            var fieldLikeNodes = TypeDeclarations
                .SelectMany(typeDeclaration => SelectMatchingDeclarations(typeDeclaration, kinds)
                    .Select(x => new NodeAndModel<FieldDeclarationSyntax>(typeDeclaration.Model, (FieldDeclarationSyntax)x)));

            return fieldLikeNodes
                .SelectMany(fieldLikeNode => fieldLikeNode.Node.Declarators.SelectMany(x => x.Names)
                    .Select(name => SelectNodeTuple(name, fieldLikeNode.Model))
                    .Where(x => IsRemovable(x.Symbol, maxAccessibility)));
        }

        internal override TypeBlockSyntax GetOwnerOfSubnodes(TypeStatementSyntax node) =>
            node.Parent as TypeBlockSyntax;
    }
}

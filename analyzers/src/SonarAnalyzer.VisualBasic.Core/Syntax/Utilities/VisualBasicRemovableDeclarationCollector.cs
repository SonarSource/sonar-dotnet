/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using NodeSymbolAndModel = SonarAnalyzer.Common.NodeSymbolAndModel<Microsoft.CodeAnalysis.SyntaxNode, Microsoft.CodeAnalysis.ISymbol>;

namespace SonarAnalyzer.VisualBasic.Core.Syntax.Utilities;

public class VisualBasicRemovableDeclarationCollector : RemovableDeclarationCollectorBase<TypeBlockSyntax, TypeStatementSyntax, SyntaxKind>
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

    public override TypeBlockSyntax GetOwnerOfSubnodes(TypeStatementSyntax node) =>
        node.Parent as TypeBlockSyntax;
}

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

namespace SonarAnalyzer.CSharp.Core.Syntax.Utilities;

public class CSharpRemovableDeclarationCollector : RemovableDeclarationCollectorBase<BaseTypeDeclarationSyntax, BaseTypeDeclarationSyntax, SyntaxKind>
{
    public CSharpRemovableDeclarationCollector(INamedTypeSymbol namedType, Compilation compilation) : base(namedType, compilation) { }

    protected override IEnumerable<SyntaxNode> SelectMatchingDeclarations(NodeAndModel<BaseTypeDeclarationSyntax> container, ISet<SyntaxKind> kinds) =>
        container.Node.DescendantNodes(IsNodeContainerTypeDeclaration).Where(node => kinds.Contains(node.Kind()));

    public override IEnumerable<NodeSymbolAndModel<SyntaxNode, ISymbol>> GetRemovableFieldLikeDeclarations(ISet<SyntaxKind> kinds, Accessibility maxAccessibility)
    {
        var fieldLikeNodes = TypeDeclarations
            .SelectMany(typeDeclaration => SelectMatchingDeclarations(typeDeclaration, kinds)
                .Select(node => new NodeAndModel<BaseFieldDeclarationSyntax>(typeDeclaration.Model, (BaseFieldDeclarationSyntax)node)));

        return fieldLikeNodes
            .SelectMany(fieldLikeNode => fieldLikeNode.Node.Declaration.Variables
                .Select(variable => SelectNodeTuple(variable, fieldLikeNode.Model))
                .Where(tuple => IsRemovable(tuple.Symbol, maxAccessibility)));
    }

    public override BaseTypeDeclarationSyntax GetOwnerOfSubnodes(BaseTypeDeclarationSyntax node) =>
        node;

    public static bool IsNodeContainerTypeDeclaration(SyntaxNode node) =>
        IsNodeStructOrClassOrRecordDeclaration(node) || node.IsKind(SyntaxKind.InterfaceDeclaration);

    private static bool IsNodeStructOrClassOrRecordDeclaration(SyntaxNode node) =>
        node?.Kind() is SyntaxKind.ClassDeclaration or SyntaxKind.StructDeclaration or SyntaxKindEx.RecordDeclaration or SyntaxKindEx.RecordStructDeclaration;
}

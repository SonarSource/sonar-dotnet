/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

    public override IEnumerable<NodeSymbolAndModel<SyntaxNode, ISymbol>> RemovableFieldLikeDeclarations(ISet<SyntaxKind> kinds, Accessibility maxAccessibility)
    {
        var fieldLikeNodes = TypeDeclarations.SelectMany(x => MatchingDeclarations(x, kinds).Select(node => new NodeAndModel<BaseFieldDeclarationSyntax>((BaseFieldDeclarationSyntax)node, x.Model)));
        return fieldLikeNodes.SelectMany(x => x.Node.Declaration.Variables.Select(variable => CreateNodeSymbolAndModel(variable, x.Model)).Where(tuple => IsRemovable(tuple.Symbol, maxAccessibility)));
    }

    public override BaseTypeDeclarationSyntax OwnerOfSubnodes(BaseTypeDeclarationSyntax node) =>
        node;

    public static bool IsNodeContainerTypeDeclaration(SyntaxNode node) =>
        IsNodeStructOrClassOrRecordDeclaration(node) || node.IsKind(SyntaxKind.InterfaceDeclaration);

    protected override IEnumerable<SyntaxNode> MatchingDeclarations(NodeAndModel<BaseTypeDeclarationSyntax> container, ISet<SyntaxKind> kinds) =>
        container.Node.DescendantNodes(IsNodeContainerTypeDeclaration).Where(x => kinds.Contains(x.Kind()));

    private static bool IsNodeStructOrClassOrRecordDeclaration(SyntaxNode node) =>
        node?.Kind() is SyntaxKind.ClassDeclaration or SyntaxKind.StructDeclaration or SyntaxKindEx.RecordDeclaration or SyntaxKindEx.RecordStructDeclaration;
}

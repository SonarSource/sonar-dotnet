/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Core.Extensions;

public static class ISymbolExtensions
{
    private static readonly SyntaxKind[] DeclarationsTypesWithPrimaryConstructor =
    {
        SyntaxKind.ClassDeclaration,
        SyntaxKind.StructDeclaration,
        SyntaxKindEx.RecordDeclaration,
        SyntaxKindEx.RecordStructDeclaration
    };

    extension(ISymbol symbol)
    {
        public SyntaxToken? FirstDeclaringReferenceIdentifier =>
            symbol.DeclaringReferenceIdentifiers.FirstOrDefault();

        public ImmutableArray<SyntaxToken> DeclaringReferenceIdentifiers =>
            symbol.DeclaringSyntaxReferences
               .Select(x => x.GetSyntax().GetIdentifier())
               .WhereNotNull()
               .ToImmutableArray();

        public bool IsPrimaryConstructor =>
            symbol.IsConstructor()
            && symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is { } syntax
            && DeclarationsTypesWithPrimaryConstructor.Contains(syntax.Kind());

        public IEnumerable<SyntaxNode> LocationNodes(SyntaxNode node) =>
            symbol.Locations.SelectMany(location => GetDescendantNodes(location, node));
    }

    private static IEnumerable<SyntaxNode> GetDescendantNodes(Location location, SyntaxNode invocation)
    {
        var locationRootNode = location.SourceTree?.GetRoot();
        var invocationRootNode = invocation.SyntaxTree.GetRoot();

        // We don't look for descendants when the location is outside the current context root
        if (locationRootNode != null && locationRootNode != invocationRootNode)
        {
            return Enumerable.Empty<SyntaxNode>();
        }

        // To optimise, we search first for the class constructor, then for the method declaration.
        // If these cannot be found (e.g. fields), we get the root of the syntax tree and search from there.
        var root = locationRootNode?.FindNode(location.SourceSpan)
                   ?? invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>()
                   ?? invocationRootNode;

        return root.DescendantNodes();
    }
}

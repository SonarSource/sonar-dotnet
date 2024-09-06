/*
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

    public static IEnumerable<SyntaxNode> GetLocationNodes(this ISymbol symbol, SyntaxNode node) =>
        symbol.Locations.SelectMany(location => GetDescendantNodes(location, node));

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

    public static SyntaxToken? FirstDeclaringReferenceIdentifier(this ISymbol symbol) =>
        symbol.DeclaringReferenceIdentifiers().FirstOrDefault();

    public static ImmutableArray<SyntaxToken> DeclaringReferenceIdentifiers(this ISymbol symbol) =>
        symbol.DeclaringSyntaxReferences
           .Select(x => x.GetSyntax().GetIdentifier())
           .WhereNotNull()
           .ToImmutableArray();

    public static bool IsPrimaryConstructor(this ISymbol symbol) =>
        symbol.IsConstructor()
        && symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is { } syntax
        && DeclarationsTypesWithPrimaryConstructor.Contains(syntax.Kind());
}

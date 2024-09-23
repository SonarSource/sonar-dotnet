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

using NodeSymbolAndModel = SonarAnalyzer.Common.NodeSymbolAndModel<Microsoft.CodeAnalysis.SyntaxNode, Microsoft.CodeAnalysis.ISymbol>;

namespace SonarAnalyzer.Helpers;

// For C#, TOwnerOfSubnodes == TDeclaration == BaseTypeDeclarationSyntax
// For VB, TOwnerOfSubnodes == TypeBlockSyntax, TDeclaration = TypeStatementSyntax
public abstract class RemovableDeclarationCollectorBase<TOwnerOfSubnodes, TDeclaration, TSyntaxKind>
    where TOwnerOfSubnodes : SyntaxNode
    where TDeclaration : SyntaxNode
{
    private readonly Compilation compilation;
    private readonly INamedTypeSymbol namedType;

    private IEnumerable<NodeAndModel<TOwnerOfSubnodes>> typeDeclarations;

    public abstract IEnumerable<NodeSymbolAndModel> GetRemovableFieldLikeDeclarations(ISet<TSyntaxKind> kinds, Accessibility maxAccessibility);
    public abstract TOwnerOfSubnodes GetOwnerOfSubnodes(TDeclaration node);
    protected abstract IEnumerable<SyntaxNode> SelectMatchingDeclarations(NodeAndModel<TOwnerOfSubnodes> container, ISet<TSyntaxKind> kinds);

    public IEnumerable<NodeAndModel<TOwnerOfSubnodes>> TypeDeclarations
    {
        get
        {
            if (typeDeclarations == null)
            {
                typeDeclarations = namedType.DeclaringSyntaxReferences
                    .Select(x => x.GetSyntax())
                    .OfType<TDeclaration>()
                    .Select(x => new NodeAndModel<TOwnerOfSubnodes>(compilation.GetSemanticModel(x.SyntaxTree), GetOwnerOfSubnodes(x)))
                    .Where(x => x.Model != null);
            }
            return typeDeclarations;
        }
    }

    protected RemovableDeclarationCollectorBase(INamedTypeSymbol namedType, Compilation compilation)
    {
        this.namedType = namedType;
        this.compilation = compilation;
    }

    public IEnumerable<NodeSymbolAndModel> GetRemovableDeclarations(ISet<TSyntaxKind> kinds, Accessibility maxAccessibility) =>
        TypeDeclarations.SelectMany(container => SelectMatchingDeclarations(container, kinds)
                                    .Select(x => SelectNodeTuple(x, container.Model)))
                                    .Where(x => IsRemovable(x.Symbol, maxAccessibility));

    public static bool IsRemovable(IMethodSymbol methodSymbol, Accessibility maxAccessibility) =>
        IsRemovable((ISymbol)methodSymbol, maxAccessibility)
        && (methodSymbol.MethodKind == MethodKind.Ordinary || methodSymbol.MethodKind == MethodKind.Constructor)
        && !methodSymbol.IsMainMethod()
        && !methodSymbol.IsEventHandler()
        && !methodSymbol.IsSerializationConstructor();

    protected static bool IsRemovable(ISymbol symbol, Accessibility maxAccessibility) =>
        symbol != null
        && symbol.GetEffectiveAccessibility() <= maxAccessibility
        && !symbol.IsImplicitlyDeclared
        && !symbol.IsAbstract
        && !symbol.IsVirtual
        && !symbol.GetAttributes().Any()
        && !symbol.ContainingType.IsInterface()
        && symbol.GetInterfaceMember() == null
        && symbol.GetOverriddenMember() == null;

    protected static NodeSymbolAndModel SelectNodeTuple(SyntaxNode node, SemanticModel semanticModel) =>
        new(semanticModel, node, semanticModel.GetDeclaredSymbol(node));
}

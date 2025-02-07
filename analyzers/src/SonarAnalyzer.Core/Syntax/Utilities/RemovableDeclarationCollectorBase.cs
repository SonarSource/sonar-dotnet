/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using NodeSymbolAndModel = SonarAnalyzer.Core.Common.NodeSymbolAndModel<Microsoft.CodeAnalysis.SyntaxNode, Microsoft.CodeAnalysis.ISymbol>;

namespace SonarAnalyzer.Core.Syntax.Utilities;

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

    public IEnumerable<NodeAndModel<TOwnerOfSubnodes>> TypeDeclarations =>
        typeDeclarations ??= namedType.DeclaringSyntaxReferences
            .Select(x => x.GetSyntax())
            .OfType<TDeclaration>()
            .Select(x => new NodeAndModel<TOwnerOfSubnodes>(compilation.GetSemanticModel(x.SyntaxTree), GetOwnerOfSubnodes(x)))
            .Where(x => x.Model is not null);

    protected RemovableDeclarationCollectorBase(INamedTypeSymbol namedType, Compilation compilation)
    {
        this.namedType = namedType;
        this.compilation = compilation;
    }

    public IEnumerable<NodeSymbolAndModel> GetRemovableDeclarations(ISet<TSyntaxKind> kinds, Accessibility maxAccessibility) =>
        TypeDeclarations
            .SelectMany(x => SelectMatchingDeclarations(x, kinds).Select(declaration => SelectNodeTuple(declaration, x.Model)))
            .Where(x => IsRemovable(x.Symbol, maxAccessibility));

    public static bool IsRemovable(IMethodSymbol methodSymbol, Accessibility maxAccessibility) =>
        IsRemovable((ISymbol)methodSymbol, maxAccessibility)
        && (methodSymbol.MethodKind == MethodKind.Ordinary || methodSymbol.MethodKind == MethodKind.Constructor)
        && !methodSymbol.IsMainMethod()
        && !methodSymbol.IsEventHandler()
        && !methodSymbol.IsSerializationConstructor();

    protected static bool IsRemovable(ISymbol symbol, Accessibility maxAccessibility) =>
        symbol is not null
        && symbol.GetEffectiveAccessibility() <= maxAccessibility
        && !symbol.IsImplicitlyDeclared
        && !symbol.IsAbstract
        && !symbol.IsVirtual
        && !symbol.GetAttributes().Any()
        && !symbol.ContainingType.IsInterface()
        && symbol.GetInterfaceMember() is null
        && symbol.GetOverriddenMember() is null;

    protected static NodeSymbolAndModel SelectNodeTuple(SyntaxNode node, SemanticModel model) =>
        new(model, node, model.GetDeclaredSymbol(node));
}

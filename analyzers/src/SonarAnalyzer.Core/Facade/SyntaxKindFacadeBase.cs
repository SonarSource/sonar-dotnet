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

using System.Collections.Concurrent;

namespace SonarAnalyzer.Core.Facade;

public abstract class SyntaxKindFacadeBase<TSyntaxKind> where TSyntaxKind : struct
{
    private readonly ConcurrentDictionary<TypeDeclarationKinds, TSyntaxKind[]> typeDeclarations = new();
    private readonly Func<TypeDeclarationKinds, TSyntaxKind[]> expand;

    /// <summary>SyntaxKinds of a single, non-composite type declaration kind. Empty if the language does not have it, several if the language splits it (for example VB delegates).</summary>
    protected abstract TSyntaxKind[] MapTypeDeclaration(TypeDeclarationKinds kind);

    protected abstract TSyntaxKind Kind(SyntaxNode node);

    public TSyntaxKind[] ClassAndRecordDeclarations => TypeDeclarations(TypeDeclarationKinds.Class | TypeDeclarationKinds.Record);
    public TSyntaxKind[] ClassAndModuleDeclarations => TypeDeclarations(TypeDeclarationKinds.Class | TypeDeclarationKinds.Module);
    public TSyntaxKind[] TypeDeclaration => TypeDeclarations(TypeDeclarationKinds.ClassOrStructLike | TypeDeclarationKinds.Interface | TypeDeclarationKinds.Enum);

    protected SyntaxKindFacadeBase() =>
        expand = Expand;

    /// <summary>Cached SyntaxKinds of the selected type declarations. The shared array must not be modified.</summary>
    public TSyntaxKind[] TypeDeclarations(TypeDeclarationKinds kinds) =>
        kinds == TypeDeclarationKinds.None || (kinds & ~TypeDeclarationKinds.All) != 0
            ? throw new ArgumentException("At least one defined kind is required.", nameof(kinds))
            : typeDeclarations.GetOrAdd(kinds, expand);

    public bool IsTypeDeclaration(SyntaxNode node, TypeDeclarationKinds kinds) =>
        node is not null && Array.IndexOf(TypeDeclarations(kinds), Kind(node)) >= 0;

    private TSyntaxKind[] Expand(TypeDeclarationKinds kinds) =>
        Enum.GetValues(typeof(TypeDeclarationKinds)).Cast<TypeDeclarationKinds>()
            .Where(x => IsSingleKind(x) && kinds.HasFlag(x))
            .SelectMany(MapTypeDeclaration)
            .ToArray();

    // Excludes None and the composite values (a power of two has exactly one bit set)
    private static bool IsSingleKind(TypeDeclarationKinds kind) =>
        kind != TypeDeclarationKinds.None && (kind & (kind - 1)) == 0;
}

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

namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;

public static class TypeSyntaxExtensions
{
    /// <summary>
    /// Recursively unwraps array, pointer, nullable, ref, and scoped type modifiers,
    /// returning the innermost base <see cref="TypeSyntax"/>.
    /// Types that are returned as-is include <see cref="IdentifierNameSyntax"/>,
    /// <see cref="QualifiedNameSyntax"/>, <see cref="AliasQualifiedNameSyntax"/>,
    /// <see cref="GenericNameSyntax"/>, and <see cref="PredefinedTypeSyntax"/>.
    /// </summary>
    public static TypeSyntax Unwrap(this TypeSyntax type) =>
        type switch
        {
            ArrayTypeSyntax x => x.ElementType.Unwrap(),
            NullableTypeSyntax x => x.ElementType.Unwrap(),
            PointerTypeSyntax x => x.ElementType.Unwrap(),
            { RawKind: (int)SyntaxKindEx.RefType } x => ((RefTypeSyntaxWrapper)x).Type.Unwrap(),
            { RawKind: (int)SyntaxKindEx.ScopedType } x => ((ScopedTypeSyntaxWrapper)x).Type.Unwrap(),
            _ => type
        };
}

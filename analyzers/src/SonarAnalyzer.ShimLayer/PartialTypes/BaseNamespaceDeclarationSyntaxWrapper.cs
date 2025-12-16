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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.ShimLayer;

public partial struct BaseNamespaceDeclarationSyntaxWrapper
{
    public const string FallbackWrappedTypeName = "Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax";

    private static Func<MemberDeclarationSyntax, SyntaxList<UsingDirectiveSyntax>, MemberDeclarationSyntax> withUsingsAccessor;

    public BaseNamespaceDeclarationSyntaxWrapper WithUsings(SyntaxList<UsingDirectiveSyntax> usings)    // This should be removed once we Shim methods
    {
        withUsingsAccessor ??= LightupHelpers.CreateSyntaxWithPropertyAccessor<MemberDeclarationSyntax, SyntaxList<UsingDirectiveSyntax>>(WrappedType, nameof(Usings));
        return new BaseNamespaceDeclarationSyntaxWrapper(withUsingsAccessor(Node, usings));
    }

    public static implicit operator BaseNamespaceDeclarationSyntaxWrapper(NamespaceDeclarationSyntax node) =>
        new(node);
}

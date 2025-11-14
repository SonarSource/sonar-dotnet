/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StyleCop.Analyzers.Lightup;

internal static partial class TypeDeclarationSyntaxExtensions
{
    private static readonly Func<TypeDeclarationSyntax, ParameterListSyntax> RecordDeclarationParameterListAccessor;

    // In earlier versions, the ParameterList was only available on the derived RecordDeclarationSyntax (starting from version 3.7)
    // To work with version 3.7 to version 4.6 we need to special case the record declaration and access
    // the parameter list from the derived RecordDeclarationSyntax.
    public static ParameterListSyntax ParameterList(this TypeDeclarationSyntax syntax) =>
        syntax.Kind() is SyntaxKindEx.RecordDeclaration or SyntaxKindEx.RecordStructDeclaration
            ? RecordDeclarationParameterListAccessor(syntax)
            : ParameterListAccessor(syntax);
}

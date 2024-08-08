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

﻿/*
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

public static class ITypeSymbolExtensions
{
    public static bool IsDisposableRefStruct(this ITypeSymbol symbol, LanguageVersion languageVersion) =>
        languageVersion.IsAtLeast(LanguageVersionEx.CSharp8)
        && IsRefStruct(symbol)
        && symbol.GetMembers(nameof(IDisposable.Dispose)).Any(x => x.DeclaredAccessibility == Accessibility.Public && KnownMethods.IsIDisposableDispose(x as IMethodSymbol));

    public static bool IsRefStruct(this ITypeSymbol symbol) =>
        symbol != null
        && symbol.IsStruct()
        && symbol.DeclaringSyntaxReferences.Length > 0
        && symbol.DeclaringSyntaxReferences[0].GetSyntax() is StructDeclarationSyntax structDeclaration
        && structDeclaration.Modifiers.Any(SyntaxKind.RefKeyword);
}
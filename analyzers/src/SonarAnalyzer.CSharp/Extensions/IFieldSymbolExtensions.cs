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

namespace SonarAnalyzer.Extensions
{
    internal static class IFieldSymbolExtensions
    {
        internal static bool IsNonStaticNonPublicDisposableField(this IFieldSymbol fieldSymbol, LanguageVersion languageVersion) =>
            fieldSymbol != null
            && !fieldSymbol.IsStatic
            && (fieldSymbol.DeclaredAccessibility == Accessibility.Protected || fieldSymbol.DeclaredAccessibility == Accessibility.Private)
            && IsDisposable(fieldSymbol, languageVersion);

        private static bool IsDisposable(this IFieldSymbol fieldSymbol, LanguageVersion languageVersion) =>
            fieldSymbol.Type.Is(KnownType.System_IDisposable)
            || fieldSymbol.Type.Implements(KnownType.System_IDisposable)
            || fieldSymbol.Type.Is(KnownType.System_IAsyncDisposable)
            || fieldSymbol.Type.Implements(KnownType.System_IAsyncDisposable)
            || fieldSymbol.Type.IsDisposableRefStruct(languageVersion);
    }
}

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

namespace SonarAnalyzer.CSharp.Core.Extensions;

public static class IFieldSymbolExtensions
{
    public static bool IsNonStaticNonPublicDisposableField(this IFieldSymbol fieldSymbol, LanguageVersion languageVersion) =>
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

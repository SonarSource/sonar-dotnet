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

namespace SonarAnalyzer.CSharp.Core.Extensions;

public static class ITypeSymbolExtensions
{
    extension(ITypeSymbol symbol)
    {
        public bool IsRefStruct =>
            symbol is not null
            && symbol.IsStruct()
            && symbol.IsRefLikeType();

        public bool IsDisposableRefStruct(LanguageVersion languageVersion) =>
            languageVersion.IsAtLeast(LanguageVersionEx.CSharp8)
            && symbol.IsRefStruct
            && symbol.GetMembers(nameof(IDisposable.Dispose)).Any(x => x.DeclaredAccessibility == Accessibility.Public && KnownMethods.IsIDisposableDispose(x as IMethodSymbol));
    }
}

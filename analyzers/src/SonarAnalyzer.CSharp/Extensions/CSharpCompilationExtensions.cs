/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Extensions
{
    internal static class CSharpCompilationExtensions
    {
        internal static bool IsCoalesceAssignmentSupported(this Compilation compilation) =>
            compilation.IsAtLeastLanguageVersion(LanguageVersionEx.CSharp8);

        internal static bool IsTargetTypeConditionalSupported(this Compilation compilation) =>
            compilation.IsAtLeastLanguageVersion(LanguageVersionEx.CSharp9);

        internal static bool IsAtLeastLanguageVersion(this Compilation compilation, LanguageVersion languageVersion) =>
            compilation.GetLanguageVersion().IsAtLeast(languageVersion);

        internal static LanguageVersion GetLanguageVersion(this Compilation compilation) => ((CSharpCompilation)compilation).LanguageVersion;
    }
}

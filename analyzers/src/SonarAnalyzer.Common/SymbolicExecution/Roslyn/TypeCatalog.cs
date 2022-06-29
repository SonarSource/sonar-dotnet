/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.SymbolicExecution.Roslyn;

internal class TypeCatalog
{
    private readonly Compilation compilation;
    private readonly Dictionary<string, INamedTypeSymbol> cache = new();

    public INamedTypeSymbol SystemIndexOutOfRangeException => Get("System.IndexOutOfRangeException");
    public INamedTypeSymbol SystemNullReferenceException => Get("System.NullReferenceException");
    public INamedTypeSymbol SystemInvalidCastException => Get("System.InvalidCastException");

    public TypeCatalog(Compilation compilation)
    {
        this.compilation = compilation;
    }

    private INamedTypeSymbol Get(string typeName) =>
        cache.GetOrAdd(typeName, name => compilation.GetTypeByMetadataName(name));
}

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

namespace SonarAnalyzer.SymbolicExecution.Roslyn;

internal class TypeCatalog
{
    public INamedTypeSymbol SystemIndexOutOfRangeException { get; }
    public INamedTypeSymbol SystemNullReferenceException { get; }
    public INamedTypeSymbol SystemInvalidCastException { get; }
    public INamedTypeSymbol SystemArgumentOutOfRangeException { get; }
    public INamedTypeSymbol SystemDivideByZeroException { get; }

    public TypeCatalog(Compilation compilation)
    {
        SystemIndexOutOfRangeException = compilation.GetTypeByMetadataName("System.IndexOutOfRangeException");
        SystemNullReferenceException = compilation.GetTypeByMetadataName("System.NullReferenceException");
        SystemInvalidCastException = compilation.GetTypeByMetadataName("System.InvalidCastException");
        SystemArgumentOutOfRangeException = compilation.GetTypeByMetadataName("System.ArgumentOutOfRangeException");
        SystemDivideByZeroException = compilation.GetTypeByMetadataName("System.DivideByZeroException");
    }
}

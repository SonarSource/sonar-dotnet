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

using System.ComponentModel;

namespace System.Runtime.CompilerServices;

// This empty class needs to exist when C# 9 init-only setters are used in project targeting .NET Framework.
// https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isexternalinit?view=net-6.0
// It is used only by compiler to track metadata. It does not affect MSIL, CLR nor runtime.
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/init#metadata-encoding
[EditorBrowsable(EditorBrowsableState.Never)]
public static class IsExternalInit { }

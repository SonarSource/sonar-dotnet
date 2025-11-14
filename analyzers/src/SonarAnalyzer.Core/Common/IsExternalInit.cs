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

using System.ComponentModel;

namespace System.Runtime.CompilerServices;

// This empty class needs to exist when C# 9 init-only setters are used in project targeting .NET Framework.
// https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isexternalinit?view=net-6.0
// It is used only by compiler to track metadata. It does not affect MSIL, CLR nor runtime.
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/init#metadata-encoding
[EditorBrowsable(EditorBrowsableState.Never)]
public static class IsExternalInit { }

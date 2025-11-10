/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Semantics;

public enum NetFrameworkVersion
{
    // cannot tell
    Unknown,
    // probably .NET 3.5
    Probably35,
    // between .NET 4.0 (inclusive) and .NET 4.5.1 (inclusive)
    Between4And451,
    // after .NET 4.5.2 (inclusive)
    After452
}

/// <summary>
/// This class provides an approximation of the .NET Framework version of the Compilation.
/// </summary>
/// <remarks>
/// This class has been added for the requirements of the S2755 C# implementation, so it is quite limited.
/// </remarks>
public class NetFrameworkVersionProvider
{
    public virtual NetFrameworkVersion Version(Compilation compilation)
    {
        if (compilation is null)
        {
            return NetFrameworkVersion.Unknown;
        }

        /// See https://docs.microsoft.com/en-us/previous-versions/dotnet/netframework-4.0/ee471421(v=vs.100)
        var debuggerSymbol = compilation.GetTypeByMetadataName(KnownType.System_Diagnostics_Debugger);
        var mscorlibAssembly = debuggerSymbol?.ContainingAssembly;
        if (mscorlibAssembly is null || !mscorlibAssembly.Identity.Name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase))
        {
            return NetFrameworkVersion.Unknown;     // it could be .NET Core or .NET Standard
        }

        var debuggerConstructorSymbol = debuggerSymbol.GetMembers(".ctor").FirstOrDefault();
        if (debuggerConstructorSymbol is null)
        {
            return NetFrameworkVersion.Unknown;     // e.g. .NET Standard or maybe another .NET distribution
        }
        else if (!debuggerConstructorSymbol.GetAttributes().Any(x => x.AttributeClass.Name.Equals("ObsoleteAttribute")))
        {
            return NetFrameworkVersion.Probably35;  // the constructor was still not deprecated in .NET Framework 3.5
        }
        else if (mscorlibAssembly.GetTypeByMetadataName("System.IO.UnmanagedMemoryStream") is { } typeSymbol && !typeSymbol.GetMembers("FlushAsync").IsEmpty)
        {
            return NetFrameworkVersion.After452;    // FlushAsync was not present in .NET Framework 4.5.1 and became present in 4.5.2
        }
        else
        {
            return NetFrameworkVersion.Between4And451;
        }
    }
}

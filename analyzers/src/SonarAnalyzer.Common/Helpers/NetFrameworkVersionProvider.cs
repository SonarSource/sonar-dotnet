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

namespace SonarAnalyzer.Helpers
{
    /// <summary>
    /// This class provides an approximation of the .NET Framework version of the Compilation.
    /// </summary>
    /// <remarks>
    /// This class has been added for the requirements of the S2755 C# implementation, so it is quite limited.
    /// </remarks>
    public class NetFrameworkVersionProvider : INetFrameworkVersionProvider
    {
        public NetFrameworkVersion GetDotNetFrameworkVersion(Compilation compilation)
        {
            if (compilation == null)
            {
                return NetFrameworkVersion.Unknown;
            }

            /// See https://docs.microsoft.com/en-us/previous-versions/dotnet/netframework-4.0/ee471421(v=vs.100)
            var debuggerSymbol = compilation.GetTypeByMetadataName(KnownType.System_Diagnostics_Debugger);

            var mscorlibAssembly = debuggerSymbol?.ContainingAssembly;
            if (mscorlibAssembly == null ||
                !mscorlibAssembly.Identity.Name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase))
            {
                // it could be .NET Core or .NET Standard
                return NetFrameworkVersion.Unknown;
            }

            var debuggerConstructorSymbol = debuggerSymbol.GetMembers(".ctor").FirstOrDefault();
            if (debuggerConstructorSymbol == null)
            {
                // e.g. .NET Standard or maybe another .NET distribution
                return NetFrameworkVersion.Unknown;
            }

            if (!debuggerConstructorSymbol.GetAttributes().Any(attribute => attribute.AttributeClass.Name.Equals("ObsoleteAttribute")))
            {
                // the constructor was still not deprecated in .NET Framework 3.5
                return NetFrameworkVersion.Probably35;
            }

            var typeSymbol = mscorlibAssembly.GetTypeByMetadataName("System.IO.UnmanagedMemoryStream");
            // this was not present in .NET Framework 4.5.1 and became present in 4.5.2
            if (typeSymbol != null && !typeSymbol.GetMembers("FlushAsync").IsEmpty)
            {
                return NetFrameworkVersion.After452;
            }

            return NetFrameworkVersion.Between4And451;
        }
    }
}

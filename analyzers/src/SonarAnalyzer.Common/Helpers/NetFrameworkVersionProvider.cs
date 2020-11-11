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

using System;
using System.Linq;
using Microsoft.CodeAnalysis;

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
            var debuggerSymbol = compilation.GetTypeByMetadataName("System.Diagnostics.Debugger");

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

            if (Probably35(debuggerConstructorSymbol))
            {
                return NetFrameworkVersion.Probably35;
            }
            else if (AtLeast46(mscorlibAssembly))
            {
                return NetFrameworkVersion.After46;
            }
            else if (AtLeast452(mscorlibAssembly))
            {
                return NetFrameworkVersion.After452;
            }
            else
            {
                return NetFrameworkVersion.Between4And451;
            }
        }

        /// <remarks>the constructor was still not deprecated in .NET Framework 3.5</remarks>
        private static bool Probably35(ISymbol debuggerConstructorSymbol)
            => !debuggerConstructorSymbol
            .GetAttributes()
            .Any(attribute => attribute.AttributeClass.Name.Equals("ObsoleteAttribute"));

        /// <summary>Returns true if the .NET version was at least 4.5.2.</summary>
        /// <remarks>
        /// Detected by checking the existence of System.IO.UnmanagedMemoryStream.FlushAsync
        /// introduced in .NET 4.5.2
        /// </remarks>
        private static bool AtLeast452(IAssemblySymbol mscorlibAssembly)
            => mscorlibAssembly
            .GetTypeByMetadataName("System.IO.UnmanagedMemoryStream")?
            .GetMembers("FlushAsync")
            .Any() == true;

        /// <summary>Returns true if the .NET version was at least 4.6.</summary>
        /// <remarks>
        /// Detected by checking the existence of System.Array.Empty&lt;T&gt;,
        /// introduced in .NET 4.6
        /// </remarks>
        private static bool AtLeast46(IAssemblySymbol mscorlibAssembly)
            => mscorlibAssembly
            .GetTypeByMetadataName("System.Array")?
            .GetMembers("Empty")
            .Any() == true;
    }
}

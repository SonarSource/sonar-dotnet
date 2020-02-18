/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

            var containingAssemblyName = debuggerSymbol?.ContainingAssembly.Identity.Name;
            if (containingAssemblyName == null ||
                !containingAssemblyName.Equals("mscorlib", StringComparison.OrdinalIgnoreCase))
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
                return NetFrameworkVersion.Below4;
            }

            // See https://docs.microsoft.com/en-us/dotnet/framework/whats-new/obsolete-types#mscorlib
            // ContractHelper was not obsolete in 4.0 but became obsolete in 4.5
            var contractHelperSymbol = compilation.GetTypeByMetadataName("System.Diagnostics.Contracts.Internal.ContractHelper");
            if (contractHelperSymbol.GetAttributes().Any(attribute => attribute.AttributeClass.Name.Equals("ObsoleteAttribute")))
            {
                return NetFrameworkVersion.After45;
            }

            return NetFrameworkVersion.Between4And45;
        }
    }
}

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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace SonarAnalyzer.Helpers
{
    public class NetFrameworkVersionProvider : INetFrameworkVersionProvider
    {
        public static NetFrameworkVersionProvider Instance = new NetFrameworkVersionProvider();

        // The following is based on https://docs.microsoft.com/en-us/dotnet/framework/whats-new/obsolete-types#xml
        public NetFrameworkVersion GetDotNetFrameworkVersion(Compilation compilation)
        {
            if (compilation == null)
            {
                return NetFrameworkVersion.Unknown;
            }
            var xmlTextReaderSymbol = compilation.GetTypeByMetadataName("System.Xml.XmlTextReader");
            var containingAssemblyName = xmlTextReaderSymbol?.ContainingAssembly.Identity.Name;
            if (containingAssemblyName == null || !containingAssemblyName.Equals("System.Xml"))
            {
                // it is not .NET Framework; maybe it is .NET Standard or .NET Core
                return NetFrameworkVersion.Unknown;
            }

            if (xmlTextReaderSymbol.GetMembers("DtdProcessing").IsEmpty)
            {
                // DtdProcessing has been introduced in .NET Framework 4
                return NetFrameworkVersion.Below4;
            }

            // https://docs.microsoft.com/en-us/dotnet/framework/whats-new/obsolete-types#mscorlib
            // System.Diagnostics.Contracts.Internal.ContractHelper was not obsolete in 4.0 but became obsolete in 4.5
            var contractHelperSymbol = compilation.GetTypeByMetadataName("System.Diagnostics.Contracts.Internal.ContractHelper");
            if (contractHelperSymbol.GetAttributes().Any(attribute => attribute.AttributeClass.Name.Equals("ObsoleteAttribute")))
            {
                return NetFrameworkVersion.After45;
            }

            return NetFrameworkVersion.Between4And45;
        }
    }
}

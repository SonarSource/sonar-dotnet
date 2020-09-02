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

#if NETCOREAPP

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

using static SonarAnalyzer.UnitTest.MetadataReferences.MetadataReferenceFactory;

namespace SonarAnalyzer.UnitTest.MetadataReferences
{
    internal static class AspNetCoreMetadataReference
    {
        internal static IEnumerable<MetadataReference> MicrosoftAspNetCoreDiagnostics { get; } = Create(typeof(Microsoft.AspNetCore.Diagnostics.StatusCodeContext));
        internal static IEnumerable<MetadataReference> MicrosoftAspNetCoreHostingAbstractions { get; } = Create(typeof(Microsoft.AspNetCore.Hosting.IWebHost));
        internal static IEnumerable<MetadataReference> MicrosoftAspNetCoreHttpAbstractions { get; } = Create(typeof(Microsoft.AspNetCore.Http.IHttpContextFactory));
        internal static IEnumerable<MetadataReference> MicrosoftExtensionsHostingAbstractions { get; } = Create(typeof(Microsoft.Extensions.Hosting.IHost));

    }
}
#endif

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

#if NET

using Microsoft.CodeAnalysis;
using static SonarAnalyzer.UnitTest.MetadataReferences.MetadataReferenceFactory;

namespace SonarAnalyzer.UnitTest.MetadataReferences
{
    internal static class AspNetCoreMetadataReference
    {
        internal static References MicrosoftAspNetCoreDiagnostics { get; } = Create(typeof(Microsoft.AspNetCore.Diagnostics.StatusCodeContext));
        internal static References MicrosoftAspNetCoreHostingAbstractions { get; } = Create(typeof(Microsoft.AspNetCore.Hosting.IWebHost));
        internal static References MicrosoftAspNetCoreEventSourceLoggerFactoryExtensions { get; } = Create(typeof(Microsoft.Extensions.Logging.EventSourceLoggerFactoryExtensions));
        internal static References MicrosoftAspNetCoreHostingWebHostBuilderExtensions { get; } = Create(typeof(Microsoft.AspNetCore.Hosting.WebHostBuilderExtensions));
        internal static References MicrosoftAspNetCoreHttpAbstractions { get; } = Create(typeof(Microsoft.AspNetCore.Http.IHttpContextFactory));
        internal static References MicrosoftExtensionsHostingAbstractions { get; } = Create(typeof(Microsoft.Extensions.Hosting.IHost));
        internal static References MicrosoftAspNetCoreWebHost { get; } = Create(typeof(Microsoft.AspNetCore.WebHost));
    }
}
#endif

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

#if NET

using static SonarAnalyzer.TestFramework.MetadataReferences.MetadataReferenceFactory;

namespace SonarAnalyzer.TestFramework.MetadataReferences;

public static class AspNetCoreMetadataReference
{
    public static MetadataReference MicrosoftAspNetCore { get; } = CreateReference("Microsoft.AspNetCore.dll", Sdk.AspNetCore);
    public static MetadataReference MicrosoftAspNetCoreCors { get; } = CreateReference("Microsoft.AspNetCore.Cors.dll", Sdk.AspNetCore);
    public static MetadataReference MicrosoftAspNetCoreDiagnostics { get; } = CreateReference("Microsoft.AspNetCore.Diagnostics.dll", Sdk.AspNetCore);
    public static MetadataReference MicrosoftAspNetCoreHosting { get; } = CreateReference("Microsoft.AspNetCore.Hosting.dll", Sdk.AspNetCore);
    public static MetadataReference MicrosoftAspNetCoreHostingAbstractions { get; } = CreateReference("Microsoft.AspNetCore.Hosting.Abstractions.dll", Sdk.AspNetCore);
    public static MetadataReference MicrosoftAspNetCoreHttpAbstractions { get; } = CreateReference("Microsoft.AspNetCore.Http.Abstractions.dll", Sdk.AspNetCore);
    public static MetadataReference MicrosoftAspNetCoreHttpFeatures { get; } = CreateReference("Microsoft.AspNetCore.Http.Features.dll", Sdk.AspNetCore);
    public static MetadataReference MicrosoftAspNetCoreHttpResults { get; } = CreateReference("Microsoft.AspNetCore.Http.Results.dll", Sdk.AspNetCore);
    public static MetadataReference MicrosoftAspNetCoreMvc { get; } = CreateReference("Microsoft.AspNetCore.Mvc.dll", Sdk.AspNetCore);
    public static MetadataReference MicrosoftAspNetCoreMvcAbstractions { get; } = CreateReference("Microsoft.AspNetCore.Mvc.Abstractions.dll", Sdk.AspNetCore);
    public static MetadataReference MicrosoftAspNetCoreMvcCore { get; } = CreateReference("Microsoft.AspNetCore.Mvc.Core.dll", Sdk.AspNetCore);
    public static MetadataReference MicrosoftAspNetCoreMvcRazorPages { get; } = CreateReference("Microsoft.AspNetCore.Mvc.RazorPages.dll", Sdk.AspNetCore);
    public static MetadataReference MicrosoftAspNetCoreMvcViewFeatures { get; } = CreateReference("Microsoft.AspNetCore.Mvc.ViewFeatures.dll", Sdk.AspNetCore);
    public static MetadataReference MicrosoftAspNetCoreRouting { get; } = CreateReference("Microsoft.AspNetCore.Routing.dll", Sdk.AspNetCore);
    public static MetadataReference MicrosoftExtensionsHostingAbstractions { get; } = CreateReference("Microsoft.Extensions.Hosting.Abstractions.dll", Sdk.AspNetCore);
    public static MetadataReference MicrosoftExtensionsIdentityCore { get; } = CreateReference("Microsoft.Extensions.Identity.Core.dll", Sdk.AspNetCore);
    public static MetadataReference MicrosoftAspNetCoreCryptographyKeyDerivation { get; } = CreateReference("Microsoft.AspNetCore.Cryptography.KeyDerivation.dll", Sdk.AspNetCore);
    public static MetadataReference MicrosoftExtensionsDependencyInjectionAbstractions { get; } = CreateReference("Microsoft.Extensions.DependencyInjection.Abstractions.dll", Sdk.AspNetCore);
    public static MetadataReference MicrosoftExtensionsLoggingAbstractions { get; } = CreateReference("Microsoft.Extensions.Logging.Abstractions.dll", Sdk.AspNetCore);
    public static MetadataReference MicrosoftExtensionsLoggingEventSource { get; } = CreateReference("Microsoft.Extensions.Logging.EventSource.dll", Sdk.AspNetCore);
    public static MetadataReference MicrosoftExtensionsPrimitives { get; } = CreateReference("Microsoft.Extensions.Primitives.dll", Sdk.AspNetCore);
    public static MetadataReference MicrosoftNetHttpHeadersHeaderNames { get; } = CreateReference("Microsoft.Net.Http.Headers.dll", Sdk.AspNetCore);
}
#endif

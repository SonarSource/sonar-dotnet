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

using static SonarAnalyzer.Test.MetadataReferences.MetadataReferenceFactory;

namespace SonarAnalyzer.TestFramework.MetadataReferences
{
    public static class AspNetCoreMetadataReference
    {
        public static MetadataReference MicrosoftAspNetCoreCors { get; } = Create(typeof(Microsoft.Extensions.DependencyInjection.CorsServiceCollectionExtensions));
        public static MetadataReference MicrosoftAspNetCoreDiagnostics { get; } = Create(typeof(Microsoft.AspNetCore.Diagnostics.StatusCodeContext));
        public static MetadataReference MicrosoftAspNetCoreEventSourceLoggerFactoryExtensions { get; } = Create(typeof(Microsoft.Extensions.Logging.EventSourceLoggerFactoryExtensions));
        public static MetadataReference MicrosoftAspNetCoreHostingAbstractions { get; } = Create(typeof(Microsoft.AspNetCore.Hosting.IWebHost));
        public static MetadataReference MicrosoftAspNetCoreHostingWebHostBuilderExtensions { get; } = Create(typeof(Microsoft.AspNetCore.Hosting.WebHostBuilderExtensions));
        public static MetadataReference MicrosoftAspNetCoreHttpAbstractions { get; } = Create(typeof(Microsoft.AspNetCore.Http.IHttpContextFactory));
        public static MetadataReference MicrosoftAspNetCoreHttpFeatures { get; } = Create(typeof(Microsoft.AspNetCore.Http.IHeaderDictionary));
        public static MetadataReference MicrosoftAspNetCoreMvc { get; } = Create(typeof(Microsoft.Extensions.DependencyInjection.MvcServiceCollectionExtensions));
        public static MetadataReference MicrosoftAspNetCoreMvcAbstractions { get; } = Create(typeof(Microsoft.AspNetCore.Mvc.IActionResult));
        public static MetadataReference MicrosoftAspNetCoreMvcCore { get; } = Create(typeof(Microsoft.AspNetCore.Mvc.ControllerBase));
        public static MetadataReference MicrosoftAspNetCoreMvcViewFeatures { get; } = Create(typeof(Microsoft.AspNetCore.Mvc.Controller));
        public static MetadataReference MicrosoftAspNetCoreRazorPages { get; } = Create(typeof(Microsoft.AspNetCore.Mvc.RazorPages.PageModel));
        public static MetadataReference MicrosoftAspNetCoreRouting { get; } = Create(typeof(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder));
        public static MetadataReference MicrosoftAspNetCoreWebHost { get; } = Create(typeof(Microsoft.AspNetCore.WebHost));
        public static MetadataReference MicrosoftExtensionsHostingAbstractions { get; } = Create(typeof(Microsoft.Extensions.Hosting.IHost));
    }
}
#endif

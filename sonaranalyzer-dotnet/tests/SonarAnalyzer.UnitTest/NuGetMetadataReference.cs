/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest
{
    internal static class NuGetMetadataReference
    {
        // hardcoded version

        public static IEnumerable<MetadataReference> MicrosoftVisualStudioQualityToolsUnitTestFramework =>
            NugetMetadataFactory.Create("VS.QualityTools.UnitTestFramework", "15.0.27323.2");

        public static IEnumerable<MetadataReference> MSTestTestFrameworkV1 =>
            NugetMetadataFactory.Create("MSTest.TestFramework", "1.1.11");

        public static IEnumerable<MetadataReference> XunitFrameworkV1 =>
            NugetMetadataFactory.Create("xunit", "1.9.1")
            .Concat(NugetMetadataFactory.Create("xunit.extensions", "1.9.1"));

        // passed version

        public static IEnumerable<MetadataReference> FluentAssertions(string packageVersion) =>
            NugetMetadataFactory.Create("FluentAssertions", packageVersion);

        public static IEnumerable<MetadataReference> Log4Net(string packageVersion, string targetFramework) =>
            NugetMetadataFactory.Create("log4net", packageVersion, targetFramework);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCore(string packageVersion) =>
            NugetMetadataFactory.Create("Microsoft.AspNetCore", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCoreDiagnostics(string packageVersion) =>
            NugetMetadataFactory.Create("Microsoft.AspNetCore.Diagnostics", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCoreDiagnosticsEntityFrameworkCore(string packageVersion) =>
            NugetMetadataFactory.Create("Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCoreHosting(string packageVersion) =>
            NugetMetadataFactory.Create("Microsoft.AspNetCore.Hosting", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCoreHostingAbstractions(string packageVersion) =>
            NugetMetadataFactory.Create("Microsoft.AspNetCore.Hosting.Abstractions", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCoreHttpAbstractions(string packageVersion) =>
            NugetMetadataFactory.Create("Microsoft.AspNetCore.Http.Abstractions", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCoreHttpFeatures(string packageVersion) =>
            NugetMetadataFactory.Create("Microsoft.AspNetCore.Http.Features", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCoreMvcAbstractions(string packageVersion) =>
            NugetMetadataFactory.Create("Microsoft.AspNetCore.Mvc.Abstractions", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCoreMvcCore(string packageVersion) =>
            NugetMetadataFactory.Create("Microsoft.AspNetCore.Mvc.Core", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCoreMvcViewFeatures(string packageVersion) =>
            NugetMetadataFactory.Create("Microsoft.AspNetCore.Mvc.ViewFeatures", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCoreMvcWebApiCompatShim(string packageVersion) =>
            NugetMetadataFactory.Create("Microsoft.AspNetCore.Mvc.WebApiCompatShim", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCoreRouting(string packageVersion) =>
            NugetMetadataFactory.Create("Microsoft.AspNetCore.Routing", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCoreRoutingAbstractions(string packageVersion) =>
            NugetMetadataFactory.Create("Microsoft.AspNetCore.Routing.Abstractions", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetMvc(string packageVersion) =>
            NugetMetadataFactory.Create("Microsoft.AspNet.Mvc", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetMvcCore(string packageVersion) =>
            NugetMetadataFactory.Create("Microsoft.AspNet.Mvc.Core", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftEntityFrameworkCore(string packageVersion) =>
            NugetMetadataFactory.CreateWithCommandLine("Microsoft.EntityFrameworkCore", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftEntityFrameworkCoreRelational(string packageVersion) =>
            NugetMetadataFactory.CreateWithCommandLine("Microsoft.EntityFrameworkCore.Relational", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftExtensionsConfigurationAbstractions(string packageVersion) =>
            NugetMetadataFactory.Create("Microsoft.Extensions.Configuration.Abstractions", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftExtensionsDependencyInjectionAbstractions(string packageVersion) =>
            NugetMetadataFactory.Create("Microsoft.Extensions.DependencyInjection.Abstractions", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftExtensionsLoggingPackages(string packageVersion) =>
            NugetMetadataFactory.Create("Microsoft.Extensions.Logging", packageVersion)
            .Concat(NugetMetadataFactory.Create("Microsoft.Extensions.Logging.AzureAppServices", packageVersion))
            .Concat(NugetMetadataFactory.Create("Microsoft.Extensions.Logging.Abstractions", packageVersion))
            .Concat(NugetMetadataFactory.Create("Microsoft.Extensions.Logging.Console", packageVersion))
            .Concat(NugetMetadataFactory.Create("Microsoft.Extensions.Logging.Debug", packageVersion))
            .Concat(NugetMetadataFactory.Create("Microsoft.Extensions.Logging.EventLog", packageVersion));

        public static IEnumerable<MetadataReference> MicrosoftExtensionsOptions(string packageVersion) =>
            NugetMetadataFactory.Create("Microsoft.Extensions.Options", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftExtensionsPrimitives(string packageVersion) =>
            NugetMetadataFactory.Create("Microsoft.Extensions.Primitives", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftNetHttpHeaders(string packageVersion) =>
            NugetMetadataFactory.Create("Microsoft.Net.Http.Headers", packageVersion);

        public static IEnumerable<MetadataReference> MSTestTestFramework(string packageVersion) =>
            NugetMetadataFactory.Create("MSTest.TestFramework", packageVersion);

        public static IEnumerable<MetadataReference> MvvmLightLibs(string packageVersion) =>
            NugetMetadataFactory.Create("MvvmLightLibs", packageVersion);

        public static IEnumerable<MetadataReference> NLog(string packageVersion) =>
            NugetMetadataFactory.Create("NLog", packageVersion);

        public static IEnumerable<MetadataReference> NUnit(string packageVersion) =>
            NugetMetadataFactory.Create("NUnit", packageVersion);

        public static IEnumerable<MetadataReference> RestSharp(string packageVersion) =>
            NugetMetadataFactory.Create("RestSharp", packageVersion);

        public static IEnumerable<MetadataReference> SerilogPackages(string packageVersion) =>
            NugetMetadataFactory.Create("Serilog", packageVersion)
                .Concat(NugetMetadataFactory.Create("Serilog.Sinks.Console", packageVersion));

        public static IEnumerable<MetadataReference> SystemCollectionsImmutable(string packageVersion) =>
            NugetMetadataFactory.Create("System.Collections.Immutable", packageVersion);

        public static IEnumerable<MetadataReference> SystemDataSqlServerCe(string packageVersion) =>
            NugetMetadataFactory.Create("Microsoft.SqlServer.Compact", packageVersion);

        public static IEnumerable<MetadataReference> SystemThreadingTasksExtensions(string packageVersion) =>
            NugetMetadataFactory.Create("System.Threading.Tasks.Extensions", packageVersion);

        public static IEnumerable<MetadataReference> SystemValueTuple(string packageVersion) =>
            NugetMetadataFactory.Create("System.ValueTuple", packageVersion);

        public static IEnumerable<MetadataReference> XunitFramework(string packageVersion) =>
            NugetMetadataFactory.Create("xunit.assert", packageVersion)
                .Concat(NugetMetadataFactory.Create("xunit.extensibility.core", packageVersion));
    }
}

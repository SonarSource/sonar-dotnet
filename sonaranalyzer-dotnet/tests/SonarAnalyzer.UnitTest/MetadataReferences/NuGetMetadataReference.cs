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

using System.Linq;
using References = System.Collections.Generic.IEnumerable<Microsoft.CodeAnalysis.MetadataReference>;
using static SonarAnalyzer.UnitTest.TestFramework.NugetMetadataFactory;

namespace SonarAnalyzer.UnitTest.MetadataReferences
{
    internal static class NuGetMetadataReference
    {
        // hardcoded version

        public static References MicrosoftVisualStudioQualityToolsUnitTestFramework => Create("VS.QualityTools.UnitTestFramework", "15.0.27323.2");
        public static References MSTestTestFrameworkV1 => Create("MSTest.TestFramework", "1.1.11");
        public static References XunitFrameworkV1 =>
            CreateWithCommandLine("xunit", "1.9.1")
            .Concat(CreateWithCommandLine("xunit.extensions", "1.9.1"));
        public static References NETStandardV2_1_0 => CreateNETStandard21();

        // passed version

        public static References BouncyCastle(string packageVersion = "1.8.5") => Create("BouncyCastle", packageVersion);
        public static References Dapper(string packageVersion = "1.50.5") => CreateWithCommandLine("Dapper", packageVersion);
        public static References EntityFramework(string packageVersion = "6.2.0") => CreateWithCommandLine("EntityFramework", packageVersion);
        public static References FluentAssertions(string packageVersion) => CreateWithCommandLine("FluentAssertions", packageVersion);
        public static References JWT(string packageVersion = "6.1.0") => Create("JWT", packageVersion);
        public static References Log4Net(string packageVersion, string targetFramework) => Create("log4net", packageVersion, targetFramework);
        public static References MicrosoftAspNetCore(string packageVersion) => Create("Microsoft.AspNetCore", packageVersion);
        public static References MicrosoftAspNetCoreDiagnostics(string packageVersion) => Create("Microsoft.AspNetCore.Diagnostics", packageVersion);
        public static References MicrosoftAspNetCoreDiagnosticsEntityFrameworkCore(string packageVersion) => Create("Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore", packageVersion);
        public static References MicrosoftAspNetCoreHosting(string packageVersion) => Create("Microsoft.AspNetCore.Hosting", packageVersion);
        public static References MicrosoftAspNetCoreHostingAbstractions(string packageVersion) => Create("Microsoft.AspNetCore.Hosting.Abstractions", packageVersion);
        public static References MicrosoftAspNetCoreHttpAbstractions(string packageVersion = "2.2.0") => Create("Microsoft.AspNetCore.Http.Abstractions", packageVersion);
        public static References MicrosoftAspNetCoreHttpFeatures(string packageVersion) => CreateWithCommandLine("Microsoft.AspNetCore.Http.Features", packageVersion);
        public static References MicrosoftAspNetCoreMvcAbstractions(string packageVersion) => CreateWithCommandLine("Microsoft.AspNetCore.Mvc.Abstractions", packageVersion);
        public static References MicrosoftAspNetCoreMvcCore(string packageVersion) => Create("Microsoft.AspNetCore.Mvc.Core", packageVersion);
        public static References MicrosoftAspNetCoreMvcViewFeatures(string packageVersion) => Create("Microsoft.AspNetCore.Mvc.ViewFeatures", packageVersion);
        public static References MicrosoftAspNetCoreMvcWebApiCompatShim(string packageVersion) => Create("Microsoft.AspNetCore.Mvc.WebApiCompatShim", packageVersion);
        public static References MicrosoftAspNetCoreRouting(string packageVersion) => CreateWithCommandLine("Microsoft.AspNetCore.Routing", packageVersion);
        public static References MicrosoftAspNetCoreMvcRazorPages(string packageVersion = "2.2.5") => CreateWithCommandLine("Microsoft.AspNetCore.Mvc.RazorPages", packageVersion);
        public static References MicrosoftAspNetCoreRoutingAbstractions(string packageVersion) => Create("Microsoft.AspNetCore.Routing.Abstractions", packageVersion);
        public static References MicrosoftAspNetMvc(string packageVersion) => Create("Microsoft.AspNet.Mvc", packageVersion);
        public static References MicrosoftAspNetSignalRCore(string packageVersion = "2.4.1") => Create("Microsoft.AspNet.SignalR.Core", packageVersion);
        public static References MicrosoftDataSqliteCore(string packageVersion = "2.0.0") => CreateWithCommandLine("Microsoft.Data.Sqlite.Core", packageVersion);
        public static References MicrosoftEntityFrameworkCore(string packageVersion) => CreateWithCommandLine("Microsoft.EntityFrameworkCore", packageVersion);
        public static References MicrosoftEntityFrameworkCoreSqlServer(string packageVersion) => CreateWithCommandLine("Microsoft.EntityFrameworkCore.SqlServer", packageVersion);
        public static References MicrosoftEntityFrameworkCoreRelational(string packageVersion) => CreateWithCommandLine("Microsoft.EntityFrameworkCore.Relational", packageVersion);
        public static References MicrosoftExtensionsConfigurationAbstractions(string packageVersion) => Create("Microsoft.Extensions.Configuration.Abstractions", packageVersion);
        public static References MicrosoftExtensionsDependencyInjectionAbstractions(string packageVersion) => Create("Microsoft.Extensions.DependencyInjection.Abstractions", packageVersion);
        public static References MicrosoftExtensionsLoggingPackages(string packageVersion) =>
            Create("Microsoft.Extensions.Logging", packageVersion)
            .Concat(Create("Microsoft.Extensions.Logging.AzureAppServices", packageVersion))
            .Concat(Create("Microsoft.Extensions.Logging.Abstractions", packageVersion))
            .Concat(Create("Microsoft.Extensions.Logging.Console", packageVersion))
            .Concat(Create("Microsoft.Extensions.Logging.Debug", packageVersion))
            .Concat(Create("Microsoft.Extensions.Logging.EventLog", packageVersion));
        public static References MicrosoftExtensionsOptions(string packageVersion) => Create("Microsoft.Extensions.Options", packageVersion);
        public static References MicrosoftExtensionsPrimitives(string packageVersion) => Create("Microsoft.Extensions.Primitives", packageVersion);
        public static References MicrosoftNetHttpHeaders(string packageVersion) => CreateWithCommandLine("Microsoft.Net.Http.Headers", packageVersion);
        public static References MicrosoftSqlServerCompact(string packageVersion = "4.0.8876.1") => CreateWithCommandLine("Microsoft.SqlServer.Compact", packageVersion);
        public static References MicrosoftWebXdt(string packageVersion = "3.0.0") => CreateWithCommandLine("Microsoft.Web.Xdt", packageVersion);
        public static References MSTestTestFramework(string packageVersion) => CreateWithCommandLine("MSTest.TestFramework", packageVersion);
        public static References MvvmLightLibs(string packageVersion) => Create("MvvmLightLibs", packageVersion);
        public static References Nancy(string packageVersion = "2.0.0") => Create("Nancy", packageVersion);
        public static References NLog(string packageVersion) => Create("NLog", packageVersion);
        public static References NHibernate(string packageVersion = "5.2.2") => CreateWithCommandLine("NHibernate", packageVersion);
        public static References NSubstitute(string packageVersion) => CreateWithCommandLine("NSubstitute", packageVersion);
        public static References NUnit(string packageVersion) => CreateWithCommandLine("NUnit", packageVersion);
        public static References PetaPocoCompiled(string packageVersion = "6.0.353") => CreateWithCommandLine("PetaPoco.Compiled", packageVersion);
        public static References RestSharp(string packageVersion) => Create("RestSharp", packageVersion);
        public static References SerilogPackages(string packageVersion) =>
            Create("Serilog", packageVersion)
            .Concat(Create("Serilog.Sinks.Console", packageVersion));
        public static References ServiceStackOrmLite(string packageVersion = "5.1.0") => CreateWithCommandLine("ServiceStack.OrmLite", packageVersion);
        public static References SystemCollectionsImmutable(string packageVersion) => Create("System.Collections.Immutable", packageVersion);
        public static References SystemConfigurationConfigurationManager(string packageVersion = "4.7.0") => CreateWithCommandLine("System.Configuration.ConfigurationManager", packageVersion);
        public static References SystemComponentModelComposition(string packageVersion = "4.7.0") => CreateWithCommandLine("System.ComponentModel.Composition", packageVersion);
        public static References SystemDataSqlServerCe(string packageVersion) => CreateWithCommandLine("Microsoft.SqlServer.Compact", packageVersion);
        public static References SystemDataOdbc(string packageVersion = "4.5.0") => CreateWithCommandLine("System.Data.Odbc", packageVersion);
        public static References SystemDataSqlClient(string packageVersion = "4.5.0") => CreateWithCommandLine("System.Data.SqlClient", packageVersion);
        public static References SystemDataSQLiteCore(string packageVersion = "1.0.109.0") => CreateWithCommandLine("System.Data.SQLite.Core", packageVersion);
        public static References SystemDataOracleClient(string packageVersion = "1.0.8") => CreateWithCommandLine("System.Data.OracleClient", packageVersion);
        public static References SystemDDirectoryServices(string packageVersion = "4.7.0") => CreateWithCommandLine("System.DirectoryServices", packageVersion);
        public static References SystemDrawingCommon(string packageVersion = "4.7.0") => CreateWithCommandLine("System.Drawing.Common", packageVersion);
        public static References SystemSecurityCryptographyOpenSsl(string packageVersion = "4.7.0") => Create("System.Security.Cryptography.OpenSsl", packageVersion);
        public static References SystemSecurityPermissions(string packageVersion = "4.7.0") => CreateWithCommandLine("System.Security.Permissions", packageVersion);
        public static References SystemPrivateServiceModel(string packageVersion = "4.7.0") => CreateWithCommandLine("System.Private.ServiceModel", packageVersion);
        public static References SystemServiceModelPrimitives(string packageVersion = "4.7.0") => CreateWithCommandLine("System.ServiceModel.Primitives", packageVersion);
        public static References SystemTextRegularExpressions(string packageVersion = "4.3.1") => CreateWithCommandLine("System.Text.RegularExpressions", packageVersion);
        public static References SystemThreadingTasksExtensions(string packageVersion) => Create("System.Threading.Tasks.Extensions", packageVersion);
        public static References SystemValueTuple(string packageVersion) => Create("System.ValueTuple", packageVersion);
        public static References SystemNetHttp(string packageVersion = "4.3.4") => CreateWithCommandLine("System.Net.Http", packageVersion);
        public static References XunitFramework(string packageVersion) =>
            Create("xunit.assert", packageVersion)
            .Concat(Create("xunit.extensibility.core", packageVersion));
    }
}

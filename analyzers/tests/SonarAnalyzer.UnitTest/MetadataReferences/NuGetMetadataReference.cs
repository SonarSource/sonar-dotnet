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

using System.Linq;
using static SonarAnalyzer.UnitTest.MetadataReferences.NuGetMetadataFactory;
using References = System.Collections.Generic.IEnumerable<Microsoft.CodeAnalysis.MetadataReference>;

namespace SonarAnalyzer.UnitTest.MetadataReferences
{
    internal static class NuGetMetadataReference
    {
        // Hardcoded version
        public static References MicrosoftVisualStudioQualityToolsUnitTestFramework =>
            Create("VS.QualityTools.UnitTestFramework", "15.0.27323.2", null, "Microsoft.VisualStudio.QualityTools.UnitTestFramework.dll");
        public static References MSTestTestFrameworkV1 => Create("MSTest.TestFramework", "1.1.11");
        public static References XunitFrameworkV1 =>
            Create("xunit", "1.9.1")
            .Concat(Create("xunit.extensions", "1.9.1"));
        public static References NETStandardV2_1_0 => CreateNetStandard21();

        // Passed version
        public static References BouncyCastle(string packageVersion = "1.8.5") => Create("BouncyCastle", packageVersion);
        public static References Dapper(string packageVersion = "1.50.5") => Create("Dapper", packageVersion);
        public static References EntityFramework(string packageVersion = "6.2.0") => Create("EntityFramework", packageVersion);
        public static References FluentAssertions(string packageVersion) => Create("FluentAssertions", packageVersion);
        public static References FakeItEasy(string packageVersion) => Create("FakeItEasy", packageVersion);
        public static References JetBrainsDotMemoryUnit(string packageVersion) => Create("JetBrains.DotMemoryUnit", packageVersion);
        public static References JustMock(string packageVersion) => Create("JustMock", packageVersion);
        public static References JWT(string packageVersion) => Create("JWT", packageVersion);
        public static References Log4Net(string packageVersion, string targetFramework) => Create("log4net", packageVersion, null, targetFramework);
        public static References MachineSpecifications(string packageVersion) => Create("Machine.Specifications", packageVersion);
        public static References MicrosoftAspNetCore(string packageVersion) => Create("Microsoft.AspNetCore", packageVersion);
        public static References MicrosoftAspNetCoreDiagnostics(string packageVersion) => Create("Microsoft.AspNetCore.Diagnostics", packageVersion);
        public static References MicrosoftAspNetCoreDiagnosticsEntityFrameworkCore(string packageVersion) => Create("Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore", packageVersion);
        public static References MicrosoftAspNetCoreHosting(string packageVersion) => Create("Microsoft.AspNetCore.Hosting", packageVersion);
        public static References MicrosoftAspNetCoreHostingAbstractions(string packageVersion) => Create("Microsoft.AspNetCore.Hosting.Abstractions", packageVersion);
        public static References MicrosoftAspNetCoreHttpAbstractions(string packageVersion = "2.2.0") => Create("Microsoft.AspNetCore.Http.Abstractions", packageVersion);
        public static References MicrosoftAspNetCoreHttpFeatures(string packageVersion) => Create("Microsoft.AspNetCore.Http.Features", packageVersion);
        public static References MicrosoftAspNetCoreMvcAbstractions(string packageVersion) => Create("Microsoft.AspNetCore.Mvc.Abstractions", packageVersion);
        public static References MicrosoftAspNetCoreMvcCore(string packageVersion) => Create("Microsoft.AspNetCore.Mvc.Core", packageVersion);
        public static References MicrosoftAspNetCoreMvcViewFeatures(string packageVersion) => Create("Microsoft.AspNetCore.Mvc.ViewFeatures", packageVersion);
        public static References MicrosoftAspNetCoreMvcWebApiCompatShim(string packageVersion) => Create("Microsoft.AspNetCore.Mvc.WebApiCompatShim", packageVersion);
        public static References MicrosoftAspNetCoreRouting(string packageVersion) => Create("Microsoft.AspNetCore.Routing", packageVersion);
        public static References MicrosoftAspNetCoreMvcRazorRuntime(string packageVersion = "2.2.0") => Create("Microsoft.AspNetCore.Razor.Runtime", packageVersion);
        public static References MicrosoftAspNetCoreRoutingAbstractions(string packageVersion) => Create("Microsoft.AspNetCore.Routing.Abstractions", packageVersion);
        public static References MicrosoftAspNetMvc(string packageVersion) => Create("Microsoft.AspNet.Mvc", packageVersion);
        public static References MicrosoftAspNetSignalRCore(string packageVersion = "2.4.1") => Create("Microsoft.AspNet.SignalR.Core", packageVersion);
        public static References MicrosoftDataSqliteCore(string packageVersion = "2.0.0") => Create("Microsoft.Data.Sqlite.Core", packageVersion);
        public static References MicrosoftEntityFrameworkCore(string packageVersion) => Create("Microsoft.EntityFrameworkCore", packageVersion);

        public static References MicrosoftEntityFrameworkCoreSqlServer(string packageVersion) => Create("Microsoft.EntityFrameworkCore.SqlServer", packageVersion);
        public static References MicrosoftEntityFrameworkCoreRelational(string packageVersion) => Create("Microsoft.EntityFrameworkCore.Relational", packageVersion);
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
        public static References MicrosoftNetHttpHeaders(string packageVersion) => Create("Microsoft.Net.Http.Headers", packageVersion);
        public static References MicrosoftSqlServerCompact(string packageVersion = "4.0.8876.1") => Create("Microsoft.SqlServer.Compact", packageVersion);
        public static References MicrosoftWebXdt(string packageVersion = "3.0.0") => Create("Microsoft.Web.Xdt", packageVersion);
        public static References MonoPosixNetStandard(string packageVersion = "1.0.0") => Create("Mono.Posix.NETStandard", packageVersion, "linux-x64");
        public static References Moq(string packageVersion) => Create("Moq", packageVersion);
        public static References MSTestTestFramework(string packageVersion) => Create("MSTest.TestFramework", packageVersion);
        public static References MvvmLightLibs(string packageVersion) => Create("MvvmLightLibs", packageVersion);
        public static References MySqlData(string packageVersion) => Create("MySql.Data", packageVersion);
        public static References MySqlDataEntityFrameworkCore(string packageVersion = "6.10.4") => Create("MySql.Data.EntityFrameworkCore", packageVersion);
        public static References Nancy(string packageVersion = "2.0.0") => Create("Nancy", packageVersion);
        public static References NLog(string packageVersion) => Create("NLog", packageVersion);
        public static References NHibernate(string packageVersion = "5.2.2") => Create("NHibernate", packageVersion);
        public static References NpgsqlEntityFrameworkCorePostgreSQL(string packageVersion = "3.1.3") => Create("Npgsql.EntityFrameworkCore.PostgreSQL", packageVersion);
        public static References NSubstitute(string packageVersion) => Create("NSubstitute", packageVersion);
        public static References NUnit(string packageVersion) => Create("NUnit", packageVersion);
        public static References NUnitLite(string packageVersion) => Create("NUnitLite", packageVersion);
        public static References PetaPocoCompiled(string packageVersion = "6.0.353") => Create("PetaPoco.Compiled", packageVersion);
        public static References RhinoMocks(string packageVersion) => Create("RhinoMocks", packageVersion);
        public static References Shouldly(string packageVersion) => Create("Shouldly", packageVersion);
        public static References SerilogPackages(string packageVersion) =>
            Create("Serilog", packageVersion)
            .Concat(Create("Serilog.Sinks.Console", packageVersion));
        public static References ServiceStackOrmLite(string packageVersion = "5.1.0") => Create("ServiceStack.OrmLite", packageVersion);
        public static References SpecFlow(string packageVersion) => Create("SpecFlow", packageVersion);
        public static References SystemCollectionsImmutable(string packageVersion) => Create("System.Collections.Immutable", packageVersion);
        public static References SystemConfigurationConfigurationManager(string packageVersion = "4.7.0") => Create("System.Configuration.ConfigurationManager", packageVersion);
        public static References SystemComponentModelComposition(string packageVersion = "4.7.0") => Create("System.ComponentModel.Composition", packageVersion);
        public static References SystemDataSqlServerCe(string packageVersion) => Create("Microsoft.SqlServer.Compact", packageVersion);
        public static References SystemDataOdbc(string packageVersion = "4.5.0") => Create("System.Data.Odbc", packageVersion);
        public static References SystemDataSqlClient(string packageVersion = "4.5.0") => Create("System.Data.SqlClient", packageVersion);
        public static References SystemDataSQLiteCore(string packageVersion = "1.0.109.0") => Create("System.Data.SQLite.Core", packageVersion);
        public static References SystemDataOracleClient(string packageVersion = "1.0.8") => Create("System.Data.OracleClient", packageVersion);
        public static References SystemDDirectoryServices(string packageVersion = "4.7.0") => Create("System.DirectoryServices", packageVersion);
        public static References SystemDrawingCommon(string packageVersion = "4.7.0") => Create("System.Drawing.Common", packageVersion);
        public static References SystemSecurityCryptographyOpenSsl(string packageVersion = "4.7.0") => Create("System.Security.Cryptography.OpenSsl", packageVersion);
        public static References SystemSecurityPermissions(string packageVersion = "4.7.0") => Create("System.Security.Permissions", packageVersion);
        public static References SystemPrivateServiceModel(string packageVersion = "4.7.0") => Create("System.Private.ServiceModel", packageVersion);
        public static References SystemServiceModelPrimitives(string packageVersion = "4.7.0") => Create("System.ServiceModel.Primitives", packageVersion);
        public static References SystemTextRegularExpressions(string packageVersion = "4.3.1") => Create("System.Text.RegularExpressions", packageVersion);
        public static References SystemThreadingTasksExtensions(string packageVersion) => Create("System.Threading.Tasks.Extensions", packageVersion);
        public static References SystemValueTuple(string packageVersion) => Create("System.ValueTuple", packageVersion);
        public static References XunitFramework(string packageVersion) =>
            Create("xunit.assert", packageVersion)
            .Concat(Create("xunit.extensibility.core", packageVersion));
    }
}

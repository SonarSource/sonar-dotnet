﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using static SonarAnalyzer.TestFramework.MetadataReferences.NuGetMetadataFactory;
using References = System.Collections.Generic.IEnumerable<Microsoft.CodeAnalysis.MetadataReference>;

namespace SonarAnalyzer.TestFramework.MetadataReferences;

public static class NuGetMetadataReference
{
#pragma warning disable S103  // Lines should not be too long
#pragma warning disable T0016 // Add an empty line before this declaration
    // Hardcoded version
    public static References MicrosoftVisualStudioQualityToolsUnitTestFramework =>
        Create("VS.QualityTools.UnitTestFramework", "15.0.27323.2", null, "Microsoft.VisualStudio.QualityTools.UnitTestFramework.dll");
    public static References MSTestTestFrameworkV1 => Create("MSTest.TestFramework", "1.1.11");
    public static References XunitFrameworkV1 => Create("xunit", "1.9.1").Concat(Create("xunit.extensions", "1.9.1"));

    // Passed version
    public static References AzureCore(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Azure.Core", packageVersion);
    public static References AzureIdentity(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Azure.Identity", packageVersion);
    public static References AzureMessagingServiceBus(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Azure.Messaging.ServiceBus", packageVersion);
    public static References AzureResourceManager(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Azure.ResourceManager", packageVersion);
    public static References AzureStorageCommon(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Azure.Storage.Common", packageVersion);
    public static References AzureStorageBlobs(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Azure.Storage.Blobs", packageVersion);
    public static References AzureStorageFilesDataLake(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Azure.Storage.Files.DataLake", packageVersion);
    public static References AzureStorageFilesShares(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Azure.Storage.Files.Shares", packageVersion);
    public static References AzureStorageQueues(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Azure.Storage.Queues", packageVersion);
    public static References BouncyCastle(string packageVersion = "1.8.5") => Create("BouncyCastle", packageVersion);
    public static References BouncyCastleCryptography(string packageVersion = TestConstants.NuGetLatestVersion) => Create("BouncyCastle.Cryptography", packageVersion);
    public static References CastleCore(string packageVersion = "5.1.1") => Create("Castle.Core", packageVersion);
    public static References Dapper(string packageVersion = "1.50.5") => Create("Dapper", packageVersion);
    public static References CommonLoggingCore(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Common.Logging.Core", packageVersion);
    public static References EntityFramework(string packageVersion = "6.2.0") => Create("EntityFramework", packageVersion);
    public static References FluentAssertions(string packageVersion) => Create("FluentAssertions", packageVersion);
    public static References FluentValidation(string packageVersion = TestConstants.NuGetLatestVersion) => Create("FluentValidation", packageVersion);
    public static References FakeItEasy(string packageVersion) => Create("FakeItEasy", packageVersion);
    public static References FsCheckXunit(string packageVersion = TestConstants.NuGetLatestVersion) =>
        [
            .. Create("FsCheck", packageVersion),
            .. Create("FsCheck.Xunit", packageVersion),
            .. XunitFramework(TestConstants.NuGetLatestVersion),
        ];
    public static References FsCheckNunit(string packageVersion = TestConstants.NuGetLatestVersion) =>
        [
            .. Create("FsCheck", packageVersion),
            .. Create("FsCheck.NUnit", packageVersion),
            .. NUnit(TestConstants.NuGetLatestVersion),
        ];
    public static References JetBrainsDotMemoryUnit(string packageVersion) => Create("JetBrains.DotMemoryUnit", packageVersion);
    public static References JustMock(string packageVersion) => Create("JustMock", packageVersion);
    public static References JWT(string packageVersion) => Create("JWT", packageVersion);
    public static References Log4Net(string packageVersion, string targetFramework) => Create("log4net", packageVersion, null, targetFramework);
    public static References MachineSpecifications(string packageVersion) => Create("Machine.Specifications", packageVersion);
    public static References MicrosoftAspNetCore(string packageVersion) => Create("Microsoft.AspNetCore", packageVersion);
    public static References MicrosoftAspNetCoreComponents(string packageVersion) => Create("Microsoft.AspNetCore.Components", packageVersion);
    public static References MicrosoftAspNetCoreComponentsWeb(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Microsoft.AspNetCore.Components.Web", packageVersion);
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
    // There is no package version of Microsoft.AspNet.Identity that is NOT a pre-release.
    public static References MicrosoftAspNetIdentity(string packageVersion = "3.0.0-rc1-final") => Create("Microsoft.AspNet.Identity", packageVersion);
    public static References MicrosoftAspNetMvc(string packageVersion) => Create("Microsoft.AspNet.Mvc", packageVersion);
    public static References MicrosoftAspNetCoreAppRef(string packageVersion) => Create("Microsoft.AspNetCore.App.Ref", packageVersion);
    public static References MicrosoftAspNetSignalRCore(string packageVersion = "2.4.1") => Create("Microsoft.AspNet.SignalR.Core", packageVersion);
    public static References MicrosoftAspNetWebApiCors(string packageVersion) => Create("Microsoft.AspNet.WebApi.Cors", packageVersion);
    public static References MicrosoftAzureCosmos(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Microsoft.Azure.Cosmos", packageVersion);
    public static References MicrosoftAzureDocumentDB(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Microsoft.Azure.DocumentDB", packageVersion);
    public static References MicrosoftAzureServiceBus(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Microsoft.Azure.ServiceBus", packageVersion);
    public static References MicrosoftAzureWebJobs(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Microsoft.Azure.WebJobs", packageVersion);
    public static References MicrosoftAzureWebJobsCore(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Microsoft.Azure.WebJobs.Core", packageVersion);
    public static References MicrosoftAzureWebJobsExtensionsDurableTask(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Microsoft.Azure.WebJobs.Extensions.DurableTask", packageVersion);
    public static References MicrosoftAzureWebJobsExtensionsHttp(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Microsoft.Azure.WebJobs.Extensions.Http", packageVersion);
    public static References MicrosoftBuildNoTargets(string packageVersion = "3.1.0") => Create("Microsoft.Build.NoTargets", packageVersion);
    public static References MicrosoftCodeAnalysisCSharp(string packageVersion = TestConstants.NuGetLatestVersion) => [
        ..Create("Microsoft.CodeAnalysis.Common", packageVersion),
        ..Create("Microsoft.CodeAnalysis.CSharp", packageVersion),
        ];
    public static References MicrosoftDataSqlClient(string packageVersion = "5.1.0") => Create("Microsoft.Data.SqlClient", packageVersion);
    public static References MicrosoftDataSqliteCore(string packageVersion = "2.0.0") => Create("Microsoft.Data.Sqlite.Core", packageVersion);
    public static References MicrosoftEntityFramework(string packageVersion) => Create("EntityFramework", packageVersion);
    public static References MicrosoftEntityFrameworkCore(string packageVersion) => Create("Microsoft.EntityFrameworkCore", packageVersion);
    public static References MicrosoftEntityFrameworkCoreAbstractions(string packageVersion) => Create("Microsoft.EntityFrameworkCore.Abstractions", packageVersion);
    public static References MicrosoftEntityFrameworkCoreSqliteCore(string packageVersion) => Create("Microsoft.EntityFrameworkCore.Sqlite.Core", packageVersion);
    public static References MicrosoftEntityFrameworkCoreSqlServer(string packageVersion) => Create("Microsoft.EntityFrameworkCore.SqlServer", packageVersion);
    public static References MicrosoftEntityFrameworkCoreRelational(string packageVersion) => Create("Microsoft.EntityFrameworkCore.Relational", packageVersion);
    public static References MicrosoftExtensionsConfigurationAbstractions(string packageVersion) => Create("Microsoft.Extensions.Configuration.Abstractions", packageVersion);
    public static References MicrosoftExtensionsDependencyInjectionAbstractions(string packageVersion) => Create("Microsoft.Extensions.DependencyInjection.Abstractions", packageVersion);
    public static References MicrosoftExtensionsHttp(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Microsoft.Extensions.Http", packageVersion);
    public static References MicrosoftExtensionsLoggingPackages(string packageVersion) =>
        Create("Microsoft.Extensions.Logging", packageVersion)
        .Concat(Create("Microsoft.Extensions.Logging.AzureAppServices", packageVersion))
        .Concat(Create("Microsoft.Extensions.Logging.Abstractions", packageVersion == TestConstants.NuGetLatestVersion
                                                                            ? TestConstants.DotNet9Preview7                // Work around for .Net 9 preview. Can be removed after release.
                                                                            : packageVersion))
        .Concat(Create("Microsoft.Extensions.Logging.Console", packageVersion))
        .Concat(Create("Microsoft.Extensions.Logging.Debug", packageVersion))
        .Concat(Create("Microsoft.Extensions.Logging.EventLog", packageVersion));
    public static References MicrosoftExtensionsLoggingAbstractions(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Microsoft.Extensions.Logging.Abstractions", packageVersion);
    public static References MicrosoftExtensionsOptions(string packageVersion) => Create("Microsoft.Extensions.Options", packageVersion);
    public static References MicrosoftExtensionsPrimitives(string packageVersion) => Create("Microsoft.Extensions.Primitives", packageVersion);
    public static References MicrosoftIdentityModelTokens(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Microsoft.IdentityModel.Tokens", packageVersion);
    public static References MicrosoftJSInterop(string packageVersion) => Create("Microsoft.JSInterop", packageVersion);
    public static References MicrosoftNetHttpHeaders(string packageVersion) => Create("Microsoft.Net.Http.Headers", packageVersion);
    public static References MicrosoftNetSdkFunctions(string packageVersion = TestConstants.NuGetLatestVersion) =>
        Create("Microsoft.NET.Sdk.Functions", packageVersion)
        .Concat(MicrosoftAzureWebJobs(packageVersion))
        .Concat(MicrosoftAzureWebJobsCore(packageVersion))
        .Concat(MicrosoftAzureWebJobsExtensionsHttp(packageVersion))
        .Concat(MicrosoftExtensionsLoggingPackages(packageVersion))
        .Concat(MicrosoftAspNetCoreMvcAbstractions(packageVersion))
        .Concat(MicrosoftAspNetCoreMvcCore(packageVersion))
        .Concat(MicrosoftAspNetCoreHttpAbstractions(packageVersion));
    public static References MicrosoftNetWebApiCore(string packageVersion) => Create("Microsoft.AspNet.WebApi.Core", packageVersion);
    public static References MicrosoftSqlServerCompact(string packageVersion = "4.0.8876.1") => Create("Microsoft.SqlServer.Compact", packageVersion);
    public static References MicrosoftWebXdt(string packageVersion = "3.0.0") => Create("Microsoft.Web.Xdt", packageVersion);
    public static References MongoDBDriver(string packageVersion = TestConstants.NuGetLatestVersion) =>
        Create("MongoDB.Driver", packageVersion)
        .Concat(MongoDBDriverCore(packageVersion));
    public static References MongoDBDriverCore(string packageVersion = TestConstants.NuGetLatestVersion) => Create("MongoDB.Driver.Core", packageVersion);
    public static References MonoPosixNetStandard(string packageVersion = "1.0.0") => Create("Mono.Posix.NETStandard", packageVersion, "linux-x64");
    public static References MonoDataSqlite(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Mono.Data.Sqlite", packageVersion);
    public static References Moq(string packageVersion) => Create("Moq", packageVersion);
    public static References MoreLinq(string packageVersion = TestConstants.NuGetLatestVersion) => Create("morelinq", packageVersion);
    public static References MSTestTestFramework(string packageVersion) => Create("MSTest.TestFramework", packageVersion);
    public static References MvvmLightLibs(string packageVersion) => Create("MvvmLightLibs", packageVersion);
    public static References MySqlData(string packageVersion) => Create("MySql.Data", packageVersion);
    public static References MySqlDataEntityFrameworkCore(string packageVersion = "8.0.22") => Create("MySql.Data.EntityFrameworkCore", packageVersion);
    public static References Nancy(string packageVersion = "2.0.0") => Create("Nancy", packageVersion);
    public static References NFluent(string packageVersion) => Create("NFluent", packageVersion);
    public static References NLog(string packageVersion = TestConstants.NuGetLatestVersion) => Create("NLog", packageVersion);
    public static References NHibernate(string packageVersion = "5.2.2") => Create("NHibernate", packageVersion);
    public static References NpgsqlEntityFrameworkCorePostgreSQL(string packageVersion) => Create("Npgsql.EntityFrameworkCore.PostgreSQL", packageVersion);
    public static References NSubstitute(string packageVersion) => Create("NSubstitute", packageVersion);
    public static References NewtonsoftJson(string packageVersion) => Create("Newtonsoft.Json", packageVersion);
    public static References NUnit(string packageVersion) => Create("NUnit", packageVersion);
    public static References NUnitLite(string packageVersion) => Create("NUnitLite", packageVersion);
    public static References OracleEntityFrameworkCore(string packageVersion) => Create("Oracle.EntityFrameworkCore", packageVersion);
    public static References PetaPocoCompiled(string packageVersion = "6.0.353") => Create("PetaPoco.Compiled", packageVersion);
    public static References RhinoMocks(string packageVersion) => Create("RhinoMocks", packageVersion);
    public static References Shouldly(string packageVersion) => Create("Shouldly", packageVersion);
    public static References Serilog(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Serilog", packageVersion);
    public static References SerilogSinksConsole(string packageVersion) => Create("Serilog.Sinks.Console", packageVersion);
    public static References ServiceStackOrmLite(string packageVersion = "5.1.0") => Create("ServiceStack.OrmLite", packageVersion);
    public static References SpecFlow(string packageVersion) => Create("SpecFlow", packageVersion);
    public static References SystemCollectionsImmutable(string packageVersion) => Create("System.Collections.Immutable", packageVersion);
    public static References SystemConfigurationConfigurationManager(string packageVersion = "4.7.0") => Create("System.Configuration.ConfigurationManager", packageVersion);
    public static References SystemComponentModelAnnotations(string packageVersion = "5.0.0") => Create("System.ComponentModel.Annotations", packageVersion);
    public static References SystemComponentModelComposition(string packageVersion = "4.7.0") => Create("System.ComponentModel.Composition", packageVersion);
    public static References SystemComponentModelTypeConverter(string packageVersion = "4.3.0") => Create("System.ComponentModel.TypeConverter", packageVersion);
    public static References SystemCompositionAttributedModel(string packageVersion = "6.0.0") => Create("System.Composition.AttributedModel", packageVersion);
    public static References SystemDataSqlServerCe(string packageVersion) => Create("Microsoft.SqlServer.Compact", packageVersion);
    public static References SystemDataOdbc(string packageVersion = "4.5.0") => Create("System.Data.Odbc", packageVersion);
    public static References SystemDataSqlClient(string packageVersion = "4.5.0") => Create("System.Data.SqlClient", packageVersion);
    public static References SystemDataSQLiteCore(string packageVersion = "1.0.109.0") => Create("System.Data.SQLite.Core", packageVersion);
    public static References SystemDataOracleClient(string packageVersion = "1.0.8") => Create("System.Data.OracleClient", packageVersion);
    public static References SystemDDirectoryServices(string packageVersion = "4.7.0") => Create("System.DirectoryServices", packageVersion);
    public static References SystemDrawingCommon(string packageVersion = "4.7.0") => Create("System.Drawing.Common", packageVersion);
    public static References SystemIdentityModelTokensJwt(string packageVersion = TestConstants.NuGetLatestVersion) => Create("System.IdentityModel.Tokens.Jwt", packageVersion);
    public static References SystemMemory(string packageVersion = TestConstants.NuGetLatestVersion) => Create("System.Memory", packageVersion);
    public static References SystemNetHttp(string packageVersion = TestConstants.NuGetLatestVersion) => Create("System.Net.Http", packageVersion);
    public static References SystemSecurityCryptographyOpenSsl(string packageVersion = "4.7.0") => Create("System.Security.Cryptography.OpenSsl", packageVersion);
    public static References SystemSecurityCryptographyXml(string packageVersion = TestConstants.NuGetLatestVersion) => Create("System.Security.Cryptography.Xml", packageVersion);
    public static References SystemSecurityPermissions(string packageVersion = "4.7.0") => Create("System.Security.Permissions", packageVersion);
    public static References SystemPrivateServiceModel(string packageVersion = "4.7.0") => Create("System.Private.ServiceModel", packageVersion);
    public static References SystemServiceModelPrimitives(string packageVersion = "4.7.0") => Create("System.ServiceModel.Primitives", packageVersion);
    public static References SystemTextEncodingsWeb(string packageVersion) => Create("System.Text.Encodings.Web", packageVersion);
    public static References SystemTextJson(string packageVersion) => Create("System.Text.Json", packageVersion);
    public static References SystemTextRegularExpressions(string packageVersion = "4.3.1") => Create("System.Text.RegularExpressions", packageVersion);
    public static References SystemThreadingTasksExtensions(string packageVersion) => Create("System.Threading.Tasks.Extensions", packageVersion);
    public static References SystemValueTuple(string packageVersion) => Create("System.ValueTuple", packageVersion);
    public static References SwashbuckleAspNetCoreAnnotations(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Swashbuckle.AspNetCore.Annotations", packageVersion);
    public static References SwashbuckleAspNetCoreSwagger(string packageVersion = TestConstants.NuGetLatestVersion) => Create("Swashbuckle.AspNetCore.Swagger", packageVersion);
    public static References TimeZoneConverter(string packageVersion = TestConstants.NuGetLatestVersion) => Create("TimeZoneConverter", packageVersion);
    public static References XunitFramework(string packageVersion) =>
        [.. Create("xunit.assert", packageVersion), .. Create("xunit.extensibility.core", packageVersion)];
    public static References XunitFrameworkV3(string packageVersion = TestConstants.NuGetLatestVersion) =>
        [.. Create("xunit.v3.assert", packageVersion), .. Create("xunit.v3.extensibility.core", packageVersion)];
}

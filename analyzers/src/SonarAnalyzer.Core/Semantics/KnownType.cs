/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.Core.Semantics;

public sealed partial class KnownType
{
#pragma warning disable S103    // Lines should not be too long
#pragma warning disable SA1310  // FieldNamesMustNotContainUnderscore
#pragma warning disable SA1311  // Static readonly fields should begin with upper-case letter
#pragma warning disable SA1307  // Field 'log4net_Config_XmlConfigurator' should begin with upper-case letter
#pragma warning disable SA1304  // Non-private readonly fields should begin with upper-case letter
#pragma warning disable T0016   // Empty lines between multiline declarations

    public static readonly KnownType Azure_Messaging_ServiceBus_Administration_ServiceBusAdministrationClient = new("Azure.Messaging.ServiceBus.Administration.ServiceBusAdministrationClient");
    public static readonly KnownType Azure_Messaging_ServiceBus_ServiceBusClient = new("Azure.Messaging.ServiceBus.ServiceBusClient");
    public static readonly KnownType Azure_Storage_Blobs_BlobServiceClient = new("Azure.Storage.Blobs.BlobServiceClient");
    public static readonly KnownType Azure_Storage_Queues_QueueServiceClient = new("Azure.Storage.Queues.QueueServiceClient");
    public static readonly KnownType Azure_Storage_Files_Shares_ShareServiceClient = new("Azure.Storage.Files.Shares.ShareServiceClient");
    public static readonly KnownType Azure_Storage_Files_DataLake_DataLakeServiceClient = new("Azure.Storage.Files.DataLake.DataLakeServiceClient");
    public static readonly KnownType Azure_ResourceManager_ArmClient = new("Azure.ResourceManager.ArmClient");
    public static readonly KnownType Castle_Core_Logging_ILogger = new("Castle.Core.Logging.ILogger");
    public static readonly KnownType Common_Logging_ILog = new("Common.Logging.ILog");
    public static readonly KnownType Dapper_SqlMapper = new("Dapper.SqlMapper");
    public static readonly KnownType Dapper_CommandDefinition = new("Dapper.CommandDefinition");
    public static readonly KnownType FluentAssertions_AssertionExtensions = new("FluentAssertions.AssertionExtensions");
    public static readonly KnownType FluentAssertions_Execution_AssertionScope = new("FluentAssertions.Execution.AssertionScope");
    public static readonly KnownType FluentAssertions_Primitives_ReferenceTypeAssertions = new("FluentAssertions.Primitives.ReferenceTypeAssertions", "TSubject", "TAssertions");
    public static readonly KnownType FluentValidation_IValidator = new("FluentValidation.IValidator");
    public static readonly KnownType FluentValidation_IValidator_T = new("FluentValidation.IValidator", "T");
    public static readonly KnownType JWT_Builder_JwtBuilder = new("JWT.Builder.JwtBuilder");
    public static readonly KnownType JWT_IJwtDecoder = new("JWT.IJwtDecoder");
    public static readonly KnownType JWT_JwtDecoderExtensions = new("JWT.JwtDecoderExtensions");
    public static readonly KnownType log4net_Config_BasicConfigurator = new("log4net.Config.BasicConfigurator");
    public static readonly KnownType log4net_Config_DOMConfigurator = new("log4net.Config.DOMConfigurator");
    public static readonly KnownType log4net_Config_XmlConfigurator = new("log4net.Config.XmlConfigurator");
    public static readonly KnownType log4net_Core_ILogger = new("log4net.Core.ILogger");
    public static readonly KnownType log4net_ILog = new("log4net.ILog");
    public static readonly KnownType log4net_LogManager = new("log4net.LogManager");
    public static readonly KnownType log4net_Util_ILogExtensions = new("log4net.Util.ILogExtensions");
    public static readonly KnownType Microsoft_AspNet_Identity_PasswordHasherOptions = new("Microsoft.AspNet.Identity.PasswordHasherOptions");
    public static readonly KnownType Microsoft_AspNet_SignalR_Hub = new("Microsoft.AspNet.SignalR.Hub");
    public static readonly KnownType Microsoft_AspNetCore_Builder_DeveloperExceptionPageExtensions = new("Microsoft.AspNetCore.Builder.DeveloperExceptionPageExtensions");
    public static readonly KnownType Microsoft_AspNetCore_Builder_DatabaseErrorPageExtensions = new("Microsoft.AspNetCore.Builder.DatabaseErrorPageExtensions");
    public static readonly KnownType Microsoft_AspNetCore_Components_Forms_IBrowserFile = new("Microsoft.AspNetCore.Components.Forms.IBrowserFile");
    public static readonly KnownType Microsoft_AspNetCore_Components_Forms_InputFileChangeEventArgs = new("Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs");
    public static readonly KnownType Microsoft_AspNetCore_Components_ParameterAttribute = new("Microsoft.AspNetCore.Components.ParameterAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Components_Rendering_RenderTreeBuilder = new("Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder");
    public static readonly KnownType Microsoft_AspNetCore_Components_RouteAttribute = new("Microsoft.AspNetCore.Components.RouteAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Components_SupplyParameterFromQueryAttribute = new("Microsoft.AspNetCore.Components.SupplyParameterFromQueryAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Cors_Infrastructure_CorsPolicyBuilder = new("Microsoft.AspNetCore.Cors.Infrastructure.CorsPolicyBuilder");
    public static readonly KnownType Microsoft_AspNetCore_Cryptography_KeyDerivation_KeyDerivation = new("Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivation");
    public static readonly KnownType Microsoft_AspNetCore_Hosting_HostingEnvironmentExtensions = new("Microsoft.AspNetCore.Hosting.HostingEnvironmentExtensions");
    public static readonly KnownType Microsoft_AspNetCore_Hosting_WebHostBuilderExtensions = new("Microsoft.AspNetCore.Hosting.WebHostBuilderExtensions");
    public static readonly KnownType Microsoft_AspNetCore_Http_CookieOptions = new("Microsoft.AspNetCore.Http.CookieOptions");
    public static readonly KnownType Microsoft_AspNetCore_Http_HeaderDictionaryExtensions = new("Microsoft.AspNetCore.Http.HeaderDictionaryExtensions");
    public static readonly KnownType Microsoft_AspNetCore_Http_HttpResponse = new("Microsoft.AspNetCore.Http.HttpResponse");
    public static readonly KnownType Microsoft_AspNetCore_Http_IFormCollection = new("Microsoft.AspNetCore.Http.IFormCollection");
    public static readonly KnownType Microsoft_AspNetCore_Http_IFormFile = new("Microsoft.AspNetCore.Http.IFormFile");
    public static readonly KnownType Microsoft_AspNetCore_Http_IFormFileCollection = new("Microsoft.AspNetCore.Http.IFormFileCollection");
    public static readonly KnownType Microsoft_AspNetCore_Http_IHeaderDictionary = new("Microsoft.AspNetCore.Http.IHeaderDictionary");
    public static readonly KnownType Microsoft_AspNetCore_Http_IQueryCollection = new("Microsoft.AspNetCore.Http.IQueryCollection");
    public static readonly KnownType Microsoft_AspNetCore_Http_IRequestCookieCollection = new("Microsoft.AspNetCore.Http.IRequestCookieCollection");
    public static readonly KnownType Microsoft_AspNetCore_Http_IResponseCookies = new("Microsoft.AspNetCore.Http.IResponseCookies");
    public static readonly KnownType Microsoft_AspNetCore_Http_IResult = new("Microsoft.AspNetCore.Http.IResult");
    public static readonly KnownType Microsoft_AspNetCore_Http_Results = new("Microsoft.AspNetCore.Http.Results");
    public static readonly KnownType Microsoft_AspNetCore_Identity_PasswordHasherOptions = new("Microsoft.AspNetCore.Identity.PasswordHasherOptions");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_AcceptVerbsAttribute = new("Microsoft.AspNetCore.Mvc.AcceptVerbsAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_ApiControllerAttribute = new("Microsoft.AspNetCore.Mvc.ApiControllerAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_ApiConventionMethodAttribute = new("Microsoft.AspNetCore.Mvc.ApiConventionMethodAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_ApiConventionTypeAttribute = new("Microsoft.AspNetCore.Mvc.ApiConventionTypeAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_ApiExplorerSettingsAttribute = new("Microsoft.AspNetCore.Mvc.ApiExplorerSettingsAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_Controller = new("Microsoft.AspNetCore.Mvc.Controller");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_ControllerBase = new("Microsoft.AspNetCore.Mvc.ControllerBase");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_ControllerAttribute = new("Microsoft.AspNetCore.Mvc.ControllerAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_DisableRequestSizeLimitAttribute = new("Microsoft.AspNetCore.Mvc.DisableRequestSizeLimitAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_Filters_ActionFilterAttribute = new("Microsoft.AspNetCore.Mvc.Filters.ActionFilterAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_Filters_IActionFilter = new("Microsoft.AspNetCore.Mvc.Filters.IActionFilter");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_Filters_IAsyncActionFilter = new("Microsoft.AspNetCore.Mvc.Filters.IAsyncActionFilter");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_FromServicesAttribute = new("Microsoft.AspNetCore.Mvc.FromServicesAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_HttpDeleteAttribute = new("Microsoft.AspNetCore.Mvc.HttpDeleteAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_HttpGetAttribute = new("Microsoft.AspNetCore.Mvc.HttpGetAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_HttpHeadAttribute = new("Microsoft.AspNetCore.Mvc.HttpHeadAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_HttpOptionsAttribute = new("Microsoft.AspNetCore.Mvc.HttpOptionsAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_HttpPatchAttribute = new("Microsoft.AspNetCore.Mvc.HttpPatchAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_HttpPostAttribute = new("Microsoft.AspNetCore.Mvc.HttpPostAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_HttpPutAttribute = new("Microsoft.AspNetCore.Mvc.HttpPutAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_IActionResult = new("Microsoft.AspNetCore.Mvc.IActionResult");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_IgnoreAntiforgeryTokenAttribute = new("Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_Infrastructure_ActionResultObjectValueAttribute = new("Microsoft.AspNetCore.Mvc.Infrastructure.ActionResultObjectValueAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_ModelBinding_BindNeverAttribute = new("Microsoft.AspNetCore.Mvc.ModelBinding.BindNeverAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_ModelBinding_ModelStateDictionary = new("Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_ModelBinding_Validation_ValidateNeverAttribute = new("Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNeverAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_NonActionAttribute = new("Microsoft.AspNetCore.Mvc.NonActionAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_NonControllerAttribute = new("Microsoft.AspNetCore.Mvc.NonControllerAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_ObjectResult = new("Microsoft.AspNetCore.Mvc.ObjectResult");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_ProducesAttribute = new("Microsoft.AspNetCore.Mvc.ProducesAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_ProducesAttribute_T = new("Microsoft.AspNetCore.Mvc.ProducesAttribute", "T");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_ProducesResponseTypeAttribute = new("Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_ProducesResponseTypeAttribute_T = new("Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute", "T");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_RazorPages_PageModel = new("Microsoft.AspNetCore.Mvc.RazorPages.PageModel");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_RequestFormLimitsAttribute = new("Microsoft.AspNetCore.Mvc.RequestFormLimitsAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_RequestSizeLimitAttribute = new("Microsoft.AspNetCore.Mvc.RequestSizeLimitAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_RouteAttribute = new("Microsoft.AspNetCore.Mvc.RouteAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_Routing_HttpMethodAttribute = new("Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Mvc_Routing_IRouteTemplateProvider = new("Microsoft.AspNetCore.Mvc.Routing.IRouteTemplateProvider");
    public static readonly KnownType Microsoft_AspNetCore_Razor_Hosting_RazorCompiledItemAttribute = new("Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute");
    public static readonly KnownType Microsoft_AspNetCore_Routing_RouteValueDictionary = new("Microsoft.AspNetCore.Routing.RouteValueDictionary");
    public static readonly KnownType Microsoft_Azure_Cosmos_CosmosClient = new("Microsoft.Azure.Cosmos.CosmosClient");
    public static readonly KnownType Microsoft_Azure_Documents_Client_DocumentClient = new("Microsoft.Azure.Documents.Client.DocumentClient");
    public static readonly KnownType Microsoft_Azure_ServiceBus_Management_ManagementClient = new("Microsoft.Azure.ServiceBus.Management.ManagementClient");
    public static readonly KnownType Microsoft_Azure_ServiceBus_QueueClient = new("Microsoft.Azure.ServiceBus.QueueClient");
    public static readonly KnownType Microsoft_Azure_ServiceBus_SessionClient = new("Microsoft.Azure.ServiceBus.SessionClient");
    public static readonly KnownType Microsoft_Azure_ServiceBus_SubscriptionClient = new("Microsoft.Azure.ServiceBus.SubscriptionClient");
    public static readonly KnownType Microsoft_Azure_ServiceBus_TopicClient = new("Microsoft.Azure.ServiceBus.TopicClient");
    public static readonly KnownType Microsoft_Azure_WebJobs_Extensions_DurableTask_IDurableEntityClient = new("Microsoft.Azure.WebJobs.Extensions.DurableTask.IDurableEntityClient");
    public static readonly KnownType Microsoft_Azure_WebJobs_Extensions_DurableTask_IDurableEntityContext = new("Microsoft.Azure.WebJobs.Extensions.DurableTask.IDurableEntityContext");
    public static readonly KnownType Microsoft_Azure_WebJobs_Extensions_DurableTask_IDurableOrchestrationContext = new("Microsoft.Azure.WebJobs.Extensions.DurableTask.IDurableOrchestrationContext");
    public static readonly KnownType Microsoft_Azure_WebJobs_FunctionNameAttribute = new("Microsoft.Azure.WebJobs.FunctionNameAttribute");
    public static readonly KnownType Microsoft_Data_Sqlite_SqliteCommand = new("Microsoft.Data.Sqlite.SqliteCommand");
    public static readonly KnownType Microsoft_EntityFramework_DbContext = new("System.Data.Entity.DbContext");
    public static readonly KnownType Microsoft_EntityFrameworkCore_DbContext = new("Microsoft.EntityFrameworkCore.DbContext");
    public static readonly KnownType Microsoft_EntityFrameworkCore_DbContextOptionsBuilder = new("Microsoft.EntityFrameworkCore.DbContextOptionsBuilder");
    public static readonly KnownType Microsoft_EntityFrameworkCore_DbSet_TEntity = new("Microsoft.EntityFrameworkCore.DbSet", "TEntity");
    public static readonly KnownType Microsoft_EntityFrameworkCore_EntityFrameworkQueryableExtensions = new("Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions");
    public static readonly KnownType Microsoft_EntityFrameworkCore_IDbContextFactory_TContext = new("Microsoft.EntityFrameworkCore.IDbContextFactory", "TContext");
    public static readonly KnownType Microsoft_EntityFrameworkCore_Migrations_Migration = new("Microsoft.EntityFrameworkCore.Migrations.Migration");
    public static readonly KnownType Microsoft_EntityFrameworkCore_MySQLDbContextOptionsExtensions = new("Microsoft.EntityFrameworkCore.MySQLDbContextOptionsExtensions");
    public static readonly KnownType Microsoft_EntityFrameworkCore_NpgsqlDbContextOptionsExtensions = new("Microsoft.EntityFrameworkCore.NpgsqlDbContextOptionsExtensions");
    public static readonly KnownType Microsoft_EntityFrameworkCore_NpgsqlDbContextOptionsBuilderExtensions = new("Microsoft.EntityFrameworkCore.NpgsqlDbContextOptionsBuilderExtensions");
    public static readonly KnownType Microsoft_EntityFrameworkCore_OracleDbContextOptionsExtensions = new("Microsoft.EntityFrameworkCore.OracleDbContextOptionsExtensions");
    public static readonly KnownType Microsoft_EntityFrameworkCore_RawSqlString = new("Microsoft.EntityFrameworkCore.RawSqlString");
    public static readonly KnownType Microsoft_EntityFrameworkCore_RelationalDatabaseFacadeExtensions = new("Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions");
    public static readonly KnownType Microsoft_EntityFrameworkCore_RelationalQueryableExtensions = new("Microsoft.EntityFrameworkCore.RelationalQueryableExtensions");
    public static readonly KnownType Microsoft_EntityFrameworkCore_SqliteDbContextOptionsBuilderExtensions = new("Microsoft.EntityFrameworkCore.SqliteDbContextOptionsBuilderExtensions");
    public static readonly KnownType Microsoft_EntityFrameworkCore_SqlServerDbContextOptionsExtensions = new("Microsoft.EntityFrameworkCore.SqlServerDbContextOptionsExtensions");
    public static readonly KnownType Microsoft_Extensions_DependencyInjection_LoggingServiceCollectionExtensions = new("Microsoft.Extensions.DependencyInjection.LoggingServiceCollectionExtensions");
    public static readonly KnownType Microsoft_Extensions_Hosting_HostEnvironmentEnvExtensions = new("Microsoft.Extensions.Hosting.HostEnvironmentEnvExtensions");
    public static readonly KnownType Microsoft_Extensions_Configuration_IConfiguration = new("Microsoft.Extensions.Configuration.IConfiguration");
    public static readonly KnownType Microsoft_Extensions_Logging_AzureAppServicesLoggerFactoryExtensions = new("Microsoft.Extensions.Logging.AzureAppServicesLoggerFactoryExtensions");
    public static readonly KnownType Microsoft_Extensions_Logging_ConsoleLoggerExtensions = new("Microsoft.Extensions.Logging.ConsoleLoggerExtensions");
    public static readonly KnownType Microsoft_Extensions_Logging_DebugLoggerFactoryExtensions = new("Microsoft.Extensions.Logging.DebugLoggerFactoryExtensions");
    public static readonly KnownType Microsoft_Extensions_Logging_EventLoggerFactoryExtensions = new("Microsoft.Extensions.Logging.EventLoggerFactoryExtensions");
    public static readonly KnownType Microsoft_Extensions_Logging_EventSourceLoggerFactoryExtensions = new("Microsoft.Extensions.Logging.EventSourceLoggerFactoryExtensions");
    public static readonly KnownType Microsoft_Extensions_Logging_EventId = new("Microsoft.Extensions.Logging.EventId");
    public static readonly KnownType Microsoft_Extensions_Logging_ILogger = new("Microsoft.Extensions.Logging.ILogger");
    public static readonly KnownType Microsoft_Extensions_Logging_ILogger_TCategoryName = new("Microsoft.Extensions.Logging.ILogger", "TCategoryName");
    public static readonly KnownType Microsoft_Extensions_Logging_ILoggerFactory = new("Microsoft.Extensions.Logging.ILoggerFactory");
    public static readonly KnownType Microsoft_Extensions_Logging_LoggerExtensions = new("Microsoft.Extensions.Logging.LoggerExtensions");
    public static readonly KnownType Microsoft_Extensions_Logging_LoggerFactoryExtensions = new("Microsoft.Extensions.Logging.LoggerFactoryExtensions");
    public static readonly KnownType Microsoft_Extensions_Logging_LogLevel = new("Microsoft.Extensions.Logging.LogLevel");
    public static readonly KnownType Microsoft_Extensions_Primitives_StringValues = new("Microsoft.Extensions.Primitives.StringValues");
    public static readonly KnownType Microsoft_IdentityModel_Tokens_SymmetricSecurityKey = new("Microsoft.IdentityModel.Tokens.SymmetricSecurityKey");
    public static readonly KnownType Microsoft_Net_Http_Headers_HeaderNames = new("Microsoft.Net.Http.Headers.HeaderNames");
    public static readonly KnownType Microsoft_JSInterop_JSInvokable = new("Microsoft.JSInterop.JSInvokableAttribute");
    public static readonly KnownType Microsoft_VisualBasic_Information = new("Microsoft.VisualBasic.Information");
    public static readonly KnownType Microsoft_VisualBasic_Interaction = new("Microsoft.VisualBasic.Interaction");
    public static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_Assert = new("Microsoft.VisualStudio.TestTools.UnitTesting.Assert");
    public static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_AssertFailedException = new("Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException");
    public static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_ExpectedExceptionBaseAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.ExpectedExceptionBaseAttribute");
    public static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_ExpectedExceptionAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.ExpectedExceptionAttribute");
    public static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_IgnoreAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute");
    public static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_TestClassAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute");
    public static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_TestMethodAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute");
    public static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_DataTestMethodAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.DataTestMethodAttribute");
    public static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_WorkItemAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.WorkItemAttribute");
    public static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_AssemblyCleanupAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.AssemblyCleanupAttribute");
    public static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_AssemblyInitializeAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.AssemblyInitializeAttribute");
    public static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_ClassCleanupAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute");
    public static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_ClassInitializeAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute");
    public static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_TestCleanupAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute");
    public static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_TestInitializeAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute");
    public static readonly KnownType Microsoft_Web_XmlTransform_XmlFileInfoDocument = new("Microsoft.Web.XmlTransform.XmlFileInfoDocument");
    public static readonly KnownType Microsoft_Web_XmlTransform_XmlTransformableDocument = new("Microsoft.Web.XmlTransform.XmlTransformableDocument");
    public static readonly KnownType MongoDB_Driver_IMongoCollectionExtensions = new("MongoDB.Driver.IMongoCollectionExtensions");
    public static readonly KnownType Mono_Data_Sqlite_SqliteCommand = new("Mono.Data.Sqlite.SqliteCommand");
    public static readonly KnownType Mono_Data_Sqlite_SqliteDataAdapter = new("Mono.Data.Sqlite.SqliteDataAdapter");
    public static readonly KnownType Mono_Unix_FileAccessPermissions = new("Mono.Unix.FileAccessPermissions");
    public static readonly KnownType MySql_Data_MySqlClient_MySqlDataAdapter = new("MySql.Data.MySqlClient.MySqlDataAdapter");
    public static readonly KnownType MySql_Data_MySqlClient_MySqlCommand = new("MySql.Data.MySqlClient.MySqlCommand");
    public static readonly KnownType MySql_Data_MySqlClient_MySqlHelper = new("MySql.Data.MySqlClient.MySqlHelper");
    public static readonly KnownType MySql_Data_MySqlClient_MySqlScript = new("MySql.Data.MySqlClient.MySqlScript");
    public static readonly KnownType Nancy_Cookies_NancyCookie = new("Nancy.Cookies.NancyCookie");
    public static readonly KnownType NFluent_Check = new("NFluent.Check");
    public static readonly KnownType NFluent_FluentCheckException = new("NFluent.FluentCheckException");
    public static readonly KnownType NFluent_Kernel_FluentCheckException = new("NFluent.Kernel.FluentCheckException");
    public static readonly KnownType NHibernate_ISession = new("NHibernate.ISession");
    public static readonly KnownType NHibernate_Impl_AbstractSessionImpl = new("NHibernate.Impl.AbstractSessionImpl");
    public static readonly KnownType NLog_ILogger = new("NLog.ILogger");
    public static readonly KnownType NLog_ILoggerBase = new("NLog.ILoggerBase");
    public static readonly KnownType NLog_ILoggerExtensions = new("NLog.ILoggerExtensions");
    public static readonly KnownType NLog_LogLevel = new("NLog.LogLevel");
    public static readonly KnownType NLog_LogFactory = new("NLog.LogFactory");
    public static readonly KnownType NLog_LogManager = new("NLog.LogManager");
    public static readonly KnownType NLog_Logger = new("NLog.Logger");
    public static readonly KnownType Newtonsoft_Json_JsonPropertyAttribute = new("Newtonsoft.Json.JsonPropertyAttribute");
    public static readonly KnownType Newtonsoft_Json_JsonRequiredAttribute = new("Newtonsoft.Json.JsonRequiredAttribute");
    public static readonly KnownType Newtonsoft_Json_JsonIgnoreAttribute = new("Newtonsoft.Json.JsonIgnoreAttribute");
    public static readonly KnownType NUnit_Framework_Assert = new("NUnit.Framework.Assert");
    public static readonly KnownType NUnit_Framework_AssertionException = new("NUnit.Framework.AssertionException");
    public static readonly KnownType NUnit_Framework_ExpectedExceptionAttribute = new("NUnit.Framework.ExpectedExceptionAttribute");
    public static readonly KnownType NUnit_Framework_IgnoreAttribute = new("NUnit.Framework.IgnoreAttribute");
    public static readonly KnownType NUnit_Framework_ITestBuilderInterface = new("NUnit.Framework.Interfaces.ITestBuilder");
    public static readonly KnownType NUnit_Framework_TestAttribute = new("NUnit.Framework.TestAttribute");
    public static readonly KnownType NUnit_Framework_TestCaseAttribute = new("NUnit.Framework.TestCaseAttribute");
    public static readonly KnownType NUnit_Framework_TestCaseSourceAttribute = new("NUnit.Framework.TestCaseSourceAttribute");
    public static readonly KnownType NUnit_Framework_TestFixtureAttribute = new("NUnit.Framework.TestFixtureAttribute");
    public static readonly KnownType NUnit_Framework_TheoryAttribute = new("NUnit.Framework.TheoryAttribute");
    public static readonly KnownType Org_BouncyCastle_Asn1_Nist_NistNamedCurves = new("Org.BouncyCastle.Asn1.Nist.NistNamedCurves");
    public static readonly KnownType Org_BouncyCastle_Asn1_Sec_SecNamedCurves = new("Org.BouncyCastle.Asn1.Sec.SecNamedCurves");
    public static readonly KnownType Org_BouncyCastle_Asn1_TeleTrust_TeleTrusTNamedCurves = new("Org.BouncyCastle.Asn1.TeleTrust.TeleTrusTNamedCurves");
    public static readonly KnownType Org_BouncyCastle_Asn1_X9_ECNamedCurveTable = new("Org.BouncyCastle.Asn1.X9.ECNamedCurveTable");
    public static readonly KnownType Org_BouncyCastle_Asn1_X9_X962NamedCurves = new("Org.BouncyCastle.Asn1.X9.X962NamedCurves");
    public static readonly KnownType Org_BouncyCastle_Crypto_Engines_AesFastEngine = new("Org.BouncyCastle.Crypto.Engines.AesFastEngine");
    public static readonly KnownType Org_BouncyCastle_Crypto_Generators_BCrypt = new("Org.BouncyCastle.Crypto.Generators.BCrypt");
    public static readonly KnownType Org_BouncyCastle_Crypto_Generators_SCrypt = new("Org.BouncyCastle.Crypto.Generators.SCrypt");
    public static readonly KnownType Org_BouncyCastle_Crypto_Generators_DHParametersGenerator = new("Org.BouncyCastle.Crypto.Generators.DHParametersGenerator");
    public static readonly KnownType Org_BouncyCastle_Crypto_Generators_OpenBsdBCrypt = new("Org.BouncyCastle.Crypto.Generators.OpenBsdBCrypt");
    public static readonly KnownType Org_BouncyCastle_Crypto_Generators_DsaParametersGenerator = new("Org.BouncyCastle.Crypto.Generators.DsaParametersGenerator");
    public static readonly KnownType Org_BouncyCastle_Crypto_Parameters_RsaKeyGenerationParameters = new("Org.BouncyCastle.Crypto.Parameters.RsaKeyGenerationParameters");
    public static readonly KnownType Org_BouncyCastle_Crypto_PbeParametersGenerator = new("Org.BouncyCastle.Crypto.PbeParametersGenerator");
    public static readonly KnownType Org_BouncyCastle_Crypto_Prng_DigestRandomGenerator = new("Org.BouncyCastle.Crypto.Prng.DigestRandomGenerator");
    public static readonly KnownType Org_BouncyCastle_Crypto_Prng_IRandomGenerator = new("Org.BouncyCastle.Crypto.Prng.IRandomGenerator");
    public static readonly KnownType Org_BouncyCastle_Crypto_Prng_VmpcRandomGenerator = new("Org.BouncyCastle.Crypto.Prng.VmpcRandomGenerator");
    public static readonly KnownType Org_BouncyCastle_Security_SecureRandom = new("Org.BouncyCastle.Security.SecureRandom");
    public static readonly KnownType Serilog_Events_LogEventLevel = new("Serilog.Events.LogEventLevel");
    public static readonly KnownType Serilog_ILogger = new("Serilog.ILogger");
    public static readonly KnownType Serilog_LoggerConfiguration = new("Serilog.LoggerConfiguration");
    public static readonly KnownType Serilog_Log = new("Serilog.Log");
    public static readonly KnownType ServiceStack_OrmLite_OrmLiteReadApi = new("ServiceStack.OrmLite.OrmLiteReadApi");
    public static readonly KnownType ServiceStack_OrmLite_OrmLiteReadApiAsync = new("ServiceStack.OrmLite.OrmLiteReadApiAsync");
    public static readonly KnownType System_Action = new("System.Action");
    public static readonly KnownType System_Action_T = new("System.Action", "T");
    public static readonly KnownType System_Action_T1_T2 = new("System.Action", "T1", "T2");
    public static readonly KnownType System_Action_T1_T2_T3 = new("System.Action", "T1", "T2", "T3");
    public static readonly KnownType System_Action_T1_T2_T3_T4 = new("System.Action", "T1", "T2", "T3", "T4");
    public static readonly KnownType System_Activator = new("System.Activator");
    public static readonly KnownType System_ApplicationException = new("System.ApplicationException");
    public static readonly KnownType System_AppDomain = new("System.AppDomain");
    public static readonly KnownType System_ArgumentException = new("System.ArgumentException");
    public static readonly KnownType System_ArgumentNullException = new("System.ArgumentNullException");
    public static readonly KnownType System_ArgumentOutOfRangeException = new("System.ArgumentOutOfRangeException");
    public static readonly KnownType System_Array = new("System.Array");
    public static readonly KnownType System_Attribute = new("System.Attribute");
    public static readonly KnownType System_AttributeUsageAttribute = new("System.AttributeUsageAttribute");
    public static readonly KnownType System_Boolean = new("System.Boolean");
    public static readonly KnownType System_Byte = new("System.Byte");
    public static readonly KnownType System_Byte_Array = new("System.Byte") { IsArray = true };
    public static readonly KnownType System_Char = new("System.Char");
    public static readonly KnownType System_Char_Array = new("System.Char") { IsArray = true };
    public static readonly KnownType System_Convert = new("System.Convert");
    public static readonly KnownType System_CLSCompliantAttribute = new("System.CLSCompliantAttribute");
    public static readonly KnownType System_CodeDom_Compiler_GeneratedCodeAttribute = new("System.CodeDom.Compiler.GeneratedCodeAttribute");
    public static readonly KnownType System_Collections_CollectionBase = new("System.Collections.CollectionBase");
    public static readonly KnownType System_Collections_Concurrent_ConcurrentDictionary_TKey_TValue = new("System.Collections.Concurrent.ConcurrentDictionary", "TKey", "TValue");
    public static readonly KnownType System_Collections_DictionaryBase = new("System.Collections.DictionaryBase");
    public static readonly KnownType System_Collections_Frozen_FrozenDictionary_TKey_TValue = new("System.Collections.Frozen.FrozenDictionary", "TKey", "TValue");
    public static readonly KnownType System_Collections_Frozen_FrozenSet = new("System.Collections.Frozen.FrozenSet");
    public static readonly KnownType System_Collections_Frozen_FrozenSet_T = new("System.Collections.Frozen.FrozenSet", "T");
    public static readonly KnownType System_Collections_Generic_Comparer_T = new("System.Collections.Generic.Comparer", "T");
    public static readonly KnownType System_Collections_Generic_Dictionary_TKey_TValue = new("System.Collections.Generic.Dictionary", "TKey", "TValue");
    public static readonly KnownType System_Collections_Generic_HashSet_T = new("System.Collections.Generic.HashSet", "T");
    public static readonly KnownType System_Collections_Generic_IAsyncEnumerable_T = new("System.Collections.Generic.IAsyncEnumerable", "T");
    public static readonly KnownType System_Collections_Generic_ICollection_T = new("System.Collections.Generic.ICollection", "T");
    public static readonly KnownType System_Collections_Generic_IDictionary_TKey_TValue = new("System.Collections.Generic.IDictionary", "TKey", "TValue");
    public static readonly KnownType System_Collections_Generic_IEnumerable_T = new("System.Collections.Generic.IEnumerable", "T");
    public static readonly KnownType System_Collections_Generic_IList_T = new("System.Collections.Generic.IList", "T");
    public static readonly KnownType System_Collections_Generic_IReadOnlyCollection_T = new("System.Collections.Generic.IReadOnlyCollection", "T");
    public static readonly KnownType System_Collections_Generic_IReadOnlyList_T = new("System.Collections.Generic.IReadOnlyList", "T");
    public static readonly KnownType System_Collections_Generic_ISet_T = new("System.Collections.Generic.ISet", "T");
    public static readonly KnownType System_Collections_Generic_KeyValuePair_TKey_TValue = new("System.Collections.Generic.KeyValuePair", "TKey", "TValue");
    public static readonly KnownType System_Collections_Generic_List_T = new("System.Collections.Generic.List", "T");
    public static readonly KnownType System_Collections_Generic_Queue_T = new("System.Collections.Generic.Queue", "T");
    public static readonly KnownType System_Collections_Generic_SortedSet_T = new("System.Collections.Generic.SortedSet", "T");
    public static readonly KnownType System_Collections_Generic_Stack_T = new("System.Collections.Generic.Stack", "T");
    public static readonly KnownType System_Collections_Generic_LinkedList_T = new("System.Collections.Generic.LinkedList", "T");
    public static readonly KnownType System_Collections_Generic_OrderedDictionary_TKey_TValue = new("System.Collections.Generic.OrderedDictionary", "TKey", "TValue");
    public static readonly KnownType System_Collections_ICollection = new("System.Collections.ICollection");
    public static readonly KnownType System_Collections_IEnumerable = new("System.Collections.IEnumerable");
    public static readonly KnownType System_Collections_IList = new("System.Collections.IList");
    public static readonly KnownType System_Collections_Immutable_IImmutableArray_T = new("System.Collections.Immutable.IImmutableArray", "T");
    public static readonly KnownType System_Collections_Immutable_IImmutableDictionary_TKey_TValue = new("System.Collections.Immutable.IImmutableDictionary", "TKey", "TValue");
    public static readonly KnownType System_Collections_Immutable_IImmutableList_T = new("System.Collections.Immutable.IImmutableList", "T");
    public static readonly KnownType System_Collections_Immutable_IImmutableQueue_T = new("System.Collections.Immutable.IImmutableQueue", "T");
    public static readonly KnownType System_Collections_Immutable_IImmutableSet_T = new("System.Collections.Immutable.IImmutableSet", "T");
    public static readonly KnownType System_Collections_Immutable_IImmutableStack_T = new("System.Collections.Immutable.IImmutableStack", "T");
    public static readonly KnownType System_Collections_Immutable_ImmutableArray = new("System.Collections.Immutable.ImmutableArray");
    public static readonly KnownType System_Collections_Immutable_ImmutableArray_T = new("System.Collections.Immutable.ImmutableArray", "T");
    public static readonly KnownType System_Collections_Immutable_ImmutableDictionary = new("System.Collections.Immutable.ImmutableDictionary");
    public static readonly KnownType System_Collections_Immutable_ImmutableDictionary_TKey_TValue = new("System.Collections.Immutable.ImmutableDictionary", "TKey", "TValue");
    public static readonly KnownType System_Collections_Immutable_ImmutableHashSet = new("System.Collections.Immutable.ImmutableHashSet");
    public static readonly KnownType System_Collections_Immutable_ImmutableHashSet_T = new("System.Collections.Immutable.ImmutableHashSet", "T");
    public static readonly KnownType System_Collections_Immutable_ImmutableList = new("System.Collections.Immutable.ImmutableList");
    public static readonly KnownType System_Collections_Immutable_ImmutableList_T = new("System.Collections.Immutable.ImmutableList", "T");
    public static readonly KnownType System_Collections_Immutable_ImmutableQueue = new("System.Collections.Immutable.ImmutableQueue");
    public static readonly KnownType System_Collections_Immutable_ImmutableQueue_T = new("System.Collections.Immutable.ImmutableQueue", "T");
    public static readonly KnownType System_Collections_Immutable_ImmutableSortedDictionary = new("System.Collections.Immutable.ImmutableSortedDictionary");
    public static readonly KnownType System_Collections_Immutable_ImmutableSortedDictionary_TKey_TValue = new("System.Collections.Immutable.ImmutableSortedDictionary", "TKey", "TValue");
    public static readonly KnownType System_Collections_Immutable_ImmutableSortedSet = new("System.Collections.Immutable.ImmutableSortedSet");
    public static readonly KnownType System_Collections_Immutable_ImmutableSortedSet_T = new("System.Collections.Immutable.ImmutableSortedSet", "T");
    public static readonly KnownType System_Collections_Immutable_ImmutableStack = new("System.Collections.Immutable.ImmutableStack");
    public static readonly KnownType System_Collections_Immutable_ImmutableStack_T = new("System.Collections.Immutable.ImmutableStack", "T");
    public static readonly KnownType System_Collections_ObjectModel_Collection_T = new("System.Collections.ObjectModel.Collection", "T");
    public static readonly KnownType System_Collections_ObjectModel_ObservableCollection_T = new("System.Collections.ObjectModel.ObservableCollection", "T");
    public static readonly KnownType System_Collections_ObjectModel_ReadOnlyCollection_T = new("System.Collections.ObjectModel.ReadOnlyCollection", "T");
    public static readonly KnownType System_Collections_ObjectModel_ReadOnlyDictionary_TKey_TValue = new("System.Collections.ObjectModel.ReadOnlyDictionary", "TKey", "TValue");
    public static readonly KnownType System_Collections_ObjectModel_ReadOnlySet_T = new("System.Collections.ObjectModel.ReadOnlySet", "T");
    public static readonly KnownType System_Collections_Queue = new("System.Collections.Queue");
    public static readonly KnownType System_Collections_ReadOnlyCollectionBase = new("System.Collections.ReadOnlyCollectionBase");
    public static readonly KnownType System_Collections_SortedList = new("System.Collections.SortedList");
    public static readonly KnownType System_Collections_Stack = new("System.Collections.Stack");
    public static readonly KnownType System_Collections_Specialized_NameObjectCollectionBase = new("System.Collections.Specialized.NameObjectCollectionBase");
    public static readonly KnownType System_Collections_Specialized_NameValueCollection = new("System.Collections.Specialized.NameValueCollection");
    public static readonly KnownType System_Composition_ExportAttribute = new("System.Composition.ExportAttribute");
    public static readonly KnownType System_ComponentModel_Composition_CreationPolicy = new("System.ComponentModel.Composition.CreationPolicy");
    public static readonly KnownType System_ComponentModel_Composition_ExportAttribute = new("System.ComponentModel.Composition.ExportAttribute");
    public static readonly KnownType System_ComponentModel_Composition_InheritedExportAttribute = new("System.ComponentModel.Composition.InheritedExportAttribute");
    public static readonly KnownType System_ComponentModel_Composition_PartCreationPolicyAttribute = new("System.ComponentModel.Composition.PartCreationPolicyAttribute");
    public static readonly KnownType System_ComponentModel_DataAnnotations_KeyAttribute = new("System.ComponentModel.DataAnnotations.KeyAttribute");
    public static readonly KnownType System_ComponentModel_DataAnnotations_RegularExpressionAttribute = new("System.ComponentModel.DataAnnotations.RegularExpressionAttribute");
    public static readonly KnownType System_ComponentModel_DataAnnotations_RangeAttribute = new("System.ComponentModel.DataAnnotations.RangeAttribute");
    public static readonly KnownType System_ComponentModel_DataAnnotations_IValidatableObject = new("System.ComponentModel.DataAnnotations.IValidatableObject");
    public static readonly KnownType System_ComponentModel_DataAnnotations_RequiredAttribute = new("System.ComponentModel.DataAnnotations.RequiredAttribute");
    public static readonly KnownType System_ComponentModel_DataAnnotations_ValidationAttribute = new("System.ComponentModel.DataAnnotations.ValidationAttribute");
    public static readonly KnownType System_ComponentModel_DefaultValueAttribute = new("System.ComponentModel.DefaultValueAttribute");
    public static readonly KnownType System_ComponentModel_EditorBrowsableAttribute = new("System.ComponentModel.EditorBrowsableAttribute");
    public static readonly KnownType System_ComponentModel_LocalizableAttribute = new("System.ComponentModel.LocalizableAttribute");
    public static readonly KnownType System_Configuration_ConfigXmlDocument = new("System.Configuration.ConfigXmlDocument");
    public static readonly KnownType System_Configuration_ConfigurationManager = new("System.Configuration.ConfigurationManager");
    public static readonly KnownType System_Console = new("System.Console");
    public static readonly KnownType System_Data_Common_CommandTrees_DbExpression = new("System.Data.Common.CommandTrees.DbExpression");
    public static readonly KnownType System_Data_DataSet = new("System.Data.DataSet");
    public static readonly KnownType System_Data_DataTable = new("System.Data.DataTable");
    public static readonly KnownType System_Data_Entity_Core_Objects_ObjectQuery = new("System.Data.Entity.Core.Objects.ObjectQuery");
    public static readonly KnownType System_Data_Entity_Database = new("System.Data.Entity.Database");
    public static readonly KnownType System_Data_Entity_DbSet = new("System.Data.Entity.DbSet");
    public static readonly KnownType System_Data_Entity_DbSet_TEntity = new("System.Data.Entity.DbSet", "TEntity");
    public static readonly KnownType System_Data_Entity_Infrastructure_DbQuery = new("System.Data.Entity.Infrastructure.DbQuery");
    public static readonly KnownType System_Data_Entity_Infrastructure_DbQuery_TResult = new("System.Data.Entity.Infrastructure.DbQuery", "TResult");
    public static readonly KnownType System_Data_IDbCommand = new("System.Data.IDbCommand");
    public static readonly KnownType System_Data_Linq_ITable = new("System.Data.Linq.ITable");
    public static readonly KnownType System_Data_Odbc_OdbcCommand = new("System.Data.Odbc.OdbcCommand");
    public static readonly KnownType System_Data_Odbc_OdbcDataAdapter = new("System.Data.Odbc.OdbcDataAdapter");
    public static readonly KnownType System_Data_OracleClient_OracleCommand = new("System.Data.OracleClient.OracleCommand");
    public static readonly KnownType System_Data_OracleClient_OracleDataAdapter = new("System.Data.OracleClient.OracleDataAdapter");
    public static readonly KnownType System_Data_SqlClient_SqlCommand = new("System.Data.SqlClient.SqlCommand");
    public static readonly KnownType System_Data_SqlClient_SqlDataAdapter = new("System.Data.SqlClient.SqlDataAdapter");
    public static readonly KnownType System_Data_Sqlite_SqliteCommand = new("System.Data.SQLite.SQLiteCommand");
    public static readonly KnownType System_Data_Sqlite_SQLiteDataAdapter = new("System.Data.SQLite.SQLiteDataAdapter");
    public static readonly KnownType System_Data_SqlServerCe_SqlCeCommand = new("System.Data.SqlServerCe.SqlCeCommand");
    public static readonly KnownType System_Data_SqlServerCe_SqlCeDataAdapter = new("System.Data.SqlServerCe.SqlCeDataAdapter");
    public static readonly KnownType System_DateOnly = new("System.DateOnly");
    public static readonly KnownType System_DateTime = new("System.DateTime");
    public static readonly KnownType System_DateTimeKind = new("System.DateTimeKind");
    public static readonly KnownType System_DateTimeOffset = new("System.DateTimeOffset");
    public static readonly KnownType System_Decimal = new("System.Decimal");
    public static readonly KnownType System_Delegate = new("System.Delegate");
    public static readonly KnownType System_Diagnostics_CodeAnalysis_DynamicallyAccessedMembersAttribute = new("System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembersAttribute");
    public static readonly KnownType System_Diagnostics_CodeAnalysis_ExcludeFromCodeCoverageAttribute = new("System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute");
    public static readonly KnownType System_Diagnostics_CodeAnalysis_SuppressMessageAttribute = new("System.Diagnostics.CodeAnalysis.SuppressMessageAttribute");
    public static readonly KnownType System_Diagnostics_CodeAnalysis_StringSyntaxAttribute = new("System.Diagnostics.CodeAnalysis.StringSyntaxAttribute");
    public static readonly KnownType System_Diagnostics_ConditionalAttribute = new("System.Diagnostics.ConditionalAttribute");
    public static readonly KnownType System_Diagnostics_Contracts_PureAttribute = new("System.Diagnostics.Contracts.PureAttribute");
    public static readonly KnownType System_Diagnostics_Debug = new("System.Diagnostics.Debug");
    public static readonly KnownType System_Diagnostics_Debugger = new("System.Diagnostics.Debugger");
    public static readonly KnownType System_Diagnostics_DebuggerDisplayAttribute = new("System.Diagnostics.DebuggerDisplayAttribute");
    public static readonly KnownType System_Diagnostics_Process = new("System.Diagnostics.Process");
    public static readonly KnownType System_Diagnostics_ProcessStartInfo = new("System.Diagnostics.ProcessStartInfo");
    public static readonly KnownType System_Diagnostics_Trace = new("System.Diagnostics.Trace");
    public static readonly KnownType System_Diagnostics_TraceSource = new("System.Diagnostics.TraceSource");
    public static readonly KnownType System_Diagnostics_TraceSwitch = new("System.Diagnostics.TraceSwitch");
    public static readonly KnownType System_DirectoryServices_AuthenticationTypes = new("System.DirectoryServices.AuthenticationTypes");
    public static readonly KnownType System_DirectoryServices_DirectoryEntry = new("System.DirectoryServices.DirectoryEntry");
    public static readonly KnownType System_Double = new("System.Double");
    public static readonly KnownType System_Drawing_Bitmap = new("System.Drawing.Bitmap");
    public static readonly KnownType System_Drawing_Image = new("System.Drawing.Image");
    public static readonly KnownType System_DuplicateWaitObjectException = new("System.DuplicateWaitObjectException");
    public static readonly KnownType System_Enum = new("System.Enum");
    public static readonly KnownType System_Environment = new("System.Environment");
    public static readonly KnownType System_EventArgs = new("System.EventArgs");
    public static readonly KnownType System_EventHandler = new("System.EventHandler");
    public static readonly KnownType System_EventHandler_TEventArgs = new("System.EventHandler", "TEventArgs");
    public static readonly KnownType System_Exception = new("System.Exception");
    public static readonly KnownType System_ExecutionEngineException = new("System.ExecutionEngineException");
    public static readonly KnownType System_FlagsAttribute = new("System.FlagsAttribute");
    public static readonly KnownType System_FormattableString = new("System.FormattableString");
    public static readonly KnownType System_Func_TResult = new("System.Func", "TResult");
    public static readonly KnownType System_Func_T_TResult = new("System.Func", "T", "TResult");
    public static readonly KnownType System_Func_T1_T2_TResult = new("System.Func", "T1", "T2", "TResult");
    public static readonly KnownType System_Func_T1_T2_T3_TResult = new("System.Func", "T1", "T2", "T3", "TResult");
    public static readonly KnownType System_Func_T1_T2_T3_T4_TResult = new("System.Func", "T1", "T2", "T3", "T4", "TResult");
    public static readonly KnownType System_GC = new("System.GC");
    public static readonly KnownType System_Globalization_CompareOptions = new("System.Globalization.CompareOptions");
    public static readonly KnownType System_Globalization_CultureInfo = new("System.Globalization.CultureInfo");
    public static readonly KnownType System_Guid = new("System.Guid");
    public static readonly KnownType System_Half = new("System.Half");
    public static readonly KnownType System_IAsyncDisposable = new("System.IAsyncDisposable");
    public static readonly KnownType System_IComparable = new("System.IComparable");
    public static readonly KnownType System_IComparable_T = new("System.IComparable", "T");
    public static readonly KnownType System_IdentityModel_Tokens_SecurityTokenHandler = new("System.IdentityModel.Tokens.SecurityTokenHandler");
    public static readonly KnownType System_IdentityModel_Tokens_SymmetricSecurityKey = new("System.IdentityModel.Tokens.SymmetricSecurityKey");
    public static readonly KnownType System_IDisposable = new("System.IDisposable");
    public static readonly KnownType System_IEquatable_T = new("System.IEquatable", "T");
    public static readonly KnownType System_IFormatProvider = new("System.IFormatProvider");
    public static readonly KnownType System_Index = new("System.Index");
    public static readonly KnownType System_IndexOutOfRangeException = new("System.IndexOutOfRangeException");
    public static readonly KnownType System_Int16 = new("System.Int16");
    public static readonly KnownType System_Int32 = new("System.Int32");
    public static readonly KnownType System_Int64 = new("System.Int64");
    public static readonly KnownType System_IntPtr = new("System.IntPtr");
    public static readonly KnownType System_InvalidOperationException = new("System.InvalidOperationException");
    public static readonly KnownType System_IO_Compression_ZipFile = new("System.IO.Compression.ZipFile");
    public static readonly KnownType System_IO_Compression_ZipFileExtensions = new("System.IO.Compression.ZipFileExtensions");
    public static readonly KnownType System_IO_FileStream = new("System.IO.FileStream");
    public static readonly KnownType System_IO_Path = new("System.IO.Path");
    public static readonly KnownType System_IO_Stream = new("System.IO.Stream");
    public static readonly KnownType System_IO_StreamReader = new("System.IO.StreamReader");
    public static readonly KnownType System_IO_StreamWriter = new("System.IO.StreamWriter");
    public static readonly KnownType System_IO_TextWriter = new("System.IO.TextWriter");
    public static readonly KnownType System_Lazy = new("System.Lazy", "T");
    public static readonly KnownType System_Linq_Enumerable = new("System.Linq.Enumerable");
    public static readonly KnownType System_Linq_Expressions_Expression = new("System.Linq.Expressions.Expression");
    public static readonly KnownType System_Linq_Expressions_Expression_T = new("System.Linq.Expressions.Expression", "TDelegate");
    public static readonly KnownType System_Linq_ImmutableArrayExtensions = new("System.Linq.ImmutableArrayExtensions");
    public static readonly KnownType System_Linq_IQueryable = new("System.Linq.IQueryable", "T");
    public static readonly KnownType System_Linq_Queryable = new("System.Linq.Queryable");
    public static readonly KnownType System_MarshalByRefObject = new("System.MarshalByRefObject");
    public static readonly KnownType System_MTAThreadAttribute = new("System.MTAThreadAttribute");
    public static readonly KnownType System_Net_FtpWebRequest = new("System.Net.FtpWebRequest");
    public static readonly KnownType System_Net_Http_HttpClient = new("System.Net.Http.HttpClient");
    public static readonly KnownType System_Net_Http_Headers_HttpHeaders = new("System.Net.Http.Headers.HttpHeaders");
    public static readonly KnownType System_Net_Http_HttpClientHandler = new("System.Net.Http.HttpClientHandler");
    public static readonly KnownType System_Net_Mail_SmtpClient = new("System.Net.Mail.SmtpClient");
    public static readonly KnownType System_Net_NetworkCredential = new("System.Net.NetworkCredential");
    public static readonly KnownType System_Net_Security_RemoteCertificateValidationCallback = new("System.Net.Security.RemoteCertificateValidationCallback");
    public static readonly KnownType System_Net_Security_SslPolicyErrors = new("System.Net.Security.SslPolicyErrors");
    public static readonly KnownType System_Net_SecurityProtocolType = new("System.Net.SecurityProtocolType");
    public static readonly KnownType System_Net_Sockets_Socket = new("System.Net.Sockets.Socket");
    public static readonly KnownType System_Net_Sockets_SocketTaskExtensions = new("System.Net.Sockets.SocketTaskExtensions");
    public static readonly KnownType System_Net_Sockets_TcpClient = new("System.Net.Sockets.TcpClient");
    public static readonly KnownType System_Net_Sockets_UdpClient = new("System.Net.Sockets.UdpClient");
    public static readonly KnownType System_Net_WebClient = new("System.Net.WebClient");
    public static readonly KnownType System_NonSerializedAttribute = new("System.NonSerializedAttribute");
    public static readonly KnownType System_NotImplementedException = new("System.NotImplementedException");
    public static readonly KnownType System_NotSupportedException = new("System.NotSupportedException");
    public static readonly KnownType System_Nullable_T = new("System.Nullable", "T");
    public static readonly KnownType System_NullReferenceException = new("System.NullReferenceException");
    public static readonly KnownType System_Numerics_IEqualityOperators_TSelf_TOther_TResult = new("System.Numerics.IEqualityOperators", "TSelf", "TOther", "TResult");
    public static readonly KnownType System_Numerics_IFloatingPointIeee754_TSelf = new("System.Numerics.IFloatingPointIeee754", "TSelf");
    public static readonly KnownType System_Object = new("System.Object");
    public static readonly KnownType System_Object_Array = new("System.Object") { IsArray = true };
    public static readonly KnownType System_ObsoleteAttribute = new("System.ObsoleteAttribute");
    public static readonly KnownType System_OutOfMemoryException = new("System.OutOfMemoryException");
    public static readonly KnownType System_Random = new("System.Random");
    public static readonly KnownType System_Range = new("System.Range");
    public static readonly KnownType System_ReadOnlySpan_T = new("System.ReadOnlySpan", "T");
    public static readonly KnownType System_Reflection_Assembly = new("System.Reflection.Assembly");
    public static readonly KnownType System_Reflection_BindingFlags = new("System.Reflection.BindingFlags");
    public static readonly KnownType System_Reflection_AssemblyVersionAttribute = new("System.Reflection.AssemblyVersionAttribute");
    public static readonly KnownType System_Reflection_MemberInfo = new("System.Reflection.MemberInfo");
    public static readonly KnownType System_Reflection_Module = new("System.Reflection.Module");
    public static readonly KnownType System_Reflection_ParameterInfo = new("System.Reflection.ParameterInfo");
    public static readonly KnownType System_Resources_NeutralResourcesLanguageAttribute = new("System.Resources.NeutralResourcesLanguageAttribute");
    public static readonly KnownType System_Runtime_CompilerServices_ExtensionAttribute = new("System.Runtime.CompilerServices.ExtensionAttribute");
    public static readonly KnownType System_Runtime_CompilerServices_CallerArgumentExpressionAttribute = new("System.Runtime.CompilerServices.CallerArgumentExpressionAttribute");
    public static readonly KnownType System_Runtime_CompilerServices_CallerFilePathAttribute = new("System.Runtime.CompilerServices.CallerFilePathAttribute");
    public static readonly KnownType System_Runtime_CompilerServices_CallerLineNumberAttribute = new("System.Runtime.CompilerServices.CallerLineNumberAttribute");
    public static readonly KnownType System_Runtime_CompilerServices_CallerMemberNameAttribute = new("System.Runtime.CompilerServices.CallerMemberNameAttribute");
    public static readonly KnownType System_Runtime_CompilerServices_InternalsVisibleToAttribute = new("System.Runtime.CompilerServices.InternalsVisibleToAttribute");
    public static readonly KnownType System_Runtime_CompilerServices_ModuleInitializerAttribute = new("System.Runtime.CompilerServices.ModuleInitializerAttribute");
    public static readonly KnownType System_Runtime_CompilerServices_ValueTaskAwaiter = new("System.Runtime.CompilerServices.ValueTaskAwaiter");
    public static readonly KnownType System_Runtime_CompilerServices_ValueTaskAwaiter_TResult = new("System.Runtime.CompilerServices.ValueTaskAwaiter", "TResult");
    public static readonly KnownType System_Runtime_CompilerServices_TaskAwaiter = new("System.Runtime.CompilerServices.TaskAwaiter");
    public static readonly KnownType System_Runtime_CompilerServices_TaskAwaiter_TResult = new("System.Runtime.CompilerServices.TaskAwaiter", "TResult");
    public static readonly KnownType System_Runtime_InteropServices_ComImportAttribute = new("System.Runtime.InteropServices.ComImportAttribute");
    public static readonly KnownType System_Runtime_InteropServices_ComVisibleAttribute = new("System.Runtime.InteropServices.ComVisibleAttribute");
    public static readonly KnownType System_Runtime_InteropServices_DefaultParameterValueAttribute = new("System.Runtime.InteropServices.DefaultParameterValueAttribute");
    public static readonly KnownType System_Runtime_InteropServices_DllImportAttribute = new("System.Runtime.InteropServices.DllImportAttribute");
    public static readonly KnownType System_Runtime_InteropServices_Exception = new("System.Runtime.InteropServices._Exception");
    public static readonly KnownType System_Runtime_InteropServices_HandleRef = new("System.Runtime.InteropServices.HandleRef");
    public static readonly KnownType System_Runtime_InteropServices_InterfaceTypeAttribute = new("System.Runtime.InteropServices.InterfaceTypeAttribute");
    public static readonly KnownType System_Runtime_InteropServices_LibraryImportAttribute = new("System.Runtime.InteropServices.LibraryImportAttribute");
    public static readonly KnownType System_Runtime_InteropServices_NFloat = new("System.Runtime.InteropServices.NFloat");
    public static readonly KnownType System_Runtime_InteropServices_OptionalAttribute = new("System.Runtime.InteropServices.OptionalAttribute");
    public static readonly KnownType System_Runtime_InteropServices_SafeHandle = new("System.Runtime.InteropServices.SafeHandle");
    public static readonly KnownType System_Runtime_InteropServices_StructLayoutAttribute = new("System.Runtime.InteropServices.StructLayoutAttribute");
    public static readonly KnownType System_Runtime_Serialization_DataMemberAttribute = new("System.Runtime.Serialization.DataMemberAttribute");
    public static readonly KnownType System_Runtime_Serialization_Formatters_Binary_BinaryFormatter = new("System.Runtime.Serialization.Formatters.Binary.BinaryFormatter");
    public static readonly KnownType System_Runtime_Serialization_Formatters_Soap_SoapFormatter = new("System.Runtime.Serialization.Formatters.Soap.SoapFormatter");
    public static readonly KnownType System_Runtime_Serialization_ISerializable = new("System.Runtime.Serialization.ISerializable");
    public static readonly KnownType System_Runtime_Serialization_IDeserializationCallback = new("System.Runtime.Serialization.IDeserializationCallback");
    public static readonly KnownType System_Runtime_Serialization_NetDataContractSerializer = new("System.Runtime.Serialization.NetDataContractSerializer");
    public static readonly KnownType System_Runtime_Serialization_OnDeserializedAttribute = new("System.Runtime.Serialization.OnDeserializedAttribute");
    public static readonly KnownType System_Runtime_Serialization_OnDeserializingAttribute = new("System.Runtime.Serialization.OnDeserializingAttribute");
    public static readonly KnownType System_Runtime_Serialization_OnSerializedAttribute = new("System.Runtime.Serialization.OnSerializedAttribute");
    public static readonly KnownType System_Runtime_Serialization_OnSerializingAttribute = new("System.Runtime.Serialization.OnSerializingAttribute");
    public static readonly KnownType System_Runtime_Serialization_OptionalFieldAttribute = new("System.Runtime.Serialization.OptionalFieldAttribute");
    public static readonly KnownType System_Runtime_Serialization_SerializationInfo = new("System.Runtime.Serialization.SerializationInfo");
    public static readonly KnownType System_Runtime_Serialization_StreamingContext = new("System.Runtime.Serialization.StreamingContext");
    public static readonly KnownType System_SByte = new("System.SByte");
    public static readonly KnownType System_Security_AccessControl_FileSystemAccessRule = new("System.Security.AccessControl.FileSystemAccessRule");
    public static readonly KnownType System_Security_AccessControl_FileSystemSecurity = new("System.Security.AccessControl.FileSystemSecurity");
    public static readonly KnownType System_Security_AllowPartiallyTrustedCallersAttribute = new("System.Security.AllowPartiallyTrustedCallersAttribute");
    public static readonly KnownType System_Security_Authentication_SslProtocols = new("System.Security.Authentication.SslProtocols");
    public static readonly KnownType System_Security_Cryptography_AesManaged = new("System.Security.Cryptography.AesManaged");
    public static readonly KnownType System_Security_Cryptography_AsymmetricAlgorithm = new("System.Security.Cryptography.AsymmetricAlgorithm");
    public static readonly KnownType System_Security_Cryptography_AsymmetricKeyExchangeDeformatter = new("System.Security.Cryptography.AsymmetricKeyExchangeDeformatter");
    public static readonly KnownType System_Security_Cryptography_AsymmetricKeyExchangeFormatter = new("System.Security.Cryptography.AsymmetricKeyExchangeFormatter");
    public static readonly KnownType System_Security_Cryptography_AsymmetricSignatureDeformatter = new("System.Security.Cryptography.AsymmetricSignatureDeformatter");
    public static readonly KnownType System_Security_Cryptography_AsymmetricSignatureFormatter = new("System.Security.Cryptography.AsymmetricSignatureFormatter");
    public static readonly KnownType System_Security_Cryptography_CryptoConfig = new("System.Security.Cryptography.CryptoConfig");
    public static readonly KnownType System_Security_Cryptography_CspParameters = new("System.Security.Cryptography.CspParameters");
    public static readonly KnownType System_Security_Cryptography_DES = new("System.Security.Cryptography.DES");
    public static readonly KnownType System_Security_Cryptography_DeriveBytes = new("System.Security.Cryptography.DeriveBytes");
    public static readonly KnownType System_Security_Cryptography_DSA = new("System.Security.Cryptography.DSA");
    public static readonly KnownType System_Security_Cryptography_DSACryptoServiceProvider = new("System.Security.Cryptography.DSACryptoServiceProvider");
    public static readonly KnownType System_Security_Cryptography_ECDiffieHellman = new("System.Security.Cryptography.ECDiffieHellman");
    public static readonly KnownType System_Security_Cryptography_ECDsa = new("System.Security.Cryptography.ECDsa");
    public static readonly KnownType System_Security_Cryptography_ECAlgorythm = new("System.Security.Cryptography.ECAlgorithm");
    public static readonly KnownType System_Security_Cryptography_HashAlgorithm = new("System.Security.Cryptography.HashAlgorithm");
    public static readonly KnownType System_Security_Cryptography_HMAC = new("System.Security.Cryptography.HMAC");
    public static readonly KnownType System_Security_Cryptography_HMACMD5 = new("System.Security.Cryptography.HMACMD5");
    public static readonly KnownType System_Security_Cryptography_HMACRIPEMD160 = new("System.Security.Cryptography.HMACRIPEMD160");
    public static readonly KnownType System_Security_Cryptography_HMACSHA1 = new("System.Security.Cryptography.HMACSHA1");
    public static readonly KnownType System_Security_Cryptography_ICryptoTransform = new("System.Security.Cryptography.ICryptoTransform");
    public static readonly KnownType System_Security_Cryptography_KeyedHashAlgorithm = new("System.Security.Cryptography.KeyedHashAlgorithm");
    public static readonly KnownType System_Security_Cryptography_MD5 = new("System.Security.Cryptography.MD5");
    public static readonly KnownType System_Security_Cryptography_PasswordDeriveBytes = new("System.Security.Cryptography.PasswordDeriveBytes");
    public static readonly KnownType System_Security_Cryptography_RC2 = new("System.Security.Cryptography.RC2");
    public static readonly KnownType System_Security_Cryptography_RandomNumberGenerator = new("System.Security.Cryptography.RandomNumberGenerator");
    public static readonly KnownType System_Security_Cryptography_Rfc2898DeriveBytes = new("System.Security.Cryptography.Rfc2898DeriveBytes");
    public static readonly KnownType System_Security_Cryptography_RIPEMD160 = new("System.Security.Cryptography.RIPEMD160");
    public static readonly KnownType System_Security_Cryptography_RNGCryptoServiceProvider = new("System.Security.Cryptography.RNGCryptoServiceProvider");
    public static readonly KnownType System_Security_Cryptography_RSA = new("System.Security.Cryptography.RSA");
    public static readonly KnownType System_Security_Cryptography_RSACryptoServiceProvider = new("System.Security.Cryptography.RSACryptoServiceProvider");
    public static readonly KnownType System_Security_Cryptography_SHA1 = new("System.Security.Cryptography.SHA1");
    public static readonly KnownType System_Security_Cryptography_SymmetricAlgorithm = new("System.Security.Cryptography.SymmetricAlgorithm");
    public static readonly KnownType System_Security_Cryptography_TripleDES = new("System.Security.Cryptography.TripleDES");
    public static readonly KnownType System_Security_Cryptography_X509Certificates_X509Certificate2 = new("System.Security.Cryptography.X509Certificates.X509Certificate2");
    public static readonly KnownType System_Security_Cryptography_X509Certificates_X509Chain = new("System.Security.Cryptography.X509Certificates.X509Chain");
    public static readonly KnownType System_Security_Cryptography_Xml_SignedXml = new("System.Security.Cryptography.Xml.SignedXml");
    public static readonly KnownType System_Security_Permissions_CodeAccessSecurityAttribute = new("System.Security.Permissions.CodeAccessSecurityAttribute");
    public static readonly KnownType System_Security_Permissions_PrincipalPermission = new("System.Security.Permissions.PrincipalPermission");
    public static readonly KnownType System_Security_Permissions_PrincipalPermissionAttribute = new("System.Security.Permissions.PrincipalPermissionAttribute");
    public static readonly KnownType System_Security_PermissionSet = new("System.Security.PermissionSet");
    public static readonly KnownType System_Security_Principal_IIdentity = new("System.Security.Principal.IIdentity");
    public static readonly KnownType System_Security_Principal_IPrincipal = new("System.Security.Principal.IPrincipal");
    public static readonly KnownType System_Security_Principal_NTAccount = new("System.Security.Principal.NTAccount");
    public static readonly KnownType System_Security_Principal_SecurityIdentifier = new("System.Security.Principal.SecurityIdentifier");
    public static readonly KnownType System_Security_Principal_WindowsIdentity = new("System.Security.Principal.WindowsIdentity");
    public static readonly KnownType System_Security_SecureString = new("System.Security.SecureString");
    public static readonly KnownType System_Security_SecurityCriticalAttribute = new("System.Security.SecurityCriticalAttribute");
    public static readonly KnownType System_Security_SecuritySafeCriticalAttribute = new("System.Security.SecuritySafeCriticalAttribute");
    public static readonly KnownType System_SerializableAttribute = new("System.SerializableAttribute");
    public static readonly KnownType System_ServiceModel_OperationContractAttribute = new("System.ServiceModel.OperationContractAttribute");
    public static readonly KnownType System_ServiceModel_ServiceContractAttribute = new("System.ServiceModel.ServiceContractAttribute");
    public static readonly KnownType System_Single = new("System.Single");
    public static readonly KnownType System_StackOverflowException = new("System.StackOverflowException");
    public static readonly KnownType System_STAThreadAttribute = new("System.STAThreadAttribute");
    public static readonly KnownType System_String = new("System.String");
    public static readonly KnownType System_String_Array = new("System.String") { IsArray = true };
    public static readonly KnownType System_StringComparison = new("System.StringComparison");
    public static readonly KnownType System_SystemException = new("System.SystemException");
    public static readonly KnownType System_Text_Encoding = new("System.Text.Encoding");
    public static readonly KnownType System_Text_Json_Serialization_JsonIgnoreAttribute = new("System.Text.Json.Serialization.JsonIgnoreAttribute");
    public static readonly KnownType System_Text_Json_Serialization_JsonRequiredAttribute = new("System.Text.Json.Serialization.JsonRequiredAttribute");
    public static readonly KnownType System_Text_RegularExpressions_Regex = new("System.Text.RegularExpressions.Regex");
    public static readonly KnownType System_Text_RegularExpressions_RegexOptions = new("System.Text.RegularExpressions.RegexOptions");
    public static readonly KnownType System_Text_StringBuilder = new("System.Text.StringBuilder");
    public static readonly KnownType System_Threading_CancellationToken = new("System.Threading.CancellationToken");
    public static readonly KnownType System_Threading_CancellationTokenSource = new("System.Threading.CancellationTokenSource");
    public static readonly KnownType System_Threading_Monitor = new("System.Threading.Monitor");
    public static readonly KnownType System_Threading_Mutex = new("System.Threading.Mutex");
    public static readonly KnownType System_Threading_ReaderWriterLock = new("System.Threading.ReaderWriterLock");
    public static readonly KnownType System_Threading_ReaderWriterLockSlim = new("System.Threading.ReaderWriterLockSlim");
    public static readonly KnownType System_Threading_SpinLock = new("System.Threading.SpinLock");
    public static readonly KnownType System_Threading_Tasks_Task = new("System.Threading.Tasks.Task");
    public static readonly KnownType System_Threading_Tasks_Task_T = new("System.Threading.Tasks.Task", "TResult");
    public static readonly KnownType System_Threading_Tasks_TaskFactory = new("System.Threading.Tasks.TaskFactory");
    public static readonly KnownType System_Threading_Tasks_ValueTask = new("System.Threading.Tasks.ValueTask");
    public static readonly KnownType System_Threading_Tasks_ValueTask_TResult = new("System.Threading.Tasks.ValueTask", "TResult");
    public static readonly KnownType System_Threading_Thread = new("System.Threading.Thread");
    public static readonly KnownType System_Threading_WaitHandle = new("System.Threading.WaitHandle");
    public static readonly KnownType System_ThreadStaticAttribute = new("System.ThreadStaticAttribute");
    public static readonly KnownType System_TimeOnly = new("System.TimeOnly");
    public static readonly KnownType System_TimeSpan = new("System.TimeSpan");
    public static readonly KnownType System_Type = new("System.Type");
    public static readonly KnownType System_UInt16 = new("System.UInt16");
    public static readonly KnownType System_UInt32 = new("System.UInt32");
    public static readonly KnownType System_UInt64 = new("System.UInt64");
    public static readonly KnownType System_UIntPtr = new("System.UIntPtr");
    public static readonly KnownType System_Uri = new("System.Uri");
    public static readonly KnownType System_ValueTuple = new("System.ValueTuple");
    public static readonly KnownType System_ValueType = new("System.ValueType");
    public static readonly KnownType System_Web_HttpApplication = new("System.Web.HttpApplication");
    public static readonly KnownType System_Web_HttpCookie = new("System.Web.HttpCookie");
    public static readonly KnownType System_Web_HttpContext = new("System.Web.HttpContext");
    public static readonly KnownType System_Web_HttpResponse = new("System.Web.HttpResponse");
    public static readonly KnownType System_Web_HttpResponseBase = new("System.Web.HttpResponseBase");
    public static readonly KnownType System_Web_Http_ApiController = new("System.Web.Http.ApiController");
    public static readonly KnownType System_Web_Http_Cors_EnableCorsAttribute = new("System.Web.Http.Cors.EnableCorsAttribute");
    public static readonly KnownType System_Web_Mvc_Controller = new("System.Web.Mvc.Controller");
    public static readonly KnownType System_Web_Mvc_HttpPostAttribute = new("System.Web.Mvc.HttpPostAttribute");
    public static readonly KnownType System_Web_Mvc_NonActionAttribute = new("System.Web.Mvc.NonActionAttribute");
    public static readonly KnownType System_Web_Mvc_RouteAttribute = new("System.Web.Mvc.RouteAttribute");
    public static readonly KnownType System_Web_Mvc_Routing_IRouteInfoProvider = new("System.Web.Mvc.Routing.IRouteInfoProvider");
    public static readonly KnownType System_Web_Mvc_RoutePrefixAttribute = new("System.Web.Mvc.RoutePrefixAttribute");
    public static readonly KnownType System_Web_Mvc_ValidateInputAttribute = new("System.Web.Mvc.ValidateInputAttribute");
    public static readonly KnownType System_Web_Script_Serialization_JavaScriptSerializer = new("System.Web.Script.Serialization.JavaScriptSerializer");
    public static readonly KnownType System_Web_Script_Serialization_JavaScriptTypeResolver = new("System.Web.Script.Serialization.JavaScriptTypeResolver");
    public static readonly KnownType System_Web_Script_Serialization_SimpleTypeResolver = new("System.Web.Script.Serialization.SimpleTypeResolver");
    public static readonly KnownType System_Web_UI_LosFormatter = new("System.Web.UI.LosFormatter");
    public static readonly KnownType System_Windows_DependencyObject = new("System.Windows.DependencyObject");
    public static readonly KnownType System_Windows_Forms_Application = new("System.Windows.Forms.Application");
    public static readonly KnownType System_Windows_Forms_IContainerControl = new("System.Windows.Forms.IContainerControl");
    public static readonly KnownType System_Windows_FrameworkElement = new("System.Windows.FrameworkElement");
    public static readonly KnownType System_Windows_Markup_ConstructorArgumentAttribute = new("System.Windows.Markup.ConstructorArgumentAttribute");
    public static readonly KnownType System_Windows_Markup_XmlnsPrefixAttribute = new("System.Windows.Markup.XmlnsPrefixAttribute");
    public static readonly KnownType System_Windows_Markup_XmlnsDefinitionAttribute = new("System.Windows.Markup.XmlnsDefinitionAttribute");
    public static readonly KnownType System_Windows_Markup_XmlnsCompatibleWithAttribute = new("System.Windows.Markup.XmlnsCompatibleWithAttribute");
    public static readonly KnownType System_Xml_Resolvers_XmlPreloadedResolver = new("System.Xml.Resolvers.XmlPreloadedResolver");
    public static readonly KnownType System_Xml_Serialization_XmlElementAttribute = new("System.Xml.Serialization.XmlElementAttribute");
    public static readonly KnownType System_Xml_XmlDocument = new("System.Xml.XmlDocument");
    public static readonly KnownType System_Xml_XmlDataDocument = new("System.Xml.XmlDataDocument");
    public static readonly KnownType System_Xml_XmlNode = new("System.Xml.XmlNode");
    public static readonly KnownType System_Xml_XPath_XPathDocument = new("System.Xml.XPath.XPathDocument");
    public static readonly KnownType System_Xml_XmlReader = new("System.Xml.XmlReader");
    public static readonly KnownType System_Xml_XmlReaderSettings = new("System.Xml.XmlReaderSettings");
    public static readonly KnownType System_Xml_XmlUrlResolver = new("System.Xml.XmlUrlResolver");
    public static readonly KnownType System_Xml_XmlTextReader = new("System.Xml.XmlTextReader");
    public static readonly KnownType System_Xml_XmlWriter = new("System.Xml.XmlWriter");
    public static readonly KnownType Void = new("System.Void");
    public static readonly KnownType NSubstitute_SubstituteExtensions = new("NSubstitute.SubstituteExtensions");
    public static readonly KnownType NSubstitute_Received = new("NSubstitute.Received");
    public static readonly KnownType NSubstitute_ReceivedExtensions_ReceivedExtensions = new("NSubstitute.ReceivedExtensions.ReceivedExtensions");
    public static readonly ImmutableArray<KnownType> SystemActionVariants =
        ImmutableArray.Create<KnownType>(
            new("System.Action"),
            new("System.Action", "T"),
            new("System.Action", "T1", "T2"),
            new("System.Action", "T1", "T2", "T3"),
            new("System.Action", "T1", "T2", "T3", "T4"),
            new("System.Action", "T1", "T2", "T3", "T4", "T5"),
            new("System.Action", "T1", "T2", "T3", "T4", "T5", "T6"),
            new("System.Action", "T1", "T2", "T3", "T4", "T5", "T6", "T7"),
            new("System.Action", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8"),
            new("System.Action", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9"),
            new("System.Action", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10"),
            new("System.Action", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11"),
            new("System.Action", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12"),
            new("System.Action", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12", "T13"),
            new("System.Action", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12", "T13", "T14"),
            new("System.Action", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12", "T13", "T14", "T15"),
            new("System.Action", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12", "T13", "T14", "T15", "T16"));
    public static readonly ImmutableArray<KnownType> SystemFuncVariants =
        ImmutableArray.Create<KnownType>(
            new("System.Func", "TResult"),
            new("System.Func", "T", "TResult"),
            new("System.Func", "T1", "T2", "TResult"),
            new("System.Func", "T1", "T2", "T3", "TResult"),
            new("System.Func", "T1", "T2", "T3", "T4", "TResult"),
            new("System.Func", "T1", "T2", "T3", "T4", "T5", "TResult"),
            new("System.Func", "T1", "T2", "T3", "T4", "T5", "T6", "TResult"),
            new("System.Func", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "TResult"),
            new("System.Func", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "TResult"),
            new("System.Func", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "TResult"),
            new("System.Func", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "TResult"),
            new("System.Func", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "TResult"),
            new("System.Func", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12", "TResult"),
            new("System.Func", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12", "T13", "TResult"),
            new("System.Func", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12", "T13", "T14", "TResult"),
            new("System.Func", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12", "T13", "T14", "T15", "TResult"),
            new("System.Func", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12", "T13", "T14", "T15", "T16", "TResult"));
    public static readonly ImmutableArray<KnownType> SystemTasks =
        ImmutableArray.Create(
            System_Threading_Tasks_Task,
            System_Threading_Tasks_Task_T,
            System_Threading_Tasks_ValueTask_TResult);
    public static readonly KnownType System_Resources_ResourceManager = new("System.Resources.ResourceManager");
    public static readonly KnownType TimeZoneConverter_TZConvert = new("TimeZoneConverter.TZConvert");
    public static readonly KnownType UnityEditor_AssetModificationProcessor = new("UnityEditor.AssetModificationProcessor");
    public static readonly KnownType UnityEditor_AssetPostprocessor = new("UnityEditor.AssetPostprocessor");
    public static readonly KnownType UnityEngine_MonoBehaviour = new("UnityEngine.MonoBehaviour");
    public static readonly KnownType UnityEngine_ScriptableObject = new("UnityEngine.ScriptableObject");
    public static readonly KnownType Xunit_Assert = new("Xunit.Assert");
    public static readonly KnownType Xunit_Sdk_AssertException = new("Xunit.Sdk.AssertException");
    public static readonly KnownType Xunit_FactAttribute = new("Xunit.FactAttribute");
    public static readonly KnownType Xunit_Sdk_XunitException = new("Xunit.Sdk.XunitException");
    public static readonly KnownType Xunit_TheoryAttribute = new("Xunit.TheoryAttribute");
    public static readonly KnownType LegacyXunit_TheoryAttribute = new("Xunit.Extensions.TheoryAttribute");
    public static readonly ImmutableArray<KnownType> CallerInfoAttributes =
        ImmutableArray.Create(
            System_Runtime_CompilerServices_CallerArgumentExpressionAttribute,
            System_Runtime_CompilerServices_CallerFilePathAttribute,
            System_Runtime_CompilerServices_CallerLineNumberAttribute,
            System_Runtime_CompilerServices_CallerMemberNameAttribute);
    public static readonly ImmutableArray<KnownType> DatabaseBaseQueryTypes =
        ImmutableArray.Create(
            System_Data_Entity_Infrastructure_DbQuery,
            System_Data_Entity_Infrastructure_DbQuery_TResult,
            Microsoft_EntityFrameworkCore_DbSet_TEntity,
            System_Data_Linq_ITable,
            System_Data_Entity_Core_Objects_ObjectQuery);
    public static readonly ImmutableArray<KnownType> FloatingPointNumbers =
        ImmutableArray.Create(
            System_Half,
            System_Single,
            System_Double,
            System_Runtime_InteropServices_NFloat);
    public static readonly ImmutableArray<KnownType> IntegralNumbers =
        ImmutableArray.Create(
            System_Int16,
            System_Int32,
            System_Int64,
            System_UInt16,
            System_UInt32,
            System_UInt64,
            System_Char,
            System_Byte,
            System_SByte);
    public static readonly ImmutableArray<KnownType> IntegralNumbersIncludingNative =
        ImmutableArray.Create(
            System_Int16,
            System_Int32,
            System_Int64,
            System_UInt16,
            System_UInt32,
            System_UInt64,
            System_Char,
            System_Byte,
            System_SByte,
            System_IntPtr,
            System_UIntPtr);
    public static readonly ImmutableArray<KnownType> NonIntegralNumbers =
        ImmutableArray.Create(
            System_Single,
            System_Double,
            System_Decimal);
    public static readonly ImmutableArray<KnownType> PointerTypes =
        ImmutableArray.Create(
            System_IntPtr,
            System_UIntPtr);
    public static readonly ImmutableArray<KnownType> UnsignedIntegers =
        ImmutableArray.Create(
            System_UInt64,
            System_UInt32,
            System_UInt16);
    public static readonly ImmutableArray<KnownType> RouteAttributes =
        ImmutableArray.Create(
            Microsoft_AspNetCore_Mvc_RouteAttribute,
            System_Web_Mvc_RouteAttribute);
}

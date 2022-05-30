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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    internal abstract partial class KnownType
    {
#pragma warning disable S103    // Lines should not be too long
#pragma warning disable SA1310  // FieldNamesMustNotContainUnderscore
#pragma warning disable SA1311  // Static readonly fields should begin with upper-case letter
#pragma warning disable SA1307  // Field 'log4net_Config_XmlConfigurator' should begin with upper-case letter
#pragma warning disable SA1304  // Non-private readonly fields should begin with upper-case letter

        #region Known types

        internal static readonly KnownType Void = new SpecialKnownType(SpecialType.System_Void, "void");
        internal static readonly KnownType Azure_Messaging_ServiceBus_Administration_ServiceBusAdministrationClient = new RegularKnownType("Azure.Messaging.ServiceBus.Administration.ServiceBusAdministrationClient");
        internal static readonly KnownType Azure_Messaging_ServiceBus_ServiceBusClient = new RegularKnownType("Azure.Messaging.ServiceBus.ServiceBusClient");
        internal static readonly KnownType Azure_Storage_Blobs_BlobServiceClient = new RegularKnownType("Azure.Storage.Blobs.BlobServiceClient");
        internal static readonly KnownType Azure_Storage_Queues_QueueServiceClient = new RegularKnownType("Azure.Storage.Queues.QueueServiceClient");
        internal static readonly KnownType Azure_Storage_Files_Shares_ShareServiceClient = new RegularKnownType("Azure.Storage.Files.Shares.ShareServiceClient");
        internal static readonly KnownType Azure_Storage_Files_DataLake_DataLakeServiceClient = new RegularKnownType("Azure.Storage.Files.DataLake.DataLakeServiceClient");
        internal static readonly KnownType Azure_ResourceManager_ArmClient = new RegularKnownType("Azure.ResourceManager.ArmClient");
        internal static readonly KnownType FluentAssertions_Execution_AssertionScope = new RegularKnownType("FluentAssertions.Execution.AssertionScope");
        internal static readonly KnownType JWT_Builder_JwtBuilder = new RegularKnownType("JWT.Builder.JwtBuilder");
        internal static readonly KnownType JWT_IJwtDecoder = new RegularKnownType("JWT.IJwtDecoder");
        internal static readonly KnownType JWT_JwtDecoderExtensions = new RegularKnownType("JWT.JwtDecoderExtensions");
        internal static readonly KnownType log4net_Config_XmlConfigurator = new RegularKnownType("log4net.Config.XmlConfigurator");
        internal static readonly KnownType log4net_Config_DOMConfigurator = new RegularKnownType("log4net.Config.DOMConfigurator");
        internal static readonly KnownType log4net_Config_BasicConfigurator = new RegularKnownType("log4net.Config.BasicConfigurator");
        internal static readonly KnownType Microsoft_AspNet_SignalR_Hub = new RegularKnownType("Microsoft.AspNet.SignalR.Hub");
        internal static readonly KnownType Microsoft_AspNetCore_Builder_DeveloperExceptionPageExtensions = new RegularKnownType("Microsoft.AspNetCore.Builder.DeveloperExceptionPageExtensions");
        internal static readonly KnownType Microsoft_AspNetCore_Builder_DatabaseErrorPageExtensions = new RegularKnownType("Microsoft.AspNetCore.Builder.DatabaseErrorPageExtensions");
        internal static readonly KnownType Microsoft_AspNetCore_Cors_Infrastructure_CorsPolicyBuilder = new RegularKnownType("Microsoft.AspNetCore.Cors.Infrastructure.CorsPolicyBuilder");
        internal static readonly KnownType Microsoft_AspNetCore_Hosting_HostingEnvironmentExtensions = new RegularKnownType("Microsoft.AspNetCore.Hosting.HostingEnvironmentExtensions");
        internal static readonly KnownType Microsoft_AspNetCore_Hosting_WebHostBuilderExtensions = new RegularKnownType("Microsoft.AspNetCore.Hosting.WebHostBuilderExtensions");
        internal static readonly KnownType Microsoft_AspNetCore_Http_CookieOptions = new RegularKnownType("Microsoft.AspNetCore.Http.CookieOptions");
        internal static readonly KnownType Microsoft_AspNetCore_Http_HeaderDictionaryExtensions = new RegularKnownType("Microsoft.AspNetCore.Http.HeaderDictionaryExtensions");
        internal static readonly KnownType Microsoft_AspNetCore_Http_IHeaderDictionary = new RegularKnownType("Microsoft.AspNetCore.Http.IHeaderDictionary");
        internal static readonly KnownType Microsoft_AspNetCore_Http_IRequestCookieCollection = new RegularKnownType("Microsoft.AspNetCore.Http.IRequestCookieCollection");
        internal static readonly KnownType Microsoft_AspNetCore_Http_IResponseCookies = new RegularKnownType("Microsoft.AspNetCore.Http.IResponseCookies");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_Controller = new RegularKnownType("Microsoft.AspNetCore.Mvc.Controller");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_ControllerBase = new RegularKnownType("Microsoft.AspNetCore.Mvc.ControllerBase");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_ControllerAttribute = new RegularKnownType("Microsoft.AspNetCore.Mvc.ControllerAttribute");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_DisableRequestSizeLimitAttribute = new RegularKnownType("Microsoft.AspNetCore.Mvc.DisableRequestSizeLimitAttribute");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_FromServicesAttribute = new RegularKnownType("Microsoft.AspNetCore.Mvc.FromServicesAttribute");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_IgnoreAntiforgeryTokenAttribute = new RegularKnownType("Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_NonActionAttribute = new RegularKnownType("Microsoft.AspNetCore.Mvc.NonActionAttribute");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_NonControllerAttribute = new RegularKnownType("Microsoft.AspNetCore.Mvc.NonControllerAttribute");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_RequestFormLimitsAttribute = new RegularKnownType("Microsoft.AspNetCore.Mvc.RequestFormLimitsAttribute");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_RequestSizeLimitAttribute = new RegularKnownType("Microsoft.AspNetCore.Mvc.RequestSizeLimitAttribute");
        internal static readonly KnownType Microsoft_AspNetCore_Razor_Hosting_RazorCompiledItemAttribute = new RegularKnownType("Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute");
        internal static readonly KnownType Microsoft_Azure_Cosmos_CosmosClient = new RegularKnownType("Microsoft.Azure.Cosmos.CosmosClient");
        internal static readonly KnownType Microsoft_Azure_Documents_Client_DocumentClient = new RegularKnownType("Microsoft.Azure.Documents.Client.DocumentClient");
        internal static readonly KnownType Microsoft_Azure_ServiceBus_Management_ManagementClient = new RegularKnownType("Microsoft.Azure.ServiceBus.Management.ManagementClient");
        internal static readonly KnownType Microsoft_Azure_ServiceBus_QueueClient = new RegularKnownType("Microsoft.Azure.ServiceBus.QueueClient");
        internal static readonly KnownType Microsoft_Azure_ServiceBus_SessionClient = new RegularKnownType("Microsoft.Azure.ServiceBus.SessionClient");
        internal static readonly KnownType Microsoft_Azure_ServiceBus_SubscriptionClient = new RegularKnownType("Microsoft.Azure.ServiceBus.SubscriptionClient");
        internal static readonly KnownType Microsoft_Azure_ServiceBus_TopicClient = new RegularKnownType("Microsoft.Azure.ServiceBus.TopicClient");
        internal static readonly KnownType Microsoft_Azure_WebJobs_Extensions_DurableTask_IDurableEntityClient = new RegularKnownType("Microsoft.Azure.WebJobs.Extensions.DurableTask.IDurableEntityClient");
        internal static readonly KnownType Microsoft_Azure_WebJobs_Extensions_DurableTask_IDurableEntityContext = new RegularKnownType("Microsoft.Azure.WebJobs.Extensions.DurableTask.IDurableEntityContext");
        internal static readonly KnownType Microsoft_Azure_WebJobs_Extensions_DurableTask_IDurableOrchestrationContext = new RegularKnownType("Microsoft.Azure.WebJobs.Extensions.DurableTask.IDurableOrchestrationContext");
        internal static readonly KnownType Microsoft_Azure_WebJobs_FunctionNameAttribute = new RegularKnownType("Microsoft.Azure.WebJobs.FunctionNameAttribute");
        internal static readonly KnownType Microsoft_Data_Sqlite_SqliteCommand = new RegularKnownType("Microsoft.Data.Sqlite.SqliteCommand");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_DbContextOptionsBuilder = new RegularKnownType("Microsoft.EntityFrameworkCore.DbContextOptionsBuilder");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_Migrations_Migration = new RegularKnownType("Microsoft.EntityFrameworkCore.Migrations.Migration");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_MySQLDbContextOptionsExtensions = new RegularKnownType("Microsoft.EntityFrameworkCore.MySQLDbContextOptionsExtensions");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_NpgsqlDbContextOptionsExtensions = new RegularKnownType("Microsoft.EntityFrameworkCore.NpgsqlDbContextOptionsExtensions");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_NpgsqlDbContextOptionsBuilderExtensions = new RegularKnownType("Microsoft.EntityFrameworkCore.NpgsqlDbContextOptionsBuilderExtensions");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_OracleDbContextOptionsExtensions = new RegularKnownType("Microsoft.EntityFrameworkCore.OracleDbContextOptionsExtensions");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_RawSqlString = new RegularKnownType("Microsoft.EntityFrameworkCore.RawSqlString");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_RelationalDatabaseFacadeExtensions = new RegularKnownType("Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_RelationalQueryableExtensions = new RegularKnownType("Microsoft.EntityFrameworkCore.RelationalQueryableExtensions");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_SqliteDbContextOptionsBuilderExtensions = new RegularKnownType("Microsoft.EntityFrameworkCore.SqliteDbContextOptionsBuilderExtensions");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_SqlServerDbContextOptionsExtensions = new RegularKnownType("Microsoft.EntityFrameworkCore.SqlServerDbContextOptionsExtensions");
        internal static readonly KnownType Microsoft_Extensions_DependencyInjection_LoggingServiceCollectionExtensions = new RegularKnownType("Microsoft.Extensions.DependencyInjection.LoggingServiceCollectionExtensions");
        internal static readonly KnownType Microsoft_Extensions_Hosting_HostEnvironmentEnvExtensions = new RegularKnownType("Microsoft.Extensions.Hosting.HostEnvironmentEnvExtensions");
        internal static readonly KnownType Microsoft_Extensions_Logging_AzureAppServicesLoggerFactoryExtensions = new RegularKnownType("Microsoft.Extensions.Logging.AzureAppServicesLoggerFactoryExtensions");
        internal static readonly KnownType Microsoft_Extensions_Logging_ConsoleLoggerExtensions = new RegularKnownType("Microsoft.Extensions.Logging.ConsoleLoggerExtensions");
        internal static readonly KnownType Microsoft_Extensions_Logging_DebugLoggerFactoryExtensions = new RegularKnownType("Microsoft.Extensions.Logging.DebugLoggerFactoryExtensions");
        internal static readonly KnownType Microsoft_Extensions_Logging_EventLoggerFactoryExtensions = new RegularKnownType("Microsoft.Extensions.Logging.EventLoggerFactoryExtensions");
        internal static readonly KnownType Microsoft_Extensions_Logging_EventSourceLoggerFactoryExtensions = new RegularKnownType("Microsoft.Extensions.Logging.EventSourceLoggerFactoryExtensions");
        internal static readonly KnownType Microsoft_Extensions_Logging_ILogger = new RegularKnownType("Microsoft.Extensions.Logging.ILogger");
        internal static readonly KnownType Microsoft_Extensions_Logging_ILoggerFactory = new RegularKnownType("Microsoft.Extensions.Logging.ILoggerFactory");
        internal static readonly KnownType Microsoft_Extensions_Logging_LoggerExtensions = new RegularKnownType("Microsoft.Extensions.Logging.LoggerExtensions");
        internal static readonly KnownType Microsoft_Extensions_Primitives_StringValues = new RegularKnownType("Microsoft.Extensions.Primitives.StringValues");
        internal static readonly KnownType Microsoft_Net_Http_Headers_HeaderNames = new RegularKnownType("Microsoft.Net.Http.Headers.HeaderNames");
        internal static readonly KnownType Microsoft_VisualBasic_Interaction = new RegularKnownType("Microsoft.VisualBasic.Interaction");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_Assert = new RegularKnownType("Microsoft.VisualStudio.TestTools.UnitTesting.Assert");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_AssertFailedException = new RegularKnownType("Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_ExpectedExceptionAttribute = new RegularKnownType("Microsoft.VisualStudio.TestTools.UnitTesting.ExpectedExceptionAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_IgnoreAttribute = new RegularKnownType("Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_TestClassAttribute = new RegularKnownType("Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_TestMethodAttribute = new RegularKnownType("Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_DataTestMethodAttribute = new RegularKnownType("Microsoft.VisualStudio.TestTools.UnitTesting.DataTestMethodAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_WorkItemAttribute = new RegularKnownType("Microsoft.VisualStudio.TestTools.UnitTesting.WorkItemAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_AssemblyCleanupAttribute = new RegularKnownType("Microsoft.VisualStudio.TestTools.UnitTesting.AssemblyCleanupAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_AssemblyInitializeAttribute = new RegularKnownType("Microsoft.VisualStudio.TestTools.UnitTesting.AssemblyInitializeAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_ClassCleanupAttribute = new RegularKnownType("Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_ClassInitializeAttribute = new RegularKnownType("Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_TestCleanupAttribute = new RegularKnownType("Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_TestInitializeAttribute = new RegularKnownType("Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute");
        internal static readonly KnownType Microsoft_Web_XmlTransform_XmlFileInfoDocument = new RegularKnownType("Microsoft.Web.XmlTransform.XmlFileInfoDocument");
        internal static readonly KnownType Microsoft_Web_XmlTransform_XmlTransformableDocument = new RegularKnownType("Microsoft.Web.XmlTransform.XmlTransformableDocument");
        internal static readonly KnownType Mono_Unix_FileAccessPermissions = new RegularKnownType("Mono.Unix.FileAccessPermissions");
        internal static readonly KnownType MySql_Data_MySqlClient_MySqlDataAdapter = new RegularKnownType("MySql.Data.MySqlClient.MySqlDataAdapter");
        internal static readonly KnownType MySql_Data_MySqlClient_MySqlCommand = new RegularKnownType("MySql.Data.MySqlClient.MySqlCommand");
        internal static readonly KnownType MySql_Data_MySqlClient_MySqlHelper = new RegularKnownType("MySql.Data.MySqlClient.MySqlHelper");
        internal static readonly KnownType MySql_Data_MySqlClient_MySqlScript = new RegularKnownType("MySql.Data.MySqlClient.MySqlScript");
        internal static readonly KnownType Nancy_Cookies_NancyCookie = new RegularKnownType("Nancy.Cookies.NancyCookie");
        internal static readonly KnownType NFluent_Check = new RegularKnownType("NFluent.Check");
        internal static readonly KnownType NFluent_FluentCheckException = new RegularKnownType("NFluent.FluentCheckException");
        internal static readonly KnownType NFluent_Kernel_FluentCheckException = new RegularKnownType("NFluent.Kernel.FluentCheckException");
        internal static readonly KnownType NLog_LogManager = new RegularKnownType("NLog.LogManager");
        internal static readonly KnownType NUnit_Framework_Assert = new RegularKnownType("NUnit.Framework.Assert");
        internal static readonly KnownType NUnit_Framework_AssertionException = new RegularKnownType("NUnit.Framework.AssertionException");
        internal static readonly KnownType NUnit_Framework_ExpectedExceptionAttribute = new RegularKnownType("NUnit.Framework.ExpectedExceptionAttribute");
        internal static readonly KnownType NUnit_Framework_IgnoreAttribute = new RegularKnownType("NUnit.Framework.IgnoreAttribute");
        internal static readonly KnownType NUnit_Framework_ITestBuilderInterface = new RegularKnownType("NUnit.Framework.Interfaces.ITestBuilder");
        internal static readonly KnownType NUnit_Framework_TestAttribute = new RegularKnownType("NUnit.Framework.TestAttribute");
        internal static readonly KnownType NUnit_Framework_TestCaseAttribute = new RegularKnownType("NUnit.Framework.TestCaseAttribute");
        internal static readonly KnownType NUnit_Framework_TestCaseSourceAttribute = new RegularKnownType("NUnit.Framework.TestCaseSourceAttribute");
        internal static readonly KnownType NUnit_Framework_TestFixtureAttribute = new RegularKnownType("NUnit.Framework.TestFixtureAttribute");
        internal static readonly KnownType NUnit_Framework_TheoryAttribute = new RegularKnownType("NUnit.Framework.TheoryAttribute");
        internal static readonly KnownType Org_BouncyCastle_Asn1_Nist_NistNamedCurves = new RegularKnownType("Org.BouncyCastle.Asn1.Nist.NistNamedCurves");
        internal static readonly KnownType Org_BouncyCastle_Asn1_Sec_SecNamedCurves = new RegularKnownType("Org.BouncyCastle.Asn1.Sec.SecNamedCurves");
        internal static readonly KnownType Org_BouncyCastle_Asn1_TeleTrust_TeleTrusTNamedCurves = new RegularKnownType("Org.BouncyCastle.Asn1.TeleTrust.TeleTrusTNamedCurves");
        internal static readonly KnownType Org_BouncyCastle_Asn1_X9_ECNamedCurveTable = new RegularKnownType("Org.BouncyCastle.Asn1.X9.ECNamedCurveTable");
        internal static readonly KnownType Org_BouncyCastle_Asn1_X9_X962NamedCurves = new RegularKnownType("Org.BouncyCastle.Asn1.X9.X962NamedCurves");
        internal static readonly KnownType Org_BouncyCastle_Crypto_Engines_AesFastEngine = new RegularKnownType("Org.BouncyCastle.Crypto.Engines.AesFastEngine");
        internal static readonly KnownType Org_BouncyCastle_Crypto_Generators_DHParametersGenerator = new RegularKnownType("Org.BouncyCastle.Crypto.Generators.DHParametersGenerator");
        internal static readonly KnownType Org_BouncyCastle_Crypto_Generators_DsaParametersGenerator = new RegularKnownType("Org.BouncyCastle.Crypto.Generators.DsaParametersGenerator");
        internal static readonly KnownType Org_BouncyCastle_Crypto_Parameters_RsaKeyGenerationParameters = new RegularKnownType("Org.BouncyCastle.Crypto.Parameters.RsaKeyGenerationParameters");
        internal static readonly KnownType Serilog_LoggerConfiguration = new RegularKnownType("Serilog.LoggerConfiguration");
        internal static readonly KnownType System_Action = new RegularKnownType("System.Action");
        internal static readonly KnownType System_Action_T = new RegularKnownType("System.Action", "T");
        internal static readonly KnownType System_Action_T1_T2 = new RegularKnownType("System.Action", "T1", "T2");
        internal static readonly KnownType System_Action_T1_T2_T3 = new RegularKnownType("System.Action", "T1", "T2", "T3");
        internal static readonly KnownType System_Action_T1_T2_T3_T4 = new RegularKnownType("System.Action", "T1", "T2", "T3", "T4");
        internal static readonly KnownType System_Activator = new RegularKnownType("System.Activator");
        internal static readonly KnownType System_ApplicationException = new RegularKnownType("System.ApplicationException");
        internal static readonly KnownType System_AppDomain = new RegularKnownType("System.AppDomain");
        internal static readonly KnownType System_ArgumentException = new RegularKnownType("System.ArgumentException");
        internal static readonly KnownType System_ArgumentNullException = new RegularKnownType("System.ArgumentNullException");
        internal static readonly KnownType System_ArgumentOutOfRangeException = new RegularKnownType("System.ArgumentOutOfRangeException");
        internal static readonly KnownType System_Array = new SpecialKnownType(SpecialType.System_Array, "System.Array");
        internal static readonly KnownType System_Attribute = new RegularKnownType("System.Attribute");
        internal static readonly KnownType System_AttributeUsageAttribute = new RegularKnownType("System.AttributeUsageAttribute");
        internal static readonly KnownType System_Boolean = new SpecialKnownType(SpecialType.System_Boolean, "bool");
        internal static readonly KnownType System_Byte = new SpecialKnownType(SpecialType.System_Byte, "byte");
        internal static readonly KnownType System_Byte_Array = new RegularKnownType("System.Byte") { IsArray = true };
        internal static readonly KnownType System_Char = new SpecialKnownType(SpecialType.System_Char, "char");
        internal static readonly KnownType System_CLSCompliantAttribute = new RegularKnownType("System.CLSCompliantAttribute");
        internal static readonly KnownType System_CodeDom_Compiler_GeneratedCodeAttribute = new RegularKnownType("System.CodeDom.Compiler.GeneratedCodeAttribute");
        internal static readonly KnownType System_Collections_CollectionBase = new RegularKnownType("System.Collections.CollectionBase");
        internal static readonly KnownType System_Collections_DictionaryBase = new RegularKnownType("System.Collections.DictionaryBase");
        internal static readonly KnownType System_Collections_Generic_Dictionary_TKey_TValue = new RegularKnownType("System.Collections.Generic.Dictionary", "TKey", "TValue");
        internal static readonly KnownType System_Collections_Generic_HashSet_T = new RegularKnownType("System.Collections.Generic.HashSet", "T");
        internal static readonly KnownType System_Collections_Generic_IAsyncEnumerable_T = new RegularKnownType("System.Collections.Generic.IAsyncEnumerable", "T");
        internal static readonly KnownType System_Collections_Generic_ICollection_T = new SpecialKnownType(SpecialType.System_Collections_Generic_ICollection_T, "System.Collections.Generic.ICollection<T>");
        internal static readonly KnownType System_Collections_Generic_IDictionary_TKey_TValue = new RegularKnownType("System.Collections.Generic.IDictionary", "TKey", "TValue");
        internal static readonly KnownType System_Collections_Generic_IEnumerable_T = new SpecialKnownType(SpecialType.System_Collections_Generic_IEnumerable_T, "System.Collections.Generic.IEnumerable<T>");
        internal static readonly KnownType System_Collections_Generic_IList_T = new SpecialKnownType(SpecialType.System_Collections_Generic_IList_T, "System.Collections.Generic.IList<T>");
        internal static readonly KnownType System_Collections_Generic_IReadOnlyCollection_T = new SpecialKnownType(SpecialType.System_Collections_Generic_IReadOnlyCollection_T, "System.Collections.Generic.IReadOnlyCollection<T>");
        internal static readonly KnownType System_Collections_Generic_ISet_T = new RegularKnownType("System.Collections.Generic.ISet", "T");
        internal static readonly KnownType System_Collections_Generic_KeyValuePair_TKey_TValue = new RegularKnownType("System.Collections.Generic.KeyValuePair", "TKey", "TValue");
        internal static readonly KnownType System_Collections_Generic_List_T = new RegularKnownType("System.Collections.Generic.List", "T");
        internal static readonly KnownType System_Collections_Generic_Queue_T = new RegularKnownType("System.Collections.Generic.Queue", "T");
        internal static readonly KnownType System_Collections_Generic_Stack_T = new RegularKnownType("System.Collections.Generic.Stack", "T");
        internal static readonly KnownType System_Collections_ICollection = new RegularKnownType("System.Collections.ICollection");
        internal static readonly KnownType System_Collections_IEnumerable = new SpecialKnownType(SpecialType.System_Collections_IEnumerable, "System.Collections.IEnumerable");
        internal static readonly KnownType System_Collections_IList = new RegularKnownType("System.Collections.IList");
        internal static readonly KnownType System_Collections_Immutable_IImmutableArray_T = new RegularKnownType("System.Collections.Immutable.IImmutableArray", "T");
        internal static readonly KnownType System_Collections_Immutable_IImmutableDictionary_TKey_TValue = new RegularKnownType("System.Collections.Immutable.IImmutableDictionary", "TKey", "TValue");
        internal static readonly KnownType System_Collections_Immutable_IImmutableList_T = new RegularKnownType("System.Collections.Immutable.IImmutableList", "T");
        internal static readonly KnownType System_Collections_Immutable_IImmutableQueue_T = new RegularKnownType("System.Collections.Immutable.IImmutableQueue", "T");
        internal static readonly KnownType System_Collections_Immutable_IImmutableSet_T = new RegularKnownType("System.Collections.Immutable.IImmutableSet", "T");
        internal static readonly KnownType System_Collections_Immutable_IImmutableStack_T = new RegularKnownType("System.Collections.Immutable.IImmutableStack", "T");
        internal static readonly KnownType System_Collections_Immutable_ImmutableArray = new RegularKnownType("System.Collections.Immutable.ImmutableArray");
        internal static readonly KnownType System_Collections_Immutable_ImmutableArray_T = new RegularKnownType("System.Collections.Immutable.ImmutableArray", "T");
        internal static readonly KnownType System_Collections_Immutable_ImmutableDictionary = new RegularKnownType("System.Collections.Immutable.ImmutableDictionary");
        internal static readonly KnownType System_Collections_Immutable_ImmutableDictionary_TKey_TValue = new RegularKnownType("System.Collections.Immutable.ImmutableDictionary", "TKey", "TValue");
        internal static readonly KnownType System_Collections_Immutable_ImmutableHashSet = new RegularKnownType("System.Collections.Immutable.ImmutableHashSet");
        internal static readonly KnownType System_Collections_Immutable_ImmutableHashSet_T = new RegularKnownType("System.Collections.Immutable.ImmutableHashSet", "T");
        internal static readonly KnownType System_Collections_Immutable_ImmutableList = new RegularKnownType("System.Collections.Immutable.ImmutableList");
        internal static readonly KnownType System_Collections_Immutable_ImmutableList_T = new RegularKnownType("System.Collections.Immutable.ImmutableList", "T");
        internal static readonly KnownType System_Collections_Immutable_ImmutableQueue = new RegularKnownType("System.Collections.Immutable.ImmutableQueue");
        internal static readonly KnownType System_Collections_Immutable_ImmutableQueue_T = new RegularKnownType("System.Collections.Immutable.ImmutableQueue", "T");
        internal static readonly KnownType System_Collections_Immutable_ImmutableSortedDictionary = new RegularKnownType("System.Collections.Immutable.ImmutableSortedDictionary");
        internal static readonly KnownType System_Collections_Immutable_ImmutableSortedDictionary_TKey_TValue = new RegularKnownType("System.Collections.Immutable.ImmutableSortedDictionary", "TKey", "TValue");
        internal static readonly KnownType System_Collections_Immutable_ImmutableSortedSet = new RegularKnownType("System.Collections.Immutable.ImmutableSortedSet");
        internal static readonly KnownType System_Collections_Immutable_ImmutableSortedSet_T = new RegularKnownType("System.Collections.Immutable.ImmutableSortedSet", "T");
        internal static readonly KnownType System_Collections_Immutable_ImmutableStack = new RegularKnownType("System.Collections.Immutable.ImmutableStack");
        internal static readonly KnownType System_Collections_Immutable_ImmutableStack_T = new RegularKnownType("System.Collections.Immutable.ImmutableStack", "T");
        internal static readonly KnownType System_Collections_ObjectModel_Collection_T = new RegularKnownType("System.Collections.ObjectModel.Collection", "T");
        internal static readonly KnownType System_Collections_ObjectModel_ObservableCollection_T = new RegularKnownType("System.Collections.ObjectModel.ObservableCollection", "T");
        internal static readonly KnownType System_Collections_ObjectModel_ReadOnlyCollection_T = new RegularKnownType("System.Collections.ObjectModel.ReadOnlyCollection", "T");
        internal static readonly KnownType System_Collections_ObjectModel_ReadOnlyDictionary_TKey_TValue = new RegularKnownType("System.Collections.ObjectModel.ReadOnlyDictionary", "TKey", "TValue");
        internal static readonly KnownType System_Collections_Queue = new RegularKnownType("System.Collections.Queue");
        internal static readonly KnownType System_Collections_ReadOnlyCollectionBase = new RegularKnownType("System.Collections.ReadOnlyCollectionBase");
        internal static readonly KnownType System_Collections_SortedList = new RegularKnownType("System.Collections.SortedList");
        internal static readonly KnownType System_Collections_Stack = new RegularKnownType("System.Collections.Stack");
        internal static readonly KnownType System_Collections_Specialized_NameObjectCollectionBase = new RegularKnownType("System.Collections.Specialized.NameObjectCollectionBase");
        internal static readonly KnownType System_Collections_Specialized_NameValueCollection = new RegularKnownType("System.Collections.Specialized.NameValueCollection");
        internal static readonly KnownType System_Composition_ExportAttribute = new RegularKnownType("System.Composition.ExportAttribute");
        internal static readonly KnownType System_ComponentModel_DefaultValueAttribute = new RegularKnownType("System.ComponentModel.DefaultValueAttribute");
        internal static readonly KnownType System_ComponentModel_EditorBrowsableAttribute = new RegularKnownType("System.ComponentModel.EditorBrowsableAttribute");
        internal static readonly KnownType System_ComponentModel_LocalizableAttribute = new RegularKnownType("System.ComponentModel.LocalizableAttribute");
        internal static readonly KnownType System_ComponentModel_Composition_CreationPolicy = new RegularKnownType("System.ComponentModel.Composition.CreationPolicy");
        internal static readonly KnownType System_ComponentModel_Composition_ExportAttribute = new RegularKnownType("System.ComponentModel.Composition.ExportAttribute");
        internal static readonly KnownType System_ComponentModel_Composition_InheritedExportAttribute = new RegularKnownType("System.ComponentModel.Composition.InheritedExportAttribute");
        internal static readonly KnownType System_ComponentModel_Composition_PartCreationPolicyAttribute = new RegularKnownType("System.ComponentModel.Composition.PartCreationPolicyAttribute");
        internal static readonly KnownType System_Configuration_ConfigXmlDocument = new RegularKnownType("System.Configuration.ConfigXmlDocument");
        internal static readonly KnownType System_Console = new RegularKnownType("System.Console");
        internal static readonly KnownType System_Data_Common_CommandTrees_DbExpression = new RegularKnownType("System.Data.Common.CommandTrees.DbExpression");
        internal static readonly KnownType System_Data_DataSet = new RegularKnownType("System.Data.DataSet");
        internal static readonly KnownType System_Data_DataTable = new RegularKnownType("System.Data.DataTable");
        internal static readonly KnownType System_Data_Odbc_OdbcCommand = new RegularKnownType("System.Data.Odbc.OdbcCommand");
        internal static readonly KnownType System_Data_Odbc_OdbcDataAdapter = new RegularKnownType("System.Data.Odbc.OdbcDataAdapter");
        internal static readonly KnownType System_Data_OracleClient_OracleCommand = new RegularKnownType("System.Data.OracleClient.OracleCommand");
        internal static readonly KnownType System_Data_OracleClient_OracleDataAdapter = new RegularKnownType("System.Data.OracleClient.OracleDataAdapter");
        internal static readonly KnownType System_Data_SqlClient_SqlCommand = new RegularKnownType("System.Data.SqlClient.SqlCommand");
        internal static readonly KnownType System_Data_SqlClient_SqlDataAdapter = new RegularKnownType("System.Data.SqlClient.SqlDataAdapter");
        internal static readonly KnownType System_Data_Sqlite_SqliteCommand = new RegularKnownType("System.Data.SQLite.SQLiteCommand");
        internal static readonly KnownType System_Data_Sqlite_SQLiteDataAdapter = new RegularKnownType("System.Data.SQLite.SQLiteDataAdapter");
        internal static readonly KnownType System_Data_SqlServerCe_SqlCeCommand = new RegularKnownType("System.Data.SqlServerCe.SqlCeCommand");
        internal static readonly KnownType System_Data_SqlServerCe_SqlCeDataAdapter = new RegularKnownType("System.Data.SqlServerCe.SqlCeDataAdapter");
        internal static readonly KnownType System_DateTime = new SpecialKnownType(SpecialType.System_DateTime, "DateTime");
        internal static readonly KnownType System_DateTimeOffset = new RegularKnownType("System.DateTimeOffset");
        internal static readonly KnownType System_Decimal = new SpecialKnownType(SpecialType.System_Decimal, "decimal");
        internal static readonly KnownType System_Delegate = new RegularKnownType("System.Delegate");
        internal static readonly KnownType System_Diagnostics_CodeAnalysis_ExcludeFromCodeCoverageAttribute = new RegularKnownType("System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute");
        internal static readonly KnownType System_Diagnostics_CodeAnalysis_SuppressMessageAttribute = new RegularKnownType("System.Diagnostics.CodeAnalysis.SuppressMessageAttribute");
        internal static readonly KnownType System_Diagnostics_ConditionalAttribute = new RegularKnownType("System.Diagnostics.ConditionalAttribute");
        internal static readonly KnownType System_Diagnostics_Contracts_PureAttribute = new RegularKnownType("System.Diagnostics.Contracts.PureAttribute");
        internal static readonly KnownType System_Diagnostics_Debug = new RegularKnownType("System.Diagnostics.Debug");
        internal static readonly KnownType System_Diagnostics_DebuggerDisplayAttribute = new RegularKnownType("System.Diagnostics.DebuggerDisplayAttribute");
        internal static readonly KnownType System_Diagnostics_Process = new RegularKnownType("System.Diagnostics.Process");
        internal static readonly KnownType System_Diagnostics_ProcessStartInfo = new RegularKnownType("System.Diagnostics.ProcessStartInfo");
        internal static readonly KnownType System_Diagnostics_Trace = new RegularKnownType("System.Diagnostics.Trace");
        internal static readonly KnownType System_Diagnostics_TraceSource = new RegularKnownType("System.Diagnostics.TraceSource");
        internal static readonly KnownType System_DirectoryServices_AuthenticationTypes = new RegularKnownType("System.DirectoryServices.AuthenticationTypes");
        internal static readonly KnownType System_DirectoryServices_DirectoryEntry = new RegularKnownType("System.DirectoryServices.DirectoryEntry");
        internal static readonly KnownType System_Double = new SpecialKnownType(SpecialType.System_Double, "double");
        internal static readonly KnownType System_Drawing_Bitmap = new RegularKnownType("System.Drawing.Bitmap");
        internal static readonly KnownType System_Drawing_Image = new RegularKnownType("System.Drawing.Image");
        internal static readonly KnownType System_DuplicateWaitObjectException = new RegularKnownType("System.DuplicateWaitObjectException");
        internal static readonly KnownType System_Enum = new SpecialKnownType(SpecialType.System_Enum, "Enum");
        internal static readonly KnownType System_Environment = new RegularKnownType("System.Environment");
        internal static readonly KnownType System_EventArgs = new RegularKnownType("System.EventArgs");
        internal static readonly KnownType System_EventHandler = new RegularKnownType("System.EventHandler");
        internal static readonly KnownType System_EventHandler_TEventArgs = new RegularKnownType("System.EventHandler", "TEventArgs");
        internal static readonly KnownType System_Exception = new RegularKnownType("System.Exception");
        internal static readonly KnownType System_ExecutionEngineException = new RegularKnownType("System.ExecutionEngineException");
        internal static readonly KnownType System_FlagsAttribute = new RegularKnownType("System.FlagsAttribute");
        internal static readonly KnownType System_Func_TResult = new RegularKnownType("System.Func", "TResult");
        internal static readonly KnownType System_Func_T_TResult = new RegularKnownType("System.Func", "T", "TResult");
        internal static readonly KnownType System_Func_T1_T2_TResult = new RegularKnownType("System.Func", "T1", "T2", "TResult");
        internal static readonly KnownType System_Func_T1_T2_T3_TResult = new RegularKnownType("System.Func", "T1", "T2", "T3", "TResult");
        internal static readonly KnownType System_Func_T1_T2_T3_T4_TResult = new RegularKnownType("System.Func", "T1", "T2", "T3", "T4", "TResult");
        internal static readonly KnownType System_GC = new RegularKnownType("System.GC");
        internal static readonly KnownType System_Globalization_CompareOptions = new RegularKnownType("System.Globalization.CompareOptions");
        internal static readonly KnownType System_Globalization_CultureInfo = new RegularKnownType("System.Globalization.CultureInfo");
        internal static readonly KnownType System_Guid = new RegularKnownType("System.Guid");
        internal static readonly KnownType System_IAsyncDisposable = new RegularKnownType("System.IAsyncDisposable");
        internal static readonly KnownType System_IComparable = new RegularKnownType("System.IComparable");
        internal static readonly KnownType System_IComparable_T = new RegularKnownType("System.IComparable", "T");
        internal static readonly KnownType System_IdentityModel_Tokens_SecurityTokenHandler = new RegularKnownType("System.IdentityModel.Tokens.SecurityTokenHandler");
        internal static readonly KnownType System_IDisposable = new SpecialKnownType(SpecialType.System_IDisposable, "System.IDisposable");
        internal static readonly KnownType System_IEquatable_T = new RegularKnownType("System.IEquatable", "T");
        internal static readonly KnownType System_IFormatProvider = new RegularKnownType("System.IFormatProvider");
        internal static readonly KnownType System_Index = new RegularKnownType("System.Index");
        internal static readonly KnownType System_IndexOutOfRangeException = new RegularKnownType("System.IndexOutOfRangeException");
        internal static readonly KnownType System_Int16 = new SpecialKnownType(SpecialType.System_Int16, "short");
        internal static readonly KnownType System_Int32 = new SpecialKnownType(SpecialType.System_Int32, "int");
        internal static readonly KnownType System_Int64 = new SpecialKnownType(SpecialType.System_Int64, "long");
        internal static readonly KnownType System_IntPtr = new SpecialKnownType(SpecialType.System_IntPtr, "IntPtr");
        internal static readonly KnownType System_InvalidOperationException = new RegularKnownType("System.InvalidOperationException");
        internal static readonly KnownType System_IO_Compression_ZipFile = new RegularKnownType("System.IO.Compression.ZipFile");
        internal static readonly KnownType System_IO_Compression_ZipFileExtensions = new RegularKnownType("System.IO.Compression.ZipFileExtensions");
        internal static readonly KnownType System_IO_FileStream = new RegularKnownType("System.IO.FileStream");
        internal static readonly KnownType System_IO_Path = new RegularKnownType("System.IO.Path");
        internal static readonly KnownType System_IO_Stream = new RegularKnownType("System.IO.Stream");
        internal static readonly KnownType System_IO_StreamReader = new RegularKnownType("System.IO.StreamReader");
        internal static readonly KnownType System_IO_StreamWriter = new RegularKnownType("System.IO.StreamWriter");
        internal static readonly KnownType System_IO_TextWriter = new RegularKnownType("System.IO.TextWriter");
        internal static readonly KnownType System_Lazy = new RegularKnownType("System.Lazy", "T");
        internal static readonly KnownType System_Linq_Enumerable = new RegularKnownType("System.Linq.Enumerable");
        internal static readonly KnownType System_Linq_Expressions_Expression_T = new RegularKnownType("System.Linq.Expressions.Expression", "TDelegate");
        internal static readonly KnownType System_Linq_ImmutableArrayExtensions = new RegularKnownType("System.Linq.ImmutableArrayExtensions");
        internal static readonly KnownType System_MarshalByRefObject = new RegularKnownType("System.MarshalByRefObject");
        internal static readonly KnownType System_MTAThreadAttribute = new RegularKnownType("System.MTAThreadAttribute");
        internal static readonly KnownType System_Net_FtpWebRequest = new RegularKnownType("System.Net.FtpWebRequest");
        internal static readonly KnownType System_Net_Http_Headers_HttpHeaders = new RegularKnownType("System.Net.Http.Headers.HttpHeaders");
        internal static readonly KnownType System_Net_Http_HttpClient = new RegularKnownType("System.Net.Http.HttpClient");
        internal static readonly KnownType System_Net_Http_HttpClientHandler = new RegularKnownType("System.Net.Http.HttpClientHandler");
        internal static readonly KnownType System_Net_Mail_SmtpClient = new RegularKnownType("System.Net.Mail.SmtpClient");
        internal static readonly KnownType System_Net_NetworkCredential = new RegularKnownType("System.Net.NetworkCredential");
        internal static readonly KnownType System_Net_Security_RemoteCertificateValidationCallback = new RegularKnownType("System.Net.Security.RemoteCertificateValidationCallback");
        internal static readonly KnownType System_Net_Security_SslPolicyErrors = new RegularKnownType("System.Net.Security.SslPolicyErrors");
        internal static readonly KnownType System_Net_SecurityProtocolType = new RegularKnownType("System.Net.SecurityProtocolType");
        internal static readonly KnownType System_Net_Sockets_Socket = new RegularKnownType("System.Net.Sockets.Socket");
        internal static readonly KnownType System_Net_Sockets_TcpClient = new RegularKnownType("System.Net.Sockets.TcpClient");
        internal static readonly KnownType System_Net_Sockets_UdpClient = new RegularKnownType("System.Net.Sockets.UdpClient");
        internal static readonly KnownType System_Net_WebClient = new RegularKnownType("System.Net.WebClient");
        internal static readonly KnownType System_NotImplementedException = new RegularKnownType("System.NotImplementedException");
        internal static readonly KnownType System_NotSupportedException = new RegularKnownType("System.NotSupportedException");
        internal static readonly KnownType System_Nullable_T = new SpecialKnownType(SpecialType.System_Nullable_T, "System.Nullable<T>");
        internal static readonly KnownType System_NullReferenceException = new RegularKnownType("System.NullReferenceException");
        internal static readonly KnownType System_Object = new SpecialKnownType(SpecialType.System_Object, "object");
        internal static readonly KnownType System_ObsoleteAttribute = new RegularKnownType("System.ObsoleteAttribute");
        internal static readonly KnownType System_OutOfMemoryException = new RegularKnownType("System.OutOfMemoryException");
        internal static readonly KnownType System_Random = new RegularKnownType("System.Random");
        internal static readonly KnownType System_Range = new RegularKnownType("System.Range");
        internal static readonly KnownType System_Reflection_Assembly = new RegularKnownType("System.Reflection.Assembly");
        internal static readonly KnownType System_Reflection_BindingFlags = new RegularKnownType("System.Reflection.BindingFlags");
        internal static readonly KnownType System_Reflection_AssemblyVersionAttribute = new RegularKnownType("System.Reflection.AssemblyVersionAttribute");
        internal static readonly KnownType System_Reflection_MemberInfo = new RegularKnownType("System.Reflection.MemberInfo");
        internal static readonly KnownType System_Reflection_Module = new RegularKnownType("System.Reflection.Module");
        internal static readonly KnownType System_Reflection_ParameterInfo = new RegularKnownType("System.Reflection.ParameterInfo");
        internal static readonly KnownType System_Resources_NeutralResourcesLanguageAttribute = new RegularKnownType("System.Resources.NeutralResourcesLanguageAttribute");
        internal static readonly KnownType System_Runtime_CompilerServices_CallerFilePathAttribute = new RegularKnownType("System.Runtime.CompilerServices.CallerFilePathAttribute");
        internal static readonly KnownType System_Runtime_CompilerServices_CallerLineNumberAttribute = new RegularKnownType("System.Runtime.CompilerServices.CallerLineNumberAttribute");
        internal static readonly KnownType System_Runtime_CompilerServices_CallerMemberNameAttribute = new RegularKnownType("System.Runtime.CompilerServices.CallerMemberNameAttribute");
        internal static readonly KnownType System_Runtime_CompilerServices_InternalsVisibleToAttribute = new RegularKnownType("System.Runtime.CompilerServices.InternalsVisibleToAttribute");
        internal static readonly KnownType System_Runtime_CompilerServices_ModuleInitializerAttribute = new RegularKnownType("System.Runtime.CompilerServices.ModuleInitializerAttribute");
        internal static readonly KnownType System_Runtime_CompilerServices_ValueTaskAwaiter = new RegularKnownType("System.Runtime.CompilerServices.ValueTaskAwaiter");
        internal static readonly KnownType System_Runtime_CompilerServices_ValueTaskAwaiter_TResult = new RegularKnownType("System.Runtime.CompilerServices.ValueTaskAwaiter", "TResult");
        internal static readonly KnownType System_Runtime_CompilerServices_TaskAwaiter = new RegularKnownType("System.Runtime.CompilerServices.TaskAwaiter");
        internal static readonly KnownType System_Runtime_CompilerServices_TaskAwaiter_TResult = new RegularKnownType("System.Runtime.CompilerServices.TaskAwaiter", "TResult");
        internal static readonly KnownType System_Runtime_InteropServices_ComImportAttribute = new RegularKnownType("System.Runtime.InteropServices.ComImportAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_ComVisibleAttribute = new RegularKnownType("System.Runtime.InteropServices.ComVisibleAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_DefaultParameterValueAttribute = new RegularKnownType("System.Runtime.InteropServices.DefaultParameterValueAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_DllImportAttribute = new RegularKnownType("System.Runtime.InteropServices.DllImportAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_HandleRef = new RegularKnownType("System.Runtime.InteropServices.HandleRef");
        internal static readonly KnownType System_Runtime_InteropServices_InterfaceTypeAttribute = new RegularKnownType("System.Runtime.InteropServices.InterfaceTypeAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_OptionalAttribute = new RegularKnownType("System.Runtime.InteropServices.OptionalAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_SafeHandle = new RegularKnownType("System.Runtime.InteropServices.SafeHandle");
        internal static readonly KnownType System_Runtime_InteropServices_StructLayoutAttribute = new RegularKnownType("System.Runtime.InteropServices.StructLayoutAttribute");
        internal static readonly KnownType System_Runtime_Serialization_DataMemberAttribute = new RegularKnownType("System.Runtime.Serialization.DataMemberAttribute");
        internal static readonly KnownType System_Runtime_Serialization_Formatters_Binary_BinaryFormatter = new RegularKnownType("System.Runtime.Serialization.Formatters.Binary.BinaryFormatter");
        internal static readonly KnownType System_Runtime_Serialization_Formatters_Soap_SoapFormatter = new RegularKnownType("System.Runtime.Serialization.Formatters.Soap.SoapFormatter");
        internal static readonly KnownType System_Runtime_Serialization_ISerializable = new RegularKnownType("System.Runtime.Serialization.ISerializable");
        internal static readonly KnownType System_Runtime_Serialization_IDeserializationCallback = new RegularKnownType("System.Runtime.Serialization.IDeserializationCallback");
        internal static readonly KnownType System_Runtime_Serialization_NetDataContractSerializer = new RegularKnownType("System.Runtime.Serialization.NetDataContractSerializer");
        internal static readonly KnownType System_Runtime_Serialization_OnDeserializedAttribute = new RegularKnownType("System.Runtime.Serialization.OnDeserializedAttribute");
        internal static readonly KnownType System_Runtime_Serialization_OnDeserializingAttribute = new RegularKnownType("System.Runtime.Serialization.OnDeserializingAttribute");
        internal static readonly KnownType System_Runtime_Serialization_OnSerializedAttribute = new RegularKnownType("System.Runtime.Serialization.OnSerializedAttribute");
        internal static readonly KnownType System_Runtime_Serialization_OnSerializingAttribute = new RegularKnownType("System.Runtime.Serialization.OnSerializingAttribute");
        internal static readonly KnownType System_Runtime_Serialization_OptionalFieldAttribute = new RegularKnownType("System.Runtime.Serialization.OptionalFieldAttribute");
        internal static readonly KnownType System_Runtime_Serialization_SerializationInfo = new RegularKnownType("System.Runtime.Serialization.SerializationInfo");
        internal static readonly KnownType System_Runtime_Serialization_StreamingContext = new RegularKnownType("System.Runtime.Serialization.StreamingContext");
        internal static readonly KnownType System_SByte = new SpecialKnownType(SpecialType.System_SByte, "sbyte");
        internal static readonly KnownType System_Security_AccessControl_FileSystemAccessRule = new RegularKnownType("System.Security.AccessControl.FileSystemAccessRule");
        internal static readonly KnownType System_Security_AccessControl_FileSystemSecurity = new RegularKnownType("System.Security.AccessControl.FileSystemSecurity");
        internal static readonly KnownType System_Security_AllowPartiallyTrustedCallersAttribute = new RegularKnownType("System.Security.AllowPartiallyTrustedCallersAttribute");
        internal static readonly KnownType System_Security_Authentication_SslProtocols = new RegularKnownType("System.Security.Authentication.SslProtocols");
        internal static readonly KnownType System_Security_Cryptography_AesManaged = new RegularKnownType("System.Security.Cryptography.AesManaged");
        internal static readonly KnownType System_Security_Cryptography_AsymmetricAlgorithm = new RegularKnownType("System.Security.Cryptography.AsymmetricAlgorithm");
        internal static readonly KnownType System_Security_Cryptography_AsymmetricKeyExchangeDeformatter = new RegularKnownType("System.Security.Cryptography.AsymmetricKeyExchangeDeformatter");
        internal static readonly KnownType System_Security_Cryptography_AsymmetricKeyExchangeFormatter = new RegularKnownType("System.Security.Cryptography.AsymmetricKeyExchangeFormatter");
        internal static readonly KnownType System_Security_Cryptography_AsymmetricSignatureDeformatter = new RegularKnownType("System.Security.Cryptography.AsymmetricSignatureDeformatter");
        internal static readonly KnownType System_Security_Cryptography_AsymmetricSignatureFormatter = new RegularKnownType("System.Security.Cryptography.AsymmetricSignatureFormatter");
        internal static readonly KnownType System_Security_Cryptography_CryptoConfig = new RegularKnownType("System.Security.Cryptography.CryptoConfig");
        internal static readonly KnownType System_Security_Cryptography_CspParameters = new RegularKnownType("System.Security.Cryptography.CspParameters");
        internal static readonly KnownType System_Security_Cryptography_DES = new RegularKnownType("System.Security.Cryptography.DES");
        internal static readonly KnownType System_Security_Cryptography_DeriveBytes = new RegularKnownType("System.Security.Cryptography.DeriveBytes");
        internal static readonly KnownType System_Security_Cryptography_DSA = new RegularKnownType("System.Security.Cryptography.DSA");
        internal static readonly KnownType System_Security_Cryptography_DSACryptoServiceProvider = new RegularKnownType("System.Security.Cryptography.DSACryptoServiceProvider");
        internal static readonly KnownType System_Security_Cryptography_ECDiffieHellman = new RegularKnownType("System.Security.Cryptography.ECDiffieHellman");
        internal static readonly KnownType System_Security_Cryptography_ECDsa = new RegularKnownType("System.Security.Cryptography.ECDsa");
        internal static readonly KnownType System_Security_Cryptography_HashAlgorithm = new RegularKnownType("System.Security.Cryptography.HashAlgorithm");
        internal static readonly KnownType System_Security_Cryptography_HMAC = new RegularKnownType("System.Security.Cryptography.HMAC");
        internal static readonly KnownType System_Security_Cryptography_HMACMD5 = new RegularKnownType("System.Security.Cryptography.HMACMD5");
        internal static readonly KnownType System_Security_Cryptography_HMACRIPEMD160 = new RegularKnownType("System.Security.Cryptography.HMACRIPEMD160");
        internal static readonly KnownType System_Security_Cryptography_HMACSHA1 = new RegularKnownType("System.Security.Cryptography.HMACSHA1");
        internal static readonly KnownType System_Security_Cryptography_ICryptoTransform = new RegularKnownType("System.Security.Cryptography.ICryptoTransform");
        internal static readonly KnownType System_Security_Cryptography_KeyedHashAlgorithm = new RegularKnownType("System.Security.Cryptography.KeyedHashAlgorithm");
        internal static readonly KnownType System_Security_Cryptography_MD5 = new RegularKnownType("System.Security.Cryptography.MD5");
        internal static readonly KnownType System_Security_Cryptography_PasswordDeriveBytes = new RegularKnownType("System.Security.Cryptography.PasswordDeriveBytes");
        internal static readonly KnownType System_Security_Cryptography_RC2 = new RegularKnownType("System.Security.Cryptography.RC2");
        internal static readonly KnownType System_Security_Cryptography_RandomNumberGenerator = new RegularKnownType("System.Security.Cryptography.RandomNumberGenerator");
        internal static readonly KnownType System_Security_Cryptography_Rfc2898DeriveBytes = new RegularKnownType("System.Security.Cryptography.Rfc2898DeriveBytes");
        internal static readonly KnownType System_Security_Cryptography_RIPEMD160 = new RegularKnownType("System.Security.Cryptography.RIPEMD160");
        internal static readonly KnownType System_Security_Cryptography_RNGCryptoServiceProvider = new RegularKnownType("System.Security.Cryptography.RNGCryptoServiceProvider");
        internal static readonly KnownType System_Security_Cryptography_RSA = new RegularKnownType("System.Security.Cryptography.RSA");
        internal static readonly KnownType System_Security_Cryptography_RSACryptoServiceProvider = new RegularKnownType("System.Security.Cryptography.RSACryptoServiceProvider");
        internal static readonly KnownType System_Security_Cryptography_SHA1 = new RegularKnownType("System.Security.Cryptography.SHA1");
        internal static readonly KnownType System_Security_Cryptography_SymmetricAlgorithm = new RegularKnownType("System.Security.Cryptography.SymmetricAlgorithm");
        internal static readonly KnownType System_Security_Cryptography_TripleDES = new RegularKnownType("System.Security.Cryptography.TripleDES");
        internal static readonly KnownType System_Security_Cryptography_X509Certificates_X509Certificate2 = new RegularKnownType("System.Security.Cryptography.X509Certificates.X509Certificate2");
        internal static readonly KnownType System_Security_Cryptography_X509Certificates_X509Chain = new RegularKnownType("System.Security.Cryptography.X509Certificates.X509Chain");
        internal static readonly KnownType System_Security_Permissions_CodeAccessSecurityAttribute = new RegularKnownType("System.Security.Permissions.CodeAccessSecurityAttribute");
        internal static readonly KnownType System_Security_Permissions_PrincipalPermission = new RegularKnownType("System.Security.Permissions.PrincipalPermission");
        internal static readonly KnownType System_Security_Permissions_PrincipalPermissionAttribute = new RegularKnownType("System.Security.Permissions.PrincipalPermissionAttribute");
        internal static readonly KnownType System_Security_PermissionSet = new RegularKnownType("System.Security.PermissionSet");
        internal static readonly KnownType System_Security_Principal_IIdentity = new RegularKnownType("System.Security.Principal.IIdentity");
        internal static readonly KnownType System_Security_Principal_IPrincipal = new RegularKnownType("System.Security.Principal.IPrincipal");
        internal static readonly KnownType System_Security_Principal_NTAccount = new RegularKnownType("System.Security.Principal.NTAccount");
        internal static readonly KnownType System_Security_Principal_SecurityIdentifier = new RegularKnownType("System.Security.Principal.SecurityIdentifier");
        internal static readonly KnownType System_Security_Principal_WindowsIdentity = new RegularKnownType("System.Security.Principal.WindowsIdentity");
        internal static readonly KnownType System_Security_SecurityCriticalAttribute = new RegularKnownType("System.Security.SecurityCriticalAttribute");
        internal static readonly KnownType System_Security_SecuritySafeCriticalAttribute = new RegularKnownType("System.Security.SecuritySafeCriticalAttribute");
        internal static readonly KnownType System_SerializableAttribute = new RegularKnownType("System.SerializableAttribute");
        internal static readonly KnownType System_ServiceModel_OperationContractAttribute = new RegularKnownType("System.ServiceModel.OperationContractAttribute");
        internal static readonly KnownType System_ServiceModel_ServiceContractAttribute = new RegularKnownType("System.ServiceModel.ServiceContractAttribute");
        internal static readonly KnownType System_Single = new SpecialKnownType(SpecialType.System_Single, "float");
        internal static readonly KnownType System_StackOverflowException = new RegularKnownType("System.StackOverflowException");
        internal static readonly KnownType System_STAThreadAttribute = new RegularKnownType("System.STAThreadAttribute");
        internal static readonly KnownType System_String = new SpecialKnownType(SpecialType.System_String, "string");
        internal static readonly KnownType System_String_Array = new RegularKnownType("System.String") { IsArray = true};
        internal static readonly KnownType System_StringComparison = new RegularKnownType("System.StringComparison");
        internal static readonly KnownType System_SystemException = new RegularKnownType("System.SystemException");
        internal static readonly KnownType System_Text_RegularExpressions_Regex = new RegularKnownType("System.Text.RegularExpressions.Regex");
        internal static readonly KnownType System_Text_StringBuilder = new RegularKnownType("System.Text.StringBuilder");
        internal static readonly KnownType System_Threading_Monitor = new RegularKnownType("System.Threading.Monitor");
        internal static readonly KnownType System_Threading_Mutex = new RegularKnownType("System.Threading.Mutex");
        internal static readonly KnownType System_Threading_ReaderWriterLock = new RegularKnownType("System.Threading.ReaderWriterLock");
        internal static readonly KnownType System_Threading_ReaderWriterLockSlim = new RegularKnownType("System.Threading.ReaderWriterLockSlim");
        internal static readonly KnownType System_Threading_SpinLock = new RegularKnownType("System.Threading.SpinLock");
        internal static readonly KnownType System_Threading_Tasks_Task = new RegularKnownType("System.Threading.Tasks.Task");
        internal static readonly KnownType System_Threading_Tasks_Task_T = new RegularKnownType("System.Threading.Tasks.Task", "TResult");
        internal static readonly KnownType System_Threading_Tasks_TaskFactory = new RegularKnownType("System.Threading.Tasks.TaskFactory");
        internal static readonly KnownType System_Threading_Tasks_ValueTask = new RegularKnownType("System.Threading.Tasks.ValueTask");
        internal static readonly KnownType System_Threading_Tasks_ValueTask_TResult = new RegularKnownType("System.Threading.Tasks.ValueTask", "TResult");
        internal static readonly KnownType System_Threading_Thread = new RegularKnownType("System.Threading.Thread");
        internal static readonly KnownType System_Threading_WaitHandle = new RegularKnownType("System.Threading.WaitHandle");
        internal static readonly KnownType System_ThreadStaticAttribute = new RegularKnownType("System.ThreadStaticAttribute");
        internal static readonly KnownType System_Type = new RegularKnownType("System.Type");
        internal static readonly KnownType System_UInt16 = new SpecialKnownType(SpecialType.System_UInt16, "ushort");
        internal static readonly KnownType System_UInt32 = new SpecialKnownType(SpecialType.System_UInt32, "uint");
        internal static readonly KnownType System_UInt64 = new SpecialKnownType(SpecialType.System_UInt64, "ulong");
        internal static readonly KnownType System_UIntPtr = new SpecialKnownType(SpecialType.System_UIntPtr, "UIntPtr");
        internal static readonly KnownType System_Uri = new RegularKnownType("System.Uri");
        internal static readonly KnownType System_ValueType = new SpecialKnownType(SpecialType.System_ValueType, "ValueType");
        internal static readonly KnownType System_Web_HttpApplication = new RegularKnownType("System.Web.HttpApplication");
        internal static readonly KnownType System_Web_HttpCookie = new RegularKnownType("System.Web.HttpCookie");
        internal static readonly KnownType System_Web_HttpContext = new RegularKnownType("System.Web.HttpContext");
        internal static readonly KnownType System_Web_HttpResponse = new RegularKnownType("System.Web.HttpResponse");
        internal static readonly KnownType System_Web_HttpResponseBase = new RegularKnownType("System.Web.HttpResponseBase");
        internal static readonly KnownType System_Web_Http_ApiController = new RegularKnownType("System.Web.Http.ApiController");
        internal static readonly KnownType System_Web_Http_Cors_EnableCorsAttribute = new RegularKnownType("System.Web.Http.Cors.EnableCorsAttribute");
        internal static readonly KnownType System_Web_Mvc_NonActionAttribute = new RegularKnownType("System.Web.Mvc.NonActionAttribute");
        internal static readonly KnownType System_Web_Mvc_Controller = new RegularKnownType("System.Web.Mvc.Controller");
        internal static readonly KnownType System_Web_Mvc_HttpPostAttribute = new RegularKnownType("System.Web.Mvc.HttpPostAttribute");
        internal static readonly KnownType System_Web_Mvc_ValidateInputAttribute = new RegularKnownType("System.Web.Mvc.ValidateInputAttribute");
        internal static readonly KnownType System_Web_Script_Serialization_JavaScriptSerializer = new RegularKnownType("System.Web.Script.Serialization.JavaScriptSerializer");
        internal static readonly KnownType System_Web_Script_Serialization_JavaScriptTypeResolver = new RegularKnownType("System.Web.Script.Serialization.JavaScriptTypeResolver");
        internal static readonly KnownType System_Web_Script_Serialization_SimpleTypeResolver = new RegularKnownType("System.Web.Script.Serialization.SimpleTypeResolver");
        internal static readonly KnownType System_Web_UI_LosFormatter = new RegularKnownType("System.Web.UI.LosFormatter");
        internal static readonly KnownType System_Windows_DependencyObject = new RegularKnownType("System.Windows.DependencyObject");
        internal static readonly KnownType System_Windows_Forms_Application = new RegularKnownType("System.Windows.Forms.Application");
        internal static readonly KnownType System_Windows_Markup_ConstructorArgumentAttribute = new RegularKnownType("System.Windows.Markup.ConstructorArgumentAttribute");
        internal static readonly KnownType System_Xml_Serialization_XmlElementAttribute = new RegularKnownType("System.Xml.Serialization.XmlElementAttribute");
        internal static readonly KnownType System_Xml_XmlDocument = new RegularKnownType("System.Xml.XmlDocument");
        internal static readonly KnownType System_Xml_XmlDataDocument = new RegularKnownType("System.Xml.XmlDataDocument");
        internal static readonly KnownType System_Xml_XmlNode = new RegularKnownType("System.Xml.XmlNode");
        internal static readonly KnownType System_Xml_XPath_XPathDocument = new RegularKnownType("System.Xml.XPath.XPathDocument");
        internal static readonly KnownType System_Xml_XmlReader = new RegularKnownType("System.Xml.XmlReader");
        internal static readonly KnownType System_Xml_XmlReaderSettings = new RegularKnownType("System.Xml.XmlReaderSettings");
        internal static readonly KnownType System_Xml_XmlUrlResolver = new RegularKnownType("System.Xml.XmlUrlResolver");
        internal static readonly KnownType System_Xml_XmlTextReader = new RegularKnownType("System.Xml.XmlTextReader");
        internal static readonly KnownType System_Xml_Resolvers_XmlPreloadedResolver = new RegularKnownType("System.Xml.Resolvers.XmlPreloadedResolver");
        internal static readonly KnownType NSubstitute_SubstituteExtensions = new RegularKnownType("NSubstitute.SubstituteExtensions");
        internal static readonly KnownType NSubstitute_Received = new RegularKnownType("NSubstitute.Received");
        internal static readonly ImmutableArray<RegularKnownType> SystemActionVariants =
            ImmutableArray.Create(
                new RegularKnownType("System.Action"),
                new RegularKnownType("System.Action", "T"),
                new RegularKnownType("System.Action", "T1", "T2"),
                new RegularKnownType("System.Action", "T1", "T2", "T3"),
                new RegularKnownType("System.Action", "T1", "T2", "T3", "T4"),
                new RegularKnownType("System.Action", "T1", "T2", "T3", "T4", "T5"),
                new RegularKnownType("System.Action", "T1", "T2", "T3", "T4", "T5", "T6"),
                new RegularKnownType("System.Action", "T1", "T2", "T3", "T4", "T5", "T6", "T7"),
                new RegularKnownType("System.Action", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8"),
                new RegularKnownType("System.Action", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9"),
                new RegularKnownType("System.Action", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10"),
                new RegularKnownType("System.Action", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11"),
                new RegularKnownType("System.Action", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12"),
                new RegularKnownType("System.Action", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12", "T13"),
                new RegularKnownType("System.Action", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12", "T13", "T14"),
                new RegularKnownType("System.Action", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12", "T13", "T14", "T15"),
                new RegularKnownType("System.Action", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12", "T13", "T14", "T15", "T16"));
        internal static readonly ImmutableArray<RegularKnownType> SystemFuncVariants =
            ImmutableArray.Create(
                new RegularKnownType("System.Func", "TResult"),
                new RegularKnownType("System.Func", "T", "TResult"),
                new RegularKnownType("System.Func", "T1", "T2", "TResult"),
                new RegularKnownType("System.Func", "T1", "T2", "T3", "TResult"),
                new RegularKnownType("System.Func", "T1", "T2", "T3", "T4", "TResult"),
                new RegularKnownType("System.Func", "T1", "T2", "T3", "T4", "T5", "TResult"),
                new RegularKnownType("System.Func", "T1", "T2", "T3", "T4", "T5", "T6", "TResult"),
                new RegularKnownType("System.Func", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "TResult"),
                new RegularKnownType("System.Func", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "TResult"),
                new RegularKnownType("System.Func", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "TResult"),
                new RegularKnownType("System.Func", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "TResult"),
                new RegularKnownType("System.Func", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "TResult"),
                new RegularKnownType("System.Func", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12", "TResult"),
                new RegularKnownType("System.Func", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12", "T13", "TResult"),
                new RegularKnownType("System.Func", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12", "T13", "T14", "TResult"),
                new RegularKnownType("System.Func", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12", "T13", "T14", "T15", "TResult"),
                new RegularKnownType("System.Func", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12", "T13", "T14", "T15", "T16", "TResult"));
        internal static readonly ImmutableArray<KnownType> SystemTasks =
            ImmutableArray.Create(
                System_Threading_Tasks_Task,
                System_Threading_Tasks_Task_T,
                System_Threading_Tasks_ValueTask_TResult);
        internal static readonly KnownType Sytem_Resources_ResourceManager = new RegularKnownType("System.Resources.ResourceManager");
        internal static readonly KnownType UnityEditor_AssetModificationProcessor = new RegularKnownType("UnityEditor.AssetModificationProcessor");
        internal static readonly KnownType UnityEditor_AssetPostprocessor = new RegularKnownType("UnityEditor.AssetPostprocessor");
        internal static readonly KnownType UnityEngine_MonoBehaviour = new RegularKnownType("UnityEngine.MonoBehaviour");
        internal static readonly KnownType UnityEngine_ScriptableObject = new RegularKnownType("UnityEngine.ScriptableObject");
        internal static readonly KnownType Xunit_Assert = new RegularKnownType("Xunit.Assert");
        internal static readonly KnownType Xunit_Sdk_AssertException = new RegularKnownType("Xunit.Sdk.AssertException");
        internal static readonly KnownType Xunit_FactAttribute = new RegularKnownType("Xunit.FactAttribute");
        internal static readonly KnownType Xunit_Sdk_XunitException = new RegularKnownType("Xunit.Sdk.XunitException");
        internal static readonly KnownType Xunit_TheoryAttribute = new RegularKnownType("Xunit.TheoryAttribute");
        internal static readonly KnownType LegacyXunit_TheoryAttribute = new RegularKnownType("Xunit.Extensions.TheoryAttribute");
        internal static readonly ImmutableArray<KnownType> CallerInfoAttributes =
            ImmutableArray.Create(
                System_Runtime_CompilerServices_CallerFilePathAttribute,
                System_Runtime_CompilerServices_CallerLineNumberAttribute,
                System_Runtime_CompilerServices_CallerMemberNameAttribute);
        internal static readonly ImmutableArray<KnownType> FloatingPointNumbers =
            ImmutableArray.Create(
                System_Single,
                System_Double);
        internal static readonly ImmutableArray<KnownType> IntegralNumbers =
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
        internal static readonly ImmutableArray<KnownType> IntegralNumbersIncludingNative =
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
        internal static readonly ImmutableArray<KnownType> NonIntegralNumbers =
            ImmutableArray.Create(
                System_Single,
                System_Double,
                System_Decimal);
        internal static readonly ImmutableArray<KnownType> PointerTypes =
            ImmutableArray.Create(
                System_IntPtr,
                System_UIntPtr);
        internal static readonly ImmutableArray<KnownType> UnsignedIntegers =
            ImmutableArray.Create(
                System_UInt64,
                System_UInt32,
                System_UInt16);

        #endregion Known types

#pragma warning restore S103    // Lines should not be too long
#pragma warning restore SA1310  // FieldNamesMustNotContainUnderscore
#pragma warning restore SA1311  // Static readonly fields should begin with upper-case letter
#pragma warning restore SA1307  // Field 'log4net_Config_XmlConfigurator' should begin with upper-case letter
#pragma warning restore SA1304  // Non-private readonly fields should begin with upper-case letter
    }
}

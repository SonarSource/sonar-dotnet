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

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    internal sealed class KnownType
    {
#pragma warning disable S103    // Lines should not be too long
#pragma warning disable SA1310  // FieldNamesMustNotContainUnderscore
#pragma warning disable SA1311  // Static readonly fields should begin with upper-case letter
#pragma warning disable SA1307  // Field 'log4net_Config_XmlConfigurator' should begin with upper-case letter
#pragma warning disable SA1304  // Non-private readonly fields should begin with upper-case letter

        #region Known types

        internal static readonly KnownType Void = new(SpecialType.System_Void, "void");

        internal static readonly KnownType FluentAssertions_Execution_AssertionScope = new("FluentAssertions.Execution.AssertionScope");
        internal static readonly KnownType JWT_Builder_JwtBuilder = new("JWT.Builder.JwtBuilder");
        internal static readonly KnownType JWT_IJwtDecoder = new("JWT.IJwtDecoder");
        internal static readonly KnownType JWT_JwtDecoderExtensions = new("JWT.JwtDecoderExtensions");
        internal static readonly KnownType log4net_Config_XmlConfigurator = new("log4net.Config.XmlConfigurator");
        internal static readonly KnownType log4net_Config_DOMConfigurator = new("log4net.Config.DOMConfigurator");
        internal static readonly KnownType log4net_Config_BasicConfigurator = new("log4net.Config.BasicConfigurator");
        internal static readonly KnownType Microsoft_AspNet_SignalR_Hub = new("Microsoft.AspNet.SignalR.Hub");
        internal static readonly KnownType Microsoft_AspNetCore_Builder_DeveloperExceptionPageExtensions = new("Microsoft.AspNetCore.Builder.DeveloperExceptionPageExtensions");
        internal static readonly KnownType Microsoft_AspNetCore_Builder_DatabaseErrorPageExtensions = new("Microsoft.AspNetCore.Builder.DatabaseErrorPageExtensions");
        internal static readonly KnownType Microsoft_AspNetCore_Cors_Infrastructure_CorsPolicyBuilder = new("Microsoft.AspNetCore.Cors.Infrastructure.CorsPolicyBuilder");
        internal static readonly KnownType Microsoft_AspNetCore_Hosting_HostingEnvironmentExtensions = new("Microsoft.AspNetCore.Hosting.HostingEnvironmentExtensions");
        internal static readonly KnownType Microsoft_AspNetCore_Hosting_WebHostBuilderExtensions = new("Microsoft.AspNetCore.Hosting.WebHostBuilderExtensions");
        internal static readonly KnownType Microsoft_AspNetCore_Http_CookieOptions = new("Microsoft.AspNetCore.Http.CookieOptions");
        internal static readonly KnownType Microsoft_AspNetCore_Http_HeaderDictionaryExtensions = new("Microsoft.AspNetCore.Http.HeaderDictionaryExtensions");
        internal static readonly KnownType Microsoft_AspNetCore_Http_IHeaderDictionary = new("Microsoft.AspNetCore.Http.IHeaderDictionary");
        internal static readonly KnownType Microsoft_AspNetCore_Http_IRequestCookieCollection = new("Microsoft.AspNetCore.Http.IRequestCookieCollection");
        internal static readonly KnownType Microsoft_AspNetCore_Http_IResponseCookies = new("Microsoft.AspNetCore.Http.IResponseCookies");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_Controller = new("Microsoft.AspNetCore.Mvc.Controller");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_ControllerBase = new("Microsoft.AspNetCore.Mvc.ControllerBase");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_ControllerAttribute = new("Microsoft.AspNetCore.Mvc.ControllerAttribute");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_DisableRequestSizeLimitAttribute = new("Microsoft.AspNetCore.Mvc.DisableRequestSizeLimitAttribute");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_FromServicesAttribute = new("Microsoft.AspNetCore.Mvc.FromServicesAttribute");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_IgnoreAntiforgeryTokenAttribute = new("Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_NonActionAttribute = new("Microsoft.AspNetCore.Mvc.NonActionAttribute");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_NonControllerAttribute = new("Microsoft.AspNetCore.Mvc.NonControllerAttribute");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_RequestFormLimitsAttribute = new("Microsoft.AspNetCore.Mvc.RequestFormLimitsAttribute");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_RequestSizeLimitAttribute = new("Microsoft.AspNetCore.Mvc.RequestSizeLimitAttribute");
        internal static readonly KnownType Microsoft_AspNetCore_Razor_Hosting_RazorCompiledItemAttribute = new("Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute");
        internal static readonly KnownType Microsoft_Data_Sqlite_SqliteCommand = new("Microsoft.Data.Sqlite.SqliteCommand");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_DbContextOptionsBuilder = new("Microsoft.EntityFrameworkCore.DbContextOptionsBuilder");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_Migrations_Migration = new("Microsoft.EntityFrameworkCore.Migrations.Migration");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_MySQLDbContextOptionsExtensions = new("Microsoft.EntityFrameworkCore.MySQLDbContextOptionsExtensions");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_NpgsqlDbContextOptionsExtensions = new("Microsoft.EntityFrameworkCore.NpgsqlDbContextOptionsExtensions");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_NpgsqlDbContextOptionsBuilderExtensions = new("Microsoft.EntityFrameworkCore.NpgsqlDbContextOptionsBuilderExtensions");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_OracleDbContextOptionsExtensions = new("Microsoft.EntityFrameworkCore.OracleDbContextOptionsExtensions");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_RawSqlString = new("Microsoft.EntityFrameworkCore.RawSqlString");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_RelationalDatabaseFacadeExtensions = new("Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_RelationalQueryableExtensions = new("Microsoft.EntityFrameworkCore.RelationalQueryableExtensions");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_SqliteDbContextOptionsBuilderExtensions = new("Microsoft.EntityFrameworkCore.SqliteDbContextOptionsBuilderExtensions");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_SqlServerDbContextOptionsExtensions = new("Microsoft.EntityFrameworkCore.SqlServerDbContextOptionsExtensions");
        internal static readonly KnownType Microsoft_Extensions_DependencyInjection_LoggingServiceCollectionExtensions = new("Microsoft.Extensions.DependencyInjection.LoggingServiceCollectionExtensions");
        internal static readonly KnownType Microsoft_Extensions_Hosting_HostEnvironmentEnvExtensions = new("Microsoft.Extensions.Hosting.HostEnvironmentEnvExtensions");
        internal static readonly KnownType Microsoft_Extensions_Logging_AzureAppServicesLoggerFactoryExtensions = new("Microsoft.Extensions.Logging.AzureAppServicesLoggerFactoryExtensions");
        internal static readonly KnownType Microsoft_Extensions_Logging_ConsoleLoggerExtensions = new("Microsoft.Extensions.Logging.ConsoleLoggerExtensions");
        internal static readonly KnownType Microsoft_Extensions_Logging_DebugLoggerFactoryExtensions = new("Microsoft.Extensions.Logging.DebugLoggerFactoryExtensions");
        internal static readonly KnownType Microsoft_Extensions_Logging_EventLoggerFactoryExtensions = new("Microsoft.Extensions.Logging.EventLoggerFactoryExtensions");
        internal static readonly KnownType Microsoft_Extensions_Logging_EventSourceLoggerFactoryExtensions = new("Microsoft.Extensions.Logging.EventSourceLoggerFactoryExtensions");
        internal static readonly KnownType Microsoft_Extensions_Logging_ILoggerFactory = new("Microsoft.Extensions.Logging.ILoggerFactory");
        internal static readonly KnownType Microsoft_Extensions_Primitives_StringValues = new("Microsoft.Extensions.Primitives.StringValues");
        internal static readonly KnownType Microsoft_Net_Http_Headers_HeaderNames = new("Microsoft.Net.Http.Headers.HeaderNames");
        internal static readonly KnownType Microsoft_VisualBasic_Interaction = new("Microsoft.VisualBasic.Interaction");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_Assert = new("Microsoft.VisualStudio.TestTools.UnitTesting.Assert");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_AssertFailedException = new("Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_ExpectedExceptionAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.ExpectedExceptionAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_IgnoreAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_TestClassAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_TestMethodAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_DataTestMethodAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.DataTestMethodAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_WorkItemAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.WorkItemAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_AssemblyCleanupAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.AssemblyCleanupAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_AssemblyInitializeAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.AssemblyInitializeAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_ClassCleanupAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_ClassInitializeAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_TestCleanupAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_TestInitializeAttribute = new("Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute");
        internal static readonly KnownType Microsoft_Web_XmlTransform_XmlFileInfoDocument = new("Microsoft.Web.XmlTransform.XmlFileInfoDocument");
        internal static readonly KnownType Microsoft_Web_XmlTransform_XmlTransformableDocument = new("Microsoft.Web.XmlTransform.XmlTransformableDocument");
        internal static readonly KnownType Mono_Unix_FileAccessPermissions = new("Mono.Unix.FileAccessPermissions");
        internal static readonly KnownType MySql_Data_MySqlClient_MySqlDataAdapter = new("MySql.Data.MySqlClient.MySqlDataAdapter");
        internal static readonly KnownType MySql_Data_MySqlClient_MySqlCommand = new("MySql.Data.MySqlClient.MySqlCommand");
        internal static readonly KnownType MySql_Data_MySqlClient_MySqlHelper = new("MySql.Data.MySqlClient.MySqlHelper");
        internal static readonly KnownType MySql_Data_MySqlClient_MySqlScript = new("MySql.Data.MySqlClient.MySqlScript");
        internal static readonly KnownType Nancy_Cookies_NancyCookie = new("Nancy.Cookies.NancyCookie");
        internal static readonly KnownType NFluent_Check = new("NFluent.Check");
        internal static readonly KnownType NFluent_FluentCheckException = new("NFluent.FluentCheckException");
        internal static readonly KnownType NFluent_Kernel_FluentCheckException = new("NFluent.Kernel.FluentCheckException");
        internal static readonly KnownType NLog_LogManager = new("NLog.LogManager");
        internal static readonly KnownType NUnit_Framework_Assert = new("NUnit.Framework.Assert");
        internal static readonly KnownType NUnit_Framework_AssertionException = new("NUnit.Framework.AssertionException");
        internal static readonly KnownType NUnit_Framework_ExpectedExceptionAttribute = new("NUnit.Framework.ExpectedExceptionAttribute");
        internal static readonly KnownType NUnit_Framework_IgnoreAttribute = new("NUnit.Framework.IgnoreAttribute");
        internal static readonly KnownType NUnit_Framework_ITestBuilderInterface = new("NUnit.Framework.Interfaces.ITestBuilder");
        internal static readonly KnownType NUnit_Framework_TestAttribute = new("NUnit.Framework.TestAttribute");
        internal static readonly KnownType NUnit_Framework_TestCaseAttribute = new("NUnit.Framework.TestCaseAttribute");
        internal static readonly KnownType NUnit_Framework_TestCaseSourceAttribute = new("NUnit.Framework.TestCaseSourceAttribute");
        internal static readonly KnownType NUnit_Framework_TestFixtureAttribute = new("NUnit.Framework.TestFixtureAttribute");
        internal static readonly KnownType NUnit_Framework_TheoryAttribute = new("NUnit.Framework.TheoryAttribute");
        internal static readonly KnownType Org_BouncyCastle_Asn1_Nist_NistNamedCurves = new("Org.BouncyCastle.Asn1.Nist.NistNamedCurves");
        internal static readonly KnownType Org_BouncyCastle_Asn1_Sec_SecNamedCurves = new("Org.BouncyCastle.Asn1.Sec.SecNamedCurves");
        internal static readonly KnownType Org_BouncyCastle_Asn1_TeleTrust_TeleTrusTNamedCurves = new("Org.BouncyCastle.Asn1.TeleTrust.TeleTrusTNamedCurves");
        internal static readonly KnownType Org_BouncyCastle_Asn1_X9_ECNamedCurveTable = new("Org.BouncyCastle.Asn1.X9.ECNamedCurveTable");
        internal static readonly KnownType Org_BouncyCastle_Asn1_X9_X962NamedCurves = new("Org.BouncyCastle.Asn1.X9.X962NamedCurves");
        internal static readonly KnownType Org_BouncyCastle_Crypto_Engines_AesFastEngine = new("Org.BouncyCastle.Crypto.Engines.AesFastEngine");
        internal static readonly KnownType Org_BouncyCastle_Crypto_Generators_DHParametersGenerator = new("Org.BouncyCastle.Crypto.Generators.DHParametersGenerator");
        internal static readonly KnownType Org_BouncyCastle_Crypto_Generators_DsaParametersGenerator = new("Org.BouncyCastle.Crypto.Generators.DsaParametersGenerator");
        internal static readonly KnownType Org_BouncyCastle_Crypto_Parameters_RsaKeyGenerationParameters = new("Org.BouncyCastle.Crypto.Parameters.RsaKeyGenerationParameters");
        internal static readonly KnownType Serilog_LoggerConfiguration = new("Serilog.LoggerConfiguration");
        internal static readonly KnownType System_Action = new("System.Action");
        internal static readonly KnownType System_Action_T = new("System.Action<T>");
        internal static readonly KnownType System_Action_T1_T2 = new("System.Action<T1, T2>");
        internal static readonly KnownType System_Action_T1_T2_T3 = new("System.Action<T1, T2, T3>");
        internal static readonly KnownType System_Action_T1_T2_T3_T4 = new("System.Action<T1, T2, T3, T4>");
        internal static readonly KnownType System_Activator = new("System.Activator");
        internal static readonly KnownType System_ApplicationException = new("System.ApplicationException");
        internal static readonly KnownType System_AppDomain = new("System.AppDomain");
        internal static readonly KnownType System_ArgumentException = new("System.ArgumentException");
        internal static readonly KnownType System_ArgumentNullException = new("System.ArgumentNullException");
        internal static readonly KnownType System_ArgumentOutOfRangeException = new("System.ArgumentOutOfRangeException");
        internal static readonly KnownType System_Array = new(SpecialType.System_Array, "System.Array");
        internal static readonly KnownType System_Attribute = new("System.Attribute");
        internal static readonly KnownType System_AttributeUsageAttribute = new("System.AttributeUsageAttribute");
        internal static readonly KnownType System_Boolean = new(SpecialType.System_Boolean, "bool");
        internal static readonly KnownType System_Byte = new(SpecialType.System_Byte, "byte");
        internal static readonly KnownType System_Byte_Array = new("byte[]");
        internal static readonly KnownType System_Char = new(SpecialType.System_Char, "char");
        internal static readonly KnownType System_CLSCompliantAttribute = new("System.CLSCompliantAttribute");
        internal static readonly KnownType System_CodeDom_Compiler_GeneratedCodeAttribute = new("System.CodeDom.Compiler.GeneratedCodeAttribute");
        internal static readonly KnownType System_Collections_CollectionBase = new("System.Collections.CollectionBase");
        internal static readonly KnownType System_Collections_DictionaryBase = new("System.Collections.DictionaryBase");
        internal static readonly KnownType System_Collections_Generic_Dictionary_TKey_TValue = new("System.Collections.Generic.Dictionary<TKey, TValue>");
        internal static readonly KnownType System_Collections_Generic_HashSet_T = new("System.Collections.Generic.HashSet<T>");
        internal static readonly KnownType System_Collections_Generic_IAsyncEnumerable_T = new("System.Collections.Generic.IAsyncEnumerable<T>");
        internal static readonly KnownType System_Collections_Generic_ICollection_T = new(SpecialType.System_Collections_Generic_ICollection_T, "System.Collections.Generic.ICollection<T>");
        internal static readonly KnownType System_Collections_Generic_IDictionary_TKey_TValue = new("System.Collections.Generic.IDictionary<TKey, TValue>");
        internal static readonly KnownType System_Collections_Generic_IDictionary_TKey_TValue_VB = new("System.Collections.Generic.IDictionary(Of TKey, TValue)");
        internal static readonly KnownType System_Collections_Generic_IEnumerable_T = new(SpecialType.System_Collections_Generic_IEnumerable_T, "System.Collections.Generic.IEnumerable<T>");
        internal static readonly KnownType System_Collections_Generic_IList_T = new(SpecialType.System_Collections_Generic_IList_T, "System.Collections.Generic.IList<T>");
        internal static readonly KnownType System_Collections_Generic_IReadOnlyCollection_T = new(SpecialType.System_Collections_Generic_IReadOnlyCollection_T, "System.Collections.Generic.IReadOnlyCollection<T>");
        internal static readonly KnownType System_Collections_Generic_ISet_T = new("System.Collections.Generic.ISet<T>");
        internal static readonly KnownType System_Collections_Generic_KeyValuePair_TKey_TValue = new("System.Collections.Generic.KeyValuePair<TKey, TValue>");
        internal static readonly KnownType System_Collections_Generic_List_T = new("System.Collections.Generic.List<T>");
        internal static readonly KnownType System_Collections_Generic_Queue_T = new("System.Collections.Generic.Queue<T>");
        internal static readonly KnownType System_Collections_Generic_Stack_T = new("System.Collections.Generic.Stack<T>");
        internal static readonly KnownType System_Collections_ICollection = new("System.Collections.ICollection");
        internal static readonly KnownType System_Collections_IEnumerable = new(SpecialType.System_Collections_IEnumerable, "System.Collections.IEnumerable");
        internal static readonly KnownType System_Collections_IList = new("System.Collections.IList");
        internal static readonly KnownType System_Collections_Immutable_IImmutableArray_T = new("System.Collections.Immutable.IImmutableArray<T>");
        internal static readonly KnownType System_Collections_Immutable_IImmutableDictionary_TKey_TValue = new("System.Collections.Immutable.IImmutableDictionary<TKey, TValue>");
        internal static readonly KnownType System_Collections_Immutable_IImmutableList_T = new("System.Collections.Immutable.IImmutableList<T>");
        internal static readonly KnownType System_Collections_Immutable_IImmutableQueue_T = new("System.Collections.Immutable.IImmutableQueue<T>");
        internal static readonly KnownType System_Collections_Immutable_IImmutableSet_T = new("System.Collections.Immutable.IImmutableSet<T>");
        internal static readonly KnownType System_Collections_Immutable_IImmutableStack_T = new("System.Collections.Immutable.IImmutableStack<T>");
        internal static readonly KnownType System_Collections_Immutable_ImmutableArray = new("System.Collections.Immutable.ImmutableArray");
        internal static readonly KnownType System_Collections_Immutable_ImmutableArray_T = new("System.Collections.Immutable.ImmutableArray<T>");
        internal static readonly KnownType System_Collections_Immutable_ImmutableDictionary = new("System.Collections.Immutable.ImmutableDictionary");
        internal static readonly KnownType System_Collections_Immutable_ImmutableDictionary_TKey_TValue = new("System.Collections.Immutable.ImmutableDictionary<TKey, TValue>");
        internal static readonly KnownType System_Collections_Immutable_ImmutableHashSet = new("System.Collections.Immutable.ImmutableHashSet");
        internal static readonly KnownType System_Collections_Immutable_ImmutableHashSet_T = new("System.Collections.Immutable.ImmutableHashSet<T>");
        internal static readonly KnownType System_Collections_Immutable_ImmutableList = new("System.Collections.Immutable.ImmutableList");
        internal static readonly KnownType System_Collections_Immutable_ImmutableList_T = new("System.Collections.Immutable.ImmutableList<T>");
        internal static readonly KnownType System_Collections_Immutable_ImmutableQueue = new("System.Collections.Immutable.ImmutableQueue");
        internal static readonly KnownType System_Collections_Immutable_ImmutableQueue_T = new("System.Collections.Immutable.ImmutableQueue<T>");
        internal static readonly KnownType System_Collections_Immutable_ImmutableSortedDictionary = new("System.Collections.Immutable.ImmutableSortedDictionary");
        internal static readonly KnownType System_Collections_Immutable_ImmutableSortedDictionary_TKey_TValue = new("System.Collections.Immutable.ImmutableSortedDictionary<TKey, TValue>");
        internal static readonly KnownType System_Collections_Immutable_ImmutableSortedSet = new("System.Collections.Immutable.ImmutableSortedSet");
        internal static readonly KnownType System_Collections_Immutable_ImmutableSortedSet_T = new("System.Collections.Immutable.ImmutableSortedSet<T>");
        internal static readonly KnownType System_Collections_Immutable_ImmutableStack = new("System.Collections.Immutable.ImmutableStack");
        internal static readonly KnownType System_Collections_Immutable_ImmutableStack_T = new("System.Collections.Immutable.ImmutableStack<T>");
        internal static readonly KnownType System_Collections_ObjectModel_Collection_T = new("System.Collections.ObjectModel.Collection<T>");
        internal static readonly KnownType System_Collections_ObjectModel_ObservableCollection_T = new("System.Collections.ObjectModel.ObservableCollection<T>");
        internal static readonly KnownType System_Collections_ObjectModel_ReadOnlyCollection_T = new("System.Collections.ObjectModel.ReadOnlyCollection<T>");
        internal static readonly KnownType System_Collections_ObjectModel_ReadOnlyDictionary_TKey_TValue = new("System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>");
        internal static readonly KnownType System_Collections_Queue = new("System.Collections.Queue");
        internal static readonly KnownType System_Collections_ReadOnlyCollectionBase = new("System.Collections.ReadOnlyCollectionBase");
        internal static readonly KnownType System_Collections_SortedList = new("System.Collections.SortedList");
        internal static readonly KnownType System_Collections_Stack = new("System.Collections.Stack");
        internal static readonly KnownType System_Collections_Specialized_NameObjectCollectionBase = new("System.Collections.Specialized.NameObjectCollectionBase");
        internal static readonly KnownType System_Collections_Specialized_NameValueCollection = new("System.Collections.Specialized.NameValueCollection");
        internal static readonly KnownType System_ComponentModel_DefaultValueAttribute = new("System.ComponentModel.DefaultValueAttribute");
        internal static readonly KnownType System_ComponentModel_EditorBrowsableAttribute = new("System.ComponentModel.EditorBrowsableAttribute");
        internal static readonly KnownType System_ComponentModel_LocalizableAttribute = new("System.ComponentModel.LocalizableAttribute");
        internal static readonly KnownType System_ComponentModel_Composition_CreationPolicy = new("System.ComponentModel.Composition.CreationPolicy");
        internal static readonly KnownType System_ComponentModel_Composition_ExportAttribute = new("System.ComponentModel.Composition.ExportAttribute");
        internal static readonly KnownType System_ComponentModel_Composition_InheritedExportAttribute = new("System.ComponentModel.Composition.InheritedExportAttribute");
        internal static readonly KnownType System_ComponentModel_Composition_PartCreationPolicyAttribute = new("System.ComponentModel.Composition.PartCreationPolicyAttribute");
        internal static readonly KnownType System_Configuration_ConfigXmlDocument = new("System.Configuration.ConfigXmlDocument");
        internal static readonly KnownType System_Console = new("System.Console");
        internal static readonly KnownType System_Data_Common_CommandTrees_DbExpression = new("System.Data.Common.CommandTrees.DbExpression");
        internal static readonly KnownType System_Data_DataSet = new("System.Data.DataSet");
        internal static readonly KnownType System_Data_DataTable = new("System.Data.DataTable");
        internal static readonly KnownType System_Data_Odbc_OdbcCommand = new("System.Data.Odbc.OdbcCommand");
        internal static readonly KnownType System_Data_Odbc_OdbcDataAdapter = new("System.Data.Odbc.OdbcDataAdapter");
        internal static readonly KnownType System_Data_OracleClient_OracleCommand = new("System.Data.OracleClient.OracleCommand");
        internal static readonly KnownType System_Data_OracleClient_OracleDataAdapter = new("System.Data.OracleClient.OracleDataAdapter");
        internal static readonly KnownType System_Data_SqlClient_SqlCommand = new("System.Data.SqlClient.SqlCommand");
        internal static readonly KnownType System_Data_SqlClient_SqlDataAdapter = new("System.Data.SqlClient.SqlDataAdapter");
        internal static readonly KnownType System_Data_Sqlite_SqliteCommand = new("System.Data.SQLite.SQLiteCommand");
        internal static readonly KnownType System_Data_Sqlite_SQLiteDataAdapter = new("System.Data.SQLite.SQLiteDataAdapter");
        internal static readonly KnownType System_Data_SqlServerCe_SqlCeCommand = new("System.Data.SqlServerCe.SqlCeCommand");
        internal static readonly KnownType System_Data_SqlServerCe_SqlCeDataAdapter = new("System.Data.SqlServerCe.SqlCeDataAdapter");
        internal static readonly KnownType System_DateTime = new(SpecialType.System_DateTime, "DateTime");
        internal static readonly KnownType System_DateTimeOffset = new("System.DateTimeOffset");
        internal static readonly KnownType System_Decimal = new(SpecialType.System_Decimal, "decimal");
        internal static readonly KnownType System_Delegate = new("System.Delegate");
        internal static readonly KnownType System_Diagnostics_CodeAnalysis_ExcludeFromCodeCoverageAttribute = new("System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute");
        internal static readonly KnownType System_Diagnostics_CodeAnalysis_SuppressMessageAttribute = new("System.Diagnostics.CodeAnalysis.SuppressMessageAttribute");
        internal static readonly KnownType System_Diagnostics_ConditionalAttribute = new("System.Diagnostics.ConditionalAttribute");
        internal static readonly KnownType System_Diagnostics_Contracts_PureAttribute = new("System.Diagnostics.Contracts.PureAttribute");
        internal static readonly KnownType System_Diagnostics_Debug = new("System.Diagnostics.Debug");
        internal static readonly KnownType System_Diagnostics_DebuggerDisplayAttribute = new("System.Diagnostics.DebuggerDisplayAttribute");
        internal static readonly KnownType System_Diagnostics_Process = new("System.Diagnostics.Process");
        internal static readonly KnownType System_Diagnostics_ProcessStartInfo = new("System.Diagnostics.ProcessStartInfo");
        internal static readonly KnownType System_Diagnostics_Trace = new("System.Diagnostics.Trace");
        internal static readonly KnownType System_Diagnostics_TraceSource = new("System.Diagnostics.TraceSource");
        internal static readonly KnownType System_DirectoryServices_AuthenticationTypes = new("System.DirectoryServices.AuthenticationTypes");
        internal static readonly KnownType System_DirectoryServices_DirectoryEntry = new("System.DirectoryServices.DirectoryEntry");
        internal static readonly KnownType System_Double = new(SpecialType.System_Double, "double");
        internal static readonly KnownType System_Drawing_Bitmap = new("System.Drawing.Bitmap");
        internal static readonly KnownType System_Drawing_Image = new("System.Drawing.Image");
        internal static readonly KnownType System_DuplicateWaitObjectException = new("System.DuplicateWaitObjectException");
        internal static readonly KnownType System_Enum = new(SpecialType.System_Enum, "Enum");
        internal static readonly KnownType System_Environment = new("System.Environment");
        internal static readonly KnownType System_EventArgs = new("System.EventArgs");
        internal static readonly KnownType System_EventHandler = new("System.EventHandler");
        internal static readonly KnownType System_EventHandler_TEventArgs = new("System.EventHandler<TEventArgs>");
        internal static readonly KnownType System_Exception = new("System.Exception");
        internal static readonly KnownType System_ExecutionEngineException = new("System.ExecutionEngineException");
        internal static readonly KnownType System_FlagsAttribute = new("System.FlagsAttribute");
        internal static readonly KnownType System_Func_TResult = new("System.Func<TResult>");
        internal static readonly KnownType System_Func_T_TResult = new("System.Func<T, TResult>");
        internal static readonly KnownType System_Func_T1_T2_TResult = new("System.Func<T1, T2, TResult>");
        internal static readonly KnownType System_Func_T1_T2_T3_TResult = new("System.Func<T1, T2, T3, TResult>");
        internal static readonly KnownType System_Func_T1_T2_T3_T4_TResult = new("System.Func<T1, T2, T3, T4, TResult>");
        internal static readonly KnownType System_Func_T1_T2_T3_T4_TResult_VB = new("System.Func(Of In T1, In T2, In T3, In T4, Out TResult)");
        internal static readonly KnownType System_GC = new("System.GC");
        internal static readonly KnownType System_Globalization_CompareOptions = new("System.Globalization.CompareOptions");
        internal static readonly KnownType System_Globalization_CultureInfo = new("System.Globalization.CultureInfo");
        internal static readonly KnownType System_Guid = new("System.Guid");
        internal static readonly KnownType System_IAsyncDisposable = new("System.IAsyncDisposable");
        internal static readonly KnownType System_IComparable = new("System.IComparable");
        internal static readonly KnownType System_IComparable_T = new("System.IComparable<T>");
        internal static readonly KnownType System_IdentityModel_Tokens_SecurityTokenHandler = new("System.IdentityModel.Tokens.SecurityTokenHandler");
        internal static readonly KnownType System_IDisposable = new(SpecialType.System_IDisposable, "System.IDisposable");
        internal static readonly KnownType System_IEquatable_T = new("System.IEquatable<T>");
        internal static readonly KnownType System_IFormatProvider = new("System.IFormatProvider");
        internal static readonly KnownType System_Index = new("System.Index");
        internal static readonly KnownType System_IndexOutOfRangeException = new("System.IndexOutOfRangeException");
        internal static readonly KnownType System_Int16 = new(SpecialType.System_Int16, "short");
        internal static readonly KnownType System_Int32 = new(SpecialType.System_Int32, "int");
        internal static readonly KnownType System_Int64 = new(SpecialType.System_Int64, "long");
        internal static readonly KnownType System_IntPtr = new(SpecialType.System_IntPtr, "IntPtr");
        internal static readonly KnownType System_InvalidOperationException = new("System.InvalidOperationException");
        internal static readonly KnownType System_IO_Compression_ZipFile = new("System.IO.Compression.ZipFile");
        internal static readonly KnownType System_IO_Compression_ZipFileExtensions = new("System.IO.Compression.ZipFileExtensions");
        internal static readonly KnownType System_IO_FileStream = new("System.IO.FileStream");
        internal static readonly KnownType System_IO_Path = new("System.IO.Path");
        internal static readonly KnownType System_IO_Stream = new("System.IO.Stream");
        internal static readonly KnownType System_IO_StreamReader = new("System.IO.StreamReader");
        internal static readonly KnownType System_IO_StreamWriter = new("System.IO.StreamWriter");
        internal static readonly KnownType System_IO_TextWriter = new("System.IO.TextWriter");
        internal static readonly KnownType System_Lazy = new("System.Lazy<T>");
        internal static readonly KnownType System_Linq_Enumerable = new("System.Linq.Enumerable");
        internal static readonly KnownType System_Linq_Expressions_Expression_T = new("System.Linq.Expressions.Expression<TDelegate>");
        internal static readonly KnownType System_Linq_ImmutableArrayExtensions = new("System.Linq.ImmutableArrayExtensions");
        internal static readonly KnownType System_MarshalByRefObject = new("System.MarshalByRefObject");
        internal static readonly KnownType System_MTAThreadAttribute = new("System.MTAThreadAttribute");
        internal static readonly KnownType System_Net_FtpWebRequest = new("System.Net.FtpWebRequest");
        internal static readonly KnownType System_Net_Http_Headers_HttpHeaders = new("System.Net.Http.Headers.HttpHeaders");
        internal static readonly KnownType System_Net_Http_HttpClientHandler = new("System.Net.Http.HttpClientHandler");
        internal static readonly KnownType System_Net_Mail_SmtpClient = new("System.Net.Mail.SmtpClient");
        internal static readonly KnownType System_Net_NetworkCredential = new("System.Net.NetworkCredential");
        internal static readonly KnownType System_Net_Security_RemoteCertificateValidationCallback = new("System.Net.Security.RemoteCertificateValidationCallback");
        internal static readonly KnownType System_Net_Security_SslPolicyErrors = new("System.Net.Security.SslPolicyErrors");
        internal static readonly KnownType System_Net_SecurityProtocolType = new("System.Net.SecurityProtocolType");
        internal static readonly KnownType System_Net_Sockets_Socket = new("System.Net.Sockets.Socket");
        internal static readonly KnownType System_Net_Sockets_TcpClient = new("System.Net.Sockets.TcpClient");
        internal static readonly KnownType System_Net_Sockets_UdpClient = new("System.Net.Sockets.UdpClient");
        internal static readonly KnownType System_Net_WebClient = new("System.Net.WebClient");
        internal static readonly KnownType System_NotImplementedException = new("System.NotImplementedException");
        internal static readonly KnownType System_NotSupportedException = new("System.NotSupportedException");
        internal static readonly KnownType System_Nullable_T = new(SpecialType.System_Nullable_T, "System.Nullable<T>");
        internal static readonly KnownType System_NullReferenceException = new("System.NullReferenceException");
        internal static readonly KnownType System_Object = new(SpecialType.System_Object, "object");
        internal static readonly KnownType System_ObsoleteAttribute = new("System.ObsoleteAttribute");
        internal static readonly KnownType System_OutOfMemoryException = new("System.OutOfMemoryException");
        internal static readonly KnownType System_Random = new("System.Random");
        internal static readonly KnownType System_Range = new("System.Range");
        internal static readonly KnownType System_Reflection_Assembly = new("System.Reflection.Assembly");
        internal static readonly KnownType System_Reflection_BindingFlags = new("System.Reflection.BindingFlags");
        internal static readonly KnownType System_Reflection_AssemblyVersionAttribute = new("System.Reflection.AssemblyVersionAttribute");
        internal static readonly KnownType System_Reflection_MemberInfo = new("System.Reflection.MemberInfo");
        internal static readonly KnownType System_Reflection_Module = new("System.Reflection.Module");
        internal static readonly KnownType System_Reflection_ParameterInfo = new("System.Reflection.ParameterInfo");
        internal static readonly KnownType System_Resources_NeutralResourcesLanguageAttribute = new("System.Resources.NeutralResourcesLanguageAttribute");
        internal static readonly KnownType System_Runtime_CompilerServices_CallerFilePathAttribute = new("System.Runtime.CompilerServices.CallerFilePathAttribute");
        internal static readonly KnownType System_Runtime_CompilerServices_CallerLineNumberAttribute = new("System.Runtime.CompilerServices.CallerLineNumberAttribute");
        internal static readonly KnownType System_Runtime_CompilerServices_CallerMemberNameAttribute = new("System.Runtime.CompilerServices.CallerMemberNameAttribute");
        internal static readonly KnownType System_Runtime_CompilerServices_InternalsVisibleToAttribute = new("System.Runtime.CompilerServices.InternalsVisibleToAttribute");
        internal static readonly KnownType System_Runtime_CompilerServices_ModuleInitializerAttribute = new("System.Runtime.CompilerServices.ModuleInitializerAttribute");
        internal static readonly KnownType System_Runtime_CompilerServices_ValueTaskAwaiter = new("System.Runtime.CompilerServices.ValueTaskAwaiter");
        internal static readonly KnownType System_Runtime_CompilerServices_ValueTaskAwaiter_TResult = new("System.Runtime.CompilerServices.ValueTaskAwaiter<TResult>");
        internal static readonly KnownType System_Runtime_CompilerServices_TaskAwaiter = new("System.Runtime.CompilerServices.TaskAwaiter");
        internal static readonly KnownType System_Runtime_CompilerServices_TaskAwaiter_TResult = new("System.Runtime.CompilerServices.TaskAwaiter<TResult>");
        internal static readonly KnownType System_Runtime_InteropServices_ComImportAttribute = new("System.Runtime.InteropServices.ComImportAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_ComVisibleAttribute = new("System.Runtime.InteropServices.ComVisibleAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_DefaultParameterValueAttribute = new("System.Runtime.InteropServices.DefaultParameterValueAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_DllImportAttribute = new("System.Runtime.InteropServices.DllImportAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_HandleRef = new("System.Runtime.InteropServices.HandleRef");
        internal static readonly KnownType System_Runtime_InteropServices_InterfaceTypeAttribute = new("System.Runtime.InteropServices.InterfaceTypeAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_OptionalAttribute = new("System.Runtime.InteropServices.OptionalAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_SafeHandle = new("System.Runtime.InteropServices.SafeHandle");
        internal static readonly KnownType System_Runtime_InteropServices_StructLayoutAttribute = new("System.Runtime.InteropServices.StructLayoutAttribute");
        internal static readonly KnownType System_Runtime_Serialization_DataMemberAttribute = new("System.Runtime.Serialization.DataMemberAttribute");
        internal static readonly KnownType System_Runtime_Serialization_Formatters_Binary_BinaryFormatter = new("System.Runtime.Serialization.Formatters.Binary.BinaryFormatter");
        internal static readonly KnownType System_Runtime_Serialization_Formatters_Soap_SoapFormatter = new("System.Runtime.Serialization.Formatters.Soap.SoapFormatter");
        internal static readonly KnownType System_Runtime_Serialization_ISerializable = new("System.Runtime.Serialization.ISerializable");
        internal static readonly KnownType System_Runtime_Serialization_IDeserializationCallback = new("System.Runtime.Serialization.IDeserializationCallback");
        internal static readonly KnownType System_Runtime_Serialization_NetDataContractSerializer = new("System.Runtime.Serialization.NetDataContractSerializer");
        internal static readonly KnownType System_Runtime_Serialization_OnDeserializedAttribute = new("System.Runtime.Serialization.OnDeserializedAttribute");
        internal static readonly KnownType System_Runtime_Serialization_OnDeserializingAttribute = new("System.Runtime.Serialization.OnDeserializingAttribute");
        internal static readonly KnownType System_Runtime_Serialization_OnSerializedAttribute = new("System.Runtime.Serialization.OnSerializedAttribute");
        internal static readonly KnownType System_Runtime_Serialization_OnSerializingAttribute = new("System.Runtime.Serialization.OnSerializingAttribute");
        internal static readonly KnownType System_Runtime_Serialization_OptionalFieldAttribute = new("System.Runtime.Serialization.OptionalFieldAttribute");
        internal static readonly KnownType System_Runtime_Serialization_SerializationInfo = new("System.Runtime.Serialization.SerializationInfo");
        internal static readonly KnownType System_Runtime_Serialization_StreamingContext = new("System.Runtime.Serialization.StreamingContext");
        internal static readonly KnownType System_SByte = new(SpecialType.System_SByte, "sbyte");
        internal static readonly KnownType System_Security_AccessControl_FileSystemAccessRule = new("System.Security.AccessControl.FileSystemAccessRule");
        internal static readonly KnownType System_Security_AccessControl_FileSystemSecurity = new("System.Security.AccessControl.FileSystemSecurity");
        internal static readonly KnownType System_Security_AllowPartiallyTrustedCallersAttribute = new("System.Security.AllowPartiallyTrustedCallersAttribute");
        internal static readonly KnownType System_Security_Authentication_SslProtocols = new("System.Security.Authentication.SslProtocols");
        internal static readonly KnownType System_Security_Cryptography_AesManaged = new("System.Security.Cryptography.AesManaged");
        internal static readonly KnownType System_Security_Cryptography_AsymmetricAlgorithm = new("System.Security.Cryptography.AsymmetricAlgorithm");
        internal static readonly KnownType System_Security_Cryptography_AsymmetricKeyExchangeDeformatter = new("System.Security.Cryptography.AsymmetricKeyExchangeDeformatter");
        internal static readonly KnownType System_Security_Cryptography_AsymmetricKeyExchangeFormatter = new("System.Security.Cryptography.AsymmetricKeyExchangeFormatter");
        internal static readonly KnownType System_Security_Cryptography_AsymmetricSignatureDeformatter = new("System.Security.Cryptography.AsymmetricSignatureDeformatter");
        internal static readonly KnownType System_Security_Cryptography_AsymmetricSignatureFormatter = new("System.Security.Cryptography.AsymmetricSignatureFormatter");
        internal static readonly KnownType System_Security_Cryptography_CryptoConfig = new("System.Security.Cryptography.CryptoConfig");
        internal static readonly KnownType System_Security_Cryptography_CspParameters = new("System.Security.Cryptography.CspParameters");
        internal static readonly KnownType System_Security_Cryptography_DES = new("System.Security.Cryptography.DES");
        internal static readonly KnownType System_Security_Cryptography_DeriveBytes = new("System.Security.Cryptography.DeriveBytes");
        internal static readonly KnownType System_Security_Cryptography_DSA = new("System.Security.Cryptography.DSA");
        internal static readonly KnownType System_Security_Cryptography_DSACryptoServiceProvider = new("System.Security.Cryptography.DSACryptoServiceProvider");
        internal static readonly KnownType System_Security_Cryptography_ECDiffieHellman = new("System.Security.Cryptography.ECDiffieHellman");
        internal static readonly KnownType System_Security_Cryptography_ECDsa = new("System.Security.Cryptography.ECDsa");
        internal static readonly KnownType System_Security_Cryptography_HashAlgorithm = new("System.Security.Cryptography.HashAlgorithm");
        internal static readonly KnownType System_Security_Cryptography_HMAC = new("System.Security.Cryptography.HMAC");
        internal static readonly KnownType System_Security_Cryptography_HMACMD5 = new("System.Security.Cryptography.HMACMD5");
        internal static readonly KnownType System_Security_Cryptography_HMACRIPEMD160 = new("System.Security.Cryptography.HMACRIPEMD160");
        internal static readonly KnownType System_Security_Cryptography_HMACSHA1 = new("System.Security.Cryptography.HMACSHA1");
        internal static readonly KnownType System_Security_Cryptography_ICryptoTransform = new("System.Security.Cryptography.ICryptoTransform");
        internal static readonly KnownType System_Security_Cryptography_KeyedHashAlgorithm = new("System.Security.Cryptography.KeyedHashAlgorithm");
        internal static readonly KnownType System_Security_Cryptography_MD5 = new("System.Security.Cryptography.MD5");
        internal static readonly KnownType System_Security_Cryptography_PasswordDeriveBytes = new("System.Security.Cryptography.PasswordDeriveBytes");
        internal static readonly KnownType System_Security_Cryptography_RC2 = new("System.Security.Cryptography.RC2");
        internal static readonly KnownType System_Security_Cryptography_RandomNumberGenerator = new("System.Security.Cryptography.RandomNumberGenerator");
        internal static readonly KnownType System_Security_Cryptography_Rfc2898DeriveBytes = new("System.Security.Cryptography.Rfc2898DeriveBytes");
        internal static readonly KnownType System_Security_Cryptography_RIPEMD160 = new("System.Security.Cryptography.RIPEMD160");
        internal static readonly KnownType System_Security_Cryptography_RNGCryptoServiceProvider = new("System.Security.Cryptography.RNGCryptoServiceProvider");
        internal static readonly KnownType System_Security_Cryptography_RSA = new("System.Security.Cryptography.RSA");
        internal static readonly KnownType System_Security_Cryptography_RSACryptoServiceProvider = new("System.Security.Cryptography.RSACryptoServiceProvider");
        internal static readonly KnownType System_Security_Cryptography_SHA1 = new("System.Security.Cryptography.SHA1");
        internal static readonly KnownType System_Security_Cryptography_SymmetricAlgorithm = new("System.Security.Cryptography.SymmetricAlgorithm");
        internal static readonly KnownType System_Security_Cryptography_TripleDES = new("System.Security.Cryptography.TripleDES");
        internal static readonly KnownType System_Security_Cryptography_X509Certificates_X509Certificate2 = new("System.Security.Cryptography.X509Certificates.X509Certificate2");
        internal static readonly KnownType System_Security_Cryptography_X509Certificates_X509Chain = new("System.Security.Cryptography.X509Certificates.X509Chain");
        internal static readonly KnownType System_Security_Permissions_CodeAccessSecurityAttribute = new("System.Security.Permissions.CodeAccessSecurityAttribute");
        internal static readonly KnownType System_Security_Permissions_PrincipalPermission = new("System.Security.Permissions.PrincipalPermission");
        internal static readonly KnownType System_Security_Permissions_PrincipalPermissionAttribute = new("System.Security.Permissions.PrincipalPermissionAttribute");
        internal static readonly KnownType System_Security_PermissionSet = new("System.Security.PermissionSet");
        internal static readonly KnownType System_Security_Principal_IIdentity = new("System.Security.Principal.IIdentity");
        internal static readonly KnownType System_Security_Principal_IPrincipal = new("System.Security.Principal.IPrincipal");
        internal static readonly KnownType System_Security_Principal_NTAccount = new("System.Security.Principal.NTAccount");
        internal static readonly KnownType System_Security_Principal_SecurityIdentifier = new("System.Security.Principal.SecurityIdentifier");
        internal static readonly KnownType System_Security_Principal_WindowsIdentity = new("System.Security.Principal.WindowsIdentity");
        internal static readonly KnownType System_Security_SecurityCriticalAttribute = new("System.Security.SecurityCriticalAttribute");
        internal static readonly KnownType System_Security_SecuritySafeCriticalAttribute = new("System.Security.SecuritySafeCriticalAttribute");
        internal static readonly KnownType System_SerializableAttribute = new("System.SerializableAttribute");
        internal static readonly KnownType System_ServiceModel_OperationContractAttribute = new("System.ServiceModel.OperationContractAttribute");
        internal static readonly KnownType System_ServiceModel_ServiceContractAttribute = new("System.ServiceModel.ServiceContractAttribute");
        internal static readonly KnownType System_Single = new(SpecialType.System_Single, "float");
        internal static readonly KnownType System_StackOverflowException = new("System.StackOverflowException");
        internal static readonly KnownType System_STAThreadAttribute = new("System.STAThreadAttribute");
        internal static readonly KnownType System_String = new(SpecialType.System_String, "string");
        internal static readonly KnownType System_String_Array = new("string[]");
        internal static readonly KnownType System_String_Array_VB = new("String()");
        internal static readonly KnownType System_StringComparison = new("System.StringComparison");
        internal static readonly KnownType System_SystemException = new("System.SystemException");
        internal static readonly KnownType System_Text_RegularExpressions_Regex = new("System.Text.RegularExpressions.Regex");
        internal static readonly KnownType System_Text_StringBuilder = new("System.Text.StringBuilder");
        internal static readonly KnownType System_Threading_Monitor = new("System.Threading.Monitor");
        internal static readonly KnownType System_Threading_Mutex = new("System.Threading.Mutex");
        internal static readonly KnownType System_Threading_ReaderWriterLock = new("System.Threading.ReaderWriterLock");
        internal static readonly KnownType System_Threading_ReaderWriterLockSlim = new("System.Threading.ReaderWriterLockSlim");
        internal static readonly KnownType System_Threading_SpinLock = new("System.Threading.SpinLock");
        internal static readonly KnownType System_Threading_Tasks_Task = new("System.Threading.Tasks.Task");
        internal static readonly KnownType System_Threading_Tasks_Task_T = new("System.Threading.Tasks.Task<TResult>");
        internal static readonly KnownType System_Threading_Tasks_TaskFactory = new("System.Threading.Tasks.TaskFactory");
        internal static readonly KnownType System_Threading_Tasks_ValueTask = new("System.Threading.Tasks.ValueTask");
        internal static readonly KnownType System_Threading_Tasks_ValueTask_TResult = new("System.Threading.Tasks.ValueTask<TResult>");
        internal static readonly KnownType System_Threading_Thread = new("System.Threading.Thread");
        internal static readonly KnownType System_Threading_WaitHandle = new("System.Threading.WaitHandle");
        internal static readonly KnownType System_ThreadStaticAttribute = new("System.ThreadStaticAttribute");
        internal static readonly KnownType System_Type = new("System.Type");
        internal static readonly KnownType System_UInt16 = new(SpecialType.System_UInt16, "ushort");
        internal static readonly KnownType System_UInt32 = new(SpecialType.System_UInt32, "uint");
        internal static readonly KnownType System_UInt64 = new(SpecialType.System_UInt64, "ulong");
        internal static readonly KnownType System_UIntPtr = new(SpecialType.System_UIntPtr, "UIntPtr");
        internal static readonly KnownType System_Uri = new("System.Uri");
        internal static readonly KnownType System_ValueType = new(SpecialType.System_ValueType, "ValueType");
        internal static readonly KnownType System_Web_HttpApplication = new("System.Web.HttpApplication");
        internal static readonly KnownType System_Web_HttpCookie = new("System.Web.HttpCookie");
        internal static readonly KnownType System_Web_HttpContext = new("System.Web.HttpContext");
        internal static readonly KnownType System_Web_HttpResponse = new("System.Web.HttpResponse");
        internal static readonly KnownType System_Web_HttpResponseBase = new("System.Web.HttpResponseBase");
        internal static readonly KnownType System_Web_Http_ApiController = new("System.Web.Http.ApiController");
        internal static readonly KnownType System_Web_Http_Cors_EnableCorsAttribute = new("System.Web.Http.Cors.EnableCorsAttribute");
        internal static readonly KnownType System_Web_Mvc_NonActionAttribute = new("System.Web.Mvc.NonActionAttribute");
        internal static readonly KnownType System_Web_Mvc_Controller = new("System.Web.Mvc.Controller");
        internal static readonly KnownType System_Web_Mvc_HttpPostAttribute = new("System.Web.Mvc.HttpPostAttribute");
        internal static readonly KnownType System_Web_Mvc_ValidateInputAttribute = new("System.Web.Mvc.ValidateInputAttribute");
        internal static readonly KnownType System_Web_Script_Serialization_JavaScriptSerializer = new("System.Web.Script.Serialization.JavaScriptSerializer");
        internal static readonly KnownType System_Web_Script_Serialization_JavaScriptTypeResolver = new("System.Web.Script.Serialization.JavaScriptTypeResolver");
        internal static readonly KnownType System_Web_Script_Serialization_SimpleTypeResolver = new("System.Web.Script.Serialization.SimpleTypeResolver");
        internal static readonly KnownType System_Web_UI_LosFormatter = new("System.Web.UI.LosFormatter");
        internal static readonly KnownType System_Windows_DependencyObject = new("System.Windows.DependencyObject");
        internal static readonly KnownType System_Windows_Forms_Application = new("System.Windows.Forms.Application");
        internal static readonly KnownType System_Windows_Markup_ConstructorArgumentAttribute = new("System.Windows.Markup.ConstructorArgumentAttribute");
        internal static readonly KnownType System_Xml_Serialization_XmlElementAttribute = new("System.Xml.Serialization.XmlElementAttribute");
        internal static readonly KnownType System_Xml_XmlDocument = new("System.Xml.XmlDocument");
        internal static readonly KnownType System_Xml_XmlDataDocument = new("System.Xml.XmlDataDocument");
        internal static readonly KnownType System_Xml_XmlNode = new("System.Xml.XmlNode");
        internal static readonly KnownType System_Xml_XPath_XPathDocument = new("System.Xml.XPath.XPathDocument");
        internal static readonly KnownType System_Xml_XmlReader = new("System.Xml.XmlReader");
        internal static readonly KnownType System_Xml_XmlReaderSettings = new("System.Xml.XmlReaderSettings");
        internal static readonly KnownType System_Xml_XmlUrlResolver = new("System.Xml.XmlUrlResolver");
        internal static readonly KnownType System_Xml_XmlTextReader = new("System.Xml.XmlTextReader");
        internal static readonly KnownType System_Xml_Resolvers_XmlPreloadedResolver = new("System.Xml.Resolvers.XmlPreloadedResolver");
        internal static readonly KnownType NSubstitute_SubstituteExtensions = new("NSubstitute.SubstituteExtensions");
        internal static readonly KnownType NSubstitute_Received = new("NSubstitute.Received");
        internal static readonly ImmutableArray<KnownType> SystemActionVariants =
            ImmutableArray.Create(
                new KnownType("System.Action"),
                new KnownType("System.Action<T>"),
                new KnownType("System.Action<T1, T2>"),
                new KnownType("System.Action<T1, T2, T3>"),
                new KnownType("System.Action<T1, T2, T3, T4>"),
                new KnownType("System.Action<T1, T2, T3, T4, T5>"),
                new KnownType("System.Action<T1, T2, T3, T4, T5, T6>"),
                new KnownType("System.Action<T1, T2, T3, T4, T5, T6, T7>"),
                new KnownType("System.Action<T1, T2, T3, T4, T5, T6, T7, T8>"),
                new KnownType("System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>"),
                new KnownType("System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>"),
                new KnownType("System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>"),
                new KnownType("System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>"),
                new KnownType("System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>"),
                new KnownType("System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>"),
                new KnownType("System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>"),
                new KnownType("System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>"));
        internal static readonly ImmutableArray<KnownType> SystemFuncVariants =
            ImmutableArray.Create(
                new KnownType("System.Func<TResult>"),
                new KnownType("System.Func<T, TResult>"),
                new KnownType("System.Func<T1, T2, TResult>"),
                new KnownType("System.Func<T1, T2, T3, TResult>"),
                new KnownType("System.Func<T1, T2, T3, T4, TResult>"),
                new KnownType("System.Func<T1, T2, T3, T4, T5, TResult>"),
                new KnownType("System.Func<T1, T2, T3, T4, T5, T6, TResult>"),
                new KnownType("System.Func<T1, T2, T3, T4, T5, T6, T7, TResult>"),
                new KnownType("System.Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>"),
                new KnownType("System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>"),
                new KnownType("System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>"),
                new KnownType("System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>"),
                new KnownType("System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>"),
                new KnownType("System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>"),
                new KnownType("System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>"),
                new KnownType("System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>"),
                new KnownType("System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>"));
        internal static readonly ImmutableArray<KnownType> SystemTasks =
            ImmutableArray.Create(
                System_Threading_Tasks_Task,
                System_Threading_Tasks_Task_T,
                System_Threading_Tasks_ValueTask_TResult);
        internal static readonly KnownType Sytem_Resources_ResourceManager = new("System.Resources.ResourceManager");
        internal static readonly KnownType UnityEditor_AssetModificationProcessor = new("UnityEditor.AssetModificationProcessor");
        internal static readonly KnownType UnityEditor_AssetPostprocessor = new("UnityEditor.AssetPostprocessor");
        internal static readonly KnownType UnityEngine_MonoBehaviour = new("UnityEngine.MonoBehaviour");
        internal static readonly KnownType UnityEngine_ScriptableObject = new("UnityEngine.ScriptableObject");
        internal static readonly KnownType Xunit_Assert = new("Xunit.Assert");
        internal static readonly KnownType Xunit_Sdk_AssertException = new("Xunit.Sdk.AssertException");
        internal static readonly KnownType Xunit_FactAttribute = new("Xunit.FactAttribute");
        internal static readonly KnownType Xunit_Sdk_XunitException = new("Xunit.Sdk.XunitException");
        internal static readonly KnownType Xunit_TheoryAttribute = new("Xunit.TheoryAttribute");
        internal static readonly KnownType LegacyXunit_TheoryAttribute = new("Xunit.Extensions.TheoryAttribute");
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

        private readonly bool isSpecialType;
        private readonly SpecialType specialType;
        private readonly Lazy<string> shortName;

        public string TypeName { get; }
        public string ShortName => shortName.Value;

        private KnownType(string typeName) : this(SpecialType.None, typeName) { }

        private KnownType(SpecialType specialType, string typeName)
        {
            this.specialType = specialType;
            TypeName = typeName;
            shortName = new Lazy<string>(() => typeName.Split('.').Last());
            isSpecialType = specialType != SpecialType.None;
        }

        internal bool Matches(string type) =>
            !isSpecialType && TypeName == type;

        internal bool Matches(SpecialType type) =>
            isSpecialType && specialType == type;
    }
}

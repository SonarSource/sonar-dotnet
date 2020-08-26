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

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    internal sealed class KnownType
    {
        #region Known types

        internal static readonly KnownType Void = new KnownType(SpecialType.System_Void, "void");

        internal static readonly KnownType JWT_Builder_JwtBuilder = new KnownType("JWT.Builder.JwtBuilder");
        internal static readonly KnownType JWT_IJwtDecoder = new KnownType("JWT.IJwtDecoder");
        internal static readonly KnownType log4net_Config_XmlConfigurator = new KnownType("log4net.Config.XmlConfigurator");
        internal static readonly KnownType log4net_Config_DOMConfigurator = new KnownType("log4net.Config.DOMConfigurator");
        internal static readonly KnownType log4net_Config_BasicConfigurator = new KnownType("log4net.Config.BasicConfigurator");
        internal static readonly KnownType Microsoft_AspNet_SignalR_Hub = new KnownType("Microsoft.AspNet.SignalR.Hub");
        internal static readonly KnownType Microsoft_AspNetCore_Builder_DeveloperExceptionPageExtensions = new KnownType("Microsoft.AspNetCore.Builder.DeveloperExceptionPageExtensions");
        internal static readonly KnownType Microsoft_AspNetCore_Builder_DatabaseErrorPageExtensions = new KnownType("Microsoft.AspNetCore.Builder.DatabaseErrorPageExtensions");
        internal static readonly KnownType Microsoft_AspNetCore_Hosting_HostingEnvironmentExtensions = new KnownType("Microsoft.AspNetCore.Hosting.HostingEnvironmentExtensions");
        internal static readonly KnownType Microsoft_AspNetCore_Hosting_WebHostBuilderExtensions = new KnownType("Microsoft.AspNetCore.Hosting.WebHostBuilderExtensions");
        internal static readonly KnownType Microsoft_AspNetCore_Http_CookieOptions = new KnownType("Microsoft.AspNetCore.Http.CookieOptions");
        internal static readonly KnownType Microsoft_AspNetCore_Http_IHeaderDictionary = new KnownType("Microsoft.AspNetCore.Http.IHeaderDictionary");
        internal static readonly KnownType Microsoft_AspNetCore_Http_IRequestCookieCollection = new KnownType("Microsoft.AspNetCore.Http.IRequestCookieCollection");
        internal static readonly KnownType Microsoft_AspNetCore_Http_IResponseCookies = new KnownType("Microsoft.AspNetCore.Http.IResponseCookies");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_Controller = new KnownType("Microsoft.AspNetCore.Mvc.Controller");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_ControllerBase = new KnownType("Microsoft.AspNetCore.Mvc.ControllerBase");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_ControllerAttribute = new KnownType("Microsoft.AspNetCore.Mvc.ControllerAttribute");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_NonActionAttribute = new KnownType("Microsoft.AspNetCore.Mvc.NonActionAttribute");
        internal static readonly KnownType Microsoft_AspNetCore_Mvc_NonControllerAttribute = new KnownType("Microsoft.AspNetCore.Mvc.NonControllerAttribute");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_Migrations_Migration = new KnownType("Microsoft.EntityFrameworkCore.Migrations.Migration");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_RawSqlString = new KnownType("Microsoft.EntityFrameworkCore.RawSqlString");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_RelationalDatabaseFacadeExtensions = new KnownType("Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions");
        internal static readonly KnownType Microsoft_EntityFrameworkCore_RelationalQueryableExtensions = new KnownType("Microsoft.EntityFrameworkCore.RelationalQueryableExtensions");
        internal static readonly KnownType Microsoft_Extensions_DependencyInjection_LoggingServiceCollectionExtensions = new KnownType("Microsoft.Extensions.DependencyInjection.LoggingServiceCollectionExtensions");
        internal static readonly KnownType Microsoft_Extensions_Logging_AzureAppServicesLoggerFactoryExtensions = new KnownType("Microsoft.Extensions.Logging.AzureAppServicesLoggerFactoryExtensions");
        internal static readonly KnownType Microsoft_Extensions_Logging_ConsoleLoggerExtensions = new KnownType("Microsoft.Extensions.Logging.ConsoleLoggerExtensions");
        internal static readonly KnownType Microsoft_Extensions_Logging_DebugLoggerFactoryExtensions = new KnownType("Microsoft.Extensions.Logging.DebugLoggerFactoryExtensions");
        internal static readonly KnownType Microsoft_Extensions_Logging_EventLoggerFactoryExtensions = new KnownType("Microsoft.Extensions.Logging.EventLoggerFactoryExtensions");
        internal static readonly KnownType Microsoft_Extensions_Logging_EventSourceLoggerFactoryExtensions = new KnownType("Microsoft.Extensions.Logging.EventSourceLoggerFactoryExtensions");
        internal static readonly KnownType Microsoft_Extensions_Logging_ILoggerFactory = new KnownType("Microsoft.Extensions.Logging.ILoggerFactory");
        internal static readonly KnownType Microsoft_Extensions_Primitives_StringValues = new KnownType("Microsoft.Extensions.Primitives.StringValues");
        internal static readonly KnownType Microsoft_VisualBasic_Interaction = new KnownType("Microsoft.VisualBasic.Interaction");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_Assert = new KnownType("Microsoft.VisualStudio.TestTools.UnitTesting.Assert");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_ExpectedExceptionAttribute = new KnownType("Microsoft.VisualStudio.TestTools.UnitTesting.ExpectedExceptionAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_IgnoreAttribute = new KnownType("Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_TestClassAttribute = new KnownType("Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_TestMethodAttribute = new KnownType("Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_DataTestMethodAttribute = new KnownType("Microsoft.VisualStudio.TestTools.UnitTesting.DataTestMethodAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_WorkItemAttribute = new KnownType("Microsoft.VisualStudio.TestTools.UnitTesting.WorkItemAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_AssemblyCleanupAttribute = new KnownType("Microsoft.VisualStudio.TestTools.UnitTesting.AssemblyCleanupAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_AssemblyInitializeAttribute = new KnownType("Microsoft.VisualStudio.TestTools.UnitTesting.AssemblyInitializeAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_ClassCleanupAttribute = new KnownType("Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_ClassInitializeAttribute = new KnownType("Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_TestCleanupAttribute = new KnownType("Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_TestInitializeAttribute = new KnownType("Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute");
        internal static readonly KnownType Microsoft_Web_XmlTransform_XmlFileInfoDocument = new KnownType("Microsoft.Web.XmlTransform.XmlFileInfoDocument");
        internal static readonly KnownType Microsoft_Web_XmlTransform_XmlTransformableDocument = new KnownType("Microsoft.Web.XmlTransform.XmlTransformableDocument");
        internal static readonly KnownType Nancy_Cookies_NancyCookie = new KnownType("Nancy.Cookies.NancyCookie");
        internal static readonly KnownType NLog_LogManager = new KnownType("NLog.LogManager");
        internal static readonly KnownType NUnit_Framework_Assert = new KnownType("NUnit.Framework.Assert");
        internal static readonly KnownType NUnit_Framework_ExpectedExceptionAttribute = new KnownType("NUnit.Framework.ExpectedExceptionAttribute");
        internal static readonly KnownType NUnit_Framework_IgnoreAttribute = new KnownType("NUnit.Framework.IgnoreAttribute");
        internal static readonly KnownType NUnit_Framework_TestAttribute = new KnownType("NUnit.Framework.TestAttribute");
        internal static readonly KnownType NUnit_Framework_TestCaseAttribute = new KnownType("NUnit.Framework.TestCaseAttribute");
        internal static readonly KnownType NUnit_Framework_TestCaseSourceAttribute = new KnownType("NUnit.Framework.TestCaseSourceAttribute");
        internal static readonly KnownType NUnit_Framework_TestFixtureAttribute = new KnownType("NUnit.Framework.TestFixtureAttribute");
        internal static readonly KnownType NUnit_Framework_TheoryAttribute = new KnownType("NUnit.Framework.TheoryAttribute");
        internal static readonly KnownType Org_BouncyCastle_Asn1_Nist_NistNamedCurves = new KnownType("Org.BouncyCastle.Asn1.Nist.NistNamedCurves");
        internal static readonly KnownType Org_BouncyCastle_Asn1_Sec_SecNamedCurves = new KnownType("Org.BouncyCastle.Asn1.Sec.SecNamedCurves");
        internal static readonly KnownType Org_BouncyCastle_Asn1_TeleTrust_TeleTrusTNamedCurves = new KnownType("Org.BouncyCastle.Asn1.TeleTrust.TeleTrusTNamedCurves");
        internal static readonly KnownType Org_BouncyCastle_Asn1_X9_ECNamedCurveTable = new KnownType("Org.BouncyCastle.Asn1.X9.ECNamedCurveTable");
        internal static readonly KnownType Org_BouncyCastle_Asn1_X9_X962NamedCurves = new KnownType("Org.BouncyCastle.Asn1.X9.X962NamedCurves");
        internal static readonly KnownType Org_BouncyCastle_Crypto_Generators_DHParametersGenerator = new KnownType("Org.BouncyCastle.Crypto.Generators.DHParametersGenerator");
        internal static readonly KnownType Org_BouncyCastle_Crypto_Generators_DsaParametersGenerator = new KnownType("Org.BouncyCastle.Crypto.Generators.DsaParametersGenerator");
        internal static readonly KnownType Org_BouncyCastle_Crypto_Parameters_RsaKeyGenerationParameters = new KnownType("Org.BouncyCastle.Crypto.Parameters.RsaKeyGenerationParameters");
        internal static readonly KnownType Serilog_LoggerConfiguration = new KnownType("Serilog.LoggerConfiguration");
        internal static readonly KnownType System_Action = new KnownType("System.Action");
        internal static readonly KnownType System_Action_T = new KnownType("System.Action<T>");
        internal static readonly KnownType System_Action_T1_T2 = new KnownType("System.Action<T1, T2>");
        internal static readonly KnownType System_Action_T1_T2_T3 = new KnownType("System.Action<T1, T2, T3>");
        internal static readonly KnownType System_Action_T1_T2_T3_T4 = new KnownType("System.Action<T1, T2, T3, T4>");
        internal static readonly KnownType System_Activator = new KnownType("System.Activator");
        internal static readonly KnownType System_ApplicationException = new KnownType("System.ApplicationException");
        internal static readonly KnownType System_AppDomain = new KnownType("System.AppDomain");
        internal static readonly KnownType System_ArgumentException = new KnownType("System.ArgumentException");
        internal static readonly KnownType System_ArgumentNullException = new KnownType("System.ArgumentNullException");
        internal static readonly KnownType System_ArgumentOutOfRangeException = new KnownType("System.ArgumentOutOfRangeException");
        internal static readonly KnownType System_Array = new KnownType(SpecialType.System_Array, "System.Array");
        internal static readonly KnownType System_Attribute = new KnownType("System.Attribute");
        internal static readonly KnownType System_AttributeUsageAttribute = new KnownType("System.AttributeUsageAttribute");
        internal static readonly KnownType System_Boolean = new KnownType(SpecialType.System_Boolean, "bool");
        internal static readonly KnownType System_Byte = new KnownType(SpecialType.System_Byte, "byte");
        internal static readonly KnownType System_Char = new KnownType(SpecialType.System_Char, "char");
        internal static readonly KnownType System_CLSCompliantAttribute = new KnownType("System.CLSCompliantAttribute");
        internal static readonly KnownType System_CodeDom_Compiler_GeneratedCodeAttribute = new KnownType("System.CodeDom.Compiler.GeneratedCodeAttribute");
        internal static readonly KnownType System_Collections_CollectionBase = new KnownType("System.Collections.CollectionBase");
        internal static readonly KnownType System_Collections_DictionaryBase = new KnownType("System.Collections.DictionaryBase");
        internal static readonly KnownType System_Collections_Generic_Dictionary_TKey_TValue = new KnownType("System.Collections.Generic.Dictionary<TKey, TValue>");
        internal static readonly KnownType System_Collections_Generic_HashSet_T = new KnownType("System.Collections.Generic.HashSet<T>");
        internal static readonly KnownType System_Collections_Generic_IAsyncEnumerable_T = new KnownType("System.Collections.Generic.IAsyncEnumerable<T>");
        internal static readonly KnownType System_Collections_Generic_ICollection_T = new KnownType(SpecialType.System_Collections_Generic_ICollection_T, "System.Collections.Generic.ICollection<T>");
        internal static readonly KnownType System_Collections_Generic_IDictionary_TKey_TValue = new KnownType("System.Collections.Generic.IDictionary<TKey, TValue>");
        internal static readonly KnownType System_Collections_Generic_IDictionary_TKey_TValue_VB = new KnownType("System.Collections.Generic.IDictionary(Of TKey, TValue)");
        internal static readonly KnownType System_Collections_Generic_IEnumerable_T = new KnownType(SpecialType.System_Collections_Generic_IEnumerable_T, "System.Collections.Generic.IEnumerable<T>");
        internal static readonly KnownType System_Collections_Generic_IList_T = new KnownType(SpecialType.System_Collections_Generic_IList_T, "System.Collections.Generic.IList<T>");
        internal static readonly KnownType System_Collections_Generic_ISet_T = new KnownType("System.Collections.Generic.ISet<T>");
        internal static readonly KnownType System_Collections_Generic_KeyValuePair_TKey_TValue = new KnownType("System.Collections.Generic.KeyValuePair<TKey, TValue>");
        internal static readonly KnownType System_Collections_Generic_List_T = new KnownType("System.Collections.Generic.List<T>");
        internal static readonly KnownType System_Collections_Generic_Queue_T = new KnownType("System.Collections.Generic.Queue<T>");
        internal static readonly KnownType System_Collections_Generic_Stack_T = new KnownType("System.Collections.Generic.Stack<T>");
        internal static readonly KnownType System_Collections_ICollection = new KnownType("System.Collections.ICollection");
        internal static readonly KnownType System_Collections_IEnumerable = new KnownType(SpecialType.System_Collections_IEnumerable, "System.Collections.IEnumerable");
        internal static readonly KnownType System_Collections_IList = new KnownType("System.Collections.IList");
        internal static readonly KnownType System_Collections_Immutable_IImmutableArray_T = new KnownType("System.Collections.Immutable.IImmutableArray<T>");
        internal static readonly KnownType System_Collections_Immutable_IImmutableDictionary_TKey_TValue = new KnownType("System.Collections.Immutable.IImmutableDictionary<TKey, TValue>");
        internal static readonly KnownType System_Collections_Immutable_IImmutableList_T = new KnownType("System.Collections.Immutable.IImmutableList<T>");
        internal static readonly KnownType System_Collections_Immutable_IImmutableQueue_T = new KnownType("System.Collections.Immutable.IImmutableQueue<T>");
        internal static readonly KnownType System_Collections_Immutable_IImmutableSet_T = new KnownType("System.Collections.Immutable.IImmutableSet<T>");
        internal static readonly KnownType System_Collections_Immutable_IImmutableStack_T = new KnownType("System.Collections.Immutable.IImmutableStack<T>");
        internal static readonly KnownType System_Collections_Immutable_ImmutableArray = new KnownType("System.Collections.Immutable.ImmutableArray");
        internal static readonly KnownType System_Collections_Immutable_ImmutableArray_T = new KnownType("System.Collections.Immutable.ImmutableArray<T>");
        internal static readonly KnownType System_Collections_Immutable_ImmutableDictionary = new KnownType("System.Collections.Immutable.ImmutableDictionary");
        internal static readonly KnownType System_Collections_Immutable_ImmutableDictionary_TKey_TValue = new KnownType("System.Collections.Immutable.ImmutableDictionary<TKey, TValue>");
        internal static readonly KnownType System_Collections_Immutable_ImmutableHashSet = new KnownType("System.Collections.Immutable.ImmutableHashSet");
        internal static readonly KnownType System_Collections_Immutable_ImmutableHashSet_T = new KnownType("System.Collections.Immutable.ImmutableHashSet<T>");
        internal static readonly KnownType System_Collections_Immutable_ImmutableList = new KnownType("System.Collections.Immutable.ImmutableList");
        internal static readonly KnownType System_Collections_Immutable_ImmutableList_T = new KnownType("System.Collections.Immutable.ImmutableList<T>");
        internal static readonly KnownType System_Collections_Immutable_ImmutableQueue = new KnownType("System.Collections.Immutable.ImmutableQueue");
        internal static readonly KnownType System_Collections_Immutable_ImmutableQueue_T = new KnownType("System.Collections.Immutable.ImmutableQueue<T>");
        internal static readonly KnownType System_Collections_Immutable_ImmutableSortedDictionary = new KnownType("System.Collections.Immutable.ImmutableSortedDictionary");
        internal static readonly KnownType System_Collections_Immutable_ImmutableSortedDictionary_TKey_TValue = new KnownType("System.Collections.Immutable.ImmutableSortedDictionary<TKey, TValue>");
        internal static readonly KnownType System_Collections_Immutable_ImmutableSortedSet = new KnownType("System.Collections.Immutable.ImmutableSortedSet");
        internal static readonly KnownType System_Collections_Immutable_ImmutableSortedSet_T = new KnownType("System.Collections.Immutable.ImmutableSortedSet<T>");
        internal static readonly KnownType System_Collections_Immutable_ImmutableStack = new KnownType("System.Collections.Immutable.ImmutableStack");
        internal static readonly KnownType System_Collections_Immutable_ImmutableStack_T = new KnownType("System.Collections.Immutable.ImmutableStack<T>");
        internal static readonly KnownType System_Collections_ObjectModel_Collection_T = new KnownType("System.Collections.ObjectModel.Collection<T>");
        internal static readonly KnownType System_Collections_ObjectModel_ObservableCollection_T = new KnownType("System.Collections.ObjectModel.ObservableCollection<T>");
        internal static readonly KnownType System_Collections_ObjectModel_ReadOnlyCollection_T = new KnownType("System.Collections.ObjectModel.ReadOnlyCollection<T>");
        internal static readonly KnownType System_Collections_ObjectModel_ReadOnlyDictionary_TKey_TValue = new KnownType("System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>");
        internal static readonly KnownType System_Collections_Queue = new KnownType("System.Collections.Queue");
        internal static readonly KnownType System_Collections_ReadOnlyCollectionBase = new KnownType("System.Collections.ReadOnlyCollectionBase");
        internal static readonly KnownType System_Collections_SortedList = new KnownType("System.Collections.SortedList");
        internal static readonly KnownType System_Collections_Stack = new KnownType("System.Collections.Stack");
        internal static readonly KnownType System_Collections_Specialized_NameObjectCollectionBase = new KnownType("System.Collections.Specialized.NameObjectCollectionBase");
        internal static readonly KnownType System_Collections_Specialized_NameValueCollection = new KnownType("System.Collections.Specialized.NameValueCollection");
        internal static readonly KnownType System_ComponentModel_DefaultValueAttribute = new KnownType("System.ComponentModel.DefaultValueAttribute");
        internal static readonly KnownType System_ComponentModel_LocalizableAttribute = new KnownType("System.ComponentModel.LocalizableAttribute");
        internal static readonly KnownType System_ComponentModel_Composition_CreationPolicy = new KnownType("System.ComponentModel.Composition.CreationPolicy");
        internal static readonly KnownType System_ComponentModel_Composition_ExportAttribute = new KnownType("System.ComponentModel.Composition.ExportAttribute");
        internal static readonly KnownType System_ComponentModel_Composition_InheritedExportAttribute = new KnownType("System.ComponentModel.Composition.InheritedExportAttribute");
        internal static readonly KnownType System_ComponentModel_Composition_PartCreationPolicyAttribute = new KnownType("System.ComponentModel.Composition.PartCreationPolicyAttribute");
        internal static readonly KnownType System_Configuration_ConfigXmlDocument = new KnownType("System.Configuration.ConfigXmlDocument");
        internal static readonly KnownType System_Console = new KnownType("System.Console");
        internal static readonly KnownType System_Data_Common_CommandTrees_DbExpression = new KnownType("System.Data.Common.CommandTrees.DbExpression");
        internal static readonly KnownType System_Data_DataSet = new KnownType("System.Data.DataSet");
        internal static readonly KnownType System_Data_DataTable = new KnownType("System.Data.DataTable");
        internal static readonly KnownType System_Data_Odbc_OdbcCommand = new KnownType("System.Data.Odbc.OdbcCommand");
        internal static readonly KnownType System_Data_Odbc_OdbcDataAdapter = new KnownType("System.Data.Odbc.OdbcDataAdapter");
        internal static readonly KnownType System_Data_OracleClient_OracleCommand = new KnownType("System.Data.OracleClient.OracleCommand");
        internal static readonly KnownType System_Data_OracleClient_OracleDataAdapter = new KnownType("System.Data.OracleClient.OracleDataAdapter");
        internal static readonly KnownType System_Data_SqlClient_SqlCommand = new KnownType("System.Data.SqlClient.SqlCommand");
        internal static readonly KnownType System_Data_SqlClient_SqlDataAdapter = new KnownType("System.Data.SqlClient.SqlDataAdapter");
        internal static readonly KnownType System_Data_SqlServerCe_SqlCeCommand = new KnownType("System.Data.SqlServerCe.SqlCeCommand");
        internal static readonly KnownType System_Data_SqlServerCe_SqlCeDataAdapter = new KnownType("System.Data.SqlServerCe.SqlCeDataAdapter");
        internal static readonly KnownType System_DateTime = new KnownType(SpecialType.System_DateTime, "DateTime");
        internal static readonly KnownType System_Decimal = new KnownType(SpecialType.System_Decimal, "decimal");
        internal static readonly KnownType System_Delegate = new KnownType("System.Delegate");
        internal static readonly KnownType System_Diagnostics_CodeAnalysis_ExcludeFromCodeCoverageAttribute = new KnownType("System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute");
        internal static readonly KnownType System_Diagnostics_CodeAnalysis_SuppressMessageAttribute = new KnownType("System.Diagnostics.CodeAnalysis.SuppressMessageAttribute");
        internal static readonly KnownType System_Diagnostics_ConditionalAttribute = new KnownType("System.Diagnostics.ConditionalAttribute");
        internal static readonly KnownType System_Diagnostics_Contracts_PureAttribute = new KnownType("System.Diagnostics.Contracts.PureAttribute");
        internal static readonly KnownType System_Diagnostics_Debug = new KnownType("System.Diagnostics.Debug");
        internal static readonly KnownType System_Diagnostics_DebuggerDisplayAttribute = new KnownType("System.Diagnostics.DebuggerDisplayAttribute");
        internal static readonly KnownType System_Diagnostics_Trace = new KnownType("System.Diagnostics.Trace");
        internal static readonly KnownType System_Diagnostics_TraceSource = new KnownType("System.Diagnostics.TraceSource");
        internal static readonly KnownType System_DirectoryServices_AuthenticationTypes = new KnownType("System.DirectoryServices.AuthenticationTypes");
        internal static readonly KnownType System_DirectoryServices_DirectoryEntry = new KnownType("System.DirectoryServices.DirectoryEntry");
        internal static readonly KnownType System_Double = new KnownType(SpecialType.System_Double, "double");
        internal static readonly KnownType System_Drawing_Bitmap = new KnownType("System.Drawing.Bitmap");
        internal static readonly KnownType System_Drawing_Image = new KnownType("System.Drawing.Image");
        internal static readonly KnownType System_DuplicateWaitObjectException = new KnownType("System.DuplicateWaitObjectException");
        internal static readonly KnownType System_Enum = new KnownType(SpecialType.System_Enum, "Enum");
        internal static readonly KnownType System_Environment = new KnownType("System.Environment");
        internal static readonly KnownType System_EventArgs = new KnownType("System.EventArgs");
        internal static readonly KnownType System_EventHandler = new KnownType("System.EventHandler");
        internal static readonly KnownType System_EventHandler_TEventArgs = new KnownType("System.EventHandler<TEventArgs>");
        internal static readonly KnownType System_Exception = new KnownType("System.Exception");
        internal static readonly KnownType System_ExecutionEngineException = new KnownType("System.ExecutionEngineException");
        internal static readonly KnownType System_FlagsAttribute = new KnownType("System.FlagsAttribute");
        internal static readonly KnownType System_Func_TResult = new KnownType("System.Func<TResult>");
        internal static readonly KnownType System_Func_T_TResult = new KnownType("System.Func<T, TResult>");
        internal static readonly KnownType System_Func_T1_T2_TResult = new KnownType("System.Func<T1, T2, TResult>");
        internal static readonly KnownType System_Func_T1_T2_T3_TResult = new KnownType("System.Func<T1, T2, T3, TResult>");
        internal static readonly KnownType System_Func_T1_T2_T3_T4_TResult = new KnownType("System.Func<T1, T2, T3, T4, TResult>");
        internal static readonly KnownType System_Func_T1_T2_T3_T4_TResult_VB = new KnownType("System.Func(Of In T1, In T2, In T3, In T4, Out TResult)");
        internal static readonly KnownType System_GC = new KnownType("System.GC");
        internal static readonly KnownType System_Globalization_CompareOptions = new KnownType("System.Globalization.CompareOptions");
        internal static readonly KnownType System_Globalization_CultureInfo = new KnownType("System.Globalization.CultureInfo");
        internal static readonly KnownType System_Guid = new KnownType("System.Guid");
        internal static readonly KnownType System_IComparable = new KnownType("System.IComparable");
        internal static readonly KnownType System_IComparable_T = new KnownType("System.IComparable<T>");
        internal static readonly KnownType System_IdentityModel_Tokens_SecurityTokenHandler = new KnownType("System.IdentityModel.Tokens.SecurityTokenHandler");
        internal static readonly KnownType System_IDisposable = new KnownType(SpecialType.System_IDisposable, "System.IDisposable");
        internal static readonly KnownType System_IEquatable_T = new KnownType("System.IEquatable<T>");
        internal static readonly KnownType System_IFormatProvider = new KnownType("System.IFormatProvider");
        internal static readonly KnownType System_IndexOutOfRangeException = new KnownType("System.IndexOutOfRangeException");
        internal static readonly KnownType System_Int16 = new KnownType(SpecialType.System_Int16, "short");
        internal static readonly KnownType System_Int32 = new KnownType(SpecialType.System_Int32, "int");
        internal static readonly KnownType System_Int64 = new KnownType(SpecialType.System_Int64, "long");
        internal static readonly KnownType System_IntPtr = new KnownType(SpecialType.System_IntPtr, "IntPtr");
        internal static readonly KnownType System_InvalidOperationException = new KnownType("System.InvalidOperationException");
        internal static readonly KnownType System_IO_Compression_ZipArchiveEntry = new KnownType("System.IO.Compression.ZipArchiveEntry");
        internal static readonly KnownType System_IO_Compression_ZipFileExtensions = new KnownType("System.IO.Compression.ZipFileExtensions");
        internal static readonly KnownType System_IO_FileStream = new KnownType("System.IO.FileStream");
        internal static readonly KnownType System_IO_Stream = new KnownType("System.IO.Stream");
        internal static readonly KnownType System_IO_StreamReader = new KnownType("System.IO.StreamReader");
        internal static readonly KnownType System_IO_StreamWriter = new KnownType("System.IO.StreamWriter");
        internal static readonly KnownType System_IO_TextWriter = new KnownType("System.IO.TextWriter");
        internal static readonly KnownType System_Lazy = new KnownType("System.Lazy<T>");
        internal static readonly KnownType System_Linq_Enumerable = new KnownType("System.Linq.Enumerable");
        internal static readonly KnownType System_Linq_Expressions_Expression_T = new KnownType("System.Linq.Expressions.Expression<TDelegate>");
        internal static readonly KnownType System_Linq_ImmutableArrayExtensions = new KnownType("System.Linq.ImmutableArrayExtensions");
        internal static readonly KnownType System_MarshalByRefObject = new KnownType("System.MarshalByRefObject");
        internal static readonly KnownType System_MTAThreadAttribute = new KnownType("System.MTAThreadAttribute");
        internal static readonly KnownType System_Net_NetworkCredential = new KnownType("System.Net.NetworkCredential");
        internal static readonly KnownType System_Net_Security_RemoteCertificateValidationCallback = new KnownType("System.Net.Security.RemoteCertificateValidationCallback");
        internal static readonly KnownType System_Net_Security_SslPolicyErrors = new KnownType("System.Net.Security.SslPolicyErrors");
        internal static readonly KnownType System_Net_Sockets_Socket = new KnownType("System.Net.Sockets.Socket");
        internal static readonly KnownType System_Net_Sockets_TcpClient = new KnownType("System.Net.Sockets.TcpClient");
        internal static readonly KnownType System_Net_Sockets_UdpClient = new KnownType("System.Net.Sockets.UdpClient");
        internal static readonly KnownType System_Net_WebClient = new KnownType("System.Net.WebClient");
        internal static readonly KnownType System_NotImplementedException = new KnownType("System.NotImplementedException");
        internal static readonly KnownType System_NotSupportedException = new KnownType("System.NotSupportedException");
        internal static readonly KnownType System_Nullable_T = new KnownType(SpecialType.System_Nullable_T, "System.Nullable<T>");
        internal static readonly KnownType System_NullReferenceException = new KnownType("System.NullReferenceException");
        internal static readonly KnownType System_Object = new KnownType(SpecialType.System_Object, "object");
        internal static readonly KnownType System_ObsoleteAttribute = new KnownType("System.ObsoleteAttribute");
        internal static readonly KnownType System_OutOfMemoryException = new KnownType("System.OutOfMemoryException");
        internal static readonly KnownType System_Random = new KnownType("System.Random");
        internal static readonly KnownType System_Reflection_Assembly = new KnownType("System.Reflection.Assembly");
        internal static readonly KnownType System_Reflection_BindingFlags = new KnownType("System.Reflection.BindingFlags");
        internal static readonly KnownType System_Reflection_AssemblyVersionAttribute = new KnownType("System.Reflection.AssemblyVersionAttribute");
        internal static readonly KnownType System_Reflection_MemberInfo = new KnownType("System.Reflection.MemberInfo");
        internal static readonly KnownType System_Reflection_Module = new KnownType("System.Reflection.Module");
        internal static readonly KnownType System_Reflection_ParameterInfo = new KnownType("System.Reflection.ParameterInfo");
        internal static readonly KnownType System_Resources_NeutralResourcesLanguageAttribute = new KnownType("System.Resources.NeutralResourcesLanguageAttribute");
        internal static readonly KnownType System_Runtime_CompilerServices_CallerFilePathAttribute = new KnownType("System.Runtime.CompilerServices.CallerFilePathAttribute");
        internal static readonly KnownType System_Runtime_CompilerServices_CallerLineNumberAttribute = new KnownType("System.Runtime.CompilerServices.CallerLineNumberAttribute");
        internal static readonly KnownType System_Runtime_CompilerServices_CallerMemberNameAttribute = new KnownType("System.Runtime.CompilerServices.CallerMemberNameAttribute");
        internal static readonly KnownType System_Runtime_CompilerServices_InternalsVisibleToAttribute = new KnownType("System.Runtime.CompilerServices.InternalsVisibleToAttribute");
        internal static readonly KnownType System_Runtime_CompilerServices_ValueTaskAwaiter = new KnownType("System.Runtime.CompilerServices.ValueTaskAwaiter");
        internal static readonly KnownType System_Runtime_CompilerServices_ValueTaskAwaiter_TResult = new KnownType("System.Runtime.CompilerServices.ValueTaskAwaiter<TResult>");
        internal static readonly KnownType System_Runtime_CompilerServices_TaskAwaiter = new KnownType("System.Runtime.CompilerServices.TaskAwaiter");
        internal static readonly KnownType System_Runtime_CompilerServices_TaskAwaiter_TResult = new KnownType("System.Runtime.CompilerServices.TaskAwaiter<TResult>");
        internal static readonly KnownType System_Runtime_InteropServices_ComImportAttribute = new KnownType("System.Runtime.InteropServices.ComImportAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_ComVisibleAttribute = new KnownType("System.Runtime.InteropServices.ComVisibleAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_DefaultParameterValueAttribute = new KnownType("System.Runtime.InteropServices.DefaultParameterValueAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_DllImportAttribute = new KnownType("System.Runtime.InteropServices.DllImportAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_HandleRef = new KnownType("System.Runtime.InteropServices.HandleRef");
        internal static readonly KnownType System_Runtime_InteropServices_InterfaceTypeAttribute = new KnownType("System.Runtime.InteropServices.InterfaceTypeAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_OptionalAttribute = new KnownType("System.Runtime.InteropServices.OptionalAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_SafeHandle = new KnownType("System.Runtime.InteropServices.SafeHandle");
        internal static readonly KnownType System_Runtime_InteropServices_StructLayoutAttribute = new KnownType("System.Runtime.InteropServices.StructLayoutAttribute");
        internal static readonly KnownType System_Runtime_Serialization_DataMemberAttribute = new KnownType("System.Runtime.Serialization.DataMemberAttribute");
        internal static readonly KnownType System_Runtime_Serialization_Formatters_Binary_BinaryFormatter = new KnownType("System.Runtime.Serialization.Formatters.Binary.BinaryFormatter");
        internal static readonly KnownType System_Runtime_Serialization_Formatters_Soap_SoapFormatter = new KnownType("System.Runtime.Serialization.Formatters.Soap.SoapFormatter");
        internal static readonly KnownType System_Runtime_Serialization_ISerializable = new KnownType("System.Runtime.Serialization.ISerializable");
        internal static readonly KnownType System_Runtime_Serialization_IDeserializationCallback  = new KnownType("System.Runtime.Serialization.IDeserializationCallback");
        internal static readonly KnownType System_Runtime_Serialization_NetDataContractSerializer = new KnownType("System.Runtime.Serialization.NetDataContractSerializer");
        internal static readonly KnownType System_Runtime_Serialization_OnDeserializedAttribute = new KnownType("System.Runtime.Serialization.OnDeserializedAttribute");
        internal static readonly KnownType System_Runtime_Serialization_OnDeserializingAttribute = new KnownType("System.Runtime.Serialization.OnDeserializingAttribute");
        internal static readonly KnownType System_Runtime_Serialization_OnSerializedAttribute = new KnownType("System.Runtime.Serialization.OnSerializedAttribute");
        internal static readonly KnownType System_Runtime_Serialization_OnSerializingAttribute = new KnownType("System.Runtime.Serialization.OnSerializingAttribute");
        internal static readonly KnownType System_Runtime_Serialization_OptionalFieldAttribute = new KnownType("System.Runtime.Serialization.OptionalFieldAttribute");
        internal static readonly KnownType System_Runtime_Serialization_SerializationInfo = new KnownType("System.Runtime.Serialization.SerializationInfo");
        internal static readonly KnownType System_Runtime_Serialization_StreamingContext = new KnownType("System.Runtime.Serialization.StreamingContext");
        internal static readonly KnownType System_SByte = new KnownType(SpecialType.System_SByte, "sbyte");
        internal static readonly KnownType System_Security_AllowPartiallyTrustedCallersAttribute = new KnownType("System.Security.AllowPartiallyTrustedCallersAttribute");
        internal static readonly KnownType System_Security_Cryptography_AesManaged = new KnownType("System.Security.Cryptography.AesManaged");
        internal static readonly KnownType System_Security_Cryptography_AsymmetricAlgorithm = new KnownType("System.Security.Cryptography.AsymmetricAlgorithm");
        internal static readonly KnownType System_Security_Cryptography_CspParameters = new KnownType("System.Security.Cryptography.CspParameters");
        internal static readonly KnownType System_Security_Cryptography_DES = new KnownType("System.Security.Cryptography.DES");
        internal static readonly KnownType System_Security_Cryptography_DSA = new KnownType("System.Security.Cryptography.DSA");
        internal static readonly KnownType System_Security_Cryptography_DSACryptoServiceProvider = new KnownType("System.Security.Cryptography.DSACryptoServiceProvider");
        internal static readonly KnownType System_Security_Cryptography_ECDiffieHellman = new KnownType("System.Security.Cryptography.ECDiffieHellman");
        internal static readonly KnownType System_Security_Cryptography_ECDsa = new KnownType("System.Security.Cryptography.ECDsa");
        internal static readonly KnownType System_Security_Cryptography_HashAlgorithm = new KnownType("System.Security.Cryptography.HashAlgorithm");
        internal static readonly KnownType System_Security_Cryptography_HMACMD5 = new KnownType("System.Security.Cryptography.HMACMD5");
        internal static readonly KnownType System_Security_Cryptography_HMACRIPEMD160 = new KnownType("System.Security.Cryptography.HMACRIPEMD160");
        internal static readonly KnownType System_Security_Cryptography_HMACSHA1 = new KnownType("System.Security.Cryptography.HMACSHA1");
        internal static readonly KnownType System_Security_Cryptography_MD5 = new KnownType("System.Security.Cryptography.MD5");
        internal static readonly KnownType System_Security_Cryptography_PasswordDeriveBytes = new KnownType("System.Security.Cryptography.PasswordDeriveBytes");
        internal static readonly KnownType System_Security_Cryptography_RC2 = new KnownType("System.Security.Cryptography.RC2");
        internal static readonly KnownType System_Security_Cryptography_RIPEMD160 = new KnownType("System.Security.Cryptography.RIPEMD160");
        internal static readonly KnownType System_Security_Cryptography_RSA = new KnownType("System.Security.Cryptography.RSA");
        internal static readonly KnownType System_Security_Cryptography_RSACryptoServiceProvider = new KnownType("System.Security.Cryptography.RSACryptoServiceProvider");
        internal static readonly KnownType System_Security_Cryptography_SHA1 = new KnownType("System.Security.Cryptography.SHA1");
        internal static readonly KnownType System_Security_Cryptography_SymmetricAlgorithm = new KnownType("System.Security.Cryptography.SymmetricAlgorithm");
        internal static readonly KnownType System_Security_Cryptography_TripleDES = new KnownType("System.Security.Cryptography.TripleDES");
        internal static readonly KnownType System_Security_Cryptography_X509Certificates_X509Certificate2 = new KnownType("System.Security.Cryptography.X509Certificates.X509Certificate2");
        internal static readonly KnownType System_Security_Cryptography_X509Certificates_X509Chain = new KnownType("System.Security.Cryptography.X509Certificates.X509Chain");
        internal static readonly KnownType System_Security_Permissions_CodeAccessSecurityAttribute = new KnownType("System.Security.Permissions.CodeAccessSecurityAttribute");
        internal static readonly KnownType System_Security_Permissions_PrincipalPermission = new KnownType("System.Security.Permissions.PrincipalPermission");
        internal static readonly KnownType System_Security_Permissions_PrincipalPermissionAttribute = new KnownType("System.Security.Permissions.PrincipalPermissionAttribute");
        internal static readonly KnownType System_Security_PermissionSet = new KnownType("System.Security.PermissionSet");
        internal static readonly KnownType System_Security_Principal_IIdentity = new KnownType("System.Security.Principal.IIdentity");
        internal static readonly KnownType System_Security_Principal_IPrincipal = new KnownType("System.Security.Principal.IPrincipal");
        internal static readonly KnownType System_Security_Principal_WindowsIdentity = new KnownType("System.Security.Principal.WindowsIdentity");
        internal static readonly KnownType System_Security_SecurityCriticalAttribute = new KnownType("System.Security.SecurityCriticalAttribute");
        internal static readonly KnownType System_Security_SecuritySafeCriticalAttribute = new KnownType("System.Security.SecuritySafeCriticalAttribute");
        internal static readonly KnownType System_SerializableAttribute = new KnownType("System.SerializableAttribute");
        internal static readonly KnownType System_ServiceModel_OperationContractAttribute = new KnownType("System.ServiceModel.OperationContractAttribute");
        internal static readonly KnownType System_ServiceModel_ServiceContractAttribute = new KnownType("System.ServiceModel.ServiceContractAttribute");
        internal static readonly KnownType System_Single = new KnownType(SpecialType.System_Single, "float");
        internal static readonly KnownType System_StackOverflowException = new KnownType("System.StackOverflowException");
        internal static readonly KnownType System_STAThreadAttribute = new KnownType("System.STAThreadAttribute");
        internal static readonly KnownType System_String = new KnownType(SpecialType.System_String, "string");
        internal static readonly KnownType System_String_Array = new KnownType("string[]");
        internal static readonly KnownType System_String_Array_VB = new KnownType("String()");
        internal static readonly KnownType System_StringComparison = new KnownType("System.StringComparison");
        internal static readonly KnownType System_SystemException = new KnownType("System.SystemException");
        internal static readonly KnownType System_Text_RegularExpressions_Regex = new KnownType("System.Text.RegularExpressions.Regex");
        internal static readonly KnownType System_Text_StringBuilder = new KnownType("System.Text.StringBuilder");
        internal static readonly KnownType System_Threading_Tasks_Task = new KnownType("System.Threading.Tasks.Task");
        internal static readonly KnownType System_Threading_Tasks_Task_T = new KnownType("System.Threading.Tasks.Task<TResult>");
        internal static readonly KnownType System_Threading_Tasks_TaskFactory = new KnownType("System.Threading.Tasks.TaskFactory");
        internal static readonly KnownType System_Threading_Tasks_ValueTask = new KnownType("System.Threading.Tasks.ValueTask");
        internal static readonly KnownType System_Threading_Tasks_ValueTask_TResult = new KnownType("System.Threading.Tasks.ValueTask<TResult>");
        internal static readonly KnownType System_Threading_Thread = new KnownType("System.Threading.Thread");
        internal static readonly KnownType System_ThreadStaticAttribute = new KnownType("System.ThreadStaticAttribute");
        internal static readonly KnownType System_Type = new KnownType("System.Type");
        internal static readonly KnownType System_UInt16 = new KnownType(SpecialType.System_UInt16, "ushort");
        internal static readonly KnownType System_UInt32 = new KnownType(SpecialType.System_UInt32, "uint");
        internal static readonly KnownType System_UInt64 = new KnownType(SpecialType.System_UInt64, "ulong");
        internal static readonly KnownType System_UIntPtr = new KnownType(SpecialType.System_UIntPtr, "UIntPtr");
        internal static readonly KnownType System_Uri = new KnownType("System.Uri");
        internal static readonly KnownType System_ValueType = new KnownType(SpecialType.System_ValueType, "ValueType");
        internal static readonly KnownType System_Web_HttpApplication = new KnownType("System.Web.HttpApplication");
        internal static readonly KnownType System_Web_HttpCookie = new KnownType("System.Web.HttpCookie");
        internal static readonly KnownType System_Web_HttpContext = new KnownType("System.Web.HttpContext");
        internal static readonly KnownType System_Web_Http_ApiController = new KnownType("System.Web.Http.ApiController");
        internal static readonly KnownType System_Web_Mvc_NonActionAttribute = new KnownType("System.Web.Mvc.NonActionAttribute");
        internal static readonly KnownType System_Web_Mvc_Controller = new KnownType("System.Web.Mvc.Controller");
        internal static readonly KnownType System_Web_Mvc_HttpPostAttribute = new KnownType("System.Web.Mvc.HttpPostAttribute");
        internal static readonly KnownType System_Web_Mvc_ValidateInputAttribute = new KnownType("System.Web.Mvc.ValidateInputAttribute");
        internal static readonly KnownType System_Web_Script_Serialization_JavaScriptSerializer = new KnownType("System.Web.Script.Serialization.JavaScriptSerializer");
        internal static readonly KnownType System_Web_Script_Serialization_JavaScriptTypeResolver = new KnownType("System.Web.Script.Serialization.JavaScriptTypeResolver");
        internal static readonly KnownType System_Web_Script_Serialization_SimpleTypeResolver = new KnownType("System.Web.Script.Serialization.SimpleTypeResolver");
        internal static readonly KnownType System_Web_UI_LosFormatter = new KnownType("System.Web.UI.LosFormatter");
        internal static readonly KnownType System_Windows_DependencyObject = new KnownType("System.Windows.DependencyObject");
        internal static readonly KnownType System_Windows_Forms_Application = new KnownType("System.Windows.Forms.Application");
        internal static readonly KnownType System_Windows_Markup_ConstructorArgumentAttribute = new KnownType("System.Windows.Markup.ConstructorArgumentAttribute");
        internal static readonly KnownType System_Xml_XmlDocument = new KnownType("System.Xml.XmlDocument");
        internal static readonly KnownType System_Xml_XmlDataDocument = new KnownType("System.Xml.XmlDataDocument");
        internal static readonly KnownType System_Xml_XmlNode = new KnownType("System.Xml.XmlNode");
        internal static readonly KnownType System_Xml_XPath_XPathDocument = new KnownType("System.Xml.XPath.XPathDocument");
        internal static readonly KnownType System_Xml_XmlReader = new KnownType("System.Xml.XmlReader");
        internal static readonly KnownType System_Xml_XmlReaderSettings = new KnownType("System.Xml.XmlReaderSettings");
        internal static readonly KnownType System_Xml_XmlUrlResolver = new KnownType("System.Xml.XmlUrlResolver");
        internal static readonly KnownType System_Xml_XmlTextReader = new KnownType("System.Xml.XmlTextReader");
        internal static readonly KnownType System_Xml_Resolvers_XmlPreloadedResolver = new KnownType("System.Xml.Resolvers.XmlPreloadedResolver");
        internal static readonly KnownType NSubstitute_SubstituteExtensions = new KnownType("NSubstitute.SubstituteExtensions");
        internal static readonly KnownType NSubstitute_Received = new KnownType("NSubstitute.Received");
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
                new KnownType("System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>")
            );
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
                new KnownType("System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>")
            );
        internal static readonly ImmutableArray<KnownType> SystemTasks =
            ImmutableArray.Create(
                System_Threading_Tasks_Task,
                System_Threading_Tasks_Task_T,
                System_Threading_Tasks_ValueTask_TResult
            );
        internal static readonly KnownType Sytem_Resources_ResourceManager = new KnownType("System.Resources.ResourceManager");
        internal static readonly KnownType UnityEditor_AssetModificationProcessor = new KnownType("UnityEditor.AssetModificationProcessor");
        internal static readonly KnownType UnityEditor_AssetPostprocessor = new KnownType("UnityEditor.AssetPostprocessor");
        internal static readonly KnownType UnityEngine_MonoBehaviour = new KnownType("UnityEngine.MonoBehaviour");
        internal static readonly KnownType UnityEngine_ScriptableObject = new KnownType("UnityEngine.ScriptableObject");
        internal static readonly KnownType Xunit_Assert = new KnownType("Xunit.Assert");
        internal static readonly KnownType Xunit_FactAttribute = new KnownType("Xunit.FactAttribute");
        internal static readonly KnownType Xunit_TheoryAttribute = new KnownType("Xunit.TheoryAttribute");
        internal static readonly KnownType LegacyXunit_TheoryAttribute = new KnownType("Xunit.Extensions.TheoryAttribute");
        internal static readonly ImmutableArray<KnownType> CallerInfoAttributes =
            ImmutableArray.Create(
                System_Runtime_CompilerServices_CallerFilePathAttribute,
                System_Runtime_CompilerServices_CallerLineNumberAttribute,
                System_Runtime_CompilerServices_CallerMemberNameAttribute
            );
        internal static readonly ImmutableArray<KnownType> FloatingPointNumbers =
            ImmutableArray.Create(
                System_Single,
                System_Double
            );
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
                System_SByte
            );
        internal static readonly ImmutableArray<KnownType> NonIntegralNumbers =
            ImmutableArray.Create(
                System_Single,
                System_Double,
                System_Decimal
            );
        internal static readonly ImmutableArray<KnownType> PointerTypes =
            ImmutableArray.Create(
                System_IntPtr,
                System_UIntPtr
            );
        internal static readonly ImmutableArray<KnownType> UnsignedIntegers =
            ImmutableArray.Create(
                System_UInt64,
                System_UInt32,
                System_UInt16
            );
        #endregion Known types

        private readonly bool isSpecialType;
        private readonly SpecialType specialType;
        private readonly Lazy<string> shortName;

        private KnownType(string typeName)
            : this(SpecialType.None, typeName)
        {
        }

        private KnownType(SpecialType specialType, string typeName)
        {
            TypeName = typeName;
            shortName = new Lazy<string>(() => typeName.Split('.').Last());
            this.specialType = specialType;
            isSpecialType = specialType != SpecialType.None;
        }

        public string TypeName { get; }

        public string ShortName => shortName.Value;

        internal bool Matches(string type) => !isSpecialType && TypeName == type;

        internal bool Matches(SpecialType type) => isSpecialType && specialType == type;
    }
}

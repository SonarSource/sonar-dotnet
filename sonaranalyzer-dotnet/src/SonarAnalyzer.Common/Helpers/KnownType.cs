/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    internal sealed class KnownType
    {
        #region Known types

        internal static readonly KnownType Microsoft_AspNetCore_Mvc_Controller = new KnownType("Microsoft.AspNetCore.Mvc.Controller");
        internal static readonly KnownType Microsoft_VisualBasic_Interaction = new KnownType("Microsoft.VisualBasic.Interaction");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_Assert = new KnownType("Microsoft.VisualStudio.TestTools.UnitTesting.Assert");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_ExpectedExceptionAttribute = new KnownType("Microsoft.VisualStudio.TestTools.UnitTesting.ExpectedExceptionAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_IgnoreAttribute = new KnownType("Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_TestClassAttribute = new KnownType("Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_TestMethodAttribute = new KnownType("Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_DataTestMethodAttribute = new KnownType("Microsoft.VisualStudio.TestTools.UnitTesting.DataTestMethodAttribute");
        internal static readonly KnownType Microsoft_VisualStudio_TestTools_UnitTesting_WorkItemAttribute = new KnownType("Microsoft.VisualStudio.TestTools.UnitTesting.WorkItemAttribute");
        internal static readonly KnownType NUnit_Framework_Assert = new KnownType("NUnit.Framework.Assert");
        internal static readonly KnownType NUnit_Framework_ExpectedExceptionAttribute = new KnownType("NUnit.Framework.ExpectedExceptionAttribute");
        internal static readonly KnownType NUnit_Framework_IgnoreAttribute = new KnownType("NUnit.Framework.IgnoreAttribute");
        internal static readonly KnownType NUnit_Framework_TestAttribute = new KnownType("NUnit.Framework.TestAttribute");
        internal static readonly KnownType NUnit_Framework_TestCaseAttribute = new KnownType("NUnit.Framework.TestCaseAttribute");
        internal static readonly KnownType NUnit_Framework_TestCaseSourceAttribute = new KnownType("NUnit.Framework.TestCaseSourceAttribute");
        internal static readonly KnownType NUnit_Framework_TestFixtureAttribute = new KnownType("NUnit.Framework.TestFixtureAttribute");
        internal static readonly KnownType NUnit_Framework_TheoryAttribute = new KnownType("NUnit.Framework.TheoryAttribute");
        internal static readonly KnownType Oracle_ManagedDataAccess_Client_OracleCommand = new KnownType("Oracle.ManagedDataAccess.Client.OracleCommand");
        internal static readonly KnownType Oracle_ManagedDataAccess_Client_OracleDataAdapter = new KnownType("Oracle.ManagedDataAccess.Client.OracleDataAdapter");
        internal static readonly KnownType System_Action_T = new KnownType("System.Action<T>");
        internal static readonly KnownType System_Activator = new KnownType("System.Activator");
        internal static readonly KnownType System_ApplicationException = new KnownType("System.ApplicationException");
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
        internal static readonly KnownType System_Collections_Generic_ICollection_T = new KnownType(SpecialType.System_Collections_Generic_ICollection_T, "System.Collections.Generic.ICollection<T>");
        internal static readonly KnownType System_Collections_Generic_IEnumerable_T = new KnownType(SpecialType.System_Collections_Generic_IEnumerable_T, "System.Collections.Generic.IEnumerable<T>");
        internal static readonly KnownType System_Collections_Generic_IList_T = new KnownType(SpecialType.System_Collections_Generic_IList_T, "System.Collections.Generic.IList<T>");
        internal static readonly KnownType System_Collections_Generic_IReadOnlyCollection_T = new KnownType("System.Collections.Generic.IReadOnlyCollection<T>");
        internal static readonly KnownType System_Collections_Generic_IReadOnlyDictionary_TKey_TValue = new KnownType("System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>");
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
        internal static readonly KnownType System_ComponentModel_DefaultValueAttribute = new KnownType("System.ComponentModel.DefaultValueAttribute");
        internal static readonly KnownType System_ComponentModel_LocalizableAttribute = new KnownType("System.ComponentModel.LocalizableAttribute");
        internal static readonly KnownType System_ComponentModel_Composition_CreationPolicy = new KnownType("System.ComponentModel.Composition.CreationPolicy");
        internal static readonly KnownType System_ComponentModel_Composition_ExportAttribute = new KnownType("System.ComponentModel.Composition.ExportAttribute");
        internal static readonly KnownType System_ComponentModel_Composition_InheritedExportAttribute = new KnownType("System.ComponentModel.Composition.InheritedExportAttribute");
        internal static readonly KnownType System_ComponentModel_Composition_PartCreationPolicyAttribute = new KnownType("System.ComponentModel.Composition.PartCreationPolicyAttribute");
        internal static readonly KnownType System_Console = new KnownType("System.Console");
        internal static readonly KnownType System_Data_Common_CommandTrees_DbExpression = new KnownType("System.Data.Common.CommandTrees.DbExpression");
        internal static readonly KnownType System_Data_DataSet = new KnownType("System.Data.DataSet");
        internal static readonly KnownType System_Data_DataTable = new KnownType("System.Data.DataTable");
        internal static readonly KnownType System_Data_Odbc_OdbcCommand = new KnownType("System.Data.Odbc.OdbcCommand");
        internal static readonly KnownType System_Data_Odbc_OdbcDataAdapter = new KnownType("System.Data.Odbc.OdbcDataAdapter");
        internal static readonly KnownType System_Data_OleDb_OleDbCommand = new KnownType("System.Data.OleDb.OleDbCommand");
        internal static readonly KnownType System_Data_OleDb_OleDbDataAdapter = new KnownType("System.Data.OleDb.OleDbDataAdapter");
        internal static readonly KnownType System_Data_SqlClient_SqlCommand = new KnownType("System.Data.SqlClient.SqlCommand");
        internal static readonly KnownType System_Data_SqlClient_SqlDataAdapter = new KnownType("System.Data.SqlClient.SqlDataAdapter");
        internal static readonly KnownType System_Data_SqlServerCe_SqlCeCommand = new KnownType("System.Data.SqlServerCe.SqlCeCommand");
        internal static readonly KnownType System_Data_SqlServerCe_SqlCeDataAdapter = new KnownType("System.Data.SqlServerCe.SqlCeDataAdapter");
        internal static readonly KnownType System_DateTime = new KnownType(SpecialType.System_DateTime, "DateTime");
        internal static readonly KnownType System_Decimal = new KnownType(SpecialType.System_Decimal, "decimal");
        internal static readonly KnownType System_Delegate = new KnownType("System.Delegate");
        internal static readonly KnownType System_Diagnostics_CodeAnalysis_SuppressMessageAttribute = new KnownType("System.Diagnostics.CodeAnalysis.SuppressMessageAttribute");
        internal static readonly KnownType System_Diagnostics_Contracts_PureAttribute = new KnownType("System.Diagnostics.Contracts.PureAttribute");
        internal static readonly KnownType System_Diagnostics_Debug = new KnownType("System.Diagnostics.Debug");
        internal static readonly KnownType System_Diagnostics_Trace = new KnownType("System.Diagnostics.Trace");
        internal static readonly KnownType System_Diagnostics_TraceSource = new KnownType("System.Diagnostics.TraceSource");
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
        internal static readonly KnownType System_Func_T = new KnownType("System.Func<T, TResult>");
        internal static readonly KnownType System_GC = new KnownType("System.GC");
        internal static readonly KnownType System_Globalization_CompareOptions = new KnownType("System.Globalization.CompareOptions");
        internal static readonly KnownType System_Globalization_CultureInfo = new KnownType("System.Globalization.CultureInfo");
        internal static readonly KnownType System_IComparable = new KnownType("System.IComparable");
        internal static readonly KnownType System_IComparable_T = new KnownType("System.IComparable<T>");
        internal static readonly KnownType System_IDisposable = new KnownType(SpecialType.System_IDisposable, "System.IDisposable");
        internal static readonly KnownType System_IEquatable_T = new KnownType("System.IEquatable<T>");
        internal static readonly KnownType System_IFormatProvider = new KnownType("System.IFormatProvider");
        internal static readonly KnownType System_IndexOutOfRangeException = new KnownType("System.IndexOutOfRangeException");
        internal static readonly KnownType System_Int16 = new KnownType(SpecialType.System_Int16, "short");
        internal static readonly KnownType System_Int32 = new KnownType(SpecialType.System_Int32, "int");
        internal static readonly KnownType System_Int64 = new KnownType(SpecialType.System_Int64, "long");
        internal static readonly KnownType System_IntPtr = new KnownType(SpecialType.System_IntPtr, "IntPtr");
        internal static readonly KnownType System_InvalidOperationException = new KnownType("System.InvalidOperationException");
        internal static readonly KnownType System_IO_FileStream = new KnownType("System.IO.FileStream");
        internal static readonly KnownType System_IO_Stream = new KnownType("System.IO.Stream");
        internal static readonly KnownType System_IO_StreamReader = new KnownType("System.IO.StreamReader");
        internal static readonly KnownType System_IO_StreamWriter = new KnownType("System.IO.StreamWriter");
        internal static readonly KnownType System_IO_TextWriter = new KnownType("System.IO.TextWriter");
        internal static readonly KnownType System_Linq_Enumerable = new KnownType("System.Linq.Enumerable");
        internal static readonly KnownType System_Linq_Expressions_Expression_T = new KnownType("System.Linq.Expressions.Expression<TDelegate>");
        internal static readonly KnownType System_Linq_ImmutableArrayExtensions = new KnownType("System.Linq.ImmutableArrayExtensions");
        internal static readonly KnownType System_MarshalByRefObject = new KnownType("System.MarshalByRefObject");
        internal static readonly KnownType System_MTAThreadAttribute = new KnownType("System.MTAThreadAttribute");
        internal static readonly KnownType System_Net_Sockets_TcpClient = new KnownType("System.Net.Sockets.TcpClient");
        internal static readonly KnownType System_Net_Sockets_UdpClient = new KnownType("System.Net.Sockets.UdpClient");
        internal static readonly KnownType System_Net_WebClient = new KnownType("System.Net.WebClient");
        internal static readonly KnownType System_NonSerializedAttribute = new KnownType("System.NonSerializedAttribute");
        internal static readonly KnownType System_NotImplementedException = new KnownType("System.NotImplementedException");
        internal static readonly KnownType System_NotSupportedException = new KnownType("System.NotSupportedException");
        internal static readonly KnownType System_Nullable_T = new KnownType(SpecialType.System_Nullable_T, "System.Nullable<T>");
        internal static readonly KnownType System_NullReferenceException = new KnownType("System.NullReferenceException");
        internal static readonly KnownType System_Object = new KnownType(SpecialType.System_Object, "object");
        internal static readonly KnownType System_ObsoleteAttribute = new KnownType("System.ObsoleteAttribute");
        internal static readonly KnownType System_OutOfMemoryException = new KnownType("System.OutOfMemoryException");
        internal static readonly KnownType System_Reflection_Assembly = new KnownType("System.Reflection.Assembly");
        internal static readonly KnownType System_Reflection_AssemblyVersionAttribute = new KnownType("System.Reflection.AssemblyVersionAttribute");
        internal static readonly KnownType System_Reflection_MemberInfo = new KnownType("System.Reflection.MemberInfo");
        internal static readonly KnownType System_Reflection_Module = new KnownType("System.Reflection.Module");
        internal static readonly KnownType System_Reflection_ParameterInfo = new KnownType("System.Reflection.ParameterInfo");
        internal static readonly KnownType System_Resources_NeutralResourcesLanguageAttribute = new KnownType("System.Resources.NeutralResourcesLanguageAttribute");
        internal static readonly KnownType System_Runtime_CompilerServices_CallerFilePathAttribute = new KnownType("System.Runtime.CompilerServices.CallerFilePathAttribute");
        internal static readonly KnownType System_Runtime_CompilerServices_CallerLineNumberAttribute = new KnownType("System.Runtime.CompilerServices.CallerLineNumberAttribute");
        internal static readonly KnownType System_Runtime_CompilerServices_CallerMemberNameAttribute = new KnownType("System.Runtime.CompilerServices.CallerMemberNameAttribute");
        internal static readonly KnownType System_Runtime_CompilerServices_InternalsVisibleToAttribute = new KnownType("System.Runtime.CompilerServices.InternalsVisibleToAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_ComImportAttribute = new KnownType("System.Runtime.InteropServices.ComImportAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_ComVisibleAttribute = new KnownType("System.Runtime.InteropServices.ComVisibleAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_DefaultParameterValueAttribute = new KnownType("System.Runtime.InteropServices.DefaultParameterValueAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_DllImportAttribute = new KnownType("System.Runtime.InteropServices.DllImportAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_HandleRef = new KnownType("System.Runtime.InteropServices.HandleRef");
        internal static readonly KnownType System_Runtime_InteropServices_InterfaceTypeAttribute = new KnownType("System.Runtime.InteropServices.InterfaceTypeAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_OptionalAttribute = new KnownType("System.Runtime.InteropServices.OptionalAttribute");
        internal static readonly KnownType System_Runtime_InteropServices_SafeHandle = new KnownType("System.Runtime.InteropServices.SafeHandle");
        internal static readonly KnownType System_Runtime_InteropServices_StructLayoutAttribute = new KnownType("System.Runtime.InteropServices.StructLayout");
        internal static readonly KnownType System_Runtime_Serialization_DataMemberAttribute = new KnownType("System.Runtime.Serialization.DataMemberAttribute");
        internal static readonly KnownType System_Runtime_Serialization_ISerializable = new KnownType("System.Runtime.Serialization.ISerializable");
        internal static readonly KnownType System_Runtime_Serialization_OnDeserializedAttribute = new KnownType("System.Runtime.Serialization.OnDeserializedAttribute");
        internal static readonly KnownType System_Runtime_Serialization_OnDeserializingAttribute = new KnownType("System.Runtime.Serialization.OnDeserializingAttribute");
        internal static readonly KnownType System_Runtime_Serialization_OnSerializedAttribute = new KnownType("System.Runtime.Serialization.OnSerializedAttribute");
        internal static readonly KnownType System_Runtime_Serialization_OnSerializingAttribute = new KnownType("System.Runtime.Serialization.OnSerializingAttribute");
        internal static readonly KnownType System_Runtime_Serialization_OptionalFieldAttribute = new KnownType("System.Runtime.Serialization.OptionalFieldAttribute");
        internal static readonly KnownType System_Runtime_Serialization_SerializationInfo = new KnownType("System.Runtime.Serialization.SerializationInfo");
        internal static readonly KnownType System_Runtime_Serialization_StreamingContext = new KnownType("System.Runtime.Serialization.StreamingContext");
        internal static readonly KnownType System_SByte = new KnownType(SpecialType.System_SByte, "sbyte");
        internal static readonly KnownType System_Security_AllowPartiallyTrustedCallersAttribute = new KnownType("System.Security.AllowPartiallyTrustedCallersAttribute");
        internal static readonly KnownType System_Security_Cryptography_DES = new KnownType("System.Security.Cryptography.DES");
        internal static readonly KnownType System_Security_Cryptography_DSA = new KnownType("System.Security.Cryptography.DSA");
        internal static readonly KnownType System_Security_Cryptography_HashAlgorithm = new KnownType("System.Security.Cryptography.HashAlgorithm");
        internal static readonly KnownType System_Security_Cryptography_HMACMD5 = new KnownType("System.Security.Cryptography.HMACMD5");
        internal static readonly KnownType System_Security_Cryptography_HMACRIPEMD160 = new KnownType("System.Security.Cryptography.HMACRIPEMD160");
        internal static readonly KnownType System_Security_Cryptography_HMACSHA1 = new KnownType("System.Security.Cryptography.HMACSHA1");
        internal static readonly KnownType System_Security_Cryptography_MD5 = new KnownType("System.Security.Cryptography.MD5");
        internal static readonly KnownType System_Security_Cryptography_RC2 = new KnownType("System.Security.Cryptography.RC2");
        internal static readonly KnownType System_Security_Cryptography_RIPEMD160 = new KnownType("System.Security.Cryptography.RIPEMD160");
        internal static readonly KnownType System_Security_Cryptography_RSACryptoServiceProvider = new KnownType("System.Security.Cryptography.RSACryptoServiceProvider");
        internal static readonly KnownType System_Security_Cryptography_SHA1 = new KnownType("System.Security.Cryptography.SHA1");
        internal static readonly KnownType System_Security_Cryptography_TripleDES = new KnownType("System.Security.Cryptography.TripleDES");
        internal static readonly KnownType System_Security_Permissions_CodeAccessSecurityAttribute = new KnownType("System.Security.Permissions.CodeAccessSecurityAttribute");
        internal static readonly KnownType System_Security_PermissionSet = new KnownType("System.Security.PermissionSet");
        internal static readonly KnownType System_SerializableAttribute = new KnownType("System.SerializableAttribute");
        internal static readonly KnownType System_ServiceModel_OperationContractAttribute = new KnownType("System.ServiceModel.OperationContractAttribute");
        internal static readonly KnownType System_ServiceModel_ServiceContractAttribute = new KnownType("System.ServiceModel.ServiceContractAttribute");
        internal static readonly KnownType System_Single = new KnownType(SpecialType.System_Single, "float");
        internal static readonly KnownType System_StackOverflowException = new KnownType("System.StackOverflowException");
        internal static readonly KnownType System_STAThreadAttribute = new KnownType("System.STAThreadAttribute");
        internal static readonly KnownType System_String = new KnownType(SpecialType.System_String, "string");
        internal static readonly KnownType System_String_Array = new KnownType("string[]");
        internal static readonly KnownType System_StringComparison = new KnownType("System.StringComparison");
        internal static readonly KnownType System_SystemException = new KnownType("System.SystemException");
        internal static readonly KnownType System_Text_StringBuilder = new KnownType("System.Text.StringBuilder");
        internal static readonly KnownType System_Threading_Tasks_Task = new KnownType("System.Threading.Tasks.Task");
        internal static readonly KnownType System_Threading_Tasks_Task_T = new KnownType("System.Threading.Tasks.Task<TResult>");
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
        internal static readonly KnownType System_Web_Http_ApiController = new KnownType("System.Web.Http.ApiController");
        internal static readonly KnownType System_Web_Mvc_Controller = new KnownType("System.Web.Mvc.Controller");
        internal static readonly KnownType System_Windows_DependencyObject = new KnownType("System.Windows.DependencyObject");
        internal static readonly KnownType System_Windows_Forms_Application = new KnownType("System.Windows.Forms.Application");
        internal static readonly KnownType System_Windows_Markup_ConstructorArgumentAttribute = new KnownType("System.Windows.Markup.ConstructorArgumentAttribute");
        internal static readonly KnownType System_Xml_XmlDocument = new KnownType("System.Xml.XmlDocument");
        internal static readonly KnownType System_Xml_XmlNode = new KnownType("System.Xml.XmlNode");
        internal static readonly ISet<KnownType> SystemActionVariants = new HashSet<KnownType>
        {
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
        };
        internal static readonly ISet<KnownType> SystemFuncVariants = new HashSet<KnownType>
        {
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
        };
        internal static readonly KnownType Sytem_Resources_ResourceManager = new KnownType("System.Resources.ResourceManager");
        internal static readonly KnownType Xunit_Assert = new KnownType("Xunit.Assert");
        internal static readonly KnownType Xunit_FactAttribute = new KnownType("Xunit.FactAttribute");
        internal static readonly KnownType Xunit_TheoryAttribute = new KnownType("Xunit.TheoryAttribute");
        internal static readonly ISet<KnownType> CallerInfoAttributes = new HashSet<KnownType>
        {
            System_Runtime_CompilerServices_CallerFilePathAttribute,
            System_Runtime_CompilerServices_CallerLineNumberAttribute,
            System_Runtime_CompilerServices_CallerMemberNameAttribute
        };
        internal static readonly ISet<KnownType> FloatingPointNumbers = new HashSet<KnownType>
        {
            System_Single,
            System_Double
        };
        internal static readonly ISet<KnownType> IntegralNumbers = new HashSet<KnownType>
        {
            System_Int16,
            System_Int32,
            System_Int64,
            System_UInt16,
            System_UInt32,
            System_UInt64,
            System_Char,
            System_Byte,
            System_SByte
        };
        internal static readonly ISet<KnownType> NonIntegralNumbers = new HashSet<KnownType>
        {
            System_Single,
            System_Double,
            System_Decimal
        };
        internal static readonly ISet<KnownType> PointerTypes = new HashSet<KnownType>
        {
            System_IntPtr,
            System_UIntPtr
        };
        internal static readonly ISet<KnownType> UnsignedIntegers = new HashSet<KnownType>
        {
            System_UInt64,
            System_UInt32,
            System_UInt16
        };

        #endregion Known types

        private readonly bool isSpecialType;
        private readonly SpecialType specialType;
        private KnownType(string typeName)
        {
            TypeName = typeName;
            specialType = SpecialType.None;
            isSpecialType = false;
        }

        private KnownType(SpecialType specialType, string typeName)
        {
            TypeName = typeName;
            this.specialType = specialType;
            isSpecialType = true;
        }

        public string TypeName { get; }
        internal bool Matches(string type)
        {
            return !isSpecialType && TypeName == type;
        }

        internal bool Matches(SpecialType type)
        {
            return isSpecialType && specialType == type;
        }
    }
}

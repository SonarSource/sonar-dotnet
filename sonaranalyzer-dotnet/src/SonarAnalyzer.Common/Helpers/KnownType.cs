/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    internal sealed class KnownType
    {
        #region Known types

        public static readonly KnownType System_Nullable_T = new KnownType(SpecialType.System_Nullable_T, "System.Nullable<T>");
        public static readonly KnownType System_Collections_Generic_IEnumerable_T = new KnownType(SpecialType.System_Collections_Generic_IEnumerable_T, "System.Collections.Generic.IEnumerable<T>");
        public static readonly KnownType System_Collections_IEnumerable = new KnownType(SpecialType.System_Collections_IEnumerable, "System.Collections.IEnumerable");
        public static readonly KnownType System_IDisposable = new KnownType(SpecialType.System_IDisposable, "System.IDisposable");
        public static readonly KnownType System_Array = new KnownType(SpecialType.System_Array, "System.Array");
        public static readonly KnownType System_Collections_Generic_IList_T = new KnownType(SpecialType.System_Collections_Generic_IList_T, "System.Collections.Generic.IList<T>");
        public static readonly KnownType System_Collections_Generic_ICollection_T = new KnownType(SpecialType.System_Collections_Generic_ICollection_T, "System.Collections.Generic.ICollection<T>");

        public static readonly KnownType System_Object = new KnownType(SpecialType.System_Object, "object");
        public static readonly KnownType System_String = new KnownType(SpecialType.System_String, "string");
        public static readonly KnownType System_Boolean = new KnownType(SpecialType.System_Boolean, "bool");
        public static readonly KnownType System_Int64 = new KnownType(SpecialType.System_Int64, "long");
        public static readonly KnownType System_Int32 = new KnownType(SpecialType.System_Int32, "int");
        public static readonly KnownType System_Int16 = new KnownType(SpecialType.System_Int16, "short");
        public static readonly KnownType System_UInt64 = new KnownType(SpecialType.System_UInt64, "ulong");
        public static readonly KnownType System_UInt32 = new KnownType(SpecialType.System_UInt32, "uint");
        public static readonly KnownType System_UInt16 = new KnownType(SpecialType.System_UInt16, "ushort");
        public static readonly KnownType System_Decimal = new KnownType(SpecialType.System_Decimal, "decimal");
        public static readonly KnownType System_Single = new KnownType(SpecialType.System_Single, "float");
        public static readonly KnownType System_Double = new KnownType(SpecialType.System_Double, "double");
        public static readonly KnownType System_Char = new KnownType(SpecialType.System_Char, "char");
        public static readonly KnownType System_Byte = new KnownType(SpecialType.System_Byte, "byte");
        public static readonly KnownType System_SByte = new KnownType(SpecialType.System_SByte, "sbyte");
        public static readonly KnownType System_DateTime = new KnownType(SpecialType.System_DateTime, "DateTime");

        public static readonly KnownType System_String_Array = new KnownType("string[]");

        public static readonly ISet<KnownType> FloatingPointNumbers = ImmutableHashSet.Create(
            System_Single,
            System_Double);

        public static readonly ISet<KnownType> NonIntegralNumbers = ImmutableHashSet.Create(
            System_Single,
            System_Double,
            System_Decimal);

        public static readonly ISet<KnownType> UnsignedIntegers = ImmutableHashSet.Create(
            System_UInt64,
            System_UInt32,
            System_UInt16);

        public static readonly ISet<KnownType> IntegralNumbers = ImmutableHashSet.Create(
            System_Int16,
            System_Int32,
            System_Int64,
            System_UInt16,
            System_UInt32,
            System_UInt64,
            System_Char,
            System_Byte,
            System_SByte);

        public static readonly KnownType System_Exception = new KnownType("System.Exception");
        public static readonly KnownType System_Type = new KnownType("System.Type");
        public static readonly KnownType System_GC = new KnownType("System.GC");
        public static readonly KnownType System_IFormatProvider = new KnownType("System.IFormatProvider");
        public static readonly KnownType System_FlagsAttribute = new KnownType("System.FlagsAttribute");
        public static readonly KnownType System_ThreadStaticAttribute = new KnownType("System.ThreadStaticAttribute");
        public static readonly KnownType System_Console = new KnownType("System.Console");
        public static readonly KnownType System_Collections_IList = new KnownType("System.Collections.IList");
        public static readonly KnownType System_Collections_Generic_List_T = new KnownType("System.Collections.Generic.List<T>");
        public static readonly KnownType System_EventArgs = new KnownType("System.EventArgs");
        public static readonly KnownType System_Delegate = new KnownType("System.Delegate");
        public static readonly KnownType System_SerializableAttribute = new KnownType("System.SerializableAttribute");
        public static readonly KnownType System_NonSerializedAttribute = new KnownType("System.NonSerializedAttribute");
        public static readonly KnownType System_Attribute = new KnownType("System.Attribute");
        public static readonly KnownType System_AttributeUsageAttribute = new KnownType("System.AttributeUsageAttribute");

        public static readonly KnownType System_Collections_Generic_IReadOnlyCollection_T = new KnownType("System.Collections.Generic.IReadOnlyCollection<T>");
        public static readonly KnownType System_Collections_Generic_IReadOnlyDictionary_TKey_TValue = new KnownType("System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>");

        public static readonly KnownType System_IO_FileStream = new KnownType("System.IO.FileStream");
        public static readonly KnownType System_IO_StreamReader = new KnownType("System.IO.StreamReader");
        public static readonly KnownType System_IO_StreamWriter = new KnownType("System.IO.StreamWriter");
        public static readonly KnownType System_IO_Stream = new KnownType("System.IO.Stream");

        public static readonly KnownType System_Net_WebClient = new KnownType("System.Net.WebClient");

        public static readonly KnownType System_Net_Sockets_TcpClient = new KnownType("System.Net.Sockets.TcpClient");
        public static readonly KnownType System_Net_Sockets_UdpClient = new KnownType("System.Net.Sockets.UdpClient");

        public static readonly KnownType System_Drawing_Image = new KnownType("System.Drawing.Image");
        public static readonly KnownType System_Drawing_Bitmap = new KnownType("System.Drawing.Bitmap");

        public static readonly KnownType System_Runtime_InteropServices_DefaultParameterValueAttribute = new KnownType("System.Runtime.InteropServices.DefaultParameterValueAttribute");
        public static readonly KnownType System_Runtime_InteropServices_OptionalAttribute = new KnownType("System.Runtime.InteropServices.OptionalAttribute");
        public static readonly KnownType System_Runtime_InteropServices_SafeHandle = new KnownType("System.Runtime.InteropServices.SafeHandle");
        public static readonly KnownType System_Runtime_InteropServices_DllImportAttribute = new KnownType("System.Runtime.InteropServices.DllImportAttribute");
        public static readonly KnownType System_ComponentModel_DefaultValueAttribute = new KnownType("System.ComponentModel.DefaultValueAttribute");

        public static readonly KnownType System_Globalization_CultureInfo = new KnownType("System.Globalization.CultureInfo");
        public static readonly KnownType System_Globalization_CompareOptions = new KnownType("System.Globalization.CompareOptions");
        public static readonly KnownType System_StringComparison = new KnownType("System.StringComparison");

        public static readonly KnownType System_Security_Cryptography_DES = new KnownType("System.Security.Cryptography.DES");
        public static readonly KnownType System_Security_Cryptography_TripleDES = new KnownType("System.Security.Cryptography.TripleDES");
        public static readonly KnownType System_Security_Cryptography_HashAlgorithm = new KnownType("System.Security.Cryptography.HashAlgorithm");
        public static readonly KnownType System_Security_Cryptography_SHA1 = new KnownType("System.Security.Cryptography.SHA1");
        public static readonly KnownType System_Security_Cryptography_MD5 = new KnownType("System.Security.Cryptography.MD5");
        public static readonly KnownType System_Security_Cryptography_DSA = new KnownType("System.Security.Cryptography.DSA");
        public static readonly KnownType System_Security_Cryptography_RC2 = new KnownType("System.Security.Cryptography.RC2");
        public static readonly KnownType System_Security_Cryptography_RIPEMD160 = new KnownType("System.Security.Cryptography.RIPEMD160");
        public static readonly KnownType System_Security_Cryptography_HMACSHA1 = new KnownType("System.Security.Cryptography.HMACSHA1");
        public static readonly KnownType System_Security_Cryptography_HMACMD5 = new KnownType("System.Security.Cryptography.HMACMD5");
        public static readonly KnownType System_Security_Cryptography_HMACRIPEMD160 = new KnownType("System.Security.Cryptography.HMACRIPEMD160");

        public static readonly KnownType System_Reflection_Assembly = new KnownType("System.Reflection.Assembly");
        public static readonly KnownType System_Reflection_AssemblyVersionAttribute = new KnownType("System.Reflection.AssemblyVersionAttribute");
        public static readonly KnownType System_Reflection_MemberInfo = new KnownType("System.Reflection.MemberInfo");
        public static readonly KnownType System_Reflection_Module = new KnownType("System.Reflection.Module");
        public static readonly KnownType System_Data_Common_CommandTrees_DbExpression = new KnownType("System.Data.Common.CommandTrees.DbExpression");
        public static readonly KnownType System_Windows_DependencyObject = new KnownType("System.Windows.DependencyObject");

        public static readonly KnownType System_Collections_Immutable_ImmutableArray = new KnownType("System.Collections.Immutable.ImmutableArray");
        public static readonly KnownType System_Collections_Immutable_ImmutableArray_T = new KnownType("System.Collections.Immutable.ImmutableArray<T>");
        public static readonly KnownType System_Collections_Immutable_IImmutableArray_T = new KnownType("System.Collections.Immutable.IImmutableArray<T>");
        public static readonly KnownType System_Collections_Immutable_ImmutableDictionary = new KnownType("System.Collections.Immutable.ImmutableDictionary");
        public static readonly KnownType System_Collections_Immutable_ImmutableDictionary_TKey_TValue = new KnownType("System.Collections.Immutable.ImmutableDictionary<TKey, TValue>");
        public static readonly KnownType System_Collections_Immutable_IImmutableDictionary_TKey_TValue = new KnownType("System.Collections.Immutable.IImmutableDictionary<TKey, TValue>");
        public static readonly KnownType System_Collections_Immutable_ImmutableHashSet = new KnownType("System.Collections.Immutable.ImmutableHashSet");
        public static readonly KnownType System_Collections_Immutable_ImmutableHashSet_T = new KnownType("System.Collections.Immutable.ImmutableHashSet<T>");
        public static readonly KnownType System_Collections_Immutable_IImmutableSet_T = new KnownType("System.Collections.Immutable.IImmutableSet<T>");
        public static readonly KnownType System_Collections_Immutable_ImmutableList = new KnownType("System.Collections.Immutable.ImmutableList");
        public static readonly KnownType System_Collections_Immutable_ImmutableList_T = new KnownType("System.Collections.Immutable.ImmutableList<T>");
        public static readonly KnownType System_Collections_Immutable_IImmutableList_T = new KnownType("System.Collections.Immutable.IImmutableList<T>");
        public static readonly KnownType System_Collections_Immutable_ImmutableQueue = new KnownType("System.Collections.Immutable.ImmutableQueue");
        public static readonly KnownType System_Collections_Immutable_ImmutableQueue_T = new KnownType("System.Collections.Immutable.ImmutableQueue<T>");
        public static readonly KnownType System_Collections_Immutable_IImmutableQueue_T = new KnownType("System.Collections.Immutable.IImmutableQueue<T>");
        public static readonly KnownType System_Collections_Immutable_ImmutableSortedDictionary = new KnownType("System.Collections.Immutable.ImmutableSortedDictionary");
        public static readonly KnownType System_Collections_Immutable_ImmutableSortedDictionary_TKey_TValue = new KnownType("System.Collections.Immutable.ImmutableSortedDictionary<TKey, TValue>");
        public static readonly KnownType System_Collections_Immutable_ImmutableSortedSet = new KnownType("System.Collections.Immutable.ImmutableSortedSet");
        public static readonly KnownType System_Collections_Immutable_ImmutableSortedSet_T = new KnownType("System.Collections.Immutable.ImmutableSortedSet<T>");
        public static readonly KnownType System_Collections_Immutable_ImmutableStack = new KnownType("System.Collections.Immutable.ImmutableStack");
        public static readonly KnownType System_Collections_Immutable_ImmutableStack_T = new KnownType("System.Collections.Immutable.ImmutableStack<T>");
        public static readonly KnownType System_Collections_Immutable_IImmutableStack_T = new KnownType("System.Collections.Immutable.IImmutableStack<T>");

        public static readonly KnownType System_Collections_ObjectModel_ReadOnlyCollection_T = new KnownType("System.Collections.ObjectModel.ReadOnlyCollection<T>");
        public static readonly KnownType System_Collections_ObjectModel_ReadOnlyDictionary_TKey_TValue = new KnownType("System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>");

        public static readonly KnownType System_Diagnostics_Contracts_PureAttribute = new KnownType("System.Diagnostics.Contracts.PureAttribute");

        public static readonly KnownType System_Runtime_InteropServices_ComImportAttribute = new KnownType("System.Runtime.InteropServices.ComImportAttribute");
        public static readonly KnownType System_Runtime_InteropServices_InterfaceTypeAttribute = new KnownType("System.Runtime.InteropServices.InterfaceTypeAttribute");

        public static readonly KnownType System_Threading_Tasks_Task = new KnownType("System.Threading.Tasks.Task");

        public static readonly KnownType System_Runtime_CompilerServices_CallerMemberNameAttribute = new KnownType("System.Runtime.CompilerServices.CallerMemberNameAttribute");
        public static readonly KnownType System_Runtime_CompilerServices_CallerFilePathAttribute = new KnownType("System.Runtime.CompilerServices.CallerFilePathAttribute");
        public static readonly KnownType System_Runtime_CompilerServices_CallerLineNumberAttribute = new KnownType("System.Runtime.CompilerServices.CallerLineNumberAttribute");
        public static readonly KnownType System_Runtime_Serialization_OptionalFieldAttribute = new KnownType("System.Runtime.Serialization.OptionalFieldAttribute");
        public static readonly KnownType System_Runtime_Serialization_OnDeserializingAttribute = new KnownType("System.Runtime.Serialization.OnDeserializingAttribute");
        public static readonly KnownType System_Runtime_Serialization_OnDeserializedAttribute = new KnownType("System.Runtime.Serialization.OnDeserializedAttribute");

        public static readonly ISet<KnownType> CallerInfoAttributes = ImmutableHashSet.Create(
            System_Runtime_CompilerServices_CallerFilePathAttribute,
            System_Runtime_CompilerServices_CallerLineNumberAttribute,
            System_Runtime_CompilerServices_CallerMemberNameAttribute);

        public static readonly KnownType System_ServiceModel_ServiceContractAttribute = new KnownType("System.ServiceModel.ServiceContractAttribute");
        public static readonly KnownType System_ServiceModel_OperationContractAttribute = new KnownType("System.ServiceModel.OperationContractAttribute");

        public static readonly KnownType System_Linq_Enumerable = new KnownType("System.Linq.Enumerable");
        public static readonly KnownType System_Linq_ImmutableArrayExtensions = new KnownType("System.Linq.ImmutableArrayExtensions");

        public static readonly KnownType System_Runtime_Serialization_ISerializable = new KnownType("System.Runtime.Serialization.ISerializable");
        public static readonly KnownType System_Runtime_Serialization_SerializationInfo = new KnownType("System.Runtime.Serialization.SerializationInfo");
        public static readonly KnownType System_Runtime_Serialization_StreamingContext = new KnownType("System.Runtime.Serialization.StreamingContext");

        public static readonly KnownType System_Diagnostics_CodeAnalysis_SuppressMessageAttribute = new KnownType("System.Diagnostics.CodeAnalysis.SuppressMessageAttribute");

        public static readonly KnownType System_ApplicationException = new KnownType("System.ApplicationException");
        public static readonly KnownType System_SystemException = new KnownType("System.SystemException");
        public static readonly KnownType System_ExecutionEngineException = new KnownType("System.ExecutionEngineException");
        public static readonly KnownType System_IndexOutOfRangeException = new KnownType("System.IndexOutOfRangeException");
        public static readonly KnownType System_NullReferenceException = new KnownType("System.NullReferenceException");
        public static readonly KnownType System_OutOfMemoryException = new KnownType("System.OutOfMemoryException");
        public static readonly KnownType System_NotImplementedException = new KnownType("System.NotImplementedException");
        public static readonly KnownType System_InvalidOperationException = new KnownType("System.InvalidOperationException");
        public static readonly KnownType System_NotSupportedException = new KnownType("System.NotSupportedException");
        public static readonly KnownType System_ArgumentException = new KnownType("System.ArgumentException");
        public static readonly KnownType System_ArgumentNullException = new KnownType("System.ArgumentNullException");
        public static readonly KnownType System_ArgumentOutOfRangeException = new KnownType("System.ArgumentOutOfRangeException");
        public static readonly KnownType System_DuplicateWaitObjectException = new KnownType("System.DuplicateWaitObjectException");


        public static readonly KnownType System_IComparable = new KnownType("System.IComparable");
        public static readonly KnownType System_IComparable_T = new KnownType("System.IComparable<T>");
        public static readonly KnownType System_IEquatable_T = new KnownType("System.IEquatable<T>");

        public static readonly KnownType System_Threading_Thread = new KnownType("System.Threading.Thread");

        public static readonly KnownType System_Text_StringBuilder = new KnownType("System.Text.StringBuilder");
        public static readonly KnownType System_IO_TextWriter = new KnownType("System.IO.TextWriter");
        public static readonly KnownType System_Diagnostics_Debug = new KnownType("System.Diagnostics.Debug");
        public static readonly KnownType System_Diagnostics_Trace = new KnownType("System.Diagnostics.Trace");
        public static readonly KnownType System_Diagnostics_TraceSource = new KnownType("System.Diagnostics.TraceSource");

        public static readonly KnownType System_Runtime_CompilerServices_InternalsVisibleToAttribute = new KnownType("System.Runtime.CompilerServices.InternalsVisibleToAttribute");

        #endregion

        public string TypeName { get; }
        private readonly SpecialType specialType;
        private readonly bool isSpecialType;

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

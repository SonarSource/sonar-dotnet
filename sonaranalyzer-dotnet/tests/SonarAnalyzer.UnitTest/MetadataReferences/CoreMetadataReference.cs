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

#if !NETFRAMEWORK

using Microsoft.CodeAnalysis;
using static SonarAnalyzer.UnitTest.MetadataReferences.MetadataReferenceFactory;

namespace SonarAnalyzer.UnitTest.MetadataReferences
{
    internal static class CoreMetadataReference
    {
        internal static MetadataReference MsCorLib { get; } = CreateReference("mscorlib.dll");
        internal static MetadataReference MicrosoftVisualBasic { get; } = CreateReference("Microsoft.VisualBasic.dll");
        internal static MetadataReference MicrosoftVisualBasicCore { get; } = CreateReference("Microsoft.VisualBasic.Core.dll");
        internal static MetadataReference MicrosoftWin32Registry { get; } = CreateReference("Microsoft.Win32.Registry.dll");
        internal static MetadataReference MicrosoftWin32Primitives { get; } = CreateReference("Microsoft.Win32.Primitives.dll");
        internal static MetadataReference System { get; } = CreateReference("System.dll");
        internal static MetadataReference SystemCollections { get; } = CreateReference("System.Collections.dll");
        internal static MetadataReference SystemCore { get; } = CreateReference("System.Core.dll");
        internal static MetadataReference SystemCollectionsImmutable { get; } = CreateReference("System.Collections.Immutable.dll");
        internal static MetadataReference SystemCollectionsSpecialized { get; } = CreateReference("System.Collections.Specialized.dll");
        internal static MetadataReference SystemCollectionsNonGeneric { get; } = CreateReference("System.Collections.NonGeneric.dll");
        internal static MetadataReference SystemConsole { get; } = CreateReference("System.Console.dll");
        internal static MetadataReference SystemComponentModelPrimitives { get; } = CreateReference("System.ComponentModel.Primitives.dll");
        internal static MetadataReference SystemDataCommon { get; } = CreateReference("System.Data.Common.dll");
        internal static MetadataReference SystemDiagnosticsTraceSource { get; } = CreateReference("System.Diagnostics.TraceSource.dll");
        internal static MetadataReference SystemDiagnosticsTools { get; } = CreateReference("System.Diagnostics.Tools.dll");
        internal static MetadataReference SystemGlobalization { get; } = CreateReference("System.Globalization.dll");
        internal static MetadataReference SystemIoCompression { get; } = CreateReference("System.IO.Compression.dll");
        internal static MetadataReference SystemIoCompressionZipFile { get; } = CreateReference("System.IO.Compression.ZipFile.dll");
        internal static MetadataReference SystemIoFileSystem { get; } = CreateReference("System.IO.FileSystem.dll");
        internal static MetadataReference SystemLinq { get; } = CreateReference("System.Linq.dll");
        internal static MetadataReference SystemLinqExpressions { get; } = CreateReference("System.Linq.Expressions.dll");
        internal static MetadataReference SystemLinqQueryable { get; } = CreateReference("System.Linq.Queryable.dll");
        internal static MetadataReference SystemNetHttp { get; } = CreateReference("System.Net.Http.dll");
        internal static MetadataReference SystemNetRequests { get; } = CreateReference("System.Net.Requests.dll");
        internal static MetadataReference SystemNetSecurity { get; } = CreateReference("System.Net.Security.dll");
        internal static MetadataReference SystemNetServicePoint { get; } = CreateReference("System.Net.ServicePoint.dll");
        internal static MetadataReference SystemNetSockets { get; } = CreateReference("System.Net.Sockets.dll");
        internal static MetadataReference SystemNetPrimitives { get; } = CreateReference("System.Net.Primitives.dll");
        internal static MetadataReference SystemNetWebClient { get; } = CreateReference("System.Net.WebClient.dll");
        internal static MetadataReference SystemObjectModel { get; } = CreateReference("System.ObjectModel.dll");
        internal static MetadataReference SystemPrivateCoreLib { get; } = CreateReference("System.Private.CoreLib.dll");
        internal static MetadataReference SystemPrivateUri { get; } = CreateReference("System.Private.Uri.dll");
        internal static MetadataReference SystemPrivateXml { get; } = CreateReference("System.Private.Xml.dll");
        internal static MetadataReference SystemPrivateXmlLinq { get; } = CreateReference("System.Private.Xml.Linq.dll");
        internal static MetadataReference SystemRuntime { get; } = CreateReference("System.Runtime.dll");
        internal static MetadataReference SystemRuntimeExtensions { get; } = CreateReference("System.Runtime.Extensions.dll");
        internal static MetadataReference SystemRuntimeInteropServices { get; } = CreateReference("System.Runtime.InteropServices.dll");
        internal static MetadataReference SystemRuntimeSerialization { get; } = CreateReference("System.Runtime.Serialization.dll");
        internal static MetadataReference SystemRuntimeSerializationPrimitives { get; } = CreateReference("System.Runtime.Serialization.Primitives.dll");
        internal static MetadataReference SystemSecurityCryptographyAlgorithms { get; } = CreateReference("System.Security.Cryptography.Algorithms.dll");
        internal static MetadataReference SystemSecurityCryptographyX509Certificates { get; } = CreateReference("System.Security.Cryptography.X509Certificates.dll");
        internal static MetadataReference SystemSecurityCryptographyCsp { get; } = CreateReference("System.Security.Cryptography.Csp.dll");
        internal static MetadataReference SystemSecurityCryptographyCng { get; } = CreateReference("System.Security.Cryptography.Cng.dll");
        internal static MetadataReference SystemSecurityCryptographyPrimitives { get; } = CreateReference("System.Security.Cryptography.Primitives.dll");
        internal static MetadataReference SystemThreadingTasks { get; } = CreateReference("System.Threading.Tasks.dll");
        internal static MetadataReference SystemXmlReaderWriter { get; } = CreateReference("System.Xml.ReaderWriter.dll");
        internal static MetadataReference SystemXml { get; } = CreateReference("System.Xml.dll");
        internal static MetadataReference SystemXmlXDocument { get; } = CreateReference("System.Xml.XDocument.dll");
        internal static MetadataReference SystemXmlLinq { get; } = CreateReference("System.Xml.Linq.dll");
        internal static MetadataReference SystemWeb { get; } = CreateReference("System.Web.dll");
        internal static MetadataReference SystemWebHttpUtility { get; } = CreateReference("System.Web.HttpUtility.dll");
    }
}
#endif

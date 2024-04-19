/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

#if NET

using static SonarAnalyzer.TestFramework.MetadataReferences.MetadataReferenceFactory;

namespace SonarAnalyzer.TestFramework.MetadataReferences;

public static class CoreMetadataReference
{
    public static MetadataReference MicrosoftVisualBasic { get; } = CreateReference("Microsoft.VisualBasic.dll");
    public static MetadataReference MicrosoftVisualBasicCore { get; } = CreateReference("Microsoft.VisualBasic.Core.dll");
    public static MetadataReference MicrosoftWin32Registry { get; } = CreateReference("Microsoft.Win32.Registry.dll");
    public static MetadataReference MicrosoftWin32Primitives { get; } = CreateReference("Microsoft.Win32.Primitives.dll");
    public static MetadataReference MsCorLib { get; } = CreateReference("mscorlib.dll");
    public static MetadataReference System { get; } = CreateReference("System.dll");
    public static MetadataReference SystemCollections { get; } = CreateReference("System.Collections.dll");
    public static MetadataReference SystemCore { get; } = CreateReference("System.Core.dll");
    public static MetadataReference SystemCollectionsConcurrent { get; } = CreateReference("System.Collections.Concurrent.dll");
    public static MetadataReference SystemCollectionsImmutable { get; } = CreateReference("System.Collections.Immutable.dll");
    public static MetadataReference SystemCollectionsSpecialized { get; } = CreateReference("System.Collections.Specialized.dll");
    public static MetadataReference SystemCollectionsNonGeneric { get; } = CreateReference("System.Collections.NonGeneric.dll");
    public static MetadataReference SystemComponentModel { get; } = CreateReference("System.ComponentModel.dll");
    public static MetadataReference SystemComponentModelPrimitives { get; } = CreateReference("System.ComponentModel.Primitives.dll");
    public static MetadataReference SystemConsole { get; } = CreateReference("System.Console.dll");
    public static MetadataReference SystemDataCommon { get; } = CreateReference("System.Data.Common.dll");
    public static MetadataReference SystemDiagnosticsTraceSource { get; } = CreateReference("System.Diagnostics.TraceSource.dll");
    public static MetadataReference SystemDiagnosticsProcess { get; } = CreateReference("System.Diagnostics.Process.dll");
    public static MetadataReference SystemDiagnosticsTools { get; } = CreateReference("System.Diagnostics.Tools.dll");
    public static MetadataReference SystemGlobalization { get; } = CreateReference("System.Globalization.dll");
    public static MetadataReference SystemIoCompression { get; } = CreateReference("System.IO.Compression.dll");
    public static MetadataReference SystemIoCompressionZipFile { get; } = CreateReference("System.IO.Compression.ZipFile.dll");
    public static MetadataReference SystemIoFileSystem { get; } = CreateReference("System.IO.FileSystem.dll");
    public static MetadataReference SystemIoFileSystemAccessControl { get; } = CreateReference("System.IO.FileSystem.AccessControl.dll");
    public static MetadataReference SystemLinq { get; } = CreateReference("System.Linq.dll");
    public static MetadataReference SystemLinqExpressions { get; } = CreateReference("System.Linq.Expressions.dll");
    public static MetadataReference SystemLinqQueryable { get; } = CreateReference("System.Linq.Queryable.dll");
    public static MetadataReference SystemNetHttp { get; } = CreateReference("System.Net.Http.dll");
    public static MetadataReference SystemNetMail { get; } = CreateReference("System.Net.Mail.dll");
    public static MetadataReference SystemNetRequests { get; } = CreateReference("System.Net.Requests.dll");
    public static MetadataReference SystemNetSecurity { get; } = CreateReference("System.Net.Security.dll");
    public static MetadataReference SystemNetServicePoint { get; } = CreateReference("System.Net.ServicePoint.dll");
    public static MetadataReference SystemNetSockets { get; } = CreateReference("System.Net.Sockets.dll");
    public static MetadataReference SystemNetPrimitives { get; } = CreateReference("System.Net.Primitives.dll");
    public static MetadataReference SystemNetWebClient { get; } = CreateReference("System.Net.WebClient.dll");
    public static MetadataReference SystemObjectModel { get; } = CreateReference("System.ObjectModel.dll");
    public static MetadataReference SystemPrivateCoreLib { get; } = CreateReference("System.Private.CoreLib.dll");
    public static MetadataReference SystemPrivateUri { get; } = CreateReference("System.Private.Uri.dll");
    public static MetadataReference SystemPrivateXml { get; } = CreateReference("System.Private.Xml.dll");
    public static MetadataReference SystemPrivateXmlLinq { get; } = CreateReference("System.Private.Xml.Linq.dll");
    public static MetadataReference SystemRuntime { get; } = CreateReference("System.Runtime.dll");
    public static MetadataReference SystemRuntimeExtensions { get; } = CreateReference("System.Runtime.Extensions.dll");
    public static MetadataReference SystemRuntimeInteropServices { get; } = CreateReference("System.Runtime.InteropServices.dll");
    public static MetadataReference SystemRuntimeSerialization { get; } = CreateReference("System.Runtime.Serialization.dll");
    public static MetadataReference SystemRuntimeSerializationFormatters { get; } = CreateReference("System.Runtime.Serialization.Formatters.dll");
    public static MetadataReference SystemRuntimeSerializationPrimitives { get; } = CreateReference("System.Runtime.Serialization.Primitives.dll");
    public static MetadataReference SystemSecurityAccessControl { get; } = CreateReference("System.Security.AccessControl.dll");
    public static MetadataReference SystemSecurityClaims { get; } = CreateReference("System.Security.Claims.dll");
    public static MetadataReference SystemSecurityCryptography { get; } = CreateReference("System.Security.Cryptography.dll");
    public static MetadataReference SystemSecurityCryptographyX509Certificates { get; } = CreateReference("System.Security.Cryptography.X509Certificates.dll");
    public static MetadataReference SystemSecurityCryptographyCsp { get; } = CreateReference("System.Security.Cryptography.Csp.dll");
    public static MetadataReference SystemSecurityCryptographyCng { get; } = CreateReference("System.Security.Cryptography.Cng.dll");
    public static MetadataReference SystemSecurityCryptographyPrimitives { get; } = CreateReference("System.Security.Cryptography.Primitives.dll");
    public static MetadataReference SystemSecurityPrincipalWindows { get; } = CreateReference("System.Security.Principal.Windows.dll");
    public static MetadataReference SystemThreading { get; } = CreateReference("System.Threading.dll");
    public static MetadataReference SystemThreadingTasks { get; } = CreateReference("System.Threading.Tasks.dll");
    public static MetadataReference SystemXmlReaderWriter { get; } = CreateReference("System.Xml.ReaderWriter.dll");
    public static MetadataReference SystemXml { get; } = CreateReference("System.Xml.dll");
    public static MetadataReference SystemXmlXDocument { get; } = CreateReference("System.Xml.XDocument.dll");
    public static MetadataReference SystemXmlLinq { get; } = CreateReference("System.Xml.Linq.dll");
    public static MetadataReference SystemWeb { get; } = CreateReference("System.Web.dll");
}
#endif

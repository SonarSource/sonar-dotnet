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
using Microsoft.CodeAnalysis;
using References = System.Collections.Generic.IEnumerable<Microsoft.CodeAnalysis.MetadataReference>;

namespace SonarAnalyzer.UnitTest.MetadataReferences
{
    internal static class MetadataReferenceFacade
    {
        internal static References MsCorLib =>
#if NETFRAMEWORK
            FrameworkMetadataReference.Mscorlib;
#else
            new[] {CoreMetadataReference.MsCorLib};
#endif

        internal static References MicrosoftVisualBasic =>
#if NETFRAMEWORK
            FrameworkMetadataReference.MicrosoftVisualBasic;
#else
            new[]
            {
                CoreMetadataReference.MicrosoftVisualBasic,
                CoreMetadataReference.MicrosoftVisualBasicCore
            };
#endif

        internal static References MicrosoftWin32Registry =>
#if NETFRAMEWORK
            Enumerable.Empty<MetadataReference>();
#else
            new[] {CoreMetadataReference.MicrosoftWin32Registry};
#endif

        internal static References MicrosoftWin32Primitives =>
#if NETFRAMEWORK
            Enumerable.Empty<MetadataReference>();
#else
            new[] {CoreMetadataReference.MicrosoftWin32Primitives};
#endif

        internal static References PresentationFramework =>
#if NETFRAMEWORK
            FrameworkMetadataReference.PresentationFramework;
#else
            Enumerable.Empty<MetadataReference>();
#endif

        internal static References SystemCollections =>
#if NETFRAMEWORK
            Enumerable.Empty<MetadataReference>();
#else
            new[]
            {
                CoreMetadataReference.SystemCollections,
                CoreMetadataReference.SystemCollectionsImmutable,
                CoreMetadataReference.SystemCollectionsNonGeneric
            };
#endif

        internal static References SystemData =>
#if NETFRAMEWORK
            FrameworkMetadataReference.SystemData;
#else
            new[]
            {
                CoreMetadataReference.SystemDataCommon
            };
#endif

        internal static References SystemDiagnosticsProcess =>
#if NETFRAMEWORK
            Enumerable.Empty<MetadataReference>();
#else
            new[]
            {
                CoreMetadataReference.SystemComponentModelPrimitives,   // Type "Process" needs this for it's parent type "Component"
                CoreMetadataReference.SystemDiagnosticsProcess
            };
#endif

        internal static References SystemDrawing =>
#if NETFRAMEWORK
            FrameworkMetadataReference.SystemDrawing;
#else
            NuGetMetadataReference.SystemDrawingCommon();
#endif

        internal static References SystemDirectoryServices =>
#if NETFRAMEWORK
            FrameworkMetadataReference.SystemDirectoryServices;
#else
            NuGetMetadataReference.SystemDDirectoryServices();
#endif

        internal static References SystemIoCompression =>
#if NETFRAMEWORK
            FrameworkMetadataReference.SystemIOCompression
                .Concat(FrameworkMetadataReference.SystemIOCompressionFileSystem);
#else
            new[]
            {
                CoreMetadataReference.SystemIoCompression,
                CoreMetadataReference.SystemIoCompressionZipFile
            };
#endif

        internal static References SystemServiceModel =>
#if NETFRAMEWORK
            FrameworkMetadataReference.SystemServiceModel;
#else
            NuGetMetadataReference.SystemPrivateServiceModel()
                .Concat(NuGetMetadataReference.SystemServiceModelPrimitives());
#endif

        internal static References SystemNetHttp =>
#if NETFRAMEWORK
            FrameworkMetadataReference.SystemNetHttp;
#else
            new[]
            {
                CoreMetadataReference.SystemNetHttp,
                CoreMetadataReference.SystemNetMail,
                CoreMetadataReference.SystemNetRequests,
                CoreMetadataReference.SystemNetSecurity,
                CoreMetadataReference.SystemNetServicePoint,
                CoreMetadataReference.SystemNetSockets,
                CoreMetadataReference.SystemNetPrimitives,
                CoreMetadataReference.SystemNetWebClient
            };
#endif

        internal static References SystemSecurityCryptography =>
#if NETFRAMEWORK
            FrameworkMetadataReference.SystemSecurityCryptographyAlgorithms;
#else
            new[]
            {
                CoreMetadataReference.SystemSecurityCryptographyAlgorithms,
                CoreMetadataReference.SystemSecurityCryptographyX509Certificates,
                CoreMetadataReference.SystemSecurityCryptographyCsp,
                CoreMetadataReference.SystemSecurityCryptographyCng,
                CoreMetadataReference.SystemSecurityCryptographyPrimitives
            };
#endif

        internal static References SystemSecurityPermissions =>
#if NETFRAMEWORK
            Enumerable.Empty<MetadataReference>();
#else
            NuGetMetadataReference.SystemSecurityPermissions();
#endif

        internal static References SystemThreadingTasks =>
#if NETFRAMEWORK
            FrameworkMetadataReference.SystemThreadingTasks
                .Concat(NuGetMetadataReference.SystemThreadingTasksExtensions("4.0.0"));
#else
            new[] {CoreMetadataReference.SystemThreadingTasks};
#endif

        internal static References SystemThreadingTasksExtensions(string version) =>
#if NETFRAMEWORK
            NuGetMetadataReference.SystemThreadingTasksExtensions(version);
#else
            new[] {CoreMetadataReference.SystemThreadingTasks};
#endif

        internal static References RegularExpressions =>
#if NETFRAMEWORK
            Enumerable.Empty<MetadataReference>();
#else
            NuGetMetadataReference.SystemTextRegularExpressions();
#endif

        internal static References SystemRuntimeSerialization =>
#if NETFRAMEWORK
            FrameworkMetadataReference.SystemRuntimeSerialization;
#else
            new[]
            {
                CoreMetadataReference.SystemRuntimeSerialization,
                CoreMetadataReference.SystemRuntimeSerializationPrimitives
            };
#endif

        internal static References SystemXaml =>
#if NETFRAMEWORK
            FrameworkMetadataReference.SystemXaml;
#else
            new[]
            {
                CoreMetadataReference.SystemXmlReaderWriter,
                CoreMetadataReference.SystemPrivateXml
            };
#endif

        internal static References SystemXml =>
#if NETFRAMEWORK
            FrameworkMetadataReference.SystemXml;
#else
            new[]
            {
                CoreMetadataReference.SystemPrivateXml,
                CoreMetadataReference.SystemPrivateXmlLinq,
                CoreMetadataReference.SystemXml,
                CoreMetadataReference.SystemXmlXDocument,
                CoreMetadataReference.SystemXmlReaderWriter,
            }
            .Union(NuGetMetadataReference.SystemConfigurationConfigurationManager());
#endif

        internal static References SystemXmlLinq =>
#if NETFRAMEWORK
            FrameworkMetadataReference.SystemXmlLinq;
#else
            new[] {CoreMetadataReference.SystemXmlLinq};
#endif

        internal static References SystemWeb =>
#if NETFRAMEWORK
            FrameworkMetadataReference.SystemWeb;
#else
            new[] {CoreMetadataReference.SystemWeb};
#endif

        internal static References SystemWindowsForms =>
#if NETFRAMEWORK
            FrameworkMetadataReference.SystemWindowsForms;
#else
            WindowsFormsMetadataReference.SystemWindowsForms;
#endif

        internal static References SystemComponentModelComposition =>
#if NETFRAMEWORK
            FrameworkMetadataReference.SystemComponentModelComposition;
#else
            NuGetMetadataReference.SystemComponentModelComposition();
#endif

        internal static References SystemComponentModelPrimitives =>
#if NETFRAMEWORK
            Enumerable.Empty<MetadataReference>();
#else
            new[] {CoreMetadataReference.SystemComponentModelPrimitives};
#endif

        internal static References SystemNetSockets =>
#if NETFRAMEWORK
            Enumerable.Empty<MetadataReference>();
#else
            new[] {CoreMetadataReference.SystemNetSockets};
#endif
        internal static References SystemNetPrimitives =>
#if NETFRAMEWORK
            Enumerable.Empty<MetadataReference>();
#else
            new[] {CoreMetadataReference.SystemNetPrimitives};
#endif

        internal static References WindowsBase =>
#if NETFRAMEWORK
            FrameworkMetadataReference.WindowsBase;
#else
            Enumerable.Empty<MetadataReference>();
#endif

        internal static References ProjectDefaultReferences =>
#if NETFRAMEWORK
            FrameworkMetadataReference.Mscorlib
                .Concat(FrameworkMetadataReference.System)
                .Concat(FrameworkMetadataReference.SystemCore)
                .Concat(FrameworkMetadataReference.SystemRuntime)
                .Concat(FrameworkMetadataReference.SystemGlobalization);
#else
            new[]
                {
                    CoreMetadataReference.MsCorLib,
                    CoreMetadataReference.System,
                    CoreMetadataReference.SystemCollections,
                    CoreMetadataReference.SystemCollectionsSpecialized,
                    CoreMetadataReference.SystemConsole,
                    CoreMetadataReference.SystemCore,
                    CoreMetadataReference.SystemDiagnosticsTools,
                    CoreMetadataReference.SystemDiagnosticsTraceSource,
                    CoreMetadataReference.SystemGlobalization,
                    CoreMetadataReference.SystemIoFileSystem,
                    CoreMetadataReference.SystemIoFileSystemAccessControl,
                    CoreMetadataReference.SystemLinq,
                    CoreMetadataReference.SystemLinqExpressions,
                    CoreMetadataReference.SystemLinqQueryable,
                    CoreMetadataReference.SystemObjectModel,
                    CoreMetadataReference.SystemPrivateCoreLib,
                    CoreMetadataReference.SystemPrivateUri,
                    CoreMetadataReference.SystemRuntime,
                    CoreMetadataReference.SystemRuntimeExtensions,
                    CoreMetadataReference.SystemRuntimeInteropServices,
                    CoreMetadataReference.SystemSecurityAccessControl,
                    CoreMetadataReference.SystemSecurityPrincipalWindows
                }
                .Concat(NetStandardMetadataReference.Netstandard);
#endif
    }
}

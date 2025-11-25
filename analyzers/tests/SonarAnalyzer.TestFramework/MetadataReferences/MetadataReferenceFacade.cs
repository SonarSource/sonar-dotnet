/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using References = System.Collections.Generic.IEnumerable<Microsoft.CodeAnalysis.MetadataReference>;

namespace SonarAnalyzer.TestFramework.MetadataReferences;

#pragma warning disable T0030 // Move this expression to the previous line

public static class MetadataReferenceFacade
{
    public static References NetStandard { get; } = MetadataReferenceFactory.Create("netstandard.dll");

    public static References MsCorLib =>
#if NETFRAMEWORK
        FrameworkMetadataReference.Mscorlib;
#else
        [CoreMetadataReference.MsCorLib];
#endif

    public static References MicrosoftExtensionsDependencyInjectionAbstractions =>
#if NETFRAMEWORK
        NuGetMetadataReference.MicrosoftExtensionsDependencyInjectionAbstractions(TestConstants.DotNetCore220Version);
#else
        [AspNetCoreMetadataReference.MicrosoftExtensionsDependencyInjectionAbstractions];
#endif

    public static References MicrosoftVisualBasic =>
#if NETFRAMEWORK
        FrameworkMetadataReference.MicrosoftVisualBasic;
#else
        [
            CoreMetadataReference.MicrosoftVisualBasic,
            CoreMetadataReference.MicrosoftVisualBasicCore
        ];
#endif

    public static References MicrosoftWin32Registry =>
#if NETFRAMEWORK
        [];
#else
        [CoreMetadataReference.MicrosoftWin32Registry];
#endif

    public static References MicrosoftWin32Primitives =>
#if NETFRAMEWORK
        [];
#else
        [CoreMetadataReference.MicrosoftWin32Primitives];
#endif

    public static References NetStandard21 =>
#if NETFRAMEWORK
        NuGetMetadataFactory.Create("NETStandard.Library.Ref", "2.1.0", "netstandard2.1");
#else
        [];
#endif

    public static References PresentationFramework =>
#if NETFRAMEWORK
        FrameworkMetadataReference.PresentationFramework;
#else
        [WindowsDesktopMetadataReference.PresentationCore,
            WindowsDesktopMetadataReference.PresentationFramework,
            WindowsDesktopMetadataReference.WindowsBase];
#endif

    public static References SystemCollections =>
#if NETFRAMEWORK
        NuGetMetadataReference.SystemCollectionsImmutable(TestConstants.NuGetLatestVersion);
#else
        [
            CoreMetadataReference.SystemCollections,
            CoreMetadataReference.SystemCollectionsImmutable,
            CoreMetadataReference.SystemCollectionsNonGeneric,
            CoreMetadataReference.SystemCollectionsConcurrent
        ];
#endif

    public static References SystemData =>
#if NETFRAMEWORK
        FrameworkMetadataReference.SystemData;
#else
        [
            CoreMetadataReference.SystemDataCommon
        ];
#endif

    public static References SystemDiagnosticsProcess =>
#if NETFRAMEWORK
        [];
#else
        [
            CoreMetadataReference.SystemComponentModelPrimitives,   // Type "Process" needs this for it's parent type "Component"
            CoreMetadataReference.SystemDiagnosticsProcess
        ];
#endif

    public static References SystemDrawing =>
#if NETFRAMEWORK
        FrameworkMetadataReference.SystemDrawing;
#else
        NuGetMetadataReference.SystemDrawingCommon();
#endif

    public static References SystemDirectoryServices =>
#if NETFRAMEWORK
        FrameworkMetadataReference.SystemDirectoryServices;
#else
        NuGetMetadataReference.SystemDDirectoryServices();
#endif

    public static References SystemIoCompression =>
#if NETFRAMEWORK
        FrameworkMetadataReference.SystemIOCompression
            .Concat(FrameworkMetadataReference.SystemIOCompressionFileSystem);
#else
        [
            CoreMetadataReference.SystemIoCompression,
            CoreMetadataReference.SystemIoCompressionZipFile
        ];
#endif

    public static References SystemMemory =>
#if NETFRAMEWORK
        NuGetMetadataReference.SystemMemory();
#else
        [];
#endif

    public static References SystemServiceModel =>
#if NETFRAMEWORK
        FrameworkMetadataReference.SystemServiceModel;
#else
        NuGetMetadataReference.SystemPrivateServiceModel()
            .Concat(NuGetMetadataReference.SystemServiceModelPrimitives());
#endif

    public static References SystemNetHttp =>
#if NETFRAMEWORK
        FrameworkMetadataReference.SystemNetHttp;
#else
        [
            CoreMetadataReference.SystemNetHttp,
            CoreMetadataReference.SystemNetMail,
            CoreMetadataReference.SystemNetRequests,
            CoreMetadataReference.SystemNetSecurity,
            CoreMetadataReference.SystemNetServicePoint,
            CoreMetadataReference.SystemNetSockets,
            CoreMetadataReference.SystemNetPrimitives,
            CoreMetadataReference.SystemNetWebClient
        ];
#endif

    public static References SystemSecurityCryptography =>
#if NETFRAMEWORK
        FrameworkMetadataReference.SystemSecurityCryptographyAlgorithms;
#else
        [
            CoreMetadataReference.SystemSecurityCryptography,
            CoreMetadataReference.SystemSecurityCryptographyX509Certificates,
            CoreMetadataReference.SystemSecurityCryptographyCsp,
            CoreMetadataReference.SystemSecurityCryptographyCng,
            CoreMetadataReference.SystemSecurityCryptographyPrimitives
        ];
#endif

    public static References SystemSecurityPermissions =>
#if NETFRAMEWORK
        [];
#else
        NuGetMetadataReference.SystemSecurityPermissions();
#endif

    public static References SystemThreading =>
#if NETFRAMEWORK
        [];
#else
        [CoreMetadataReference.SystemThreading];
#endif

    public static References SystemThreadingTasks =>
#if NETFRAMEWORK
        FrameworkMetadataReference.SystemThreadingTasks
            .Concat(NuGetMetadataReference.SystemThreadingTasksExtensions("4.0.0"));
#else
        [CoreMetadataReference.SystemThreadingTasks, CoreMetadataReference.SystemThreadingTasksParallel];
#endif

    public static References RegularExpressions =>
#if NETFRAMEWORK
        [];
#else
        [CoreMetadataReference.SystemTextRegularExpressions];
#endif

    public static References SystemRuntimeSerialization =>
#if NETFRAMEWORK
        FrameworkMetadataReference.SystemRuntimeSerialization;
#else
        [
            CoreMetadataReference.SystemRuntimeSerialization,
            CoreMetadataReference.SystemRuntimeSerializationPrimitives
        ];
#endif

    public static References SystemXaml =>
#if NETFRAMEWORK
        FrameworkMetadataReference.SystemXaml;
#else
        [
            CoreMetadataReference.SystemXmlReaderWriter,
            CoreMetadataReference.SystemPrivateXml
        ];
#endif

    public static References SystemXml =>
#if NETFRAMEWORK
        FrameworkMetadataReference.SystemXml;
#else
        NuGetMetadataReference.SystemConfigurationConfigurationManager()
            .Union([
                CoreMetadataReference.SystemPrivateXml,
                CoreMetadataReference.SystemPrivateXmlLinq,
                CoreMetadataReference.SystemXml,
                CoreMetadataReference.SystemXmlXDocument,
                CoreMetadataReference.SystemXmlReaderWriter]);
#endif

    public static References SystemXmlLinq =>
#if NETFRAMEWORK
        FrameworkMetadataReference.SystemXmlLinq;
#else
        [CoreMetadataReference.SystemXmlLinq];
#endif

    public static References SystemWeb =>
#if NETFRAMEWORK
        FrameworkMetadataReference.SystemWeb;
#else
        [CoreMetadataReference.SystemWeb];
#endif

    public static References SystemWindowsForms =>
#if NETFRAMEWORK
        FrameworkMetadataReference.SystemWindowsForms;
#else
        [WindowsDesktopMetadataReference.SystemWindowsForms];
#endif

    public static References SystemComponentModelComposition =>
#if NETFRAMEWORK
        FrameworkMetadataReference.SystemComponentModelComposition;
#else
        NuGetMetadataReference.SystemComponentModelComposition();
#endif

    public static References SystemCompositionAttributedModel =>
        NuGetMetadataReference.SystemCompositionAttributedModel();

    public static References SystemComponentModelPrimitives =>
#if NETFRAMEWORK
        [];
#else
        [CoreMetadataReference.SystemComponentModelPrimitives];
#endif

    public static References SystemComponentModelTypeConverter =>
#if NETFRAMEWORK
        [];
#else
        [CoreMetadataReference.SystemComponentModelTypeConverter];
#endif

    public static References SystemNetSockets =>
#if NETFRAMEWORK
        [];
#else
        [CoreMetadataReference.SystemNetSockets];
#endif

    public static References SystemNetPrimitives =>
#if NETFRAMEWORK
        [];
#else
        [CoreMetadataReference.SystemNetPrimitives];
#endif

    public static References WindowsBase =>
#if NETFRAMEWORK
        FrameworkMetadataReference.WindowsBase;
#else
        [];
#endif

    public static References ProjectDefaultReferences =>
#if NETFRAMEWORK
        FrameworkMetadataReference.Mscorlib
            .Concat(FrameworkMetadataReference.System)
            .Concat(FrameworkMetadataReference.SystemCore)
            .Concat(FrameworkMetadataReference.SystemRuntime)
            .Concat(FrameworkMetadataReference.SystemGlobalization);
#else
        NetStandard.Concat([
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
            CoreMetadataReference.SystemSecurityPrincipalWindows]);
#endif

    public static References SystemThreadingTasksExtensions(string version) =>
#if NETFRAMEWORK
        NuGetMetadataReference.SystemThreadingTasksExtensions(version);
#else
        [CoreMetadataReference.SystemThreadingTasks];
#endif
}

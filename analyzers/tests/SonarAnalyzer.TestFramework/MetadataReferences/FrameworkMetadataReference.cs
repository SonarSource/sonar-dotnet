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

#if NETFRAMEWORK

using static SonarAnalyzer.TestFramework.MetadataReferences.MetadataReferenceFactory;
using References = System.Collections.Generic.IEnumerable<Microsoft.CodeAnalysis.MetadataReference>;

namespace SonarAnalyzer.TestFramework.MetadataReferences;

public static class FrameworkMetadataReference
{
    public static References MicrosoftVisualBasic { get; } = Create("Microsoft.VisualBasic.dll");
    public static References Mscorlib { get; } = Create("mscorlib.dll");
    public static References PresentationFramework { get; } = new[] { CreateReference("PresentationFramework.dll", "WPF"), CreateReference("PresentationCore.dll", "WPF") };
    public static References System { get; } = Create("System.dll");
    public static References SystemComponentModelComposition { get; } = Create("System.ComponentModel.Composition.dll");
    public static References SystemNetHttp { get; } = Create("System.Net.Http.dll");
    public static References SystemIOCompression { get; } = Create("System.IO.Compression.dll");
    public static References SystemIOCompressionFileSystem { get; } = Create("System.IO.Compression.FileSystem.dll");
    public static References SystemCore { get; } = Create("System.Core.dll");
    public static References SystemData { get; } = Create("System.Data.dll");
    public static References SystemDataLinq { get; } = Create("System.Data.Linq.dll");
    public static References SystemDataOracleClient { get; } = Create("System.Data.OracleClient.dll");
    public static References SystemDirectoryServices { get; } = Create("System.DirectoryServices.dll");
    public static References SystemDrawing { get; } = Create("System.Drawing.dll");
    public static References SystemGlobalization { get; } = Create("System.Globalization.dll");
    public static References SystemIdentityModel { get; } = Create("System.IdentityModel.dll");
    public static References SystemReflection { get; } = Create("System.Reflection.dll");
    public static References SystemRuntime { get; } = Create("System.Runtime.dll");
    public static References SystemRuntimeSerialization { get; } = Create("System.Runtime.Serialization.dll");
    public static References SystemRuntimeSerializationFormattersSoap { get; } = Create("System.Runtime.Serialization.Formatters.Soap.dll");
    public static References SystemSecurityCryptographyAlgorithms { get; } = Create("System.Security.Cryptography.Algorithms.dll");
    public static References SystemServiceModel { get; } = Create("System.ServiceModel.dll");
    public static References SystemThreadingTasks { get; } = Create("System.Threading.Tasks.dll");
    public static References SystemWeb { get; } = Create("System.Web.dll");
    public static References SystemWebExtensions { get; } = Create("System.Web.Extensions.dll");
    public static References SystemWebServices { get; } = Create("System.Web.Services.dll");
    public static References SystemWindowsForms { get; } = Create("System.Windows.Forms.dll");
    public static References SystemXaml { get; } = Create("System.Xaml.dll");
    public static References SystemXml { get; } = Create("System.Xml.dll");
    public static References SystemXmlLinq { get; } = Create("System.Xml.Linq.dll");
    public static References SystemXmlXDocument { get; } = Create("System.Xml.XDocument.dll");
    public static References WindowsBase { get; } = new[] { CreateReference("WindowsBase.dll", "WPF") };
}

#endif

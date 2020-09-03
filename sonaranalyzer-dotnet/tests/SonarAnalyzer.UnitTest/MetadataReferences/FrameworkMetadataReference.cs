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

#if NETFRAMEWORK

using References = System.Collections.Generic.IEnumerable<Microsoft.CodeAnalysis.MetadataReference>;
using static SonarAnalyzer.UnitTest.MetadataReferences.MetadataReferenceFactory;

namespace SonarAnalyzer.UnitTest.MetadataReferences
{
    internal static class FrameworkMetadataReference
    {
        internal static References MicrosoftVisualBasic { get; } = Create("Microsoft.VisualBasic.dll");
        internal static References Mscorlib { get; } = Create("mscorlib.dll");
        internal static References PresentationFramework { get; } = new[] { CreateReference("PresentationFramework.dll", "WPF"), CreateReference("PresentationCore.dll", "WPF") };
        internal static References System { get; } = Create("System.dll");
        internal static References SystemComponentModelComposition { get; } = Create("System.ComponentModel.Composition.dll");
        internal static References SystemNetHttp { get; } = Create("System.Net.Http.dll");
        internal static References SystemIOCompression { get; } = Create("System.IO.Compression.dll");
        internal static References SystemIOCompressionFileSystem { get; } = Create("System.IO.Compression.FileSystem.dll");
        internal static References SystemCore { get; } = Create("System.Core.dll");
        internal static References SystemData { get; } = Create("System.Data.dll");
        internal static References SystemDataOracleClient { get; } = Create("System.Data.OracleClient.dll");
        internal static References SystemDirectoryServices { get; } = Create("System.DirectoryServices.dll");
        internal static References SystemDrawing { get; } = Create("System.Drawing.dll");
        internal static References SystemGlobalization { get; } = Create("System.Globalization.dll");
        internal static References SystemIdentityModel { get; } = Create("System.IdentityModel.dll");
        internal static References SystemReflection { get; } = Create("System.Reflection.dll");
        internal static References SystemRuntime { get; } = Create("System.Runtime.dll");
        internal static References SystemRuntimeSerialization { get; } = Create("System.Runtime.Serialization.dll");
        internal static References SystemRuntimeSerializationFormattersSoap { get; } = Create("System.Runtime.Serialization.Formatters.Soap.dll");
        internal static References SystemSecurityCryptographyAlgorithms { get; } = Create("System.Security.Cryptography.Algorithms.dll");
        internal static References SystemServiceModel { get; } = Create("System.ServiceModel.dll");
        internal static References SystemThreadingTasks { get; } = Create("System.Threading.Tasks.dll");
        internal static References SystemWeb { get; } = Create("System.Web.dll");
        internal static References SystemWebExtensions { get; } = Create("System.Web.Extensions.dll");
        internal static References SystemWindowsForms { get; } = Create("System.Windows.Forms.dll");
        internal static References SystemXaml { get; } = Create("System.Xaml.dll");
        internal static References SystemXml { get; } = Create("System.Xml.dll");
        internal static References SystemXmlLinq { get; } = Create("System.Xml.Linq.dll");
        internal static References SystemXmlXDocument { get; } = Create("System.Xml.XDocument.dll");
        internal static References WindowsBase { get; } = new[] { CreateReference("WindowsBase.dll", "WPF") };
    }
}

#endif

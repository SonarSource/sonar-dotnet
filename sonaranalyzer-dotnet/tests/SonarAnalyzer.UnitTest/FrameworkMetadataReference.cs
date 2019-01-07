/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using System.IO;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.UnitTest
{
    internal static class FrameworkMetadataReference
    {
        #region Helpers

        private static readonly string systemAssembliesFolder =
            new FileInfo(typeof(object).Assembly.Location).Directory.FullName;

        private static IEnumerable<MetadataReference> Create(string assemblyName) =>
            ImmutableArray.Create((MetadataReference)MetadataReference.CreateFromFile(
                Path.Combine(systemAssembliesFolder, assemblyName)));

        #endregion Helpers

        internal static IEnumerable<MetadataReference> MicrosoftVisualBasic { get; }
            = Create("Microsoft.VisualBasic.dll");

        internal static IEnumerable<MetadataReference> Mscorlib { get; }
            = Create("mscorlib.dll");

        internal static IEnumerable<MetadataReference> Netstandard { get; }
            = Create("netstandard.dll");

        internal static IEnumerable<MetadataReference> System { get; }
            = Create("System.dll");

        internal static IEnumerable<MetadataReference> SystemComponentModelComposition { get; }
            = Create("System.ComponentModel.Composition.dll");

        internal static IEnumerable<MetadataReference> SystemNetHttp { get; }
            = Create("System.Net.Http.dll");

        internal static IEnumerable<MetadataReference> SystemIOCompression { get; }
            = Create("System.IO.Compression.dll");

        internal static IEnumerable<MetadataReference> SystemIOCompressionFileSystem { get; }
            = Create("System.IO.Compression.FileSystem.dll");

        internal static IEnumerable<MetadataReference> SystemCore { get; }
            = Create("System.Core.dll");

        internal static IEnumerable<MetadataReference> SystemData { get; }
            = Create("System.Data.dll");

        internal static IEnumerable<MetadataReference> SystemDataOracleClient { get; }
            = Create("System.Data.OracleClient.dll");

        internal static IEnumerable<MetadataReference> SystemDirectoryServices { get; }
            = Create("System.DirectoryServices.dll");

        internal static IEnumerable<MetadataReference> SystemDrawing { get; }
            = Create("System.Drawing.dll");

        internal static IEnumerable<MetadataReference> SystemGlobalization { get; }
            = Create("System.Globalization.dll");

        internal static IEnumerable<MetadataReference> SystemIdentityModel { get; }
            = Create("System.IdentityModel.dll");

        internal static IEnumerable<MetadataReference> SystemReflection { get; }
            = Create("System.Reflection.dll");

        internal static IEnumerable<MetadataReference> SystemRuntime { get; }
            = Create("System.Runtime.dll");

        internal static IEnumerable<MetadataReference> SystemRuntimeSerialization { get; }
            = Create("System.Runtime.Serialization.dll");

        internal static IEnumerable<MetadataReference> SystemServiceModel { get; }
            = Create("System.ServiceModel.dll");

        internal static IEnumerable<MetadataReference> SystemThreadingTasks { get; }
            = Create("System.Threading.Tasks.dll");

        internal static IEnumerable<MetadataReference> SystemWeb { get; }
            = Create("System.Web.dll");

        internal static IEnumerable<MetadataReference> SystemWindowsForms { get; }
            = Create("System.Windows.Forms.dll");

        internal static IEnumerable<MetadataReference> SystemXaml { get; }
            = Create("System.Xaml.dll");

        internal static IEnumerable<MetadataReference> SystemXml { get; }
            = Create("System.Xml.dll");

        internal static IEnumerable<MetadataReference> SystemXmlLinq { get; }
            = Create("System.Xml.Linq.dll");

        internal static IEnumerable<MetadataReference> SystemXmlXDocument { get; }
            = Create("System.Xml.XDocument.dll");
    }
}

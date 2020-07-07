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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class NetFrameworkVersionProviderTest
    {
        private const string CodeSnippet = @"
            public class Foo
            {
                public class Foo
                {
                    public void Bar() { }
                }
            }
            ";

        [TestMethod]
        public void NetFrameworkVersionProvider_WithNullCompilation_ReturnsUnknown()
        {
            // Arrange
            var versionProvider = new NetFrameworkVersionProvider();

            // Act & Assert
            versionProvider.GetDotNetFrameworkVersion(null).Should().Be(NetFrameworkVersion.Unknown);
        }

        [TestMethod]
        public void NetFrameworkVersionProvider_CurrentAssemblyMscorlib()
        {
            var compilation = GetRawCompilation(GetAdditionalReferences());
            var versionProvider = new NetFrameworkVersionProvider();

#if NETFRAMEWORK
            versionProvider.GetDotNetFrameworkVersion(compilation).Should().Be(NetFrameworkVersion.After452);
#else
            versionProvider.GetDotNetFrameworkVersion(compilation).Should().Be(NetFrameworkVersion.Unknown);
#endif
        }

        [TestMethod]
        public void NetFrameworkVersionProvider_CurrentAssemblyMscorlib_Netstandard()
        {
            var compilation = GetRawCompilation(GetAdditionalReferences());
            var versionProvider = new NetFrameworkVersionProvider();

#if NETFRAMEWORK
            // the local .net framework mscorlib is actually used
            versionProvider.GetDotNetFrameworkVersion(compilation).Should().Be(NetFrameworkVersion.After452);
#else
            versionProvider.GetDotNetFrameworkVersion(compilation).Should().Be(NetFrameworkVersion.Unknown);
#endif
        }

        [TestMethod]
        public void NetFrameworkVersionProvider_Net35()
        {
            var mscorlib35 = ImmutableArray.Create((MetadataReference)
                MetadataReference.CreateFromFile(CreateMockPath("3.5/mscorlib.dll")));
            var compilation = GetRawCompilation(mscorlib35);
            var versionProvider = new NetFrameworkVersionProvider();
            versionProvider.GetDotNetFrameworkVersion(compilation).Should().Be(NetFrameworkVersion.Probably35);
        }

        [TestMethod]
        public void NetFrameworkVersionProvider_Net40_NoIOClass()
        {
            var mscorlib35 = ImmutableArray.Create((MetadataReference)
                MetadataReference.CreateFromFile(CreateMockPath("4.0_no_IO/mscorlib.dll")));
            var compilation = GetRawCompilation(mscorlib35);
            var versionProvider = new NetFrameworkVersionProvider();
            versionProvider.GetDotNetFrameworkVersion(compilation).Should().Be(NetFrameworkVersion.Between4And451);
        }

        [TestMethod]
        public void NetFrameworkVersionProvider_Net40_WithIOClass()
        {
            var mscorlib35 = ImmutableArray.Create((MetadataReference)
                MetadataReference.CreateFromFile(CreateMockPath("4.0_with_IO/mscorlib.dll")));
            var compilation = GetRawCompilation(mscorlib35);
            var versionProvider = new NetFrameworkVersionProvider();
            versionProvider.GetDotNetFrameworkVersion(compilation).Should().Be(NetFrameworkVersion.Between4And451);
        }

        [TestMethod]
        public void NetFrameworkVersionProvider_NoReference()
        {

            var compilation = GetRawCompilation();
            var versionProvider = new NetFrameworkVersionProvider();
            versionProvider.GetDotNetFrameworkVersion(compilation).Should().Be(NetFrameworkVersion.Unknown);
        }

        private static Compilation GetRawCompilation(IEnumerable<MetadataReference> theReferences = null)
        {
            var solution = new AdhocWorkspace().CurrentSolution;
            var project = solution.AddProject("test", "test", LanguageNames.CSharp);
            var projectBuilder = ProjectBuilder.FromProject(project);
            return projectBuilder
                .AddSnippet(CodeSnippet)
                .AddReferences(theReferences)
                .GetCompilation();
        }

        private static string CreateMockPath(string mockName)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            return Path.Combine(Path.GetDirectoryName(assembly.Location), "../../../../FrameworkMocks/lib/", mockName);
        }

        private static IEnumerable<MetadataReference> GetAdditionalReferences() =>
            MetadataReferenceFacade.GetMsCorLib()
                .Concat(MetadataReferenceFacade.GetSystemComponentModelComposition());
    }
}

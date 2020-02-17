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
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
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
            var compilation = GetCompilation(FrameworkMetadataReference.Mscorlib.Concat(FrameworkMetadataReference.System));
            var versionProvider = new NetFrameworkVersionProvider();
            versionProvider.GetDotNetFrameworkVersion(compilation).Should().Be(NetFrameworkVersion.After45);
        }

        [TestMethod]
        public void NetFrameworkVersionProvider_CurrentAssemblyMscorlib_Netstandard()
        {
            var compilation = GetCompilation(FrameworkMetadataReference.Mscorlib
                .Concat(FrameworkMetadataReference.System)
                .Concat(FrameworkMetadataReference.Netstandard));
            var versionProvider = new NetFrameworkVersionProvider();
            // the local .net framework mscorlib is actually used
            versionProvider.GetDotNetFrameworkVersion(compilation).Should().Be(NetFrameworkVersion.After45);
        }

        [TestMethod]
        public void NetFrameworkVersionProvider_NoReference()
        {

            var compilation = GetNoReferencesCompilation();
            var versionProvider = new NetFrameworkVersionProvider();
            versionProvider.GetDotNetFrameworkVersion(compilation).Should().Be(NetFrameworkVersion.Unknown);
        }

        private Compilation GetCompilation(IEnumerable<MetadataReference> additionalReferences)
        {
            return SolutionBuilder
                .Create()
                .AddProject(AnalyzerLanguage.CSharp, createExtraEmptyFile: false)
                .AddSnippet(CodeSnippet)
                .AddReferences(additionalReferences)
                .GetCompilation();
        }

        private Compilation GetNoReferencesCompilation()
        {
            var solution = new AdhocWorkspace().CurrentSolution;
            var project = solution.AddProject("test", "test", LanguageNames.CSharp);
            var projectBuilder = ProjectBuilder.FromProject(project);
            return projectBuilder
                .AddSnippet(CodeSnippet)
                .GetCompilation();
        }
    }
}

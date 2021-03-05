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

using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class FilesToAnalyzeProviderTest
    {
        private const string MixedSlashesWebConfigPath1 = @"C:\Projects/DummyProj/wEB.config";
        private const string MixedSlashesWebConfigPath2 = @"C:\Projects/DummyProj/Views\Web.confiG";
        private const string FilesToAnalyzePath = @"ResourceTests\FilesToAnalyze\FilesToAnalyze.txt";
        private const string InvalidFilesToAnalyzePath = @"ResourceTests\FilesToAnalyze\InvalidValuesFileToAnalyze.txt";

        [TestMethod]
        public void FileNameWithMixedCapitalizationAndMixedSlashes_FindFilesWithFileName_ReturnsAllWebConfigFiles()
        {
            var sut = new FilesToAnalyzeProvider(FilesToAnalyzePath);

            var results = sut.FindFiles("Web.config");
            results.Should().HaveCount(2);
            results.Should().Contain(MixedSlashesWebConfigPath1);
            results.Should().Contain(MixedSlashesWebConfigPath2);
        }

        [TestMethod]
        public void FileNameWithMixedCapitalizationAndMixedSlashes_FindFilesWithRegex_ReturnsAllWebConfigFiles()
        {
            var fileNamePattern = new Regex("web\\.config$", RegexOptions.IgnoreCase);

            var sut = new FilesToAnalyzeProvider(FilesToAnalyzePath);

            var results = sut.FindFiles(fileNamePattern);
            results.Should().HaveCount(2);
            results.Should().Contain(MixedSlashesWebConfigPath1);
            results.Should().Contain(MixedSlashesWebConfigPath2);
        }

        [TestMethod]
        public void FileWithInvalidvalues_FindFilesWithFileName_ReturnsValidValue()
        {
            var sut = new FilesToAnalyzeProvider(InvalidFilesToAnalyzePath);

            var results = sut.FindFiles("Web.config");
            results.Should().HaveCount(1);
            results.Should().Contain(MixedSlashesWebConfigPath2);
        }

        [TestMethod]
        public void FileWithInvalidvalues_FindFilesWithRegex_ReturnsValidValue()
        {
            var fileNamePattern = new Regex("web\\.config$", RegexOptions.IgnoreCase);

            var sut = new FilesToAnalyzeProvider(InvalidFilesToAnalyzePath);

            var results = sut.FindFiles(fileNamePattern);
            results.Should().HaveCount(1);
            results.Should().Contain(MixedSlashesWebConfigPath2);
        }

        [TestMethod]
        public void NonExistentFile_FindFiles_ReturnsEmptyEnumerable()
        {
            var sut = new FilesToAnalyzeProvider(@"ResourceTests\FilesToAnalyze\NonExistingFile.txt");

            var results = sut.FindFiles("Web.config");
            results.Should().BeEmpty();
        }

        [TestMethod]
        public void EmptyFile_FindFiles_ReturnsEmptyEnumerable()
        {
            var sut = new FilesToAnalyzeProvider(@"ResourceTests\FilesToAnalyze\EmptyFilesToAnalyze.txt");

            var results = sut.FindFiles("Web.config");
            results.Should().BeEmpty();
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow(null)]
        [DataRow("invalidPath")]
        public void InvalidPath_FindFiles_ReturnsEmptyEnumerable(string filePath)
        {
            var sut = new FilesToAnalyzeProvider(filePath);

            var results = sut.FindFiles("Web.config");
            results.Should().BeEmpty();
        }
    }
}

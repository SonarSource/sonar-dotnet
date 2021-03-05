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

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class FilesToAnalyzeStorageTest
    {
        private const string FilePath = "DummyFilePath";
        private const string MixedCasingWebConfigPath1 = @"C:\Projects\DummyProj\wEB.config";
        private const string MixedCasingWebConfigPath2 = @"C:\Projects\DummyProj\Views\Web.confiG";
        private const string MixedSlashesWebConfigPath1 = @"C:\Projects/DummyProj/wEB.config";
        private const string MixedSlashesWebConfigPath2 = @"C:\Projects/DummyProj/Views\Web.confiG";
        private const string GlobalAsaxFilePath = @"C:\Projects\DummyProj\Views\Global.asax";
        private const string CSharpHomeControllerPath = @"C:\Projects\DummyProj\Csharp\Controllers\HomeController.cs";
        private const string VisualBasicHomeControllerPath = @"C:\Projects\DummyProj\VisualBasic\Controllers\HomeController.vb";

        [TestMethod]
        public void FileNameWithMixedCapitalization_FilesToAnalyzeWithFileName_ReturnsAllWebConfigFilesIgnoringCaseMismatch()
        {
            var filesToAnalyze = new List<string>()
            {
                MixedCasingWebConfigPath1,
                MixedCasingWebConfigPath2,
                GlobalAsaxFilePath,
                CSharpHomeControllerPath,
                VisualBasicHomeControllerPath,
            };

            var filesToAnalyzeRetriever = SetupFilesToAnalyzeRetriever(FilePath, filesToAnalyze);

            var sut = new FilesToAnalyzeStorage(FilePath, filesToAnalyzeRetriever);

            var results = sut.FilesToAnalyze("Web.config");
            results.Should().HaveCount(2);
            results.Should().Contain(MixedCasingWebConfigPath1);
            results.Should().Contain(MixedCasingWebConfigPath2);
        }

        [TestMethod]
        public void FilePathsWithMixedSlashes_FilesToAnalyzeWithFileName_ReturnsAllValueWebConfigFiles()
        {
            var filesToAnalyze = new List<string>()
            {
                MixedSlashesWebConfigPath1,
                MixedSlashesWebConfigPath2,
                GlobalAsaxFilePath,
                CSharpHomeControllerPath,
                VisualBasicHomeControllerPath,
            };

            var filesToAnalyzeRetriever = SetupFilesToAnalyzeRetriever(FilePath, filesToAnalyze);

            var sut = new FilesToAnalyzeStorage(FilePath, filesToAnalyzeRetriever);

            var results = sut.FilesToAnalyze("Web.config");
            results.Should().HaveCount(2);
            results.Should().Contain(MixedSlashesWebConfigPath1);
            results.Should().Contain(MixedSlashesWebConfigPath2);
        }

        [TestMethod]
        public void FileNameWithMixedCapitalization_FilesToAnalyzeWithRegex_ReturnsAllWebConfigFilesIgnoringCaseMismatch()
        {
            var filesToAnalyze = new List<string>()
            {
                MixedCasingWebConfigPath1,
                MixedCasingWebConfigPath2,
                GlobalAsaxFilePath,
                CSharpHomeControllerPath,
                VisualBasicHomeControllerPath,
            };

            var filesToAnalyzeRetriever = SetupFilesToAnalyzeRetriever(FilePath, filesToAnalyze);
            Regex fileNamePattern = new Regex("web\\.config", RegexOptions.IgnoreCase);

            var sut = new FilesToAnalyzeStorage(FilePath, filesToAnalyzeRetriever);

            var results = sut.FilesToAnalyze(fileNamePattern);
            results.Should().HaveCount(2);
            results.Should().Contain(MixedCasingWebConfigPath1);
            results.Should().Contain(MixedCasingWebConfigPath2);
        }

        [TestMethod]
        public void FilePathsWithMixedSlashes_FilesToAnalyzeWithRegex_ReturnsAllValueWebConfigFiles()
        {
            var filesToAnalyze = new List<string>()
            {
                MixedSlashesWebConfigPath1,
                MixedSlashesWebConfigPath2,
                GlobalAsaxFilePath,
                CSharpHomeControllerPath,
                VisualBasicHomeControllerPath,
            };

            var filesToAnalyzeRetriever = SetupFilesToAnalyzeRetriever(FilePath, filesToAnalyze);
            Regex fileNamePattern = new Regex("web\\.config", RegexOptions.IgnoreCase);

            var sut = new FilesToAnalyzeStorage(FilePath, filesToAnalyzeRetriever);

            var results = sut.FilesToAnalyze(fileNamePattern);
            results.Should().HaveCount(2);
            results.Should().Contain(MixedSlashesWebConfigPath1);
            results.Should().Contain(MixedSlashesWebConfigPath2);
        }

        [TestMethod]
        public void NoFilesToAnalyze_FilesToAnalyze_ReturnsEmptyEnumerable()
        {
            var filesToAnalyzeRetriever = SetupFilesToAnalyzeRetriever(FilePath, Enumerable.Empty<string>());

            var sut = new FilesToAnalyzeStorage(FilePath, filesToAnalyzeRetriever);

            var results = sut.FilesToAnalyze("Web.config");
            results.Should().BeEmpty();
        }

        private IFilesToAnalyzeRetriever SetupFilesToAnalyzeRetriever(string filePath, IEnumerable<string> filesToAnalyze)
        {
            var filesToAnalyzeRetrieverMock = new Mock<IFilesToAnalyzeRetriever>();
            filesToAnalyzeRetrieverMock.Setup(x => x.RetrieveFilesToAnalyze(filePath)).Returns(filesToAnalyze);
            return filesToAnalyzeRetrieverMock.Object;
        }
    }
}

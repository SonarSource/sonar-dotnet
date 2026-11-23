/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.IO;
using System.Text.RegularExpressions;

namespace SonarAnalyzer.Core.Configuration.Test;

[TestClass]
public class FilesToAnalyzeProviderTest
{
    private const string MixedSlashesWebConfigPath1 = @"C:\Projects/DummyProj/wEB.config";
    private const string MixedSlashesWebConfigPath2 = @"C:\Projects/DummyProj/Views\Web.confiG";
    private const string FilesToAnalyzePath = @"TestResources\FilesToAnalyze\FilesToAnalyze.txt";
    private const string InvalidFilesToAnalyzePath = @"TestResources\FilesToAnalyze\FilesToAnalyzeWithInvalidValues.txt";

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void FileNameWithMixedCapitalizationAndMixedSlashes_FindFilesWithFileName_ReturnsAllWebConfigFiles()
    {
        var sut = new FilesToAnalyzeProvider(FilesToAnalyzePath);

        var results = sut.FindFiles("Web.config", false);
        results.Should().BeEquivalentTo(new[] { MixedSlashesWebConfigPath1, MixedSlashesWebConfigPath2 });
    }

    [TestMethod]
    public void FileNameWithMixedCapitalizationAndMixedSlashes_FindFilesWithRegex_ReturnsAllWebConfigFiles()
    {
        var fileNamePattern = new Regex(@"[\\\/]web\.config$", RegexOptions.IgnoreCase);

        var sut = new FilesToAnalyzeProvider(FilesToAnalyzePath);

        var results = sut.FindFiles(fileNamePattern, false);
        results.Should().BeEquivalentTo(new[] { MixedSlashesWebConfigPath1, MixedSlashesWebConfigPath2 });
    }

    [TestMethod]
    public void FileWithInvalidValues_FindFilesWithFileName_ReturnsValidValue()
    {
        var sut = new FilesToAnalyzeProvider(InvalidFilesToAnalyzePath);

        var results = sut.FindFiles("123", false);
        results.Should().ContainSingle();
        results.Should().Contain("123");
    }

    [TestMethod]
    public void FileWithInvalidValues_FindFilesWithRegex_ReturnsValidValue()
    {
        var fileNamePattern = new Regex("web\\.config$", RegexOptions.IgnoreCase);

        var sut = new FilesToAnalyzeProvider(InvalidFilesToAnalyzePath);

        var results = sut.FindFiles(fileNamePattern, false);
        results.Should().BeEquivalentTo(new[]
        {
            MixedSlashesWebConfigPath2,
            @"C:\Projects\Controllers:web.config",
            @"C:web.config",
            @"C:\Projects<web.config",
            @"C:\Projects>\Controllers/web.config"
        });
    }

    [TestMethod]
    public void EmptyFile_FindFiles_ReturnsEmptyEnumerable()
    {
        var sut = new FilesToAnalyzeProvider(@"TestResources\FilesToAnalyze\EmptyFilesToAnalyze.txt");

        var results = sut.FindFiles(new Regex(".*"), false);
        results.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(null)]
    [DataRow("invalidPath")]
    [DataRow(@"TestResources\FilesToAnalyze\NonExistingFile.txt")]
    public void InvalidPath_FindFiles_ReturnsEmptyEnumerable(string filePath)
    {
        var sut = new FilesToAnalyzeProvider(filePath);

        var results = sut.FindFiles(new Regex(".*"), false);
        results.Should().BeEmpty();
    }

    [TestMethod]
    public void FileWithValidValues_FindFilesRequestingAnyFile_AllValuesFromTheFileAreReturned()
    {
        var sut = new FilesToAnalyzeProvider(FilesToAnalyzePath);

        var results = sut.FindFiles(new Regex(".*"), false);
        results.Should().BeEquivalentTo(new[]
        {
            MixedSlashesWebConfigPath1,
            MixedSlashesWebConfigPath2,
            @"C:\Projects\DummyProj\Views\Global.asax",
            @"C:\Projects\DummyProj\Csharp\Controllers\HomeController.cs",
            @"C:\Projects\DummyProj\VisualBasic\Controllers\HomeController.vb",
            @"C:\Projects/DummyProj/Views\Web.confiGuration"
        });
    }

    [TestMethod]
    public void WebConfigFiles_ReturnsExistingWebConfigs_ExceptDebugAndTest()
    {
        var expected = new[] { TestFiles.WriteFile(TestContext, "Web.config"), TestFiles.WriteFile(TestContext, "web.Release.config") };
        var sut = new FilesToAnalyzeProvider(TestFiles.WriteFile(TestContext, "FilesToAnalyze.txt", string.Join(
            Environment.NewLine,
            expected[0],
            expected[1],
            TestFiles.WriteFile(TestContext, "web.debug.config"),
            TestFiles.WriteFile(TestContext, "web.Test.config"),
            TestFiles.WriteFile(TestContext, "Web.configuration"),
            TestFiles.TestPath(TestContext, @"NonExisting\web.config"))));

        sut.WebConfigFiles.Should().BeEquivalentTo(expected);
        sut.WebConfigFiles.Should().BeSameAs(sut.WebConfigFiles);
    }

    [TestMethod]
    public void AppSettingsFiles_ReturnsExistingAppSettings_ExceptDevelopmentAndTest()
    {
        var expected = new[] { TestFiles.WriteFile(TestContext, "appsettings.json"), TestFiles.WriteFile(TestContext, "AppSettings.Production.json") };
        var sut = new FilesToAnalyzeProvider(TestFiles.WriteFile(TestContext, "FilesToAnalyze.txt", string.Join(
            Environment.NewLine,
            expected[0],
            expected[1],
            TestFiles.WriteFile(TestContext, "appsettings.Development.json"),
            TestFiles.WriteFile(TestContext, "appsettings.Test.json"),
            TestFiles.WriteFile(TestContext, "appsettings.jsonc"),
            TestFiles.TestPath(TestContext, @"NonExisting\appsettings.json"))));

        sut.AppSettingsFiles.Should().BeEquivalentTo(expected);
        sut.AppSettingsFiles.Should().BeSameAs(sut.AppSettingsFiles);
    }

    [TestMethod]
    [DataRow("web.config", "web.config", true)]
    [DataRow("Web.config", "WEB.CONFIG", true)]
    [DataRow(@"Views\web.CONFIG", "web.config", true)]
    [DataRow("web.Release.config", "web.Release.config", true)]
    [DataRow("web.Release.config", "web.config", false)]
    [DataRow("web.debug.config", "web.debug.config", false)]
    [DataRow("web.Test.config", "web.Test.config", false)]
    [DataRow("appsettings.json", "appsettings.json", false)]
    public void FindWebConfigs_ReturnsWebConfigsWithTheFileName(string writtenFile, string fileName, bool isFound)
    {
        var path = TestFiles.WriteFile(TestContext, writtenFile);
        new FilesToAnalyzeProvider(TestFiles.WriteFile(TestContext, "FilesToAnalyze.txt", path)).FindWebConfigs(fileName).Should().BeEquivalentTo(isFound ? [path] : []);
    }

    [TestMethod]
    public void FindWebConfigs_NonExistingFile_IsNotFound() =>
        new FilesToAnalyzeProvider(TestFiles.WriteFile(TestContext, "FilesToAnalyze.txt", TestFiles.TestPath(TestContext, "web.config"))).FindWebConfigs("web.config").Should().BeEmpty();

    [TestMethod]
    public void LaunchSettingsFiles_ReturnsExistingLaunchSettings()
    {
        var expected = TestFiles.WriteFile(TestContext, "LAUNCHSETTINGS.json");
        var sut = new FilesToAnalyzeProvider(TestFiles.WriteFile(TestContext, "FilesToAnalyze.txt", string.Join(
            Environment.NewLine,
            expected,
            TestFiles.WriteFile(TestContext, "launchSettings.Test.json"),
            TestFiles.TestPath(TestContext, @"NonExisting\launchSettings.json"))));

        sut.LaunchSettingsFiles.Should().BeEquivalentTo(expected);
        sut.LaunchSettingsFiles.Should().BeSameAs(sut.LaunchSettingsFiles);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow(@"TestResources\FilesToAnalyze\NonExistingFile.txt")]
    public void InvalidPath_SettingsFiles_ReturnsEmpty(string filePath)
    {
        var sut = new FilesToAnalyzeProvider(filePath);

        sut.WebConfigFiles.Should().BeEmpty();
        sut.AppSettingsFiles.Should().BeEmpty();
        sut.LaunchSettingsFiles.Should().BeEmpty();
    }

    [TestMethod]
    public void UnableToOpenFile_FindFiles_ReturnsEmptyEnumerable()
    {
        using (Stream iStream = File.Open(FilesToAnalyzePath, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            var sut = new FilesToAnalyzeProvider(FilesToAnalyzePath);

            var results = sut.FindFiles(new Regex(".*"));
            results.Should().BeEmpty();
        }
    }
}

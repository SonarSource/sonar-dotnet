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

namespace SonarAnalyzer.Core.Configuration;

public class FilesToAnalyzeProvider
{
    // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments#development-and-launchsettingsjson
    private const string LaunchSettingsFileName = "launchSettings.json";

    private static readonly TimeSpan FileNameTimeout = TimeSpan.FromMilliseconds(100);
    private static readonly Regex WebConfigRegex = new(@"[\\/]web\.([^\\/]+\.)?config$", RegexOptions.IgnoreCase, FileNameTimeout);
    private static readonly Regex AppSettingsRegex = new(@"[\\/]appsettings\.([^\\/]+\.)?json$", RegexOptions.IgnoreCase, FileNameTimeout);

    private readonly IEnumerable<string> allFiles;
    private readonly Lazy<string[]> webConfigFiles;
    private readonly Lazy<string[]> appSettingsFiles;
    private readonly Lazy<string[]> launchSettingsFiles;

    public IReadOnlyList<string> WebConfigFiles => webConfigFiles.Value;
    public IReadOnlyList<string> AppSettingsFiles => appSettingsFiles.Value;
    public IReadOnlyList<string> LaunchSettingsFiles => launchSettingsFiles.Value;

    public FilesToAnalyzeProvider(string filePath)
    {
        allFiles = ReadLines(filePath);
        webConfigFiles = new(() => FindFiles(".config", WebConfigRegex).Where(x => ShouldProcess(x, "web.debug.config")).ToArray());
        appSettingsFiles = new(() => FindFiles(".json", AppSettingsRegex).Where(x => ShouldProcess(x, "appsettings.development.json")).ToArray());
        launchSettingsFiles = new(() => FindFiles(LaunchSettingsFileName).ToArray());
    }

    public IEnumerable<string> FindWebConfigs(string fileName) =>
        WebConfigFiles.Where(x => FilterByFileName(x, fileName));

    internal IEnumerable<string> FindFiles(string fileName, bool onlyExistingFiles = true) =>
        allFiles.Where(x => FilterByFileName(x, fileName) && (!onlyExistingFiles || File.Exists(x)));

    internal IEnumerable<string> FindFiles(Regex fullPathRegex, bool onlyExistingFiles = true) =>
        allFiles.Where(x => fullPathRegex.SafeIsMatch(x) && (!onlyExistingFiles || File.Exists(x)));

    // Performance: the EndsWith check is much cheaper than the regex, and rules out most files
    private IEnumerable<string> FindFiles(string extension, Regex fullPathRegex) =>
        allFiles.Where(x => x.EndsWith(extension, StringComparison.OrdinalIgnoreCase) && fullPathRegex.SafeIsMatch(x) && File.Exists(x));

    private static bool ShouldProcess(string path, string excludedFileName)
    {
        var name = Path.GetFileName(path);
        return !name.Equals(excludedFileName, StringComparison.OrdinalIgnoreCase)
            && name.IndexOf(".Test", StringComparison.OrdinalIgnoreCase) < 0;
    }

    private static IEnumerable<string> ReadLines(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return Enumerable.Empty<string>();
        }

        try
        {
            return File.ReadAllLines(filePath);
        }
        catch
        {
            // cannot log exception
            return Enumerable.Empty<string>();
        }
    }

    private static bool FilterByFileName(string fullPath, string fileName)
    {
        try
        {
            return Path.GetFileName(fullPath).Equals(fileName, StringComparison.OrdinalIgnoreCase);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}

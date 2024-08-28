/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using System.IO;
using Roslyn.Utilities;

namespace SonarAnalyzer.Extensions;

public static class AnalyzerOptionsExtensions
{
    public static AdditionalText SonarLintXml(this AnalyzerOptions options) =>
        options.AdditionalFile("SonarLint.xml");

    public static AdditionalText SonarProjectConfig(this AnalyzerOptions options) =>
        options.AdditionalFile("SonarProjectConfig.xml");

    public static AdditionalText ProjectOutFolderPath(this AnalyzerOptions options) =>
        options.AdditionalFile("ProjectOutFolderPath.txt");

    [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7440", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
    private static AdditionalText AdditionalFile(this AnalyzerOptions options, string fileName)
    {
        // HotPath: This code path needs to be allocation free. Don't use Linq.
        foreach (var additionalText in options.AdditionalFiles) // Uses the struct enumerator of ImmutableArray
        {
            // Don't use Path.GetFilename. It allocates a string.
            if (additionalText.Path is { } path
                && path.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
            {
                // The character before the filename (if there is a character) must be a directory separator
                var separatorPosition = path.Length - fileName.Length - 1;
                if (separatorPosition < 0 || IsDirectorySeparator(path[separatorPosition]))
                {
                    return additionalText;
                }
            }
        }
        return null;

        static bool IsDirectorySeparator(char c) =>
            c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;
    }
}

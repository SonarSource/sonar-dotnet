/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Extensions;

public static class AnalyzerOptionsExtensions
{
    public static AdditionalText SonarLintXml(this AnalyzerOptions options) =>
        options.AdditionalFile("SonarLint.xml");

    public static AdditionalText SonarProjectConfig(this AnalyzerOptions options) =>
        options.AdditionalFile("SonarProjectConfig.xml");

    public static AdditionalText ProjectOutFolderPath(this AnalyzerOptions options) =>
        options.AdditionalFile("ProjectOutFolderPath.txt");

    private static AdditionalText AdditionalFile(this AnalyzerOptions options, string fileName)
    {
        foreach (var additionalText in options.AdditionalFiles)
        {
            if (additionalText.Path?.EndsWith(fileName, StringComparison.OrdinalIgnoreCase) is true)
            {
                return additionalText;
            }
        }
        return null;
    }
}

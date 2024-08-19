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
using System.Xml.Linq;

namespace SonarAnalyzer.Helpers;

/// <summary>
/// This class reads and encapsulates <see cref="AnalysisConfig"/>, exposing only the configuration our analyzers need.
/// </summary>
public class AnalysisConfigReader
{
    private readonly AnalysisConfig analysisConfig;

    public AnalysisConfigReader(string analysisConfigPath)
    {
        try
        {
            analysisConfig = new(XDocument.Load(analysisConfigPath));
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"File '{analysisConfigPath}' could not be parsed.", e);
        }
    }

    public string[] UnchangedFiles() =>
        ConfigValue("UnchangedFilesPath") is { } unchangedFilesPath
            ? File.ReadAllLines(unchangedFilesPath)
            : Array.Empty<string>();

    private string ConfigValue(string id) =>
        analysisConfig.AdditionalConfig.FirstOrDefault(x => x.Id == id)?.Value;
}

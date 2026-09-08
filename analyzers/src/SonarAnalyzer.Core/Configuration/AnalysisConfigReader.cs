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
using System.Xml.Linq;

namespace SonarAnalyzer.Core.Configuration;

/// <summary>
/// This class reads and encapsulates <see cref="AnalysisConfig"/>, exposing only the configuration our analyzers need.
/// </summary>
public class AnalysisConfigReader
{
    private readonly AnalysisConfig analysisConfig;

    public bool IsCloud => analysisConfig.SonarQubeVersion == "Cloud"                   // S4NET from version 12.0 onwards
                            || analysisConfig.SonarQubeVersion.StartsWith("8.0.0");     // S4NET up to 11.3. We don't care about exact SQ 8.0 release version, this will never be used there

    public AnalysisConfigReader(string analysisConfigPath)
    {
        if (File.Exists(analysisConfigPath))
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
        else
        {
            analysisConfig = AnalysisConfig.Empty;
        }
    }

    public string[] UnchangedFiles() =>
        ConfigValue("UnchangedFilesPath") is { } unchangedFilesPath
            ? File.ReadAllLines(unchangedFilesPath)
            : [];

    private string ConfigValue(string id) =>
        analysisConfig.AdditionalConfig.FirstOrDefault(x => x.Id == id)?.Value;
}

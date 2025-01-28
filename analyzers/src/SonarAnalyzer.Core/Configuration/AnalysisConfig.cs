/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Xml.Linq;

namespace SonarAnalyzer.Core.Configuration;

/// <summary>
/// Data class to describe an analysis configuration.
/// </summary>
/// <remarks>
/// This class is the counterpart of SonarScanner.MSBuild.Common.AnalysisConfig.
/// This class should not be used directly in this codebase. To get configuration properties, use <see cref="AnalysisConfigReader"/>.
/// </remarks>
internal sealed class AnalysisConfig
{
    public ConfigSetting[] AdditionalConfig { get; }

    public AnalysisConfig(XDocument document)
    {
        var xmlns = XNamespace.Get("http://www.sonarsource.com/msbuild/integration/2015/1");
        if (document.Root.Name != xmlns + "AnalysisConfig")
        {
            throw new InvalidOperationException("Unexpected Root node: " + document.Root.Name);
        }
        AdditionalConfig = document.Root.Element(xmlns + "AdditionalConfig")?.Elements(xmlns + "ConfigSetting").Select(x => new ConfigSetting(x)).ToArray() ?? [];
    }
}

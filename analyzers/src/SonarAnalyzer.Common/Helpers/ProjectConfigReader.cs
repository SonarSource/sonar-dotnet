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

using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SonarAnalyzer.Helpers
{
    /// <summary>
    /// This class reads and encapsulates <see cref="ProjectConfig"/>, exposing only the configuration our analyzers need.
    /// </summary>
    internal class ProjectConfigReader
    {
        private readonly Lazy<ProjectConfig> projectConfig;

        public string AnalysisConfigPath => ProjectConfigValue?.AnalysisConfigPath;
        public string FilesToAnalyzePath => ProjectConfigValue?.FilesToAnalyzePath;
        public string OutPath => ProjectConfigValue?.OutPath;
        public string ProjectPath => ProjectConfigValue?.ProjectPath;
        public ProjectType? ProjectType => ProjectConfigValue?.ProjectType;
        public string TargetFramework => ProjectConfigValue?.TargetFramework;

        private ProjectConfig ProjectConfigValue => projectConfig.Value;

        public ProjectConfigReader(AnalyzerOptions options)
        {
            projectConfig = new Lazy<ProjectConfig>(() => ReadContent(options));
        }

        private static ProjectConfig ReadContent(AnalyzerOptions options)
        {
            var sonarProjectConfig = options.AdditionalFiles.FirstOrDefault(IsSonarProjectConfig);
            if (sonarProjectConfig == null || !File.Exists(sonarProjectConfig.Path))
            {
                return null;
            }
            try
            {
                var ser = new XmlSerializer(typeof(ProjectConfig));
                using var fs = File.Open(sonarProjectConfig.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
                return (ProjectConfig)ser.Deserialize(fs);
            }
            catch (Exception)
            {
                // cannot log exception
                return null;
            }
        }

        private static bool IsSonarProjectConfig(AdditionalText additionalText) =>
            additionalText.Path != null
            && new FileInfo(additionalText.Path).Name.Equals("SonarProjectConfig.xml", StringComparison.OrdinalIgnoreCase);
    }
}

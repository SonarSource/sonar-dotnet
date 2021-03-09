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
using System.Xml.Serialization;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Helpers
{
    /// <summary>
    /// This class reads and encapsulates <see cref="ProjectConfig"/>, exposing only the configuration our analyzers need.
    /// </summary>
    internal class ProjectConfigReader
    {
        private readonly ProjectConfig projectConfig;
        private readonly Lazy<ProjectType> projectType;

        public string AnalysisConfigPath => projectConfig.AnalysisConfigPath;
        public string FilesToAnalyzePath => projectConfig.FilesToAnalyzePath;
        public string OutPath => projectConfig.OutPath;
        public string ProjectPath => projectConfig.ProjectPath;
        public string TargetFramework => projectConfig.TargetFramework;
        public ProjectType ProjectType => projectType.Value;

        public ProjectConfigReader(SourceText sonarProjectConfig, string logFileName)
        {
            projectConfig = sonarProjectConfig == null ? ProjectConfig.Empty : ReadContent(sonarProjectConfig, logFileName);
            projectType = new Lazy<ProjectType>(() => ParseProjectType());
        }

        private static ProjectConfig ReadContent(SourceText sonarProjectConfig, string logFileName)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(ProjectConfig));
                using var sr = new StringReader(sonarProjectConfig.ToString());
                return (ProjectConfig)serializer.Deserialize(sr);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"File {logFileName} could not be parsed.", e);
            }
        }

        private ProjectType ParseProjectType() =>
            Enum.TryParse<ProjectType>(projectConfig.ProjectType, out var result) ? result : ProjectType.Product;
    }
}

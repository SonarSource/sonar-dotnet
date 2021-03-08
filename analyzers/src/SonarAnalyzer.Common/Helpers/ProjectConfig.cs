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

using System.Xml.Serialization;

namespace SonarAnalyzer.Helpers
{
    /// <summary>
    /// Data class to describe a single project configuration for our analyzers.
    /// </summary>
    /// <remarks>
    /// This class is the counterpart of SonarScanner.MSBuild.Common.ProjectConfig, and is used for easy deserialization.
    /// This class should not be used in this codebase. To get configuration properties, use <see cref="ProjectConfigReader"/>.
    /// </remarks>
    [XmlRoot(ElementName = "SonarProjectConfig", Namespace = "http://www.sonarsource.com/msbuild/analyzer/2021/1")]
    public class ProjectConfig
    {
        public static readonly ProjectConfig Empty = new ProjectConfig();

        /// <summary>
        /// Full path to the SonarQubeAnalysisConfig.xml file.
        /// </summary>
        public string AnalysisConfigPath { get; set; }

        /// <summary>
        /// The full name and path of the project file.
        /// </summary>
        public string ProjectPath { get; set; }

        /// <summary>
        ///The full name and path of the text file containing all files to analyze.
        /// </summary>
        public string FilesToAnalyzePath { get; set; }

        /// <summary>
        /// Root of the project-specific output directory. Analyzer should write protobuf and other files there.
        /// </summary>
        public string OutPath { get; set; }

        /// <summary>
        /// The kind of the project.
        /// </summary>
        public string ProjectType { get; set; }

        /// <summary>
        /// MSBuild target framework for the current build.
        /// </summary>
        public string TargetFramework { get; set; }
    }
}

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

using System.Xml.Serialization;

namespace SonarAnalyzer.Helpers;

/// <summary>
/// Data class to describe an analysis configuration.
/// </summary>
/// <remarks>
/// This class is the counterpart of SonarScanner.MSBuild.Common.AnalysisConfig, and is used for easy deserialization.
/// This class should not be used in this codebase. To get configuration properties, use <see cref="AnalysisConfigReader"/>.
/// </remarks>
[XmlRoot(Namespace = "http://www.sonarsource.com/msbuild/integration/2015/1")]
public class AnalysisConfig
{
    public List<ConfigSetting> AdditionalConfig { get; set; }
}

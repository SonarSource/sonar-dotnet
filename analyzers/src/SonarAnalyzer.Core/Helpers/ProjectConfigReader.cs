/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Helpers;

/// <summary>
/// This class reads and encapsulates <see cref="ProjectConfig"/>, exposing only the configuration our analyzers need.
/// </summary>
public class ProjectConfigReader
{
    public static readonly ProjectConfigReader Empty = new(null);

    private readonly ProjectConfig projectConfig;
    private readonly Lazy<ProjectType> projectType;
    private readonly Lazy<FilesToAnalyzeProvider> filesToAnalyze;
    private readonly Lazy<AnalysisConfigReader> analysisConfig;

    public bool IsScannerRun => !string.IsNullOrEmpty(projectConfig.OutPath);
    public string AnalysisConfigPath => projectConfig.AnalysisConfigPath;
    public string FilesToAnalyzePath => projectConfig.FilesToAnalyzePath;
    public string OutPath => projectConfig.OutPath;
    public string ProjectPath => projectConfig.ProjectPath;
    public string TargetFramework => projectConfig.TargetFramework;
    public ProjectType ProjectType => projectType.Value;
    public FilesToAnalyzeProvider FilesToAnalyze => filesToAnalyze.Value;
    public AnalysisConfigReader AnalysisConfig => analysisConfig.Value;

    public ProjectConfigReader(SourceText sonarProjectConfig)
    {
        projectConfig = sonarProjectConfig == null ? ProjectConfig.Empty : ReadContent(sonarProjectConfig);
        projectType = new Lazy<ProjectType>(ParseProjectType);
        filesToAnalyze = new Lazy<FilesToAnalyzeProvider>(() => new FilesToAnalyzeProvider(FilesToAnalyzePath));
        analysisConfig = new(() => sonarProjectConfig == null ? null : new(AnalysisConfigPath));
    }

    private static ProjectConfig ReadContent(SourceText sonarProjectConfig)
    {
        try
        {
            return new(XDocument.Parse(sonarProjectConfig.ToString()));
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"{nameof(sonarProjectConfig)} could not be parsed.", e);
        }
    }

    private ProjectType ParseProjectType() =>
        Enum.TryParse<ProjectType>(projectConfig.ProjectType, out var result) ? result : ProjectType.Product;
}

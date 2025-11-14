/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
using SonarAnalyzer.CFG.Common;

namespace SonarAnalyzer.Core.Rules;

public abstract class AnalysisWarningAnalyzerBase : UtilityAnalyzerBase
{
    private const string DiagnosticId = "S9999-warning";
    private const string Title = "Analysis Warning generator";

    protected virtual int VS2017MajorVersion => RoslynVersion.VS2017MajorVersion; // For testing
    protected virtual int MinimalSupportedRoslynVersion => RoslynVersion.MinimalSupportedMajorVersion; // For testing

    protected AnalysisWarningAnalyzerBase() : base(DiagnosticId, Title) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationAction(c =>
            {
                var parameter = ReadParameters(c);
                if (!parameter.IsAnalyzerEnabled || parameter.OutPath is null)
                {
                    return;
                }

                var path = Path.GetFullPath(Path.Combine(parameter.OutPath, "../../AnalysisWarnings.MsBuild.json"));
                if (!File.Exists(path))
                {
                    // This can be removed after we bump Microsoft.CodeAnalysis references to 2.0 or higher. MsBuild 14 is bound with Roslyn 1.x.
                    if (RoslynVersion.IsVersionLessThan(VS2017MajorVersion))
                    {
                        WriteAllText(path, "The analysis using MsBuild 14 is no longer supported and the analysis with MsBuild 15 is deprecated. Please update your pipeline to MsBuild 16 or higher.");
                    }
                    // This can be removed after we bump Microsoft.CodeAnalysis references to 3.0 or higher. MsBuild 15 is bound with Roslyn 2.x.
                    else if (RoslynVersion.IsVersionLessThan(MinimalSupportedRoslynVersion))
                    {
                        WriteAllText(path, "The analysis using MsBuild 15 is deprecated. Please update your pipeline to MsBuild 16 or higher.");
                    }
                }
            });

    private static void WriteAllText(string path, string text)
    {
        try
        {
            File.WriteAllText(path, $$"""[{"text": "{{text}}"}]""");
        }
        catch
        {
            // Nothing to do here. Two compilations running on two different processes are unlikely to lock each other out on a small file write.
        }
    }
}

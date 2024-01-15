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
using SonarAnalyzer.CFG.Helpers;

namespace SonarAnalyzer.Rules;

public abstract class AnalysisWarningAnalyzerBase : UtilityAnalyzerBase
{
    private const string DiagnosticId = "S9999-warning";
    private const string Title = "Analysis Warning generator";

    protected virtual int MinimalSupportedRoslynVersion => RoslynHelper.MinimalSupportedMajorVersion;   // For testing

    protected AnalysisWarningAnalyzerBase() : base(DiagnosticId, Title) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationAction(c =>
            {
                var parameter = ReadParameters(c);
                if (parameter.IsAnalyzerEnabled && !RoslynHelper.IsRoslynCfgSupported(MinimalSupportedRoslynVersion))     // MsBuild 15 is bound with Roslyn 2.x, where Roslyn CFG is not available.
                {
                    // This can be removed after we bump Microsoft.CodeAnalysis references to 3.0 or higher.
                    var path = Path.GetFullPath(Path.Combine(parameter.OutPath, "../../AnalysisWarnings.MsBuild.json"));
                    if (!File.Exists(path))
                    {
                        try
                        {
                            File.WriteAllText(path, """[{"text": "Analysis using MsBuild 14 and 15 build tools is deprecated. Please update your pipeline to MsBuild 16 or higher."}]""");
                        }
                        catch
                        {
                            // Nothing to do here. Two compilations running on two different processes are unlikely to lock each other out on a small file write.
                        }
                    }
                }
            });
}

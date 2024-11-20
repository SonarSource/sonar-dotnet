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
using Google.Protobuf;
using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Rules;

public abstract class TelemetryAnalyzerBase<TSyntaxKind> : UtilityAnalyzerBase
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S9999-telemetry";
    private const string Title = "Telemetry generator";
    private const string FileName = "telemetry.pb";

    protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
    protected abstract string LanguageVersion(Compilation compilation);

    protected TelemetryAnalyzerBase() : base(DiagnosticId, Title) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(startContext =>
        {
            var parameters = ReadParameters(startContext);
            if (!parameters.IsAnalyzerEnabled)
            {
                return;
            }
            startContext.RegisterCompilationEndAction(endContext =>
            {
                Directory.CreateDirectory(parameters.OutPath);
                using var stream = File.Create(Path.Combine(parameters.OutPath, FileName));
                CreateTelemetry(endContext).WriteDelimitedTo(stream);
            });
        });

    private Telemetry CreateTelemetry(SonarCompilationReportingContext c)
    {
        var projectConfiguration = c.ProjectConfiguration();
        var telemetry = new Telemetry
        {
            ProjectFullPath = projectConfiguration.ProjectPath is { } path ? path : string.Empty,
            LanguageVersion = LanguageVersion(c.Compilation) is { } language ? language : string.Empty,
        };
        if (!string.IsNullOrEmpty(projectConfiguration.TargetFramework?.Trim()))
        {
            var sanitizedFrameworks = projectConfiguration.TargetFramework
                .Split([';'], StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x));
            telemetry.TargetFramework.AddRange(sanitizedFrameworks);
        }
        return telemetry;
    }
}

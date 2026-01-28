/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using System.IO;
using Google.Protobuf;
using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Core.Rules;

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
        return new Telemetry
        {
            ProjectFullPath = projectConfiguration.ProjectPath is { } path ? path : string.Empty,
            LanguageVersion = LanguageVersion(c.Compilation) is { } language ? language : string.Empty,
        };
    }
}

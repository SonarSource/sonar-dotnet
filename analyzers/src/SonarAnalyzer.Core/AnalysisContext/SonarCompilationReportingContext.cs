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

using System.IO;
using System.Text.RegularExpressions;

namespace SonarAnalyzer.AnalysisContext;

public sealed class SonarCompilationReportingContext : SonarCompilationReportingContextBase<CompilationAnalysisContext>
{
    private static readonly TimeSpan FileNameTimeout = TimeSpan.FromMilliseconds(100);
    private static readonly Regex WebConfigRegex = new(@"[\\\/]web\.([^\\\/]+\.)?config$", RegexOptions.IgnoreCase, FileNameTimeout);
    private static readonly Regex AppSettingsRegex = new(@"[\\\/]appsettings\.([^\\\/]+\.)?json$", RegexOptions.IgnoreCase, FileNameTimeout);

    public override Compilation Compilation => Context.Compilation;
    public override AnalyzerOptions Options => Context.Options;
    public override CancellationToken Cancel => Context.CancellationToken;

    internal SonarCompilationReportingContext(SonarAnalysisContext analysisContext, CompilationAnalysisContext context) : base(analysisContext, context) { }

    public IEnumerable<string> WebConfigFiles()
    {
        return ProjectConfiguration().FilesToAnalyze.FindFiles(WebConfigRegex).Where(ShouldProcess);

        static bool ShouldProcess(string path) =>
            !Path.GetFileName(path).Equals("web.debug.config", StringComparison.OrdinalIgnoreCase);
    }

    public IEnumerable<string> AppSettingsFiles()
    {
        return ProjectConfiguration().FilesToAnalyze.FindFiles(AppSettingsRegex).Where(ShouldProcess);

        static bool ShouldProcess(string path) =>
            !Path.GetFileName(path).Equals("appsettings.development.json", StringComparison.OrdinalIgnoreCase);
    }

    private protected override ReportingContext CreateReportingContext(Diagnostic diagnostic) =>
        new(this, diagnostic);
}

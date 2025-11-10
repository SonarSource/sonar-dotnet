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
using System.Text.RegularExpressions;

namespace SonarAnalyzer.Core.AnalysisContext;

public readonly record struct SonarCompilationReportingContext(SonarAnalysisContext AnalysisContext, CompilationAnalysisContext Context) : ICompilationReport, IAnalysisContext
{
    // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments#development-and-launchsettingsjson
    private const string LaunchSettingsFileName = "launchSettings.json";

    private static readonly TimeSpan FileNameTimeout = TimeSpan.FromMilliseconds(100);
    private static readonly Regex WebConfigRegex = new(@"[\\\/]web\.([^\\\/]+\.)?config$", RegexOptions.IgnoreCase, FileNameTimeout);
    private static readonly Regex AppSettingsRegex = new(@"[\\\/]appsettings\.([^\\\/]+\.)?json$", RegexOptions.IgnoreCase, FileNameTimeout);

    public Compilation Compilation => Context.Compilation;
    public AnalyzerOptions Options => Context.Options;
    public CancellationToken Cancel => Context.CancellationToken;

    public IEnumerable<string> WebConfigFiles()
    {
        return this.ProjectConfiguration().FilesToAnalyze.FindFiles(WebConfigRegex).Where(ShouldProcess);

        static bool ShouldProcess(string path) =>
            !Path.GetFileName(path).Equals("web.debug.config", StringComparison.OrdinalIgnoreCase);
    }

    public IEnumerable<string> AppSettingsFiles()
    {
        return this.ProjectConfiguration().FilesToAnalyze.FindFiles(AppSettingsRegex).Where(ShouldProcess);

        static bool ShouldProcess(string path) =>
            !Path.GetFileName(path).Equals("appsettings.development.json", StringComparison.OrdinalIgnoreCase);
    }

    public IEnumerable<string> LaunchSettingsFiles() =>
        this.ProjectConfiguration().FilesToAnalyze.FindFiles(LaunchSettingsFileName);

    public ReportingContext CreateReportingContext(Diagnostic diagnostic) =>
        new(this, diagnostic);

    public void ReportIssue(GeneratedCodeRecognizer generatedCodeRecognizer,
                        DiagnosticDescriptor rule,
                        Location primaryLocation,
                        IEnumerable<SecondaryLocation> secondaryLocations = null,
                        params string[] messageArgs)
    {
        if (this.ShouldAnalyzeTree(primaryLocation?.SourceTree, generatedCodeRecognizer))
        {
            var @this = this;
            secondaryLocations = secondaryLocations?.Where(x => x.Location.IsValid(@this.Compilation)).ToArray();
            IssueReporter.ReportIssueCore(
                Compilation,
                x => @this.HasMatchingScope(x),
                CreateReportingContext,
                rule,
                primaryLocation,
                secondaryLocations,
                ImmutableDictionary<string, string>.Empty,
                messageArgs);
        }
    }

    [Obsolete("Use another overload of ReportIssue, without calling Diagnostic.Create")]
    public void ReportIssue(GeneratedCodeRecognizer generatedCodeRecognizer, Diagnostic diagnostic)
    {
        if (this.ShouldAnalyzeTree(diagnostic.Location.SourceTree, generatedCodeRecognizer))
        {
            var @this = this;
            IssueReporter.ReportIssueCore(
                x => @this.HasMatchingScope(x),
                CreateReportingContext,
                diagnostic);
        }
    }
}

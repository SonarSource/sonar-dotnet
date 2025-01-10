/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.AnalysisContext;

// SyntaxTreeAnalysisContext doesn't hold a Compilation reference, we need to provide it from CompilationStart context via constructor
public readonly record struct SonarSyntaxTreeReportingContext(SonarAnalysisContext AnalysisContext, SyntaxTreeAnalysisContext Context, Compilation Compilation) : ITreeReport, IAnalysisContext
{
    public SyntaxTree Tree => Context.Tree;
    public AnalyzerOptions Options => Context.Options;
    public CancellationToken Cancel => Context.CancellationToken;

    public ReportingContext CreateReportingContext(Diagnostic diagnostic) =>
        new(this, diagnostic);

    public void ReportIssue(DiagnosticDescriptor rule,
                            Location primaryLocation,
                            IEnumerable<SecondaryLocation> secondaryLocations = null,
                            ImmutableDictionary<string, string> properties = null,
                            params string[] messageArgs)
    {
        var @this = this;
        IssueReporter.ReportIssueCore(
            Compilation,
            x => @this.HasMatchingScope(x),
            CreateReportingContext,
            rule,
            primaryLocation,
            secondaryLocations,
            properties,
            messageArgs);
    }

    [Obsolete("Use another overload of ReportIssue, without calling Diagnostic.Create")]
    public void ReportIssue(Diagnostic diagnostic)
    {
        var @this = this;
        IssueReporter.ReportIssueCore(
            x => @this.HasMatchingScope(x),
            CreateReportingContext,
            diagnostic);
    }
}

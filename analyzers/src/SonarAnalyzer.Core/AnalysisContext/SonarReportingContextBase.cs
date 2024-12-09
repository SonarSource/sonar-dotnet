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

namespace SonarAnalyzer.AnalysisContext;

public abstract class SonarReportingContextBase<TContext> : SonarAnalysisContextBase<TContext>, IReport
{
    public abstract ReportingContext CreateReportingContext(Diagnostic diagnostic);

    protected SonarReportingContextBase(SonarAnalysisContext analysisContext, TContext context) : base(analysisContext, context) { }
}

/// <summary>
/// Base class for reporting contexts that are executed on a known Tree. The decisions about generated code and unchanged files are taken during action registration.
/// </summary>
public abstract class SonarTreeReportingContextBase<TContext> : SonarReportingContextBase<TContext>, ITreeReport
{
    public abstract SyntaxTree Tree { get; }

    protected SonarTreeReportingContextBase(SonarAnalysisContext analysisContext, TContext context) : base(analysisContext, context) { }

    public void ReportIssue(DiagnosticDescriptor rule,
                             Location primaryLocation,
                             IEnumerable<SecondaryLocation> secondaryLocations = null,
                             ImmutableDictionary<string, string> properties = null,
                             params string[] messageArgs) =>
        IssueReporter.ReportIssueCore(
            Compilation,
            x => this.HasMatchingScope(x),
            CreateReportingContext,
            rule,
            primaryLocation,
            secondaryLocations,
            properties,
            messageArgs);

    [Obsolete("Use another overload of ReportIssue, without calling Diagnostic.Create")]
    public void ReportIssue(Diagnostic diagnostic) =>
        IssueReporter.ReportIssueCore(
            Compilation,
            x => this.HasMatchingScope(x),
            CreateReportingContext,
            diagnostic);
}

/// <summary>
/// Base class for reporting contexts that are common for the entire compilation. Specific tree is not known before the action is executed.
/// </summary>
public abstract class SonarCompilationReportingContextBase<TContext> : SonarReportingContextBase<TContext>, ICompilationReport
{
    protected SonarCompilationReportingContextBase(SonarAnalysisContext analysisContext, TContext context) : base(analysisContext, context) { }

    public void ReportIssue(GeneratedCodeRecognizer generatedCodeRecognizer, DiagnosticDescriptor rule, SyntaxNode locationSyntax, params string[] messageArgs) =>
        ReportIssue(generatedCodeRecognizer, rule, locationSyntax.GetLocation(), messageArgs);

    public void ReportIssue(GeneratedCodeRecognizer generatedCodeRecognizer, DiagnosticDescriptor rule, SyntaxToken locationToken, params string[] messageArgs) =>
        ReportIssue(generatedCodeRecognizer, rule, locationToken.GetLocation(), messageArgs);

    public void ReportIssue(GeneratedCodeRecognizer generatedCodeRecognizer, DiagnosticDescriptor rule, Location location, params string[] messageArgs) =>
        ReportIssue(generatedCodeRecognizer, rule, location, [], messageArgs);

    public void ReportIssue(GeneratedCodeRecognizer generatedCodeRecognizer,
                            DiagnosticDescriptor rule,
                            Location primaryLocation,
                            IEnumerable<SecondaryLocation> secondaryLocations = null,
                            params string[] messageArgs)
    {
        if (this.ShouldAnalyzeTree(primaryLocation?.SourceTree, generatedCodeRecognizer))
        {
            secondaryLocations = secondaryLocations?.Where(x => x.Location.IsValid(Compilation)).ToArray();
            IssueReporter.ReportIssueCore(
                Compilation,
                x => this.HasMatchingScope(x),
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
            IssueReporter.ReportIssueCore(Compilation, this.HasMatchingScope, CreateReportingContext, diagnostic);
        }
    }
}

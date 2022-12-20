/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer;

public abstract class SonarAnalysisContextBase
{
    private static readonly SourceTextValueProvider<ProjectConfigReader> ProjectConfigProvider = new(x => new ProjectConfigReader(x));

    public abstract bool TryGetValue<TValue>(SourceText text, SourceTextValueProvider<TValue> valueProvider, out TValue value);

    /// <summary>
    /// Reads configuration from SonarProjectConfig.xml file and caches the result for scope of this analysis.
    /// </summary>
    protected ProjectConfigReader ProjectConfiguration(AnalyzerOptions options)
    {
        if (options.SonarProjectConfig() is { } sonarProjectConfig)
        {
            return sonarProjectConfig.GetText() is { } sourceText
                // TryGetValue catches all exceptions from SourceTextValueProvider and returns false when exception is thrown
                && TryGetValue(sourceText, ProjectConfigProvider, out var cachedProjectConfigReader)
                ? cachedProjectConfigReader
                : throw new InvalidOperationException($"File '{Path.GetFileName(sonarProjectConfig.Path)}' has been added as an AdditionalFile but could not be read and parsed.");
        }
        else
        {
            return ProjectConfigReader.Empty;
        }
    }
}

public abstract class SonarAnalysisContextBase<TContext> : SonarAnalysisContextBase
{
    public abstract SyntaxTree Tree { get; }
    public abstract Compilation Compilation { get; }
    public abstract AnalyzerOptions Options { get; }

    public SonarAnalysisContext AnalysisContext { get; }
    public TContext Context { get; }

    protected SonarAnalysisContextBase(SonarAnalysisContext analysisContext, TContext context)
    {
        AnalysisContext = analysisContext ?? throw new ArgumentNullException(nameof(analysisContext));
        Context = context;
    }

    public override bool TryGetValue<TValue>(SourceText text, SourceTextValueProvider<TValue> valueProvider, out TValue value) =>
        AnalysisContext.TryGetValue(text, valueProvider, out value);

    public ProjectConfigReader ProjectConfiguration() =>
        ProjectConfiguration(Options);

    public bool IsTestProject()
    {
        var projectType = ProjectConfiguration(Options).ProjectType;
        return projectType == ProjectType.Unknown
            ? Compilation.IsTest()              // SonarLint, NuGet or Scanner <= 5.0
            : projectType == ProjectType.Test;  // Scanner >= 5.1 does authoritative decision that we follow
    }

    private protected void ReportIssue(ReportingContext reportingContext)
    {
        if (!SonarAnalysisContext.IsAnalysisScopeMatching(reportingContext.Compilation, IsTestProject(), ProjectConfiguration().IsScannerRun, new[] { reportingContext.Diagnostic.Descriptor }))
        {
            return;
        }

        if (reportingContext is { Compilation: { } compilation, Diagnostic.Location: { Kind: LocationKind.SourceFile, SourceTree: { } syntaxTree } }
            && !compilation.ContainsSyntaxTree(syntaxTree))
        {
            Debug.Fail("Primary location should be part of the compilation. An AD0001 is raised if this is not the case.");
            return;
        }

        // This is the current way SonarLint will handle how and what to report.
        if (SonarAnalysisContext.ReportDiagnostic != null)
        {
            Debug.Assert(SonarAnalysisContext.ShouldDiagnosticBeReported == null, "Not expecting SonarLint to set both the old and the new delegates.");
            SonarAnalysisContext.ReportDiagnostic(reportingContext);
            return;
        }

        // Standalone NuGet, Scanner run and SonarLint < 4.0 used with latest NuGet
        if (!VbcHelper.IsTriggeringVbcError(reportingContext.Diagnostic)
            && (SonarAnalysisContext.ShouldDiagnosticBeReported?.Invoke(reportingContext.SyntaxTree, reportingContext.Diagnostic) ?? true))
        {
            reportingContext.ReportDiagnostic(reportingContext.Diagnostic);
        }
    }
}

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

namespace SonarAnalyzer;

public abstract class SonarReportingContextBase<TContext> : SonarAnalysisContextBase<TContext>
{
    private protected abstract ReportingContext CreateReportingContext(Diagnostic diagnostic);

    protected SonarReportingContextBase(SonarAnalysisContext analysisContext, TContext context) : base(analysisContext, context) { }

    public void ReportIssue(Diagnostic diagnostic)  // FIXME: This is wrong design now, see next commit. This overload should not be accessible from Symbol-based and Compilation-based contexts
    {
        if (HasMatchingScope(diagnostic.Descriptor))
        {
            var reportingContext = CreateReportingContext(diagnostic);
            if (reportingContext is { Compilation: { } compilation, Diagnostic.Location: { Kind: LocationKind.SourceFile, SourceTree: { } tree } } && !compilation.ContainsSyntaxTree(tree))
            {
                Debug.Fail("Primary location should be part of the compilation. An AD0001 is raised if this is not the case.");
                return;
            }
            // This is the current way SonarLint will handle how and what to report.
            if (SonarAnalysisContext.ReportDiagnostic is not null)
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

    protected void ReportIssueCore(GeneratedCodeRecognizer generatedCodeRecognizer, Diagnostic diagnostic) // FIXME: this is wrong desing now, see next commit
    {
        if (ShouldAnalyzeTree(diagnostic.Location.SourceTree, Compilation, Options, generatedCodeRecognizer))
        {
            ReportIssue(diagnostic);
        }
    }
}

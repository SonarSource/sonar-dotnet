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

namespace SonarAnalyzer.AnalysisContext;

public sealed class SonarSyntaxTreeReportingContext : SonarTreeReportingContextBase<SyntaxTreeAnalysisContext>
{
    public override SyntaxTree Tree => Context.Tree;
    public override Compilation Compilation { get; }    // SyntaxTreeAnalysisContext doesn't hold a Compilation reference, we need to provide it from CompilationStart context via constructor
    public override AnalyzerOptions Options => Context.Options;
    public override CancellationToken Cancel => Context.CancellationToken;

    internal SonarSyntaxTreeReportingContext(SonarAnalysisContext analysisContext, SyntaxTreeAnalysisContext context, Compilation compilation) : base(analysisContext, context) =>
        Compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));

    private protected override ReportingContext CreateReportingContext(Diagnostic diagnostic) =>
        new(this, diagnostic);
}

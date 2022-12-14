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
using System.Text.RegularExpressions;

namespace SonarAnalyzer;

public sealed class SonarCompilationAnalysisContext : SonarAnalysisContextBase<CompilationAnalysisContext>
{
    private static readonly Regex WebConfigRegex = new(@"[\\\/]web\.([^\\\/]+\.)?config$", RegexOptions.IgnoreCase);
    private static readonly Regex AppSettingsRegex = new(@"[\\\/]appsettings\.([^\\\/]+\.)?json$", RegexOptions.IgnoreCase);

    public override SyntaxTree Tree => Context.GetFirstSyntaxTree();
    public override Compilation Compilation => Context.Compilation;
    public override AnalyzerOptions Options => Context.Options;

    internal SonarCompilationAnalysisContext(SonarAnalysisContext analysisContext, CompilationAnalysisContext context) : base(analysisContext, context) { }

    public void ReportIssue(Diagnostic diagnostic) =>
        ReportIssue(new ReportingContext(Context, diagnostic));

    public void ReportDiagnosticIfNonGenerated(GeneratedCodeRecognizer generatedCodeRecognizer, Diagnostic diagnostic)
    {
        if (ShouldAnalyze(generatedCodeRecognizer, diagnostic.Location.SourceTree, Compilation, Options))
        {
            ReportIssue(diagnostic);
        }
    }

    internal IEnumerable<string> WebConfigFiles()
    {
        return ProjectConfiguration().FilesToAnalyze.FindFiles(WebConfigRegex).Where(ShouldProcess);

        static bool ShouldProcess(string path) =>
            !Path.GetFileName(path).Equals("web.debug.config", StringComparison.OrdinalIgnoreCase);
    }

    internal IEnumerable<string> AppSettingsFiles()
    {
        return ProjectConfiguration().FilesToAnalyze.FindFiles(AppSettingsRegex).Where(ShouldProcess);

        static bool ShouldProcess(string path) =>
            !Path.GetFileName(path).Equals("appsettings.development.json", StringComparison.OrdinalIgnoreCase);
    }


}

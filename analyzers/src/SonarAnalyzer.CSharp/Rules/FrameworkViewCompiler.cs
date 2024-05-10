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
using Microsoft.AspNetCore.Razor.Language;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FrameworkViewCompiler : SonarDiagnosticAnalyzer
{
    private readonly HashSet<string> BadBoys = new()
    {
        "S3904",    // AssemblyVersion attribute
        "S3990",    // CLSCompliant attribute
        "S3992",    // ComVisible attribute
        "S1451",    // License header
        "S103",     // Lines too long
        "S104",     // Files too many lines
        "S109",     // Magic number
        "S113",     // Files without newline
        "S1147",    // Exit methods
        "S1192",    // String literals duplicated
        "S1944",    // Invalid cast
        "S1905",    // Redundant cast
        "S1116",    // Empty statements
    };

    private ImmutableArray<DiagnosticAnalyzer> Rules;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

    protected override bool EnableConcurrentExecution => false;

    public FrameworkViewCompiler()
    {
        Rules = RuleFinder2.CreateAnalyzers(AnalyzerLanguage.CSharp, false)
            .Where(x => x.SupportedDiagnostics.All(d => !BadBoys.Contains(d.Id)))
            .ToImmutableArray();

        SupportedDiagnostics = Rules.SelectMany(x => x.SupportedDiagnostics).ToImmutableArray();
    }

    protected override void Initialize(SonarAnalysisContext context) =>

        context.RegisterCompilationAction(
            c =>
            {
                // TODO: Maybe there is a better check, maybe use References(KnownAssembly for System.Web.Mvc)
                if (c.Compilation.GetTypeByMetadataName(KnownType.System_Web_Mvc_Controller) is null)
                {
                    return;
                }

                var projectConfiguration = c.ProjectConfiguration();
                var root = Path.GetDirectoryName(projectConfiguration.ProjectPath);
                var dummy = CompileViews(c.Compilation, root).WithAnalyzers(Rules, c.Options);
                var diagnostics = dummy.GetAnalyzerDiagnosticsAsync().Result;
                foreach (var diagnostic in diagnostics)
                {
                    c.ReportIssue(diagnostic);
                }
            });

    Compilation CompileViews(Compilation compilation, string rootDir)
    {
        FilesToAnalyzeProvider filesProvider = new(Directory.GetFiles(rootDir, "*.*", SearchOption.AllDirectories));
        var razorCompiler = new RazorCompiler(rootDir, filesProvider);
        var dummyCompilation = compilation;

        var documents = razorCompiler.CompileAll();
        var razorTrees = new List<SyntaxTree>();

        var i = 0;

        foreach (var razorDocument in documents)
        {
            if (razorDocument.GetCSharpDocument()?.GeneratedCode is { } csharpCode)
            {
                var razorTree = CSharpSyntaxTree.ParseText(
                    csharpCode,
                    new CSharpParseOptions(compilation.GetLanguageVersion()),
                    path: $"x_{i++}.cshtml.g.cs");
                razorTrees.Add(razorTree);
            }
        }
        dummyCompilation = dummyCompilation.AddSyntaxTrees(razorTrees);
        return dummyCompilation;
    }
}

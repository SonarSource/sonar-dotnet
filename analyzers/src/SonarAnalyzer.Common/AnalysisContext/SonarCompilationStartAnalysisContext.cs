/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

public sealed class SonarCompilationStartAnalysisContext : SonarAnalysisContextBase<CompilationStartAnalysisContext>
{
    public override Compilation Compilation => Context.Compilation;
    public override AnalyzerOptions Options => Context.Options;
    public override CancellationToken Cancel => Context.CancellationToken;

    internal SonarCompilationStartAnalysisContext(SonarAnalysisContext analysisContext, CompilationStartAnalysisContext context) : base(analysisContext, context) { }

    public void RegisterSymbolAction(Action<SonarSymbolReportingContext> action, params SymbolKind[] symbolKinds) =>
        Context.RegisterSymbolAction(x => action(new(AnalysisContext, x)), symbolKinds);

    public void RegisterCompilationEndAction(Action<SonarCompilationReportingContext> action) =>
        Context.RegisterCompilationEndAction(x => action(new(AnalysisContext, x)));

    public void RegisterSemanticModelAction(Action<SonarSemanticModelReportingContext> action) =>
        Context.RegisterSemanticModelAction(x => action(new(AnalysisContext, x)));

    public void RegisterNodeAction<TSyntaxKind>(GeneratedCodeRecognizer generatedCodeRecognizer, Action<SonarSyntaxNodeReportingContext> action, params TSyntaxKind[] syntaxKinds)
        where TSyntaxKind : struct
    {
        if (HasMatchingScope(AnalysisContext.SupportedDiagnostics))
        {
            var lastShouldAnalyze = default(Tuple<SyntaxTree, bool>);
            Context.RegisterSyntaxNodeAction(x =>
            {
                var tree = x.Node.SyntaxTree;
                var last = lastShouldAnalyze; // Make a local copy of the reference to avoid concurrency issues between the access of Item1 and Item2
                bool shouldAnalyze;
                if (ReferenceEquals(last?.Item1, tree))
                {
                    shouldAnalyze = last!.Item2;
                }
                else
                {
                    // Inlined from "Execute"
                    shouldAnalyze = ShouldAnalyzeTree(x.Node.SyntaxTree, generatedCodeRecognizer)
                                    && SonarAnalysisContext.LegacyIsRegisteredActionEnabled(AnalysisContext.SupportedDiagnostics, x.Node.SyntaxTree)
                                    && AnalysisContext.ShouldAnalyzeRazorFile(x.Node.SyntaxTree);
                    lastShouldAnalyze = new Tuple<SyntaxTree, bool>(tree, shouldAnalyze);
                }
                if (shouldAnalyze)
                {
                    action(new(AnalysisContext, x));
                }
            }, syntaxKinds);
        }
    }
}

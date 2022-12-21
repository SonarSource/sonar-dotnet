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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer;

public sealed partial /*FIXME: REMOVE partial */ class SonarAnalysisContext : SonarAnalysisContextBase
{
    private readonly AnalysisContext analysisContext;
    private readonly IEnumerable<DiagnosticDescriptor> supportedDiagnostics;

    /// <summary>
    /// This delegate is called on all specific contexts, after the registration to the <see cref="AnalysisContext"/>, to
    /// control whether or not the action should be executed.
    /// </summary>
    /// <remarks>
    /// This delegate is set by old SonarLint (from v4.0 to v5.5) when the project has the NuGet package installed to avoid
    /// duplicated analysis and issues. When both the NuGet and the VSIX are available, NuGet will take precedence and VSIX
    /// will be inhibited.
    /// This delegate was removed from SonarLint v6.0.
    /// </remarks>
    public static Func<IEnumerable<DiagnosticDescriptor>, SyntaxTree, bool> ShouldExecuteRegisteredAction { get; set; }

    /// <summary>
    /// This delegates control whether or not a diagnostic should be reported to Roslyn.
    /// </summary>
    /// <remarks>
    /// Currently this delegate is set by SonarLint (older than v4.0) to provide a suppression mechanism (i.e. specific issues turned off on the bound SonarQube).
    /// </remarks>
    public static Func<SyntaxTree, Diagnostic, bool> ShouldDiagnosticBeReported { get; set; }

    /// <summary>
    /// This delegate is used to supersede the default reporting action.
    /// When this delegate is set, the delegate set for <see cref="ShouldDiagnosticBeReported"/> is ignored.
    /// </summary>
    /// <remarks>
    /// Currently this delegate is set by SonarLint (4.0+) to control how the diagnostic should be reported to Roslyn (including not being reported).
    /// </remarks>
    public static Action<IReportingContext> ReportDiagnostic { get; set; }

    internal SonarAnalysisContext(AnalysisContext analysisContext, IEnumerable<DiagnosticDescriptor> supportedDiagnostics)
    {
        this.analysisContext = analysisContext ?? throw new ArgumentNullException(nameof(analysisContext));
        this.supportedDiagnostics = supportedDiagnostics ?? throw new ArgumentNullException(nameof(supportedDiagnostics));
    }

    public override bool TryGetValue<TValue>(SourceText text, SourceTextValueProvider<TValue> valueProvider, out TValue value) =>
        analysisContext.TryGetValue(text, valueProvider, out value);

    /// <summary>
    /// Legacy API for backward compatibility with SonarLint v4.0 - v5.5. See <see cref="ShouldExecuteRegisteredAction"/>.
    /// </summary>
    internal static bool LegacyIsRegisteredActionEnabled(IEnumerable<DiagnosticDescriptor> diagnostics, SyntaxTree tree) =>
        ShouldExecuteRegisteredAction == null || tree == null || ShouldExecuteRegisteredAction(diagnostics, tree);

    public void RegisterCodeBlockStartAction<TSyntaxKind>(Action<SonarCodeBlockStartAnalysisContext<TSyntaxKind>> action) where TSyntaxKind : struct =>
        analysisContext.RegisterCodeBlockStartAction<TSyntaxKind>(c => Execute<SonarCodeBlockStartAnalysisContext<TSyntaxKind>, CodeBlockStartAnalysisContext<TSyntaxKind>>(new(this, c), action));

    public void RegisterCompilationAction(Action<SonarCompilationAnalysisContext> action) =>
        analysisContext.RegisterCompilationAction(c => Execute<SonarCompilationAnalysisContext, CompilationAnalysisContext>(new(this, c), action));

    public void RegisterCompilationStartAction(Action<SonarCompilationStartAnalysisContext> action) =>
        analysisContext.RegisterCompilationStartAction(c => Execute<SonarCompilationStartAnalysisContext, CompilationStartAnalysisContext>(new(this, c), action));

    public void RegisterSymbolAction(Action<SonarSymbolAnalysisContext> action, params SymbolKind[] symbolKinds) =>
        analysisContext.RegisterSymbolAction(c => Execute<SonarSymbolAnalysisContext, SymbolAnalysisContext>(new(this, c), action), symbolKinds);

    public void RegisterSyntaxNodeAction<TSyntaxKind>(Action<SonarSyntaxNodeAnalysisContext> action, params TSyntaxKind[] syntaxKinds) where TSyntaxKind : struct =>
        analysisContext.RegisterSyntaxNodeAction(c => Execute<SonarSyntaxNodeAnalysisContext, SyntaxNodeAnalysisContext>(new(this, c), action), syntaxKinds);

    public void RegisterSyntaxTreeAction(Action<SonarSyntaxTreeAnalysisContext> action) =>
        analysisContext.RegisterCompilationStartAction(WrapSyntaxTreeAction(action));

    public Action<CompilationStartAnalysisContext> WrapSyntaxTreeAction(Action<SonarSyntaxTreeAnalysisContext> action) =>
        c => c.RegisterSyntaxTreeAction(treeContext => Execute<SonarSyntaxTreeAnalysisContext, SyntaxTreeAnalysisContext>(new(this, treeContext, c.Compilation), action));

    private void Execute<TSonarContext, TRoslynContext>(TSonarContext context, Action<TSonarContext> action) where TSonarContext : SonarAnalysisContextBase<TRoslynContext>
    {
        // For each action registered on context we need to do some pre-processing before actually calling the rule.
        // First, we need to ensure the rule does apply to the current scope (main vs test source).
        // Second, we call an external delegate (set by legacy SonarLint for VS) to ensure the rule should be run (usually
        // the decision is made on based on whether the project contains the analyzer as NuGet).
        if (supportedDiagnostics.Any(x => x.HasMatchingScope(context.Compilation, context.IsTestProject(), context.IsScannerRun()))
            && LegacyIsRegisteredActionEnabled(supportedDiagnostics, context.Tree))
        {
            action(context);
        }
    }
}

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

using Microsoft.CodeAnalysis.Text;
using RoslynAnalysisContext = Microsoft.CodeAnalysis.Diagnostics.AnalysisContext;

namespace SonarAnalyzer.AnalysisContext;

public class SonarAnalysisContext
{
    internal ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

    private readonly HashSet<string> rulesDisabledForRazor = new()
        {
            "S103",
            "S104",
            "S109",
            "S113",
            "S1147",
            "S1192",
            "S1451",
        };

    private readonly RoslynAnalysisContext analysisContext;

    /// <summary>
    /// This delegate is called on all specific contexts, after the registration to the <see cref="RoslynAnalysisContext"/>, to
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

    internal SonarAnalysisContext(RoslynAnalysisContext analysisContext, ImmutableArray<DiagnosticDescriptor> supportedDiagnostics)
    {
        this.analysisContext = analysisContext ?? throw new ArgumentNullException(nameof(analysisContext));
        SupportedDiagnostics = supportedDiagnostics;
    }

    private protected SonarAnalysisContext(SonarAnalysisContext context) : this(context.analysisContext, context.SupportedDiagnostics) { }

    public bool TryGetValue<TValue>(SourceText text, SourceTextValueProvider<TValue> valueProvider, out TValue value) =>
        analysisContext.TryGetValue(text, valueProvider, out value);

    /// <summary>
    /// Legacy API for backward compatibility with SonarLint v4.0 - v5.5. See <see cref="ShouldExecuteRegisteredAction"/>.
    /// </summary>
    internal static bool LegacyIsRegisteredActionEnabled(IEnumerable<DiagnosticDescriptor> diagnostics, SyntaxTree tree) =>
        ShouldExecuteRegisteredAction == null || tree == null || ShouldExecuteRegisteredAction(diagnostics, tree);

    /// <summary>
    /// Legacy API for backward compatibility with SonarLint v4.0 - v5.5. See <see cref="ShouldExecuteRegisteredAction"/>.
    /// </summary>
    internal static bool LegacyIsRegisteredActionEnabled(DiagnosticDescriptor diagnostic, SyntaxTree tree) =>
        ShouldExecuteRegisteredAction == null || tree == null || ShouldExecuteRegisteredAction(new[] { diagnostic }, tree);

    public void RegisterCodeBlockStartAction<TSyntaxKind>(GeneratedCodeRecognizer generatedCodeRecognizer, Action<SonarCodeBlockStartAnalysisContext<TSyntaxKind>> action)
        where TSyntaxKind : struct =>
        analysisContext.RegisterCodeBlockStartAction<TSyntaxKind>(
            c => Execute(new(this, c), action, c.CodeBlock.SyntaxTree, generatedCodeRecognizer));

    public void RegisterCompilationAction(Action<SonarCompilationReportingContext> action) =>
        analysisContext.RegisterCompilationAction(
            c => Execute(new(this, c), action, null));

    public virtual void RegisterCompilationStartAction(Action<SonarCompilationStartAnalysisContext> action) =>
        analysisContext.RegisterCompilationStartAction(
            c => Execute(new(this, c), action, null));

    public void RegisterSymbolAction(Action<SonarSymbolReportingContext> action, params SymbolKind[] symbolKinds) =>
        RegisterCompilationStartAction(
            c => c.RegisterSymbolAction(action, symbolKinds));

    public void RegisterNodeAction<TSyntaxKind>(GeneratedCodeRecognizer generatedCodeRecognizer, Action<SonarSyntaxNodeReportingContext> action, params TSyntaxKind[] syntaxKinds)
        where TSyntaxKind : struct =>
        RegisterCompilationStartAction(
            c => c.RegisterNodeAction(generatedCodeRecognizer, action, syntaxKinds));

    public void RegisterSemanticModelAction(GeneratedCodeRecognizer generatedCodeRecognizer, Action<SonarSemanticModelReportingContext> action) =>
        RegisterCompilationStartAction(c =>
            c.RegisterSemanticModelAction(generatedCodeRecognizer, action));

    public void RegisterTreeAction(GeneratedCodeRecognizer generatedCodeRecognizer, Action<SonarSyntaxTreeReportingContext> action) =>
        analysisContext.RegisterCompilationStartAction(
            c => c.RegisterSyntaxTreeAction(
                treeContext => Execute(new(this, treeContext, c.Compilation), action, treeContext.Tree, generatedCodeRecognizer)));

    /// <summary>
    /// Register action for a SyntaxNode that is executed unconditionally:
    /// * For all non-generated code.
    /// * For all generated code.
    /// * For all unchanged files under PR analysis.
    /// This should NOT be used for actions that report issues.
    /// </summary>
    public void RegisterNodeActionInAllFiles<TSyntaxKind>(Action<SonarSyntaxNodeReportingContext> action, params TSyntaxKind[] syntaxKinds) where TSyntaxKind : struct =>
        analysisContext.RegisterSyntaxNodeAction(c => action(new(this, c)), syntaxKinds);

    internal bool ShouldAnalyzeRazorFile(SyntaxTree sourceTree) =>
        !GeneratedCodeRecognizer.IsRazorGeneratedFile(sourceTree)
        || !SupportedDiagnostics.Any(x => (x.CustomTags.Count() == 1 && x.CustomTags.Contains(DiagnosticDescriptorFactory.TestSourceScopeTag))
                                          || rulesDisabledForRazor.Contains(x.Id));

    /// This method will be replaced by <see cref="SonarCompilationStartAnalysisContext.Execute"/> in the future.
    /// <param name="sourceTree">Tree that is definitely known to be analyzed. Pass 'null' if the context doesn't know a specific tree to be analyzed, like a CompilationContext.</param>
    private void Execute<TSonarContext>(TSonarContext context, Action<TSonarContext> action, SyntaxTree sourceTree, GeneratedCodeRecognizer generatedCodeRecognizer = null)
        where TSonarContext : IAnalysisContext
    {
        // For each action registered on context we need to do some pre-processing before actually calling the rule.
        // First, we need to ensure the rule does apply to the current scope (main vs test source).
        // Second, we call an external delegate (set by legacy SonarLint for VS) to ensure the rule should be run (usually
        // the decision is made on based on whether the project contains the analyzer as NuGet).
        if (context.HasMatchingScope(SupportedDiagnostics)
            && context.ShouldAnalyzeTree(sourceTree, generatedCodeRecognizer)
            && LegacyIsRegisteredActionEnabled(SupportedDiagnostics, sourceTree)
            && ShouldAnalyzeRazorFile(sourceTree))
        {
            action(context);
        }
    }
}

/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.Core.AnalysisContext;

public class SonarAnalysisContext
{
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

    internal ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

    internal SonarAnalysisContext(RoslynAnalysisContext analysisContext, ImmutableArray<DiagnosticDescriptor> supportedDiagnostics)
    {
        this.analysisContext = analysisContext ?? throw new ArgumentNullException(nameof(analysisContext));
        SupportedDiagnostics = supportedDiagnostics;
    }

    private protected SonarAnalysisContext(SonarAnalysisContext context) : this(context.analysisContext, context.SupportedDiagnostics) { }

    public bool TryGetValue<TValue>(SourceText text, SourceTextValueProvider<TValue> valueProvider, out TValue value) =>
        analysisContext.TryGetValue(text, valueProvider, out value);

    public void RegisterCodeBlockStartAction<TSyntaxKind>(GeneratedCodeRecognizer generatedCodeRecognizer, Action<SonarCodeBlockStartAnalysisContext<TSyntaxKind>> action)
        where TSyntaxKind : struct =>
        RegisterCompilationStartAction(
            x => x.RegisterCodeBlockStartAction(generatedCodeRecognizer, action));

    public void RegisterCompilationAction(Action<SonarCompilationReportingContext> action) =>
        analysisContext.RegisterCompilationAction(
            x => Execute(new(this, x), action));

    public virtual void RegisterCompilationStartAction(Action<SonarCompilationStartAnalysisContext> action) =>
        analysisContext.RegisterCompilationStartAction(
            x => Execute(new(this, x), action));

    public void RegisterSymbolAction(Action<SonarSymbolReportingContext> action, params SymbolKind[] symbolKinds) =>
        RegisterCompilationStartAction(
            x => x.RegisterSymbolAction(action, symbolKinds));

    public void RegisterSymbolStartAction(Action<SonarSymbolStartAnalysisContext> action, SymbolKind symbolKind) =>
        RegisterCompilationStartAction(
            x => x.RegisterSymbolStartAction(action, symbolKind));

    public void RegisterNodeAction<TSyntaxKind>(GeneratedCodeRecognizer generatedCodeRecognizer, Action<SonarSyntaxNodeReportingContext> action, params TSyntaxKind[] syntaxKinds)
        where TSyntaxKind : struct =>
        RegisterCompilationStartAction(
            x => x.RegisterNodeAction(generatedCodeRecognizer, action, syntaxKinds));

    public void RegisterSemanticModelAction(GeneratedCodeRecognizer generatedCodeRecognizer, Action<SonarSemanticModelReportingContext> action) =>
        RegisterCompilationStartAction(
            x => x.RegisterSemanticModelAction(generatedCodeRecognizer, action));

    public void RegisterTreeAction(GeneratedCodeRecognizer generatedCodeRecognizer, Action<SonarSyntaxTreeReportingContext> action) =>
        RegisterCompilationStartAction(
            x => x.RegisterTreeAction(generatedCodeRecognizer, action));

    /// <summary>
    /// Register action for a SyntaxNode that is executed unconditionally:
    /// <list type="bullet">
    /// <item>For all non-generated code.</item>
    /// <item>For all generated code.</item>
    /// <item>For all unchanged files under PR analysis.</item>
    /// </list>
    /// This should NOT be used for actions that report issues.
    /// </summary>
    public void RegisterNodeActionInAllFiles<TSyntaxKind>(Action<SonarSyntaxNodeReportingContext> action, params TSyntaxKind[] syntaxKinds) where TSyntaxKind : struct =>
        analysisContext.RegisterSyntaxNodeAction(x => action(new(this, x)), syntaxKinds);

    /// <summary>
    /// Legacy API for backward compatibility with SonarLint v4.0 - v5.5. See <see cref="ShouldExecuteRegisteredAction"/>.
    /// </summary>
    internal static bool LegacyIsRegisteredActionEnabled(IEnumerable<DiagnosticDescriptor> diagnostics, SyntaxTree tree) =>
        ShouldExecuteRegisteredAction is null || tree is null || ShouldExecuteRegisteredAction(diagnostics, tree);

    /// <summary>
    /// Legacy API for backward compatibility with SonarLint v4.0 - v5.5. See <see cref="ShouldExecuteRegisteredAction"/>.
    /// </summary>
    internal static bool LegacyIsRegisteredActionEnabled(DiagnosticDescriptor diagnostic, SyntaxTree tree) =>
        ShouldExecuteRegisteredAction is null || tree is null || ShouldExecuteRegisteredAction(new[] { diagnostic }, tree);

    private void Execute<TSonarContext>(TSonarContext context, Action<TSonarContext> action)
        where TSonarContext : IAnalysisContext // Generic specialization: The JIT emmits a specialized version of Execute() for each struct TSonarContext, which means it gets called without boxing.
    {
        // For each action registered on context we need to do some pre-processing before actually calling the rule.
        // We need to ensure the rule does apply to the current scope (main vs test source).
        // Further checks are done in SonarCompilationStartAnalysisContext for registrations that have a syntax tree.
        if (context.HasMatchingScope(SupportedDiagnostics))
        {
            action(context);
        }
    }
}

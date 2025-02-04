using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.Core.AnalysisContext;

class UseInnermostRegistrationContext
{
    void SonarAnalysisContext(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStart => context.RegisterTreeAction(null, treeAction => { })); // Noncompliant {{Use inner-most registration context 'compilationStart' instead of 'context'.}}
        //                                                         ^^^^^^^

    void SonarCompilationStartAnalysisContext(SonarCompilationStartAnalysisContext compilationStart) =>
        compilationStart.RegisterTreeAction(null, tree => _ = compilationStart.Cancel); // Noncompliant {{Use inner-most registration context 'tree' instead of 'compilationStart'.}}
        //                                                    ^^^^^^^^^^^^^^^^

    void SonarCodeBlockStartAnalysisContext(SonarCodeBlockStartAnalysisContext<SyntaxKind> sonarCodeBlockStart) =>
        sonarCodeBlockStart.RegisterCodeBlockEndAction(end => _ = sonarCodeBlockStart.CodeBlock); // Noncompliant {{Use inner-most registration context 'end' instead of 'sonarCodeBlockStart'.}}

    void SonarSymbolStartAnalysisContext(SonarSymbolStartAnalysisContext symbolStart) =>
        symbolStart.RegisterSymbolEndAction(end => _ = symbolStart.Symbol); // Noncompliant {{Use inner-most registration context 'end' instead of 'symbolStart'.}}

    void SonarParametrizedAnalysisContext(SonarParametrizedAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStart => context.RegisterTreeAction(null, treeAction => { })); // Noncompliant {{Use inner-most registration context 'compilationStart' instead of 'context'.}}

    void NestedAndDuplicated(SonarAnalysisContext context)
    {
        context.RegisterCompilationStartAction(compilationStart =>
        {
            context.RegisterCompilationStartAction(innerCompilationStart => { }); // Noncompliant
            _ = compilationStart.Cancel;                                          // Compliant
            compilationStart.RegisterTreeAction(null, treeAction =>
            {
                context.RegisterTreeAction(null, innerTreeAction => { });         // Noncompliant {{Use inner-most registration context 'treeAction' instead of 'context'.}}
                _ = compilationStart.Cancel;                                      // Noncompliant
                _ = treeAction.Cancel;                                            // Compliant
            });
            compilationStart.RegisterCompilationEndAction(end =>
            {
                compilationStart.RegisterCompilationEndAction(innerEnd => { });   // Noncompliant {{Use inner-most registration context 'end' instead of 'compilationStart'.}}
                _ = compilationStart.Cancel;                                      // Noncompliant
                _ = end.Cancel;                                                   // Compliant
            });
            compilationStart.RegisterSymbolStartAction(symbolStart => symbolStart.RegisterCodeBlockStartAction<SyntaxKind>(codeBlockStart => codeBlockStart.RegisterNodeAction(node =>
            {
                context.RegisterTreeAction(null, _ => { });                       // Noncompliant {{Use inner-most registration context 'node' instead of 'context'.}}
                _ = compilationStart.Cancel;                                      // Noncompliant {{Use inner-most registration context 'node' instead of 'compilationStart'.}}
                _ = symbolStart.Cancel;                                           // Noncompliant {{Use inner-most registration context 'node' instead of 'symbolStart'.}}
                _ = codeBlockStart.Cancel;                                        // Noncompliant {{Use inner-most registration context 'node' instead of 'codeBlockStart'.}}
                _ = node.Cancel;                                                  // Compliant
            })), SymbolKind.NamedType);
        });
    }

    void TwoContextInOutterScope(SonarAnalysisContext context, SonarCompilationStartAnalysisContext compilationStart, int p)
    {
        context.RegisterCompilationStartAction(innerCompilationStart => { });                             // Compliant
        _ = compilationStart.Cancel;                                                                      // Compliant
        context.RegisterCompilationStartAction(_ => context.RegisterTreeAction(null, treeAction => { })); // Noncompliant {{Use inner-most registration context '_' instead of 'context'.}}
        compilationStart.RegisterTreeAction(null, _ =>
        {
            context.RegisterTreeAction(null, _ => { });                                                   // Noncompliant {{Use inner-most registration context '_' instead of 'context'.}}
            var c1 = compilationStart.Cancel;                                                             // Noncompliant
            var c2 = _.Cancel;                                                                            // Compliant
        });
    }
}

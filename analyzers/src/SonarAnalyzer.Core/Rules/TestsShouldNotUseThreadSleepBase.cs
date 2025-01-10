/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Rules;

public abstract class TestsShouldNotUseThreadSleepBase<TMethodSyntax, TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TMethodSyntax : SyntaxNode
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S2925";

    protected override string MessageFormat => "Do not use 'Thread.Sleep()' in a test.";

    protected abstract SyntaxNode MethodDeclaration(TMethodSyntax method);

    protected TestsShouldNotUseThreadSleepBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            if (c.Node.ToStringContains(nameof(Thread.Sleep), Language.NameComparison)
                && c.SemanticModel.GetSymbolInfo(c.Node).Symbol is IMethodSymbol method
                && method.Is(KnownType.System_Threading_Thread, nameof(Thread.Sleep))
                && IsInTestMethod(c.Node, c.SemanticModel))
            {
                c.ReportIssue(Rule, c.Node);
            }
        },
        Language.SyntaxKind.InvocationExpression);

    private bool IsInTestMethod(SyntaxNode node, SemanticModel model) =>
        node.Ancestors().OfType<TMethodSyntax>().FirstOrDefault() is { } method
        && model.GetDeclaredSymbol(MethodDeclaration(method)) is IMethodSymbol symbol
        && symbol.IsTestMethod();
}

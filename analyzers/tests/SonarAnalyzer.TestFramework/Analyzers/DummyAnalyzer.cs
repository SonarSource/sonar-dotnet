/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.Core.AnalysisContext;
using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.TestFramework.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DummyAnalyzerCS : DummyAnalyzer<CS.SyntaxKind>
{
    protected override CS.SyntaxKind NumericLiteralExpression => CS.SyntaxKind.NumericLiteralExpression;
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DummyAnalyzerThatThrowsCS : DummyAnalyzerThatThrows<CS.SyntaxKind>
{
    protected override CS.SyntaxKind NumericLiteralExpression => CS.SyntaxKind.NumericLiteralExpression;
}

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public class DummyAnalyzerVB : DummyAnalyzer<VB.SyntaxKind>
{
    protected override VB.SyntaxKind NumericLiteralExpression => VB.SyntaxKind.NumericLiteralExpression;
}

public abstract class DummyAnalyzer<TSyntaxKind> : SonarDiagnosticAnalyzer where TSyntaxKind : struct
{
    private readonly DiagnosticDescriptor rule = AnalysisScaffolding.CreateDescriptorMain("SDummy");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);
    public int DummyProperty { get; set; }

    protected abstract TSyntaxKind NumericLiteralExpression { get; }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(TestGeneratedCodeRecognizer.Instance, c => c.ReportIssue(rule, c.Node), NumericLiteralExpression);
}

public abstract class DummyAnalyzerThatThrows<TSyntaxKind> : SonarDiagnosticAnalyzer where TSyntaxKind : struct
{
    private readonly DiagnosticDescriptor rule = AnalysisScaffolding.CreateDescriptorMain("SDummyThatThrows");

    protected abstract TSyntaxKind NumericLiteralExpression { get; }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [rule];

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(TestGeneratedCodeRecognizer.Instance, c => throw new NotSupportedException(), NumericLiteralExpression);
}

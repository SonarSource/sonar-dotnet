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

using SonarAnalyzer.Core.AnalysisContext;
using SonarAnalyzer.Core.Syntax.Utilities;

namespace SonarAnalyzer.TestFramework.Analyzers;

public abstract class TestAnalyzer : SonarDiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor Rule = AnalysisScaffolding.CreateDescriptorMain("STestAnalyzer");
    protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TestAnalyzerCS : TestAnalyzer
{
    private readonly Action<SonarAnalysisContext, GeneratedCodeRecognizer> initializeAction;

    protected override GeneratedCodeRecognizer GeneratedCodeRecognizer => TestGeneratedCodeRecognizer.Instance;

    public TestAnalyzerCS(Action<SonarAnalysisContext, GeneratedCodeRecognizer> action) =>
        initializeAction = action;

    protected override void Initialize(SonarAnalysisContext context) =>
        initializeAction(context, GeneratedCodeRecognizer);
}

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public class TestAnalyzerVB : TestAnalyzer
{
    private readonly Action<SonarAnalysisContext, GeneratedCodeRecognizer> initializeAction;

    protected override GeneratedCodeRecognizer GeneratedCodeRecognizer => TestGeneratedCodeRecognizer.Instance;

    public TestAnalyzerVB(Action<SonarAnalysisContext, GeneratedCodeRecognizer> action) =>
        initializeAction = action;

    protected override void Initialize(SonarAnalysisContext context) =>
        initializeAction(context, GeneratedCodeRecognizer);
}

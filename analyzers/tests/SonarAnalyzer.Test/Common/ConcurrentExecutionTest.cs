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

using SonarAnalyzer.AnalysisContext;

namespace SonarAnalyzer.Test.Common;

[TestClass]
public class ConcurrentExecutionTest
{
    [TestMethod]
    public void Verify_ConcurrentExecutionIsEnabledByDefault()
    {
        var reader = new ConcurrentExecutionReader();
        reader.IsConcurrentExecutionEnabled.Should().BeNull();
        VerifyNoExceptionThrown("TestCases\\AsyncVoidMethod.cs", [reader]);
        reader.IsConcurrentExecutionEnabled.Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("true")]
    [DataRow("tRUE")]
    [DataRow("loremipsum")]
    public void Verify_ConcurrentExecutionIsExplicitlyEnabled(string value)
    {
        using var scope = new EnvironmentVariableScope(false);
        scope.SetVariable(SonarDiagnosticAnalyzer.EnableConcurrentExecutionVariable, value);
        var reader = new ConcurrentExecutionReader();
        reader.IsConcurrentExecutionEnabled.Should().BeNull();
        VerifyNoExceptionThrown("TestCases\\AsyncVoidMethod.cs", [reader]);
        reader.IsConcurrentExecutionEnabled.Should().BeTrue();
    }

    [TestMethod]
    [DataRow("false")]
    [DataRow("fALSE")]
    public void Verify_ConcurrentExecutionIsExplicitlyDisabled(string value)
    {
        using var scope = new EnvironmentVariableScope(false);
        scope.SetVariable(SonarDiagnosticAnalyzer.EnableConcurrentExecutionVariable, value);
        var reader = new ConcurrentExecutionReader();
        reader.IsConcurrentExecutionEnabled.Should().BeNull();
        VerifyNoExceptionThrown("TestCases\\AsyncVoidMethod.cs", [reader]);
        reader.IsConcurrentExecutionEnabled.Should().BeFalse();
    }

    private static void VerifyNoExceptionThrown(string path, DiagnosticAnalyzer[] analyzers, CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default)
    {
        var compilation = SolutionBuilder
            .Create()
            .AddProject(AnalyzerLanguage.FromPath(path))
            .AddDocument(path)
            .GetCompilation();
        ((Action)(() => DiagnosticVerifier.AnalyzerDiagnostics(compilation, analyzers, checkMode))).Should().NotThrow();
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    private class ConcurrentExecutionReader : SonarDiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorFactory.CreateUtility("S9999", "Rule test");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];
        public new bool? IsConcurrentExecutionEnabled { get; private set; }

        protected override void Initialize(SonarAnalysisContext context) =>
            IsConcurrentExecutionEnabled = EnableConcurrentExecution;
    }
}

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

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.AnalysisContext;
using SonarAnalyzer.Analyzers;
using SonarAnalyzer.TestFramework.Verification;

namespace SonarAnalyzer.Test.TestFramework.Tests.Verification;

[TestClass]
public class CodeFixProviderTest
{
    [TestMethod]
    public void VerifyCodeFix_WithDuplicateIssues()
    {
        const string filename = "VerifyCodeFix.Empty.cs";
        var verifier = new VerifierBuilder<TestDuplicateLocationRule>()
            .WithCodeFix<TestDuplicateLocationRuleCodeFix>()
            .AddPaths(filename)
            .WithCodeFixedPaths(filename);
        Action a = () => verifier.VerifyCodeFix();
        a.Should().NotThrow();
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    private class TestDuplicateLocationRule : SonarDiagnosticAnalyzer
    {
        public const string DiagnosticId = "Test42";

        private readonly DiagnosticDescriptor rule = AnalysisScaffolding.CreateDescriptorMain(DiagnosticId);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(TestGeneratedCodeRecognizer.Instance, c =>
            {
                // Duplicate issues from different analyzer versions, see https://github.com/SonarSource/sonar-dotnet/issues/1109
                c.ReportIssue(rule, c.Context.Node);
                c.ReportIssue(rule, c.Context.Node);
            }, SyntaxKind.NamespaceDeclaration);
    }

    [ExportCodeFixProvider(LanguageNames.CSharp)]
    private class TestDuplicateLocationRuleCodeFix : SonarCodeFix
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(TestDuplicateLocationRule.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            context.RegisterCodeFix("TestTitle", c => Task.FromResult(context.Document), context.Diagnostics);
            return Task.CompletedTask;
        }
    }
}

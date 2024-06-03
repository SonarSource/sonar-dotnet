/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.AnalysisContext;
using SonarAnalyzer.Analyzers;
using SonarAnalyzer.TestFramework.Verification;

namespace SonarAnalyzer.Test.TestFramework.Tests.Verification;

[TestClass]
public class CodeFixProviderSnippetTest
{
    [TestMethod]
    public void VerifyCodeFix_Snippet()
    {
        var verifier = new VerifierBuilder
        {
            Analyzers = [() => new TestRule([SyntaxKind.ClassDeclaration])],
            CodeFix = () => new TestRuleCodeFix(async (document, node) =>
                document.WithSyntaxRoot(
                    (await node.SyntaxTree.GetRootAsync()).ReplaceNode(node, node is ClassDeclarationSyntax classDeclaration
                    ? classDeclaration.WithIdentifier(SyntaxFactory.Identifier("Changed").WithTriviaFrom(classDeclaration.Identifier))
                    : node))),
        }
        .AddSnippet("""
        class C { }
        """)
        .WithCodeFixedSnippet(
        """
        class Changed { }
        """);
        Action a = () => verifier.VerifyCodeFix();
        a.Should().NotThrow();
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TestRule : SonarDiagnosticAnalyzer
    {
        public const string DiagnosticId = "TEST001";

        private readonly DiagnosticDescriptor rule = AnalysisScaffolding.CreateDescriptorMain(DiagnosticId);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [rule];

        public SyntaxKind[] RegisterSyntaxKinds { get; }
        public Func<SyntaxNode, bool> Filter { get; }

        public TestRule(SyntaxKind[] registerSyntaxKinds, Func<SyntaxNode, bool> filter = null)
        {
            RegisterSyntaxKinds = registerSyntaxKinds;
            Filter = filter ?? (_ => true);
        }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(TestGeneratedCodeRecognizer.Instance, c =>
            {
                if (Filter(c.Node))
                {
                    c.ReportIssue(rule, c.Context.Node);
                }
            }, [..RegisterSyntaxKinds]);
    }

    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class TestRuleCodeFix : SonarCodeFix
    {
        public override ImmutableArray<string> FixableDiagnosticIds => [TestRule.DiagnosticId];

        public Func<Document, SyntaxNode, Task<Document>> CreateChangedDocument { get; }

        public TestRuleCodeFix(Func<Document, SyntaxNode, Task<Document>> createChangedDocument)
        {
            CreateChangedDocument = createChangedDocument;
        }
        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            context.RegisterCodeFix("TestTitle", async c => await CreateChangedDocument(
                context.Document,
                (await context.Document.GetSyntaxRootAsync(c)).FindNode(context.Span)),
                context.Diagnostics);
            return Task.CompletedTask;
        }
    }
}

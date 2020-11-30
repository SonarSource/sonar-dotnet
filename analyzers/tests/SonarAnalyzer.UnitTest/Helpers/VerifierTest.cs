/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

extern alias csharp;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    [TestClass]
    public class VerifierTest
    {
        [TestMethod]
        public void VerifyCodeFix_WithDuplicateIssues()
        {
            const string filename = @"TestCases\VerifyCodeFix.Empty.cs";
            Action a = () => { Verifier.VerifyCodeFix(filename, filename, new TestDuplicateLocationRule(), new TestDuplicateLocationRuleCodeFix()); };
            a.Should().NotThrow();
        }

        [Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer(LanguageNames.CSharp)]
        private class TestDuplicateLocationRule : SonarDiagnosticAnalyzer
        {
            public const string DiagnosticId = "Test42";
            private readonly DiagnosticDescriptor rule = new DiagnosticDescriptor(DiagnosticId, "Title", "Message", "Category", DiagnosticSeverity.Warning, true, null, null, "MainSourceScope");
            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

            protected override void Initialize(SonarAnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(c =>
                {
                    // Duplicate issues from different analyzer versions, see https://github.com/SonarSource/sonar-dotnet/issues/1109
                    c.ReportDiagnostic(Diagnostic.Create(this.rule, c.Node.GetLocation()));
                    c.ReportDiagnostic(Diagnostic.Create(this.rule, c.Node.GetLocation()));
                }, SyntaxKind.NamespaceDeclaration);
            }
        }

        private class TestDuplicateLocationRuleCodeFix : SonarCodeFixProvider
        {
            public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(TestDuplicateLocationRule.DiagnosticId);
            public override FixAllProvider GetFixAllProvider() => DocumentBasedFixAllProvider.Instance;

            protected override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
            {
                context.RegisterCodeFix(CodeAction.Create("TestTitle", c => Task.FromResult(context.Document)), context.Diagnostics);
                return TaskHelper.CompletedTask;
            }
        }

    }
}



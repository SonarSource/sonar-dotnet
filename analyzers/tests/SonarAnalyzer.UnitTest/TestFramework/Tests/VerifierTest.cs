/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.Rules.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.TestFramework.Tests
{
    [TestClass]
    public class VerifierTest
    {
        private static readonly VerifierBuilder Dummy = new VerifierBuilder<DummyAnalyzer>();

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Constructor_Null_Throws() =>
            ((Func<Verifier>)(() => new(null))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("builder");

        [TestMethod]
        public void Constructor_NoAnalyzers_Throws() =>
            new VerifierBuilder()
                .Invoking(x => x.Build()).Should().Throw<ArgumentException>()
                .WithMessage("Analyzers cannot be empty. Use VerifierBuilder<TAnalyzer> instead or add at least one analyzer using builder.AddAnalyzer().");

        [TestMethod]
        public void Constructor_NullAnalyzers_Throws() =>
            new VerifierBuilder().AddAnalyzer(() => null)
                .Invoking(x => x.Build()).Should().Throw<ArgumentException>().WithMessage("Analyzer instance cannot be null.");

        [TestMethod]
        public void Constructor_NoPaths_Throws() =>
            new VerifierBuilder<DummyAnalyzer>()
                .Invoking(x => x.Build()).Should().Throw<ArgumentException>().WithMessage("Paths cannot be empty. Add at least one path using builder.AddPaths().");

        [TestMethod]
        public void Constructor_MixedLanguageAnalyzers_Throws() =>
            new VerifierBuilder<DummyAnalyzer>().AddAnalyzer(() => new SonarAnalyzer.Rules.VisualBasic.OptionStrictOn())
                .Invoking(x => x.Build()).Should().Throw<ArgumentException>().WithMessage("All Analyzers must declare the same language in their DiagnosticAnalyzerAttribute.");

        [TestMethod]
        public void Constructor_MixedLanguagePaths_Throws() =>
            new VerifierBuilder<DummyAnalyzer>().AddPaths("File.txt")
                .Invoking(x => x.Build()).Should().Throw<ArgumentException>().WithMessage("Path 'File.txt' doesn't match C# file extension '.cs'.");

        [TestMethod]
        public void Verify_RaiseExpectedIssues() =>
            WithSnippet(
@"public class Sample
{
    private int a = 42;     // Noncompliant {{Dummy message}}
    private int b = 42;     // Noncompliant
    private bool c = true;
}").Invoking(x => x.Verify()).Should().NotThrow();

        [TestMethod]
        public void Verify_RaiseUnexpectedIssues() =>
            WithSnippet(
@"public class Sample
{
    private int a = 42;     // FP
    private int b = 42;     // FP
    private bool c = true;
}").Invoking(x => x.Verify()).Should().Throw<UnexpectedDiagnosticException>().WithMessage("CSharp7: Unexpected primary issue on line 3, span (2,20)-(2,22) with message 'Dummy message'*");

        [TestMethod]
        public void Verify_MissingExpectedIssues() =>
            WithSnippet(
@"public class Sample
{
    private bool a = true;   // Noncompliant - FN
    private bool b = true;   // Noncompliant - FN
    private bool c = true;
}").Invoking(x => x.Verify()).Should().Throw<AssertFailedException>().WithMessage(
@"CSharp7: Issue(s) expected but not raised in file(s):
File: File.Concurrent.cs
Line: 3, Type: primary, Id: ''
Line: 4, Type: primary, Id: ''
");

        [TestMethod]
        public void Verify_TwoAnalyzers() =>
            WithSnippet(
@"public class Sample
{
    private int a = 42;     // Noncompliant {{Dummy message}}
                            // Noncompliant@-1
    private int b = 42;     // Noncompliant
                            // Noncompliant@-1
    private bool c = true;
}")
            .AddAnalyzer(() => new DummyAnalyzer()) // Duplicate
            .Invoking(x => x.Verify()).Should().NotThrow();

        [TestMethod]
        public void Verify_TwoPaths() =>
            WithSnippet(
@"public class First
{
    private bool a = true;     // Noncompliant - FN in File.cs
}")
            .AddPaths(WriteFile("Second.cs",
@"public class Second
{
    private bool a = true;     // Noncompliant - FN in Second.cs
}"))
            .Invoking(x => x.Verify()).Should().Throw<AssertFailedException>().WithMessage(
@"CSharp7: Issue(s) expected but not raised in file(s):
File: File.cs
Line: 3, Type: primary, Id: ''

File: Second.cs
Line: 3, Type: primary, Id: ''
");

        [TestMethod]
        public void Verify_ParseOptions()
        {
            var builder = WithSnippet(
@"public class Sample
{
    private System.Exception ex = new(); // C# 9 target-typed new
}");
            builder.WithOptions(ParseOptionsHelper.FromCSharp9).Invoking(x => x.Verify()).Should().NotThrow();
            builder.WithOptions(ParseOptionsHelper.BeforeCSharp9).Invoking(x => x.Verify()).Should().Throw<UnexpectedDiagnosticException>()
                .WithMessage("CSharp5: Unexpected build error [CS8026]: Feature 'target-typed object creation' is not available in C# 5. Please use language version 9.0 or greater. on line 3");
        }

        [TestMethod]
        public void Verify_ErrorBehavior()
        {
            var builder = WithSnippet("undefined");
            builder.Invoking(x => x.Verify()).Should().Throw<UnexpectedDiagnosticException>()
                .WithMessage("CSharp7: Unexpected build error [CS0116]: A namespace cannot directly contain members such as fields, methods or statements on line 1");
            builder.WithErrorBehavior(CompilationErrorBehavior.FailTest).Invoking(x => x.Verify()).Should().Throw<UnexpectedDiagnosticException>()
                .WithMessage("CSharp7: Unexpected build error [CS0116]: A namespace cannot directly contain members such as fields, methods or statements on line 1");
            builder.WithErrorBehavior(CompilationErrorBehavior.Ignore).Invoking(x => x.Verify()).Should().NotThrow();
        }

        [TestMethod]
        public void Verify_OnlyDiagnostics()
        {
            var builder = new VerifierBuilder<SymbolicExecutionRunner>().AddPaths(WriteFile("File.cs",
@"public class Sample
{
    public void Method()
    {
        var t = true;
        if (t)          // S2583
            t = true;
        else
            t = true;
        if (t)          // S2589
            t = true;
    }
}"));
            builder.Invoking(x => x.Verify()).Should().Throw<UnexpectedDiagnosticException>().WithMessage(
@"CSharp7: Unexpected primary issue on line 6, span (5,12)-(5,13) with message 'Change this condition so that it does not always evaluate to 'true'; some subsequent code is never executed.'.*");
            builder.WithOnlyDiagnostics(ConditionEvaluatesToConstant.S2589).Invoking(x => x.Verify()).Should().Throw<UnexpectedDiagnosticException>().WithMessage(
@"CSharp7: Unexpected primary issue on line 10, span (9,12)-(9,13) with message 'Change this condition so that it does not always evaluate to 'true'.'*");
            builder.WithOnlyDiagnostics(NullPointerDereference.S2259).Invoking(x => x.Verify()).Should().NotThrow();
        }

        [TestMethod]
        public void Verify_OutputKind()
        {
            var builder = WithSnippet("var topLevelStatement = true;")
                .AddPaths(WriteFile("Second.cs", "public class Workaround { }")) // ToDo: This should be replaced with .WithNonconcurrent() inside WithTopLevelStatement
                .WithOptions(ParseOptionsHelper.FromCSharp9);
            builder.WithTopLevelStatements().Invoking(x => x.Verify()).Should().NotThrow();
            builder.WithOutputKind(OutputKind.ConsoleApplication).Invoking(x => x.Verify()).Should().NotThrow();
            builder.Invoking(x => x.Verify()).Should().Throw<UnexpectedDiagnosticException>()
                .WithMessage("CSharp9: Unexpected build error [CS8805]: Program using top-level statements must be an executable. on line 1");
        }

        private VerifierBuilder WithSnippet(string code) =>
            Dummy.AddPaths(WriteFile("File.cs", code));

        private string WriteFile(string name, string content) =>
            TestHelper.WriteFile(TestContext, name, content);
    }
}

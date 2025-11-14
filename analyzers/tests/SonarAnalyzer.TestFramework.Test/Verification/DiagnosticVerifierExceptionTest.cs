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

using SonarAnalyzer.TestFramework.Verification;

namespace SonarAnalyzer.Test.TestFramework.Tests.Verification;

[TestClass]
public class DiagnosticVerifierExceptionTest
{
    [TestMethod]
    public void Ctor_Empty_Throws() =>
        ((Func<DiagnosticVerifierException>)(() => new([]))).Should().Throw<ArgumentException>().WithMessage("messages cannot be empty*");

    [TestMethod]
    public void Ctor_ShortMessageWithoutSpace_Throws() =>
        ((Func<DiagnosticVerifierException>)(() => new([new("NoSpace", "Full Descriptin", "DiagnosticVerifierException.File1.cs", 1)]))).Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Short description must contain space for Rider to display clickable link.");

    [TestMethod]
    public void Message_SerializesAllFullDescriptions() =>
        new DiagnosticVerifierException(
            [
                new("Short 1", "Full 1", "File.cs", 1),
                new("Short 2", "Full 2", "File.cs", 1),
                new("Short 3", "Full 3 { Does not break formatting", "File.cs", 1),
                new("Short 4", "Full 4 } Does not break formatting", "File.cs", 1)
            ])
            .Message
            .Should()
            .BeIgnoringLineEndings("""
                Full 1
                Full 2
                Full 3 { Does not break formatting
                Full 4 } Does not break formatting
                """);

    [TestMethod]
    public void StackTrace_SerializesShortDescriptions() =>
        new DiagnosticVerifierException(
            [
                new("Short 1", "Full 1", "DiagnosticVerifierException.File1.cs", 11),
                new("Short 2", "Full 2", "DiagnosticVerifierException.File1.cs", 22),
                new("Short 3", "Full 3", "DiagnosticVerifierException.File1.cs", 33),
                new("Short 4", "Full 4", "DiagnosticVerifierException.File1.cs", 44),
            ])
            .StackTrace
            .Should()
            .BeIgnoringLineEndings($"""
                DiagnosticVerifierException.File1.cs
                at Short.1() in {Paths.TestsRoot}\SonarAnalyzer.TestFramework.Test\TestCases\DiagnosticVerifierException.File1.cs:line 11
                at Short.2() in {Paths.TestsRoot}\SonarAnalyzer.TestFramework.Test\TestCases\DiagnosticVerifierException.File1.cs:line 22
                at Short.3() in {Paths.TestsRoot}\SonarAnalyzer.TestFramework.Test\TestCases\DiagnosticVerifierException.File1.cs:line 33
                at Short.4() in {Paths.TestsRoot}\SonarAnalyzer.TestFramework.Test\TestCases\DiagnosticVerifierException.File1.cs:line 44
                ---

                """);

    [TestMethod]
    public void StackTrace_SeparatesMultipleFiles() =>
        new DiagnosticVerifierException(
            [
                new("Short 1", "Full 1", "DiagnosticVerifierException.File1.cs", 11),
                new("Short 2", "Full 2", "DiagnosticVerifierException.File1.cs", 22),
                new("Short 3", "Full 3", "DiagnosticVerifierException.File2.cs", 33),
                new("Short 4", "Full 4", "DiagnosticVerifierException.File2.cs", 44),
                new("Concurrent File", "Full 5", "DiagnosticVerifierException.Concurrent.cs", 55) // This file has ".Concurent." in the name and exists on the disk, so it should be listed.
            ])
            .StackTrace
            .Should()
            .BeIgnoringLineEndings($"""
                DiagnosticVerifierException.Concurrent.cs
                at Concurrent.File() in {Paths.TestsRoot}\SonarAnalyzer.TestFramework.Test\TestCases\DiagnosticVerifierException.Concurrent.cs:line 55
                ---
                DiagnosticVerifierException.File1.cs
                at Short.1() in {Paths.TestsRoot}\SonarAnalyzer.TestFramework.Test\TestCases\DiagnosticVerifierException.File1.cs:line 11
                at Short.2() in {Paths.TestsRoot}\SonarAnalyzer.TestFramework.Test\TestCases\DiagnosticVerifierException.File1.cs:line 22
                ---
                DiagnosticVerifierException.File2.cs
                at Short.3() in {Paths.TestsRoot}\SonarAnalyzer.TestFramework.Test\TestCases\DiagnosticVerifierException.File2.cs:line 33
                at Short.4() in {Paths.TestsRoot}\SonarAnalyzer.TestFramework.Test\TestCases\DiagnosticVerifierException.File2.cs:line 44
                ---

                """);

    [TestMethod]
    public void StackTrace_Ignore_NullShortDescription() =>
        new DiagnosticVerifierException(
            [
                new(null, "Full 1", "DiagnosticVerifierException.File1.cs", 11),
                new(null, "Full 2", "DiagnosticVerifierException.File1.cs", 22),
            ])
            .StackTrace
            .Should()
            .BeEmpty();

    [TestMethod]
    public void StackTrace_Ignore_NonexistentFiles() =>
        new DiagnosticVerifierException(
            [
                new("Short 1", "Full 1", "Nonexistent.cs", 11),
                new("Short 2", "Full 2", "Nonexistent.Concurrent.cs", 22),
                new("Short 3", "Full 3", "Snippet0.cs", 33),
                new("Short 4", "Full 4", "Snippet1.cs", 44),
            ])
            .StackTrace
            .Should()
            .BeEmpty();
}

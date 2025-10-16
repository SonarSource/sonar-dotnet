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

using FluentAssertions.Execution;

namespace SonarAnalyzer.Test.TestFramework.Tests.Common;

[TestClass]
public class LogTesterTest
{
    private const string StandardMessage = nameof(StandardMessage);
    private const string ErrorMessage = nameof(ErrorMessage);

    [TestMethod]
    public void AssertContain_CapturesStandardStream()
    {
        using var sut = new LogTester();
        Console.WriteLine(StandardMessage);
        Console.Error.WriteLine(ErrorMessage);

        sut.Invoking(x => x.AssertContain(StandardMessage)).Should().NotThrow();
        sut.Invoking(x => x.AssertContain(ErrorMessage)).Should().Throw<AssertionFailedException>();
    }

    [TestMethod]
    public void AssertContainError_CapturesErrorStream()
    {
        using var sut = new LogTester();
        Console.WriteLine(StandardMessage);
        Console.Error.WriteLine(ErrorMessage);

        sut.Invoking(x => x.AssertContainError(StandardMessage)).Should().Throw<AssertionFailedException>();
        sut.Invoking(x => x.AssertContainError(ErrorMessage)).Should().NotThrow();
    }

    [TestMethod]
    public void Dispose_StopsCapturing()
    {
        Console.WriteLine(StandardMessage);
        Console.Error.WriteLine(ErrorMessage);
        var sut = new LogTester();
        // Nothing to write. Messages were written before and after.
        sut.Dispose();
        Console.WriteLine(StandardMessage);
        Console.Error.WriteLine(ErrorMessage);

        sut.Invoking(x => x.AssertContain(StandardMessage)).Should().Throw<AssertionFailedException>();
        sut.Invoking(x => x.AssertContainError(ErrorMessage)).Should().Throw<AssertionFailedException>();
    }
}

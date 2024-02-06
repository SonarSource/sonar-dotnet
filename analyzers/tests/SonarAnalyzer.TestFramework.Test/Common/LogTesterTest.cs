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
        sut.Invoking(x => x.AssertContain(ErrorMessage)).Should().Throw<AssertFailedException>();
    }

    [TestMethod]
    public void AssertContainError_CapturesErrorStream()
    {
        using var sut = new LogTester();
        Console.WriteLine(StandardMessage);
        Console.Error.WriteLine(ErrorMessage);

        sut.Invoking(x => x.AssertContainError(StandardMessage)).Should().Throw<AssertFailedException>();
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

        sut.Invoking(x => x.AssertContain(StandardMessage)).Should().Throw<AssertFailedException>();
        sut.Invoking(x => x.AssertContainError(ErrorMessage)).Should().Throw<AssertFailedException>();
    }
}

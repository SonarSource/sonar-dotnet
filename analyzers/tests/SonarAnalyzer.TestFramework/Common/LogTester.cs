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

namespace SonarAnalyzer.TestFramework.Common;

internal sealed class LogTester : IDisposable
{
    private readonly TextWriter originalOut;
    private readonly TextWriter originalError;
    private readonly StringWriter outWriter = new();
    private readonly StringWriter errorWriter = new();

    public LogTester()
    {
        originalOut = Console.Out;
        Console.SetOut(outWriter);
        originalError = Console.Error;
        Console.SetError(errorWriter);
    }

    public void AssertContain(string value) =>
        outWriter.ToString().Should().ContainIgnoringLineEndings(value);

    public void AssertContainError(string value) =>
        errorWriter.ToString().Should().ContainIgnoringLineEndings(value);

    public void Dispose()
    {
        Console.SetOut(originalOut);
        Console.SetError(originalError);
    }
}

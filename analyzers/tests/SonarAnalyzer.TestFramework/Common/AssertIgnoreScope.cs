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

namespace SonarAnalyzer.TestFramework.Common;

/// <summary>
/// Some of the tests cover exceptions in which we have asserts as well, we want to ignore those asserts during tests.
/// </summary>
public sealed class AssertIgnoreScope : IDisposable
{
    private readonly DefaultTraceListener listener;

    public AssertIgnoreScope()
    {
        listener = Trace.Listeners.OfType<DefaultTraceListener>().FirstOrDefault();
        Trace.Listeners.Remove(listener);
    }

    public void Dispose() =>
        Trace.Listeners.Add(listener);
}

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

using SonarAnalyzer.TestFramework.Common;

namespace ITs.JsonParser.Test;

// This file does not belong in this project. It must be here, because this is the only test project that doesn't have TestCases directory in it.
[TestClass]
public class PathsTest
{
    [TestMethod]
    public void CurrentTestCases_NoTestCases_Throws() =>
        ((Func<string>)(() => Paths.CurrentTestCases())).Should().Throw<InvalidOperationException>().WithMessage("Could not find TestCases directory from current path:*");
}

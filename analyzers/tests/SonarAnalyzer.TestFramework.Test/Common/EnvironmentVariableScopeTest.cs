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

namespace SonarAnalyzer.Test.TestFramework.Tests.Common;

[TestClass]
public class EnvironmentVariableScopeTest
{
    private const string VariableName = "RANDOM__ENVIRONMENT__VARIABLE__THAT__DOES__NOT__EXIST__ANYWHERE";

    [TestMethod]
    public void SetVariable_SetsAndRestores()
    {
        Environment.GetEnvironmentVariable(VariableName).Should().BeNullOrEmpty();
        using (var scope = new EnvironmentVariableScope())
        {
            scope.SetVariable(VariableName, "Lorem ipsum");
            Environment.GetEnvironmentVariable(VariableName).Should().Be("Lorem ipsum");
            scope.SetVariable(VariableName, "Dolor sit amet");
            Environment.GetEnvironmentVariable(VariableName).Should().Be("Dolor sit amet");
        }
        Environment.GetEnvironmentVariable(VariableName).Should().BeNullOrEmpty();
    }

    [TestMethod]
    public void Dispose_Twice_DoesNotFail()
    {
        using var outer = new EnvironmentVariableScope();
        outer.SetVariable(VariableName, "Original");
        var sut = new EnvironmentVariableScope();
        sut.SetVariable(VariableName, "SUT");
        Environment.GetEnvironmentVariable(VariableName).Should().Be("SUT");
        sut.Dispose();
        sut.Invoking(x => x.Dispose()).Should().NotThrow();  // Invoked for the second time
        Environment.GetEnvironmentVariable(VariableName).Should().Be("Original");
    }
}

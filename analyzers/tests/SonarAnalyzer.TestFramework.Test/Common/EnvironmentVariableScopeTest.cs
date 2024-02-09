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
public class EnvironmentVariableScopeTest
{
    private const string VariableName = "RANDOM__ENVIRONMENT__VARIABLE__THAT__DOES__NOT__EXIST__ANYWHERE";

    [TestMethod]
    public void SetVariable_SetsAndRestores()
    {
        Environment.GetEnvironmentVariable(VariableName).Should().BeNullOrEmpty();
        using (var scope = new EnvironmentVariableScope(false))
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
        using var outer = new EnvironmentVariableScope(false);
        outer.SetVariable(VariableName, "Original");
        var sut = new EnvironmentVariableScope(false);
        sut.SetVariable(VariableName, "SUT");
        Environment.GetEnvironmentVariable(VariableName).Should().Be("SUT");
        sut.Dispose();
        sut.Invoking(x => x.Dispose()).Should().NotThrow();  // Invoked for the second time
        Environment.GetEnvironmentVariable(VariableName).Should().Be("Original");
    }
}

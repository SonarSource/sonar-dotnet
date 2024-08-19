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

using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

[TestClass]
public class ExceptionStateTest
{
    [TestMethod]
    public void Constructor_Null_Throws() =>
        ((Func<ExceptionState>)(() => new ExceptionState(null))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("type");

    [TestMethod]
    public void ToString_Unknown() =>
        ExceptionState.UnknownException.ToString().Should().Be("Unknown");

    [DataTestMethod]
    [DataRow("System.Exception", "Exception")]
    [DataRow("System.ArgumentNullException", "ArgumentNullException")]
    [DataRow("System.IO.IOException", "IOException")]
    public void ToString_Known(string typeName, string expected) =>
        new ExceptionState(TestHelper.CompileCS(string.Empty).Model.Compilation.GetTypeByMetadataName(typeName)).ToString().Should().Be(expected);

    [TestMethod]
    public void Equals_ReturnsTrueForEquivalent()
    {
        var compilation = TestHelper.CompileCS(string.Empty).Model.Compilation;
        var first = new ExceptionState(compilation.GetTypeByMetadataName("System.Exception"));
        var same = new ExceptionState(compilation.GetTypeByMetadataName("System.Exception"));
        var second = new ExceptionState(compilation.GetTypeByMetadataName("System.ArgumentNullException"));

        first.Equals(first).Should().BeTrue();
        first.Equals(same).Should().BeTrue();
        first.Equals(null).Should().BeFalse();
        first.Equals(second).Should().BeFalse();
        first.Equals(ExceptionState.UnknownException).Should().BeFalse();

        ExceptionState.UnknownException.Equals(ExceptionState.UnknownException).Should().BeTrue();
        ExceptionState.UnknownException.Equals(first).Should().BeFalse();
        ExceptionState.UnknownException.Equals(null).Should().BeFalse();
    }

    [TestMethod]
    public void GetHashCode_ReturnsSameForEquivalent()
    {
        var compilation = TestHelper.CompileCS(string.Empty).Model.Compilation;
        var first = new ExceptionState(compilation.GetTypeByMetadataName("System.Exception"));
        var same = new ExceptionState(compilation.GetTypeByMetadataName("System.Exception"));
        var second = new ExceptionState(compilation.GetTypeByMetadataName("System.ArgumentNullException"));

        first.GetHashCode().Should().Be(same.GetHashCode()).And.NotBe(second.GetHashCode());
        ExceptionState.UnknownException.GetHashCode().Should().NotBe(first.GetHashCode());
    }
}

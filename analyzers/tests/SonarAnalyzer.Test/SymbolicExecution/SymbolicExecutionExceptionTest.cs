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

using SonarAnalyzer.SymbolicExecution;

namespace SonarAnalyzer.Test.SymbolicExecution.Sonar.Constraints;

[TestClass]
public class SymbolicExecutionExceptionTest
{
    [TestMethod]
    public void Constructor_ParameterLess()
    {
        var sut = new SymbolicExecutionException();
        sut.Message.Should().Be("Exception of type 'SonarAnalyzer.SymbolicExecution.SymbolicExecutionException' was thrown.");
        sut.InnerException.Should().BeNull();
    }

    [TestMethod]
    public void Constructor_MessageArg()
    {
        var sut = new SymbolicExecutionException("Lorem ipsum");
        sut.Message.Should().Be("Lorem ipsum");
        sut.InnerException.Should().BeNull();
    }

    [TestMethod]
    public void Constructor_MessageAndInnerException()
    {
        var inner = new InvalidOperationException();
        var sut = new SymbolicExecutionException("Lorem ipsum", inner);
        sut.Message.Should().Be("Lorem ipsum");
        sut.InnerException.Should().Be(inner);
    }

    [TestMethod]
    public void Constructor_SymbolAndLocation()
    {
        var inner = new InvalidOperationException("Lorem Ipsum");
        var (tree, semanticModel) = TestHelper.CompileCS("public class Sample { }");
        var symbol = semanticModel.LookupSymbols(0, name: "Sample").Single();
        var location = tree.GetRoot().GetLocation();

        var sut = new SymbolicExecutionException(inner, null, null);
        sut.Message.Should().Be("Error processing method: {unknown} ## Method file: {unknown} ## Method line: {unknown} ## Inner exception: System.InvalidOperationException: Lorem Ipsum");
        sut.InnerException.Should().Be(inner);

        sut = new SymbolicExecutionException(inner, symbol, location);
        sut.Message.Should().Be("Error processing method: Sample ## Method file: snippet0.cs ## Method line: 0,0 ## Inner exception: System.InvalidOperationException: Lorem Ipsum");
        sut.InnerException.Should().Be(inner);
    }
}

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

public partial class ProgramStateTest
{
    [TestMethod]
    public void PushException_IsImmutable()
    {
        var sut = ProgramState.Empty;
        sut.Exception.Should().BeNull();

        sut = sut.PushException(ExceptionState.UnknownException);
        sut.Exception.Should().Be(ExceptionState.UnknownException);

        ProgramState.Empty.Exception.Should().BeNull();
    }

    [TestMethod]
    public void SetException_IsImmutable()
    {
        var sut = ProgramState.Empty;
        sut.Exception.Should().BeNull();

        sut = sut.SetException(ExceptionState.UnknownException);
        sut.Exception.Should().Be(ExceptionState.UnknownException);

        ProgramState.Empty.Exception.Should().BeNull();
    }

    [TestMethod]
    public void SetException_RemovesAllPrevious()
    {
        var sut = ProgramState.Empty.PushException(ExceptionState.UnknownException).PushException(ExceptionState.UnknownException).PushException(ExceptionState.UnknownException);
        sut.ToString().Should().BeIgnoringLineEndings(
@"Exception: Unknown
Exception: Unknown
Exception: Unknown
");
        sut = sut.SetException(ExceptionState.UnknownException);
        sut.ToString().Should().BeIgnoringLineEndings(
@"Exception: Unknown
");
    }

    [TestMethod]
    public void PopException_IsImmutable()
    {
        var original = ProgramState.Empty.PushException(ExceptionState.UnknownException);
        var other = original.PopException();
        other.Exception.Should().BeNull();
        original.Exception.Should().Be(ExceptionState.UnknownException);
    }

    [TestMethod]
    public void PopException_RemovesInCorrectOrder()
    {
        var compilation = TestHelper.CompileCS(string.Empty).Model.Compilation;
        var sut = ProgramState.Empty
            .PushException(new(compilation.GetTypeByMetadataName("System.NotImplementedException")))
            .PushException(new(compilation.GetTypeByMetadataName("System.ArgumentNullException")))
            .PushException(new(compilation.GetTypeByMetadataName("System.FormatException")));

        sut.Exception.Should().NotBeNull();
        sut.Exception.Type.Name.Should().Be("FormatException");

        sut = sut.PopException();
        sut.Exception.Should().NotBeNull();
        sut.Exception.Type.Name.Should().Be("ArgumentNullException");

        sut = sut.PopException();
        sut.Exception.Should().NotBeNull();
        sut.Exception.Type.Name.Should().Be("NotImplementedException");

        sut = sut.PopException();
        sut.Exception.Should().BeNull();
    }
}

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

using Moq;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.Test.TestFramework.SymbolicExecution;

using ProgramStates = SonarAnalyzer.SymbolicExecution.Roslyn.States<SonarAnalyzer.SymbolicExecution.Roslyn.ProgramState>;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

[TestClass]
public class SymbolicCheckListTest
{
    [TestMethod]
    public void Constructor_Null_Throws() =>
        ((Func<SymbolicCheckList>)(() => new SymbolicCheckList(null))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("checks");

    [TestMethod]
    public void Notifications_ExecutedForAll()
    {
        var a = new Mock<SymbolicCheck>();
        var b = new Mock<SymbolicCheck>();
        var context = new SymbolicContext(null, default, ProgramState.Empty, false, Array.Empty<ISymbol>());
        a.Setup(x => x.PreProcess(context)).Returns(new ProgramStates(context.State));
        a.Setup(x => x.PostProcess(context)).Returns(new ProgramStates(context.State));
        var sut = new SymbolicCheckList(new[] { a.Object, b.Object });

        a.Verify(x => x.ExitReached(context), Times.Never);
        b.Verify(x => x.ExitReached(context), Times.Never);
        sut.ExitReached(context);
        a.Verify(x => x.ExitReached(context), Times.Once);
        b.Verify(x => x.ExitReached(context), Times.Once);

        a.Verify(x => x.ExecutionCompleted(), Times.Never);
        b.Verify(x => x.ExecutionCompleted(), Times.Never);
        sut.ExecutionCompleted();
        a.Verify(x => x.ExecutionCompleted(), Times.Once);
        b.Verify(x => x.ExecutionCompleted(), Times.Once);

        a.Verify(x => x.PreProcess(context), Times.Never);
        b.Verify(x => x.PreProcess(context), Times.Never);
        sut.PreProcess(context);
        a.Verify(x => x.PreProcess(context), Times.Once);
        b.Verify(x => x.PreProcess(context), Times.Once);

        a.Verify(x => x.PostProcess(context), Times.Never);
        b.Verify(x => x.PostProcess(context), Times.Never);
        sut.PostProcess(context);
        a.Verify(x => x.PostProcess(context), Times.Once);
        b.Verify(x => x.PostProcess(context), Times.Once);
    }

    [TestMethod]
    public void PostProcess_CanReturnMultipleStates()
    {
        var triple = new PostProcessTestCheck(x => new ProgramStates(x.State, x.State, x.State));
        var sut = new SymbolicCheckList(new[] { triple, triple });
        sut.PostProcess(new(null, default, ProgramState.Empty, false, Array.Empty<ISymbol>())).Length.Should().Be(9);
    }

    [TestMethod]
    public void PostProcess_CanReturnNoStates()
    {
        var empty = new PostProcessTestCheck(x => new ProgramStates());
        var sut = new SymbolicCheckList(new[] { empty });
        sut.PostProcess(new(null, default, ProgramState.Empty, false, Array.Empty<ISymbol>())).Length.Should().Be(0);
    }
}

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

using NSubstitute;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.Test.TestFramework.SymbolicExecution;

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
        var a = Substitute.For<SymbolicCheck>();
        var b = Substitute.For<SymbolicCheck>();
        var context = new SymbolicContext(null, default, ProgramState.Empty, false, []);
        a.PreProcess(context).Returns([context.State]);
        a.PostProcess(context).Returns([context.State]);
        var sut = new SymbolicCheckList([a, b]);

        a.DidNotReceive().ExitReached(context);
        b.DidNotReceive().ExitReached(context);
        sut.ExitReached(context);
        a.Received(1).ExitReached(context);
        b.Received(1).ExitReached(context);

        a.DidNotReceive().ExecutionCompleted();
        b.DidNotReceive().ExecutionCompleted();
        sut.ExecutionCompleted();
        a.Received(1).ExecutionCompleted();
        b.Received(1).ExecutionCompleted();

        a.DidNotReceive().PreProcess(context);
        b.DidNotReceive().PreProcess(context);
        sut.PreProcess(context);
        a.Received(1).PreProcess(context);
        b.Received(1).PreProcess(context);

        a.DidNotReceive().PostProcess(context);
        b.DidNotReceive().PostProcess(context);
        sut.PostProcess(context);
        a.Received(1).PostProcess(context);
        b.Received(1).PostProcess(context);
    }

    [TestMethod]
    public void PostProcess_CanReturnMultipleStates()
    {
        var triple = new PostProcessTestCheck(x => new[] { x.State, x.State, x.State });
        var sut = new SymbolicCheckList(new[] { triple, triple });
        sut.PostProcess(new(null, default, ProgramState.Empty, false, Array.Empty<ISymbol>())).Should().HaveCount(9);
    }

    [TestMethod]
    public void PostProcess_CanReturnNoStates()
    {
        var empty = new PostProcessTestCheck(x => Array.Empty<ProgramState>());
        var sut = new SymbolicCheckList(new[] { empty });
        sut.PostProcess(new(null, default, ProgramState.Empty, false, Array.Empty<ISymbol>())).Should().HaveCount(0);
    }
}

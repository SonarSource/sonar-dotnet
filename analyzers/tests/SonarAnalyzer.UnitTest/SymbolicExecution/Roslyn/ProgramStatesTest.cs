/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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
using ProgramStates = SonarAnalyzer.SymbolicExecution.Roslyn.States<SonarAnalyzer.SymbolicExecution.Roslyn.ProgramState>;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn;

[TestClass]
public class ProgramStatesTest
{
    [TestMethod]
    public void IterateEmtpyStates()
    {
        var sut = new ProgramStates();
        sut.Length.Should().Be(0);
        foreach (var _ in sut)
        {
            Assert.Fail("Empty states");
        }
    }

    [TestMethod]
    public void IterateOneState()
    {
        var sut = new ProgramStates(ProgramState.Empty);
        sut.Length.Should().Be(1);
        var i = 0;
        foreach (var s in sut)
        {
            s.Should().Be(ProgramState.Empty);
            i++;
        }
        i.Should().Be(1);
    }

    [TestMethod]
    public void IterateTwoStates()
    {
        var s1 = ProgramState.Empty;
        var s2 = s1.AddVisit(1);
        var sut = new ProgramStates(s1, s2);
        sut.Length.Should().Be(2);
        var i = 0;
        foreach (var s in sut)
        {
            switch (i)
            {
                case 0:
                    s.Should().Be(s1);
                    break;
                case 1:
                    s.Should().Be(s2);
                    break;
                default:
                    Assert.Fail("Unreachable");
                    break;
            }
            i++;
        }
        i.Should().Be(2);
    }

    [TestMethod]
    public void IterateThreeStates()
    {
        var s1 = ProgramState.Empty;
        var s2 = s1.AddVisit(1);
        var s3 = s2.AddVisit(2);
        var sut = new ProgramStates(s1, s2, s3);
        sut.Length.Should().Be(3);
        var i = 0;
        foreach (var s in sut)
        {
            switch (i)
            {
                case 0:
                    s.Should().Be(s1);
                    break;
                case 1:
                    s.Should().Be(s2);
                    break;
                case 2:
                    s.Should().Be(s3);
                    break;
                default:
                    Assert.Fail("Unreachable");
                    break;
            }
            i++;
        }
        i.Should().Be(3);
    }

    [TestMethod]
    public void IterateTenStates()
    {
        var states = Enumerable.Range(1, 10).Select(i => ProgramState.Empty.AddVisit(i)).ToList();
        var sut = new ProgramStates(states[0], states[1], states.Skip(2).ToArray());
        sut.Length.Should().Be(10);
        var i = 0;
        foreach (var s in sut)
        {
            s.Should().BeSameAs(states[i]);
            i++;
        }
        i.Should().Be(10);
    }
}

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

using Moq;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.Test.TestFramework.SymbolicExecution;

using ProgramStates = SonarAnalyzer.SymbolicExecution.Roslyn.States<SonarAnalyzer.SymbolicExecution.Roslyn.ProgramState>;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn;

[TestClass]
public class ProgramStatesTest
{
    [TestMethod]
    public void IterateEmptyStates()
    {
        var sut = new ProgramStates();
        sut.Length.Should().Be(0);
        foreach (var s in sut)
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
        var s2 = s1.SetSymbolConstraint(Mock.Of<ISymbol>(), ObjectConstraint.Null);
        var sut = new ProgramStates(s1, s2);
        AssertStates(sut, s1, s2);
    }

    [TestMethod]
    public void IterateThreeStates()
    {
        var s1 = ProgramState.Empty;
        var s2 = s1.SetSymbolConstraint(Mock.Of<ISymbol>(), ObjectConstraint.Null);
        var s3 = s2.SetSymbolConstraint(Mock.Of<ISymbol>(), ObjectConstraint.NotNull);
        var sut = new ProgramStates(s1, s2, s3);
        AssertStates(sut, s1, s2, s3);
    }

    [TestMethod]
    public void IterateTenStates()
    {
        var states = Enumerable.Range(1, 10).Select(i => ProgramState.Empty.SetSymbolConstraint(Mock.Of<ISymbol>(), ObjectConstraint.Null)).ToArray();
        var sut = new ProgramStates(states[0], states[1], states.Skip(2).ToArray());
        AssertStates(sut, states);
    }

    [TestMethod]
    public void AddEmptyStates()
    {
        var s1 = new ProgramStates();
        var s2 = new ProgramStates();
        var sut = s1 + s2;
        sut.Should().Be(new ProgramStates());
    }

    [TestMethod]
    public void AddLeftEmpty()
    {
        var s1 = new ProgramStates();
        var s2 = new ProgramStates(ProgramState.Empty);
        var sut = s1 + s2;
        sut.Should().Be(s2);
    }

    [TestMethod]
    public void AddRightEmpty()
    {
        var s1 = new ProgramStates(ProgramState.Empty);
        var s2 = new ProgramStates();
        var sut = s1 + s2;
        sut.Should().Be(s1);
    }

    [TestMethod]
    public void AddLeftAndRightOneElement()
    {
        var s1 = ProgramState.Empty.SetSymbolConstraint(Mock.Of<ISymbol>(), ObjectConstraint.Null);
        var s2 = ProgramState.Empty.SetSymbolConstraint(Mock.Of<ISymbol>(), ObjectConstraint.NotNull);
        var sut = new ProgramStates(s1) + new ProgramStates(s2);
        AssertStates(sut, s1, s2);
    }

    [TestMethod]
    public void AddLeftOneAndRightTwoElements()
    {
        var s1 = ProgramState.Empty.SetSymbolConstraint(Mock.Of<ISymbol>(), ObjectConstraint.Null);
        var s2 = ProgramState.Empty.SetSymbolConstraint(Mock.Of<ISymbol>(), ObjectConstraint.NotNull);
        var s3 = ProgramState.Empty.SetSymbolConstraint(Mock.Of<ISymbol>(), BoolConstraint.True);
        var sut = new ProgramStates(s1) + new ProgramStates(s2, s3);
        AssertStates(sut, s1, s2, s3);
    }

    [TestMethod]
    public void AddLeftTwoAndRightOneElements()
    {
        var s1 = ProgramState.Empty.SetSymbolConstraint(Mock.Of<ISymbol>(), ObjectConstraint.Null);
        var s2 = ProgramState.Empty.SetSymbolConstraint(Mock.Of<ISymbol>(), ObjectConstraint.NotNull);
        var s3 = ProgramState.Empty.SetSymbolConstraint(Mock.Of<ISymbol>(), BoolConstraint.True);
        var sut = new ProgramStates(s1, s2) + new ProgramStates(s3);
        AssertStates(sut, s1, s2, s3);
    }

    [TestMethod]
    public void AddLeftTwoAndRightTwoElements()
    {
        var s1 = ProgramState.Empty.SetSymbolConstraint(Mock.Of<ISymbol>(), ObjectConstraint.Null);
        var s2 = ProgramState.Empty.SetSymbolConstraint(Mock.Of<ISymbol>(), ObjectConstraint.NotNull);
        var s3 = ProgramState.Empty.SetSymbolConstraint(Mock.Of<ISymbol>(), BoolConstraint.True);
        var s4 = ProgramState.Empty.SetSymbolConstraint(Mock.Of<ISymbol>(), BoolConstraint.False);
        var sut = new ProgramStates(s1, s2) + new ProgramStates(s3, s4);
        AssertStates(sut, s1, s2, s3, s4);
    }

    [TestMethod]
    public void AddLeftThreeAndRightThreeElements()
    {
        var s1 = ProgramState.Empty.SetSymbolConstraint(Mock.Of<ISymbol>(), ObjectConstraint.Null);
        var s2 = ProgramState.Empty.SetSymbolConstraint(Mock.Of<ISymbol>(), ObjectConstraint.NotNull);
        var s3 = ProgramState.Empty.SetSymbolConstraint(Mock.Of<ISymbol>(), BoolConstraint.True);
        var s4 = ProgramState.Empty.SetSymbolConstraint(Mock.Of<ISymbol>(), BoolConstraint.False);
        var s5 = ProgramState.Empty.SetSymbolConstraint(Mock.Of<ISymbol>(), TestConstraint.First);
        var s6 = ProgramState.Empty.SetSymbolConstraint(Mock.Of<ISymbol>(), TestConstraint.Second);
        var sut = new ProgramStates(s1, s2, s3) + new ProgramStates(s4, s5, s6);
        AssertStates(sut, s1, s2, s3, s4, s5, s6);
    }

    private static void AssertStates(ProgramStates sut, params ProgramState[] expectedStates)
    {
        var actualEnum = sut.GetEnumerator();
        var expectedEnum = expectedStates.GetEnumerator();
        var i = 0;
        while (actualEnum.MoveNext() && expectedEnum.MoveNext())
        {
            actualEnum.Current.Should().Be(expectedEnum.Current, $"exectedStates[{i}] says so.");
            i++;
        }
        sut.Length.Should().Be(i, "sut should have as many elements as expectedStates");
        expectedStates.Should().HaveCount(i, "expectedStates should have as many elements as sut");
    }
}

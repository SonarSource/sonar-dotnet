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
    public void AddVisit_IncreasesCounter_IsImmutable()
    {
        var sut = ProgramState.Empty;
        sut.GetVisitCount(0).Should().Be(0);
        sut.GetVisitCount(42).Should().Be(0);

        sut = sut.AddVisit(0);
        sut.GetVisitCount(0).Should().Be(1);
        sut = sut.AddVisit(0);
        sut.GetVisitCount(0).Should().Be(2);
        sut = sut.AddVisit(0);
        sut.GetVisitCount(0).Should().Be(3);

        sut = sut.AddVisit(42);
        sut.GetVisitCount(0).Should().Be(3);
        sut.GetVisitCount(42).Should().Be(1);
        ProgramState.Empty.GetVisitCount(0).Should().Be(0);
        ProgramState.Empty.GetVisitCount(42).Should().Be(0);
    }

    [TestMethod]
    public void AddVisit_DoesNotChangeGetHashCode() =>
        ProgramState.Empty.GetHashCode().Should().Be(ProgramState.Empty.AddVisit(0).GetHashCode());

    [TestMethod]
    public void AddVisit_DoesNotChangeEquals() =>
        ProgramState.Empty.Equals(ProgramState.Empty.AddVisit(0)).Should().BeTrue();
}

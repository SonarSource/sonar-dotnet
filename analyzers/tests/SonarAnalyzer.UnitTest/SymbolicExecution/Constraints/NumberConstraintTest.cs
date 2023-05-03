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

using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.UnitTest.SymbolicExecution
{
    [TestClass]
    public class NumberConstraintTest
    {
        [TestMethod]
        public void NumberConstraint_Min_Max()
        {
            var sut = NumberConstraint.From(1, 10);
            sut.Min.Value.Should().Be(1);
            sut.Max.Value.Should().Be(10);
            sut.Opposite.Should().BeNull();

            sut = NumberConstraint.From(10, 1); // Swapped
            sut.Min.Value.Should().Be(1);
            sut.Max.Value.Should().Be(10);
            sut.Opposite.Should().BeNull();

            sut = NumberConstraint.From(1, null);
            sut.Min.Value.Should().Be(1);
            sut.Max.Should().BeNull();
            sut.Opposite.Should().BeNull();

            sut = NumberConstraint.From(null, 100);
            sut.Min.Should().BeNull();
            sut.Max.Value.Should().Be(100);
            sut.Opposite.Should().BeNull();
        }

        [TestMethod]
        public void NumberConstraint_ToString()
        {
            NumberConstraint.From(0).ToString().Should().Be("Number 0");
            NumberConstraint.From(42).ToString().Should().Be("Number 42");
            NumberConstraint.From(1, 1).ToString().Should().Be("Number 1");
            NumberConstraint.From(null, null).ToString().Should().Be("Number from * to *");
            NumberConstraint.From(null, 42).ToString().Should().Be("Number from * to 42");
            NumberConstraint.From(42, null).ToString().Should().Be("Number from 42 to *");
            NumberConstraint.From(-1, null).ToString().Should().Be("Number from -1 to *");
            NumberConstraint.From(-4321, -42).ToString().Should().Be("Number from -4321 to -42");
        }
    }
}

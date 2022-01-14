/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    [TestClass]
    public class SymbolicValueTest
    {
        [TestMethod]
        public void SymbolicValueCounter_ReturnsUniqueFromOne()
        {
            var counter = new SymbolicValueCounter();
            counter.NextIdentifier().Should().Be(1);
            counter.NextIdentifier().Should().Be(2);
            counter.NextIdentifier().Should().Be(3);
            counter.NextIdentifier().Should().Be(4);
            counter.NextIdentifier().Should().Be(5);
        }

        [TestMethod]
        public void ToString_NoConstraints()
        {
            var counter = new SymbolicValueCounter();
            counter.NextIdentifier();
            counter.NextIdentifier();
            counter.NextIdentifier();
            new SymbolicValue(counter).ToString().Should().Be("SV_4");
            new SymbolicValue(counter).ToString().Should().Be("SV_5");
        }

        [TestMethod]
        public void ToString_WithConstraints()
        {
            var sut = new SymbolicValue(new SymbolicValueCounter());
            sut.ToString().Should().Be("SV_1");

            sut.SetConstraint(LockConstraint.Held);
            sut.ToString().Should().Be("SV_1: Held");

            sut.SetConstraint(TestConstraint.First);
            sut.ToString().Should().Be("SV_1: Held, First");

            sut.SetConstraint(TestConstraint.Second);   // Override First to Second
            sut.ToString().Should().Be("SV_1: Held, Second");

            sut.SetConstraint(DummyConstraint.Dummy);
            sut.ToString().Should().Be("SV_1: Held, Second, Dummy");
        }

        [TestMethod]
        public void SetConstraint_Contains()
        {
            var sut = new SymbolicValue(new SymbolicValueCounter());
            sut.SetConstraint(TestConstraint.First);
            sut.HasConstraint(TestConstraint.First).Should().BeTrue();
            sut.HasConstraint(TestConstraint.Second).Should().BeFalse();
            sut.HasConstraint(LockConstraint.Held).Should().BeFalse();
        }

        [TestMethod]
        public void SetConstraint_Overrides()
        {
            var sut = new SymbolicValue(new SymbolicValueCounter());
            sut.SetConstraint(TestConstraint.First);
            sut.SetConstraint(TestConstraint.Second);
            sut.HasConstraint(TestConstraint.First).Should().BeFalse();
            sut.HasConstraint(TestConstraint.Second).Should().BeTrue();
        }

        [TestMethod]
        public void RemoveConstraint_RemovesOnlyTheSame()
        {
            var sut = new SymbolicValue(new SymbolicValueCounter());
            sut.RemoveConstraint(TestConstraint.First);     // Do nothing
            sut.SetConstraint(TestConstraint.First);
            sut.RemoveConstraint(TestConstraint.Second);    // Do nothing
            sut.HasConstraint(TestConstraint.First).Should().BeTrue();
            sut.RemoveConstraint(TestConstraint.First);
            sut.HasConstraint(TestConstraint.First).Should().BeFalse();
        }

        [TestMethod]
        public void HasConstraint_ByType()
        {
            var sut = new SymbolicValue(new SymbolicValueCounter());
            sut.HasConstraint<TestConstraint>().Should().BeFalse();
            sut.SetConstraint(TestConstraint.First);
            sut.HasConstraint<TestConstraint>().Should().BeTrue();
            sut.HasConstraint<LockConstraint>().Should().BeFalse();
        }

        [TestMethod]
        public void HasConstraint_ByValue()
        {
            var sut = new SymbolicValue(new SymbolicValueCounter());
            sut.HasConstraint(TestConstraint.First).Should().BeFalse();
            sut.SetConstraint(TestConstraint.First);
            sut.HasConstraint(TestConstraint.First).Should().BeTrue();
            sut.HasConstraint(TestConstraint.Second).Should().BeFalse();
        }
    }
}

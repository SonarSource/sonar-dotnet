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
            var sut = new SymbolicValue(new());
            sut.ToString().Should().Be("SV_1");

            sut = sut.WithConstraint(LockConstraint.Held);
            sut.ToString().Should().Be("SV_2: Held");

            sut = sut.WithConstraint(TestConstraint.First);
            sut.ToString().Should().Be("SV_3: Held, First");

            sut = sut.WithConstraint(TestConstraint.Second);   // Override First to Second
            sut.ToString().Should().Be("SV_4: Held, Second");

            sut = sut.WithConstraint(DummyConstraint.Dummy);
            sut.ToString().Should().Be("SV_5: Held, Second, Dummy");
        }

        [TestMethod]
        public void WithConstraint_Contains()
        {
            var sut = new SymbolicValue(new()).WithConstraint(TestConstraint.First);
            sut.HasConstraint(TestConstraint.First).Should().BeTrue();
            sut.HasConstraint(TestConstraint.Second).Should().BeFalse();
            sut.HasConstraint(LockConstraint.Held).Should().BeFalse();
        }

        [TestMethod]
        public void WithConstraint_Overrides_IsImmutable()
        {
            var one = new SymbolicValue(new()).WithConstraint(TestConstraint.First);
            var two = one.WithConstraint(TestConstraint.Second);
            one.HasConstraint(TestConstraint.First).Should().BeTrue();
            two.HasConstraint(TestConstraint.First).Should().BeFalse();
            two.HasConstraint(TestConstraint.Second).Should().BeTrue();
        }

        [TestMethod]
        public void WithoutConstraint_RemovesOnlyTheSame()
        {
            var sut = new SymbolicValue(new())
                .WithoutConstraint(TestConstraint.First)    // Do nothing
                .WithConstraint(TestConstraint.First)
                .WithoutConstraint(TestConstraint.Second);  // Do nothing
            sut.HasConstraint(TestConstraint.First).Should().BeTrue();
            sut = sut.WithoutConstraint(TestConstraint.First);
            sut.HasConstraint(TestConstraint.First).Should().BeFalse();
        }

        [TestMethod]
        public void HasConstraint_ByType()
        {
            var sut = new SymbolicValue(new());
            sut.HasConstraint<TestConstraint>().Should().BeFalse();
            sut = sut.WithConstraint(TestConstraint.First);
            sut.HasConstraint<TestConstraint>().Should().BeTrue();
            sut.HasConstraint<LockConstraint>().Should().BeFalse();
        }

        [TestMethod]
        public void HasConstraint_ByValue()
        {
            var sut = new SymbolicValue(new());
            sut.HasConstraint(TestConstraint.First).Should().BeFalse();
            sut = sut.WithConstraint(TestConstraint.First);
            sut.HasConstraint(TestConstraint.First).Should().BeTrue();
            sut.HasConstraint(TestConstraint.Second).Should().BeFalse();
        }

        [TestMethod]
        public void GetHashCode_AlwaysZero()
        {
            var counter = new SymbolicValueCounter();
            new SymbolicValue(counter).GetHashCode().Should().Be(0);
            new SymbolicValue(counter).GetHashCode().Should().Be(0);
            var sut = new SymbolicValue(counter);
            sut = sut.WithConstraint(TestConstraint.First);
            sut.GetHashCode().Should().Be(0);
            sut = sut.WithConstraint(BoolConstraint.True);
            sut.GetHashCode().Should().Be(0);
        }

        [TestMethod]
        public void Equals_ReturnsTrueForSameConstraints()
        {
            var counter = new SymbolicValueCounter();
            var basicSingle = new SymbolicValue(counter).WithConstraint(TestConstraint.First);
            var sameSingle = new SymbolicValue(counter).WithConstraint(TestConstraint.First);
            var basicMore = new SymbolicValue(counter).WithConstraint(TestConstraint.First).WithConstraint(BoolConstraint.True);
            var sameMore = new SymbolicValue(counter).WithConstraint(TestConstraint.First).WithConstraint(BoolConstraint.True);
            var differentConstraintValue = new SymbolicValue(counter).WithConstraint(TestConstraint.Second);
            var differentConstraintType = new SymbolicValue(counter).WithConstraint(BoolConstraint.True);

            basicSingle.Equals(basicSingle).Should().BeTrue();
            basicSingle.Equals(sameSingle).Should().BeTrue();
            basicMore.Equals(basicMore).Should().BeTrue();
            basicMore.Equals(sameMore).Should().BeTrue();

            basicSingle.Equals(basicMore).Should().BeFalse();
            basicSingle.Equals(differentConstraintValue).Should().BeFalse();
            basicSingle.Equals(differentConstraintType).Should().BeFalse();
            basicSingle.Equals("different type").Should().BeFalse();
            basicSingle.Equals((object)null).Should().BeFalse();
            basicSingle.Equals((SymbolicValue)null).Should().BeFalse();     // Explicit cast to ensure correct overload
        }
    }
}

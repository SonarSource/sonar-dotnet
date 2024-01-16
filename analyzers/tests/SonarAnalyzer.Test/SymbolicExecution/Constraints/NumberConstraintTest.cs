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

using System.Numerics;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.Test.SymbolicExecution;

[TestClass]
public class NumberConstraintTest
{
    [TestMethod]
    public void Min_Max_Opposite()
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

    [DataTestMethod]
    [DataRow((sbyte)42)]
    [DataRow((byte)42)]
    [DataRow((short)42)]
    [DataRow((ushort)42)]
    [DataRow((int)42)]
    [DataRow((uint)42)]
    [DataRow((long)42)]
    [DataRow((ulong)42)]
    public void From_Object_IntegralTypes(object value)
    {
        var sut = NumberConstraint.From(value);
        sut.Should().NotBeNull();
        sut.Min.Should().Be(new BigInteger(42));
        sut.Max.Should().Be(new BigInteger(42));
    }

    [TestMethod]
    public void From_Object_NativeInt()
    {
        NumberConstraint.From((nint)42).Should().NotBeNull();
        NumberConstraint.From((nuint)42).Should().NotBeNull();
    }

    [TestMethod]
    public void From_Object_OtherTypes()
    {
        NumberConstraint.From(new Exception()).Should().BeNull();
        NumberConstraint.From("Lorem ipsum").Should().BeNull();
        NumberConstraint.From((object)'*').Should().BeNull();
        NumberConstraint.From(null, null).Should().BeNull();
    }

    [DataTestMethod]
    [DataRow(0, 0)]
    [DataRow(null, 42)]
    [DataRow(42, null)]
    [DataRow(0, 100)]
    public void From_IsCached(int? min, int? max)
    {
        NumberConstraint.ResetCache();  // Otherwise the cache test could reset this half way through the testing
        NumberConstraint.From(min, max).Should().BeSameAs(NumberConstraint.From(min, max));
    }

    [TestMethod]
    public void From_Cache_ResetsAfterLimit()
    {
        var zero = NumberConstraint.From(0);
        zero.Should().BeSameAs(NumberConstraint.From(0));
        for (var i = 0; i <= 100_000; i++)
        {
            NumberConstraint.From(i);   // Abuse the cache
        }
        zero.Should().NotBeSameAs(NumberConstraint.From(0), "Cache should have been cleared and this should be a new instance");
    }

    [TestMethod]
    public void From_ResetCache()
    {
        var value = NumberConstraint.From(42);
        value.Should().BeSameAs(NumberConstraint.From(42));
        NumberConstraint.ResetCache();
        value.Should().NotBeSameAs(NumberConstraint.From(42));
    }

    [TestMethod]
    public void IsSingleValue()
    {
        NumberConstraint.From(42).IsSingleValue.Should().BeTrue();
        NumberConstraint.From(1, 1).IsSingleValue.Should().BeTrue();
        NumberConstraint.From(null, 42).IsSingleValue.Should().BeFalse();
        NumberConstraint.From(42, null).IsSingleValue.Should().BeFalse();
        NumberConstraint.From(1, 100).IsSingleValue.Should().BeFalse();
    }

    [TestMethod]
    public void IsFullyPositive()
    {
        NumberConstraint.From(42).IsPositive.Should().BeTrue();
        NumberConstraint.From(-42).IsPositive.Should().BeFalse();
        NumberConstraint.From(0).IsPositive.Should().BeTrue();
        NumberConstraint.From(5, 42).IsPositive.Should().BeTrue();
        NumberConstraint.From(0, 42).IsPositive.Should().BeTrue();
        NumberConstraint.From(5, null).IsPositive.Should().BeTrue();
        NumberConstraint.From(0, null).IsPositive.Should().BeTrue();
        NumberConstraint.From(-42, -5).IsPositive.Should().BeFalse();
        NumberConstraint.From(null, -5).IsPositive.Should().BeFalse();
        NumberConstraint.From(-42, 42).IsPositive.Should().BeFalse();
        NumberConstraint.From(-42, 0).IsPositive.Should().BeFalse();
        NumberConstraint.From(-42, null).IsPositive.Should().BeFalse();
        NumberConstraint.From(null, 42).IsPositive.Should().BeFalse();
        NumberConstraint.From(null, 0).IsPositive.Should().BeFalse();
    }

    [TestMethod]
    public void IsFullyNegative()
    {
        NumberConstraint.From(42).IsNegative.Should().BeFalse();
        NumberConstraint.From(-42).IsNegative.Should().BeTrue();
        NumberConstraint.From(0).IsNegative.Should().BeFalse();
        NumberConstraint.From(5, 42).IsNegative.Should().BeFalse();
        NumberConstraint.From(0, 42).IsNegative.Should().BeFalse();
        NumberConstraint.From(5, null).IsNegative.Should().BeFalse();
        NumberConstraint.From(0, null).IsNegative.Should().BeFalse();
        NumberConstraint.From(-42, -5).IsNegative.Should().BeTrue();
        NumberConstraint.From(null, -5).IsNegative.Should().BeTrue();
        NumberConstraint.From(-42, 42).IsNegative.Should().BeFalse();
        NumberConstraint.From(-42, 0).IsNegative.Should().BeFalse();
        NumberConstraint.From(-42, null).IsNegative.Should().BeFalse();
        NumberConstraint.From(null, 42).IsNegative.Should().BeFalse();
        NumberConstraint.From(null, 0).IsPositive.Should().BeFalse();
    }

    [TestMethod]
    public void ToString_Serialization()
    {
        NumberConstraint.From(0).ToString().Should().Be("Number 0");
        NumberConstraint.From(42).ToString().Should().Be("Number 42");
        NumberConstraint.From(1, 1).ToString().Should().Be("Number 1");
        NumberConstraint.From(null, 42).ToString().Should().Be("Number from * to 42");
        NumberConstraint.From(42, null).ToString().Should().Be("Number from 42 to *");
        NumberConstraint.From(-1, null).ToString().Should().Be("Number from -1 to *");
        NumberConstraint.From(-4321, -42).ToString().Should().Be("Number from -4321 to -42");
    }

    [DataTestMethod]
    [DataRow(42, null)]
    [DataRow(null, 42)]
    [DataRow(42, 42)]
    public void Equals_ReturnsTrueForEquivalent(int? min, int? max)
    {
        var sut = NumberConstraint.From(min, max);
        sut.Equals(NumberConstraint.From(min, max)).Should().BeTrue();
        sut.Equals(NumberConstraint.From(-10, 100)).Should().BeFalse();
        sut.Equals(NumberConstraint.From(min, 100)).Should().BeFalse();
        sut.Equals(NumberConstraint.From(-10, max)).Should().BeFalse();
        sut.Equals("Lorem ipsum").Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow(42, null)]
    [DataRow(null, 42)]
    [DataRow(42, 42)]
    public void GetHashCode_ReturnsSameForEquivalent(int? min, int? max) =>
        NumberConstraint.From(min, max).GetHashCode().Should()
            .Be(NumberConstraint.From(min, max).GetHashCode())
            .And.NotBe(0)
            .And.NotBe(NumberConstraint.From(-10, 100).GetHashCode())
            .And.NotBe(NumberConstraint.From(min, 100).GetHashCode())
            .And.NotBe(NumberConstraint.From(-10, max).GetHashCode());

    [DataTestMethod]
    [DataRow(null, 42, 0)]
    [DataRow(null, 42, 42)]
    [DataRow(0, 42, 0)]
    [DataRow(0, 42, 10)]
    [DataRow(0, 42, 42)]
    [DataRow(42, null, 42)]
    [DataRow(42, null, 100)]
    public void CanContain_True(int? min, int? max, int value) =>
        NumberConstraint.From(min, max).CanContain(value).Should().BeTrue();

    [DataTestMethod]
    [DataRow(null, 42, 43)]
    [DataRow(null, 42, 100)]
    [DataRow(0, 42, -100)]
    [DataRow(0, 42, -1)]
    [DataRow(0, 42, 43)]
    [DataRow(0, 42, 100)]
    [DataRow(42, null, 0)]
    [DataRow(42, null, 41)]
    public void CanContain_False(int? min, int? max, int value) =>
        NumberConstraint.From(min, max).CanContain(value).Should().BeFalse();

    [DataTestMethod]
    [DataRow(null, 42, null, 42)]
    [DataRow(null, 42, null, 0)]
    [DataRow(null, 42, 0, 0)]
    [DataRow(null, 42, 0, 100)]
    [DataRow(null, 42, 0, null)]
    [DataRow(null, 42, null, null)]
    [DataRow(0, null, 0, null)]
    [DataRow(0, null, 0, 42)]
    [DataRow(0, null, 10, 42)]
    [DataRow(0, null, -10, 42)]
    [DataRow(0, null, null, 42)]
    [DataRow(0, null, null, null)]
    [DataRow(0, 42, 0, 42)]
    [DataRow(0, 42, -10, 42)]
    [DataRow(0, 42, null, 42)]
    [DataRow(0, 42, -10, 10)]
    [DataRow(0, 42, 10, 20)]
    [DataRow(0, 42, 10, 100)]
    [DataRow(0, 42, 10, null)]
    [DataRow(0, 42, 0, 100)]
    [DataRow(0, 42, 0, null)]
    [DataRow(0, 42, null, null)]
    public void Overlaps_True(int? min, int? max, int? otherMin, int? otherMax) =>
        NumberConstraint.From(min, max).Overlaps(NumberConstraint.From(otherMin, otherMax)).Should().BeTrue();

    [DataTestMethod]
    [DataRow(42, null, null, 0)]
    [DataRow(42, null, -10, 0)]
    [DataRow(42, 100, null, 0)]
    [DataRow(42, 100, -10, 0)]
    [DataRow(null, 0, 42, null)]
    [DataRow(null, 0, 42, 100)]
    [DataRow(-10, 0, 42, null)]
    [DataRow(-10, 0, 42, 100)]
    public void Overlaps_False(int? min, int? max, int? otherMin, int? otherMax) =>
        NumberConstraint.From(min, max).Overlaps(NumberConstraint.From(otherMin, otherMax)).Should().BeFalse();
}

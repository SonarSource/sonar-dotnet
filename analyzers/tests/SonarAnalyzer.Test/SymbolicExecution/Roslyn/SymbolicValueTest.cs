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

using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.Test.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

[TestClass]
public class SymbolicValueTest
{
    [TestMethod]
    public void ToString_NoConstraints() =>
        SymbolicValue.Empty.ToString().Should().Be("No constraints");

    [TestMethod]
    public void ToString_WithConstraints()
    {
        var sut = SymbolicValue.Empty;

        sut = sut.WithConstraint(LockConstraint.Held);
        sut.ToString().Should().Be("LockHeld");

        sut = sut.WithConstraint(TestConstraint.First);
        sut.ToString().Should().Be("First, LockHeld");

        sut = sut.WithConstraint(TestConstraint.Second);   // Override First to Second
        sut.ToString().Should().Be("LockHeld, Second");

        sut = sut.WithConstraint(DummyConstraint.Dummy);
        sut.ToString().Should().Be("Dummy, LockHeld, Second");
    }

    [TestMethod]
    public void WithConstraint_Contains()
    {
        var sut = SymbolicValue.Empty.WithConstraint(TestConstraint.First);
        sut.HasConstraint(TestConstraint.First).Should().BeTrue();
        sut.HasConstraint(TestConstraint.Second).Should().BeFalse();
        sut.HasConstraint(LockConstraint.Held).Should().BeFalse();
    }

    [TestMethod]
    public void WithConstraint_Overwrites_IsImmutable()
    {
        var one = SymbolicValue.Empty.WithConstraint(TestConstraint.First);
        var two = one.WithConstraint(TestConstraint.Second);
        one.HasConstraint(TestConstraint.First).Should().BeTrue();
        two.HasConstraint(TestConstraint.First).Should().BeFalse();
        two.HasConstraint(TestConstraint.Second).Should().BeTrue();
    }

    [TestMethod]
    public void WithoutConstraint_RemovesOnlyTheSame()
    {
        var sut = SymbolicValue.Empty
            .WithoutConstraint(TestConstraint.First)    // Do nothing
            .WithConstraint(TestConstraint.First)
            .WithoutConstraint(TestConstraint.Second);  // Do nothing
        sut.HasConstraint(TestConstraint.First).Should().BeTrue();
        sut = sut.WithoutConstraint(TestConstraint.First);
        sut.Should().BeNull();
    }

    [TestMethod]
    public void WithoutConstraint_RemovesType()
    {
        var sut = SymbolicValue.Empty
            .WithConstraint(TestConstraint.First)
            .WithConstraint(DummyConstraint.Dummy)
            .WithoutConstraint<TestConstraint>();   // Act
        sut.HasConstraint<TestConstraint>().Should().BeFalse();
        sut.HasConstraint<DummyConstraint>().Should().BeTrue();
    }

    [TestMethod]
    public void HasConstraint_ByType()
    {
        var sut = SymbolicValue.Empty;
        sut.HasConstraint<TestConstraint>().Should().BeFalse();
        sut = sut.WithConstraint(TestConstraint.First);
        sut.HasConstraint<TestConstraint>().Should().BeTrue();
        sut.HasConstraint<LockConstraint>().Should().BeFalse();
    }

    [TestMethod]
    public void HasConstraint_ByValue()
    {
        var sut = SymbolicValue.Empty;
        sut.HasConstraint(TestConstraint.First).Should().BeFalse();
        sut = sut.WithConstraint(TestConstraint.First);
        sut.HasConstraint(TestConstraint.First).Should().BeTrue();
        sut.HasConstraint(TestConstraint.Second).Should().BeFalse();
    }

    [TestMethod]
    public void Constraint_Existing_ReturnsInstance()
    {
        var sut = SymbolicValue.Empty.WithConstraint(TestConstraint.First).WithConstraint(DummyConstraint.Dummy);
        sut.Constraint<TestConstraint>().Should().Be(TestConstraint.First);
    }

    [TestMethod]
    public void Constraint_Missing_ReturnsNull() =>
        SymbolicValue.Empty.Constraint<BoolConstraint>().Should().BeNull();

    [TestMethod]
    public void GetHashCode_ComputedFromConstraints()
    {
        var empty1 = SymbolicValue.Empty;
        var empty2 = SymbolicValue.Empty;
        var basic = SymbolicValue.Empty.WithConstraint(TestConstraint.First);
        var same = SymbolicValue.Empty.WithConstraint(TestConstraint.First);
        var different = SymbolicValue.Empty.WithConstraint(BoolConstraint.True);
        empty1.GetHashCode().Should().Be(empty2.GetHashCode()).And.Be(0);   // Hash seed for empty dictionary is zero
        basic.GetHashCode().Should().Be(basic.GetHashCode()).And.NotBe(0);
        basic.GetHashCode().Should().Be(same.GetHashCode());
        basic.GetHashCode().Should().NotBe(different.GetHashCode());
    }

    [TestMethod]
    public void Equals_ReturnsTrueForSameConstraints()
    {
        var basicSingle = SymbolicValue.Empty.WithConstraint(TestConstraint.First);
        var sameSingle = SymbolicValue.Empty.WithConstraint(TestConstraint.First);
        var basicMore = SymbolicValue.Empty.WithConstraint(TestConstraint.First).WithConstraint(BoolConstraint.True);
        var sameMore = SymbolicValue.Empty.WithConstraint(TestConstraint.First).WithConstraint(BoolConstraint.True);
        var differentConstraintValue = SymbolicValue.Empty.WithConstraint(TestConstraint.Second);
        var differentConstraintType = SymbolicValue.Empty.WithConstraint(BoolConstraint.True);

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

    [TestMethod]
    public void AllConstraints_Empty() =>
        SymbolicValue.Empty.AllConstraints.Should().BeEmpty();

    [TestMethod]
    public void AllConstraints_ResturnsConstraints()
    {
        var sut = SymbolicValue.Empty.WithConstraint(TestConstraint.First).WithConstraint(DummyConstraint.Dummy);
        sut.AllConstraints.Should().HaveCount(2).And.Contain(new SymbolicConstraint[] { TestConstraint.First, DummyConstraint.Dummy });
    }

    [TestMethod]
    public void SingleCache_AddSameConstraintKind_FromPredefined()
    {
        var sut = SymbolicValue.Null;
        sut.Should().BeSameAs(SymbolicValue.Null);
        sut.Should().BeSameAs(sut.WithConstraint(ObjectConstraint.Null));
    }

    [TestMethod]
    public void SingleCache_AddSameConstraintKind_FromCustom()
    {
        var sut = SymbolicValue.Empty.WithConstraint(DummyConstraint.Dummy);
        sut.Should().BeSameAs(SymbolicValue.Empty.WithConstraint(DummyConstraint.Dummy));
        sut.Should().BeSameAs(sut.WithConstraint(DummyConstraint.Dummy));
    }

    [TestMethod]
    public void SingleCache_AddSameConstraintType()
    {
        SymbolicValue.Empty.WithConstraint(ObjectConstraint.Null).Should().BeSameAs(SymbolicValue.Null);
        SymbolicValue.NotNull.WithConstraint(ObjectConstraint.Null).Should().BeSameAs(SymbolicValue.Null);
    }

    [TestMethod]
    public void PairCache_OrderDoesNotMatter()
    {
        var one = SymbolicValue.Empty.WithConstraint(ObjectConstraint.Null).WithConstraint(DummyConstraint.Dummy);
        var two = SymbolicValue.Empty.WithConstraint(DummyConstraint.Dummy).WithConstraint(ObjectConstraint.Null);
        var three = SymbolicValue.Empty.WithConstraint(ObjectConstraint.NotNull).WithConstraint(DummyConstraint.Dummy).WithConstraint(ObjectConstraint.Null);
        one.Should().HaveOnlyConstraints(ObjectConstraint.Null, DummyConstraint.Dummy).And.BeSameAs(two).And.BeSameAs(three);
    }

    [TestMethod]
    public void PairCache_AddOtherConstraintType()
    {
        SymbolicValue.Empty.WithConstraint(ObjectConstraint.Null).Should().BeSameAs(SymbolicValue.Null);
        var one = SymbolicValue.NotNull.WithConstraint(DummyConstraint.Dummy);
        var two = SymbolicValue.NotNull.WithConstraint(DummyConstraint.Dummy);
        one.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, DummyConstraint.Dummy).And.BeSameAs(two);
    }

    [TestMethod]
    public void PairCache_ReplaceExistingConstraintType()
    {
        var sut = SymbolicValue.Null.WithConstraint(TestConstraint.First);
        var one = sut.WithConstraint(TestConstraint.Second).WithConstraint(TestConstraint.First);
        var two = one.WithConstraint(TestConstraint.Second).WithConstraint(TestConstraint.First);
        sut.Should().HaveOnlyConstraints(ObjectConstraint.Null, TestConstraint.First).And.BeSameAs(one).And.BeSameAs(two);
    }

    [TestMethod]
    public void TripletCache_AddOtherConstraintType()
    {
        var one = SymbolicValue.NotNull.WithConstraint(DummyConstraint.Dummy).WithConstraint(TestConstraint.First);
        var two = SymbolicValue.NotNull.WithConstraint(DummyConstraint.Dummy).WithConstraint(TestConstraint.First);
        one.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, DummyConstraint.Dummy, TestConstraint.First).And.NotBeSameAs(two); // Requires cache for triplets
    }

    [TestMethod]
    public void TripletCache_ReplaceExistingConstraintType()
    {
        var sut = SymbolicValue.Null.WithConstraint(TestConstraint.First).WithConstraint(DummyConstraint.Dummy);
        var one = sut.WithConstraint(TestConstraint.Second).WithConstraint(TestConstraint.First);
        var two = one.WithConstraint(TestConstraint.Second).WithConstraint(TestConstraint.First);
        sut.Should().HaveOnlyConstraints(ObjectConstraint.Null, TestConstraint.First, DummyConstraint.Dummy).And.NotBeSameAs(one).And.NotBeSameAs(two); // Requires cache for triplets
    }

    [TestMethod]
    public void SingleCache_RemoveLastEntry_Kind()
    {
        var sut = SymbolicValue.Null;
        sut.WithoutConstraint(ObjectConstraint.Null).Should().BeNull();
    }

    [TestMethod]
    public void SingleCache_RemoveLastEntry_Type()
    {
        var sut = SymbolicValue.Null;
        sut.WithoutConstraint<ObjectConstraint>().Should().BeNull();
    }

    [TestMethod]
    public void SingleCache_RemoveLastEntry_Miss_Kind()
    {
        var sut = SymbolicValue.Null;
        sut.WithoutConstraint(ObjectConstraint.NotNull).Should().BeSameAs(SymbolicValue.Null);
    }

    [TestMethod]
    public void SingleCache_RemoveLastEntry_Miss_Type()
    {
        var sut = SymbolicValue.Null;
        sut.WithoutConstraint<DummyConstraint>().Should().BeSameAs(SymbolicValue.Null);
    }

    [TestMethod]
    public void SingleCache_RemoveSecondLastEntry_Kind()
    {
        var sut = SymbolicValue.Null.WithConstraint(DummyConstraint.Dummy);
        sut.WithoutConstraint(DummyConstraint.Dummy).Should().BeSameAs(SymbolicValue.Null);
    }

    [TestMethod]
    public void SingleCache_RemoveSecondLastEntry_Type()
    {
        var sut = SymbolicValue.Null.WithConstraint(DummyConstraint.Dummy);
        sut.WithoutConstraint<DummyConstraint>().Should().BeSameAs(SymbolicValue.Null);
    }

    [TestMethod]
    public void PairCache_RemoveThirdLastEntry_Kind()
    {
        var sut = SymbolicValue.Null.WithConstraint(DummyConstraint.Dummy).WithConstraint(TestConstraint.First);
        sut.Should().HaveOnlyConstraints(ObjectConstraint.Null, DummyConstraint.Dummy, TestConstraint.First);
        var one = sut.WithoutConstraint(DummyConstraint.Dummy);
        var two = sut.WithoutConstraint(DummyConstraint.Dummy);
        one.Should().HaveOnlyConstraints(ObjectConstraint.Null, TestConstraint.First).And.BeSameAs(two);
    }

    [TestMethod]
    public void PairCache_RemoveThirdLastEntry_Type()
    {
        var sut = SymbolicValue.Null.WithConstraint(DummyConstraint.Dummy).WithConstraint(TestConstraint.First);
        var one = sut.WithoutConstraint<DummyConstraint>();
        var two = sut.WithoutConstraint<DummyConstraint>();
        one.Should().HaveOnlyConstraints(ObjectConstraint.Null, TestConstraint.First).And.BeSameAs(two);
    }

    [TestMethod]
    public void TripletCache_RemoveFourthLastEntry_Kind()
    {
        var boolSymbolValue = SymbolicValue.Empty.WithConstraint(ObjectConstraint.NotNull).WithConstraint(BoolConstraint.True);
        var sut = boolSymbolValue.WithConstraint(DummyConstraint.Dummy).WithConstraint(TestConstraint.First);
        sut.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True, DummyConstraint.Dummy, TestConstraint.First);
        var one = sut.WithoutConstraint(DummyConstraint.Dummy);
        var two = sut.WithoutConstraint(DummyConstraint.Dummy);
        one.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True, TestConstraint.First).And.NotBeSameAs(two); // Requires cache for triplets
        one.WithoutConstraint(TestConstraint.First).Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True).And.BeSameAs(boolSymbolValue);
    }

    [TestMethod]
    public void TripletCache_RemoveFourthLastEntry_Type()
    {
        var boolSymbolValue = SymbolicValue.Empty.WithConstraint(ObjectConstraint.NotNull).WithConstraint(BoolConstraint.True);
        var sut = boolSymbolValue.WithConstraint(DummyConstraint.Dummy).WithConstraint(TestConstraint.First);
        var one = sut.WithoutConstraint<DummyConstraint>();
        var two = sut.WithoutConstraint<DummyConstraint>();
        one.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True, TestConstraint.First).And.NotBeSameAs(two); // Requires cache for triplets
        one.WithoutConstraint<TestConstraint>().Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True).And.BeSameAs(boolSymbolValue);
    }

    [TestMethod]
    public void RemoveEntry_Miss_Returns_Instance_Kind()
    {
        var sut = SymbolicValue.Null.WithConstraint(TestConstraint.First);
        sut.WithoutConstraint(DummyConstraint.Dummy).Should().BeSameAs(sut);
    }

    [TestMethod]
    public void RemoveEntry_Miss_Returns_Instance_Type()
    {
        var sut = SymbolicValue.Null.WithConstraint(TestConstraint.First);
        sut.WithoutConstraint<DummyConstraint>().Should().BeSameAs(sut).And.HaveOnlyConstraints(ObjectConstraint.Null, TestConstraint.First);
    }

    [TestMethod]
    public void GetHashCode_ReturnsDifferentValuesForPredefinedValues()
    {
        SymbolicValue.Empty.GetHashCode().Should().Be(SymbolicValue.Empty.GetHashCode());
        SymbolicValue.Empty.GetHashCode().Should().NotBe(SymbolicValue.Null.GetHashCode())
            .And.NotBe(SymbolicValue.NotNull.GetHashCode())
            .And.NotBe(SymbolicValue.True.GetHashCode())
            .And.NotBe(SymbolicValue.False.GetHashCode());
    }

    [TestMethod]
    public void GetHashCode_DifferentValuesAreUnique()
    {
        new[]
        {
            SymbolicValue.Empty.GetHashCode(),
            SymbolicValue.NotNull.GetHashCode(),
            SymbolicValue.Null.GetHashCode(),
            SymbolicValue.True.GetHashCode(),
            SymbolicValue.False.GetHashCode(),
            SymbolicValue.Empty.WithConstraint(DummyConstraint.Dummy).GetHashCode(),
            SymbolicValue.Empty.WithConstraint(DummyConstraint.Dummy).WithConstraint(TestConstraint.First).WithConstraint(ObjectConstraint.Null).GetHashCode(),
            SymbolicValue.Empty.WithConstraint(DummyConstraint.Dummy).WithConstraint(TestConstraint.First).WithConstraint(ObjectConstraint.NotNull).GetHashCode(),
        }.Should().OnlyHaveUniqueItems();
    }

    [TestMethod]
    public void GetHashCode_UncachedValues()
    {
        var baseConstraint = SymbolicValue.Empty
            .WithConstraint(DummyConstraint.Dummy)
            .WithConstraint(TestConstraint.First)
            .WithConstraint(ObjectConstraint.Null)
            .WithConstraint(BoolConstraint.True);
        baseConstraint.GetHashCode().Should().Be(baseConstraint.GetHashCode());
        var similar = baseConstraint.WithoutConstraint(DummyConstraint.Dummy).WithConstraint(DummyConstraint.Dummy);
        similar.Should().NotBeSameAs(baseConstraint);
        similar.GetHashCode().Should().Be(baseConstraint.GetHashCode());
        similar.Equals(baseConstraint).Should().BeTrue();
    }
}

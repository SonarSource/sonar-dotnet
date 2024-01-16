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

using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Sonar.Constraints;

namespace SonarAnalyzer.Test.SymbolicExecution;

[TestClass]
public class SymbolicConstraintTest
{
    [TestMethod]
    public void BoolConstraint_ToString()
    {
        BoolConstraint.True.ToString().Should().Be("True");
        BoolConstraint.False.ToString().Should().Be("False");
    }

    [TestMethod]
    public void BoolConstraint_From()
    {
        BoolConstraint.From(true).Should().Be(BoolConstraint.True);
        BoolConstraint.From(false).Should().Be(BoolConstraint.False);
    }

    [TestMethod]
    public void ByteCollectionConstraint_ToString()
    {
        ByteCollectionConstraint.CryptographicallyStrong.ToString().Should().Be("CryptographicallyStrong");
        ByteCollectionConstraint.CryptographicallyWeak.ToString().Should().Be("CryptographicallyWeak");
    }

    [TestMethod]
    public void ByteCollectionConstraint_Opposite()
    {
        ByteCollectionConstraint.CryptographicallyStrong.Opposite.Should().Be(null);
    }

    [TestMethod]
    public void CollectionConstraint_ToString()
    {
        CollectionConstraint.Empty.ToString().Should().Be("CollectionEmpty");
        CollectionConstraint.NotEmpty.ToString().Should().Be("CollectionNotEmpty");
    }

    [TestMethod]
    public void CollectionConstraint_Opposite()
    {
        CollectionConstraint.Empty.Opposite.Should().Be(CollectionConstraint.NotEmpty);
        CollectionConstraint.NotEmpty.Opposite.Should().Be(null);
    }

    [TestMethod]
    public void InitializationVectorConstraint_ToString()
    {
        InitializationVectorConstraint.Initialized.ToString().Should().Be("InitializationVectorInitialized");
        InitializationVectorConstraint.NotInitialized.ToString().Should().Be("InitializationVectorNotInitialized");
    }

    [TestMethod]
    public void DisposableConstraint_ToString() =>
        DisposableConstraint.Disposed.ToString().Should().Be("DisposableDisposed");

    [TestMethod]
    public void DisposableConstraint_Opposite() =>
        DisposableConstraint.Disposed.Opposite.Should().BeNull();

    [TestMethod]
    public void LockConstraint_ToString()
    {
        LockConstraint.Held.ToString().Should().Be("LockHeld");
        LockConstraint.Released.ToString().Should().Be("LockReleased");
    }

    [TestMethod]
    public void LockConstraint_Opposite()
    {
        LockConstraint.Held.Opposite.Should().Be(LockConstraint.Released);
        LockConstraint.Released.Opposite.Should().Be(LockConstraint.Held);
    }

    [TestMethod]
    public void NullableConstraint_ToString()
    {
        NullableConstraint.HasValue.ToString().Should().Be("NullableHasValue");
        NullableConstraint.NoValue.ToString().Should().Be("NullableNoValue");
    }

    [TestMethod]
    public void ObjectConstraint_ToString()
    {
        ObjectConstraint.Null.ToString().Should().Be("Null");
        ObjectConstraint.NotNull.ToString().Should().Be("NotNull");
    }

    [TestMethod]
    public void SaltSizeConstraint_ToString()
    {
        SaltSizeConstraint.Short.ToString().Should().Be("SaltSizeShort");
    }

    [TestMethod]
    public void SaltSizeConstraint_Opposite()
    {
        SaltSizeConstraint.Short.Opposite.Should().Be(null);
    }

    [TestMethod]
    public void SerializationConstraint_ToString()
    {
        SerializationConstraint.Safe.ToString().Should().Be("SerializationSafe");
        SerializationConstraint.Unsafe.ToString().Should().Be("SerializationUnsafe");
    }

    [TestMethod]
    public void StringConstraint_ToString()
    {
        StringConstraint.EmptyString.ToString().Should().Be("StringEmpty");
        StringConstraint.FullString.ToString().Should().Be("StringFull");
        StringConstraint.FullOrNullString.ToString().Should().Be("StringFullOrNull");
        StringConstraint.WhiteSpaceString.ToString().Should().Be("StringWhiteSpace");
        StringConstraint.NotWhiteSpaceString.ToString().Should().Be("StringNotWhiteSpace");
        StringConstraint.FullNotWhiteSpaceString.ToString().Should().Be("StringFullNotWhiteSpace");
    }
}

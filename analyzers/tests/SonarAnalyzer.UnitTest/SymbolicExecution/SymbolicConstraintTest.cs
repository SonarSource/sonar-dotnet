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
using SonarAnalyzer.SymbolicExecution.Sonar.Constraints;

namespace SonarAnalyzer.UnitTest.SymbolicExecution
{
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
        public void ByteArrayConstraint_ToString()
        {
            ByteArrayConstraint.Constant.ToString().Should().Be("ByteArrayConstant");
            ByteArrayConstraint.Modified.ToString().Should().Be("ByteArrayModified");
        }

        [TestMethod]
        public void CollectionConstraint_ToString()
        {
            CollectionConstraint.Empty.ToString().Should().Be("CollectionEmpty");
            CollectionConstraint.NotEmpty.ToString().Should().Be("CollectionNotEmpty");
        }

        [TestMethod]
        public void InitializationVectorConstraint_ToString()
        {
            InitializationVectorConstraint.Initialized.ToString().Should().Be("InitializationVectorInitialized");
            InitializationVectorConstraint.NotInitialized.ToString().Should().Be("InitializationVectorNotInitialized");
        }

        [TestMethod]
        public void DisposableConstraint_ToString()
        {
            DisposableConstraint.Disposed.ToString().Should().Be("DisposableDisposed");
            DisposableConstraint.NotDisposed.ToString().Should().Be("DisposableNotDisposed");
        }

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
        public void NumberConstraint_Min_Max()
        {
            var sut = NumberConstraint.From(1, 10);
            sut.Min.Value.Should().Be(1);
            sut.Max.Value.Should().Be(10);

            sut = NumberConstraint.From(10, 1); // Swapped
            sut.Min.Value.Should().Be(1);
            sut.Max.Value.Should().Be(10);

            sut = NumberConstraint.From(1, null);
            sut.Min.Value.Should().Be(1);
            sut.Max.Should().BeNull();

            sut = NumberConstraint.From(null, 100);
            sut.Min.Should().BeNull();
            sut.Max.Value.Should().Be(100);
        }

        [TestMethod]
        public void NumberConstraint_ToString()
        {
            NumberConstraint.From(0).ToString().Should().Be("Number 0");
            NumberConstraint.From(42).ToString().Should().Be("Number 42");
            NumberConstraint.From(null, null).ToString().Should().Be("Number from * to *");
            NumberConstraint.From(null, 42).ToString().Should().Be("Number from * to 42");
            NumberConstraint.From(42, null).ToString().Should().Be("Number from 42 to *");
            NumberConstraint.From(-1, null).ToString().Should().Be("Number from -1 to *");
            NumberConstraint.From(-4321, -42).ToString().Should().Be("Number from -4321 to -42");
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
            SaltSizeConstraint.Safe.ToString().Should().Be("SaltSizeSafe");
            SaltSizeConstraint.Short.ToString().Should().Be("SaltSizeShort");
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
}

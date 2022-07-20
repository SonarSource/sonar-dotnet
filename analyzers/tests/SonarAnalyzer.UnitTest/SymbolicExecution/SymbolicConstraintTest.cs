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

using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Sonar.Constraints;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Sonar.Constraints
{
    [TestClass]
    public class SymbolicConstraintTest
    {
        [Ignore][TestMethod]
        public void BoolConstraint_ToString()
        {
            BoolConstraint.True.ToString().Should().Be("True");
            BoolConstraint.False.ToString().Should().Be("False");
        }

        [Ignore][TestMethod]
        public void BoolConstraint_From()
        {
            BoolConstraint.From(true).Should().Be(BoolConstraint.True);
            BoolConstraint.From(false).Should().Be(BoolConstraint.False);
        }

        [Ignore][TestMethod]
        public void ByteArrayConstraint_ToString()
        {
            ByteArrayConstraint.Constant.ToString().Should().Be("Constant");
            ByteArrayConstraint.Modified.ToString().Should().Be("Modified");
        }

        [Ignore][TestMethod]
        public void CollectionConstraint_ToString()
        {
            CollectionConstraint.Empty.ToString().Should().Be("Empty");
            CollectionConstraint.NotEmpty.ToString().Should().Be("NotEmpty");
        }

        [Ignore][TestMethod]
        public void InitializationVectorConstraint_ToString()
        {
            InitializationVectorConstraint.Initialized.ToString().Should().Be("Initialized");
            InitializationVectorConstraint.NotInitialized.ToString().Should().Be("NotInitialized");
        }

        [Ignore][TestMethod]
        public void DisposableConstraint_ToString()
        {
            DisposableConstraint.Disposed.ToString().Should().Be("Disposed");
            DisposableConstraint.NotDisposed.ToString().Should().Be("NotDisposed");
        }

        [Ignore][TestMethod]
        public void LockConstraint_ToString()
        {
            LockConstraint.Held.ToString().Should().Be("Held");
            LockConstraint.Released.ToString().Should().Be("Released");
        }

        [Ignore][TestMethod]
        public void LockConstraint_Opposite()
        {
            LockConstraint.Held.Opposite.Should().Be(LockConstraint.Released);
            LockConstraint.Released.Opposite.Should().Be(LockConstraint.Held);
        }

        [Ignore][TestMethod]
        public void NullableConstraint_ToString()
        {
            NullableConstraint.HasValue.ToString().Should().Be("HasValue");
            NullableConstraint.NoValue.ToString().Should().Be("NoValue");
        }

        [Ignore][TestMethod]
        public void ObjectConstraint_ToString()
        {
            ObjectConstraint.Null.ToString().Should().Be("Null");
            ObjectConstraint.NotNull.ToString().Should().Be("NotNull");
        }

        [Ignore][TestMethod]
        public void SaltSizeConstraint_ToString()
        {
            SaltSizeConstraint.Safe.ToString().Should().Be("Safe");
            SaltSizeConstraint.Short.ToString().Should().Be("Short");
        }

        [Ignore][TestMethod]
        public void SerializationConstraint_ToString()
        {
            SerializationConstraint.Safe.ToString().Should().Be("Safe");
            SerializationConstraint.Unsafe.ToString().Should().Be("Unsafe");
        }

        [Ignore][TestMethod]
        public void StringConstraint_ToString()
        {
            StringConstraint.EmptyString.ToString().Should().Be("EmptyString");
            StringConstraint.FullString.ToString().Should().Be("FullString");
            StringConstraint.FullOrNullString.ToString().Should().Be("FullOrNullString");
            StringConstraint.WhiteSpaceString.ToString().Should().Be("WhiteSpaceString");
            StringConstraint.NotWhiteSpaceString.ToString().Should().Be("NotWhiteSpaceString");
            StringConstraint.FullNotWhiteSpaceString.ToString().Should().Be("FullNotWhiteSpaceString");
        }
    }
}

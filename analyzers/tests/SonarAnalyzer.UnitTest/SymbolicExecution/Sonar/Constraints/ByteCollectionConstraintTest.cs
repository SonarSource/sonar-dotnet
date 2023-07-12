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

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Sonar.Constraints
{
    [TestClass]
    public class ByteCollectionConstraintTest
    {
        [TestMethod]
        public void GivenByteCollectionIsInitialized_OppositeShouldBe_NotInitialized() =>
            ByteCollectionConstraint.CryptographicallyStrong.Opposite.Should().Be(ByteCollectionConstraint.CryptographicallyWeak);

        [TestMethod]
        public void GivenByteCollectionIsNotInitialized_OppositeShouldBe_Initialized() =>
            ByteCollectionConstraint.CryptographicallyWeak.Opposite.Should().Be(ByteCollectionConstraint.CryptographicallyStrong);

        [TestMethod]
        public void GivenByteCollectionIsInitialized_ToStringShouldBe_Initialized() =>
            ByteCollectionConstraint.CryptographicallyStrong.ToString().Should().Be("CryptographicallyStrong");

        [TestMethod]
        public void GivenByteCollectionIsNotInitialized_ToStringShouldBe_NotInitialized() =>
            ByteCollectionConstraint.CryptographicallyWeak.ToString().Should().Be("CryptographicallyWeak");
    }
}

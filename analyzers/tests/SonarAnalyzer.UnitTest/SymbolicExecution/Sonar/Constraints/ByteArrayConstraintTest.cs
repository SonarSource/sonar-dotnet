/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using SonarAnalyzer.SymbolicExecution.Sonar.Constraints;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Sonar.Constraints
{
    [TestClass]
    public class ByteArrayConstraintTest
    {
        [TestMethod]
        public void GivenByteArrayIsInitialized_OppositeShouldBe_NotInitialized() =>
            ByteArrayConstraint.Constant.Opposite.Should().Be(ByteArrayConstraint.Modified);

        [TestMethod]
        public void GivenByteArrayIsNotInitialized_OppositeShouldBe_Initialized() =>
            ByteArrayConstraint.Modified.Opposite.Should().Be(ByteArrayConstraint.Constant);

        [TestMethod]
        public void GivenByteArrayIsInitialized_ToStringShouldBe_Initialized() =>
            ByteArrayConstraint.Constant.ToString().Should().Be("Constant");

        [TestMethod]
        public void GivenByteArrayIsNotInitialized_ToStringShouldBe_NotInitialized() =>
            ByteArrayConstraint.Modified.ToString().Should().Be("Modified");
    }
}

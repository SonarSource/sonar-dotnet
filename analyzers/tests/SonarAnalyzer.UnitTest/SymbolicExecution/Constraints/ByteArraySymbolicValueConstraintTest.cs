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
using SonarAnalyzer.SymbolicExecution.Common.Constraints;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Constraints
{
    [TestClass]
    public class ByteArraySymbolicValueConstraintTest
    {
        [TestMethod]
        public void GivenByteArrayIsInitialized_OppositeShouldBe_NotInitialized() =>
            ByteArraySymbolicValueConstraint.Constant.OppositeForLogicalNot.Should().Be(ByteArraySymbolicValueConstraint.Modified);

        [TestMethod]
        public void GivenByteArrayIsNotInitialized_OppositeShouldBe_Initialized() =>
            ByteArraySymbolicValueConstraint.Modified.OppositeForLogicalNot.Should().Be(ByteArraySymbolicValueConstraint.Constant);

        [TestMethod]
        public void GivenByteArrayIsInitialized_ToStringShouldBe_Initialized() =>
            ByteArraySymbolicValueConstraint.Constant.ToString().Should().Be("Constant");

        [TestMethod]
        public void GivenByteArrayIsNotInitialized_ToStringShouldBe_NotInitialized() =>
            ByteArraySymbolicValueConstraint.Modified.ToString().Should().Be("Modified");
    }
}

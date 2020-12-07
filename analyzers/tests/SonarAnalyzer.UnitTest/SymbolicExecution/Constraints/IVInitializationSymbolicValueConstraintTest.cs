/*
* SonarAnalyzer for .NET
* Copyright (C) 2015-2020 SonarSource SA
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
    public class IVInitializationSymbolicValueConstraintTest
    {
        [TestMethod]
        public void GivenIvIsInitialized_OppositeShouldBe_NotInitialized() =>
            IVInitializationSymbolicValueConstraint.Initialized.OppositeForLogicalNot.Should().Be(IVInitializationSymbolicValueConstraint.NotInitialized);

        [TestMethod]
        public void GivenIvIsNotInitialized_OppositeShouldBe_Initialized() =>
            IVInitializationSymbolicValueConstraint.NotInitialized.OppositeForLogicalNot.Should().Be(IVInitializationSymbolicValueConstraint.Initialized);

        [TestMethod]
        public void GivenIvIsInitialized_ToStringShouldBe_Initialized() =>
            IVInitializationSymbolicValueConstraint.Initialized.ToString().Should().Be("Initialized");

        [TestMethod]
        public void GivenIvIsNotInitialized_ToStringShouldBe_NotInitialized() =>
            IVInitializationSymbolicValueConstraint.NotInitialized.ToString().Should().Be("NotInitialized");
    }
}

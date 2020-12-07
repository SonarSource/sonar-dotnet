﻿/*
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
    public class KeyInitializationSymbolicValueConstraintTest
    {
        [TestMethod]
        public void GivenKeyIsInitialized_OppositeShouldBe_NotInitialized() =>
            KeyInitializationSymbolicValueConstraint.Initialized.OppositeForLogicalNot.Should().Be(KeyInitializationSymbolicValueConstraint.NotInitialized);

        [TestMethod]
        public void GivenKeyIsNotInitialized_OppositeShouldBe_Initialized() =>
            KeyInitializationSymbolicValueConstraint.NotInitialized.OppositeForLogicalNot.Should().Be(KeyInitializationSymbolicValueConstraint.Initialized);

        [TestMethod]
        public void GivenKeyIsInitialized_ToStringShouldBe_Initialized() =>
            KeyInitializationSymbolicValueConstraint.Initialized.ToString().Should().Be("Initialized");

        [TestMethod]
        public void GivenKeyIsNotInitialized_ToStringShouldBe_NotInitialized() =>
            KeyInitializationSymbolicValueConstraint.NotInitialized.ToString().Should().Be("NotInitialized");
    }
}

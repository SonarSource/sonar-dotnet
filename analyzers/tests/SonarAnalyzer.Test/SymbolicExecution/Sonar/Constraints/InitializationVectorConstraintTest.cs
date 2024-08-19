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

using SonarAnalyzer.SymbolicExecution.Sonar.Constraints;

namespace SonarAnalyzer.Test.SymbolicExecution.Sonar.Constraints
{
    [TestClass]
    public class InitializationVectorConstraintTest
    {
        [TestMethod]
        public void GivenIvIsInitialized_OppositeShouldBe_NotInitialized() =>
            InitializationVectorConstraint.Initialized.Opposite.Should().Be(InitializationVectorConstraint.NotInitialized);

        [TestMethod]
        public void GivenIvIsNotInitialized_OppositeShouldBe_Initialized() =>
            InitializationVectorConstraint.NotInitialized.Opposite.Should().Be(InitializationVectorConstraint.Initialized);

        [TestMethod]
        public void GivenIvIsInitialized_ToStringShouldBe_Initialized() =>
            InitializationVectorConstraint.Initialized.ToString().Should().Be("InitializationVectorInitialized");

        [TestMethod]
        public void GivenIvIsNotInitialized_ToStringShouldBe_NotInitialized() =>
            InitializationVectorConstraint.NotInitialized.ToString().Should().Be("InitializationVectorNotInitialized");
    }
}

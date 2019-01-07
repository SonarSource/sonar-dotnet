/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class DebugHelperTest
    {
        [DataTestMethod]
        [DataRow(null)]
        [DataRow("false")]
        [DataRow("foo")]
        public void IsInternalDebuggingContext_WhenEnvVarValueIsNotTrue_ReturnsFalse(string value)
        {
            SetupRunAndReset(value, false);
        }

        [TestMethod]
        public void IsInternalDebuggingContext_WhenEnvVarValueIsTrue_ReturnsTrue()
        {
            SetupRunAndReset("true", true);
        }

        private void SetupRunAndReset(string value, bool shouldBeTrue)
        {
            // Arrange
            var oldValue = Environment.GetEnvironmentVariable(DebugHelper.AnalyzerInternalDebugEnvVariable);
            Environment.SetEnvironmentVariable(DebugHelper.AnalyzerInternalDebugEnvVariable, value);

            // Act & assert
            if (shouldBeTrue)
            {
                DebugHelper.IsInternalDebuggingContext().Should().BeTrue();
            }
            else
            {
                DebugHelper.IsInternalDebuggingContext().Should().BeFalse();
            }

            // Reset state
            Environment.SetEnvironmentVariable(DebugHelper.AnalyzerInternalDebugEnvVariable, oldValue);
        }
    }
}

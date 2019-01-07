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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class VbcHelperTests
    {
        [DataTestMethod]
        [DataRow(null, false)]
        [DataRow("error", false)]
        [DataRow("error:", false)]
        [DataRow(" error:", true)]
        [DataRow("   error:", true)]
        [DataRow("   error", false)]
        [DataRow(" error :", true)]
        [DataRow(" ErRoR :", true)]
        [DataRow(" error   :", true)]
        [DataRow(" error   ", false)]
        [DataRow("error foo", false)]
        [DataRow("error foo:", false)]
        [DataRow(" error foo:", true)]
        [DataRow(" error foo :", true)]
        [DataRow(" error   foo:", true)]
        [DataRow(" error foo   :", true)]
        [DataRow(" errorfoo:", false)]
        [DataRow("   error foo:", true)]
        [DataRow("   eRrOr fOo:", true)]
        [DataRow("   error in foo:", false)]
        [DataRow("   error in foo :", false)]
        public void IsTextMatchingVbcErrorPattern_ReturnsExpected(string text, bool expectedResult)
        {
            VbcHelper.IsTextMatchingVbcErrorPattern(text).Should().Be(expectedResult);
        }
    }
}

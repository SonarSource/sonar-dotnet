/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

namespace SonarAnalyzer.UnitTest
{
    [TestClass]
    public class TestResultHelper_Tests
    {
        private static readonly DateTime expected = new DateTime(2018, 8, 17);

        [TestMethod]
        public void TestResultHelper_ParseDate_Invalid()
        {
            TestResultHelper.ParseDate("some gibberish").Should().Be(DateTime.MinValue);
        }

        [TestMethod]
        public void TestResultHelper_ParseDate_Incomplete()
        {
            TestResultHelper.ParseDate("2018-08-17").Should().Be(DateTime.MinValue);
        }

        [TestMethod]
        public void TestResultHelper_ParseDate_Valid()
        {
            TestResultHelper.ParseDate("Deploy_FirstName LastName 2018-08-17 14_52_41").Should().Be(expected);
        }

        [TestMethod]
        public void TestResultHelper_GetPreviousRunDate_Same_Date()
        {
            TestResultHelper.GetPreviousRunDate(new[]
            {
                "Deploy_FirstName LastName 2018-08-17 14_52_41",
                "Deploy_FirstName LastName 2018-08-17 14_52_41",
                "Deploy_FirstName LastName 2018-08-17 14_52_41"
            }).Should().Be(expected);
        }

        [TestMethod]
        public void TestResultHelper_GetPreviousRunDate_Empty_Tests()
        {
            TestResultHelper.GetPreviousRunDate(Array.Empty<string>())
                .Should().Be(DateTime.MinValue);
        }

        [TestMethod]
        public void TestResultHelper_GetPreviousRunDate_Invalid_Tests()
        {
            TestResultHelper.GetPreviousRunDate(new[]
            {
                "some gibberish",
                "other biggerish"
            }).Should().Be(DateTime.MinValue);
        }

        [TestMethod]
        public void TestResultHelper_GetPreviousRunDate_Unordered_Tests()
        {
            TestResultHelper.GetPreviousRunDate(new[]
            {
                "Deploy_FirstName LastName 2018-04-17 14_52_41",
                "Deploy_FirstName LastName 2018-09-17 14_52_41",
                "Deploy_FirstName LastName 2018-08-17 14_52_41",
                "Deploy_FirstName LastName 2018-06-17 14_52_41",
            }).Should().Be(expected);
        }
    }
}

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
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class EnumerableExtensionsTest
    {
        [TestMethod]
        public void TestAreEqual_01()
        {
            var c1 = new List<int> { 1, 2, 3 };
            var c2 = new List<string> { "1", "2", "3" };

            c1.Equals(c2, (e1, e2) => e1.ToString() == e2).Should().BeTrue();
        }

        [TestMethod]
        public void TestAreEqual_02()
        {
            var c1 = new List<int> { 1, 2 };
            var c2 = new List<string> { "1", "2", "3" };

            c1.Equals(c2, (e1, e2) => e1.ToString() == e2).Should().BeFalse();
        }

        [TestMethod]
        public void TestAreEqual_03()
        {
            var c1 = new List<int> { 1, 2, 3 };
            var c2 = new List<string> { "1", "2" };

            c1.Equals(c2, (e1, e2) => e1.ToString() == e2).Should().BeFalse();
        }

        [TestMethod]
        public void TestAreEqual_04()
        {
            var c1 = new List<int>();
            var c2 = new List<string> { "1", "2" };

            c1.Equals(c2, (e1, e2) => e1.ToString() == e2).Should().BeFalse();
        }

        [TestMethod]
        public void TestAreEqual_05()
        {
            var c1 = new List<int> { 1 };
            var c2 = new List<string>();

            c1.Equals(c2, (e1, e2) => e1.ToString() == e2).Should().BeFalse();
        }

        [TestMethod]
        public void TestAreEqual_06()
        {
            var c1 = new List<int>();
            var c2 = new List<string>();

            c1.Equals(c2, (e1, e2) => e1.ToString() == e2).Should().BeTrue();
        }

        [TestMethod]
        public void JoinStr_T_String()
        {
            var lst = new[] {
                Tuple.Create(1, "a"),
                Tuple.Create(2, "bb"),
                Tuple.Create(3, "ccc") };

            lst.JoinStr(null, x => x.Item2).Should().Be("abbccc");
            lst.JoinStr(", ", x => x.Item2).Should().Be("a, bb, ccc");
            lst.JoinStr(" ", x => x.Item2 + "!").Should().Be("a! bb! ccc!");
            lst.JoinStr("; ", x => x.Item1 + ":" + x.Item2).Should().Be("1:a; 2:bb; 3:ccc");
        }

        [TestMethod]
        public void JoinStr_T_Int()
        {
            var lst = new[] {
                Tuple.Create(1, "a"),
                Tuple.Create(2, "bb"),
                Tuple.Create(3, "ccc") };

            lst.JoinStr(", ", x => x.Item1).Should().Be("1, 2, 3");
            lst.JoinStr(null, x => x.Item1 + 10).Should().Be("111213");
        }

        [TestMethod]
        public void JoinStr_String()
        {
            new string[] { }.JoinStr(", ").Should().Be("");
            new[] { "a" }.JoinStr(", ").Should().Be("a");
            new[] { "a", "bb", "ccc" }.JoinStr(", ").Should().Be("a, bb, ccc");
            new[] { "a", "bb", "ccc" }.JoinStr(null).Should().Be("abbccc");
        }

        [TestMethod]
        public void JoinStr_Int()
        {
            new int[] { }.JoinStr(", ").Should().Be("");
            new[] { 1 }.JoinStr(", ").Should().Be("1");
            new[] { 1, 22, 333 }.JoinStr(", ").Should().Be("1, 22, 333");
            new[] { 1, 22, 333 }.JoinStr(null).Should().Be("122333");
        }
    }
}

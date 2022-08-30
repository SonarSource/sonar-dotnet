/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System.Globalization;
using System.Threading;

namespace SonarAnalyzer.UnitTest.Extensions
{
    [TestClass]
    public class EnumerableExtensionsTest
    {
        [TestMethod]
        public void JoinAndNull()
        {
            var result = ((object[])null).JoinAnd();
            result.Should().Be(string.Empty);
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow("", null)]
        [DataRow("", "")]
        [DataRow("", "", "")]
        [DataRow("", "", " ", "\t", null)]
        [DataRow("a", "a")]
        [DataRow("a and b", "a", "b")]
        [DataRow("a and b", "a", null, "b")]
        [DataRow("a, b, and c", "a", "b", "c")]
        [DataRow("a, b, and c", "a", null, "b", "", "c")]
        public void JoinAndStrings(string expected, params string[] collection)
        {
            var result = collection.JoinAnd();
            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow("0", 0)]
        [DataRow("0 and 1", 0, 1)]
        [DataRow("0, 1, and 2", 0, 1, 2)]
        [DataRow("1000", 1000)]
        public void JoinAndInts(string expected, params int[] collection)
        {
            var result = collection.JoinAnd();
            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow("08/30/2022 12:29:11", "2022-08-30T12:29:11")]
        [DataRow("08/30/2022 12:29:11 and 12/24/2022 16:00:00", "2022-08-30T12:29:11", "2022-12-24T16:00:00")]
        [DataRow("08/30/2022 12:29:11, 12/24/2022 16:00:00, and 12/31/2022 00:00:00", "2022-08-30T12:29:11", "2022-12-24T16:00:00", "2022-12-31T00:00:00")]
        public void JoinAndDateTime(string expected, params string[] collection)
        {
            var oldLocale = Thread.CurrentThread.CurrentCulture;
            var oldUiLocale = Thread.CurrentThread.CurrentUICulture;
            try
            {
                var culture = CultureInfo.InvariantCulture;
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                var result = collection.Select(x => DateTime.Parse(x)).JoinAnd();
                result.Should().Be(expected);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = oldLocale;
                Thread.CurrentThread.CurrentUICulture = oldUiLocale;
            }
        }

        [TestMethod]
        public void JoinAndMixedClasses()
        {
            var collection = new Exception[]
            {
                new IndexOutOfRangeException("IndexOutOfRangeMessage"),
                new InvalidOperationException("OperationMessage"),
                null,
                new NotSupportedException("NotSupportedMessage"),
            };
            var result = collection.JoinAnd();
            result.Should().Be("System.IndexOutOfRangeException: IndexOutOfRangeMessage, System.InvalidOperationException: OperationMessage, and System.NotSupportedException: NotSupportedMessage");
        }
    }
}

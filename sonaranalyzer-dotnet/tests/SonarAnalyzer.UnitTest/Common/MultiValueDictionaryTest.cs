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

using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.UnitTest.Common
{
    [TestClass]
    public class MultiValueDictionaryTest
    {
        [TestMethod]
        public void MultiValueDictionary_Add()
        {
            var mvd = new MultiValueDictionary<int, int>
            {
                { 5, 42 },
                { 5, 42 },
                { 42, 42 }
            };

            mvd.Keys.Should().HaveCount(2);
            mvd[5].Should().HaveCount(2);
        }

        [TestMethod]
        public void MultiValueDictionary_Add_Set()
        {
            var mvd = MultiValueDictionary<int, int>.Create<HashSet<int>>();
            mvd.Add(5, 42);
            mvd.Add(5, 42);
            mvd.Add(42, 42);

            mvd.Keys.Should().HaveCount(2);
            mvd[5].Should().ContainSingle();
        }

        [TestMethod]
        public void MultiValueDictionary_AddRange()
        {
            var mvd = new MultiValueDictionary<int, int>();
            mvd.AddRangeWithKey(5, new [] { 42, 42 });

            mvd[5].Should().HaveCount(2);
        }
    }
}


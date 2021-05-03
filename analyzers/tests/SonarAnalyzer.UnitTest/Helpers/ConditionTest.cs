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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Condition = SonarAnalyzer.Helpers.Trackers.CSharpBaseTypeTracker.Condition;

namespace SonarAnalyzer.UnitTest.Helpers
{
    public class ConditionTest
    {
        public static readonly Condition True = new Condition(context => true);
        public static readonly Condition False = new Condition(context => false);

        [TestMethod]
        public void True_and_True_is_True()
        {
            var combined = True & True;
            Assert.IsTrue(combined.Invoke(null));
        }

        [TestMethod]
        public void True_and_False_is_False()
        {
            var combined = True & False;
            Assert.IsFalse(combined.Invoke(null));
        }

        [TestMethod]
        public void False_or_False_is_False()
        {
            var combined = False | False;
            Assert.IsFalse(combined.Invoke(null));
        }

        [TestMethod]
        public void True_or_False_is_True()
        {
            var combined = True | False;
            Assert.IsTrue(combined.Invoke(null));
        }

        [TestMethod]
        public void Not_True_is_False()
        {
            var negated = !True;
            Assert.IsFalse(negated.Invoke(null));
        }
    }
}

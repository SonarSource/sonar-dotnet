/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.CFG;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class ProgramPointTests
    {
        public class TestBlock : Block { }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ProgramPoint_Equivalence()
        {
            var block = new TestBlock();
            var pp1 = new ProgramPoint(block, 1);
            var pp2 = new ProgramPoint(block, 1);

            pp2.Should().Be(pp1);
            pp2.GetHashCode().Should().Be(pp1.GetHashCode());
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ProgramPoint_Diff_Offset()
        {
            var block = new TestBlock();
            var pp1 = new ProgramPoint(block, 1);
            var pp2 = new ProgramPoint(block, 2);

            pp2.Should().NotBe(pp1);
            pp2.GetHashCode().Should().NotBe(pp1.GetHashCode());
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ProgramPoint_Diff_Block()
        {
            var pp1 = new ProgramPoint(new TestBlock(), 1);
            var pp2 = new ProgramPoint(new TestBlock(), 1);

            pp2.Should().NotBe(pp1);
            pp2.GetHashCode().Should().NotBe(pp1.GetHashCode());
        }
    }
}

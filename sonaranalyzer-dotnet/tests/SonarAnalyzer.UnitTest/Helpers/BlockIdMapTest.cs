extern alias csharp;
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
using SonarAnalyzer.ControlFlowGraph;

namespace SonarAnalyzer.Helpers.UnitTest
{
    [TestClass]
    public class BlockIdMapTest
    {
        private BlockIdProvider blockId;

        [TestInitialize]
        public void TestInitialize()
        {
            this.blockId = new BlockIdProvider();
        }

        [TestMethod]
        public void Get_Returns_Same_Id_For_Same_Block()
        {
            var block = new TemporaryBlock();

            this.blockId.Get(block).Should().Be("0");
            this.blockId.Get(block).Should().Be("0");
            this.blockId.Get(block).Should().Be("0");
        }

        [TestMethod]
        public void Get_Returns_Different_Id_For_Different_Block()
        {
            var id1 = this.blockId.Get(new TemporaryBlock());
            var id2 = this.blockId.Get(new TemporaryBlock());
            var id3 = this.blockId.Get(new TemporaryBlock());

            id1.Should().Be("0");
            id2.Should().Be("1");
            id3.Should().Be("2");
        }
    }
}

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

using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    [TestClass]
    public class ProgramPointTest
    {
        [TestMethod]
        public void HasSupportedSize_Supported()
        {
            var cfg = TestHelper.CompileCfgBodyCS("var a = true;");
            cfg.Blocks.Should().HaveCount(3);
            ProgramPoint.HasSupportedSize(cfg).Should().BeTrue();

            cfg = TestHelper.CompileCfgBodyCS($"var a = true{Enumerable.Repeat(" && true", 1019).JoinStr(null)};");
            cfg.Blocks.Should().HaveCount(1024);
            ProgramPoint.HasSupportedSize(cfg).Should().BeTrue();
        }

        [TestMethod]
        public void HasSupportedSize_Unsupported()
        {
            var cfg = TestHelper.CompileCfgBodyCS($"var a = true{Enumerable.Repeat(" && true", 1020).JoinStr(null)};");
            cfg.Blocks.Should().HaveCount(1025);
            ProgramPoint.HasSupportedSize(cfg).Should().BeFalse();
        }

        [TestMethod]
        public void Hash()
        {
            var cfg = TestHelper.CompileCfgBodyCS("var a = true;");
            cfg.Blocks.Should().HaveCount(3);

            ProgramPoint.Hash(cfg.Blocks[0], 0).Should().Be(0);
            ProgramPoint.Hash(cfg.Blocks[0], 1).Should().Be(1);
            ProgramPoint.Hash(cfg.Blocks[0], 2).Should().Be(2);
            ProgramPoint.Hash(cfg.Blocks[0], 4).Should().Be(4);
            ProgramPoint.Hash(cfg.Blocks[0], 42).Should().Be(42);
            ProgramPoint.Hash(cfg.Blocks[0], 65535).Should().Be(65535);
            ProgramPoint.Hash(cfg.Blocks[0], 0b1111111111111111111111).Should().Be(4194303);

            ProgramPoint.Hash(cfg.Blocks[1], 0).Should().Be(4194304 + 0);
            ProgramPoint.Hash(cfg.Blocks[1], 1).Should().Be(4194304 + 1);
            ProgramPoint.Hash(cfg.Blocks[1], 2).Should().Be(4194304 + 2);

            ProgramPoint.Hash(cfg.Blocks[2], 0).Should().Be(4194304 * 2 + 0);
            ProgramPoint.Hash(cfg.Blocks[2], 1).Should().Be(4194304 * 2 + 1);
            ProgramPoint.Hash(cfg.Blocks[2], 2).Should().Be(4194304 * 2 + 2);
        }
    }
}

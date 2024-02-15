/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

[TestClass]
public class FinallyPointTest
{
    [TestMethod]
    public void Constructor_Null_Throws()
    {
        var cfg = TestHelper.CompileCfgBodyCS();
        var previous = new FinallyPoint(null, cfg.EntryBlock.FallThroughSuccessor);
        ((Func<FinallyPoint>)(() => new FinallyPoint(previous, null))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("branch");
    }

    [TestMethod]
    public void CreateNext_ReturnsAllFinally_AndThenDestination()
    {
        const string code = """
            try
            {
                try
                {
                    true.ToString();
                }
                finally
                {
                    true.ToString();
                }
            }
            finally
            {
                true.ToString();
            }
            """;
        var cfg = TestHelper.CompileCfgBodyCS(code);
        var sut = new FinallyPoint(null, cfg.Blocks[1].FallThroughSuccessor);
        sut.BlockIndex.Should().Be(2);   // Inner finally

        sut = sut.CreateNext();
        sut.BlockIndex.Should().Be(3);   // Outer finally

        sut = sut.CreateNext();
        sut.BlockIndex.Should().Be(4);   // Exit block (destination of the original successor)
    }
}

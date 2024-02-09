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
public class LoopDetectorTest
{
    [TestMethod]
    public void LoopDetector_For()
    {
        var code = """
            _ = "Before loop";       // Block 1
            for (var i = 0; i < 10; i++)            // Block 2: assignment, Block 3: condition, Block 4: increment
            {
                _ = "Inside loop";   // Block 4
            }
            _ = "After loop";        // Block 5
            """;
        var cfg = TestHelper.CompileCfgBodyCS(code, "bool condition");
        LoopDetector loopDetector = new(cfg);
        cfg.Blocks.Should().HaveCount(7);
        cfg.Blocks.Where(x => x.Ordinal is 3 or 4).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeTrue());
        cfg.Blocks.Where(x => x.Ordinal is 0 or 1 or 2 or 5 or 6).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeFalse());
    }

    [TestMethod]
    public void LoopDetector_ForEach()
    {
        var code = """
            _ = "Before loop";       // Block 1
            foreach (var i in items)                // Block 2: capture, Block 3: MoveNext
            {
                _ = "Inside loop";   // Block 4
            }                                       // Block 5-7: finally
            _ = "After loop";        // Block 8
            """;
        var cfg = TestHelper.CompileCfgBodyCS(code, "int[] items");
        LoopDetector loopDetector = new(cfg);
        cfg.Blocks.Should().HaveCount(10);
        cfg.Blocks.Where(x => x.Ordinal is 3 or 4).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeTrue());
        cfg.Blocks.Where(x => x.Ordinal is 0 or 1 or 2 or 5 or 6 or 7 or 8 or 9).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeFalse());
    }

    [TestMethod]
    public void LoopDetector_While()
    {
        var code = """
            _ = "Before loop";       // Block 1
            while (condition)                       // Block 2
            {
                _ = "Inside loop";   // Block 3
            }
            _ = "After loop";        // Block 4
            """;
        var cfg = TestHelper.CompileCfgBodyCS(code, "bool condition");
        LoopDetector loopDetector = new(cfg);
        cfg.Blocks.Should().HaveCount(6);
        cfg.Blocks.Where(x => x.Ordinal is 2 or 3).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeTrue());
        cfg.Blocks.Where(x => x.Ordinal is 0 or 1 or 4 or 5).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeFalse());
    }

    [TestMethod]
    public void LoopDetector_DoWhile()
    {
        var code = """
            _ = "Before loop";       // Block 1
            do
            {
                _ = "Inside loop";   // Block 2
            }
            while (condition);                      // Block 2
            _ = "After loop";        // Block 3
            """;
        var cfg = TestHelper.CompileCfgBodyCS(code, "bool condition");
        LoopDetector loopDetector = new(cfg);
        cfg.Blocks.Should().HaveCount(5);
        cfg.Blocks.Where(x => x.Ordinal is 2).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeTrue());
        cfg.Blocks.Where(x => x.Ordinal is 0 or 1 or 3 or 4).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeFalse());
    }

    [TestMethod]
    public void LoopDetector_GoTo()
    {
        var code = """
            _ = "Before loop";   // Block 1
            Start:
            _ = "Inside loop";   // Block 2
            if (condition)                      // Block 2
                goto Start;
            _ = "After loop";    // Block 3
            """;
        var cfg = TestHelper.CompileCfgBodyCS(code, "bool condition");
        LoopDetector loopDetector = new(cfg);
        cfg.Blocks.Should().HaveCount(5);
        cfg.Blocks.Where(x => x.Ordinal is 2).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeTrue());
        cfg.Blocks.Where(x => x.Ordinal is 0 or 1 or 3 or 4).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeFalse());
    }

    [TestMethod]
    public void LoopDetector_GoTo_NoLoop()
    {
        var code = """
            goto Three;
            One:
            _ = "One";
            return;
            Two:
            _ = "Two";
            goto One;
            Three:
            _ = "Three";
            goto Two;
            """;
        var cfg = TestHelper.CompileCfgBodyCS(code, "bool condition");
        LoopDetector loopDetector = new(cfg);
        cfg.Blocks.Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeFalse());
    }

    [TestMethod]
    public void LoopDetector_Nested()
    {
        var code = """
            _ = "Before loop";               // Block 1
            while (condition)                               // Block 2
            {
                _ = "Inside outer loop";     // Block 3
                while (condition)                           // Block 4
                {
                    _ = "Inside inner loop"; // Block 5
                }
                _ = "Inside outer loop";     // Block 6
            }
            _ = "After loop";                // Block 7
            """;
        var cfg = TestHelper.CompileCfgBodyCS(code, "bool condition");
        LoopDetector loopDetector = new(cfg);
        cfg.Blocks.Should().HaveCount(9);
        cfg.Blocks.Where(x => x.Ordinal is 2 or 3 or 4 or 5 or 6).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeTrue());
        cfg.Blocks.Where(x => x.Ordinal is 0 or 1 or 7 or 8).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeFalse());
    }

    [TestMethod]
    public void LoopDetector_NotReducable()
    {
        var code = """
            _ = "Before loop";   // Block 1
            Entry1:
            _ = "Entry 1";       // Block 2
            Entry2:
            _ = "Entry 2";       // Block 3
            if (condition)                      // Block 3
                goto Entry1;
            if (condition)                      // Block 4
                goto Entry2;
            _ = "After loop";    // Block 5
            """;
        var cfg = TestHelper.CompileCfgBodyCS(code, "bool condition");
        LoopDetector loopDetector = new(cfg);
        cfg.Blocks.Should().HaveCount(7);
        cfg.Blocks.Where(x => x.Ordinal is 2 or 3 or 4).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeTrue());
        cfg.Blocks.Where(x => x.Ordinal is 0 or 1 or 5 or 6).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeFalse());
    }

    [TestMethod]
    public void LoopDetector_If()
    {
        var code = """
            _ = "Before If";
            if (condition)
            {
                _ = "Inside If";
            }
            else
            {
                _ = "Inside Else";
            }
            _ = "After If";
            _ = condition ? "Inside Ternary" : "Inside Ternary Else";
            _ = ""?.ToString() ?? "Inside Coalesce";
            """;
        var cfg = TestHelper.CompileCfgBodyCS(code, "bool condition");
        LoopDetector loopDetector = new(cfg);
        cfg.Blocks.Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeFalse());
    }

    [TestMethod]
    public void LoopDetector_IfInLoop()
    {
        var code = """
            _ = "Before Loop";   // Block 1
            while (condition)                   // Block 2
            {
                _ = "Before If"; // Block 3
                if (condition)                  // Block 3
                {
                    _ = "If";    // Block 5
                }
                else
                {
                    _ = "Else";  // Block 4
                }
                _ = "After If";  // Block 6
            }
            _ = "After Loop";    // Block 7
            """;
        var cfg = TestHelper.CompileCfgBodyCS(code, "bool condition");
        LoopDetector loopDetector = new(cfg);
        cfg.Blocks.Should().HaveCount(9);
        cfg.Blocks.Where(x => x.Ordinal is 2 or 3 or 4 or 5 or 6).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeTrue());
        cfg.Blocks.Where(x => x.Ordinal is 0 or 1 or 7 or 8).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeFalse());
    }

    [TestMethod]
    public void LoopDetector_TwoIfsInLoop()
    {
        var code = """
            _ = "Before Loop";       // Block 1
            while (condition)                       // Block 2
            {
                _ = "Before If";     // Block 3
                if (condition)                      // Block 3
                {
                    _ = "If 1";      // Block 5
                }
                else
                {
                    _ = "Else 1";    // Block 4
                }
                if (condition)                      // Block 6
                {
                    _ = "If 2";      // Block 8
                }
                else
                {
                    _ = "Else 2";    // Block 7
                }
                _ = "After If";      // Block 9
            }
            _ = "After Loop";        // Block 10
            """;
        var cfg = TestHelper.CompileCfgBodyCS(code, "bool condition");
        LoopDetector loopDetector = new(cfg);
        cfg.Blocks.Should().HaveCount(12);
        cfg.Blocks.Where(x => x.Ordinal is 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeTrue());
        cfg.Blocks.Where(x => x.Ordinal is 0 or 1 or 10 or 11).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeFalse());
    }

    [TestMethod]
    public void LoopDetector_NestedIfsInLoop()
    {
        var code = """
            _ = "Before Loop";               // Block 1
            while (condition)                               // Block 2
            {
                _ = "Before Outer If";       // Block 3
                if (condition)                              // Block 3
                {
                    _ = "Before Inner If";   // Block 4
                    if (condition)                          // Block 4
                    {
                        _ = "Inner If";      // Block 6
                    }
                    else
                    {
                        _ = "Inner Else";    // Block 5
                    }
                    _ = "After Inner If";    // Block 7
                }
                _ = "After If";              // Block 8
            }
            _ = "After Loop";                // Block 9
            """;
        var cfg = TestHelper.CompileCfgBodyCS(code, "bool condition");
        LoopDetector loopDetector = new(cfg);
        cfg.Blocks.Should().HaveCount(11);
        cfg.Blocks.Where(x => x.Ordinal is 2 or 3 or 4 or 5 or 6 or 7 or 8).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeTrue());
        cfg.Blocks.Where(x => x.Ordinal is 0 or 1 or 9 or 10).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeFalse());
    }

    [TestMethod]
    public void LoopDetector_LoopInIf()
    {
        var code = """
            _ = "Before If";         // Block 1
            if (condition)                          // Block 1
            {
                _ = "Before Loop";   // Block 2
                while (condition)                   // Block 3
                {
                    _ = "In Loop";   // Block 4
                }
                _ = "After Loop";    // Block 5
            }
            _ = "After If";          // Block 6
            """;
        var cfg = TestHelper.CompileCfgBodyCS(code, "bool condition");
        LoopDetector loopDetector = new(cfg);
        cfg.Blocks.Should().HaveCount(8);
        cfg.Blocks.Where(x => x.Ordinal is 3 or 4).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeTrue());
        cfg.Blocks.Where(x => x.Ordinal is 0 or 1 or 2 or 5 or 6 or 7).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeFalse());
    }

    [TestMethod]
    public void LoopDetector_LoopAndIfs()
    {
        var code = """
            _ = "Before If 1";   // Block 1
            if (condition)                      // Block 1
            {
                _ = "If 1";      // Block 3
            }
            else
            {
                _ = "Else 1";    // Block 2
            }
            _ = "Before Loop";   // Block 4
            while (condition)                   // Block 5
            {
                _ = "In Loop";   // Block 6
            }
            _ = "After Loop";    // Block 7
            if (condition)                      // Block 7
            {
                _ = "If 2";      // Block 9
            }
            else
            {
                _ = "Else 2";    // Block 8
            }
            _ = "After If 2";    // Block 10
            """;
        var cfg = TestHelper.CompileCfgBodyCS(code, "bool condition");
        LoopDetector loopDetector = new(cfg);
        cfg.Blocks.Should().HaveCount(12);
        cfg.Blocks.Where(x => x.Ordinal is 5 or 6).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeTrue());
        cfg.Blocks.Where(x => x.Ordinal is 0 or 1 or 2 or 3 or 4 or 7 or 8 or 9 or 10 or 11).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeFalse());
    }

    [TestMethod]
    public void LoopDetector_TryCatch()
    {
        var code = """
            _ = "Before loop";               // Block 1
            while (condition)                               // Block 2
            {
                _ = "Inside loop";           // Block 3
                try
                {
                    _ = "Inside try";        // Block 4
                }
                catch
                {
                    _ = "Inside catch";      // Block 5
                }
                finally
                {
                    _ = "Inside finally";    // Block 6
                }
            }
            _ = "After loop";                // Block 7
            """;
        var cfg = TestHelper.CompileCfgBodyCS(code, "bool condition");
        LoopDetector loopDetector = new(cfg);
        cfg.Blocks.Should().HaveCount(9);
        cfg.Blocks.Where(x => x.Ordinal is 2 or 3 or 4).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeTrue());      // should be 2-6
        cfg.Blocks.Where(x => x.Ordinal is 0 or 1 or 5 or 6 or 7 or 8).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeFalse());
    }

    [TestMethod]
    public void LoopDetector_TouchingLoops()
    {
        var code = """
            _ = "Start";    // Block 1
            First:
            _ = "First";    // Block 2
            if (condition)  // Block 2
                goto Third;
            Second:
            _ = "Second";   // Block 3
            if (condition)  // Block 3
                goto Third;
            goto First;
            Third:
            _ = "Third";    // Block 4
            if (condition)  // Block 4
                goto Second;
            _ = "End";      // Block 5
            """;
        var cfg = TestHelper.CompileCfgBodyCS(code, "bool condition");
        LoopDetector loopDetector = new(cfg);
        cfg.Blocks.Should().HaveCount(7);
        cfg.Blocks.Where(x => x.Ordinal is 2 or 3 or 4).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeTrue());
        cfg.Blocks.Where(x => x.Ordinal is 0 or 1 or 5 or 6).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeFalse());
    }

    [TestMethod]
    public void LoopDetector_ParallelLoops()
    {
        var code = """
            One:
            _ = "One";      // Block 1
            if (condition)  // Block 1
                goto Four;
            Two:
            _ = "Two";      // Block 2
            Three:
            _ = "Three";    // Block 3
            if (condition)  // Block 3
                goto Six;
            goto End;
            Four:
            _ = "Four";     // Block 4
            if (condition)  // Block 4
                goto Five;
            goto One;
            Five:
            _ = "Five";     // Block 5
            Six:
            _ = "Six";      // Block 6
            goto Three;
            End:
            _ = "End";      // Block 7
            """;
        var cfg = TestHelper.CompileCfgBodyCS(code, "bool condition");
        LoopDetector loopDetector = new(cfg);
        cfg.Blocks.Should().HaveCount(9);
        cfg.Blocks.Where(x => x.Ordinal is 1 or 3 or 4 or 6).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeTrue());
        cfg.Blocks.Where(x => x.Ordinal is 0 or 2 or 5 or 7 or 8).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeFalse());
    }

    [TestMethod]
    public void LoopDetector_Complex()
    {
        var code = """
            if (condition)                          // Block 1
            {
                goto AlternativeEntry;
            }
            Loop:
            if (condition)                          // Block 2
            {
                goto AfterLoop;
            }
            AlternativeEntry:
            if (condition)                          // Block 3
            {
                goto Skip;
            }
            _ = "Skippable";         // Block 4
            Skip:
            if (condition)                          // Block 5
            {
                goto Loop;
            }
            AfterLoop:
            _ = "After loop";        // Block 6
            """;
        var cfg = TestHelper.CompileCfgBodyCS(code, "bool condition");
        LoopDetector loopDetector = new(cfg);
        cfg.Blocks.Should().HaveCount(8);
        cfg.Blocks.Where(x => x.Ordinal is 2 or 3 or 4 or 5).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeTrue());
        cfg.Blocks.Where(x => x.Ordinal is 0 or 1 or 6 or 7).Should().AllSatisfy(x => loopDetector.IsInLoop(x).Should().BeFalse());
    }
}

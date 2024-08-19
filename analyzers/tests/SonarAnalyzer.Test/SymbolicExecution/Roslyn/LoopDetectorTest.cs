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
    public void LoopDetector_For() =>
        ValidateLoops("""
            _ = "Before loop";              // Block 1
            for (var i = 0; i < 10; i++)    // Block 2: assignment, Block 3: condition, Block 4: increment
            {
                _ = "Inside loop";          // Block 4
            }
            _ = "After loop";               // Block 5
            """,
            [3, 4],
            [1, 2, 5]);

    [TestMethod]
    public void LoopDetector_ForEach() =>
        ValidateLoops("""
            _ = "Before loop";          // Block 1
            foreach (var i in items)    // Block 2: capture, Block 3: MoveNext
            {
                _ = "Inside loop";      // Block 4
            }                           // Block 5-7: finally
            _ = "After loop";           // Block 8
            """,
            [3, 4],
            [1, 2, 5, 6, 7, 8]);

    [TestMethod]
    public void LoopDetector_While() =>
        ValidateLoops("""
            _ = "Before loop";       // Block 1
            while (condition)        // Block 2
            {
                _ = "Inside loop";   // Block 3
            }
            _ = "After loop";        // Block 4
            """,
            [2, 3],
            [1, 4]);

    [TestMethod]
    public void LoopDetector_DoWhile() =>
        ValidateLoops("""
            _ = "Before loop";       // Block 1
            do
            {
                _ = "Inside loop";   // Block 2
            }
            while (condition);       // Block 2
            _ = "After loop";        // Block 3
            """,
            [2],
            [1, 3]);

    [TestMethod]
    public void LoopDetector_GoTo() =>
        ValidateLoops("""
            _ = "Before loop";   // Block 1
            Start:
            _ = "Inside loop";   // Block 2
            if (condition)       // Block 2
                goto Start;
            _ = "After loop";    // Block 3
            """,
            [2],
            [1, 3]);

    [TestMethod]
    public void LoopDetector_GoTo_NoLoop() =>
        ValidateLoops("""
            goto Three;
            One:
            _ = "Last";
            return;
            Two:
            _ = "Middle";
            goto One;
            Three:
            _ = "First";
            goto Two;
            """,
            [],
            [1, 2, 3]);

    [TestMethod]
    public void LoopDetector_Nested() =>
        ValidateLoops("""
            _ = "Before loop";               // Block 1
            while (condition)                // Block 2
            {
                _ = "Inside outer loop";     // Block 3
                while (condition)            // Block 4
                {
                    _ = "Inside inner loop"; // Block 5
                }
                _ = "Inside outer loop";     // Block 6
            }
            _ = "After loop";                // Block 7
            """,
            [2, 3, 4, 5, 6],
            [1, 7]);

    [TestMethod]
    public void LoopDetector_NotReducable() =>
        ValidateLoops("""
            _ = "Before loop";   // Block 1
            Entry1:
            _ = "Entry 1";       // Block 2
            Entry2:
            _ = "Entry 2";       // Block 3
            if (condition)       // Block 3
                goto Entry1;
            if (condition)       // Block 4
                goto Entry2;
            _ = "After loop";    // Block 5
            """,
            [2, 3, 4],
            [1, 5]);

    [TestMethod]
    public void LoopDetector_If() =>
        ValidateLoops("""
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
            """,
            [],
            [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);

    [TestMethod]
    public void LoopDetector_IfInLoop() =>
        ValidateLoops("""
            _ = "Before Loop";   // Block 1
            while (condition)    // Block 2
            {
                _ = "Before If"; // Block 3
                if (condition)   // Block 3
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
            """,
            [2, 3, 4, 5, 6],
            [1, 7]);

    [TestMethod]
    public void LoopDetector_TwoIfsInLoop() =>
        ValidateLoops("""
            _ = "Before Loop";       // Block 1
            while (condition)        // Block 2
            {
                _ = "Before If";     // Block 3
                if (condition)       // Block 3
                {
                    _ = "If 1";      // Block 5
                }
                else
                {
                    _ = "Else 1";    // Block 4
                }
                if (condition)       // Block 6
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
            """,
            [2, 3, 4, 5, 6, 7, 8, 9],
            [1, 10]);

    [TestMethod]
    public void LoopDetector_NestedIfsInLoop() =>
        ValidateLoops("""
            _ = "Before Loop";               // Block 1
            while (condition)                // Block 2
            {
                _ = "Before Outer If";       // Block 3
                if (condition)               // Block 3
                {
                    _ = "Before Inner If";   // Block 4
                    if (condition)           // Block 4
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
            """,
            [2, 3, 4, 5, 6, 7, 8],
            [1, 9]);

    [TestMethod]
    public void LoopDetector_LoopInIf() =>
        ValidateLoops("""
            _ = "Before If";         // Block 1
            if (condition)           // Block 1
            {
                _ = "Before Loop";   // Block 2
                while (condition)    // Block 3
                {
                    _ = "In Loop";   // Block 4
                }
                _ = "After Loop";    // Block 5
            }
            _ = "After If";          // Block 6
            """,
            [3, 4],
            [1, 2, 5, 6]);

    [TestMethod]
    public void LoopDetector_LoopAndIfs() =>
        ValidateLoops("""
            _ = "Before If 1";   // Block 1
            if (condition)       // Block 1
            {
                _ = "If 1";      // Block 3
            }
            else
            {
                _ = "Else 1";    // Block 2
            }
            _ = "Before Loop";   // Block 4
            while (condition)    // Block 5
            {
                _ = "In Loop";   // Block 6
            }
            _ = "After Loop";    // Block 7
            if (condition)       // Block 7
            {
                _ = "If 2";      // Block 9
            }
            else
            {
                _ = "Else 2";    // Block 8
            }
            _ = "After If 2";    // Block 10
            """,
            [5, 6],
            [1, 2, 3, 4, 7, 8, 9, 10]);

    [TestMethod]
    public void LoopDetector_TryCatchFinally() =>
        ValidateLoops("""
            _ = "Before loop";               // Block 1
            while (condition)                // Block 2
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
            """,
            [2, 3, 4, 5, 6],
            [1, 7]);

    [TestMethod]
    public void LoopDetector_TryCatchFinally_Nested() =>
        ValidateLoops("""
            _ = "Before try 1";                 // Block 1
            try
            {
                _ = "Before loop";              // Block 2
                while (condition)               // Block 3
                {
                    _ = "Before try 2";         // Block 4
                    try
                    {
                        _ = "Before try 3";     // Block 5
                        try
                        {
                            _ = "Try 3";        // Block 6
                        }
                        catch
                        {
                            _ = "Catch 3";      // Block 7
                        }
                        finally
                        {
                            _ = "Finally 3";    // Block 8
                        }
                        _ = "After try 3";      // Block 9
                    }
                    catch
                    {
                        _ = "Before try 4";     // Block 10
                        try
                        {
                            _ = "Try 4";        // Block 11
                        }
                        catch
                        {
                            _ = "Catch 4";      // Block 12
                        }
                        finally
                        {
                            _ = "Finally 4";    // Block 13
                        }
                        _ = "After try 4";      // Block 14
                    }
                    finally
                    {
                        _ = "Before try 5";     // Block 15
                        try
                        {
                            _ = "Try 5";        // Block 16
                        }
                        catch
                        {
                            _ = "Catch 5";      // Block 17
                        }
                        finally
                        {
                            _ = "Finally 5";    // Block 18
                        }
                        _ = "After try 5";      // Block 19
                    }
                    _ = "After try 2";          // Block 20
                }
                _ = "After loop";               // Block 21
            }
            catch
            {
                _ = "Before try 6";             // Block 22
                try
                {
                    _ = "Try 6";                // Block 23
                }
                catch
                {
                    _ = "Catch 6";              // Block 24
                }
                finally
                {
                    _ = "Finally 6";            // Block 25
                }
                _ = "After try 6";              // Block 26
            }
            finally
            {
                _ = "Before try 7";             // Block 27
                try
                {
                    _ = "Try 7";                // Block 28
                }
                catch
                {
                    _ = "Catch 7";              // Block 29
                }
                finally
                {
                    _ = "Finally 7";            // Block 30
                }
                _ = "After try 7";              // Block 31
            }
            _ = "After try 1";                  // Block 32
            """,
            [3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20],
            [1, 2, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32]);

    [TestMethod]
    public void LoopDetector_TryCatchWhenFinally() =>
        ValidateLoops("""
            _ = "Before loop";          // Block 1
            while (condition)           // Block 2
            {
                _ = "Inside loop";      // Block 3
                try
                {
                    _ = "Try";          // Block 4
                }
                catch when (condition)  // Block 5
                {
                    _ = "Catch where";  // Block 6
                }
                catch
                {
                    _ = "Catch";        // Block 7
                }
                finally
                {
                    _ = "Finally";      // Block 8
                }
            }
            _ = "After loop";           // Block 9
            """,
            [2, 3, 4, 5, 6, 7, 8],
            [1, 9]);

    [TestMethod]
    public void LoopDetector_IfInTryFinally() =>
        ValidateLoops("""
            _ = "Before loop";                          // Block 1
            while (condition)                           // Block 2
            {
                try
                {
                    _ = "Try";                          // Block 3
                }
                finally
                {
                    if (condition)                      // Block 4
                    {
                        _ = "Not part of the loop";     // Block 5
                        throw new System.Exception();   // Block 5
                    }
                }                                       // Block 6
            }
            _ = "After loop";                           // Block 7
            """,
            [2, 3, 4, 6],
            [1, 5, 7]);

    [TestMethod]
    public void LoopDetector_TouchingLoops() =>
        ValidateLoops("""
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
            """,
            [2, 3, 4],
            [1, 5]);

    [TestMethod]
    public void LoopDetector_ParallelLoops() =>
        ValidateLoops("""
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
            """,
            [1, 3, 4, 6],
            [2, 5, 7]);

    [TestMethod]
    public void LoopDetector_Complex() =>
        ValidateLoops("""
            if (condition)      // Block 1
            {
                goto AlternativeEntry;
            }
            Loop:
            if (condition)      // Block 2
            {
                goto AfterLoop;
            }
            AlternativeEntry:
            if (condition)      // Block 3
            {
                goto Skip;
            }
            _ = "Skippable";    // Block 4
            Skip:
            if (condition)      // Block 5
            {
                goto Loop;
            }
            AfterLoop:
            _ = "After loop";   // Block 6
            """,
            [2, 3, 4, 5],
            [1, 6]);

    private static void ValidateLoops(string snippet, int[] expectedBlocksInLoop, int[] expectedBlocksOutsideLoop)
    {
        var cfg = TestHelper.CompileCfgBodyCS(snippet, "int[] items, bool condition");
        var sut = new LoopDetector(cfg);
        var count = expectedBlocksInLoop.Length + expectedBlocksOutsideLoop.Length;
        cfg.Blocks.Should().HaveCount(count + 2);
        if (expectedBlocksInLoop.Any())
        {
            cfg.Blocks.Where(x => expectedBlocksInLoop.Contains(x.Ordinal)).Should().AllSatisfy(x => sut.IsInLoop(x).Should().BeTrue());
        }
        if (expectedBlocksOutsideLoop.Any())
        {
            cfg.Blocks.Where(x => x.Ordinal is 0 || x.Ordinal == count + 1 || expectedBlocksOutsideLoop.Contains(x.Ordinal)).Should().AllSatisfy(x => sut.IsInLoop(x).Should().BeFalse());
        }
    }
}

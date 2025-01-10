/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Test.LiveVariableAnalysis;

public partial class RoslynLiveVariableAnalysisTest
{
    [TestMethod]
    public void FlowCaptrure_NullCoalescingAssignment()
    {
        /*      Block 1
        *      #0=param
        *          |
        *       Block 2
        *        #1=#0
        *      #1 is null --+
        *          |        |
        *       Block 3     |
        *       #0=Hello    |
        *          |        |
        *        Exit <-----+
        */
        const string code = """param ??= "Hello";""";
        var context = CreateContextCS(code, additionalParameters: "string param");
        context.ValidateEntry(LiveIn("param"), LiveOut("param"));
        context.Validate(context.Cfg.Blocks[1], LiveIn("param"), LiveOut("param"));
        context.Validate(context.Cfg.Blocks[2], LiveIn("param"));
        context.Validate(context.Cfg.Blocks[3]);
        context.ValidateExit();
    }

    [TestMethod]
    public void FlowCaptrure_NullCoalescingOperator()
    {
        /*       Block 1
         *      #0=param
         *     #0 is null
         *        /   \
         *       F     T
         *      /       \
         *  Block 2    Block 3
         *  #1=#0     #1="Hello"
         *       \     /
         *       Block 4
         *      result=#1
         *         |
         *        Exit
         */
        const string code = """var result = param ?? "Hello";""";
        var context = CreateContextCS(code, additionalParameters: "string param");
        context.ValidateEntry(LiveIn("param"), LiveOut("param"));
        context.Validate(context.Cfg.Blocks[1], LiveIn("param"), LiveOut("param"));
        context.Validate(context.Cfg.Blocks[2], LiveIn("param"), LiveOut("param"));
        context.Validate(context.Cfg.Blocks[3], LiveIn("param"), LiveOut("param"));
        context.Validate(context.Cfg.Blocks[4], LiveIn("param"));
        context.ValidateExit();
    }

    [TestMethod]
    public void FlowCaptrure_ConditionalAccess()
    {
        /*       Block 1
         *      #1=param
         *     #1 is null
         *        /   \
         *       F     T
         *      /       \
         *  Block 2    Block 3
         * #0=#1.Length  #0=default
         *      \        /
         *        Block 4
         *       result=#1
         *          |
         *         Exit
         */
        const string code = """var result = param?.Length;""";
        var context = CreateContextCS(code, additionalParameters: "string param");
        context.ValidateEntry(LiveIn("param"), LiveOut("param"));
        context.Validate(context.Cfg.Blocks[1], LiveIn("param"), LiveOut("param"));
        context.Validate(context.Cfg.Blocks[2], LiveIn("param"));
        context.Validate(context.Cfg.Blocks[3]);
        context.Validate(context.Cfg.Blocks[4]);
        context.ValidateExit();
    }

    [TestMethod]
    public void FlowCaptrure_Ternary()
    {
        /*         Block 1
         *       boolParameter
         *        /       \
         *       F         T
         *      /           \
         *     /             \
         *   Block 2        Block 3
         * #0=StringParam   #0="Hello"
         *         |             |
         *      Block 4 <--------+
         *     result=#0
         *         |
         *       Exit
         */
        const string code = """var result = boolParameter ? stringParam : "Hello";""";
        var context = CreateContextCS(code, additionalParameters: "string stringParam");
        context.ValidateEntry(LiveIn("boolParameter", "stringParam"), LiveOut("boolParameter", "stringParam"));
        context.Validate(context.Cfg.Blocks[1], LiveIn("boolParameter", "stringParam"), LiveOut("stringParam"));
        context.Validate(context.Cfg.Blocks[2], LiveIn("stringParam"), LiveOut("stringParam"));
        context.Validate(context.Cfg.Blocks[3], LiveIn("stringParam"), LiveOut("stringParam"));
        context.Validate(context.Cfg.Blocks[4], LiveIn("stringParam"));
        context.ValidateExit();
    }

    [TestMethod]
    public void FlowCaptrure_ReuseCaptures()
    {
        /*       Block 1
         *     boolParameter
         *       /       \
         *      F         T
         *     /           \
         *   Block 2     Block 3
         *   #0=st     #0="Hello"
         *       \     /
         *        Block 4
         *       result1=#0
         *          |
         *        Block5
         *     boolParameter
         *        /    \
         *       F      T
         *      /        \
         *  Block 6    Block 7
         *   #1=st2   #1="Hello"
         *       \     /
         *       Block 8
         *      result2=#1
         *         |
         *        Exit
         */
        const string code = """
            var result1 = boolParameter ? s1 : "Hello";
            var result2 = boolParameter ? s2 : "Hi";
            """;
        var context = CreateContextCS(code, additionalParameters: "string s1, string s2");
        context.ValidateEntry(LiveIn("boolParameter", "s1", "s2"), LiveOut("boolParameter", "s1", "s2"));
        context.Validate(context.Cfg.Blocks[1], LiveIn("boolParameter", "s1", "s2"), LiveOut("boolParameter", "s1", "s2"));
        context.Validate(context.Cfg.Blocks[2], LiveIn("boolParameter", "s1", "s2"), LiveOut("boolParameter", "s1", "s2"));
        context.Validate(context.Cfg.Blocks[3], LiveIn("boolParameter", "s1", "s2"), LiveOut("boolParameter", "s1", "s2"));
        context.Validate(context.Cfg.Blocks[4], LiveIn("boolParameter", "s1", "s2"), LiveOut("boolParameter", "s2"));
        context.Validate(context.Cfg.Blocks[5], LiveIn("boolParameter", "s2"), LiveOut("s2"));
        context.Validate(context.Cfg.Blocks[6], LiveIn("s2"), LiveOut("s2"));
        context.Validate(context.Cfg.Blocks[7], LiveIn("s2"), LiveOut("s2"));
        context.Validate(context.Cfg.Blocks[8], LiveIn("s2"));
        context.ValidateExit();
    }

    [TestMethod]
    public void FlowCaptrure_NullCoalescingOperator_ConsequentCalls()
    {
        /*       Block 1
         *       #0=s1
         *     #0 is null
         *      /       \
         *     F         T
         *    /           \
         *   /             \
         * Block 2      Block 3
         * #1=#0         #2=s2
         *   |        #2 is null
         *   |          /     \
         *   |         F       T
         *   |        /         \
         *   |      Block 4    Block 5
         *   |     #1=#2     #1="Hello"
         *   |       |___________|
         *   |             |
         *   |             |
         *   +--------->Block 6
         *             result=#1
         *                |
         *               Exit
         */
        const string code = """var result = s1 ?? s2 ?? "Hello";""";
        var context = CreateContextCS(code, additionalParameters: "string s1, string s2");
        context.ValidateEntry(LiveIn("s1", "s2"), LiveOut("s1", "s2"));
        context.Validate(context.Cfg.Blocks[1], LiveIn("s1", "s2"), LiveOut("s1", "s2"));
        context.Validate(context.Cfg.Blocks[2], LiveIn("s1", "s2"), LiveOut("s1", "s2"));
        context.Validate(context.Cfg.Blocks[3], LiveIn("s1", "s2"), LiveOut("s1", "s2"));
        context.Validate(context.Cfg.Blocks[4], LiveIn("s1", "s2"), LiveOut("s1", "s2"));
        context.Validate(context.Cfg.Blocks[5], LiveIn("s1", "s2"), LiveOut("s1", "s2"));
        context.Validate(context.Cfg.Blocks[6], LiveIn("s1", "s2"));
        context.ValidateExit();
    }

    [TestMethod]
    public void FlowCaptrure_NullCoalescingOperator_ConsequentCalls_Assignment()
    {
        const string code = """var result = s1 ??= s2 = s3 ??= s4 ?? "End";""";
        var context = CreateContextCS(code, additionalParameters: "string s1, string s2, string s3, string s4");
        context.ValidateEntry(LiveIn("s1", "s3", "s4"), LiveOut("s1", "s3", "s4"));
        context.Validate(context.Cfg.Blocks[1], LiveIn("s1", "s3", "s4"), LiveOut("s1", "s3", "s4"));   // 1: #0=s1
        context.Validate(context.Cfg.Blocks[2], LiveIn("s1", "s3", "s4"), LiveOut("s1", "s3", "s4"));   // 2: #1=#0; if #1 is null
        context.Validate(context.Cfg.Blocks[3], LiveIn("s1"), LiveOut("s1"));                           // 3: F: #2=#1
        context.Validate(context.Cfg.Blocks[4], LiveIn("s3", "s4"), LiveOut("s3", "s4"));               // 4: T: #3=s2
        context.Validate(context.Cfg.Blocks[5], LiveIn("s3", "s4"), LiveOut("s3", "s4"));               // 5: #4=s3
        context.Validate(context.Cfg.Blocks[6], LiveIn("s3", "s4"), LiveOut("s3", "s4"));               // 6: #5=#4; if #5 is null
        context.Validate(context.Cfg.Blocks[7], LiveIn("s3"), LiveOut("s3"));                           // 7: F: #6=#5
        context.Validate(context.Cfg.Blocks[8], LiveIn("s4"), LiveOut("s4"));                           // 8: T: #7=s4; if #7 is null
        context.Validate(context.Cfg.Blocks[9], LiveIn("s4"), LiveOut("s4"));                           // 9: F: #8=#7
        context.Validate(context.Cfg.Blocks[10], LiveIn("s4"), LiveOut("s4"));                          // 10: #7=null; #8="End"
        context.Validate(context.Cfg.Blocks[11], LiveIn("s4"), LiveOut("s3"));                          // 11: #6= (#4=#8)
        context.Validate(context.Cfg.Blocks[12], LiveIn("s3"), LiveOut("s1"));                          // 12: #2= (#0 = (#3=#6) )
        context.Validate(context.Cfg.Blocks[13], LiveIn("s1"));                                         // 13: result=#2
        context.ValidateExit();
    }

    [TestMethod]
    public void FlowCaptrure_NullCoalescingOperator_Nested()
    {
        /*       Block 1
         *       #0=s1
         *     #0 is null
         *      /       \
         *     F         T
         *    /           \
         *   /             \
         * Block 2     Block 3
         * #1=#0     s2 is null
         *   |         /   \
         *   |        T     F
         *   |       /       \
         *   |   Block 4    Block 5
         *   |    #2=s3    #2="NestedFalse"
         *   |      |___________|
         *   |            |
         *   |            |
         *   |        Block 6
         *   |        #2 is null
         *   |          /   \
         *   |         F     T
         *   |        /       \
         *   |     Block7   Block 8
         *   |     #1=#2    #1="Hello"
         *   |       |___________|
         *   |             |
         *   +---------->Block 9
         *              result=#1
         *                 |
         *                Exit
         */
        const string code = """var result = s1 ?? (s2 is null ? s3 : "NestedFalse") ?? "Hello";""";
        var context = CreateContextCS(code, additionalParameters: "string s1, string s2, string s3");
        context.ValidateEntry(LiveIn("s1", "s2", "s3"), LiveOut("s1", "s2", "s3"));
        context.Validate(context.Cfg.Blocks[1], LiveIn("s1", "s2", "s3"), LiveOut("s1", "s2", "s3"));
        context.Validate(context.Cfg.Blocks[2], LiveIn("s1", "s3"), LiveOut("s1", "s3"));
        context.Validate(context.Cfg.Blocks[3], LiveIn("s1", "s2", "s3"), LiveOut("s1", "s3"));
        context.Validate(context.Cfg.Blocks[4], LiveIn("s1", "s3"), LiveOut("s1", "s3"));
        context.Validate(context.Cfg.Blocks[5], LiveIn("s1", "s3"), LiveOut("s1", "s3"));
        context.Validate(context.Cfg.Blocks[6], LiveIn("s1", "s3"), LiveOut("s1", "s3"));
        context.Validate(context.Cfg.Blocks[7], LiveIn("s1", "s3"), LiveOut("s1", "s3"));
        context.Validate(context.Cfg.Blocks[8], LiveIn("s1", "s3"), LiveOut("s1", "s3"));
        context.Validate(context.Cfg.Blocks[9], LiveIn("s1", "s3"));
        context.ValidateExit();
    }

    [TestMethod]
    public void FlowCaptrure_NullCoalescingOperator_Overwrite()
    {
        /*       Block 1
         *       #0=s1
         *         |
         *       Block 2
         *   #1=(s1="overwrite")
         *     #1 is null
         *        /    \
         *       F      T
         *      /        \
         *   Block3   Block4
         *   #2=#1   #2="value"
         *    |___________|
         *          |
         *       Block5
         *       s1=#2
         */
        const string code = """s1 = (s1 = "overwrite") ?? "value";""";
        var context = CreateContextCS(code, additionalParameters: "string s1");
        // s1 is never read. The assignment returns its r-value, which is used for further calculation.
        context.ValidateEntry();
        context.Validate(context.Cfg.Blocks[1]);
        context.Validate(context.Cfg.Blocks[2]);
        context.Validate(context.Cfg.Blocks[3]);
        context.Validate(context.Cfg.Blocks[4]);
        context.Validate(context.Cfg.Blocks[5]);
        context.ValidateExit();
    }

    [TestMethod]
    public void FlowCaptrure_SwitchStatement()
    {
        const string code = """
            var result = i switch
            {
                0 =>  param,
                1 => "Something",
                _ => "Everything"
            };
            """;
        var context = CreateContextCS(code, additionalParameters: "int i, string param");
        context.ValidateEntry(LiveIn("i", "param"), LiveOut("i", "param"));
        context.Validate(context.Cfg.Blocks[1], LiveIn("i", "param"), LiveOut("i", "param")); // #1 = i; if #1 == 0
        context.Validate(context.Cfg.Blocks[2], LiveIn("param"), LiveOut("param"));           // #0 = param
        context.Validate(context.Cfg.Blocks[3], LiveIn("i", "param"), LiveOut("i", "param")); // if #1 == 1
        context.Validate(context.Cfg.Blocks[4], LiveIn("param"), LiveOut("param"));           // #0 = "Something"
        context.Validate(context.Cfg.Blocks[5], LiveIn("i", "param"), LiveOut("param"));      // if discard
        context.Validate(context.Cfg.Blocks[6], LiveIn("param"), LiveOut("param"));           // #0 = "Everything"
        context.Validate(context.Cfg.Blocks[7]);                                              // else, unreachable, throws
        context.Validate(context.Cfg.Blocks[8], LiveIn("param"));                             // result = #0
        context.ValidateExit();
    }

    [TestMethod]
    public void FlowCaptrure_ForEachCompundAssignment()
    {
        const string code = """
            int sum = 0;
            foreach (var i in array)
            {
                sum += i;
            }
            """;
        var context = CreateContextCS(code, additionalParameters: "int[] array");
        context.ValidateEntry(LiveIn("array"), LiveOut("array"));
        context.Validate(context.Cfg.Blocks[1], LiveIn("array"), LiveOut("array", "sum")); // sum = 0
        context.Validate(context.Cfg.Blocks[2], LiveIn("array", "sum"), LiveOut("sum"));   // #0 = array
        context.Validate(context.Cfg.Blocks[3], LiveIn("sum"), LiveOut("sum"));            // If IEnumerator.MoveNext
        context.Validate(context.Cfg.Blocks[4], LiveIn("sum"), LiveOut("sum"));            // sum += i
        context.Validate(context.Cfg.Blocks[5]);                                           // Finally Region: #1=#0; if #1 is null, should have LiveIn/Liveout: array
        context.Validate(context.Cfg.Blocks[6]);                                           // Finally Region: #1.Dispose, should have LiveIn: array
        context.Validate(context.Cfg.Blocks[7]);                                           // Finally Region: Empty end of finally
        context.ValidateExit();
    }

    [TestMethod]
    public void FlowCaptrure_ImplicDictionaryCreation()
    {
        const string code = """Dictionary<string, int> dict =  new() { ["Key"] = 0, ["Lorem"] = 1, [key] = value }; """;
        var context = CreateContextCS(code, additionalParameters: "string key, int value");
        context.ValidateEntry(LiveIn("key", "value"), LiveOut("key", "value"));
        context.Validate(context.Cfg.Blocks[1], LiveIn("key", "value"), LiveOut("key", "value"));
        context.Validate(context.Cfg.Blocks[2], LiveIn("key", "value"), LiveOut("key", "value"));
        context.Validate(context.Cfg.Blocks[3], LiveIn("key", "value"), LiveOut("key", "value"));
        context.Validate(context.Cfg.Blocks[4], LiveIn("key", "value"));
        context.Validate(context.Cfg.Blocks[5]);
        context.ValidateExit();
    }
}

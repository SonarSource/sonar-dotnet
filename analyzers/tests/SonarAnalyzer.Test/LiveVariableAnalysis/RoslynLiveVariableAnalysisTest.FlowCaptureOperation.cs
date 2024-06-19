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

extern alias csharp;
namespace SonarAnalyzer.Test.LiveVariableAnalysis;

public partial class RoslynLiveVariableAnalysisTest
{
    [TestMethod]
    public void FlowCaptrure_NullCoalescingAssignment()
    {
        /*      Block 1
        *     #0=stringParam
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
        const string code = """stringParam ??= "Hello";""";
        var context = CreateContextCS(code, additionalParameters: "string stringParam");
        context.ValidateEntry(LiveIn("stringParam"), LiveOut("stringParam"));
        context.Validate(context.Cfg.Blocks[1], null, LiveIn("stringParam"), LiveOut("stringParam"));
        context.Validate(context.Cfg.Blocks[2], null, LiveIn("stringParam"), LiveOut("stringParam"));
        context.Validate(context.Cfg.Blocks[3], null, LiveIn("stringParam"));
        context.ValidateExit();
    }

    [TestMethod]
    public void FlowCaptrure_NullCoalescingOperator()
    {
        /*       Block 1
         *    #0=stringParam
         *     #0 is null
         *        /   \
         *       /     \
         *   Block 2  Block 3
         *   #1=#0  #1="Hello"
         *       \     /
         *       Block 4
         *         s=#1
         *          |
         *        Exit
         */
        const string code = """string s = stringParam ?? "Hello";""";
        var context = CreateContextCS(code, additionalParameters: "string stringParam");
        context.ValidateEntry(LiveIn("stringParam"), LiveOut("stringParam"));
        context.Validate(context.Cfg.Blocks[1], null, LiveIn("stringParam"), LiveOut("stringParam"));
        context.Validate(context.Cfg.Blocks[2], null, LiveIn("stringParam"), LiveOut("stringParam"));
        context.Validate(context.Cfg.Blocks[3], null, LiveIn("stringParam"), LiveOut("stringParam"));
        context.Validate(context.Cfg.Blocks[4], null, LiveIn("stringParam"));
        context.ValidateExit();
    }

    [TestMethod]
    public void FlowCaptrure_NullConditionalOperators()
    {
        /*       Block 1
         *    #1=stringParam
         *     #1 is null
         *        /   \
         *       F     T
         *       /     \
         *   Block 2  Block 3
         *  #0=Length #0=stringParam (DefaultValueOperation)
         *       \     /
         *       Block 4
         *      result=#1
         *          |
         *        Exit
         */
        const string code = """int? anInt =  stringParam?.Length;""";
        var context = CreateContextCS(code, additionalParameters: "string stringParam");
        context.ValidateEntry(LiveIn("stringParam"), LiveOut("stringParam"));
        context.Validate(context.Cfg.Blocks[1], null, LiveIn("stringParam"), LiveOut("stringParam"));
        context.Validate(context.Cfg.Blocks[2], null, LiveIn("stringParam"));
        context.Validate(context.Cfg.Blocks[3], null); // FIXME: LiveIn("stringParam") and LiveOut("StringParam") do not appear here due to DefaultValueOperation
        context.Validate(context.Cfg.Blocks[4], null);
        context.ValidateExit();
    }

    [TestMethod]
    public void FlowCaptrure_TernaryConditionalOperator()
    {
        /*       Block 1
         *     boolParameter
         *        /   \
         *       /     \
         *   Block 2  Block 3
         *   #0=test  #0="Hello"
         *       \     /
         *       Block 4
         *      result=#0
         *          |
         *        Exit
         */
        const string code = """string s = boolParameter ? stringParam : "Hello";""";
        var context = CreateContextCS(code, additionalParameters: "string stringParam");
        context.ValidateEntry(LiveIn("boolParameter", "stringParam"), LiveOut("boolParameter", "stringParam"));
        context.Validate(context.Cfg.Blocks[1], null, LiveIn("boolParameter", "stringParam"), LiveOut("stringParam"));
        context.Validate(context.Cfg.Blocks[2], null, LiveIn("stringParam"), LiveOut("stringParam"));
        context.Validate(context.Cfg.Blocks[3], null, LiveIn("stringParam"), LiveOut("stringParam"));
        context.Validate(context.Cfg.Blocks[4], null, LiveIn("stringParam"));
        context.ValidateExit();
    }

    [TestMethod]
    public void FlowCaptrure_TernaryOperatorReuseCaptures()
    {
        /*       Block 1
         *     boolParameter
         *        /   \
         *       T     F
         *      /       \
         *   Block 2  Block 3
         *   #0=st   #0="Hello"
         *       \     /
         *       Block 4
         *        s=#0
         *          |
         *        Block5
         *     boolParameter
         *        /   \
         *       T     F
         *      /       \
         *   Block 6  Block 7
         *   #1=st2   #1="Hello"
         *       \     /
         *       Block 8
         *        q=#1
         *         |
         *        Exit
         */
        const string code = """
            string s = boolParameter ? st : "Hello";
            string q = boolParameter ? st2 : "Hello";
            """;
        var context = CreateContextCS(code, additionalParameters: "string st, string st2");
        context.ValidateEntry(LiveIn("boolParameter", "st", "st2"), LiveOut("boolParameter", "st", "st2"));
        context.Validate(context.Cfg.Blocks[1], null, LiveIn("boolParameter", "st", "st2"), LiveOut("boolParameter", "st", "st2"));
        context.Validate(context.Cfg.Blocks[2], null, LiveIn("boolParameter", "st", "st2"), LiveOut("boolParameter", "st", "st2"));
        context.Validate(context.Cfg.Blocks[3], null, LiveIn("boolParameter", "st", "st2"), LiveOut("boolParameter", "st", "st2"));
        context.Validate(context.Cfg.Blocks[4], null, LiveIn("boolParameter", "st", "st2"), LiveOut("boolParameter", "st2"));
        context.Validate(context.Cfg.Blocks[5], null, LiveIn("boolParameter", "st2"), LiveOut("st2"));
        context.Validate(context.Cfg.Blocks[6], null, LiveIn("st2"), LiveOut("st2"));
        context.Validate(context.Cfg.Blocks[7], null, LiveIn("st2"), LiveOut("st2"));
        context.Validate(context.Cfg.Blocks[8], null, LiveIn("st2"));
        context.ValidateExit();
    }

    [TestMethod]
    public void FlowCaptrure_NullCoalescingOperator_ConsequentCalls()
    {
        /*       Block 1
         *       #0=s1
         *     #0 is null
         *        /   \
         *       F     T
         *       /     \
         *    Block 2  Block 3 -+---F--> Block 4 --------+
         *    #1=#0    #2=s2    |       #1=#2            |
         *       \  #2 is null  |                        |
         *        \     /       |                        |
         *         Block 6 <-+  +--T--> Block 5--------->|
         *          s=#1     |         #1="Hello"        |
         *           |       +---------------------------+
         *         Exit
         */
        const string code = """string s = s1 ?? s2 ?? "Hello";""";
        var context = CreateContextCS(code, additionalParameters: "string s1, string s2");
        context.ValidateEntry(LiveIn("s1", "s2"), LiveOut("s1", "s2"));
        context.Validate(context.Cfg.Blocks[1], null, LiveIn("s1", "s2"), LiveOut("s1", "s2"));
        context.Validate(context.Cfg.Blocks[2], null, LiveIn("s1", "s2"), LiveOut("s1", "s2"));
        context.Validate(context.Cfg.Blocks[3], null, LiveIn("s1", "s2"), LiveOut("s1", "s2"));
        context.Validate(context.Cfg.Blocks[4], null, LiveIn("s1", "s2"), LiveOut("s1", "s2"));
        context.Validate(context.Cfg.Blocks[5], null, LiveIn("s1", "s2"), LiveOut("s1", "s2"));
        context.Validate(context.Cfg.Blocks[6], null, LiveIn("s1", "s2"));
        context.ValidateExit();
    }

    [TestMethod]
    public void FlowCaptrure_NullCoalescingOperator_ConsequentCalls_Assignment()
    {
        const string code = """string s = s1 ??= s2 = s3 ??= s4 ?? "End";""";
        var context = CreateContextCS(code, additionalParameters: "string s1, string s2, string s3, string s4");
        context.ValidateEntry(LiveIn("s1", "s2", "s3", "s4"), LiveOut("s1", "s2", "s3", "s4"));
        context.Validate(context.Cfg.Blocks[1], null, LiveIn("s1", "s2", "s3", "s4"), LiveOut("s1", "s2", "s3", "s4"));  // 1: Parents: Entry, Children: 2
        context.Validate(context.Cfg.Blocks[2], null, LiveIn("s1", "s2", "s3", "s4"), LiveOut("s1", "s2", "s3", "s4"));  // 2: Parents: 1, Children: 2
        context.Validate(context.Cfg.Blocks[3], null, LiveIn("s1"), LiveOut("s1"));                                      // 3: Parents: 2, Children: 13
        context.Validate(context.Cfg.Blocks[4], null, LiveIn("s1", "s2", "s3", "s4"), LiveOut("s1", "s2", "s3", "s4"));  // 4: Parents: 2, Children: 5
        context.Validate(context.Cfg.Blocks[5], null, LiveIn("s1", "s2", "s3", "s4"), LiveOut("s1", "s2", "s3", "s4"));  // 5: Parents: 4, Children: 6
        context.Validate(context.Cfg.Blocks[6], null, LiveIn("s1", "s2", "s3", "s4"), LiveOut("s1", "s2", "s3", "s4"));  // 6: Parents: 5, Children: 7, 8
        context.Validate(context.Cfg.Blocks[7], null, LiveIn("s1", "s2", "s3"), LiveOut("s1", "s2", "s3"));              // 7: Parents: 6, Children: 12
        context.Validate(context.Cfg.Blocks[9], null, LiveIn("s1", "s2", "s3", "s4"), LiveOut("s1", "s2", "s3", "s4"));  // 8: Parents: 6, Children: 9, 10
        context.Validate(context.Cfg.Blocks[9], null, LiveIn("s1", "s2", "s3", "s4"), LiveOut("s1", "s2", "s3", "s4"));  // 9: Parents: 8, Children: 11
        context.Validate(context.Cfg.Blocks[10], null, LiveIn("s1", "s2", "s3", "s4"), LiveOut("s1", "s2", "s3", "s4")); // 10: Parents: 8, Children: 11
        context.Validate(context.Cfg.Blocks[11], null, LiveIn("s1", "s2", "s3", "s4"), LiveOut("s1", "s2", "s3"));       // 11: Parents: 9, 10, Children: 12
        context.Validate(context.Cfg.Blocks[12], null, LiveIn("s1", "s2", "s3"), LiveOut("s1"));                         // 12: Parents: 7, 11, Children:  13
        context.Validate(context.Cfg.Blocks[13], null, LiveIn("s1"));                                                    // 13: Parents: 3, 12, Children: Exit
        context.ValidateExit();
    }

    [TestMethod]
    public void FlowCaptrure_NullCoalescingOperator_Nested()
    {
        /*       Block 1
         *       #0=s1
         *     #0 is null
         *        /   \
         *      F/     \T
         *      /       \
         *   Block 2  Block 3---+------F----> Block 4 --------+
         *   #1=#0   s2 is null |             #2=s3           |
         *       \     /        |                             |
         *        Block 6       +------T----> Block 5-------->|
         *         s=#1                   #2="NestedFalse"    |
         *          |                                         |
         *          |                                Block6<--+
         *          |                              #2 is null
         *          |                               /   \
         *          |                             T/     \F
         *          |                             /       \
         *          |                           Block8   Block7
         *          |                         #1="Hello"  #1=#2"
         *          |                             |         |
         *          +------------>Block9<---------+---------+
         *                         s=#1
         *                          |
         *                         Exit
         */
        const string code = """string s = s1 ?? (s2 is null ? s3 : "NestedFalse") ?? "Hello";""";
        var context = CreateContextCS(code, additionalParameters: "string s1, string s2, string s3");
        context.ValidateEntry(LiveIn("s1", "s2", "s3"), LiveOut("s1", "s2", "s3"));
        context.Validate(context.Cfg.Blocks[1], null, LiveIn("s1", "s2", "s3"), LiveOut("s1", "s2", "s3"));
        context.Validate(context.Cfg.Blocks[2], null, LiveIn("s1", "s3"), LiveOut("s1", "s3"));
        context.Validate(context.Cfg.Blocks[3], null, LiveIn("s1", "s2", "s3"), LiveOut("s1", "s3"));
        context.Validate(context.Cfg.Blocks[4], null, LiveIn("s1", "s3"), LiveOut("s1", "s3"));
        context.Validate(context.Cfg.Blocks[5], null, LiveIn("s1", "s3"), LiveOut("s1", "s3"));
        context.Validate(context.Cfg.Blocks[6], null, LiveIn("s1", "s3"), LiveOut("s1", "s3"));
        context.Validate(context.Cfg.Blocks[7], null, LiveIn("s1", "s3"), LiveOut("s1", "s3"));
        context.Validate(context.Cfg.Blocks[8], null, LiveIn("s1", "s3"), LiveOut("s1", "s3"));
        context.Validate(context.Cfg.Blocks[9], null, LiveIn("s1", "s3"));
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
         *        /   \
         *       T     F
         *       /     \
         *    Block3   Block4
         *    #2=#1   #2="value"
         *      \         /
         *       \       /
         *        Block5
         *        s1=#2
         */
        const string code = """s1 = (s1 = "overwrite")  ?? "value";""";
        var context = CreateContextCS(code, additionalParameters: "string s1");
        context.ValidateEntry(LiveIn("s1"), LiveOut("s1"));
        context.Validate(context.Cfg.Blocks[1], null, LiveIn("s1"));
        context.Validate(context.Cfg.Blocks[2], null);
        context.Validate(context.Cfg.Blocks[3], null, LiveOut("s1"));
        context.Validate(context.Cfg.Blocks[4], null, LiveOut("s1"));
        context.Validate(context.Cfg.Blocks[5], null, LiveIn("s1"));
        context.ValidateExit();
    }

    [TestMethod]
    public void FlowCaptrure_SwitchStatement()
    {
        const string code = """
            string result = i switch
            {
                0 =>  stringParam,
                1 => "Something",
                _ => "Everything"
            };
            """;
        var context = CreateContextCS(code, additionalParameters: "int i, string stringParam");
        context.ValidateEntry(LiveIn("i", "stringParam"), LiveOut("i", "stringParam"));
        context.Validate(context.Cfg.Blocks[1], null, LiveIn("i", "stringParam"), LiveOut("i", "stringParam")); // if i == 0
        context.Validate(context.Cfg.Blocks[2], null, LiveIn("stringParam"), LiveOut("stringParam"));           // #0 = stringParam
        context.Validate(context.Cfg.Blocks[3], null, LiveIn("i", "stringParam"), LiveOut("i", "stringParam")); // if i == 1
        context.Validate(context.Cfg.Blocks[4], null, LiveIn("stringParam"), LiveOut("stringParam"));           // #0 = "Something"
        context.Validate(context.Cfg.Blocks[5], null, LiveIn("i", "stringParam"), LiveOut("stringParam"));      // if i != 0 && i != 1
        context.Validate(context.Cfg.Blocks[6], null, LiveIn("stringParam"), LiveOut("stringParam"));           // #0 = "Everything"
        context.Validate(context.Cfg.Blocks[7], null);                                                          // to no destination block
        context.Validate(context.Cfg.Blocks[8], null, LiveIn("stringParam"));                                   // result = #0
        context.ValidateExit();
    }

    [TestMethod]
    public void FlowCaptrure_ForEachCompundAssignment()
    {
        const string code = """
            int sum = 0;
            foreach (var i in list)
            {
                sum += i;
            }
            """;
        var context = CreateContextCS(code, additionalParameters: "System.Collections.Generic.List<int> list");
        context.ValidateEntry(LiveIn("list"), LiveOut("list"));
        context.Validate(context.Cfg.Blocks[1], null, LiveIn("list"), LiveOut("list", "sum"));
        context.Validate(context.Cfg.Blocks[2], null, LiveIn("list", "sum"), LiveOut("sum"));
        context.Validate(context.Cfg.Blocks[3], null, LiveIn("sum"), LiveOut("sum"));
        context.Validate(context.Cfg.Blocks[4], null, LiveIn("sum"), LiveOut("sum"));
        context.Validate(context.Cfg.Blocks[5], null);
        context.ValidateExit();
    }

    [TestMethod]
    public void FlowCaptrure_ImplicDictionaryCreation()
    {
        const string code = """System.Collections.Generic.Dictionary<string, int> dict =  new() { ["Key"] = 0, ["Lorem"] = 1, [key] = value }; """;
        var context = CreateContextCS(code, additionalParameters: "string key, int value");
        context.ValidateEntry(LiveIn("key", "value"), LiveOut("key", "value"));
        context.Validate(context.Cfg.Blocks[1], null, LiveIn("key", "value"), LiveOut("key", "value"));
        context.Validate(context.Cfg.Blocks[2], null, LiveIn("key", "value"), LiveOut("key", "value"));
        context.Validate(context.Cfg.Blocks[3], null, LiveIn("key", "value"), LiveOut("key", "value"));
        context.Validate(context.Cfg.Blocks[4], null, LiveIn("key", "value"));
        context.Validate(context.Cfg.Blocks[5], null);
        context.ValidateExit();
    }
}

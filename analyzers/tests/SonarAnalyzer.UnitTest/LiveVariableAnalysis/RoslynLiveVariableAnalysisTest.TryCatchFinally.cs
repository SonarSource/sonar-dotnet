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

using Microsoft.CodeAnalysis.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IIsNullOperation = Microsoft.CodeAnalysis.FlowAnalysis.IIsNullOperation;

namespace SonarAnalyzer.UnitTest.LiveVariableAnalysis
{
    public partial class RoslynLiveVariableAnalysisTest
    {
        [DataTestMethod]
        [DataRow("using (var ms = new System.IO.MemoryStream()) {", "}")]
        [DataRow("using var ms = new System.IO.MemoryStream();", null)]
        public void Using_LiveInUntilTheEnd(string usingStatement, string suffix)
        {
            /*       Block 1                    Finally region:
             *       ms = new                   Block 4
             *         |                        /    \
             *         |                    Block5    \
             *       Block 2                ms.Dispose |
             *       Method(ms.Length)          \     /
             *        /   \                     Block 6
             *       /     \                      |
             *   Block 3    |                   (null)
             *   Method(0)  |
             *       \     /
             *        Exit
             */
            var code = @$"
{usingStatement}
    Method(ms.Length);
    if (boolParameter)
        Method(0);
{suffix}";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock, new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate(context.Block<ISimpleAssignmentOperation>("ms = new System.IO.MemoryStream()"), new LiveIn("boolParameter"), new LiveOut("boolParameter", "ms"));
            context.Validate(context.Block("Method(ms.Length);"), new LiveIn("boolParameter", "ms"), new LiveOut("ms"));
            context.Validate(context.Block("Method(0);"), new LiveIn("ms"), new LiveOut("ms"));
            context.Validate(context.Cfg.ExitBlock);
            // Finally region
            context.Validate(context.Block<IIsNullOperation>("ms = new System.IO.MemoryStream()"), new LiveIn("ms"), new LiveOut("ms"));    // Null check
            context.Validate(context.Block<IInvocationOperation>("ms = new System.IO.MemoryStream()"), new LiveIn("ms"));                   // Actual Dispose
            context.Validate(context.Cfg.Blocks[6]);
        }

        [TestMethod]
        public void Using_Nested_Block_LiveInUntilTheEnd()
        {
            var code = @$"
using (var msOuter = new System.IO.MemoryStream())
{{
    if (boolParameter)
    {{
        using (var msInner = new System.IO.MemoryStream())
        {{
            Method(0);
        }}
        Method(1);
    }}
}}
Method(2);";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock, new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate(context.Block("Method(0);"), new LiveIn("msOuter", "msInner"), new LiveOut("msOuter", "msInner"));
            context.Validate(context.Block("Method(1);"), new LiveIn("msOuter"), new LiveOut("msOuter"));
            context.Validate(context.Block("Method(2);"));
            context.Validate(context.Cfg.ExitBlock);
            // Finally region
            context.Validate(context.Block<IIsNullOperation>("msInner = new System.IO.MemoryStream()"), new LiveIn("msInner", "msOuter"), new LiveOut("msInner", "msOuter"));   // Null check
            context.Validate(context.Block<IInvocationOperation>("msInner = new System.IO.MemoryStream()"), new LiveIn("msInner", "msOuter"), new LiveOut("msOuter"));          // Actual Dispose
            context.Validate(context.Block<IIsNullOperation>("msOuter = new System.IO.MemoryStream()"), new LiveIn("msOuter"), new LiveOut("msOuter"));     // Null check
            context.Validate(context.Block<IInvocationOperation>("msOuter = new System.IO.MemoryStream()"), new LiveIn("msOuter"));                         // Actual Dispose
        }

        [TestMethod]
        public void Using_Nested_Declaration_LiveInUntilTheEnd()
        {
            var code = @$"
using var msOuter = new System.IO.MemoryStream();
if (boolParameter)
{{
    using var msInner = new System.IO.MemoryStream();
    Method(0);
    if (boolParameter)
    {{
        Method(1);
    }}
}}
Method(2);";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock, new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate(context.Block("Method(0);"), new LiveIn("boolParameter", "msOuter", "msInner"), new LiveOut("msOuter", "msInner"));
            context.Validate(context.Block("Method(1);"), new LiveIn("msOuter", "msInner"), new LiveOut("msOuter", "msInner"));
            context.Validate(context.Block("Method(2);"), new LiveIn("msOuter"), new LiveOut("msOuter"));
            context.Validate(context.Cfg.ExitBlock);
            // Finally region
            context.Validate(context.Block<IIsNullOperation>("msInner = new System.IO.MemoryStream()"), new LiveIn("msInner", "msOuter"), new LiveOut("msInner", "msOuter"));   // Null check
            context.Validate(context.Block<IInvocationOperation>("msInner = new System.IO.MemoryStream()"), new LiveIn("msInner", "msOuter"), new LiveOut("msOuter"));          // Actual Dispose
            context.Validate(context.Block<IIsNullOperation>("msOuter = new System.IO.MemoryStream()"), new LiveIn("msOuter"), new LiveOut("msOuter"));     // Null check
            context.Validate(context.Block<IInvocationOperation>("msOuter = new System.IO.MemoryStream()"), new LiveIn("msOuter"));                         // Actual Dispose
        }

        [TestMethod]
        public void Finally_LiveIn()
        {
            var code = @"
try
{
    Method(0);
}
finally
{
    Method(intParameter);
}
Method(1);";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock, new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate(context.Block("Method(0);"), new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate(context.Block("Method(intParameter);"), new LiveIn("intParameter"));
            context.Validate(context.Block("Method(1);"));
            context.Validate(context.Cfg.ExitBlock);
        }

        [TestMethod]
        public void Finally_VariableUsedAfter_LiveIn_LiveOut()
        {
            var code = @"
try
{
    Method(0);
}
finally
{
    Method(1);
}
Method(intParameter);";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock, new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate(context.Block("Method(0);"), new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate(context.Block("Method(1);"), new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate(context.Block("Method(intParameter);"), new LiveIn("intParameter"));
            context.Validate(context.Cfg.ExitBlock);
        }

        [TestMethod]
        public void Finally_WithThrowStatement_LiveIn()
        {
            var code = @"
try
{
    Method(0);
}
finally
{
    Method(1);
    throw new System.Exception();
    Method(2);  // Unreachable
}
Method(intParameter); // Unreachable";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock, new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate(context.Block("Method(0);"), new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate(context.Block("Method(1);"), new LiveIn("intParameter"), new LiveOut("intParameter"));
            // LVA doesn't care if it's reachable. Blocks still should have LiveIn and LiveOut
            context.Validate(context.Block("Method(2);"), new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate(context.Block("Method(intParameter);"), new LiveIn("intParameter"));
            context.Validate(context.Cfg.ExitBlock);
        }

        [TestMethod]
        public void Finally_WithThrowStatement_Conditional_LiveIn()
        {
            var code = @"
try
{
    Method(0);
}
finally
{
    Method(1);
    if (boolParameter)
    {
        throw new System.Exception();
    }
    Method(2);
}
Method(intParameter);";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock, new LiveIn("intParameter", "boolParameter"), new LiveOut("intParameter", "boolParameter"));
            context.Validate(context.Block("Method(0);"), new LiveIn("intParameter", "boolParameter"), new LiveOut("intParameter", "boolParameter"));
            context.Validate(context.Block("Method(1);"), new LiveIn("intParameter", "boolParameter"), new LiveOut("intParameter", "boolParameter"));
            context.Validate(context.Block("boolParameter"), new LiveIn("intParameter", "boolParameter"), new LiveOut("intParameter"));
            context.Validate(context.Block("Method(2);"), new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate(context.Block("Method(intParameter);"), new LiveIn("intParameter"));
            context.Validate(context.Cfg.ExitBlock);
        }

        [TestMethod]
        public void Finally_NotLiveIn_NotLiveOut()
        {
            var code = @"
try
{
    Method(0);
}
finally
{
    intParameter = 42;
    Method(intParameter);
}
Method(1);";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock);
            context.Validate(context.Block("Method(0);"));
            context.Validate(context.Block("Method(intParameter);"));
            context.Validate(context.Block("Method(1);"));
            context.Validate(context.Cfg.ExitBlock);
        }

        [TestMethod]
        public void Finally_Nested_LiveIn()
        {
            var code = @"
var outer = 42;
var inner = 42;
try
{
    try
    {
        Method(0);
    }
    finally
    {
        Method(inner);
    }
    Method(1);
}
finally
{
    Method(outer);
}
Method(2);";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock);
            context.Validate(context.Block("Method(0);"), new LiveIn("inner", "outer"), new LiveOut("inner", "outer"));
            context.Validate(context.Block("Method(inner);"), new LiveIn("inner", "outer"), new LiveOut("outer"));
            context.Validate(context.Block("Method(1);"), new LiveIn("outer"), new LiveOut("outer"));
            context.Validate(context.Block("Method(outer);"), new LiveIn("outer"));
            context.Validate(context.Block("Method(2);"));
            context.Validate(context.Cfg.ExitBlock);
        }

        [TestMethod]
        public void Finally_InvalidSyntax_LiveIn()
        {
            /*    Entry 0
             *      |
             * +----|- TryAndFinallyRegion   ------------+
             * |+---|- TryRegion -+ +-- FinallyRegion --+|
             * ||  Block 1        | |  Block 3          ||
             * ||  (empty)        | |  Method(0)        ||
             * ||   |             | |   |               ||
             * |+---|-------------+ |  (null)           ||
             * |    |               +-------------------+|
             * +----|------------------------------------+
             *      |
             *    Block 3
             *    Method(intParameter)
             *      |
             *    Exit 4
             */
            var code = @"
// Error CS1003 Syntax error, 'try' expected
// Error CS1514 { expected
// Error CS1513 } expected
finally
{
    Method(0);
}
Method(intParameter);";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock, new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate(context.Block("Method(0);"), new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate(context.Block("Method(intParameter);"), new LiveIn("intParameter"));
            context.Validate(context.Cfg.ExitBlock);
        }

        [TestMethod]
        public void Finally_Loop_Propagates_LiveIn_LiveOut()
        {
            var code = @"
A:
Method(intParameter);
if (boolParameter)
    goto B;
try
{
    Method(0);
}
finally
{
    Method(1);
}
goto A;
B:
Method(2);";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock, new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter"));
            context.Validate(context.Block("Method(intParameter);"), new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter"));
            context.Validate(context.Block("Method(0);"), new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter"));
            context.Validate(context.Block("Method(1);"), new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter"));
            context.Validate(context.Block("Method(2);"));
        }
    }
}

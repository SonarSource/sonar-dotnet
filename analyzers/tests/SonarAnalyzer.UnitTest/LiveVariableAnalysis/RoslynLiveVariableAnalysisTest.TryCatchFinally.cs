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
        [DataRow("using (var ms = new MemoryStream()) {", "}")]
        [DataRow("using var ms = new MemoryStream();", null)]
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
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate<ISimpleAssignmentOperation>("ms = new MemoryStream()", new LiveIn("boolParameter"), new LiveOut("boolParameter", "ms"));
            context.Validate("Method(ms.Length);", new LiveIn("boolParameter", "ms"), new LiveOut("ms"));
            context.Validate("Method(0);", new LiveIn("ms"), new LiveOut("ms"));
            context.ValidateExit();
            // Finally region
            context.Validate<IIsNullOperation>("ms = new MemoryStream()", new LiveIn("ms"), new LiveOut("ms"));    // Null check
            context.Validate<IInvocationOperation>("ms = new MemoryStream()", new LiveIn("ms"));                   // Actual Dispose
            context.Validate(context.Cfg.Blocks[6], null, new Expected[] { });
        }

        [TestMethod]
        public void Using_Nested_Block_LiveInUntilTheEnd()
        {
            var code = @$"
using (var msOuter = new MemoryStream())
{{
    if (boolParameter)
    {{
        using (var msInner = new MemoryStream())
        {{
            Method(0);
        }}
        Method(1);
    }}
}}
Method(2);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("Method(0);", new LiveIn("msOuter", "msInner"), new LiveOut("msOuter", "msInner"));
            context.Validate("Method(1);", new LiveIn("msOuter"), new LiveOut("msOuter"));
            context.Validate("Method(2);");
            context.ValidateExit();
            // Finally region
            context.Validate<IIsNullOperation>("msInner = new MemoryStream()", new LiveIn("msInner", "msOuter"), new LiveOut("msInner", "msOuter"));   // Null check
            context.Validate<IInvocationOperation>("msInner = new MemoryStream()", new LiveIn("msInner", "msOuter"), new LiveOut("msOuter"));          // Actual Dispose
            context.Validate<IIsNullOperation>("msOuter = new MemoryStream()", new LiveIn("msOuter"), new LiveOut("msOuter"));     // Null check
            context.Validate<IInvocationOperation>("msOuter = new MemoryStream()", new LiveIn("msOuter"));                         // Actual Dispose
        }

        [TestMethod]
        public void Using_Nested_Declaration_LiveInUntilTheEnd()
        {
            var code = @$"
using var msOuter = new MemoryStream();
if (boolParameter)
{{
    using var msInner = new MemoryStream();
    Method(0);
    if (boolParameter)
    {{
        Method(1);
    }}
}}
Method(2);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("Method(0);", new LiveIn("boolParameter", "msOuter", "msInner"), new LiveOut("msOuter", "msInner"));
            context.Validate("Method(1);", new LiveIn("msOuter", "msInner"), new LiveOut("msOuter", "msInner"));
            context.Validate("Method(2);", new LiveIn("msOuter"), new LiveOut("msOuter"));
            context.ValidateExit();
            // Finally region
            context.Validate<IIsNullOperation>("msInner = new MemoryStream()", new LiveIn("msInner", "msOuter"), new LiveOut("msInner", "msOuter"));   // Null check
            context.Validate<IInvocationOperation>("msInner = new MemoryStream()", new LiveIn("msInner", "msOuter"), new LiveOut("msOuter"));          // Actual Dispose
            context.Validate<IIsNullOperation>("msOuter = new MemoryStream()", new LiveIn("msOuter"), new LiveOut("msOuter"));     // Null check
            context.Validate<IInvocationOperation>("msOuter = new MemoryStream()", new LiveIn("msOuter"));                         // Actual Dispose
        }

        [TestMethod]
        public void Catch_LiveIn()
        {
            var code = @"
try
{
    Method(0);
}
catch
{
    Method(intParameter);
}
Method(1);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(0);", new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(intParameter);", new LiveIn("intParameter"));
            context.Validate("Method(1);");
            context.ValidateExit();
        }

        [TestMethod]
        public void Catch_VariableUsedAfter_LiveIn_LiveOut()
        {
            var code = @"
try
{
    Method(0);
}
catch
{
    Method(1);
}
Method(intParameter);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(0);", new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(1);", new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(intParameter);", new LiveIn("intParameter"));
            context.ValidateExit();
        }

        [TestMethod]
        public void Catch_WithThrowStatement_LiveIn()
        {
            var code = @"
var usedInTry = 42;
var usedInTryUnreachable = 42;
var usedInCatch = 42;
var usedInCatchUnreachable = 42;
try
{
    Method(usedInTry);
    throw new Exception();
    Method(usedInTryUnreachable);  // Unreachable
}
catch
{
    Method(usedInCatch);
    throw new Exception();
    Method(usedInCatchUnreachable);  // Unreachable
}
Method(intParameter); // Unreachable";
            var context = CreateContextCS(code);
            // LVA doesn't care if it's reachable. Blocks still should have LiveIn and LiveOut
            context.ValidateEntry();    // intParameter is used only in unreachable path => not visible here
            context.Validate("Method(usedInTry);", new LiveIn("usedInTry", "usedInCatch"), new LiveOut("usedInCatch"));     // Doesn't see usedInTryUnreachable nor intParameter
            context.Validate("Method(usedInTryUnreachable);", new LiveIn("intParameter", "usedInTryUnreachable", "usedInCatch"), new LiveOut("intParameter", "usedInCatch"));
            context.Validate("Method(usedInCatch);", new LiveIn("usedInCatch"));                                            // Doesn't see usedInCatchUnreachable nor intParameter
            context.Validate("Method(usedInCatchUnreachable);", new LiveIn("intParameter", "usedInCatchUnreachable"), new LiveOut("intParameter"));
            context.Validate("Method(intParameter);", new LiveIn("intParameter"));
            context.ValidateExit();
        }

        [TestMethod]
        public void Catch_WithThrowStatement_Conditional_LiveIn()
        {
            var code = @"
try
{
    Method(0);
}
catch
{
    Method(1);
    if (boolParameter)
    {
        throw new Exception();
    }
    Method(2);
}
Method(intParameter);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("intParameter", "boolParameter"), new LiveOut("intParameter", "boolParameter"));
            context.Validate("Method(0);", new LiveIn("intParameter", "boolParameter"), new LiveOut("intParameter", "boolParameter"));
            context.Validate("Method(1);", new LiveIn("intParameter", "boolParameter"), new LiveOut("intParameter"));
            context.Validate("boolParameter", new LiveIn("intParameter", "boolParameter"), new LiveOut("intParameter"));
            context.Validate("Method(2);", new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(intParameter);", new LiveIn("intParameter"));
            context.ValidateExit();
        }

        [TestMethod]
        public void Catch_Rethrow_LiveIn()
        {
            var code = @"
try
{
    Method(0);
}
catch
{
    Method(1);
    throw;
}
Method(intParameter);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(0);", new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(1);");
            context.Validate("Method(intParameter);", new LiveIn("intParameter"));
            context.ValidateExit();
        }

        [TestMethod]
        public void Catch_NotLiveIn_NotLiveOut()
        {
            var code = @"
try
{
    Method(0);
}
catch
{
    intParameter = 42;
    Method(intParameter);
}
Method(1);";
            var context = CreateContextCS(code);
            context.ValidateEntry();
            context.Validate("Method(0);");
            context.Validate("Method(intParameter);");
            context.Validate("Method(1);");
            context.ValidateExit();
        }

        [TestMethod]
        public void Catch_Nested_LiveIn()
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
    catch
    {
        Method(inner);
    }
    Method(1);
}
catch
{
    Method(outer);
}
Method(2);";
            var context = CreateContextCS(code);
            context.ValidateEntry();
            context.Validate("Method(0);", new LiveIn("inner", "outer"), new LiveOut("inner", "outer"));
            context.Validate("Method(inner);", new LiveIn("inner", "outer"), new LiveOut("outer"));
            context.Validate("Method(1);", new LiveIn("outer"), new LiveOut("outer"));
            context.Validate("Method(outer);", new LiveIn("outer"));
            context.Validate("Method(2);");
            context.ValidateExit();
        }

        [TestMethod]
        public void Catch_InvalidSyntax_LiveIn()
        {
            /*    Entry 0
             *      |
             * +----|- TryAndCatchRegion   --------------+
             * |+---|- TryRegion -+ +-- CatchRegion ----+|
             * ||  Block 1        | |  Block 3          ||
             * ||  (empty)        | |  Method(0)        ||
             * ||   |             | |   |               ||
             * |+---|-------------+ +---|---------------+|
             * +----|-------------------|----------------+
             *      |                  /
             *      |    /------------+
             *    Block 3
             *    Method(intParameter)
             *      |
             *    Exit 4
             */
            var code = @"
// Error CS1003 Syntax error, 'try' expected
// Error CS1514 { expected
// Error CS1513 } expected
catch
{
    Method(0);
}
Method(intParameter);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(0);", new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(intParameter);", new LiveIn("intParameter"));
            context.ValidateExit();
        }

        [TestMethod]
        public void Catch_Loop_Propagates_LiveIn_LiveOut()
        {
            var code = @"
var variableUsedInCatch = 42;
A:
Method(intParameter);
if (boolParameter)
    goto B;
try
{
    Method(0);
}
catch
{
    Method(variableUsedInCatch);
}
goto A;
B:
Method(1);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter"));
            context.Validate("Method(intParameter);", new LiveIn("boolParameter", "intParameter", "variableUsedInCatch"), new LiveOut("boolParameter", "intParameter", "variableUsedInCatch"));
            context.Validate("Method(0);", new LiveIn("boolParameter", "intParameter", "variableUsedInCatch"), new LiveOut("boolParameter", "intParameter", "variableUsedInCatch"));
            context.Validate("Method(variableUsedInCatch);", new LiveIn("boolParameter", "intParameter", "variableUsedInCatch"), new LiveOut("boolParameter", "intParameter", "variableUsedInCatch"));
            context.Validate("Method(1);");
        }

        [TestMethod]
        public void Catch_ExVariable_LiveIn()
        {
            var code = @"
try
{
    Method(0);
}
catch (Exception ex)
{
    if (boolParameter)
    {
        Method(ex.HResult);
    }
}
Method(1);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("Method(0);", new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("boolParameter", new LiveIn("boolParameter"), new LiveOut("ex"));     // ex doesn't live in here, becase this blocks starts with SimpleAssignmentOperation: (Exception ex)
            context.Validate("Method(ex.HResult);", new LiveIn("ex"));
            context.Validate("Method(1);");
            context.ValidateExit();
        }

        [TestMethod]
        public void Catch_SingleType_LiveIn()
        {
            var code = @"
var usedAfter = 42;
var usedInTry = 42;
var usedInCatch = 42;
try
{
    Method(usedInTry);
}
catch (Exception ex)
{
    Method(intParameter, usedInCatch, ex.HResult);
}
Method(usedAfter);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(usedInTry);", new LiveIn("usedInTry", "usedAfter", "usedInCatch", "intParameter"), new LiveOut("usedAfter", "usedInCatch", "intParameter"));
            context.Validate("Method(intParameter, usedInCatch, ex.HResult);", new LiveIn("intParameter", "usedInCatch", "usedAfter"), new LiveOut("usedAfter"));
            context.Validate("Method(usedAfter);", new LiveIn("usedAfter"));
            context.ValidateExit();
        }

        [TestMethod]
        public void Catch_SingleTypeWhenCondition_LiveIn()
        {
            var code = @"
var usedAfter = 42;
var usedInTry = 42;
var usedInCatch = 42;
try
{
    Method(usedInTry);
}
catch (Exception ex) when (ex.InnerException == null)
{
    Method(intParameter, usedInCatch, ex.HResult);
}
Method(usedAfter);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(usedInTry);", new LiveIn("usedInTry", "usedAfter", "usedInCatch", "intParameter"), new LiveOut("usedAfter", "usedInCatch", "intParameter"));
            context.Validate("Method(intParameter, usedInCatch, ex.HResult);", new LiveIn("intParameter", "usedInCatch", "usedAfter", "ex"), new LiveOut("usedAfter"));
            context.Validate("Method(usedAfter);", new LiveIn("usedAfter"));
            context.ValidateExit();
        }

        [TestMethod]
        public void Catch_SingleTypeWhenConditionReferencingArgument_LiveIn()
        {
            var code = @"
try
{
    Method(0);
}
catch (Exception ex) when (boolParameter)
{
    Method(intParameter);
}
Method(1);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter"));
            context.Validate("Method(0);", new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter"));
            context.Validate("Method(intParameter);", new LiveIn("intParameter"));
            context.Validate("Method(1);");
            context.ValidateExit();
        }

        [TestMethod]
        public void Catch_MultipleTypes_LiveIn()
        {
            var code = @"
var usedAfter = 42;
var usedInTry = 42;
var usedInCatchA = 42;
var usedInCatchB = 42;
try
{
    Method(usedInTry);
}
catch (FormatException ex)
{
    Method(intParameter, usedInCatchA, ex.HResult);
}
catch (Exception ex)
{
    Method(intParameter, usedInCatchB, ex.HResult);
}
Method(usedAfter);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(usedInTry);",
                new LiveIn("usedInTry", "usedAfter", "usedInCatchA", "usedInCatchB", "intParameter"),
                new LiveOut("usedAfter", "usedInCatchA", "usedInCatchB", "intParameter"));
            // ex doesn't live in here, because the blocks starts with SimpleAssignmentOperation: (Exception ex)
            context.Validate("Method(intParameter, usedInCatchA, ex.HResult);", new LiveIn("intParameter", "usedInCatchA", "usedAfter"), new LiveOut("usedAfter"));
            context.Validate("Method(intParameter, usedInCatchB, ex.HResult);", new LiveIn("intParameter", "usedInCatchB", "usedAfter"), new LiveOut("usedAfter"));
            context.Validate("Method(usedAfter);", new LiveIn("usedAfter"));
            context.ValidateExit();
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
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(0);", new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(intParameter);", new LiveIn("intParameter"));
            context.Validate("Method(1);");
            context.ValidateExit();
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
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(0);", new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(1);", new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(intParameter);", new LiveIn("intParameter"));
            context.ValidateExit();
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
    throw new Exception();
    Method(2);  // Unreachable
}
Method(intParameter); // Unreachable";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(0);", new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(1);", new LiveIn("intParameter"), new LiveOut("intParameter"));
            // LVA doesn't care if it's reachable. Blocks still should have LiveIn and LiveOut
            context.Validate("Method(2);", new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(intParameter);", new LiveIn("intParameter"));
            context.ValidateExit();
        }

        [TestMethod]
        public void Finally_WithThrowStatementAsSingleExit_LiveIn()
        {
            var code = @"
try
{
    Method(0);
}
finally
{
    Method(1);
    throw new Exception();
}
Method(intParameter); // Unreachable";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(0);", new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(1);", new LiveIn("intParameter"), new LiveOut("intParameter"));
            // LVA doesn't care if it's reachable. Blocks still should have LiveIn and LiveOut
            context.Validate("Method(intParameter);", new LiveIn("intParameter"));
            context.ValidateExit();
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
        throw new Exception();
    }
    Method(2);
}
Method(intParameter);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("intParameter", "boolParameter"), new LiveOut("intParameter", "boolParameter"));
            context.Validate("Method(0);", new LiveIn("intParameter", "boolParameter"), new LiveOut("intParameter", "boolParameter"));
            context.Validate("Method(1);", new LiveIn("intParameter", "boolParameter"), new LiveOut("intParameter"));
            context.Validate("boolParameter", new LiveIn("intParameter", "boolParameter"), new LiveOut("intParameter"));
            context.Validate("Method(2);", new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(intParameter);", new LiveIn("intParameter"));
            context.ValidateExit();
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
            var context = CreateContextCS(code);
            context.ValidateEntry();
            context.Validate("Method(0);");
            context.Validate("Method(intParameter);");
            context.Validate("Method(1);");
            context.ValidateExit();
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
            var context = CreateContextCS(code);
            context.ValidateEntry();
            context.Validate("Method(0);", new LiveIn("inner", "outer"), new LiveOut("inner", "outer"));
            context.Validate("Method(inner);", new LiveIn("inner", "outer"), new LiveOut("outer"));
            context.Validate("Method(1);", new LiveIn("outer"), new LiveOut("outer"));
            context.Validate("Method(outer);", new LiveIn("outer"));
            context.Validate("Method(2);");
            context.ValidateExit();
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
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(0);", new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(intParameter);", new LiveIn("intParameter"));
            context.ValidateExit();
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
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter"));
            context.Validate("Method(intParameter);", new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter"));
            context.Validate("Method(0);", new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter"));
            context.Validate("Method(1);", new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter"));
            context.Validate("Method(2);");
        }

        [TestMethod]
        public void TryCatchFinally_LiveIn()
        {
            var code = @"
var usedInTry = 42;
var usedInCatch = 42;
var usedInFinally = 42;
var usedAfter = 42;
try
{
    Method(usedInTry);
}
catch
{
    Method(usedInCatch);
}
finally
{
    Method(usedInFinally);
}
Method(usedAfter);";
            var context = CreateContextCS(code);
            context.ValidateEntry();
            context.Validate("Method(usedInTry);", new LiveIn("usedInTry", "usedInCatch", "usedInFinally", "usedAfter"), new LiveOut("usedInCatch", "usedInFinally", "usedAfter"));
            context.Validate("Method(usedInCatch);", new LiveIn("usedInCatch", "usedInFinally", "usedAfter"), new LiveOut("usedInFinally", "usedAfter"));
            context.Validate("Method(usedInFinally);", new LiveIn("usedInFinally", "usedAfter"), new LiveOut("usedAfter"));
            context.Validate("Method(usedAfter);", new LiveIn("usedAfter"));
            context.ValidateExit();
        }
    }
}

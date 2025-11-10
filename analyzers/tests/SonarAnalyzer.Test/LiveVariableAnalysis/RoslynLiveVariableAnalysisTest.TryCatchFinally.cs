/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using Microsoft.CodeAnalysis.Operations;
using IIsNullOperation = Microsoft.CodeAnalysis.FlowAnalysis.IIsNullOperation;

namespace SonarAnalyzer.Test.LiveVariableAnalysis;

public partial class RoslynLiveVariableAnalysisTest
{
    [TestMethod]
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
        var code = $"""
            {usingStatement}
                Method(ms.Capacity);
                if (boolParameter)
                    Method(0);
            {suffix}
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("boolParameter"), LiveOut("boolParameter"));
        context.Validate<ISimpleAssignmentOperation>("ms = new MemoryStream()", LiveIn("boolParameter"), LiveOut("boolParameter", "ms"));
        context.Validate("Method(ms.Capacity);", LiveIn("boolParameter", "ms"), LiveOut("ms"));
        context.Validate("Method(0);", LiveIn("ms"), LiveOut("ms"));
        context.ValidateExit();
        // Finally region
        context.Validate<IIsNullOperation>("ms = new MemoryStream()", LiveIn("ms"), LiveOut("ms"));     // Null check
        context.Validate<IInvocationOperation>("ms = new MemoryStream()", LiveIn("ms"));                // Actual Dispose
        context.Validate(context.Cfg.Blocks[6]);
    }

    [TestMethod]
    public void Using_Nested_Block_LiveInUntilTheEnd()
    {
        const string code = """
            using (var msOuter = new MemoryStream())
            {
                if (boolParameter)
                {
                    using (var msInner = new MemoryStream())
                    {
                        Method(0);
                    }
                    Method(1);
                }
            }
            Method(2);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("boolParameter"), LiveOut("boolParameter"));
        context.Validate("Method(0);", LiveIn("msOuter", "msInner"), LiveOut("msOuter", "msInner"));
        context.Validate("Method(1);", LiveIn("msOuter"), LiveOut("msOuter"));
        context.Validate("Method(2);");
        context.ValidateExit();
        // Finally region
        context.Validate<IIsNullOperation>("msInner = new MemoryStream()", LiveIn("msInner", "msOuter"), LiveOut("msInner", "msOuter"));   // Null check
        context.Validate<IInvocationOperation>("msInner = new MemoryStream()", LiveIn("msInner", "msOuter"), LiveOut("msOuter"));          // Actual Dispose
        context.Validate<IIsNullOperation>("msOuter = new MemoryStream()", LiveIn("msOuter"), LiveOut("msOuter"));                         // Null check
        context.Validate<IInvocationOperation>("msOuter = new MemoryStream()", LiveIn("msOuter"));                                         // Actual Dispose
    }

    [TestMethod]
    public void Using_Nested_Declaration_LiveInUntilTheEnd()
    {
        const string code = """
            using var msOuter = new MemoryStream();
            if (boolParameter)
            {
                using var msInner = new MemoryStream();
                Method(0);
                if (boolParameter)
                {
                    Method(1);
                }
            }
            Method(2);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("boolParameter"), LiveOut("boolParameter"));
        context.Validate("Method(0);", LiveIn("boolParameter", "msOuter", "msInner"), LiveOut("msOuter", "msInner"));
        context.Validate("Method(1);", LiveIn("msOuter", "msInner"), LiveOut("msOuter", "msInner"));
        context.Validate("Method(2);", LiveIn("msOuter"), LiveOut("msOuter"));
        context.ValidateExit();
        // Finally region
        context.Validate<IIsNullOperation>("msInner = new MemoryStream()", LiveIn("msInner", "msOuter"), LiveOut("msInner", "msOuter"));   // Null check
        context.Validate<IInvocationOperation>("msInner = new MemoryStream()", LiveIn("msInner", "msOuter"), LiveOut("msOuter"));          // Actual Dispose
        context.Validate<IIsNullOperation>("msOuter = new MemoryStream()", LiveIn("msOuter"), LiveOut("msOuter"));                         // Null check
        context.Validate<IInvocationOperation>("msOuter = new MemoryStream()", LiveIn("msOuter"));                                         // Actual Dispose
    }

    [TestMethod]
    public void Catch_LiveIn()
    {
        const string code = """
            try
            {
                Method(0);
            }
            catch
            {
                Method(intParameter);
            }
            Method(1);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(0);", LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(intParameter);", LiveIn("intParameter"));
        context.Validate("Method(1);");
        context.ValidateExit();
    }

    [TestMethod]
    public void VariableBeforeTry_LiveOut()
    {
        const string code = """
            var usedInCatch = 0;
            Method(0);
            try
            {
                usedInCatch = 1;
                Method(1);
            }
            catch
            {
                Method(usedInCatch);
            }
            """;
        var context = CreateContextCS(code);
        context.Validate("Method(0);", LiveOut("usedInCatch"));
        context.Validate("Method(1);", LiveOut("usedInCatch"));
        context.Validate("Method(usedInCatch);", LiveIn("usedInCatch"));
        context.ValidateExit();
    }

    [TestMethod]
    public void NestedCatch_LiveOutOuterCatch()
    {
        const string code = """
            var usedInCatch = 0;
            Method(0);
            try
            {
                Method(1);
                try
                {
                    usedInCatch = 1;
                    Method(2);
                }
                catch
                {
                    Method(usedInCatch); // This can throw again
                    Method(3);
                }
            }
            catch
            {
                Method(usedInCatch);
                Method(4);
            }
            """;
        var context = CreateContextCS(code);
        context.Validate("Method(0);", LiveOut("usedInCatch"));
        context.Validate("Method(1);", LiveIn("usedInCatch"), LiveOut("usedInCatch"));
        context.Validate("Method(2);", LiveOut("usedInCatch"));
        context.Validate("Method(3);", LiveIn("usedInCatch"), LiveOut("usedInCatch"));
        context.Validate("Method(4);", LiveIn("usedInCatch"));
        context.ValidateExit();
    }

    [TestMethod]
    public void NestedCatch_LiveOutOuterCatch_NestedTwice()
    {
        const string code = """
            var usedInCatch = 0;
            Method(0);
            try
            {
                Method(1);
                try
                {
                    Method(2);
                    try
                    {
                        usedInCatch = 1;
                        Method(3);
                    }
                    catch
                    {
                        Method(usedInCatch); // This can throw again
                        Method(4);
                    }
                }
                catch
                {
                    Method(usedInCatch);
                    Method(5);
                }
            }
            catch
            {
                Method(usedInCatch);
                Method(6);
            }
            """;
        var context = CreateContextCS(code);
        context.Validate("Method(0);", LiveOut("usedInCatch"));
        context.Validate("Method(1);", LiveIn("usedInCatch"), LiveOut("usedInCatch"));
        context.Validate("Method(2);", LiveIn("usedInCatch"), LiveOut("usedInCatch"));
        context.Validate("Method(3);", LiveOut("usedInCatch"));
        context.Validate("Method(4);", LiveIn("usedInCatch"), LiveOut("usedInCatch"));
        context.Validate("Method(5);", LiveIn("usedInCatch"), LiveOut("usedInCatch"));
        context.Validate("Method(6);", LiveIn("usedInCatch"));
        context.ValidateExit();
    }

    [TestMethod]
    public void NestedCatch_LiveOutOuterCatch_ForEach()
    {
        const string code = """
            var usedInCatch = 0;
            Method(0);
            try
            {
                Method(1);
                try
                {
                    Method(2);
                    var list = new List<int>();
                    foreach(var i in list)
                    {
                        Method(i);
                        Method(usedInCatch);
                    }
                }
                catch
                {
                    Method(usedInCatch);
                    Method(3);
                }
            }
            catch
            {
                Method(usedInCatch);
                Method(4);
            }
            """;
        var context = CreateContextCS(code);
        context.Validate("Method(0);", LiveOut("usedInCatch"));
        context.Validate("Method(1);", LiveIn("usedInCatch"), LiveOut("usedInCatch"));
        context.Validate("Method(2);", LiveIn("usedInCatch"), LiveOut("list", "usedInCatch"));
        context.Validate("Method(i);", LiveIn("usedInCatch"), LiveOut("usedInCatch"));
        context.Validate("Method(3);", LiveIn("usedInCatch"), LiveOut("usedInCatch"));
        context.Validate("Method(4);", LiveIn("usedInCatch"));
        context.ValidateExit();
    }

    [TestMethod]
    public void NestedCatch_LiveOutOuterFilterHandler_FromInnerCatch()
    {
        const string code = """
            int usedInCatch = 0;
            Method(0);
            try
            {
                try
                {
                    usedInCatch = 2;
                    Method(1);
                }
                catch
                {
                    Method(usedInCatch);
                    Method(2);
                }
            }
            catch when(usedInCatch > 10)
            {
                Method(usedInCatch);
                Method(3);
            }
            """;
        var context = CreateContextCS(code);
        context.Validate("Method(0);", LiveOut("usedInCatch"));
        context.Validate("Method(1);", LiveOut("usedInCatch"));
        context.Validate("Method(2);", LiveIn("usedInCatch"),  LiveOut("usedInCatch"));
        context.Validate("Method(3);", LiveIn("usedInCatch"));
        context.ValidateExit();
    }

    [TestMethod]
    public void NestedCatch_LiveOutOuterCatch_FromInnerFilterHandler()
    {
        const string code = """
            int usedInCatch = 0;
            Method(0);
            try
            {
                try
                {
                    usedInCatch = 2;
                    Method(1);
                }
                catch when (true)
                {
                    Method(usedInCatch);
                    Method(2);
                }
            }
            catch
            {
                Method(usedInCatch);
                Method(3);
            }
            """;
        var context = CreateContextCS(code);
        context.Validate("Method(0);", LiveOut("usedInCatch"));
        context.Validate("Method(1);", LiveOut("usedInCatch"));
        context.Validate("Method(2);", LiveIn("usedInCatch"), LiveOut("usedInCatch"));
        context.Validate("Method(3);", LiveIn("usedInCatch"));
        context.ValidateExit();
    }

    [TestMethod]
    public void NestedCatch_LiveOutOuterCatch_FromFilterHandler()
    {
        const string code = """
            int usedInCatch = 0;
            Method(0);
            try
            {
                try
                {
                    usedInCatch = 2;
                    Method(1);
                }
                catch when(usedInCatch > 10 )
                {
                    usedInCatch = 3;
                    Method(2);
                }
            }
            catch
            {
                Method(usedInCatch);
                Method(3);
            }
            """;
        var context = CreateContextCS(code);
        context.Validate("Method(0);", LiveOut("usedInCatch"));
        context.Validate("Method(1);", LiveOut("usedInCatch"));
        context.Validate("Method(2);", LiveOut("usedInCatch"));
        context.Validate("Method(3);", LiveIn("usedInCatch"));
        context.ValidateExit();
    }

    [TestMethod]
    public void NestedCatch_LiveOutOuterCatch_CanThrowFromFilterHandler()
    {
        const string code = """
            int usedInCatch = 0;
            Method(0);
            try
            {
                try
                {
                    Method(1);
                }
                catch when(Method(2) == usedInCatch)
                {
                    usedInCatch = 1;
                    Method(3);
                }
            }
            catch
            {
                Method(usedInCatch);
                Method(4);
            }
            """;
        var context = CreateContextCS(code);
        context.Validate("Method(0);", LiveOut("usedInCatch"));
        context.Validate("Method(1);", LiveIn("usedInCatch"), LiveOut("usedInCatch"));
        context.Validate("Method(2) == usedInCatch", LiveIn("usedInCatch"), LiveOut("usedInCatch"));
        context.Validate("Method(3);", LiveOut("usedInCatch"));
        context.Validate("Method(4);", LiveIn("usedInCatch"));
        context.ValidateExit();
    }

    [TestMethod]
    public void NestedCatch_LiveOut_ConsecutiveOuterCatch()
    {
        const string code = """
            var usedInCatch = 100;
            try
            {
                try
                {
                     Method(usedInCatch);
                     Method(0);
                }
                catch
                {
                    usedInCatch = 200;
                    Method(1);
                }
            }
            catch (ArgumentNullException)
            {
                Method(2);
                Method(usedInCatch);
            }
            catch (IOException)
            {
                Method(3);
                Method(usedInCatch);
            }
            catch (NullReferenceException)
            {
                Method(4);
                Method(usedInCatch);
            }
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(0);", LiveIn("usedInCatch")); // No LiveOut, because inner Catch catches all exceptions and reassigns value
        context.Validate("Method(1);", LiveOut("usedInCatch"));
        context.Validate("Method(2);", LiveIn("usedInCatch"));
        context.Validate("Method(3);", LiveIn("usedInCatch"));
        context.Validate("Method(4);", LiveIn("usedInCatch"));
        context.ValidateExit();
    }

    [TestMethod]
    public void Catch_TryHasLocalLifetimeRegion_LiveIn()
    {
        const string code = """
            try
            {
                Method(0);
                var t = true || true; // This causes LocalLivetimeRegion to be generated
            }
            catch
            {
                Method(intParameter);
            }
            Method(1);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("t = true || true", LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(intParameter);", LiveIn("intParameter"));
        context.Validate("Method(1);");
        context.ValidateExit();
    }

    [TestMethod]
    public void Catch_VariableUsedAfter_LiveIn_LiveOut()
    {
        const string code = """
            try
            {
                Method(0);
            }
            catch
            {
                Method(1);
            }
            Method(intParameter);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(0);", LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(1);", LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(intParameter);", LiveIn("intParameter"));
        context.ValidateExit();
    }

    [TestMethod]
    public void Catch_WithThrowStatement_LiveIn()
    {
        const string code = """
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
            Method(intParameter); // Unreachable
            """;
        var context = CreateContextCS(code);
        // LVA doesn't care if it's reachable. Blocks still should have LiveIn and LiveOut
        context.ValidateEntry();    // intParameter is used only in unreachable path => not visible here
        context.Validate("Method(usedInTry);", LiveIn("usedInTry", "usedInCatch"), LiveOut("usedInCatch"));         // Doesn't see usedInTryUnreachable nor intParameter
        context.Validate("Method(usedInTryUnreachable);", LiveIn("intParameter", "usedInTryUnreachable", "usedInCatch"), LiveOut("intParameter", "usedInCatch"));
        context.Validate("Method(usedInCatch);", LiveIn("usedInCatch"));                                            // Doesn't see usedInCatchUnreachable nor intParameter
        context.Validate("Method(usedInCatchUnreachable);", LiveIn("intParameter", "usedInCatchUnreachable"), LiveOut("intParameter"));
        context.Validate("Method(intParameter);", LiveIn("intParameter"));
        context.ValidateExit();
    }

    [TestMethod]
    public void Catch_WithThrowStatement_Conditional_LiveIn()
    {
        const string code = """
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
            Method(intParameter);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("intParameter", "boolParameter"), LiveOut("intParameter", "boolParameter"));
        context.Validate("Method(0);", LiveIn("intParameter", "boolParameter"), LiveOut("intParameter", "boolParameter"));
        context.Validate("Method(1);", LiveIn("intParameter", "boolParameter"), LiveOut("intParameter"));
        context.Validate("boolParameter", LiveIn("intParameter", "boolParameter"), LiveOut("intParameter"));
        context.Validate("Method(2);", LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(intParameter);", LiveIn("intParameter"));
        context.ValidateExit();
    }

    [TestMethod]
    public void Catch_Rethrow_LiveIn()
    {
        const string code = """
            try
            {
                Method(0);
            }
            catch
            {
                Method(1);
                throw;
            }
            Method(intParameter);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(0);", LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(1);");
        context.Validate("Method(intParameter);", LiveIn("intParameter"));
        context.ValidateExit();
    }

    [TestMethod]
    public void Catch_NotLiveIn_NotLiveOut()
    {
        const string code = """
            try
            {
                Method(0);
            }
            catch
            {
                intParameter = 42;
                Method(intParameter);
            }
            Method(1);
            """;
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
        const string code = """
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
            Method(2);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(0);", LiveIn("inner", "outer"), LiveOut("inner", "outer"));
        context.Validate("Method(inner);", LiveIn("inner", "outer"), LiveOut("outer"));
        context.Validate("Method(1);", LiveIn("outer"), LiveOut("outer"));
        context.Validate("Method(outer);", LiveIn("outer"));
        context.Validate("Method(2);");
        context.ValidateExit();
    }

    [TestMethod]
    public void Catch_InvalidSyntax_LiveIn()
    {
        /*    Entry 0
         *      |
         *    Block 1
         *    Method(0)
         *    Method(intParameter)
         *      |
         *    Exit 2
         */
        const string code = """
            // Error CS1003 Syntax error, 'try' expected
            // Error CS1514 { expected
            // Error CS1513 } expected
            catch
            {
                Method(0);
            }
            Method(intParameter);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(0);", LiveIn("intParameter"));
        context.Validate("Method(intParameter);", LiveIn("intParameter"));
        context.ValidateExit();
    }

    [TestMethod]
    public void Catch_Loop_Propagates_LiveIn_LiveOut()
    {
        const string code = """
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
            Method(1);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("boolParameter", "intParameter"), LiveOut("boolParameter", "intParameter"));
        context.Validate("Method(intParameter);", LiveIn("boolParameter", "intParameter", "variableUsedInCatch"), LiveOut("boolParameter", "intParameter", "variableUsedInCatch"));
        context.Validate("Method(0);", LiveIn("boolParameter", "intParameter", "variableUsedInCatch"), LiveOut("boolParameter", "intParameter", "variableUsedInCatch"));
        context.Validate("Method(variableUsedInCatch);", LiveIn("boolParameter", "intParameter", "variableUsedInCatch"), LiveOut("boolParameter", "intParameter", "variableUsedInCatch"));
        context.Validate("Method(1);");
    }

    [TestMethod]
    public void Catch_ExVariable_LiveIn()
    {
        const string code = """
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
            Method(1);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("boolParameter"), LiveOut("boolParameter"));
        context.Validate("Method(0);", LiveIn("boolParameter"), LiveOut("boolParameter"));
        context.Validate("boolParameter", LiveIn("boolParameter"), LiveOut("ex"));     // ex doesn't live in here, becase this blocks starts with SimpleAssignmentOperation: (Exception ex)
        context.Validate("Method(ex.HResult);", LiveIn("ex"));
        context.Validate("Method(1);");
        context.ValidateExit();
    }

    [TestMethod]
    public void Catch_SingleType_LiveIn()
    {
        const string code = """
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
            Method(usedAfter);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(usedInTry);", LiveIn("usedInTry", "usedAfter", "usedInCatch", "intParameter"), LiveOut("usedAfter", "usedInCatch", "intParameter"));
        context.Validate("Method(intParameter, usedInCatch, ex.HResult);", LiveIn("intParameter", "usedInCatch", "usedAfter"), LiveOut("usedAfter"));
        context.Validate("Method(usedAfter);", LiveIn("usedAfter"));
        context.ValidateExit();
    }

    [TestMethod]
    public void Catch_SingleTypeWhenCondition_LiveIn()
    {
        const string code = """
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
            Method(usedAfter);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(usedInTry);", LiveIn("usedInTry", "usedAfter", "usedInCatch", "intParameter"), LiveOut("usedAfter", "usedInCatch", "intParameter"));
        context.Validate("Method(intParameter, usedInCatch, ex.HResult);", LiveIn("intParameter", "usedInCatch", "usedAfter", "ex"), LiveOut("usedAfter"));
        context.Validate("Method(usedAfter);", LiveIn("usedAfter"));
        context.ValidateExit();
    }

    [TestMethod]
    public void Catch_SingleTypeWhenConditionReferencingArgument_LiveIn()
    {
        const string code = """
            try
            {
                Method(0);
            }
            catch (Exception ex) when (boolParameter)
            {
                Method(intParameter);
            }
            Method(1);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("boolParameter", "intParameter"), LiveOut("boolParameter", "intParameter"));
        context.Validate("Method(0);", LiveIn("boolParameter", "intParameter"), LiveOut("boolParameter", "intParameter"));
        context.Validate("Method(intParameter);", LiveIn("intParameter"));
        context.Validate("Method(1);");
        context.ValidateExit();
    }

    [TestMethod]
    public void Catch_MultipleTypes_LiveIn()
    {
        const string code = """
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
            Method(usedAfter);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(usedInTry);",
            LiveIn("usedInTry", "usedAfter", "usedInCatchA", "usedInCatchB", "intParameter"),
            LiveOut("usedAfter", "usedInCatchA", "usedInCatchB", "intParameter"));
        // ex doesn't live in here, because the blocks starts with SimpleAssignmentOperation: (Exception ex)
        context.Validate("Method(intParameter, usedInCatchA, ex.HResult);", LiveIn("intParameter", "usedInCatchA", "usedAfter"), LiveOut("usedAfter"));
        context.Validate("Method(intParameter, usedInCatchB, ex.HResult);", LiveIn("intParameter", "usedInCatchB", "usedAfter"), LiveOut("usedAfter"));
        context.Validate("Method(usedAfter);", LiveIn("usedAfter"));
        context.ValidateExit();
    }

    [TestMethod]
    public void Catch_AssignedInTry_LiveOut()
    {
        const string code = """
            int variable = 42;
            Method(0);
            try
            {
                Method(1);  // Can throw
                variable = 0;
            }
            catch
            {
                Method(variable);
            }
            Method(2);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(0);", LiveOut("variable"));
        context.Validate("Method(1);", LiveOut("variable"));
        context.Validate("Method(variable);", LiveIn("variable"));
        context.Validate("Method(2);");
    }

    [TestMethod]
    public void Catch_When_AssignedInTry_LiveOut()
    {
        const string code = """
            int variable = 42;
            Method(0);
            try
            {
                Method(1);  // Can throw
                variable = 0;
            }
            catch when(variable == 0)
            {
                Method(2);
            }
            Method(3);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(0);", LiveOut("variable"));
        context.Validate("Method(1);", LiveOut("variable"));
        context.Validate("variable == 0", LiveIn("variable"));
        context.Validate("Method(2);");
        context.Validate("Method(3);");
    }

    [TestMethod]
    public void Catch_Loop_Propagates_LiveIn()
    {
        const string code = """
            var variable = 0;
            Method(0);
            while (variable < 5)
            {
                variable++;
                Method(1);
                try
                {
                    Method(2);  // Can throw
                    return;
                }
                catch (TimeoutException)
                {
                    Method(3); // Continue loop to the next try
                }
                Method(4);
            }
            Method(5);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(0);", LiveOut("variable"));
        context.Validate("Method(1);", LiveIn("variable"), LiveOut("variable"));
        context.Validate("Method(2);", LiveIn("variable"), LiveOut("variable"));
        context.Validate("Method(3);", LiveIn("variable"), LiveOut("variable"));
        context.Validate("Method(4);", LiveIn("variable"), LiveOut("variable"));
        context.Validate("Method(5);");
    }

    [TestMethod]
    public void Throw_NestedCatch_LiveOut()
    {
        const string code = """
            var value = 100;
            try
            {
                try
                {
                     Method(value);
                     Method(0);
                }
                catch
                {
                    value = 200;
                    throw;
                }
            }
            catch
            {
                 Method(value);
                 Method(1);
            }
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(0);", LiveIn("value"));
        context.Validate("value = 200;", LiveOut("value"));
        context.Validate("Method(1);", LiveIn("value"));
        context.ValidateExit();
    }

    [TestMethod]
    public void Throw_NestedCatch_LiveInInConsecutiveOuterCatch()
    {
        const string code = """
            var value = 100;
            try
            {
                try
                {
                     Method(value);
                     Method(0);
                }
                catch
                {
                    value = 200;
                    throw;
                }
            }
            catch (ArgumentNullException)
            {
                Method(value);
                Method(1);
            }
            catch (IOException)
            {
                Method(value);
                Method(2);
            }
            catch (NullReferenceException)
            {
                Method(value);
                Method(3);
            }
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(0);", LiveIn("value"));
        context.Validate("value = 200;", LiveOut("value"));
        context.Validate("Method(1);", LiveIn("value"));
        context.Validate("Method(2);", LiveIn("value"));
        context.Validate("Method(3);", LiveIn("value"));
        context.ValidateExit();
    }

    [TestMethod]
    public void Throw_NestedCatch_LiveInInConsecutiveOuterCatchNewThrow()
    {
        const string code = """
            var value = 100;
            try
            {
                try
                {
                     Method(value);
                     Method(0);
                }
                catch (ArgumentNullException ex)
                {
                    Method(value);
                    Method(1);
                    throw new Exception("Message", ex);
                }
            }
            catch (IOException)
            {
                Method(value);
                Method(2);
            }
            catch (NullReferenceException)
            {
                Method(value);
                Method(3);
            }
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(0);", LiveIn("value"), LiveOut("value"));
        context.Validate("Method(1);", LiveIn("value"), LiveOut("value"));
        context.Validate("Method(2);", LiveIn("value"));
        context.Validate("Method(3);", LiveIn("value"));
        context.ValidateExit();
    }

    [TestMethod]
    public void Throw_NestedCatch_OuterCatchRethrows_LiveOutOuterCatch()
    {
        const string code = """
            var value = 100;
            try
            {
                try
                {
                    try
                    {
                        Method(value);
                        Method(0);
                    }
                    catch
                    {
                        value = 200;
                        throw;
                    }
                }
                catch   // Outer catch
                {
                    Method(value);
                    Method(1);
                    throw;
                }
            }
            catch
            {
                Method(value);
                Method(2);
            }
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(0);", LiveIn("value"));
        context.Validate("value = 200;", LiveOut("value"));
        context.Validate("Method(1);", LiveIn("value"), LiveOut("value"));
        context.Validate("Method(2);", LiveIn("value"));
        context.ValidateExit();
    }

    [TestMethod]
    public void Finally_LiveIn()
    {
        const string code = """
            try
            {
                Method(0);
            }
            finally
            {
                Method(intParameter);
            }
            Method(1);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(0);", LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(intParameter);", LiveIn("intParameter"));
        context.Validate("Method(1);");
        context.ValidateExit();
    }

    [TestMethod]
    public void Finally_VariableUsedAfter_LiveIn_LiveOut()
    {
        const string code = """
            try
            {
                Method(0);
            }
            finally
            {
                Method(1);
            }
            Method(intParameter);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(0);", LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(1);", LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(intParameter);", LiveIn("intParameter"));
        context.ValidateExit();
    }

    [TestMethod]
    public void Finally_VariableUsedAfter_FinallyHasLocalLifetimeRegion_LiveIn_LiveOut()
    {
        const string code = """
            try
            {
                Method(0);
            }
            finally
            {
                Method(1);
                var t = true || true; // This causes LocalLivetimeRegion to be generated, but there's also one empty block outside if before the exit branch
            }
            Method(intParameter);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(0);", LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(1);", LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(intParameter);", LiveIn("intParameter"));
        context.ValidateExit();
    }

    [TestMethod]
    public void Finally_WithThrowStatement_LiveIn()
    {
        const string code = """
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
            Method(intParameter); // Unreachable
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(0);", LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(1);", LiveIn("intParameter"), LiveOut("intParameter"));
        // LVA doesn't care if it's reachable. Blocks still should have LiveIn and LiveOut
        context.Validate("Method(2);", LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(intParameter);", LiveIn("intParameter"));
        context.ValidateExit();
    }

    [TestMethod]
    public void Finally_WithThrowStatementAsSingleExit_LiveIn()
    {
        const string code = """
            try
            {
                Method(0);
            }
            finally
            {
                Method(1);
                throw new Exception();
            }
            Method(intParameter); // Unreachable
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(0);", LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(1);", LiveIn("intParameter"), LiveOut("intParameter"));
        // LVA doesn't care if it's reachable. Blocks still should have LiveIn and LiveOut
        context.Validate("Method(intParameter);", LiveIn("intParameter"));
        context.ValidateExit();
    }

    [TestMethod]
    public void Finally_WithThrowStatement_Conditional_LiveIn()
    {
        const string code = """
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
            Method(intParameter);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("intParameter", "boolParameter"), LiveOut("intParameter", "boolParameter"));
        context.Validate("Method(0);", LiveIn("intParameter", "boolParameter"), LiveOut("intParameter", "boolParameter"));
        context.Validate("Method(1);", LiveIn("intParameter", "boolParameter"), LiveOut("intParameter"));
        context.Validate("boolParameter", LiveIn("intParameter", "boolParameter"), LiveOut("intParameter"));
        context.Validate("Method(2);", LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(intParameter);", LiveIn("intParameter"));
        context.ValidateExit();
    }

    [TestMethod]
    public void Finally_WithThrowStatementInTry_LiveOut()
    {
        const string code = """
            int variable = 42;
            Method(0);
            try
            {
                Method(1);
                throw new Exception();
            }
            finally
            {
                Method(variable);
            }
            Method(2);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(0);", LiveOut("variable"));
        context.Validate("Method(1);", LiveIn("variable"), LiveOut("variable"));
        context.Validate("Method(variable);", LiveIn("variable"));
        context.Validate("Method(2);");
    }

    [TestMethod]
    public void Finally_WithThrowStatementInTry_LiveOut_FilteredCatch()
    {
        const string code = """
            int variable = 42;
            Method(0);
            try
            {
                Method(1);
                throw new Exception();
            }
            catch when (Property == 42) { }     // FilterAndHandlerRegion
            catch (FormatException) { }
            catch (ArgumentException ex) { }
            finally
            {
                Method(variable);
            }
            Method(2);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(0);", LiveOut("variable"));
        context.Validate("Method(1);", LiveIn("variable"), LiveOut("variable"));
        context.Validate("Method(variable);", LiveIn("variable"));
        context.Validate("Method(2);");
    }

    [TestMethod]
    [DataRow("catch")]
    [DataRow("catch (Exception)")]
    [DataRow("catch (Exception ex)")]
    public void Finally_WithThrowStatementInTry_LiveOut_CatchAll(string catchAll)
    {
        var code = $$"""
            int variable = 42;
            Method(0);
            try
            {
                Method(1);
                throw new Exception();
            }
            {{catchAll}}
            {
                Method(2);
                variable = 0;
            }
            finally
            {
                Method(variable);
            }
            Method(3);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(0);");
        context.Validate("Method(1);");
        context.Validate("Method(2);", LiveOut("variable"));
        context.Validate("Method(variable);", LiveIn("variable"));
        context.Validate("Method(3);");
    }

    [TestMethod]
    public void Finally_NotLiveIn_NotLiveOut()
    {
        const string code = """
            try
            {
                Method(0);
            }
            finally
            {
                intParameter = 42;
                Method(intParameter);
            }
            Method(1);
            """;
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
        const string code = """
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
            Method(2);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(0);", LiveIn("inner", "outer"), LiveOut("inner", "outer"));
        context.Validate("Method(inner);", LiveIn("inner", "outer"), LiveOut("outer"));
        context.Validate("Method(1);", LiveIn("outer"), LiveOut("outer"));
        context.Validate("Method(outer);", LiveIn("outer"));
        context.Validate("Method(2);");
        context.ValidateExit();
    }

    [TestMethod]
    public void Finally_Nested_NoInstructionBetweenFinally_LiveIn()
    {
        const string code = """
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
                // No action here, finally branch is crossing both regions
            }
            finally
            {
                Method(outer);
            }
            Method(2);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(0);", LiveIn("inner", "outer"), LiveOut("inner", "outer"));
        context.Validate("Method(inner);", LiveIn("inner", "outer"), LiveOut("outer"));
        context.Validate("Method(outer);", LiveIn("outer"));
        context.Validate("Method(2);");
        context.ValidateExit();
    }

    [TestMethod]
    public void NestedFinally_LiveOut_OuterCatch()
    {
        const string code = """
            var usedInOuterCatch = 0;
            Method(0);
            try
            {
                Method(1);
                try
                {
                    Method(2);
                }
                finally
                {
                    usedInOuterCatch = 2;
                    Method(3);
                }
            }
            catch (Exception ex)
            {
                Method(usedInOuterCatch);
                Method(4);
            }
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(0);", LiveOut("usedInOuterCatch"));
        context.Validate("Method(1);", LiveIn("usedInOuterCatch"), LiveOut("usedInOuterCatch"));
        context.Validate("Method(2);", LiveIn("usedInOuterCatch"), LiveOut("usedInOuterCatch"));
        context.Validate("Method(3);", LiveOut("usedInOuterCatch"));
        context.Validate("Method(4);", LiveIn("usedInOuterCatch"));
        context.ValidateExit();
    }

    [TestMethod]
    public void NestedFinally_LiveOut_NestedTryInOuterCatch()
    {
        const string code = """
            var usedInOuterTry = 0;
            Method(0);
            try
            {
                Method(1);
                try
                {
                    Method(2);
                }
                finally
                {
                    usedInOuterTry = 2;
                    Method(3);
                }
            }
            catch
            {
                try
                {
                    Method(usedInOuterTry);
                    Method(4);
                }
                catch
                {
                    Method(5);
                }
            }
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(0);", LiveOut("usedInOuterTry"));
        context.Validate("Method(1);", LiveIn("usedInOuterTry"), LiveOut("usedInOuterTry"));
        context.Validate("Method(2);", LiveIn("usedInOuterTry"), LiveOut("usedInOuterTry"));
        context.Validate("Method(3);", LiveOut("usedInOuterTry"));
        context.Validate("Method(4);", LiveIn("usedInOuterTry"));
        context.Validate("Method(5);");
        context.ValidateExit();
    }

    [TestMethod]
    public void NestedFinally_LiveOut_NestedFinallyInOuterCatch()
    {
        const string code = """
            var usedInOuterFinally = 0;
            Method(0);
            try
            {
                Method(usedInOuterFinally);
                Method(1);
                try
                {
                    Method(2);
                }
                finally
                {
                    usedInOuterFinally = 2; // Compliant - used in outer finally
                    Method(3);
                }
            }
            catch
            {
                try
                {
                    Method(4);
                }
                finally
                {
                    Method(usedInOuterFinally);
                    Method(5);
                }
            }
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(0);", LiveOut("usedInOuterFinally"));
        context.Validate("Method(1);", LiveIn("usedInOuterFinally"), LiveOut("usedInOuterFinally"));
        context.Validate("Method(2);", LiveIn("usedInOuterFinally"), LiveOut("usedInOuterFinally"));
        context.Validate("Method(3);", LiveOut("usedInOuterFinally"));
        context.Validate("Method(4);", LiveIn("usedInOuterFinally"), LiveOut("usedInOuterFinally"));
        context.Validate("Method(5);", LiveIn("usedInOuterFinally"));
        context.ValidateExit();
    }

    [TestMethod]
    public void NestedFinally_LiveOut_NestedFinallyInOuter_ConsecutiveCatch()
    {
        const string code = """
            var usedInOuterFinally = 0;
            Method(0);
            try
            {
                Method(usedInOuterFinally);
                Method(1);
                try
                {
                    Method(2);
                }
                finally
                {
                    usedInOuterFinally = 2; // Compliant - used in outer finally
                    Method(3);
                }
            }
            catch(NotImplementedException)
            {
                Method(4);
            }
            catch
            {
                try
                {
                    Method(5);
                }
                finally
                {
                    Method(6);
                    Method(usedInOuterFinally);
                }
            }
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(0);", LiveOut("usedInOuterFinally"));
        context.Validate("Method(1);", LiveIn("usedInOuterFinally"), LiveOut("usedInOuterFinally"));
        context.Validate("Method(2);", LiveIn("usedInOuterFinally"), LiveOut("usedInOuterFinally"));
        context.Validate("Method(3);", LiveOut("usedInOuterFinally"));
        context.Validate("Method(4);");
        context.Validate("Method(5);", LiveIn("usedInOuterFinally"), LiveOut("usedInOuterFinally"));
        context.Validate("Method(6);", LiveIn("usedInOuterFinally"));
        context.ValidateExit();
    }

    [TestMethod]
    public void NestedFinally_LiveOutNestedFinallyInOuter_FinallyHasLocalLifetime()
    {
        const string code = """
            var usedInOuterFinally = 0;
            Method(0);
            try
            {
                Method(usedInOuterFinally);
                Method(1);
                try
                {
                    Method(2);
                }
                finally
                {
                    var t = usedInOuterFinally  > 1 ? 1 : 0; // This causes LocalLifetimeRegion to be generated
                }
            }
            catch
            {
                try
                {
                    Method(4);
                }
                finally
                {
                    Method(5);
                    Method(usedInOuterFinally);
                }
            }
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(0);", LiveOut("usedInOuterFinally"));
        context.Validate("Method(1);", LiveIn("usedInOuterFinally"), LiveOut("usedInOuterFinally"));
        context.Validate("Method(2);", LiveIn("usedInOuterFinally"), LiveOut("usedInOuterFinally"));
        context.Validate("t = usedInOuterFinally  > 1 ? 1 : 0", LiveIn("usedInOuterFinally"), LiveOut("usedInOuterFinally"));
        context.Validate("Method(4);", LiveIn("usedInOuterFinally"), LiveOut("usedInOuterFinally"));
        context.Validate("Method(5);", LiveIn("usedInOuterFinally"));
        context.ValidateExit();
    }

    [TestMethod]
    public void Finally_ForEach_LiveIn()
    {
        const string code = """
            var outer = 42;
            try
            {
                Method(0);
                foreach (var arg in args)   // Produces implicit finally
                {
                }
                // No action here, finally branch is crossing both regions
            }
            finally
            {
                Method(outer);
            }
            Method(1);
            """;
        var context = CreateContextCS(code, null, "object[] args");
        context.ValidateEntry(LiveIn("args"), LiveOut("args"));
        context.Validate("Method(0);", LiveIn("outer", "args"), LiveOut("outer", "args"));
        context.Validate("Method(outer);", LiveIn("outer"));
        context.Validate("Method(1);");
        context.ValidateExit();
    }

    [TestMethod]
    public void Finally_InvalidSyntax_LiveIn()
    {
        /*    Entry 0
         *      |
         *    Block 1
         *    Method(0)
         *    Method(intParameter)
         *      |
         *    Exit 2
         */
        const string code = """
            // Error CS1003 Syntax error, 'try' expected
            // Error CS1514 { expected
            // Error CS1513 } expected
            finally
            {
                Method(0);
            }
            Method(intParameter);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("intParameter"), LiveOut("intParameter"));
        context.Validate("Method(0);", LiveIn("intParameter"));
        context.Validate("Method(intParameter);", LiveIn("intParameter"));
        context.ValidateExit();
    }

    [TestMethod]
    public void Finally_Loop_Propagates_LiveIn_LiveOut()
    {
        const string code = """
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
            Method(2);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("boolParameter", "intParameter"), LiveOut("boolParameter", "intParameter"));
        context.Validate("Method(intParameter);", LiveIn("boolParameter", "intParameter"), LiveOut("boolParameter", "intParameter"));
        context.Validate("Method(0);", LiveIn("boolParameter", "intParameter"), LiveOut("boolParameter", "intParameter"));
        context.Validate("Method(1);", LiveIn("boolParameter", "intParameter"), LiveOut("boolParameter", "intParameter"));
        context.Validate("Method(2);");
    }

    [TestMethod]
    public void Finally_Loop_Propagates_FinallyHasLocalLifetimeRegion_LiveIn_LiveOut()
    {
        const string code = """
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
                var t = true || true; // This causes LocalLivetimeRegion to be generated
                Method(1);
            }
            goto A;
            B:
            Method(2);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry(LiveIn("boolParameter", "intParameter"), LiveOut("boolParameter", "intParameter"));
        context.Validate("Method(intParameter);", LiveIn("boolParameter", "intParameter"), LiveOut("boolParameter", "intParameter"));
        context.Validate("Method(0);", LiveIn("boolParameter", "intParameter"), LiveOut("boolParameter", "intParameter"));
        context.Validate("Method(1);", LiveIn("boolParameter", "intParameter"), LiveOut("boolParameter", "intParameter"));
        context.Validate("Method(2);");
    }

    [TestMethod]
    public void Finally_AssignedInTry_LiveOut()
    {
        const string code = """
            int variable = 42;
            Method(0);
            try
            {
                Method(1);  // Can throw
                variable = 0;
            }
            finally
            {
                Method(variable);
            }
            Method(2);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(0);", LiveOut("variable"));
        context.Validate("Method(1);", LiveOut("variable"));
        context.Validate("Method(variable);", LiveIn("variable"));
        context.Validate("Method(2);");
    }

    [TestMethod]
    public void TryCatchFinally_LiveIn()
    {
        const string code = """
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
            Method(usedAfter);
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(usedInTry);", LiveIn("usedInTry", "usedInCatch", "usedInFinally", "usedAfter"), LiveOut("usedInCatch", "usedInFinally", "usedAfter"));
        context.Validate("Method(usedInCatch);", LiveIn("usedInCatch", "usedInFinally", "usedAfter"), LiveOut("usedInFinally", "usedAfter"));
        context.Validate("Method(usedInFinally);", LiveIn("usedInFinally", "usedAfter"), LiveOut("usedAfter"));
        context.Validate("Method(usedAfter);", LiveIn("usedAfter"));
        context.ValidateExit();
    }

    [TestMethod]
    public void TryCatchFinally_Rethrow_ValueLivesOut()
    {
        const string code = """
            var value = 0;
            try
            {
                Method(0);
                value = 42;
            }
            catch
            {
                Method(value);
                value = 1;
                throw;
            }
            finally
            {
                Method(value + 1);
            }
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("value = 1;", LiveIn("value"), LiveOut("value"));
        context.Validate("Method(value + 1);", LiveIn("value"));
        context.ValidateExit();
    }

    [TestMethod]
    public void TryCatchFinally_RethrowOuterFinally()
    {
        const string code = """
            var value = 0;
            try
            {
               Method(0);
               try
               {
                   Method(0);
                   value = 42;
               }
               catch
               {
                   Method(value);
                   value = 2;
                   throw;
               }
            }
            finally
            {
               Method(value + 1);
            }
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("value = 2;", LiveIn("value"), LiveOut("value"));
        context.Validate("Method(value + 1);", LiveIn("value"));
        context.ValidateExit();
    }

    [TestMethod]
    public void TryCatchFinally_RethrowOuterCatchFinally()
    {
        const string code = """
            var value = 0;
            try
            {
               Method(0);
               try
               {
                   Method(1);
                   value = 42;
               }
               catch
               {
                   value = 2;
                   throw;
               }
            }
            catch
            {
               Method(2);
            }
            finally
            {
               Method(value + 1);
            }
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("value = 2;", LiveOut("value"));
        context.Validate("Method(2);", LiveIn("value"), LiveOut("value"));
        context.Validate("Method(value + 1);", LiveIn("value"));
        context.ValidateExit();
    }

    [TestMethod]
    public void TryCatchFinally_RethrowOuterCatchRethrowFinally()
    {
        const string code = """
            var value = 0;
            try
            {
               Method(0);
               try
               {
                   Method(1);
                   value = 42;
               }
               catch
               {
                   value = 2;
                   throw;
               }
            }
            catch
            {
               Method(2);
               throw;
            }
            finally
            {
               Method(value + 1);
            }
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("value = 2;", LiveOut("value"));
        context.Validate("Method(2);", LiveIn("value"), LiveOut("value"));
        context.Validate("Method(value + 1);", LiveIn("value"));
        context.ValidateExit();
    }

    [TestMethod]
    public void TryCatchFinally_RethrowCatchRethrowOuterFinally()
    {
        const string code = """
            var value = 0;
            try
            {
                Method(0);
                try
                {
                   Method(1);
                   value = 42;
                }
                catch
                {
                   value = 2;
                   throw;
                }
                finally
                {
                   Method(value + 1);
                }
            }
            finally
            {
               Method(value + 2);
            }
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("value = 2;", LiveOut("value"));
        context.Validate("Method(value + 1);", LiveIn("value"), LiveOut("value"));
        context.Validate("Method(value + 2);", LiveIn("value"));
        context.ValidateExit();
    }

    [TestMethod]
    public void TryCatchFinally_ConsecutiveCatchAllThrowRethrowFinally()
    {
        const string code = """
            var value = 0;
            try
            {
                Method(0);
                value = 42;
            }
            catch (IOException)
            {
                Method(value);
                value = 1;
                throw;
            }
            catch
            {
                Method(value);
                value = 2;
                throw;
            }
            finally
            {
                Method(value + 1);
            }
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("value = 1;", LiveIn("value"), LiveOut("value"));
        context.Validate("value = 2;", LiveIn("value"), LiveOut("value"));
        context.Validate("Method(value + 1);", LiveIn("value"));
        context.ValidateExit();
    }

    [TestMethod]
    public void TryCatchFinally_ConsecutiveCatchRethrow()
    {
        const string code = """
            var value = 0;
            try
            {
                Method(0);
                value = 42;
            }
            catch (IOException)
            {
                Method(value);
                value = 1;
                throw;
            }
            catch (ArgumentNullException)
            {
                Method(value + 1);
            }
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("value = 1;", LiveIn("value"));
        context.Validate("Method(value + 1);", LiveIn("value"));
        context.ValidateExit();
    }

    [TestMethod]
    public void TryCatchFinally_FilteredCatchRethrowFinally()
    {
        const string code = """
            var value = 0;
            try
            {
                Method(0);
                value = 42;
            }
            catch (IOException)
            {
                Method(value);
                value = 1;
                throw;
            }
            finally
            {
                Method(value + 1);
            }
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("value = 1;", LiveIn("value"), LiveOut("value"));
        context.Validate("Method(value + 1);", LiveIn("value"));
        context.ValidateExit();
    }

    [TestMethod]
    public void TryCatchFinally_MultipleFilteredCatchRethrowFinally()
    {
        const string code = """
            var value = 0;
            try
            {
                Method(0);
                value = 42;
            }
            catch (IOException)
            {
                Method(value);
                value = 1;
                throw;
            }
            catch (ArgumentException)
            {
                Method(value);
                value = 2;
                throw;
            }
            finally
            {
                Method(value + 1);
            }
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("value = 1;", LiveIn("value"), LiveOut("value"));
        context.Validate("value = 2;", LiveIn("value"), LiveOut("value"));
        context.Validate("Method(value + 1);", LiveIn("value"));
        context.ValidateExit();
    }

    [TestMethod]
    public void TryCatchFinally_MultipleFilteredCatchNewThrowFinally()
    {
        const string code = """
            var value = 0;
            try
            {
                Method(0);
                value = 42;
            }
            catch (IOException ex)
            {
                Method(value);
                value = 1;
                throw new Exception("Message", ex);
            }
            catch (ArgumentException)
            {
                Method(value);
                value = 2;
                throw;
            }
            finally
            {
                Method(value + 1);
            }
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("value = 1;", LiveIn("value"), LiveOut("value"));
        context.Validate("value = 2;", LiveIn("value"), LiveOut("value"));
        context.Validate("Method(value + 1);", LiveIn("value"));
        context.ValidateExit();
    }

    [TestMethod]
    public void Throw_NestedCatch_LiveInInConsecutiveOuterCatchFinally()
    {
        const string code = """
            var value = 100;
            try
            {
                try
                {
                     Method(value);
                     Method(0);
                }
                catch
                {
                    value = 200;
                    throw;
                }
            }
            catch (ArgumentNullException)
            {
                Method(value);
                Method(1);
            }
            catch (IOException)
            {
                Method(value);
                Method(2);
            }
            catch (NullReferenceException)
            {
                Method(value);
                Method(3);
            }
            finally
            {
                Method(value + 1);
            }
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(0);", LiveIn("value"), LiveOut("value"));
        context.Validate("value = 200;", LiveOut("value"));
        context.Validate("Method(1);", LiveIn("value"), LiveOut("value"));
        context.Validate("Method(2);", LiveIn("value"), LiveOut("value"));
        context.Validate("Method(3);", LiveIn("value"), LiveOut("value"));
        context.Validate("Method(value + 1);", LiveIn("value"));
        context.ValidateExit();
    }

    [TestMethod]
    public void Throw_NestedCatch_OuterCatchRethrows_LiveOutOuterCatchFinally()
    {
        const string code = """
            var value = 100;
            try
            {
                try
                {
                    try
                    {
                        Method(value);
                        Method(0);
                    }
                    catch
                    {
                        value = 200;
                        throw;
                    }
                }
                catch   // Outer catch
                {
                    Method(value);
                    Method(1);
                    throw;
                }
            }
            catch
            {
                Method(value);
                Method(2);
            }
            finally
            {
                Method(value + 1);
            }
            """;
        var context = CreateContextCS(code);
        context.ValidateEntry();
        context.Validate("Method(0);", LiveIn("value"), LiveOut("value"));
        context.Validate("value = 200;", LiveOut("value"));
        context.Validate("Method(1);", LiveIn("value"), LiveOut("value"));
        context.Validate("Method(2);", LiveIn("value"), LiveOut("value"));
        context.Validate("Method(value + 1);", LiveIn("value"));
        context.ValidateExit();
    }
}

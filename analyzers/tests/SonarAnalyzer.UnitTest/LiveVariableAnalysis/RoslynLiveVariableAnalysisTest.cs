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

using System;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.CFG;
using SonarAnalyzer.CFG.LiveVariableAnalysis;
using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.UnitTest.LiveVariableAnalysis
{
    [TestClass]
    public partial class RoslynLiveVariableAnalysisTest
    {
        [TestMethod]
        public void WriteOnly()
        {
            var code = @"
int a = 1;
var b = Method();
var c = 2 + 3;";
            CreateContextCS(code).ValidateAllEmpty();
        }

        [TestMethod]
        public void ProcessParameterReference_LiveIn()
        {
            var code = @"
Method(intParameter);
IsMethod(boolParameter);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("intParameter", "boolParameter"), new LiveOut("intParameter", "boolParameter"));
            context.Validate("Method(intParameter);", new LiveIn("intParameter", "boolParameter"));
        }

        [TestMethod]
        public void ProcessParameterReference_UsedAsOutArgument_NotLiveIn_NotLiveOut()
        {
            var code = @"Main(true, 0, out outParameter, ref refParameter);";
            var context = CreateContextCS(code, additionalParameters: "out int outParameter, ref int refParameter");
            context.ValidateEntry(new LiveIn("refParameter"), new LiveOut("refParameter"));
            context.Validate(code, new LiveIn("refParameter"));
        }

        [TestMethod]
        public void ProcessParameterReference_InNameOf_NotLiveIn_NotLiveOut()
        {
            var code = @"Method(nameof(intParameter));";
            CreateContextCS(code).ValidateAllEmpty();
        }

        [TestMethod]
        public void ProcessParameterReference_Assigned_NotLiveIn_LiveOut()
        {
            var code = @"
intParameter = 42;
if (boolParameter)
    return;
Method(intParameter);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("Method(intParameter);", new LiveIn("intParameter"));
        }

        [TestMethod]
        public void ProcessParameterReference_FlowCapture_Assigned_NotLiveIn_LiveOut()
        {
            var code = @"
Method(0);
boolParameter = true && false;
Method(1);
IsMethod(boolParameter);
boolParameter = boolParameter && false;
Method(2);
IsMethod(boolParameter);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));           // FIXME: Should be empty
            context.Validate("Method(0);", new LiveIn("boolParameter"), new LiveOut("boolParameter"));  // FIXME: Should be empty
            context.Validate("Method(1);", new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("Method(2);", new LiveIn("boolParameter"));
        }

        [TestMethod]
        public void ProcessParameterReference_MemberBinding_LiveIn()
        {
            var code = @"Method(intParameter.ToString());";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(intParameter.ToString());", new LiveIn("intParameter"));
        }

        [TestMethod]
        public void ProcessParameterReference_MemberBindingByReference_LiveIn()
        {
            var code = @"Capturing(intParameter.CompareTo);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Capturing(intParameter.CompareTo);", new LiveIn("intParameter"));
        }

        [TestMethod]
        public void ProcessParameterReference_MemberBindingByReference_DifferentCfgOnNetFx_LiveIn()
        {
            // This specific char/string scenario produces different CFG shape under .NET Framework build.
            // https://github.com/dotnet/roslyn/issues/56644
            var code = @"
char[] charArray = null;
var stringVariable = ""Lorem Ipsum"";
if (boolParameter)
    var ret = charArray.Any(stringVariable.Contains);
";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
#if NET
            context.Validate("ret = charArray.Any(stringVariable.Contains)", new LiveIn("charArray", "stringVariable"));
#else
            context.Validate("ret = charArray.Any(stringVariable.Contains)", new LiveIn("charArray"));
#endif
        }

        [TestMethod]
        public void ProcessParameterReference_Reassigned_LiveIn()
        {
            var code = @"
intParameter = intParameter + 42;
stringParameter = stringParameter.Replace('a', 'b');
Method(intParameter);
Method(stringParameter);";
            var context = CreateContextCS(code, additionalParameters: "string stringParameter");

            context.ValidateEntry(new LiveIn("intParameter", "stringParameter"), new LiveOut("intParameter", "stringParameter"));
            context.Validate("Method(intParameter);", new LiveIn("intParameter", "stringParameter"));
        }

        [TestMethod]
        public void ProcessParameterReference_SelfAssigned_LiveIn()
        {
            var code = @"
intParameter = intParameter;
Method(intParameter);";
            var context = CreateContextCS(code);

            context.ValidateEntry(new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(intParameter);", new LiveIn("intParameter"));
        }

        [TestMethod]
        public void UsedAfterBranch_LiveOut()
        {
            /*       Binary
             *       /   \
             *    Jump   Simple
             *   return  Method()
             *       \   /
             *        Exit
             */
            var code = @"
if (boolParameter)
    return;
Method(intParameter);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter"));
            context.Validate("boolParameter", new LiveIn("boolParameter", "intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(intParameter);", new LiveIn("intParameter"));
            context.ValidateExit();
        }

        [DataTestMethod]
        [DataRow("Capturing(x => field + variable + intParameter);", DisplayName = "SimpleLambda")]
        [DataRow("Capturing((x) => field + variable + intParameter);", DisplayName = "ParenthesizedLambda")]
        [DataRow("Capturing(x => { Func<int> xxx = () => field + variable + intParameter; return xxx();});", DisplayName = "NestedLambda")]
        [DataRow("VoidDelegate d = delegate { return field + variable + intParameter;};", DisplayName = "AnonymousMethod")]
        [DataRow("var items = from xxx in new int[] { 42, 100 } where xxx > field + variable + intParameter select xxx;", DisplayName = "Query")]
        public void Captured_NotLiveIn_NotLiveOut(string capturingStatement)
        {
            /*       Entry
             *         |
             *       Block 1
             *       /   \
             *      |   Block 2
             *      |   Method()
             *       \   /
             *        Exit
             */
            var code = @$"
var variable = 42;
{capturingStatement}
if (boolParameter)
    return;
Method(field, variable, intParameter);";
            capturingStatement.Should().Contain("field + variable + intParameter");
            var context = CreateContextCS(code);
            var expectedCaptured = new Captured("variable", "intParameter");
            context.ValidateEntry(expectedCaptured, new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("boolParameter", expectedCaptured, new LiveIn("boolParameter"));
            context.Validate("Method(field, variable, intParameter);", expectedCaptured);
            context.ValidateExit(expectedCaptured);
        }

        [TestMethod]
        public void Captured_StaticLambda_NotLiveIn_NotLiveOut()
        {
            var code = @"
Capturing(static x => x + 2);
if (boolParameter)
    return;
Method(0);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("boolParameter", new LiveIn("boolParameter"));
            context.Validate("Method(0);");
        }

        [TestMethod]
        public void Assigned_NotLiveIn_NotLiveOut()
        {
            /*       Entry
             *         |
             *       Block 1
             *       boolParameter
             *       /   \
             *      |   Block 2
             *      |   intParameter=0
             *       \   /
             *        Exit
             */
            var code = @"
if (boolParameter)
    return;
intParameter = 0;";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("boolParameter", new LiveIn("boolParameter"));
            context.Validate("intParameter = 0;");
            context.ValidateExit();
        }

        [TestMethod]
        public void LongPropagationChain_LiveIn_LiveOut()
        {
            /*    Entry
             *      |
             *    Block 1 -------+
             *    declare        |
             *      |            |
             *    Block 2 ------+|
             *    use & assign  ||
             *      |           ||
             *    Block 3 -----+||
             *    assign       |||
             *      |          vvv
             *    Block 4 ---> Exit
             *    use
             */
            var code = @"
var value = 0;
if (boolParameter)
    return;
Method(value);
value = 1;
if (boolParameter)
    return;
value = 42;
if (boolParameter)
    return;
Method(intParameter, value);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter"));
            context.Validate("value = 0", new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter", "value"));
            context.Validate("Method(value);", new LiveIn("boolParameter", "intParameter", "value"), new LiveOut("boolParameter", "intParameter"));
            context.Validate("value = 42;", new LiveIn("boolParameter", "intParameter"), new LiveOut("value", "intParameter"));
            context.Validate("Method(intParameter, value);", new LiveIn("value", "intParameter"));
            context.ValidateExit();
        }

        [TestMethod]
        public void BranchedPropagationChain_LiveIn_LiveOut_CS()
        {
            /*              Binary
             *              boolParameter
             *              /           \
             *             /             \
             *            /               \
             *       Binary             Binary
             *       firstBranch        secondBranch
             *       /      \              /      \
             *      /        \            /        \
             *  Simple     Simple      Simple      Simple
             *  firstTrue  firstFalse  secondTrue  secondFalse
             *      \        /            \        /
             *       \      /              \      /
             *        Simple                Simple
             *        first                 second
             *             \               /
             *              \             /
             *                  Simple
             *                  reassigned
             *                  everywhere
             */
            var code = @"
var everywhere = 42;
var reassigned = 42;
var first = 42;
var firstTrue = 42;
var firstFalse = 42;
var second = 42;
var secondTrue = 42;
var secondFalse = 42;
var firstCondition = boolParameter;
var secondCondition = boolParameter;
if (boolParameter)
{
    if (firstCondition)
    {
        Method(firstTrue);
    }
    else
    {
        Method(firstFalse);
    }
    Method(first);
}
else
{
    if (secondCondition)
    {
        Method(secondTrue);
    }
    else
    {
        Method(secondFalse);
    }
    Method(second);
}
reassigned = 0;
Method(everywhere, reassigned);";
            var context = CreateContextCS(code);
            context.Validate(
                "everywhere = 42",
                new LiveIn("boolParameter"),
                new LiveOut("everywhere", "firstCondition", "firstTrue", "firstFalse", "first", "secondCondition", "secondTrue", "secondFalse", "second"));
            // First block
            context.Validate(
                "firstCondition",
                new LiveIn("everywhere", "firstCondition", "firstTrue", "firstFalse", "first"),
                new LiveOut("everywhere", "firstTrue", "firstFalse", "first"));
            context.Validate(
                "Method(firstTrue);",
                new LiveIn("everywhere", "firstTrue", "first"),
                new LiveOut("everywhere", "first"));
            context.Validate(
                "Method(firstFalse);",
                new LiveIn("everywhere", "firstFalse", "first"),
                new LiveOut("everywhere", "first"));
            context.Validate(
                "Method(first);",
                new LiveIn("everywhere", "first"),
                new LiveOut("everywhere"));
            // Second block
            context.Validate(
                "secondCondition",
                new LiveIn("everywhere", "secondCondition", "secondTrue", "secondFalse", "second"),
                new LiveOut("everywhere", "secondTrue", "secondFalse", "second"));
            context.Validate(
                "Method(secondTrue);",
                new LiveIn("everywhere", "secondTrue", "second"),
                new LiveOut("everywhere", "second"));
            context.Validate(
                "Method(secondFalse);",
                new LiveIn("everywhere", "secondFalse", "second"),
                new LiveOut("everywhere", "second"));
            context.Validate(
                "Method(second);",
                new LiveIn("everywhere", "second"),
                new LiveOut("everywhere"));
            // Common end
            context.Validate("Method(everywhere, reassigned);", new LiveIn("everywhere"));
        }

        [TestMethod]
        public void BranchedPropagationChain_LiveIn_LiveOut_VB()
        {
            /*              Binary
             *              boolParameter
             *              /           \
             *             /             \
             *            /               \
             *       Binary             Binary
             *       firstBranch        secondBranch
             *       /      \              /      \
             *      /        \            /        \
             *  Simple     Simple      Simple      Simple
             *  firstTrue  firstFalse  secondTrue  secondFalse
             *      \        /            \        /
             *       \      /              \      /
             *        Simple                Simple
             *        first                 second
             *             \               /
             *              \             /
             *                  Simple
             *                  reassigned
             *                  everywhere
             */
            var code = @"
Dim Everywhere As Integer = 42
Dim Reassigned As Integer = 42
Dim First As Integer = 42
Dim FirstTrue As Integer = 42
Dim FirstFalse As Integer = 42
Dim Second As Integer = 42
Dim SecondTrue As Integer = 42
Dim SecondFalse As Integer = 42
Dim FirstCondition As Boolean = BoolParameter
Dim SecondCondition As Boolean = BoolParameter
If BoolParameter Then
    If (FirstCondition) Then
        Method(FirstTrue)
    Else
        Method(FirstFalse)
    End If
    Method(First)
Else
    If SecondCondition Then
        Method(SecondTrue)
    Else
        Method(SecondFalse)
    End If
    Method(Second)
End If
Reassigned = 0
Method(Everywhere, Reassigned)";
            var context = CreateContextVB(code);
            context.Validate(
                "Everywhere As Integer = 42",
                new LiveIn("BoolParameter"),
                new LiveOut("Everywhere", "FirstCondition", "FirstTrue", "FirstFalse", "First", "SecondCondition", "SecondTrue", "SecondFalse", "Second"));
            // First block
            context.Validate(
                "FirstCondition",
                new LiveIn("Everywhere", "FirstCondition", "FirstTrue", "FirstFalse", "First"),
                new LiveOut("Everywhere", "FirstTrue", "FirstFalse", "First"));
            context.Validate(
                "Method(FirstTrue)",
                new LiveIn("Everywhere", "FirstTrue", "First"),
                new LiveOut("Everywhere", "First"));
            context.Validate(
                "Method(FirstFalse)",
                new LiveIn("Everywhere", "FirstFalse", "First"),
                new LiveOut("Everywhere", "First"));
            context.Validate(
                "Method(First)",
                new LiveIn("Everywhere", "First"),
                new LiveOut("Everywhere"));
            // Second block
            context.Validate(
                "SecondCondition",
                new LiveIn("Everywhere", "SecondCondition", "SecondTrue", "SecondFalse", "Second"),
                new LiveOut("Everywhere", "SecondTrue", "SecondFalse", "Second"));
            context.Validate(
                "Method(SecondTrue)",
                new LiveIn("Everywhere", "SecondTrue", "Second"),
                new LiveOut("Everywhere", "Second"));
            context.Validate(
                "Method(SecondFalse)",
                new LiveIn("Everywhere", "SecondFalse", "Second"),
                new LiveOut("Everywhere", "Second"));
            context.Validate(
                "Method(Second)",
                new LiveIn("Everywhere", "Second"),
                new LiveOut("Everywhere"));
            // Common end
            context.Validate("Method(Everywhere, Reassigned)", new LiveIn("Everywhere"));
        }

        [TestMethod]
        public void ProcessBlockInternal_EvaluationOrder_UsedBeforeAssigned_LiveIn()
        {
            var code = @"
var variable = 42;
if (boolParameter)
    return;
Method(variable, variable = 42);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("Method(variable, variable = 42);", new LiveIn("variable"));
        }

        [TestMethod]
        public void ProcessBlockInternal_EvaluationOrder_UsedBeforeAssignedInSubexpression_LiveIn()
        {
            var code = @"
var variable = 42;
if (boolParameter)
    return;
Method(1 + 1 + Method(variable), variable = 42);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("Method(1 + 1 + Method(variable), variable = 42);", new LiveIn("variable"));
        }

        [TestMethod]
        public void ProcessBlockInternal_EvaluationOrder_AssignedBeforeUsed_NotLiveIn()
        {
            var code = @"
var variable = 42;
if (boolParameter)
    return;
Method(variable = 42, variable);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("Method(variable = 42, variable);");
        }

        [TestMethod]
        public void ProcessBlockInternal_EvaluationOrder_AssignedBeforeUsedInSubexpression_NotLiveIn()
        {
            var code = @"
var variable = 42;
if (boolParameter)
    return;
Method(variable = 42, 1 + 1 + Method(variable));";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("Method(variable = 42, 1 + 1 + Method(variable));");
        }

        [TestMethod]
        public void ProcessLocalReference_InNameOf_NotLiveIn_NotLiveOut()
        {
            var code = @"
var variable = 42;
if (boolParameter)
    return;
Method(nameof(variable));";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("Method(nameof(variable));");
        }

        [TestMethod]
        public void ProcessLocalReference_LocalScopeSymbol_LiveIn()
        {
            var code = @"
var variable = 42;
if (boolParameter)
    return;
Method(intParameter, variable);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter"));
            context.Validate("Method(intParameter, variable);", new LiveIn("intParameter", "variable"));
        }

        [TestMethod]
        public void ProcessLocalReference_ReassignedBeforeLastBlock_LiveIn()
        {
            var code = @"
var variable = 0;
variable = 42;
if (boolParameter)
    return;
Method(intParameter, variable);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter"));
            context.Validate("Method(intParameter, variable);", new LiveIn("intParameter", "variable"));
        }

        [TestMethod]
        public void ProcessLocalReference_ReassignedInLastBlock_NotLiveIn()
        {
            var code = @"
var variable = 0;
if (boolParameter)
    return;
variable = 42;
Method(intParameter, variable);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter"));
            context.Validate("Method(intParameter, variable);", new LiveIn("intParameter"));
        }

        [TestMethod]
        public void ProcessLocalReference_GlobalScopeSymbol_NotLiveIn_NotLiveOut()
        {
            var code = @"
var s = new Sample();
Method(field, s.Property);";
            CreateContextCS(code).ValidateAllEmpty();
        }

        [TestMethod]
        public void ProcessLocalReference_UndefinedSymbol_NotLiveIn_NotLiveOut()
        {
            var code = @"Method(undefined);";
            CreateContextCS(code).ValidateAllEmpty();
        }

        [TestMethod]
        public void ProcessLocalReference_UsedAsOutArgument_NotLiveIn()
        {
            var code = @"
var refVariable = 42;
var outVariable = 42;
if (boolParameter)
    return;
Main(true, 0, out outVariable, ref refVariable);";
            var context = CreateContextCS(code, additionalParameters: "out int outParameter, ref int refParameter");
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("Main(true, 0, out outVariable, ref refVariable);", new LiveIn("refVariable"));
        }

        [TestMethod]
        public void ProcessLocalReference_NotAssigned_LiveIn_LiveOut()
        {
            var code = @"
var variable = intParameter;
if (boolParameter)
    return;
Method(variable, intParameter);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter"));
            context.Validate("variable = intParameter", new LiveIn("intParameter", "boolParameter"), new LiveOut("variable", "intParameter"));
            context.Validate("Method(variable, intParameter);", new LiveIn("variable", "intParameter"));
        }

        [TestMethod]
        public void ProcessLocalReference_VariableDeclarator_NotLiveIn_LiveOut()
        {
            var code = @"
int intValue = 42;
var varValue = 42;
if (intValue == 0)
    Method(intValue, varValue);";
            var context = CreateContextCS(code);
            context.ValidateEntry();
            context.Validate("intValue = 42", new LiveOut("intValue", "varValue"));
            context.Validate("Method(intValue, varValue);", new LiveIn("intValue", "varValue"));
        }

        [TestMethod]
        public void ProcessLocalReference_MemberBinding_LiveIn()
        {
            var code = @"
var variable = 42;
if (boolParameter)
    Method(variable.ToString());";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("Method(variable.ToString());", new LiveIn("variable"));
        }

        [TestMethod]
        public void ProcessLocalReference_MemberBindingByReference_LiveIn()
        {
            var code = @"
var variable = 42;
if (boolParameter)
    Capuring(variable.CompareTo);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("Capuring(variable.CompareTo);", new LiveIn("variable"));
        }

        [TestMethod]
        public void ProcessLocalReference_Reassigned_LiveIn()
        {
            var code = @"
var intVariable = 40;
var stringVariable = ""Lorem Ipsum"";
if (boolParameter)
    return;
intVariable = intVariable + 2;
stringVariable = stringVariable.Replace('a', 'b');
Method(intVariable);
Method(stringVariable);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("boolParameter", new LiveIn("boolParameter"), new LiveOut("intVariable", "stringVariable"));
            context.Validate("Method(intVariable);", new LiveIn("intVariable", "stringVariable"));
        }

        [TestMethod]
        public void ProcessLocalReference_SelfAssigned_LiveIn()
        {
            var code = @"
var intVariable = 42;
if (boolParameter)
    return;
intVariable = intVariable;
Method(intVariable);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("boolParameter", new LiveIn("boolParameter"), new LiveOut("intVariable"));
            context.Validate("Method(intVariable);", new LiveIn("intVariable"));
        }

        [TestMethod]
        public void ProcessSimpleAssignment_Discard_NotLiveIn_NotLiveOut()
        {
            var code = @"_ = intParameter;";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("_ = intParameter;", new LiveIn("intParameter"));
        }

        [TestMethod]
        public void ProcessSimpleAssignment_UndefinedSymbol_NotLiveIn_NotLiveOut()
        {
            var code = @"
undefined = intParameter;
if (undefined == 0)
    Method(undefined);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(undefined);");
        }

        [TestMethod]
        public void ProcessSimpleAssignment_GlobalScoped_NotLiveIn_NotLiveOut()
        {
            var code = @"
field = intParameter;
if (field == 0)
    Method(field);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(field);");
        }

        [TestMethod]
        public void ProcessSimpleAssignment_LocalScoped_NotLiveIn_LiveOut()
        {
            var code = @"
int value;
value = 42;
if (value == 0)
    Method(value);";
            var context = CreateContextCS(code);
            context.ValidateEntry();
            context.Validate("value = 42;", new LiveOut("value"));
            context.Validate("Method(value);", new LiveIn("value"));
        }

        [TestMethod]
        public void ProcessVariableInForeach_Declared_LiveIn_LiveOut()
        {
            /*
             * Entry
             *    |
             * Block 1
             * new[] {1, 2, 3}
             *    |
             * Block 2 <------------------------+
             * MoveNext branch                  |
             * F|   \ Else                      |
             *  v    \                          |
             * Exit  Block 3                    |
             *       i=capture.Current          |
             *       Method(i, intParameter) -->+
             *        |                         A
             *       Block 4 ------------------>+
             *       Method(i)
             */
            var code = @"
foreach(var i in new int[] {1, 2, 3})
{
    Method(i, intParameter);
    if (boolParameter)
      Method(i);
}";
            var context = CreateContextCS(code);
            context.Validate(context.Cfg.Blocks[1], null, new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter"));
            context.Validate(context.Cfg.Blocks[2], null, new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter"));
            context.Validate("Method(i, intParameter);", new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter", "i"));
            context.Validate("Method(i);", new LiveIn("boolParameter", "intParameter", "i"), new LiveOut("boolParameter", "intParameter"));
            context.ValidateExit();
        }

        [TestMethod]
        public void ProcessVariableInForeach_Reused_LiveIn_LiveOut()
        {
            var code = @"
int i = 42;
foreach(i in new int[] {1, 2, 3})
{
    Method(i, intParameter);
}";
            var context = CreateContextCS(code);
            context.Validate(context.Cfg.Blocks[1], null, new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate(context.Cfg.Blocks[2], null, new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate("Method(i, intParameter);", new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.ValidateExit();
        }

        [TestMethod]
        public void Loop_Propagates_LiveIn_LiveOut()
        {
            var code = @"
A:
Method(intParameter);
if (boolParameter)
    goto B;
Method(0);
goto A;
B:
Method(1);";
            var context = CreateContextCS(code);
            context.ValidateEntry(new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter"));
            context.Validate("boolParameter", new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter"));
            context.Validate("Method(0);", new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter"));
            context.Validate("Method(1);");
        }

        [TestMethod]
        public void InvokedDelegate_LiveIn()
        {
            var code = @"action();";
            var context = CreateContextCS(code, additionalParameters: "Action action");
            context.ValidateEntry(new LiveIn("action"), new LiveOut("action"));
            context.Validate("action();", new LiveIn("action"));
        }

        [TestMethod]
        public void ReturningDelegate_LiveIn()
        {
            var code = @"
using System;
public class Sample
{
    public Func<int> Main(int intParameter) => () => intParameter;
}";
            var context = new Context(code, true);
            context.ValidateEntry(new Captured("intParameter"));
            context.Validate("() => intParameter", new Captured("intParameter"));
        }

        [TestMethod]
        public void ReturningDelegate_NestedByReference_LiveIn()
        {
            var code = @"
using System;
using System.Threading.Tasks;
public class Sample
{
    public Action<T> Main<T>(Func<T, Task> asyncHandler)
    {
        return x =>
        {
            Task Wrap() => asyncHandler(x);
            RunTask(Wrap);
        };
    }

    private void RunTask(Func<Task> f) { }
}";
            var context = new Context(code, true);
            context.ValidateEntry(new Captured("asyncHandler"));
        }

        [TestMethod]
        public void ReturningDelegate_Nested_LiveIn()
        {
            var code = @"
using System;
public class Sample
{
    public Func<int> Main(int intParameter) =>
        () =>
        {
            int NestedLocalFunction() => intParameter;

            return NestedLocalFunction();
        };
}";
            var context = new Context(code, true);
            context.ValidateEntry(new Captured("intParameter"));
        }

        private static Context CreateContextCS(string methodBody, string localFunctionName = null, string additionalParameters = null)
        {
            additionalParameters = additionalParameters == null ? null : ", " + additionalParameters;
            var code = @$"
using System;
using System.IO;
using System.Linq;
public class Sample
{{
    delegate void VoidDelegate();

    private int field;
    public int Property {{ get; set; }}

    public void Main(bool boolParameter, int intParameter{additionalParameters})
    {{
        {methodBody}
    }}

    private int Method(params int[] args) => 42;
    private string Method(params string[] args) => null;
    private bool IsMethod(params bool[] args) => true;
    private void Capturing(Func<int, int> f) {{ }}
}}";
            return new Context(code, true, localFunctionName);
        }

        private static Context CreateContextVB(string methodBody)
        {
            var code = @$"
Public Class Sample

    Public Delegate Sub VoidDelegate()

    Private Field As Integer
    Private Property Prop As Integer

    Public Sub Main(BoolParameter As Boolean, IntParameter As Integer)
        {methodBody}
    End Sub

    Private Function Method(ParamArray Args() As Integer) As Integer
    End Function

    Private Function Method(ParamArray Args() As String) As String
    End Function

    Private Function IsMethod(ParamArray Args() As Boolean) As Boolean
    End Function

    Private Sub Capturing(f As Func(Of Integer, Integer))
    End Sub

End Class";
            return new Context(code, false);
        }

        private class Context
        {
            public readonly RoslynLiveVariableAnalysis Lva;
            public readonly ControlFlowGraph Cfg;

            public Context(string code, bool isCSharp, string localFunctionName = null)
            {
                IMethodSymbol originalDeclaration;
                Cfg = TestHelper.CompileCfg(code, isCSharp);
                if (localFunctionName == null)
                {
                    originalDeclaration = (IMethodSymbol)Cfg.OriginalOperation.SemanticModel.GetDeclaredSymbol(Cfg.OriginalOperation.Syntax);
                }
                else
                {
                    originalDeclaration = Cfg.LocalFunctions.Single(x => x.Name == localFunctionName);
                    Cfg = Cfg.GetLocalFunctionControlFlowGraph(originalDeclaration);
                }
                Console.WriteLine(CfgSerializer.Serialize(Cfg));
                Lva = new RoslynLiveVariableAnalysis(Cfg, originalDeclaration);
            }

            public void ValidateAllEmpty()
            {
                foreach (var block in Cfg.Blocks)
                {
                    Validate(block, null, new Expected[] { });
                }
            }

            public void ValidateEntry(params Expected[] expected) =>
                Validate(Cfg.EntryBlock, null, expected);

            public void ValidateExit(params Captured[] expected) =>
                Validate(Cfg.ExitBlock, null, expected);

            public void Validate(string withSyntax, params Expected[] expected)
            {
                var block = Cfg.Blocks.Single(x => x.Kind == BasicBlockKind.Block && (withSyntax == null || x.OperationsAndBranchValue.Any(operation => operation.Syntax.ToString() == withSyntax)));
                Validate(block, withSyntax, expected);
            }

            public void Validate<TOperation>(string withSyntax, params Expected[] expected) where TOperation : IOperation
            {
                var block = Cfg.Blocks.Single(x => x.Kind == BasicBlockKind.Block && x.OperationsAndBranchValue.OfType<TOperation>().Any(operation => operation.Syntax.ToString() == withSyntax));
                Validate(block, withSyntax, expected);
            }

            public void Validate(BasicBlock block, string blockSuffix, params Expected[] expected)
            {
                // This is not very nice from OOP perspective, but it makes UTs above easy to read.
                var expectedLiveIn = expected.OfType<LiveIn>().SingleOrDefault() ?? new LiveIn();
                var expectedLiveOut = expected.OfType<LiveOut>().SingleOrDefault() ?? new LiveOut();
                var expectedCaptured = expected.OfType<Captured>().SingleOrDefault() ?? new Captured();
                Lva.LiveIn(block).Select(x => x.Name).Should().BeEquivalentTo(expectedLiveIn.Names, $"{block.Kind} #{block.Ordinal} {blockSuffix}");
                Lva.LiveOut(block).Select(x => x.Name).Should().BeEquivalentTo(expectedLiveOut.Names, $"{block.Kind} #{block.Ordinal} {blockSuffix}");
                Lva.CapturedVariables.Select(x => x.Name).Should().BeEquivalentTo(expectedCaptured.Names, $"{block.Kind} #{block.Ordinal} {blockSuffix}");
            }
        }

        private abstract class Expected
        {
            public readonly string[] Names;

            protected Expected(string[] names) =>
                Names = names;
        }

        private class LiveIn : Expected
        {
            public LiveIn(params string[] names) : base(names) { }
        }

        private class LiveOut : Expected
        {
            public LiveOut(params string[] names) : base(names) { }
        }

        private class Captured : Expected
        {
            public Captured(params string[] names) : base(names) { }
        }
    }
}

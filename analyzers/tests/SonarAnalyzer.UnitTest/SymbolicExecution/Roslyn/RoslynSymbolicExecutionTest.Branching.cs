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

using Microsoft.CodeAnalysis.Operations;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    public partial class RoslynSymbolicExecutionTest
    {
        [TestMethod]
        public void Branching_BlockProcessingOrder_CS()
        {
            const string code = @"
Tag(""Entry"");
if (Condition)
{
    Tag(""BeforeTry"");
    try
    {
        Tag(""InTry"");
    }
    catch
    {
        Tag(""InCatch"");
    }
    finally
    {
        Tag(""InFinally"");
    }
    Tag(""AfterFinally"");
}
else
{
    Tag(""Else"");
}
Tag(""End"");";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder(
                "Entry",
                "BeforeTry",        // Dequeue for "if" branch
                "Else",             // Dequeue for "else" branch
                "InTry",            // Dequeue after "if" branch
                "End",              // Dequeue after "else" branch, reaching exit block
                "InCatch",          // Dequeue after the "try body" that could throw
                "InFinally",        // Dequeue after the "try body"
                "InFinally",        // Dequeue after the "catch", with Exception
                "AfterFinally");    // Dequeue after "if" branch
        }

        [TestMethod]
        public void Branching_BlockProcessingOrder_VB()
        {
            const string code = @"
Tag(""Entry"")
If Condition Then
    Tag(""BeforeTry"")
    Try
        Tag(""InTry"")
    Catch
        Tag(""InCatch"")
    Finally
        Tag(""InFinally"")
    End Try
    Tag(""AfterFinally"")
Else
    Tag(""Else"")
End If
Tag(""End"")";
            SETestContext.CreateVB(code).Validator.ValidateTagOrder(
                "Entry",
                "BeforeTry",
                "Else",
                "InTry",
                "End",
                "InCatch",
                "InFinally",
                "InFinally",    // With Exception
                "AfterFinally");
        }

        [TestMethod]
        public void Branching_PersistSymbols_BetweenBlocks()
        {
            const string code = @"
var first = true;
var second = false;
if (boolParameter)
{
    Tag(""IfFirst"", first);
    Tag(""IfSecond"", second);
}
else
{
    Tag(""ElseFirst"", first);
    Tag(""ElseSecond"", second);
}";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IfFirst", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
            validator.ValidateTag("IfSecond", x => x.HasConstraint(BoolConstraint.False).Should().BeTrue());
            validator.ValidateTag("ElseFirst", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
            validator.ValidateTag("ElseSecond", x => x.HasConstraint(BoolConstraint.False).Should().BeTrue());
        }

        [TestMethod]
        public void EndNotifications_SimpleFlow()
        {
            var validator = SETestContext.CreateCS("var a = true;").Validator;
            validator.ValidateExitReachCount(1);
            validator.ValidateExecutionCompleted();
        }

        [TestMethod]
        public void EndNotifications_MaxStepCountReached()
        {
            // var x = true; produces 3 operations
            var code = Enumerable.Range(1, RoslynSymbolicExecution.MaxStepCount / 3 + 1).Select(x => $"var x{x} = true;").JoinStr(Environment.NewLine);
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateExitReachCount(0);
            validator.ValidateExecutionNotCompleted();
        }

        [TestMethod]
        public void EndNotifications_MultipleBranches()
        {
            const string method = @"
public int Method(bool a)
{
    if (a)
        return 1;
    else
        return 2;
}";
            var validator = SETestContext.CreateCSMethod(method).Validator;
            validator.ValidateExitReachCount(1);
            validator.ValidateExecutionCompleted();
        }

        [TestMethod]
        public void EndNotifications_Throw()
        {
            const string method = @"
public int Method(bool a)
{
    if (a)
        throw new System.NullReferenceException();
    else
        return 2;
}";
            var validator = SETestContext.CreateCSMethod(method).Validator;
            validator.ValidateExitReachCount(2);
            validator.ValidateExecutionCompleted();
            validator.ExitStates.Should().HaveCount(2)
                .And.ContainSingle(x => HasNoException(x))
                .And.ContainSingle(x => HasExceptionOfType(x, "NullReferenceException"));
        }

        [TestMethod]
        public void EndNotifications_YieldReturn()
        {
            const string method = @"
public System.Collections.Generic.IEnumerable<int> Method(bool a)
{
    if (a)
        yield return 1;

    yield return 2;
}";
            var validator = SETestContext.CreateCSMethod(method).Validator;
            validator.ValidateExitReachCount(1);
            validator.ValidateExecutionCompleted();
        }

        [TestMethod]
        public void EndNotifications_YieldBreak()
        {
            const string method = @"
public System.Collections.Generic.IEnumerable<int> Method(bool a)
{
    if (a)
        yield break;

    var b = a;
}";
            var validator = SETestContext.CreateCSMethod(method, new PreserveTestCheck("b")).Validator;
            validator.ValidateExitReachCount(2);
            validator.ValidateExecutionCompleted();
        }

        [TestMethod]
        public void Branching_ConstraintTrackedSeparatelyInBranches()
        {
            const string code = @"
bool value;
if (boolParameter)
{
    value = true;
}
else
{
    value = false;
}
Tag(""End"", value);";
            var validator = SETestContext.CreateCS(code, new PreserveTestCheck("value")).Validator;
            validator.ValidateExitReachCount(2); // Once with True constraint, once with False constraint on "value"
            validator.TagValues("End").Should().HaveCount(2)
                .And.ContainSingle(x => x.HasConstraint(BoolConstraint.True))
                .And.ContainSingle(x => x.HasConstraint(BoolConstraint.False));
        }

        [TestMethod]
        public void Branching_VisitedProgramState_IsSkipped()
        {
            const string code = @"
bool value;
if (Condition)
{
    value = true;
}
else
{
    value = true;
}
Tag(""End"", value);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateExitReachCount(1);
            validator.TagValues("End").Should().HaveCount(1).And.ContainSingle(x => x.HasConstraint(BoolConstraint.True));
        }

        [TestMethod]
        public void Branching_VisitedProgramState_IsImmutable()
        {
            const string code = @"
bool value;
if (boolParameter)
{
    value = true;
}
else
{
    value = false;
}
Tag(""End"", value);";
            var captured = new List<(SymbolicValue Value, bool ExpectedHasTrueConstraint)>();
            var postProcess = new PostProcessTestCheck(x =>
            {
                if (x.Operation.Instance.TrackedSymbol() is { } symbol && x.State[symbol] is { } value)
                {
                    captured.Add((value, value.HasConstraint(BoolConstraint.True)));
                }
                return x.State;
            });
            SETestContext.CreateCS(code, postProcess);
            captured.Should().OnlyContain(x => x.Value.HasConstraint(BoolConstraint.True) == x.ExpectedHasTrueConstraint);
        }

        [TestMethod]
        public void Branching_VisitedSymbolicValue_IsImmutable()
        {
            const string code = @"
var value = true;
if (boolParameter)
{
    value.ToString();
    Tag(""ToString"", value);
}
else
{
    value.GetHashCode();    // Another invocation to have same instruction count in both branches
    Tag(""GetHashCode"", value);
}";
            var postProcess = new PostProcessTestCheck(x =>
                x.Operation.Instance is IInvocationOperation { TargetMethod: { Name: "ToString" } } invocation
                    ? x.SetSymbolConstraint(invocation.Instance.TrackedSymbol(), TestConstraint.First)
                    : x.State);
            var validator = SETestContext.CreateCS(code, postProcess).Validator;
            validator.ValidateTag("ToString", x => x.HasConstraint(TestConstraint.First).Should().BeTrue());
            validator.ValidateTag("GetHashCode", x => x.HasConstraint(TestConstraint.First).Should().BeFalse()); // Nobody set the constraint on that path
            validator.ValidateExitReachCount(1);    // Once as the states are cleaned by LVA.
        }

        [TestMethod]
        public void Branching_TrueConstraint_VisitsIfBranch()
        {
            const string code = @"
var value = true;
if (value)
{
    Tag(""If"");
}
else
{
    Tag(""Else"");
}
Tag(""End"");";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder(
                "If",
                "End");
        }

        [TestMethod]
        public void Branching_TrueConstraintNegated_VisitsElseBranch()
        {
            const string code = @"
var value = true;
if (!value)
{
    Tag(""If"");
}
else
{
    Tag(""Else"");
}
Tag(""End"");";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder(
                "Else",
                "End");
        }

        [TestMethod]
        public void Branching_FalseConstraint_VisitsElseBranch()
        {
            const string code = @"
var value = false;
if (value)
{
    Tag(""If"");
}
else
{
    Tag(""Else"");
}
Tag(""End"");";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder(
                "Else",
                "End");
        }

        [TestMethod]
        public void Branching_FalseConstraintNegated_VisitsIfBranch()
        {
            const string code = @"
var value = false;
if (!value)
{
    Tag(""If"");
}
else
{
    Tag(""Else"");
}
Tag(""End"");";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder(
                "If",
                "End");
        }

        [TestMethod]
        public void Branching_NoConstraint_VisitsBothBranches()
        {
            const string code = @"
var value = boolParameter; // Unknown constraints
if (value)
{
    Tag(""If"");
}
else
{
    Tag(""Else"");
}
Tag(""End"");";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder(
                "If",
                "Else",
                "End");
        }

        [TestMethod]
        public void Branching_OtherConstraint_VisitsBothBranches()
        {
            const string code = @"
if (boolParameter)
{
    Tag(""If"");
}
else
{
    Tag(""Else"");
}
Tag(""End"");";
            var check = new PostProcessTestCheck(x => x.Operation.Instance.TrackedSymbol() is { } symbol ? x.SetSymbolConstraint(symbol, DummyConstraint.Dummy) : x.State);
            SETestContext.CreateCS(code, check).Validator.ValidateTagOrder(
                "If",
                "Else",
                "End");
        }

        [TestMethod]
        public void Branching_BoolConstraints_ComplexCase()
        {
            const string code = @"
var isTrue = true;
var isFalse = false;
if (isTrue && isTrue && !isFalse)
{
    if (isFalse || !isTrue)
    {
        Tag(""UnreachableIf"");
    }
    else if (isFalse)
    {
        Tag(""UnreachableElseIf"");
    }
    else
    {
        Tag(""Reachable"");
    }
}
else
{
    Tag(""UnreachableElse"");
}
Tag(""End"");";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder(
                "Reachable",
                "End");
        }

        [TestMethod]
        public void Branching_TrueLiteral_VisitsIfBranch()
        {
            const string code = @"
if (true)
{
    Tag(""If"");
}
else
{
    Tag(""Else"");
}
Tag(""End"");";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder(
                "If",
                "End");
        }

        [TestMethod]
        public void Branching_TrueConstraint_SwitchStatement_BinaryOperationNotSupported()
        {
            const string code = @"
var isTrue = true;
switch (isTrue)
{
    case true:
        Tag(""True"");
        break;
    case false:
        Tag(""False"");
        break;
    default:
        Tag(""Default"");
}
Tag(""End"");";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder(
                "True",
                "End");
        }

        [TestMethod]
        public void Branching_ConditionEvaluated()
        {
            const string code = @"
Tag(""Begin"");
if (boolParameter)
{
    Tag(""If"");
}
Tag(""End"");";
            var check = new ConditionEvaluatedTestCheck(x => x.State[x.Operation].HasConstraint(BoolConstraint.True) ? null : x.State);
            SETestContext.CreateCS(code, check).Validator.ValidateTagOrder("Begin", "End");
        }

        [TestMethod]
        public void Branching_NullConstraints_VisitsIfBranch()
        {
            const string code = @"
object value = null;
if (value == null)
{
    Tag(""If"");
}
else
{
    Tag(""Else"");
}
Tag(""End"");";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder(
                "If",
                "End");
        }

        [TestMethod]
        public void Branching_NotNullConstraints_VisitsElseBranch()
        {
            const string code = @"
var value = new Object();
if (value == null)
{
    Tag(""If"");
}
else
{
    Tag(""Else"");
}
Tag(""End"");";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder(
                "Else",
                "End");
        }

        [TestMethod]
        public void Branching_IsNullOrEmpty_SetsNotNullWhenFalse()
        {
            const string code = @"
if (string.IsNullOrEmpty(arg))
{
    Tag(""If"", arg);
}
else
{
    Tag(""Else"", arg);
}
Tag(""End"", arg);";
            var validator = SETestContext.CreateCS(code, ", string arg").Validator;
            validator.TagValues("If").Should().HaveCount(2)
                .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
                .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
            validator.ValidateTag("Else", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            // TODO validator.TagValues("End")
        }
    }
}

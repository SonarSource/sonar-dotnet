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

using Microsoft.CodeAnalysis.Operations;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.Test.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

public partial class RoslynSymbolicExecutionTest
{
    private static IEnumerable<object[]> StringIsNullOrEmptyMethods
    {
        get
        {
            yield return new object[] { nameof(string.IsNullOrEmpty) };
            yield return new object[] { nameof(string.IsNullOrWhiteSpace) };
        }
    }

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
        validator.TagValue("IfFirst").Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull);
        validator.TagValue("IfSecond").Should().HaveOnlyConstraints(BoolConstraint.False, ObjectConstraint.NotNull);
        validator.TagValue("ElseFirst").Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull);
        validator.TagValue("ElseSecond").Should().HaveOnlyConstraints(BoolConstraint.False, ObjectConstraint.NotNull);
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
    public void EndNotifications_ThrowFlowCaptureReference()
    {
        const string method = """
            public int Method(bool a)
            {
                throw a ? new System.NullReferenceException() : new System.NullReferenceException();
            }
            """;
        var validator = SETestContext.CreateCSMethod(method).Validator;
        validator.ValidateExitReachCount(2);
        validator.ValidateExecutionCompleted();
        validator.ExitStates.Should().SatisfyRespectively(
            x => HasExceptionOfType(x, "NullReferenceException"),
            x => HasExceptionOfType(x, "NullReferenceException"));
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
            if (x.Operation.Instance.TrackedSymbol(x.State) is { } symbol && x.State[symbol] is { } value)
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
            x.Operation.Instance is IInvocationOperation { TargetMethod.Name: "ToString" } invocation
                ? x.SetSymbolConstraint(invocation.Instance.TrackedSymbol(x.State), TestConstraint.First)
                : x.State);
        var validator = SETestContext.CreateCS(code, postProcess).Validator;
        validator.TagValue("ToString").Should().HaveOnlyConstraints(TestConstraint.First, BoolConstraint.True, ObjectConstraint.NotNull);
        validator.TagValue("GetHashCode").Should().HaveOnlyConstraints(new SymbolicConstraint[] { ObjectConstraint.NotNull, BoolConstraint.True }, "nobody set the TestConstraint.First on that path");
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
        var check = new PostProcessTestCheck(x => x.Operation.Instance.TrackedSymbol(x.State) is { } symbol ? x.SetSymbolConstraint(symbol, DummyConstraint.Dummy) : x.State);
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
    public void Branching_ConditionEvaluated_IsInLoop()
    {
        const string code = """
            Tag("Begin");
            while (boolParameter)
            {
                Tag("While");
            }
            Tag("End");
            """;
        var check = new ConditionEvaluatedTestCheck(x => { x.IsInLoop.Should().BeTrue(); return x.State; });
        SETestContext.CreateCS(code, check).Validator.ValidateTagOrder("Begin", "While", "End");
    }

    [TestMethod]
    public void Branching_ConditionEvaluated_IsNotInLoop()
    {
        const string code = """
            Tag("Begin");
            if (boolParameter)
            {
                Tag("If");
            }
            Tag("End");
            """;
        var check = new ConditionEvaluatedTestCheck(x => { x.IsInLoop.Should().BeFalse(); return x.State; });
        SETestContext.CreateCS(code, check).Validator.ValidateTagOrder("Begin", "If", "End");
    }

    [TestMethod]
    public void Branching_Rethrow_CallsConditionEvaluated()
    {
        const string code = """
            try
            {
                Console.WriteLine("may throw");
            }
            catch
            {
                if (boolParameter)
                    throw;
            }
            """;
        var count = 0;
        var check = new ConditionEvaluatedTestCheck(x =>
            {
                count++;
                return x.State;
            });
        SETestContext.CreateCS(code, check).Validator.ValidateTagOrder();
        count.Should().Be(2);
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

    [DataTestMethod]
    [DynamicData(nameof(StringIsNullOrEmptyMethods))]
    public void Branching_IsNullOrEmpty_SetsNullConstraintsInBranches(string methodName)
    {
        var code = $@"
if (string.{methodName}(arg))
{{
    Tag(""If"", arg);
}}
else
{{
    Tag(""Else"", arg);
}}
Tag(""End"", arg);";
        var validator = SETestContext.CreateCS(code, "string arg").Validator;
        validator.TagValues("If").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
        validator.TagValue("Else").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DynamicData(nameof(StringIsNullOrEmptyMethods))]
    public void Branching_IsNullOrEmpty_VisitsTrueBranchIfNull(string methodName)
    {
        var code = $@"
string isNull = null;
if (string.{methodName}(isNull))
{{
    Tag(""If"", isNull);
}}
else
{{
    Tag(""Else"", isNull);
}}
Tag(""End"", isNull);";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("If").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagValues("Else").Should().BeEmpty();
        validator.TagValue("End").Should().HaveOnlyConstraint(ObjectConstraint.Null);
    }

    [DataTestMethod]
    [DynamicData(nameof(StringIsNullOrEmptyMethods))]
    public void Branching_IsNullOrEmpty_VisitsBothBranchesIfNotNull(string methodName)
    {
        var code = @$"
string notNull = ""Some NotNull text"";
if (string.{methodName}(notNull))
{{
    Tag(""If"", notNull);
}}
else
{{
    Tag(""Else"", notNull);
}}
Tag(""End"", notNull);";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("If").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Else").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("End").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }
}

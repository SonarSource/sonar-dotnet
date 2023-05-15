﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn;

public partial class RoslynSymbolicExecutionTest
{
    [DataTestMethod]
    [DataRow("for (var i = 0; i < items.Length; i++)")]
    [DataRow("while (Condition)")]
    public void Loops_InstructionVisitedMaxTwice(string loop)
    {
        var code = $$"""
            {{loop}}
            {
                arg.ToString(); // Add another constraint to 'arg'
            }
            Tag("End", arg);
            """;
        var validator = SETestContext.CreateCS(code, "int arg, int[] items", new AddConstraintOnInvocationCheck(), new PreserveTestCheck("arg")).Validator;
        validator.ValidateExitReachCount(2);    // PreserveTestCheck is needed for this, otherwise, variables are thrown away by LVA when going to the Exit block
        validator.TagValues("End").Should().HaveCount(2)
            .And.SatisfyRespectively(
                x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull),
                x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First));
    }

    [DataTestMethod]
    [DataRow("for (var i = 0; i < 10; i++)")]
    [DataRow("for (var i = 0; i < 10; ++i)")]
    [DataRow("for (var i = 10; i > 0; i--)")]
    [DataRow("for (var i = 10; i > 0; --i)")]
    public void Loops_InstructionVisitedMaxTwice_For_FixedCount(string loop)
    {
        var code = $$"""
            {{loop}}
            {
                arg.ToString(); // Add another constraint to 'arg'
            }
            Tag("End", arg);
            """;
        var validator = SETestContext.CreateCS(code, "int arg", new AddConstraintOnInvocationCheck()).Validator;
        validator.ValidateExitReachCount(0);    // For now, we don't explore states after fixed loops
    }

    [TestMethod]
    public void Loops_For_ComplexCondition_MultipleVariables()
    {
        const string code = """
            for (int i = 0, j = 10; i < 10 && j > 0; i++, j++)
            {
                arg.ToString(); // Add another constraint to 'arg'
            }
            Tag("End", arg);
            """;
        var validator = SETestContext.CreateCS(code, "int arg", new AddConstraintOnInvocationCheck()).Validator;
        validator.ValidateExitReachCount(1);
        validator.ValidateTag("End", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First));  // arg has only it's final constraints after looping
    }

    [TestMethod]
    public void Loops_For_ComplexCondition_AlwaysTrue()
    {
        const string code = """
            boolParameter = true;
            for (var i = 0; i < 10 || boolParameter; i++)
            {
                arg.ToString(); // Add another constraint to 'arg'
                Tag("Arg", arg);
            }
            Tag("Unreachable", arg);
            """;
        var validator = SETestContext.CreateCS(code, "int arg", new AddConstraintOnInvocationCheck()).Validator;
        validator.ValidateExitReachCount(0);
        validator.ValidateTagOrder("Arg", "Arg");
        validator.TagValues("Arg").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True));
    }

    [TestMethod]
    public void Loops_For_ComplexCondition_AlwaysFalse()
    {
        const string code = """
            boolParameter = false;
            for (var i = 0; i < 10 && boolParameter; i++)
            {
                arg.ToString(); // Add another constraint to 'arg'
                Tag("Unreachable");
            }
            Tag("End", arg);
            """;
        var validator = SETestContext.CreateCS(code, "int arg", new AddConstraintOnInvocationCheck()).Validator;
        validator.ValidateExitReachCount(1);
        validator.ValidateTagOrder("End");
        validator.ValidateTag("End", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull));    // AddConstraintOnInvocationCheck didn't add anything
    }

    [TestMethod]
    public void Loops_InstructionVisitedMaxTwice_ForEach()
    {
        const string code = @"
foreach (var i in items)
{{
    arg.ToString(); // Add another constraint to 'arg'
    arg -= 1;
}}
Tag(""End"", arg);";
        var validator = SETestContext.CreateCS(code, "int arg, int[] items", new AddConstraintOnInvocationCheck()).Validator;
        // In the case of foreach, there are implicit method calls that in the current implementation can throw:
        // - IEnumerable<>.GetEnumerator()
        // - System.Collections.IEnumerator.MoveNext()
        // - System.IDisposable.Dispose()
        validator.ValidateExitReachCount(12);                       // foreach produces implicit TryFinally region where it can throw - these flows do not reach the Tag("End") line
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull),  // items with Null flow generated by implicty Finally block that has items?.Dispose()
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull),  // items with NotNull flow
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First));
    }

    [TestMethod]
    public void Loops_InstructionVisitedMaxTwice_EvenWithMultipleStates()
    {
        const string code = @"
bool condition;
if (Condition)      // This generates two different ProgramStates, each tracks its own visits
    condition = true;
else
    condition = false;
do
{
    arg.ToString(); // Add another constraint to 'arg'
} while (Condition);
Tag(""End"", arg);";
        var validator = SETestContext.CreateCS(code, "int arg, int[] items", new AddConstraintOnInvocationCheck(), new PreserveTestCheck("condition", "arg")).Validator;
        validator.ValidateExitReachCount(4);    // PreserveTestCheck is needed for this, otherwise, variables are thrown away by LVA when going to the Exit block
        var states = validator.TagStates("End");
        var condition = validator.Symbol("condition");
        var arg = validator.Symbol("arg");
        states.Should().HaveCount(4)
            .And.ContainSingle(x => x[condition].HasConstraint(BoolConstraint.True) && x[arg].HasConstraint(TestConstraint.First) && !x[arg].HasConstraint(BoolConstraint.True))
            .And.ContainSingle(x => x[condition].HasConstraint(BoolConstraint.True) && x[arg].HasConstraint(TestConstraint.First) && x[arg].HasConstraint(BoolConstraint.True))
            .And.ContainSingle(x => x[condition].HasConstraint(BoolConstraint.False) && x[arg].HasConstraint(TestConstraint.First) && !x[arg].HasConstraint(BoolConstraint.True))
            .And.ContainSingle(x => x[condition].HasConstraint(BoolConstraint.False) && x[arg].HasConstraint(TestConstraint.First) && x[arg].HasConstraint(BoolConstraint.True));
    }

    [TestMethod]
    public void DoLoop_InstructionVisitedMaxTwice()
    {
        var code = $@"
do
{{
    arg.ToString(); // Add another constraint to 'arg'
    arg -= 1;
}} while (arg > 0);
Tag(""End"", arg);";
        var validator = SETestContext.CreateCS(code, "int arg", new AddConstraintOnInvocationCheck()).Validator;
        validator.ValidateExitReachCount(1);
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(TestConstraint.First) && !x.HasConstraint(BoolConstraint.True))
            .And.ContainSingle(x => x.HasConstraint(TestConstraint.First) && x.HasConstraint(BoolConstraint.True) && !x.HasConstraint(DummyConstraint.Dummy));
    }

    [DataTestMethod]
    [DataRow("for( ; ; )")]
    [DataRow("while (true)")]
    public void InfiniteLoopWithNoExitBranch_InstructionVisitedMaxTwice(string loop)
    {
        var code = @$"
{loop}
{{
    arg.ToString(); // Add another constraint to 'arg'
    Tag(""Inside"", arg);
}}";
        var validator = SETestContext.CreateCS(code, "int arg", new AddConstraintOnInvocationCheck()).Validator;
        validator.ValidateExitReachCount(0);                    // There's no branch to 'Exit' block, or the exist is never reached
        validator.TagValues("Inside").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(TestConstraint.First) && !x.HasConstraint(BoolConstraint.True))
            .And.ContainSingle(x => x.HasConstraint(TestConstraint.First) && x.HasConstraint(BoolConstraint.True) && !x.HasConstraint(DummyConstraint.Dummy));
    }

    [TestMethod]
    public void GoTo_InfiniteWithNoExitBranch_InstructionVisitedMaxTwice()
    {
        const string code = @"
Start:
arg.ToString(); // Add another constraint to 'arg'
Tag(""Inside"", arg);
goto Start;";
        var validator = SETestContext.CreateCS(code, "int arg", new AddConstraintOnInvocationCheck()).Validator;
        validator.ValidateExitReachCount(0);                    // There's no branch to 'Exit' block
        validator.TagValues("Inside").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(TestConstraint.First) && !x.HasConstraint(BoolConstraint.True))
            .And.ContainSingle(x => x.HasConstraint(TestConstraint.First) && x.HasConstraint(BoolConstraint.True) && !x.HasConstraint(DummyConstraint.Dummy));
    }

    [TestMethod]
    public void GoTo_InstructionVisitedMaxTwice()
    {
        const string code = @"
Start:
arg.ToString(); // Add another constraint to 'arg'
arg -= 1;
if (arg > 0)
{
    goto Start;
}
Tag(""End"", arg);";
        var validator = SETestContext.CreateCS(code, "int arg", new AddConstraintOnInvocationCheck()).Validator;
        validator.ValidateExitReachCount(1);
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(TestConstraint.First) && !x.HasConstraint(BoolConstraint.True))
            .And.ContainSingle(x => x.HasConstraint(TestConstraint.First) && x.HasConstraint(BoolConstraint.True) && !x.HasConstraint(DummyConstraint.Dummy));
    }

    [DataTestMethod]
    [DataRow("for (var i = 0; condition; i++)")]
    [DataRow("while (condition)")]
    public void Loops_FalseConditionNotExecuted(string loop)
    {
        var code = $@"
var condition = false;
{loop}
{{
    Tag(""Loop"");
}}
Tag(""End"");";
        SETestContext.CreateCS(code).Validator.ValidateTagOrder("End");
    }

    [TestMethod]
    public void DoWhileLoopWithTryCatchAndNullFlows()
    {
        var code = @"
Exception lastEx = null;
do
{
    try
    {
        InstanceMethod(); // May throw
        Tag(""BeforeReturn"", lastEx);
        return;
    }
    catch (InvalidOperationException e)
    {
        lastEx = e;
        Tag(""InCatch"", lastEx);
    }
} while(boolParameter);

Tag(""End"", lastEx);
";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder("BeforeReturn", "InCatch", "End", "BeforeReturn", "InCatch");
        validator.TagValues("BeforeReturn").Should().SatisfyRespectively(
            x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue(),                              // InstanceMethod did not throw
            x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());                          // InstanceMethod did throw, was caught, and flow continued
        validator.ValidateTag("End", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue()); // InstanceMethod did throw and was caught
    }
}

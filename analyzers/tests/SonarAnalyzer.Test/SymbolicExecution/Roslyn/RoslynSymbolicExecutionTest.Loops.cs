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

using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.Test.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

public partial class RoslynSymbolicExecutionTest
{
    [DataTestMethod]
    [DataRow("for (var i = 0; i < items.Length; i++)")]
    [DataRow("while (Condition)")]
    public void Loops_BodyVisitedMaxTwice(string loop)
    {
        var code = $$"""
            {{loop}}
            {
                arg.ToString(); // Add another constraint to 'arg'
                Tag("InLoop", arg);
            }
            Tag("End", arg);
            """;
        var validator = SETestContext.CreateCS(code, "int arg, int[] items", new AddConstraintOnInvocationCheck(), new PreserveTestCheck("arg")).Validator;
        validator.ValidateExitReachCount(3);    // PreserveTestCheck is needed for this, otherwise, variables are thrown away by LVA when going to the Exit block
        validator.TagValues("InLoop").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True));
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True));
    }

    [DataTestMethod]
    [DataRow("for (var i = 0; i < 10; i++)")]
    [DataRow("for (var i = 0; i < 10; ++i)")]
    [DataRow("for (var i = 10; i > 0; i--)")]
    [DataRow("for (var i = 10; i > 0; --i)")]
    public void Loops_BodyVisitedMaxTwice_For_FixedCount(string loop)
    {
        var code = $$"""
            {{loop}}
            {
                arg.ToString(); // Add another constraint to 'arg'
                Tag("InLoop", arg);
            }
            Tag("End", arg);
            """;
        var validator = SETestContext.CreateCS(code, "int arg", new AddConstraintOnInvocationCheck(), new PreserveTestCheck("arg")).Validator;
        validator.ValidateExitReachCount(2);    // PreserveTestCheck is needed for this, otherwise, variables are thrown away by LVA when going to the Exit block
        validator.TagValues("InLoop").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True));
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True));
    }

    [TestMethod]
    public void Loops_BodyVisitedMaxTwice_Do_While()
    {
        var code = """
            do
            {
                arg.ToString(); // Add another constraint to 'arg'
                Tag("InLoop", arg);
            }
            while (Condition);
            Tag("End", arg);
            """;
        var validator = SETestContext.CreateCS(code, "int arg", new AddConstraintOnInvocationCheck(), new PreserveTestCheck("arg")).Validator;
        validator.ValidateExitReachCount(2);    // PreserveTestCheck is needed for this, otherwise, variables are thrown away by LVA when going to the Exit block
        validator.TagValues("InLoop").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True));
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True));
    }

    [TestMethod]
    public void Loops_Infinite_ConditionVisitedMaxTreeTimes()
    {
        var code = """
            while (i.Equals(1))
            {
            }
            Tag("End", i);
            """;
        var validator = SETestContext.CreateCS(code, "int i", new AddConstraintOnInvocationCheck(), new PreserveTestCheck("i")).Validator;
        validator.ValidateExitReachCount(3);
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy));
    }

    [DataTestMethod]
    [DataRow("i < 10")]
    [DataRow("!(i >= 10)")]
    public void Loops_BodyVisitedMaxTwice_For_FixedCount_Expanded(string condition)
    {
        var code = $$"""
            for (var i = 0; {{condition}}; i++)
            {
                Tag("Inside", i);
            }
            Tag("End");
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValues("Inside").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(0)),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(1, 9)));
        validator.TagStates("End").Should().SatisfyRespectively(    // We can assert because LVA did not kick in yet
            x => x[validator.Symbol("i")].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(10, null)),
            x => x[validator.Symbol("i")].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(10)));
    }

    [TestMethod]
    public void Loops_BodyVisitedMaxTwice_For_FixedCount_NestedNumberCondition_CS()
    {
        const string code = """
            for (var i = 0; i < 10; i++)
            {
                int value = 42;
                if (value < 100)  // Should be always true
                {
                    Tag("If", value);
                }
                else
                {
                    Tag("Unreachable");
                }
            }
            """;
        var validator = SETestContext.CreateCS(code, "int arg", new AddConstraintOnInvocationCheck()).Validator;
        validator.ValidateExitReachCount(2);
        var i = validator.Symbol("i");
        var value = validator.Symbol("value");
        validator.TagStates("If").Should().SatisfyRespectively(
            x =>
            {
                x[i].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(0));
                x[value].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(42));
            },
            x =>
            {
                x[i].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(1, 9));
                x[value].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(42));
            });
        validator.TagStates("Unreachable").Should().BeEmpty();
    }

    [TestMethod]
    public void Loops_BodyVisitedMaxTwice_For_FixedCount_NestedNumberCondition_VB()
    {
        const string code = """
            For i As Integer = 0 To 9
                Dim Value As Integer = 42
                If Value < 100 Then ' Should Then be always True
                    Tag("If", Value)
                Else
                    Tag("Unreachable")
                End If
            Next
            """;
        var validator = SETestContext.CreateVB(code, "Arg As Integer", new AddConstraintOnInvocationCheck()).Validator;
        validator.ValidateExitReachCount(2);
        var i = validator.Symbol("i");
        var value = validator.Symbol("Value");
        validator.TagStates("If").Should().SatisfyRespectively(
            x =>
            {
                x[i].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(0));
                x[value].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(42));
            },
            x =>
            {
                x[i].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(1, 9));
                x[value].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(42));
            });
        validator.TagStates("Unreachable").Should().BeEmpty();
    }

    [TestMethod]
    public void Loops_For_ComplexCondition_MultipleVariables()
    {
        const string code = """
            for (int i = 0, j = 10; i < 10 && j > 0; i++, j--)
            {
                arg.ToString(); // Add another constraint to 'arg'
            }
            Tag("End");
            """;
        var validator = SETestContext.CreateCS(code, "int arg", new AddConstraintOnInvocationCheck()).Validator;
        var arg = validator.Symbol("arg");
        var i = validator.Symbol("i");
        var j = validator.Symbol("j");
        validator.ValidateExitReachCount(1);
        validator.TagStates("End").Should().SatisfyRespectively(
            x =>
            {
                x[i].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(10, null));
                x[j].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(9));
                x[arg].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First);
            },
            x =>
            {
                x[i].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(1, 9));
                x[j].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(null, 0));
                x[arg].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First);
            },
            x =>
            {
                x[i].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(10));
                x[j].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(0, 8));
                x[arg].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True);
            },
            x =>
            {
                x[i].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(2, 9));
                x[j].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(0));
                x[arg].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True);
            });
    }

    [TestMethod]
    public void Loops_For_ComplexCondition_AlwaysTrue()
    {
        const string code = """
            boolParameter = true;
            for (var i = 0; i < 10 || boolParameter; i++)
            {
                arg.ToString(); // Add another constraint to 'arg'
                Tag("InLoop");
            }
            Tag("Unreachable");
            """;
        var validator = SETestContext.CreateCS(code, "int arg", new AddConstraintOnInvocationCheck()).Validator;
        var arg = validator.Symbol("arg");
        var i = validator.Symbol("i");
        validator.ValidateExitReachCount(0);
        validator.ValidateTagOrder("InLoop", "InLoop", "InLoop");
        validator.TagStates("InLoop").Should().SatisfyRespectively(
            x =>
            {
                x[i].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(0));
                x[arg].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First);
            },
            x =>
            {
                x[i].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(1, 9));
                x[arg].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True);
            },
            x =>
            {
                x[i].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(10, null));
                x[arg].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True);
            });
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
        validator.TagStates("End").Should().SatisfyRespectively(
            x =>
            {
                x[validator.Symbol("i")].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(0));  // We can assert because LVA did not kick in yet
                x[validator.Symbol("arg")].Should().HaveOnlyConstraints(ObjectConstraint.NotNull);                          // AddConstraintOnInvocationCheck didn't add anything
            });
    }

    [TestMethod]
    public void Loops_While_FixedCount()
    {
        const string code = """
            var i = 0;
            while(i < 10)   // Same as: for(var i=0; i < 10; i++)
            {
                arg.ToString(); // Add another constraint to 'arg'
                i++;
                Tag("Inside", i);
            }
            Tag("After", i);
            Tag("End", arg);
            """;
        var validator = SETestContext.CreateCS(code, "int arg", new AddConstraintOnInvocationCheck()).Validator;
        validator.ValidateExitReachCount(1);
        validator.TagValues("Inside").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(1)),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(2, 10)));
        validator.TagValues("After").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(10, null)),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(10)));
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True));
    }

    [TestMethod]
    public void Loops_While_NestedNumberCondition_CS()
    {
        const string code = """
            var i = 0;
            while (Condition)   // We are inside a loop => binary operations are evaluated to true/false for 1st pass, and learn range condition for 2nd pass
            {
                if (i < 10)
                {
                    Tag("Inside", i);
                    i++;
                }
                else
                {
                    Tag("Unreachable");
                }
                Tag("After", i);
            }
            """;
        var validator = SETestContext.CreateCS(code, new AddConstraintOnInvocationCheck()).Validator;
        validator.ValidateExitReachCount(3);
        validator.ValidateTagOrder("Inside", "After", "Inside", "After");
        validator.TagValues("Inside").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(0)),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(1)));
        validator.TagValues("After").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(1)),    // Initial pass through "if"
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(2)));   // Second pass through "if"
    }

    [TestMethod]
    public void Loops_While_NestedNumberCondition_VB()
    {
        const string code = """
            Dim i As Integer
            While Condition     ' We are inside a Loop => binary operations are evaluated To True/False For 1St pass, And learn range condition For 2nd pass
                If i < 10 Then
                    Tag("Inside", i)
                    i = i + 1
                Else
                    Tag("Unreachable")
                End If
                Tag("After", i)
            End While
            """;
        var validator = SETestContext.CreateVB(code, new AddConstraintOnInvocationCheck()).Validator;
        validator.ValidateExitReachCount(3);
        validator.ValidateTagOrder("Inside", "After", "Inside", "After");
        validator.TagValues("Inside").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(0)),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(1)));
        validator.TagValues("After").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(1)),    // Initial pass through "if"
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(2)));   // Second pass through "if"
    }

    [TestMethod]
    public void Loops_BodyVisitedMaxTwice_ForEach()
    {
        const string code = @"
foreach (var i in items)
{{
    arg.ToString(); // Add another constraint to 'arg'
    arg--;
}}
Tag(""End"", arg);";
        var validator = SETestContext.CreateCS(code, "int arg, int[] items", new AddConstraintOnInvocationCheck()).Validator;
        // In the case of foreach, there are implicit method calls that in the current implementation can throw:
        // - IEnumerable<>.GetEnumerator()
        // - System.Collections.IEnumerator.MoveNext()
        // - System.IDisposable.Dispose()
        validator.ValidateExitReachCount(14);                       // foreach produces implicit TryFinally region where it can throw - these flows do not reach the Tag("End") line
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull),  // items with Null flow generated by implicty Finally block that has items?.Dispose()
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull),  // items with NotNull flow
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True));
    }

    [TestMethod]
    public void Loops_BodyVisitedMaxTwice_ForEach_VariableSyntax()
    {
        const string code = """
            foreach (var (i, j) in new (int, int)[5])
            {{
                arg.ToString(); // Add another constraint to 'arg'
                arg--;
            }}
            Tag("End", arg);
            """;
        var validator = SETestContext.CreateCS(code, "int arg, int[] items", new AddConstraintOnInvocationCheck()).Validator;
        // In the case of foreach, there are implicit method calls that in the current implementation can throw:
        // - IEnumerable<>.GetEnumerator()
        // - System.Collections.IEnumerator.MoveNext()
        // - System.IDisposable.Dispose()
        validator.ValidateExitReachCount(14);                       // foreach produces implicit TryFinally region where it can throw - these flows do not reach the Tag("End") line
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull),  // items with Null flow generated by implicty Finally block that has items?.Dispose()
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull),  // items with NotNull flow
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True));
    }

    [TestMethod]
    public void Loops_BodyVisitedMaxTwice_EvenWithMultipleStates()
    {
        const string code = """
            bool condition;
            if (Condition)      // This generates two different ProgramStates, each tracks its own visits
                condition = true;
            else
                condition = false;
            do
            {
                arg.ToString(); // Add another constraint to 'arg'
            } while (Condition);
            Tag("End", arg);
            """;
        var validator = SETestContext.CreateCS(code, "int arg, int[] items", new AddConstraintOnInvocationCheck(), new PreserveTestCheck("condition", "arg")).Validator;
        validator.ValidateExitReachCount(4);    // PreserveTestCheck is needed for this, otherwise, variables are thrown away by LVA when going to the Exit block
        var states = validator.TagStates("End");
        var condition = validator.Symbol("condition");
        var arg = validator.Symbol("arg");
        states.Should().SatisfyRespectively(
            x =>
            {
                x[condition].Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull);
                x[arg].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First);
            },
            x =>
            {
                x[condition].Should().HaveOnlyConstraints(BoolConstraint.False, ObjectConstraint.NotNull);
                x[arg].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First);
            },
            x =>
            {
                x[condition].Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull);
                x[arg].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True);
            },
            x =>
            {
                x[condition].Should().HaveOnlyConstraints(BoolConstraint.False, ObjectConstraint.NotNull);
                x[arg].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True);
            });
    }

    [TestMethod]
    public void DoLoop_BodyVisitedMaxTwice()
    {
        var code = """
            do
            {
                arg.ToString(); // Add another constraint to 'arg'
                arg--;
            } while (arg > 0);
            Tag("End", arg);
            """;
        var validator = SETestContext.CreateCS(code, "int arg", new AddConstraintOnInvocationCheck()).Validator;
        validator.ValidateExitReachCount(1);
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(TestConstraint.First, ObjectConstraint.NotNull, NumberConstraint.From(null, 0)),
            x => x.Should().HaveOnlyConstraints(TestConstraint.First, ObjectConstraint.NotNull, BoolConstraint.True, NumberConstraint.From(0)));
    }

    [DataTestMethod]
    [DataRow("for( ; ; )")]
    [DataRow("while (true)")]
    public void InfiniteLoopWithNoExitBranch_BodyVisitedMaxTwice(string loop)
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
arg--;
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

    [TestMethod]
    public void GoTo_FixedCount()
    {
        const string code = """
            var i = 0;
            Start:
            if (i < 10)
            {
                Tag("InLoop", i);
                i++;
                goto Start;
            }
            Tag("End");
            """;
        var validator = SETestContext.CreateCS(code, new AddConstraintOnInvocationCheck()).Validator;
        validator.ValidateExitReachCount(0);    // We don't get out of the loop
        validator.ValidateTagOrder("InLoop", "InLoop");
        validator.TagValues("InLoop").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(0)),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(1)));
        validator.TagStates("End").Should().BeEmpty();
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
        validator.TagValue("End").Should().HaveOnlyConstraint(ObjectConstraint.NotNull); // InstanceMethod did throw and was caught
    }
}

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
    [TestMethod]
    public void Loops_IsInLoop_For()
    {
        const string code = """
            Console.WriteLine("Before loop");
            while (Condition)
            {
                _ = "Inside Loop";
            }
            Console.WriteLine("After loop");
            """;
        var inLoopCheck = new PreProcessTestCheck(OperationKind.SimpleAssignment, x => { x.IsInLoop.Should().BeTrue(); return x.State; });
        var notInLoopCheck = new PreProcessTestCheck(OperationKind.Invocation, x => { x.IsInLoop.Should().BeFalse(); return x.State; });
        SETestContext.CreateCS(code, inLoopCheck, notInLoopCheck);
    }

    [DataTestMethod]
    [DataRow("for (var i = 0; i < items.Length; i++)")]
    [DataRow("while (Condition)")]
    public void Loops_BodyVisitedMaxThreeTimes(string loop)
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
        validator.ValidateExitReachCount(4);    // PreserveTestCheck is needed for this, otherwise, variables are thrown away by LVA when going to the Exit block
        validator.TagValues("InLoop").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy));
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy));
    }

    [DataTestMethod]
    [DataRow("for (var i = 0; i < 10; i++)")]
    [DataRow("for (var i = 0; i < 10; ++i)")]
    [DataRow("for (var i = 10; i > 0; i--)")]
    [DataRow("for (var i = 10; i > 0; --i)")]
    public void Loops_BodyVisitedMaxThreeTimes_For_FixedCount(string loop)
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
        validator.ValidateExitReachCount(3);    // PreserveTestCheck is needed for this, otherwise, variables are thrown away by LVA when going to the Exit block
        validator.TagValues("InLoop").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy));
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy));
    }

    [TestMethod]
    public void Loops_BodyVisitedMaxThreeTimes_Do_While()
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
        validator.ValidateExitReachCount(3);    // PreserveTestCheck is needed for this, otherwise, variables are thrown away by LVA when going to the Exit block
        validator.TagValues("InLoop").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy));
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy));
    }

    [TestMethod]
    public void Loops_Infinite_ConditionVisitedMaxFourTimes()
    {
        var code = """
            while (i.Equals(1))
            {
            }
            Tag("End", i);
            """;
        var validator = SETestContext.CreateCS(code, "int i", new AddConstraintOnInvocationCheck(), new PreserveTestCheck("i")).Validator;
        validator.ValidateExitReachCount(4);
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy, LockConstraint.Held));
    }

    [DataTestMethod]
    [DataRow("i < 10")]
    [DataRow("!(i >= 10)")]
    public void Loops_BodyVisitedMaxThreeTimes_For_FixedCount_Expanded(string condition)
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
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(1, 9)),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(2, 9)));
        validator.TagStates("End").Should().SatisfyRespectively(    // We can assert because LVA did not kick in yet
            x => x[validator.Symbol("i")].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(10, null)));
    }

    [TestMethod]
    public void Loops_BodyVisitedMaxThreeTimes_For_FixedCount_NestedNumberCondition_CS()
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
        validator.ValidateExitReachCount(1);
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
            },
            x =>
            {
                x[i].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(2, 9));
                x[value].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(42));
            });
        validator.TagStates("Unreachable").Should().BeEmpty();
    }

    [TestMethod]
    public void Loops_BodyVisitedMaxThreeTimes_For_FixedCount_NestedNumberCondition_VB()
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
        validator.ValidateExitReachCount(1);
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
            },
            x =>
            {
                x[i].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(2, 9));
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
                x[j].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(null, 9));
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
                x[i].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(10, null));
                x[j].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(null, 8));
                x[arg].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True);
            },
            x =>
            {
                x[i].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(2, 9));
                x[j].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(null, 0));
                x[arg].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True);
            },
            x =>
            {
                x[i].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(10, null));
                x[j].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(null, 7));
                x[arg].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy);
            },
            x =>
            {
                x[i].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(3, 9));
                x[j].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(null, 0));
                x[arg].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy);
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
        validator.ValidateTagOrder("InLoop", "InLoop", "InLoop", "InLoop", "InLoop", "InLoop");
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
            },
            x =>
            {
                x[i].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(2, 9));
                x[arg].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy);
            },
            x =>
            {
                x[i].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(10, null));
                x[arg].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy);
            },
            x =>
            {
                x[i].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(11, null));
                x[arg].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy);
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
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(1, null)),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(2, null)),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(3, null)));
        validator.TagValues("After").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(10, null)),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(10, null)),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(10, null)));
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy));
    }

    [TestMethod]
    public void Loops_While_NestedNumberCondition_CS()
    {
        const string code = """
            var i = 0;
            while (Condition)
            {
                if (i < 10)
                {
                    Tag("If", i);
                    i++;
                }
                else
                {
                    Tag("Else", i);
                }
                Tag("After", i);
            }
            """;
        var validator = SETestContext.CreateCS(code, new AddConstraintOnInvocationCheck()).Validator;
        validator.ValidateExitReachCount(5);
        validator.ValidateTagOrder("If", "After", "If", "Else", "After", "After", "If", "After");
        validator.TagValues("If").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(0)),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(1, 9)),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(2, 9)));
        validator.TagValue("Else").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(10, null));
        validator.TagValues("After").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(1, null)),  // Initial pass through "if"
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(10, null)), // Second and third pass through "if", false branch
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(2, null)),  // Second pass through "if", true branch
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(3, null))); // Third pass through "if", true branch
    }

    [TestMethod]
    public void Loops_While_NestedNumberCondition_VB()
    {
        const string code = """
            Dim i As Integer
            While Condition
                If i < 10 Then
                    Tag("If", i)
                    i = i + 1
                Else
                    Tag("Else", i)
                End If
                Tag("After", i)
            End While
            """;
        var validator = SETestContext.CreateVB(code, new AddConstraintOnInvocationCheck()).Validator;
        validator.ValidateExitReachCount(5);
        validator.ValidateTagOrder("If", "After", "If", "Else", "After", "After", "If", "After");
        validator.TagValues("If").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(0)),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(1, 9)),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(2, 9)));
        validator.TagValue("Else").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(10, null));
        validator.TagValues("After").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(1, null)),  // Initial pass through "if"
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(10, null)), // Second and third pass through "if", false branch
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(2, null)),  // Second pass through "if", true branch
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(3, null))); // Third pass through "if", true branch
    }

    [TestMethod]
    public void Loops_BodyVisitedMaxThreeTimes_ForEach()
    {
        const string code = """
            foreach (var i in items)
            {
                arg.ToString(); // Add another constraint to 'arg'
                arg--;
            }
            Tag("End", arg);
            """;
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
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy));
    }

    [TestMethod]
    public void Loops_BodyVisitedMaxThreeTimes_ForEach_VariableSyntax()
    {
        const string code = """
            foreach (var (i, j) in new (int, int)[5])
            {
                arg.ToString(); // Add another constraint to 'arg'
                arg--;
            }
            Tag("End", arg);
            """;
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
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy));
    }

    [TestMethod]
    public void Loops_BodyVisitedMaxThreeTimes_EvenWithMultipleStates()
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
        validator.ValidateExitReachCount(6);    // PreserveTestCheck is needed for this, otherwise, variables are thrown away by LVA when going to the Exit block
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
            },
            x =>
            {
                x[condition].Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull);
                x[arg].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy);
            },
            x =>
            {
                x[condition].Should().HaveOnlyConstraints(BoolConstraint.False, ObjectConstraint.NotNull);
                x[arg].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy);
            });
    }

    [TestMethod]
    public void DoLoop_BodyVisitedMaxThreeTimes()
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
            x => x.Should().HaveOnlyConstraints(TestConstraint.First, ObjectConstraint.NotNull, BoolConstraint.True, NumberConstraint.From(null, 0)),
            x => x.Should().HaveOnlyConstraints(TestConstraint.First, ObjectConstraint.NotNull, BoolConstraint.True, DummyConstraint.Dummy, NumberConstraint.From(null, 0)));
    }

    [DataTestMethod]
    [DataRow("for( ; ; )")]
    [DataRow("while (true)")]
    public void InfiniteLoopWithNoExitBranch_BodyVisitedMaxThreeTimes(string loop)
    {
        var code = $$"""
            {{loop}}
            {
                arg.ToString(); // Add another constraint to 'arg'
                Tag("Inside", arg);
            }
            """;
        var validator = SETestContext.CreateCS(code, "int arg", new AddConstraintOnInvocationCheck()).Validator;
        validator.ValidateExitReachCount(0);                    // There's no branch to 'Exit' block, or the exist is never reached
        validator.TagValues("Inside").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(TestConstraint.First, ObjectConstraint.NotNull),
            x => x.Should().HaveOnlyConstraints(TestConstraint.First, ObjectConstraint.NotNull, BoolConstraint.True),
            x => x.Should().HaveOnlyConstraints(TestConstraint.First, ObjectConstraint.NotNull, BoolConstraint.True, DummyConstraint.Dummy));
    }

    [TestMethod]
    public void GoTo_InfiniteWithNoExitBranch_InstructionVisitedMaxThreeTimes()
    {
        const string code = """
            Start:
            arg.ToString(); // Add another constraint to 'arg'
            Tag("Inside", arg);
            goto Start;
            """;
        var validator = SETestContext.CreateCS(code, "int arg", new AddConstraintOnInvocationCheck()).Validator;
        validator.ValidateExitReachCount(0);                    // There's no branch to 'Exit' block
        validator.TagValues("Inside").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy));
    }

    [TestMethod]
    public void GoTo_InstructionVisitedMaxThreeTimes()
    {
        const string code = """
            Start:
            arg.ToString(); // Add another constraint to 'arg'
            arg--;
            if (arg > 0)
            {
                goto Start;
            }
            Tag("End", arg);
            """;
        var validator = SETestContext.CreateCS(code, "int arg", new AddConstraintOnInvocationCheck()).Validator;
        validator.ValidateExitReachCount(1);
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(null, 0), TestConstraint.First),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(null, 0), TestConstraint.First, BoolConstraint.True),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(null, 0), TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy));
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
            Tag("End", i);
            """;
        var validator = SETestContext.CreateCS(code, new AddConstraintOnInvocationCheck()).Validator;
        validator.ValidateExitReachCount(1);
        validator.ValidateTagOrder("InLoop", "InLoop", "End", "InLoop");
        validator.TagValues("InLoop").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(0)),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(1, 9)),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(2, 9)));
        validator.TagValue("End").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(10, null));
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

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
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.Test.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

public partial class RoslynSymbolicExecutionTest
{
    [TestMethod]
    public void Finally_Simple()
    {
        const string code = """
            Tag("BeforeTry");
            try
            {
                Tag("InTry");
            }
            finally
            {
                Tag("InFinally");
            }
            Tag("AfterFinally");
            """;
        SETestContext.CreateCS(code).Validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InFinally",    // With Exception thrown by Tag("InTry")
            "InFinally",
            "AfterFinally");
    }

    [TestMethod]
    public void Finally_Nested_ExitingTwoFinallyOnSameBranch()
    {
        const string code = """
            Tag("BeforeOuterTry");
            try
            {
                Tag("InOuterTry");
                try
                {
                    Tag("InInnerTry");
                }
                finally
                {
                    true.ToString();    // Put some operations in the way
                    Tag("InInnerFinally");
                }
            }
            finally
            {
                true.ToString();    // Put some operations in the way
                true.ToString();
                Tag("InOuterFinally");
            }
            Tag("AfterOuterFinally");
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeOuterTry",
            "InOuterTry",
            "InInnerTry",
            "InOuterFinally",   // With Exception thrown by Tag("InOuterTry")
            "InInnerFinally",   // With Exception thrown by Tag("InInnerTry")
            "InInnerFinally",
            "InOuterFinally",
            "AfterOuterFinally");

        validator.TagStates("InInnerFinally").Should().HaveCount(2)
            .And.ContainSingle(x => HasNoException(x))
            .And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("InOuterFinally").Should().HaveCount(2)
            .And.ContainSingle(x => HasNoException(x))
            .And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("AfterOuterFinally").Should().HaveCount(1)  // Not visited by flows with Exception
            .And.ContainSingle(x => HasNoException(x));
        validator.ValidateExitReachCount(2);
    }

    [TestMethod]
    public void Finally_Nested_InstructionAfterFinally()
    {
        const string code = """
            Tag("BeforeOuterTry");
            try
            {
                Tag("InOuterTry");
                try
                {
                    Tag("InInnerTry");
                }
                finally
                {
                    true.ToString();    // Put some operations in the way
                    Tag("InInnerFinally");
                }
                Tag("AfterInnerFinally");
            }
            finally
            {
                true.ToString();    // Put some operations in the way
                Tag("InOuterFinally");
            }
            Tag("AfterOuterFinally");
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeOuterTry",
            "InOuterTry",
            "InInnerTry",
            "InOuterFinally",       // With Exception thrown by Tag("InOuterTry")
            "InInnerFinally",       // With Exception thrown by Tag("InInnerTry")
            "InInnerFinally",
            "AfterInnerFinally",    // Only once, because exception run from Tag("InInnerTry") continues directlyto outer finally
            "InOuterFinally",
            "AfterOuterFinally");

        validator.TagStates("InInnerFinally").Should().HaveCount(2)
            .And.ContainSingle(x => HasNoException(x))
            .And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("AfterInnerFinally").Should().HaveCount(1)  // Not visited by flows with Exception
            .And.ContainSingle(x => HasNoException(x));
        validator.TagStates("InOuterFinally").Should().HaveCount(2)
            .And.ContainSingle(x => HasNoException(x))
            .And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("AfterOuterFinally").Should().HaveCount(1)  // Not visited by flows with Exception
            .And.ContainSingle(x => HasNoException(x));
        validator.ValidateExitReachCount(2);
    }

    [TestMethod]
    public void Finally_Nested_InstructionAfterFinally_NoThrowsInFinally()
    {
        const string code = """
            var tag = "BeforeOuterTry";
            var value = false;
            try
            {
                try
                {
                    Tag("InInnerTry");    // This can throw
                }
                finally
                {
                    tag = "InInnerFinally";
                }
                value = true;
                tag = "AfterInnerFinally";
            }
            finally
            {
                Tag("InOuterFinally", value);
            }
            Tag("AfterOuterFinally", value);
            """;
        var validator = SETestContext.CreateCS(code, new PreserveTestCheck("value")).Validator;
        validator.ValidateTagOrder(
            "BeforeOuterTry",
            "InInnerTry",
            "InInnerFinally",       // WithException thrown by Tag("InInnerTry")
            "InInnerFinally",
            "InOuterFinally",       // With Exception thrown by Tag("InInnerTry")
            "AfterInnerFinally",
            "InOuterFinally",
            "AfterOuterFinally");

        validator.TagStates("InOuterFinally").Should().HaveCount(2)
            .And.ContainSingle(x => HasNoException(x) && x.SymbolsWith(BoolConstraint.True).Any(symbol => symbol.Name == "value"))
            .And.ContainSingle(x => HasUnknownException(x) && x.SymbolsWith(BoolConstraint.False).Any(symbol => symbol.Name == "value"));
        validator.TagStates("AfterOuterFinally").Should().HaveCount(1)  // Not visited by flow with Exception
            .And.ContainSingle(x => HasNoException(x) && x.SymbolsWith(BoolConstraint.True).Any(symbol => symbol.Name == "value"));
        validator.ValidateExitReachCount(2);
    }

    [TestMethod]
    public void Finally_NestedInFinally_InstructionAfterFinally_NoThrowsInFinally()
    {
        const string code = """
            var tag = "BeforeOuterTry";
            var value = false;
            try
            {
                Tag("InOuterTry");    // This can throw
            }
            finally
            {
                tag = "BeforeInnerTry";
                try
                {
                    value = false;          // Operation that cannot throw - this doesn't do anything
                    tag = "InInnerTry";
                }
                finally
                {
                    tag = "InInnerFinally";   // Operation that cannot throw
                }
                value = true;

                Tag("InOuterFinally", value);
            }
            Tag("AfterOuterFinally", value);
            """;
        var validator = SETestContext.CreateCS(code, new PreserveTestCheck("value")).Validator;
        validator.ValidateTagOrder(
            "BeforeOuterTry",
            "InOuterTry",
            "BeforeInnerTry",       // With Exception thrown by Tag("InOuterTry")
            "BeforeInnerTry",
            "InInnerTry",
            "InInnerTry",           // With Exception thrown by Tag("InOuterTry")
            "InInnerFinally",
            "InInnerFinally",       // With Exception thrown by Tag("InOuterTry")
            "InOuterFinally",
            "InOuterFinally",       // With Exception thrown by Tag("InOuterTry")
            "AfterOuterFinally");

        validator.TagStates("InOuterFinally").Should().HaveCount(2)
            .And.ContainSingle(x => HasNoException(x) && x.SymbolsWith(BoolConstraint.True).Any(symbol => symbol.Name == "value"))
            .And.ContainSingle(x => HasUnknownException(x) && x.SymbolsWith(BoolConstraint.True).Any(symbol => symbol.Name == "value"));
        validator.TagStates("AfterOuterFinally").Should().HaveCount(1)  // Not visited by flow with Exception
            .And.ContainSingle(x => HasNoException(x) && x.SymbolsWith(BoolConstraint.True).Any(symbol => symbol.Name == "value"));
        validator.ValidateExitReachCount(2);
    }

    [TestMethod]
    public void Finally_BranchInNested()
    {
        const string code = """
            Tag("BeforeOuterTry");
            try
            {
                Tag("InOuterTry");
                try
                {
                    Tag("InInnerTry");
                    if (Condition)
                    {
                        Tag("1");
                    }
                    else
                    {
                        Tag("2");
                    }
                }
                finally
                {
                    Tag("InInnerFinally");
                }
            }
            finally
            {
                Tag("InOuterFinally");
            }
            Tag("AfterOuterFinally");
            """;
        SETestContext.CreateCS(code).Validator.ValidateTagOrder(
            "BeforeOuterTry",
            "InOuterTry",
            "InOuterFinally",   // With Exception thrown by Tag("InOuterTry")
            "InInnerTry",
            "InInnerFinally",   // With Exception thrown by Tag("InInnerTry")
            "1",
            "2",
            "InInnerFinally",
            "InOuterFinally",
            "AfterOuterFinally");
    }

    [TestMethod]
    public void Finally_BranchAfterFinally()
    {
        const string code = """
            Tag("BeforeTry");
            try
            {
                Tag("InTry");
            }
            finally
            {
                true.ToString();    // Put some operations in the way
                Tag("InFinally");
            }
            if (boolParameter)  // No operation between the finally and this. This will create a single follow up block with BranchValue
            {
                Tag("1");
            }
            else
            {
                Tag("2");
            }
            """;
        SETestContext.CreateCS(code).Validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InFinally",    // With Exception thrown by Tag("InTry")
            "InFinally",
            "1",
            "2");
    }

    [TestMethod]
    public void Finally_BranchInFinally()
    {
        const string code = """
            Tag("BeforeTry");
            try
            {
                Tag("InTry");
            }
            finally
            {
                Tag("InFinallyBeforeCondition");
                if (Condition)
                {
                    Tag("1");
                }
                else
                {
                    Tag("2");
                }
                Tag("InFinallyAfterCondition");
            }
            Tag("AfterFinally");
            """;
        SETestContext.CreateCS(code).Validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InFinallyBeforeCondition",     // With Exception thrown by Tag("InTry")
            "InFinallyBeforeCondition",
            "1",    // With Exception thrown by Tag("InTry")
            "2",    // With Exception thrown by Tag("InTry")
            "1",
            "2",
            "InFinallyAfterCondition",      // With Exception thrown by Tag("InTry")
            "InFinallyAfterCondition",
            "AfterFinally");
    }

    [TestMethod]
    public void Finally_WrappedInLocalLifetimeRegion()
    {
        const string code = """
            Tag("BeforeTry");
            try
            {
                Tag("InTry");
            }
            finally
            {
                var local = true;   // This creates LocalLifeTime region
                Tag("InFinally");
                // Here is Block#4 outside the LocalLifeTime region
            }
            Tag("AfterFinally");
            """;
        SETestContext.CreateCS(code).Validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InFinally",    // With Exception thrown by Tag("InTry")
            "InFinally",
            "AfterFinally");
    }

    [TestMethod]
    public void Finally_NestedFinally()
    {
        const string code = """
            var tag = "BeforeOuterTry";
            try
            {
                Tag("InOuterTry");
            }
            finally
            {
                Tag("InOuterFinally");
                try
                {
                    Tag("InInnerTry");
                }
                finally
                {
                    Tag("InInnerFinally");
                }
                Tag("AfterInnerFinally");
            }
            Tag("AfterOuterFinally");
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeOuterTry",
            "InOuterTry",
            "InOuterFinally",       // With Exception thrown by Tag("InOuterTry")
            "InOuterFinally",
            "InInnerTry",           // With Exception thrown by Tag("InOuterTry")
            "InInnerTry",
            "InInnerFinally",       // With Exception thrown by Tag("InOuterTry"), that visits AfterInnerFinally
            "InInnerFinally",       // With Exception thrown by Tag("InInnerTry"), that skips AfterInnerFinally and goes to exit block
            "InInnerFinally",       // No exception
            "AfterInnerFinally",    // With Exception thrown by Tag("InOuterTry"), that visits AfterInnerFinally
            "AfterInnerFinally",    // No exception
            "AfterOuterFinally");
        ValidateHasOnlyNoExceptionAndUnknownException(validator, "InOuterFinally");
        ValidateHasOnlyNoExceptionAndUnknownException(validator, "InInnerTry");
        ValidateHasOnlyNoExceptionAndUnknownException(validator, "AfterInnerFinally");
        validator.TagStates("InInnerFinally").Should().SatisfyRespectively(
            x => HasUnknownException(x).Should().BeTrue(),
            x => HasUnknownException(x).Should().BeTrue(),
            x => HasNoException(x).Should().BeTrue());
        validator.TagStates("AfterOuterFinally").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Finally_NestedInCatch()
    {
        const string code = """
            var tag = "BeforeOuterTry";
            try
            {
                try
                {
                    Tag("InInnerTry");
                }
                finally
                {
                    tag = "InInnerFinally";
                }
                tag = "AfterInnerTry";
            }
            catch
            {
                tag = "InOuterCatch";
            }
            Tag("End");
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeOuterTry",
            "InInnerTry",
            "InInnerFinally",   // With exception thrown by Tag("InInnerTry")
            "InInnerFinally",
            "InOuterCatch",
            "AfterInnerTry",
            "End");

        ValidateHasOnlyNoExceptionAndUnknownException(validator, "InInnerFinally");
        validator.TagStates("AfterInnerTry").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
        validator.TagStates("InOuterCatch").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("End").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Finally_NestedInCatch_NestedInFinally()
    {
        const string code = """
            var tag = "BeforeOuterTry";
            try
            {
                tag = "BeforeMiddleTry";
                try
                {
                    tag = "BeforeInnerTry";
                    try
                    {
                        Tag("InInnerTry");
                    }
                    finally
                    {
                        tag = "InInnerFinally";
                    }
                    tag = "AfterInnerTry";
                }
                finally
                {
                    tag = "InMiddleFinally";
                }
                tag = "AfterMiddleTry";
            }
            catch
            {
                tag = "InOuterCatch";
            }
            Tag("End");
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeOuterTry",
            "BeforeMiddleTry",
            "BeforeInnerTry",
            "InInnerTry",
            "InInnerFinally",   // With exception thrown by Tag("InInnerTry")
            "InInnerFinally",
            "InMiddleFinally",  // With exception thrown by Tag("InInnerTry")
            "AfterInnerTry",
            "InOuterCatch",
            "InMiddleFinally",
            "AfterMiddleTry",
            "End");
        ValidateHasOnlyNoExceptionAndUnknownException(validator, "InInnerFinally");
        validator.TagStates("AfterInnerTry").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
        ValidateHasOnlyNoExceptionAndUnknownException(validator, "InMiddleFinally");
        validator.TagStates("AfterMiddleTry").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
        validator.TagStates("InOuterCatch").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("End").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Catch_Simple_NoFilter()
    {
        const string code = """
            var tag = "BeforeTry";
            try
            {
                Tag("InTry");
            }
            catch
            {
                tag = "InCatch";
            }
            tag = "End";
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InCatch",
            "End");

        validator.TagStates("InCatch").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("End").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Catch_WrappedInLocalLifetimeRegion()
    {
        const string code = """
            var tag = "BeforeTry";
            try
            {
                Tag("InTry");
            }
            catch
            {
                tag = "InCatch";
                if (true)
                {
                    var local = true;   // Block #4 is wrapped in LocalLifeTime region
                }
            }
            tag = "End";
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InCatch",
            "End");

        validator.TagStates("InCatch").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("End").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Catch_Simple_TypeFilter()
    {
        const string code = """
            var tag = "BeforeTry";
            try
            {
                Tag("InTry");
            }
            catch (Exception)
            {
                tag = "InCatch";
            }
            tag = "End";
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InCatch",
            "End");

        validator.TagStates("InCatch").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("End").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Catch_Multiple()
    {
        const string code = """
            var tag = "BeforeTry";
            try
            {
                Tag("InTry");
            }
            catch (ArgumentNullException ex)
            {
                tag = "InCatchArgumentNull";
            }
            catch (NotSupportedException ex)
            {
                tag = "InCatchNotSupported";
            }
            catch (Exception ex)
            {
                tag = "InCatchEverything";
            }
            tag = "End";
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "End",
            "InCatchArgumentNull",
            "InCatchNotSupported",
            "InCatchEverything");

        validator.TagStates("InCatchArgumentNull").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("InCatchNotSupported").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("InCatchEverything").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("End").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Catch_Finally()
    {
        const string code = """
            var tag = "BeforeTry";
            try
            {
                Tag("InTry");
            }
            catch (Exception ex)
            {
                tag = "InCatch";
            }
            finally
            {
                tag = "InFinally";
            }
            tag = "End";
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InFinally",    // Happy path
            "InCatch",      // Exception thrown by Tag("InTry")
            "End");

        validator.TagStates("InCatch").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("InFinally").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
        validator.TagStates("End").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Catch_NestedInTry()
    {
        const string code = """
            var tag = "BeforeOuterTry";
            try
            {
                Tag("BeforeInnerTry");    // Can throw
                try
                {
                    Tag("InInnerTry");    // Can throw
                }
                catch (Exception exInner)
                {
                    tag = "InInnerCatch";
                }
                tag = "AfterInnerTry";
            }
            catch (Exception ex)
            {
                tag = "InOuterCatch";
            }
            tag = "End";
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeOuterTry",
            "BeforeInnerTry",
            "InOuterCatch",
            "InInnerTry",
            "End",
            "AfterInnerTry",
            "InInnerCatch");
        validator.TagStates("InInnerCatch").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("AfterInnerTry").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
        validator.TagStates("InOuterCatch").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("End").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Catch_NestedInCatch()
    {
        const string code = """
            var tag = "BeforeOuterTry";
            try
            {
                Tag("InOuterTry");
            }
            catch (Exception exOuter)
            {
                tag = "BeforeInnerTry";
                try
                {
                    Tag("InInnerTry");
                }
                catch (Exception exInner)
                {
                    tag = "InInnerCatch";
                }
                tag = "AfterInnerTry";
            }
            tag = "End";
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeOuterTry",
            "InOuterTry",
            "End",
            "BeforeInnerTry",
            "InInnerTry",
            "AfterInnerTry",
            "InInnerCatch");

        validator.TagStates("BeforeInnerTry").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("InInnerTry").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("InInnerCatch").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("AfterInnerTry").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("End").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Catch_NestedInFinally()
    {
        const string code = """
            var tag = "BeforeOuterTry";
            try
            {
                Tag("InOuterTry");
            }
            catch (Exception ex)
            {
                tag = "InOuterCatch";
            }
            finally
            {
                tag = "BeforeInnerTry";
                try
                {
                    Tag("InInnerTry");
                }
                catch (Exception exInner)
                {
                    tag = "InInnerCatch";
                }
                tag = "AfterInnerTry";
            }
            tag = "End";
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeOuterTry",
            "InOuterTry",
            "BeforeInnerTry",
            "InOuterCatch",
            "InInnerTry",
            "AfterInnerTry",
            "InInnerCatch",
            "End",
            "AfterInnerTry");

        validator.TagStates("InOuterCatch").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("BeforeInnerTry").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
        validator.TagStates("InInnerCatch").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("AfterInnerTry").Should().HaveCount(2).And.OnlyContain(x => HasNoException(x));
        validator.TagStates("End").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void CatchWhen_Simple()
    {
        const string code = """
            var tag = "BeforeTry";
            try
            {
                Tag("InTry");
            }
            catch (Exception ex) when (ex is FormatException)
            {
                tag = "InCatch";
            }
            finally
            {
                tag = "InFinally";
            }
            tag = "End";
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InFinally",
            "InFinally",
            "InCatch",
            "End");

        validator.TagStates("InCatch").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("InFinally").Should().HaveCount(2)
            .And.ContainSingle(x => HasNoException(x), "no exception, and exception processed by catch clears exception state")
            .And.ContainSingle(x => HasUnknownException(x), "if an exception not matching the filter is thrown, execution goes from 'InTry' to 'InFinally'.");
        validator.TagStates("End").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void CatchWhen_Multiple()
    {
        const string code = """
            var tag = "BeforeTry";
            try
            {
                Tag("InTry");
            }
            catch (ArgumentNullException ex) when (ex.ParamName == "value")
            {
                tag = "InCatchArgumentWhen";
            }
            catch (ArgumentNullException ex)
            {
                tag = "InCatchArgument";
            }
            catch (Exception ex) when (ex is ArgumentNullException)
            {
                tag = "InCatchAllWhen";
            }
            catch (Exception ex)
            {
                tag = "InCatchAll";
            }
            finally
            {
                tag = "InFinally";
            }
            tag = "End";
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InFinally",
            "InCatchArgument",
            "InCatchAll",
            "InCatchAllWhen",
            "End",
            "InCatchArgumentWhen");

        validator.TagStates("InCatchArgumentWhen").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("InCatchArgument").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("InCatchAllWhen").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("InCatchAll").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("InFinally").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
        validator.TagStates("End").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Finally_NestedLambda_ShouldNotFail()
    {
        const string code = """
            Tag("Unreachable - outer CFG is not analyzed");
            try
            {
                Action a = () =>
                {
                    // Lambda marker
                    var tag = "Before";
                    Tag("CanThrow");
                    tag = "After";
                };
            }
            finally
            {
                Tag("Unreachable - outer CFG is not analyzed");
            }
            """;
        SETestContext.CreateCSLambda(code, "// Lambda marker").Validator.ValidateTagOrder("Before", "CanThrow", "After");
    }

    [TestMethod]
    public void Catch_ExceptionVariableIsNotNull()
    {
        const string code = """
            try
            {
                InstanceMethod();
            }
            catch(InvalidOperationException ex) when (Tag("InFilter", ex))
            {
                Tag("InCatch", ex);
            }

            static bool Tag<T>(string name, T value) => true;
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("InFilter").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("InCatch").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    private static bool HasNoException(ProgramState state) =>
        state.Exception == null;

    private static bool HasUnknownException(ProgramState state) =>
         state.Exception == ExceptionState.UnknownException;

    private static bool HasSystemException(ProgramState state) =>
        HasExceptionOfType(state, "Exception");

    private static bool HasExceptionOfType(ProgramState state, string typeName) =>
        state.Exception?.Type?.Name == typeName;

    private static void ValidateHasOnlyUnknownExceptionAndSystemException(ValidatorTestCheck validator, string stateName) =>
        validator.TagStates(stateName).Should().HaveCount(2)
                 .And.ContainSingle(x => HasUnknownException(x))
                 .And.ContainSingle(x => HasSystemException(x));

    private static void ValidateHasOnlyNoExceptionAndUnknownException(ValidatorTestCheck validator, string stateName) =>
        validator.TagStates(stateName).Should().HaveCount(2)
                 .And.ContainSingle(x => HasNoException(x))
                 .And.ContainSingle(x => HasUnknownException(x));
}

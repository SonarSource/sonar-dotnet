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

using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    public partial class RoslynSymbolicExecutionTest
    {
        [TestMethod]
        public void Finally_Simple()
        {
            const string code = @"
Tag(""BeforeTry"");
try
{
    Tag(""InTry"");
}
finally
{
    Tag(""InFinally"");
}
Tag(""AfterFinally"");";
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
            const string code = @"
Tag(""BeforeOuterTry"");
try
{
    Tag(""InOuterTry"");
    try
    {
        Tag(""InInnerTry"");
    }
    finally
    {
        true.ToString();    // Put some operations in the way
        Tag(""InInnerFinally"");
    }
}
finally
{
    true.ToString();    // Put some operations in the way
    true.ToString();
    Tag(""InOuterFinally"");
}
Tag(""AfterOuterFinally"");";
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
                .And.ContainSingle(x => x.Exception == null)
                .And.ContainSingle(x => x.Exception != null);
            validator.TagStates("InOuterFinally").Should().HaveCount(2)
                .And.ContainSingle(x => x.Exception == null)
                .And.ContainSingle(x => x.Exception != null);
            validator.TagStates("AfterOuterFinally").Should().HaveCount(1)  // Not visited by flows with Exception
                .And.ContainSingle(x => x.Exception == null);
            validator.ValidateExitReachCount(2);
        }

        [TestMethod]
        public void Finally_Nested_InstructionAfterFinally()
        {
            const string code = @"
Tag(""BeforeOuterTry"");
try
{
    Tag(""InOuterTry"");
    try
    {
        Tag(""InInnerTry"");
    }
    finally
    {
        true.ToString();    // Put some operations in the way
        Tag(""InInnerFinally"");
    }
    Tag(""AfterInnerFinally"");
}
finally
{
    true.ToString();    // Put some operations in the way
    Tag(""InOuterFinally"");
}
Tag(""AfterOuterFinally"");";
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
                .And.ContainSingle(x => x.Exception == null)
                .And.ContainSingle(x => x.Exception != null);
            validator.TagStates("AfterInnerFinally").Should().HaveCount(1)  // Not visited by flows with Exception
                .And.ContainSingle(x => x.Exception == null);
            validator.TagStates("InOuterFinally").Should().HaveCount(2)
                .And.ContainSingle(x => x.Exception == null)
                .And.ContainSingle(x => x.Exception != null);
            validator.TagStates("AfterOuterFinally").Should().HaveCount(1)  // Not visited by flows with Exception
                .And.ContainSingle(x => x.Exception == null);
            validator.ValidateExitReachCount(2);
        }

        [TestMethod]
        public void Finally_Nested_InstructionAfterFinally_NoThrowsInFinally()
        {
            const string code = @"
var tag = ""BeforeOuterTry"";
var value = false;
try
{
    try
    {
        Tag(""InInnerTry"");    // This can throw
    }
    finally
    {
        tag = ""InInnerFinally"";
    }
    value = true;
    tag = ""AfterInnerFinally"";
}
finally
{
    Tag(""InOuterFinally"", value);
}
Tag(""AfterOuterFinally"", value);";
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
                .And.ContainSingle(x => x.Exception == null && x.SymbolsWith(BoolConstraint.True).Any(symbol => symbol.Name == "value"))
                .And.ContainSingle(x => x.Exception != null && x.SymbolsWith(BoolConstraint.False).Any(symbol => symbol.Name == "value"));
            validator.TagStates("AfterOuterFinally").Should().HaveCount(1)  // Not visited by flow with Exception
                .And.ContainSingle(x => x.Exception == null && x.SymbolsWith(BoolConstraint.True).Any(symbol => symbol.Name == "value"));
            validator.ValidateExitReachCount(2);
        }

        [TestMethod]
        public void Finally_NestedInFinally_InstructionAfterFinally_NoThrowsInFinally()
        {
            const string code = @"
var tag = ""BeforeOuterTry"";
var value = false;
try
{
    Tag(""InOuterTry"");    // This can throw
}
finally
{
    tag = ""BeforeInnerTry"";
    try
    {
        value = false;          // Operation that cannot throw - this doesn't do anything
        tag = ""InInnerTry"";
    }
    finally
    {
        tag = ""InInnerFinally"";   // Operation that cannot throw
    }
    value = true;

    Tag(""InOuterFinally"", value);
}
Tag(""AfterOuterFinally"", value);";
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
                .And.ContainSingle(x => x.Exception == null && x.SymbolsWith(BoolConstraint.True).Any(symbol => symbol.Name == "value"))
                .And.ContainSingle(x => x.Exception != null && x.SymbolsWith(BoolConstraint.True).Any(symbol => symbol.Name == "value"));
            validator.TagStates("AfterOuterFinally").Should().HaveCount(1)  // Not visited by flow with Exception
                .And.ContainSingle(x => x.Exception == null && x.SymbolsWith(BoolConstraint.True).Any(symbol => symbol.Name == "value"));
            validator.ValidateExitReachCount(2);
        }

        [TestMethod]
        public void Finally_BranchInNested()
        {
            const string code = @"
Tag(""BeforeOuterTry"");
try
{
    Tag(""InOuterTry"");
    try
    {
        Tag(""InInnerTry"");
        if (Condition)
        {
            Tag(""1"");
        }
        else
        {
            Tag(""2"");
        }
    }
    finally
    {
        Tag(""InInnerFinally"");
    }
}
finally
{
    Tag(""InOuterFinally"");
}
Tag(""AfterOuterFinally"");";
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
            const string code = @"
Tag(""BeforeTry"");
try
{
    Tag(""InTry"");
}
finally
{
    true.ToString();    // Put some operations in the way
    Tag(""InFinally"");
}
if (boolParameter)  // No operation between the finally and this. This will create a single follow up block with BranchValue
{
    Tag(""1"");
}
else
{
    Tag(""2"");
}";
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
            const string code = @"
Tag(""BeforeTry"");
try
{
    Tag(""InTry"");
}
finally
{
    Tag(""InFinallyBeforeCondition"");
    if (Condition)
    {
        Tag(""1"");
    }
    else
    {
        Tag(""2"");
    }
    Tag(""InFinallyAfterCondition"");
}
Tag(""AfterFinally"");";
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
            const string code = @"
Tag(""BeforeTry"");
try
{
    Tag(""InTry"");
}
finally
{
    var local = true;   // This creates LocalLifeTime region
    Tag(""InFinally"");
    // Here is Block#4 outside the LocalLifeTime region
}
Tag(""AfterFinally"");";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder(
                "BeforeTry",
                "InTry",
                "InFinally",    // With Exception thrown by Tag("InTry")
                "InFinally",
                "AfterFinally");
        }

        [TestMethod]
        public void Finally_ThrowInTry()
        {
            const string code = @"
var tag = ""BeforeTry"";
try
{
    Tag(""InTry"");
    throw new System.Exception();
    tag = ""UnreachableInTry"";
}
finally
{
    tag = ""InFinally"";
}
tag = ""UnreachableAfterFinally"";";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTagOrder(
                "BeforeTry",
                "InTry",
                "InFinally",                // With Exception thrown by Tag("InTry")
                "InFinally");               // With Exception thrown by `throw`

            validator.TagStates("InFinally").Should().HaveCount(2)
                     .And.ContainSingle(x => HasUnknownException(x))
                     .And.ContainSingle(x => HasExceptionOfTypeException(x));
        }

        [TestMethod]
        public void Finally_ThrowInFinally()
        {
            const string code = @"
var tag = ""BeforeTry"";
try
{
    Tag(""InTry"");
}
finally
{
    tag = ""InFinally"";
    throw new System.Exception();
    tag  = ""UnreachableInFinally"";
}
tag = ""UnreachableAfterFinally"";";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTagOrder(
                "BeforeTry",
                "InTry",
                "InFinally",
                "InFinally");

            validator.TagStates("InFinally").Should().HaveCount(2)
                .And.ContainSingle(x => HasNoException(x))
                .And.ContainSingle(x => HasUnknownException(x));
        }

        [TestMethod]
        public void Finally_NestedFinally()
        {
            const string code = @"
var tag = ""BeforeOuterTry"";
try
{
    Tag(""InOuterTry"");
}
finally
{
    Tag(""InOuterFinally"");
    try
    {
        Tag(""InInnerTry"");
    }
    finally
    {
        Tag(""InInnerFinally"");
    }
    Tag(""AfterInnerFinally"");
}
Tag(""AfterOuterFinally"");";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTagOrder(
                "BeforeOuterTry",
                "InOuterTry",
                "InOuterFinally",       // With Exception thrown by Tag("InOuterTry")
                "InOuterFinally",
                "InInnerTry",           // With Exception thrown by Tag("InOuterTry")
                "InInnerTry",
                "InInnerFinally",       // With Exception thrown by Tag("InOuterTry")
                "InInnerFinally",
                "AfterInnerFinally",
                "AfterOuterFinally");

            validator.TagStates("InOuterFinally").Should().HaveCount(2)
                     .And.ContainSingle(x => HasNoException(x))
                     .And.ContainSingle(x => HasUnknownException(x));

            validator.TagStates("InInnerFinally").Should().HaveCount(2)
                     .And.ContainSingle(x => HasNoException(x))
                     .And.ContainSingle(x => HasUnknownException(x));

            validator.TagStates("InInnerTry").Should().HaveCount(2)
                     .And.ContainSingle(x => HasNoException(x))
                     .And.ContainSingle(x => HasUnknownException(x));

            validator.TagStates("AfterInnerFinally").Should().HaveCount(1)
                     .And.ContainSingle(x => HasNoException(x));

            validator.TagStates("AfterOuterFinally").Should().HaveCount(1)
                     .And.ContainSingle(x => HasNoException(x));
        }

        [TestMethod]
        public void TryCatch_ThrowInTry_SingleCatchBlock()
        {
            const string code = @"
var tag = ""BeforeTry"";
try
{
    Tag(""InTry"");
    throw new System.Exception();
    tag = ""UnreachableInFinally"";
}
catch
{
    tag = ""InCatch"";
}
tag = ""AfterCatch"";";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTagOrder(
                "BeforeTry",
                "InTry",
                "InCatch",          // With Exception thrown by Tag("InTry")
                "InCatch",          // With Exception thrown by `throw`
                "AfterCatch",
                "AfterCatch");      // Should go away after https://github.com/SonarSource/sonar-dotnet/pull/5745 is done.

            validator.TagStates("InCatch").Should().HaveCount(2)
                     .And.ContainSingle(x => HasUnknownException(x))
                     .And.ContainSingle(x => HasExceptionOfTypeException(x));

            validator.TagStates("AfterCatch").Should().HaveCount(2)
                     .And.ContainSingle(x => HasUnknownException(x))
                     .And.ContainSingle(x => HasExceptionOfTypeException(x));
        }

        [TestMethod]
        public void TryCatch_ThrowInTry_MultipleCatchBlocks()
        {
            const string code = @"
var tag = ""BeforeTry"";
try
{
    Tag(""InTry"");
    throw new System.Exception();
    tag = ""UnreachableInFinally"";
}
catch (System.NullReferenceException)
{
    tag = ""InFirstCatch"";
}
catch (System.Exception)
{
    tag = ""InSecondCatch"";
}
catch
{
    tag = ""InThirdCatch"";
}
tag = ""AfterCatch"";";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTagOrder(
                "BeforeTry",
                "InTry",
                "InFirstCatch",     // Filtering is not implemented yet so all the catch blocks will be iterated.
                "InSecondCatch",
                "InThirdCatch",
                "InFirstCatch",
                "InSecondCatch",
                "InThirdCatch",
                "AfterCatch",
                "AfterCatch");      // Should go away after https://github.com/SonarSource/sonar-dotnet/pull/5745 is done.

            validator.TagStates("InFirstCatch").Should().HaveCount(2)
                     .And.ContainSingle(x => HasUnknownException(x))
                     .And.ContainSingle(x => HasExceptionOfTypeException(x));

            validator.TagStates("InSecondCatch").Should().HaveCount(2)
                     .And.ContainSingle(x => HasUnknownException(x))
                     .And.ContainSingle(x => HasExceptionOfTypeException(x));

            validator.TagStates("InThirdCatch").Should().HaveCount(2)
                     .And.ContainSingle(x => HasUnknownException(x))
                     .And.ContainSingle(x => HasExceptionOfTypeException(x));

            validator.TagStates("AfterCatch").Should().HaveCount(2)
                     .And.ContainSingle(x => HasUnknownException(x))
                     .And.ContainSingle(x => HasExceptionOfTypeException(x));
        }

        [TestMethod]
        public void TryCatch_ThrowInCatch_SingleCatchBlock()
        {
            const string code = @"
var tag = ""BeforeTry"";
try
{
    Tag(""InTry"");
}
catch
{
    tag = ""InCatch"";
    throw new System.Exception();
    tag = ""UnreachableInCatch"";
}
tag = ""AfterCatch"";";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTagOrder(
                "BeforeTry",
                "InTry",
                "InCatch",
                "AfterCatch"); // If there is no exception in try

            validator.TagStates("InCatch").Should().HaveCount(1)
                     .And.ContainSingle(x => HasUnknownException(x));

            validator.TagStates("AfterCatch").Should().HaveCount(1)
                     .And.ContainSingle(x => HasNoException(x));
        }

        [TestMethod]
        public void TryCatchFinally_ThrowInTry()
        {
            const string code = @"
var tag = ""BeforeTry"";
try
{
    Tag(""InTry"");
    throw new System.Exception();
    tag = ""UnreachableInFinally"";
}
catch
{
    tag = ""InCatch"";
}
finally
{
    tag = ""InFinally"";
}
tag = ""AfterFinally"";";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTagOrder(
                "BeforeTry",
                "InTry",
                "InCatch",          // With Exception thrown by Tag("InTry")
                "InCatch",          // With Exception thrown by `throw`
                "InFinally",
                "InFinally",        // Should go away after https://github.com/SonarSource/sonar-dotnet/pull/5745 is done.
                "AfterFinally",
                "AfterFinally");

            validator.TagStates("InCatch").Should().HaveCount(2)
                     .And.ContainSingle(x => HasUnknownException(x))
                     .And.ContainSingle(x => HasExceptionOfTypeException(x));

            validator.TagStates("InFinally").Should().HaveCount(2)
                     .And.ContainSingle(x => HasUnknownException(x))
                     .And.ContainSingle(x => HasExceptionOfTypeException(x));

            validator.TagStates("AfterFinally").Should().HaveCount(2)
                     .And.ContainSingle(x => HasUnknownException(x))
                     .And.ContainSingle(x => HasExceptionOfTypeException(x));
        }

        [TestMethod]
        public void TryCatchFinally_ThrowInCatch()
        {
            const string code = @"
var tag = ""BeforeTry"";
try
{
    Tag(""InTry"");
}
catch
{
    tag = ""InCatch"";
    throw new System.Exception();
    tag = ""UnreachableInCatch"";
}
finally
{
    tag = ""InFinally"";
}
tag = ""AfterFinally"";";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTagOrder(
                "BeforeTry",
                "InTry",
                "InCatch",
                "InFinally",
                "InFinally",
                "AfterFinally");

            validator.TagStates("InCatch").Should().HaveCount(1)
                .And.ContainSingle(x => HasUnknownException(x));

            validator.TagStates("InFinally").Should().HaveCount(2)
                .And.ContainSingle(x => HasNoException(x))
                .And.ContainSingle(x => HasExceptionOfTypeException(x));

            validator.TagStates("AfterFinally").Should().HaveCount(1)
                .And.ContainSingle(x => HasNoException(x));
        }

        [TestMethod]
        public void TryCatchFinally_ThrowInFinally()
        {
            const string code = @"
var tag = ""BeforeTry"";
try
{
    Tag(""InTry"");
}
catch
{
    tag = ""InCatch"";
}
finally
{
    tag = ""InFinally"";
    throw new System.Exception();
    tag = ""UnreachableInCatch"";
}
tag = ""UnreachableAfterFinally"";";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTagOrder(
                "BeforeTry",
                "InTry",
                "InCatch",
                "InFinally",
                "InFinally");

            validator.TagStates("InCatch").Should().HaveCount(1)
                     .And.ContainSingle(x => HasUnknownException(x));

            validator.TagStates("InFinally").Should().HaveCount(2)
                     .And.ContainSingle(x => HasNoException(x))
                     .And.ContainSingle(x => HasUnknownException(x));
        }

        [TestMethod]
        public void TryCatchFinally_NestedThrowWithCatchFilter()
        {
            const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    try
    {
        tag = ""InNestedTry"";
        throw new ArgumentNullException();
        tag = ""UnreachableInNestedTry"";
    }
    catch
    {
        tag = ""InNestedCatch"";
    }
}
catch (ArgumentNullException)
{
    tag = ""InCatch"";
}
tag = ""After"";";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTagOrder(
                "BeforeTry",
                "InTry",
                "InNestedTry"); // ToDo: the rest of the tags are missing since we don't support Conversion operation
        }

        [TestMethod]
        public void TryCatchFinally_NestedThrowWithNestedCatchFilter()
        {
            const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    try
    {
        tag = ""InNestedTry"";
        throw new ArgumentNullException();
        tag = ""UnreachableInNestedTry"";
    }
    catch (NotSupportedException)
    {
        tag = ""InNestedCatch"";
    }
}
catch
{
    tag = ""InCatch"";
}
tag = ""After"";";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTagOrder(
                "BeforeTry",
                "InTry",
                "InNestedTry"); // ToDo: the rest of the tags are missing since we don't support Conversion operation
        }

        [TestMethod]
        public void TryCatchFinally_NestedThrowWithMultipleCatchFiltersOnDifferentLevels()
        {
            const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    try
    {
        tag = ""InNestedTry"";
        throw new ArgumentNullException();
        tag = ""UnreachableInNestedTry"";
    }
    catch (NotSupportedException)
    {
        tag = ""InNestedCatch"";
    }
}
catch (FormatException)
{
    tag = ""InCatch"";
}
tag = ""After"";";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTagOrder(
                "BeforeTry",
                "InTry",
                "InNestedTry"); // ToDo: the rest of the tags are missing since we don't support Conversion operation
        }

        private static bool HasNoException(ProgramState state) =>
            state.Exception == null;

        private static bool HasUnknownException(ProgramState state) =>
             state.Exception == ExceptionState.UnknownException;

        private static bool HasExceptionOfTypeException(ProgramState state) =>
            HasExceptionOfType(state, "Exception");

        private static bool HasExceptionOfType(ProgramState state, string typeName) =>
            state.Exception?.Type?.Name == typeName;
    }
}

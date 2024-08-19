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

using SonarAnalyzer.Test.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

public partial class RoslynSymbolicExecutionTest
{
    [TestMethod]
    public void Throw_Finally_ThrowInTry()
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

        ValidateHasOnlyUnknownExceptionAndSystemException(validator, "InFinally");
    }

    [TestMethod]
    public void Throw_Finally_ThrowInFinally()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
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
            "InFinally");

        validator.TagStates("InFinally").Should().HaveCount(1)
                 .And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Throw_TryCatch_ThrowInTry_SingleCatchBlock()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    Tag(""InTry"");
    throw new System.Exception();
    tag = ""UnreachableInTry"";
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
            "AfterCatch");

        ValidateHasOnlyUnknownExceptionAndSystemException(validator, "InCatch");
        validator.TagStates("AfterCatch").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Throw_TryCatch_ThrowInTry_SingleCatchBlock_ReThrow()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    throw new System.Exception();
    tag = ""UnreachableInTry"";
}
catch
{
    tag = ""InCatch"";
    throw;
}
tag = ""UnreachableAfterCatch"";";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InCatch");         // With Exception thrown by `throw`

        validator.ValidateExitReachCount(1);

        validator.TagStates("InCatch").Should().HaveCount(1)
                 .And.ContainSingle(x => HasSystemException(x));
    }
    [TestMethod]
    public void Throw_TryCatch_ThrowInTry_SingleCatchBlock_ReThrowException()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    Tag(""InTry"");
    throw new System.Exception();
    tag = ""UnreachableInTry"";
}
catch (Exception ex)
{
    tag = ""InCatch"";
    throw ex;
}
tag = ""UnreachableAfterCatch"";";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InCatch",  // With Exception thrown by Tag("InTry")
            "InCatch"); // With Exception thrown by `throw`

        ValidateHasOnlyUnknownExceptionAndSystemException(validator, "InCatch");

        validator.ValidateExitReachCount(1);
    }

    [TestMethod]
    public void Throw_TryCatch_ThrowInTry_MultipleCatchBlocks()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    Tag(""InTry"");
    throw new System.Exception();
    tag = ""UnreachableInTry"";
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
            "InFirstCatch",     // With Exception thrown by Tag("InTry")
            "InSecondCatch",    // With Exception thrown by Tag("InTry")
            "InSecondCatch",    // With Exception thrown by throw new System.Exception()
            "AfterCatch");

        validator.TagStates("InFirstCatch").Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
        ValidateHasOnlyUnknownExceptionAndSystemException(validator, "InSecondCatch");
        validator.TagStates("InThirdCatch").Should().BeEmpty();
        validator.TagStates("AfterCatch").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Throw_TryCatch_ThrowInCatch_SingleCatchBlock()
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
    public void Throw_TryCatchFinally_ThrowInTry()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    Tag(""InTry"");
    throw new System.Exception();
    tag = ""UnreachableInTry"";
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
            "AfterFinally");

        ValidateHasOnlyUnknownExceptionAndSystemException(validator, "InCatch");
        validator.TagStates("InFinally").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
        validator.TagStates("AfterFinally").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Throw_TryCatchFinally_ThrowInCatch()
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
            .And.ContainSingle(x => HasSystemException(x));

        validator.TagStates("AfterFinally").Should().HaveCount(1)
            .And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Throw_TryCatchFinally_ThrowInFinally()
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
    tag = ""UnreachableInFinally"";
}
tag = ""UnreachableAfterFinally"";";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InCatch",
            "InFinally");

        validator.TagStates("InCatch").Should().HaveCount(1)
                 .And.ContainSingle(x => HasUnknownException(x));

        validator.TagStates("InFinally").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Throw_TryCatch_NestedThrowWithCatchFilter()
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
            "InNestedTry",
            "InNestedCatch",
            "After");
    }

    [TestMethod]
    public void Throw_TryCatch_NestedThrowWithNestedCatchFilter()
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
            "InNestedTry",
            "InCatch",
            "After");
    }

    [TestMethod]
    public void Throw_TryCatch_NestedThrowWithMultipleCatchFiltersOnDifferentLevels()
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
        tag = ""UnreachableInNestedCatch"";
    }
}
catch (FormatException)
{
    tag = ""UnreachableInCatch"";
}
tag = ""UnreachableEnd"";";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InNestedTry");
    }

    [TestMethod]
    public void Throw_TryCatch_Throw_CatchThrown_WithVariable()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    throw new FormatException();
    tag = ""UnreachableInTry"";
}
catch (FormatException ex)
{
    tag = ""InCatch"";
}
tag = ""End"";";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InCatch",
            "End");
        validator.TagStates("InCatch").Should().HaveCount(1).And.ContainSingle(x => HasExceptionOfType(x, "FormatException"));
    }

    [TestMethod]
    public void Throw_TryCatch_Throw_CatchThrown_NoVariable()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    throw new FormatException();
    tag = ""UnreachableInTry"";
}
catch (FormatException)
{
    tag = ""InCatch"";
}
tag = ""End"";";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InCatch",
            "End");
    }

    [TestMethod]
    public void Throw_TryCatch_Throw_CatchBaseType()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    throw new System.IO.FileNotFoundException();
    tag = ""UnreachableInTry"";
}
catch (System.IO.IOException)
{
    tag = ""InCatch"";
}
tag = ""End"";";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InCatch",
            "End");
        validator.TagStates("InCatch").Should().HaveCount(1).And.ContainSingle(x => HasExceptionOfType(x, "FileNotFoundException"));
    }

    [TestMethod]
    public void Throw_TryCatch_Throw_CatchSpecificTypeAndBaseTypeAnd()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    throw new System.IO.FileNotFoundException();
    tag = ""UnreachableInTry"";
}
catch (System.IO.FileNotFoundException)
{
    tag = ""InCatchSpecific"";
}
catch (System.IO.IOException)
{
    tag = ""InCatchBase"";
}
tag = ""End"";";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InCatchSpecific",
            "End");
        validator.TagStates("InCatchBase").Should().BeEmpty();
        validator.TagStates("InCatchSpecific").Should().HaveCount(1).And.ContainSingle(x => HasExceptionOfType(x, "FileNotFoundException"));
    }

    [TestMethod]
    public void Throw_TryCatch_Throw_CatchBaseTypeAndSpecificType_WithWhenCondition()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    throw new System.IO.FileNotFoundException();
    tag = ""UnreachableInTry"";
}
catch (System.IO.FileNotFoundException) when (condition)
{
    tag = ""InCatchSpecificWithCondition"";
}
catch (System.IO.FileNotFoundException)
{
    tag = ""InCatchSpecificNoCondition"";
}
catch (System.IO.IOException)
{
    tag = ""InCatchBase"";
}
tag = ""End"";";
        var validator = SETestContext.CreateCS(code, "bool condition").Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InCatchSpecificNoCondition",
            "InCatchSpecificWithCondition",
            "End");
        validator.TagStates("InCatchBase").Should().BeEmpty();
        validator.TagStates("InCatchSpecificWithCondition").Should().HaveCount(1).And.ContainSingle(x => HasExceptionOfType(x, "FileNotFoundException"));
        validator.TagStates("InCatchSpecificNoCondition").Should().HaveCount(1).And.ContainSingle(x => HasExceptionOfType(x, "FileNotFoundException"));
    }

    [TestMethod]
    public void Throw_TryCatch_Throw_CatchBaseTypeAndSpecificType_WithWhenCondition_Nested()
    {
        const string code = """
            var tag = "BeforeTry";
            try
            {
                try
                {
                    tag = "InTry";
                    throw new System.IO.FileNotFoundException();
                    tag = "UnreachableInTry";
                }
                catch (System.IO.FileNotFoundException) when (condition)
                {
                    tag = "InCatchSpecificWithCondition";
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                tag = "InCatchSpecificNoCondition";
            }
            catch (System.IO.IOException)
            {
                tag = "InCatchBase";
            }
            tag = "End";
            """;
        var validator = SETestContext.CreateCS(code, "bool condition").Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InCatchSpecificNoCondition",
            "InCatchSpecificWithCondition",
            "End");
        validator.TagStates("InCatchBase").Should().BeEmpty();
        validator.TagStates("InCatchSpecificWithCondition").Should().HaveCount(1).And.ContainSingle(x => HasExceptionOfType(x, "FileNotFoundException"));
        validator.TagStates("InCatchSpecificNoCondition").Should().HaveCount(1).And.ContainSingle(x => HasExceptionOfType(x, "FileNotFoundException"));
    }

    [TestMethod]
    public void Throw_TryCatch_Throw_CatchAllWhen_IsTrue()
    {
        const string code = @"
var tag = ""BeforeTry"";
var isTrue = true;
try
{
    tag = ""InTry"";
    throw new FormatException();
    tag = ""UnreachableInTry"";
}
catch when (isTrue)
{
    tag = ""InCatch"";
}
tag = ""End"";";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InCatch",
            "End");
    }

    [TestMethod]
    public void Throw_TryCatch_Throw_CatchAllWhen_IsFalse()
    {
        const string code = @"
var tag = ""BeforeTry"";
var isFalse = false;
try
{
    tag = ""InTry"";
    throw new FormatException();
    tag = ""UnreachableInTry"";
}
catch when (isFalse)
{
    tag = ""UnreachableInCatch"";
}
tag = ""UnreachableEnd"";";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry");
    }

    [TestMethod]
    public void Throw_TryCatch_Throw_CatchAllWhen_IsUnknown()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    throw new FormatException();
    tag = ""UnreachableInTry"";
}
catch when (arg)
{
    tag = ""InCatch"";
}
tag = ""End"";";
        var validator = SETestContext.CreateCS(code, "bool arg").Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InCatch",
            "End");
    }

    [TestMethod]
    public void Throw_TryCatch_Throw_CatchDoesNotDowncast()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    throw couldBeAnything;
    tag = ""UnreachableInTry"";
}
catch (FormatException)
{
    tag = ""UnreachableInCatch"";
}
tag = ""UnreachableEnd"";";
        var validator = SETestContext.CreateCS(code, "Exception couldBeAnything").Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry");   // Signature returns Exception => we do not know that it is FormatException
    }

    [TestMethod]
    public void Throw_TryCatch_ThrowUnexpectedException()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    throw new FormatException();
    tag = ""UnreachableInTry"";
}
catch (NotSupportedException)
{
    tag = ""UnreachableInCatch"";
}
tag = ""Unreachable"";";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry");
    }

    [TestMethod]
    public void Throw_TryCatch_NestedThrow_OuterCatch()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry1"";
    try
    {
        tag = ""InTry2"";
        try
        {
            tag = ""InTry3"";
            throw new FormatException();
            tag = ""UnreachableInTry3"";
        }
        catch (NotSupportedException)
        {
            tag = ""UnreachableInCatch3"";
        }
        tag = ""UnreachableInTry2"";
    }
    catch (NotImplementedException)
    {
        tag = ""UnreachableInCatch2"";
    }
    tag = ""UnreachableInTry1"";
}
catch (FormatException)
{
    tag = ""InCatch"";
}
tag = ""End"";";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry1",
            "InTry2",
            "InTry3",
            "InCatch",
            "End");
        validator.ExitStates.Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Throw_TryCatch_NestedThrow_UnexpectedException()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry1"";
    try
    {
        tag = ""InTry2"";
        try
        {
            tag = ""InTry3"";
            throw new System.IO.IOException();
            tag = ""UnreachableInTry3"";
        }
        catch (NotSupportedException)
        {
            tag = ""UnreachableInCatch3"";
        }
        tag = ""UnreachableInTry2"";
    }
    catch (NotImplementedException)
    {
        tag = ""UnreachableInCatch2"";
    }
    tag = ""UnreachableInTry1"";
}
catch (FormatException)
{
    tag = ""InCatch"";
}
tag = ""End"";";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry1",
            "InTry2",
            "InTry3");
        validator.ExitStates.Should().HaveCount(1).And.ContainSingle(x => HasExceptionOfType(x, "IOException"));
    }

    [TestMethod]
    public void Throw_Null_UnexpectedException()
    {
        const string code = "throw null;";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ExitStates.Should().HaveCount(1).And.ContainSingle(x => HasUnknownException(x));
    }
}

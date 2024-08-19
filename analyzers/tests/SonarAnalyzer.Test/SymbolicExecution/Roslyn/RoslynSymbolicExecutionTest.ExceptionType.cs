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
    public void Exception_FieldReference()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    throw fieldException;
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
            "InCatch",
            "AfterCatch");

        validator.TagStates("InCatch").Should().HaveCount(1).And.ContainSingle(x => HasExceptionOfType(x, "NotImplementedException"));
        validator.TagStates("AfterCatch").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Exception_LocalReference()
    {
        const string code = @"
NotImplementedException exception = null;
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    throw exception;
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
            "InCatch",
            "AfterCatch");

        validator.TagStates("InCatch").Should().HaveCount(1).And.ContainSingle(x => HasExceptionOfType(x, "NotImplementedException"));
        validator.TagStates("AfterCatch").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Exception_PropertyReference()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    throw PropertyException;
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
            "InCatch",
            "AfterCatch");

        validator.TagStates("InCatch").Should().HaveCount(1).And.ContainSingle(x => HasExceptionOfType(x, "NotImplementedException"));
        validator.TagStates("AfterCatch").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Exception_ParameterReference()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    throw ex;
}
catch
{
    tag = ""InCatch"";
}
tag = ""AfterCatch"";";
        var validator = SETestContext.CreateCS(code, "NotImplementedException ex").Validator;
        validator.ValidateTagOrder(
            "BeforeTry",
            "InTry",
            "InCatch",
            "AfterCatch");

        validator.TagStates("InCatch").Should().HaveCount(1).And.ContainSingle(x => HasExceptionOfType(x, "NotImplementedException"));
        validator.TagStates("AfterCatch").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Exception_ArrayElementReference()
    {
        const string code = @"
NotImplementedException[] exceptions = null;
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    throw exceptions[0];
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
            "InCatch",
            "InCatch",
            "AfterCatch");

        validator.TagStates("InCatch").Should().HaveCount(2)
                 .And.ContainSingle(x => HasExceptionOfType(x, "NotImplementedException"))
                 .And.ContainSingle(x => HasExceptionOfType(x, "IndexOutOfRangeException"));
        validator.TagStates("AfterCatch").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Exception_MethodInvocation()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    throw CreateException();
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
            "InCatch", // In case CreateException throws
            "InCatch", // throw
            "AfterCatch");

        validator.TagStates("InCatch").Should().HaveCount(2)
                 .And.ContainSingle(x => HasExceptionOfType(x, "NotImplementedException"))
                 .And.ContainSingle(x => HasUnknownException(x));
        validator.TagStates("AfterCatch").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Exception_Conversion()
    {
        const string code = @"
var tag = ""BeforeTry"";
var obj = new ArgumentNullException();
try
{
    tag = ""InTry"";
    throw obj;
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
            "InCatch",
            "AfterCatch");

        validator.TagStates("InCatch").Should().HaveCount(1).And.ContainSingle(x => HasExceptionOfType(x, "ArgumentNullException"));
        validator.TagStates("AfterCatch").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void Exception_MultipleConversion()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    throw (System.IO.IOException) new System.IO.FileNotFoundException();
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
            "InCatch",
            "AfterCatch");

        validator.TagStates("InCatch").Should().HaveCount(1).And.ContainSingle(x => HasExceptionOfType(x, "FileNotFoundException"));
        validator.TagStates("AfterCatch").Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }
}

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
    public void IsNull_Coalesce_SetsObjectConstraint()
    {
        const string code = @"
object nullValue = null;
object notNullValue = new object();
var nullToNull = nullValue ?? nullValue;
var nullToNotNull = nullValue ?? notNullValue;
var nullToUnknown = nullValue ?? arg;
var notNullToNull = notNullValue ?? nullValue;
var notNullToNotNull = notNullValue ?? notNullValue;
var notNullToUnknown = notNullValue ?? arg;
Tag(""NullToNull"", nullToNull);
Tag(""NullToNotNull"", nullToNotNull);
Tag(""NullToUnknown"", nullToUnknown);
Tag(""NotNullToNull"", notNullToNull);
Tag(""NotNullToNotNull"", notNullToNotNull);
Tag(""NotNullToUnknown"", notNullToUnknown);";
        var validator = SETestContext.CreateCS(code, "object arg").Validator;
        validator.ValidateContainsOperation(OperationKind.IsNull);
        validator.TagValue("NullToNull").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagValue("NullToNotNull").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("NullToUnknown").Should().HaveNoConstraints();
        validator.TagValue("NotNullToNull").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("NotNullToNotNull").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("NotNullToUnknown").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void IsNull_Coalesce_UnknownToNull()
    {
        const string code = """
                object nullValue = null;
                var result = arg ?? nullValue;
                Tag("End");
                """;
        var validator = SETestContext.CreateCS(code, "object arg", new PreserveTestCheck("arg", "result")).Validator;
        var arg = validator.Symbol("arg");
        var result = validator.Symbol("result");
        validator.ValidateContainsOperation(OperationKind.IsNull);
        validator.TagStates("End").Should().SatisfyRespectively(
            x =>
            {
                x[arg].Should().HaveOnlyConstraint(ObjectConstraint.Null);
                x[result].Should().HaveOnlyConstraint(ObjectConstraint.Null);   // It's from nullValue
            },
            x =>
            {
                x[arg].Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
                x[result].Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
            });
    }

    [TestMethod]
    public void IsNull_Coalesce_UnknownToNotNull()
    {
        const string code = """
                object notNullValue = new object();
                var result = arg ?? notNullValue;
                Tag("End");
                """;
        var validator = SETestContext.CreateCS(code, "object arg", new PreserveTestCheck("arg", "result")).Validator;
        var arg = validator.Symbol("arg");
        var result = validator.Symbol("result");
        validator.ValidateContainsOperation(OperationKind.IsNull);
        validator.TagStates("End").Should().SatisfyRespectively(
            x =>
            {
                x[arg].Should().HaveOnlyConstraint(ObjectConstraint.Null);
                x[result].Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
            },
            x =>
            {
                x[arg].Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
                x[result].Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
            });
    }

    [TestMethod]
    public void IsNull_Coalesce_UnknownToNotNull_WithConversion()
    {
        const string code = """
                var notNullValue = new ArgumentException();
                var result = arg ?? notNullValue;
                Tag("End");
                """;
        var validator = SETestContext.CreateCS(code, "Exception arg", new PreserveTestCheck("arg", "result")).Validator;
        var arg = validator.Symbol("arg");
        var result = validator.Symbol("result");
        validator.ValidateContainsOperation(OperationKind.IsNull);
        validator.TagStates("End").Should().SatisfyRespectively(
            x =>
            {
                x[arg].Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
                x[result].Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
            },
            x =>
            {
                x[arg].Should().HaveOnlyConstraint(ObjectConstraint.Null);
                x[result].Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
            });
    }

    [TestMethod]
    public void IsNull_Coalesce_UnknownToUnknown()
    {
        const string code = @"
var unknownToUnknown = arg1 ?? arg2;
Tag(""UnknownToUnknown"", unknownToUnknown);";
        var validator = SETestContext.CreateCS(code, "object arg1, object arg2").Validator;
        validator.ValidateContainsOperation(OperationKind.IsNull);
        validator.TagValues("UnknownToUnknown").Should().SatisfyRespectively(
            x => x.Should().HaveNoConstraints(),
            x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
    }

    [TestMethod]
    public void IsNull_Coalesce_AssignedToVariable()
    {
        const string code = """
                var value = arg ?? "N/A";
                Tag("Value", value);
                """;
        var validator = SETestContext.CreateCS(code, "string arg").Validator;
        validator.TagValue("Value").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void IsNull_Coalesce_AssignedToSelf()
    {
        const string code = """
                arg = arg ?? "N/A";
                Tag("Arg", arg);
                """;
        var validator = SETestContext.CreateCS(code, "string arg").Validator;
        validator.TagValue("Arg").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void IsNull_CoalesceAssignment_SetsObjectConstraint()
    {
        const string code = @"
object nullValue = null;
object notNullValue = new object();
var nullToNull = nullValue;     // Initial values for coalesce assignment
var nullToNotNull = nullValue;
var nullToUnknown = nullValue;
var notNullToNull = notNullValue;
var notNullToNotNull = notNullValue;
var notNullToUnknown = notNullValue;
nullToNull ??= nullValue;
nullToNotNull ??= notNullValue;
nullToUnknown ??= arg;
notNullToNull ??= nullValue;
notNullToNotNull ??= notNullValue;
notNullToUnknown ??= arg;
Tag(""NullToNull"", nullToNull);
Tag(""NullToNotNull"", nullToNotNull);
Tag(""NullToUnknown"", nullToUnknown);
Tag(""NotNullToNull"", notNullToNull);
Tag(""NotNullToNotNull"", notNullToNotNull);
Tag(""NotNullToUnknown"", notNullToUnknown);
";
        var validator = SETestContext.CreateCS(code, "object arg").Validator;
        validator.ValidateContainsOperation(OperationKind.IsNull);
        validator.TagValue("NullToNull").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagValue("NullToNotNull").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("NullToUnknown").Should().HaveNoConstraints();
        validator.TagValue("NotNullToNull").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("NotNullToNotNull").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("NotNullToUnknown").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void IsNull_CoalesceAssignment_UnknownToNull()
    {
        const string code = @"
object nullValue = null;
arg ??= nullValue;
Tag(""Arg"", arg);";
        var validator = SETestContext.CreateCS(code, "object arg").Validator;
        validator.ValidateContainsOperation(OperationKind.IsNull);
        validator.TagValues("Arg").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull),
            x => x.Should().HaveOnlyConstraint(ObjectConstraint.Null));
    }

    [TestMethod]
    public void IsNull_CoalesceAssignment_UnknownToNotNull()
    {
        const string code = """
                object notNullValue = new object();
                arg ??= notNullValue;
                Tag("End");
                """;
        var validator = SETestContext.CreateCS(code, "object arg", new PreserveTestCheck("arg", "notNullValue")).Validator;
        validator.ValidateContainsOperation(OperationKind.IsNull);
        var arg = validator.Symbol("arg");
        var notNullValue = validator.Symbol("notNullValue");
        var state = validator.TagStates("End").Should().ContainSingle().Which;
        state[arg].Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        state[notNullValue].Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void IsNull_CoalesceAssignment_UnknownToUnknown()
    {
        const string code = @"
arg1 ??= arg2;
Tag(""Arg"", arg1);";
        var validator = SETestContext.CreateCS(code, "object arg1, object arg2").Validator;
        validator.ValidateContainsOperation(OperationKind.IsNull);
        validator.TagValues("Arg").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull),
            x => x.Should().HaveNoConstraints());
    }

    [TestMethod]
    public void IsNull_ConditionalAccess_OnNull()
    {
        const string code = @"
Sample nullValue = null;
nullValue?.Tag(""Unreachable - this should not be invoked on null"");
Tag(""End"");";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKind.IsNull);
        validator.ValidateTagOrder("End");
    }

    [TestMethod]
    public void IsNull_ConditionalAccess_OnNotNull()
    {
        const string code = @"
Sample notNullValue = new();
notNullValue?.Tag(""WasNotNull"");
Tag(""End"");";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKind.IsNull);
        validator.ValidateTagOrder("WasNotNull", "End");
    }

    [TestMethod]
    public void IsNull_ConditionalAccess_OnUnknown()
    {
        const string code = @"
arg?.Tag(""WasNotNull"");
Tag(""End"", arg);";
        var validator = SETestContext.CreateCS(code, "Sample arg").Validator;
        validator.ValidateContainsOperation(OperationKind.IsNull);
        validator.ValidateTagOrder(
            "End",
            "WasNotNull",
            "End");
    }

    [TestMethod]
    public void IsNull_TryCast_DownCast()
    {
        var validator = SETestContext.CreateCS("""
            (arg as Exception)?.ToString();
            Tag("Arg", arg);
            """, "object arg").Validator;
        validator.TagValue("Arg").Should().BeNull();
    }

    [TestMethod]
    public void IsNull_TryCast_DownCast_KeepsConstraint()
    {
        var validator = SETestContext.CreateCS("""
            arg.ToString();
            (arg as Exception)?.ToString();
            Tag("Arg", arg);
            """, "object arg").Validator;
        validator.TagValue("Arg").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void IsNull_TryCast_UpCast()
    {
        var validator = SETestContext.CreateCS("""
                (arg as Exception)?.ToString();
                Tag("Arg", arg);
                """, "ArgumentException arg").Validator;
        validator.TagValues("Arg").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraint(ObjectConstraint.Null),
            x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
    }
}

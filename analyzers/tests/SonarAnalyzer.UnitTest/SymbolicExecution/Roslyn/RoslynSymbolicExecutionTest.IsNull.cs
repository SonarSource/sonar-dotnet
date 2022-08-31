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
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
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
            var validator = SETestContext.CreateCS(code, ", object arg").Validator;
            validator.ValidateContainsOperation(OperationKind.IsNull);
            validator.ValidateTag("NullToNull", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.ValidateTag("NullToNotNull", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("NullToUnknown", x => x.Should().BeNull());
            validator.ValidateTag("NotNullToNull", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("NotNullToNotNull", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("NotNullToUnknown", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
        }

        [TestMethod]
        public void IsNull_Coalesce_UnknownToNull()
        {
            const string code = @"
object nullValue = null;
var unknownToNull = arg ?? nullValue;
Tag(""UnknownToNull"", unknownToNull);";
            var validator = SETestContext.CreateCS(code, ", object arg").Validator;
            validator.ValidateContainsOperation(OperationKind.IsNull);
            validator.TagValues("UnknownToNull").Should().HaveCount(2)
                .And.ContainSingle(x => x == null)
                .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.Null));
        }

        [TestMethod]
        public void IsNull_Coalesce_UnknownToNotNull()
        {
            const string code = @"
object notNullValue = new object();
var unknownToNotNull = arg ?? notNullValue;
Tag(""UnknownToNotNull"", unknownToNotNull);";
            var validator = SETestContext.CreateCS(code, ", object arg").Validator;
            validator.ValidateContainsOperation(OperationKind.IsNull);
            validator.TagValues("UnknownToNotNull").Should().HaveCount(2)
                .And.ContainSingle(x => x == null)
                .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
        }

        [TestMethod]
        public void IsNull_Coalesce_UnknownToUnknown()
        {
            const string code = @"
var unknownToUnknown = arg1 ?? arg2;
Tag(""UnknownToUnknown"", unknownToUnknown);";
            var validator = SETestContext.CreateCS(code, ", object arg1, object arg2").Validator;
            validator.ValidateContainsOperation(OperationKind.IsNull);
            validator.TagValues("UnknownToUnknown").Should().HaveCount(1)
                .And.ContainSingle(x => x == null);
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
            var validator = SETestContext.CreateCS(code, ", object arg").Validator;
            validator.ValidateContainsOperation(OperationKind.IsNull);
            validator.ValidateTag("NullToNull", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.ValidateTag("NullToNotNull", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("NullToUnknown", x => x.Should().BeNull());
            validator.ValidateTag("NotNullToNull", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("NotNullToNotNull", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("NotNullToUnknown", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
        }

        [TestMethod]
        public void IsNull_CoalesceAssignment_UnknownToNull()
        {
            const string code = @"
object nullValue = null;
arg ??= nullValue;
Tag(""Arg"", arg);";
            var validator = SETestContext.CreateCS(code, ", object arg").Validator;
            validator.ValidateContainsOperation(OperationKind.IsNull);
            validator.TagValues("Arg").Should().HaveCount(2)
                .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull))
                .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null));
        }

        [TestMethod]
        public void IsNull_CoalesceAssignment_UnknownToNotNull()
        {
            const string code = @"
object notNullValue = new object();
arg ??= notNullValue;
Tag(""Arg"", arg);";
            var validator = SETestContext.CreateCS(code, ", object arg").Validator;
            validator.ValidateContainsOperation(OperationKind.IsNull);
            validator.TagValues("Arg").Should().HaveCount(2)
                .And.OnlyContain(x => x.HasConstraint(ObjectConstraint.NotNull));
        }

        [TestMethod]
        public void IsNull_CoalesceAssignment_UnknownToUnknown()
        {
            const string code = @"
arg1 ??= arg2;
Tag(""Arg"", arg1);";
            var validator = SETestContext.CreateCS(code, ", object arg1, object arg2").Validator;
            validator.ValidateContainsOperation(OperationKind.IsNull);
            validator.TagValues("Arg").Should().HaveCount(2)
                .And.ContainSingle(x => x == null)
                .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
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
            var validator = SETestContext.CreateCS(code, ", Sample arg").Validator;
            validator.ValidateContainsOperation(OperationKind.IsNull);
            validator.ValidateTagOrder(
                "WasNotNull",
                "End",
                "End");
        }
    }
}

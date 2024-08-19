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
    [DataRow("42", "42")]
    [DataRow("(string)null", "(string)null")]
    [DataRow("false", "true")]
    public void BinaryTuple_Equals_LearnsBoolConstraintOnEquality(string value1, string value2)
    {
        var code = $$"""
            var value1 = {{value1}};
            var value2 = {{value2}};
            bool equals = (value1, value2) == ({{value1}}, {{value2}});
            Tag("Equals", equals);
            bool notEquals = (value1, value2) != ({{value1}}, {{value2}});
            Tag("NotEquals", notEquals);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Equals").Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull);
        validator.TagValue("NotEquals").Should().HaveOnlyConstraints(BoolConstraint.False, ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void BinaryTuple_Equals_LearnsConstraintsAboutTupleMembers()
    {
        var code = $$"""
            if ((intArg, stringArg, boolArg) == (42, null, true))
            {
                Tag("EqualsTrue_IntArg", intArg);
                Tag("EqualsTrue_StringArg", stringArg);
                Tag("EqualsTrue_BoolArg", boolArg);
            }
            else
            {
                Tag("EqualsFalse_IntArg", intArg);
                Tag("EqualsFalse_StringArg", stringArg);
                Tag("EqualsFalse_BoolArg", boolArg);
            }

            if ((intArg, stringArg, boolArg) != (42, null, true))
            {
                Tag("NotEqualsTrue_IntArg", intArg);
                Tag("NotEqualsTrue_StringArg", stringArg);
                Tag("NotEqualsTrue_BoolArg", boolArg);
            }
            else
            {
                Tag("NotEqualsFalse_IntArg", intArg);
                Tag("NotEqualsFalse_StringArg", stringArg);
                Tag("NotEqualsFalse_BoolArg", boolArg);
            }
            """;
        var validator = SETestContext.CreateCS(code, "int intArg, string stringArg, bool boolArg").Validator;

        validator.TagValue("EqualsTrue_IntArg").Should().HaveOnlyConstraints(NumberConstraint.From(42, 42), ObjectConstraint.NotNull);
        validator.TagValue("EqualsTrue_StringArg").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagValue("EqualsTrue_BoolArg").Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull);

        validator.TagValue("EqualsFalse_IntArg").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("EqualsFalse_StringArg").Should().HaveNoConstraints();
        validator.TagValue("EqualsFalse_BoolArg").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);

        validator.TagValue("NotEqualsTrue_IntArg").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("NotEqualsTrue_StringArg").Should().HaveNoConstraints();
        validator.TagValue("NotEqualsTrue_BoolArg").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);

        validator.TagValue("NotEqualsFalse_IntArg").Should().HaveOnlyConstraints(NumberConstraint.From(42, 42), ObjectConstraint.NotNull);
        validator.TagValue("NotEqualsFalse_StringArg").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagValue("NotEqualsFalse_BoolArg").Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull);
    }
}

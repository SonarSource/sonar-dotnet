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
    public void CompoundAssignment_UntrackedSymbol()
    {
        const string code = """
            var result = arg.Property += 1;
            Tag("Result", result);
            """;
        var validator = SETestContext.CreateCS(code, "Sample arg").Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void CompoundAssignment_TrackedSymbolOnRightSide()
    {
        const string code = """
            var left = 40;
            var right = 2;
            var result = left += right;
            Tag("Left", left);
            Tag("Right", right);
            Tag("Result", result);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Left").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(42));
        validator.TagValue("Right").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(2));
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(42));
    }

    [TestMethod]
    public void CompoundAssignment_SelfUpdate()
    {
        const string code = """
            var value = 21;
            var result = value += value;
            Tag("Result", result);
            Tag("Value", value);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(42));
        validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(42));
    }

    [TestMethod]
    public void CompoundAssignment_TargetAndValueUnknown_DoesNotLearn()
    {
        var validator = SETestContext.CreateCS("""
            var result = target += value;
            Tag("Target", target);
            Tag("Result", result);
            """,
            "int? target, int? value").Validator;
        validator.TagValue("Target").Should().HaveNoConstraints();
        validator.TagValue("Result").Should().HaveNoConstraints();
    }

    [TestMethod]
    public void CompoundAssignment_ValueNull_UnlearnTargetConstraints()
    {
        var validator = SETestContext.CreateCS("""
            int? target = 42;
            var result = target += null;
            Tag("Target", target);
            Tag("Result", result);
            """).Validator;
        validator.TagValue("Target").Should().HaveNoConstraints();
        validator.TagValue("Result").Should().HaveNoConstraints();
    }

    [DataTestMethod]
    [DataRow("String.Empty")]
    [DataRow("null")]
    [DataRow("Unknown<string>()")]
    public void CompoundAssignment_OnString_AlwaysLearnNotNull(string stringValue)
    {
        var validator = SETestContext.CreateCS($"""
            string s = {stringValue};
            var result = s += null;
            Tag("S", s);
            Tag("Result", result);
            """).Validator;
        validator.TagValue("S").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Result").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void CompoundAssignment_OnValueType_AlwaysLearnNotNull()
    {
        var validator = SETestContext.CreateCS("""
            char c = 'a';
            var result = c += Unknown<char>();
            Tag("C", c);
            Tag("Result", result);
            """).Validator;
        validator.TagValue("C").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Result").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }
}

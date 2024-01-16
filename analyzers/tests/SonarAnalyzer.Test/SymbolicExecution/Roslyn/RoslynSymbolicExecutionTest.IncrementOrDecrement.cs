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
    public void IncrementOrDecrement_UntrackedSymbol()
    {
        const string code = """
            arg.Property = 1;   // Not tracked anyway
            var result = arg.Property++;
            Tag("Target", arg.Property);
            Tag("Result", result);
            """;
        var validator = SETestContext.CreateCS(code, "Sample arg").Validator;
        validator.TagValue("Target").Should().HaveNoConstraints();
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void IncrementOrDecrement_UntrackedSymbol_WithValue()
    {
        const string code = """
            var result = ++arg.Property;    // For coverage: This will get value even for untracked symbol
            Tag("Result", result);
            """;
        var setter = new PostProcessTestCheck(OperationKind.PropertyReference, x => x.SetOperationConstraint(NumberConstraint.From(10)));
        var validator = SETestContext.CreateCS(code, "Sample arg", setter).Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(11));
    }

    [TestMethod]
    public void IncrementOrDecrement_UnknownValue()
    {
        const string code = """
            var result = arg++;
            Tag("Arg", arg);
            Tag("Result", result);
            """;
        var validator = SETestContext.CreateCS(code, "int arg").Validator;
        validator.TagValue("Arg").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("target++", 2, 1)]
    [DataRow("target--", 0, 1)]
    [DataRow("++target", 2, 2)]
    [DataRow("--target", 0, 0)]
    public void IncrementOrDecrement_Number(string expression, int expectedTarget, int expectedResult)
    {
        var code = $"""
            var target = 1;
            var result = {expression};
            Tag("Target", target);
            Tag("Result", result);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Target").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedTarget));
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedResult));
    }

    [DataTestMethod]
    [DataRow("onlyMin++; onlyMax++;")]
    [DataRow("++onlyMin; ++onlyMax;")]
    public void IncrementOrDecrement_Range_Increment(string expressions)
    {
        var code = $$"""
            if (onlyMin > 0 && onlyMax < 100)   // Scaffold ranges
            {
                {{expressions}}
                Tag("OnlyMin", onlyMin);
                Tag("OnlyMax", onlyMax);
            }
            """;
        var validator = SETestContext.CreateCS(code, "int onlyMin, int onlyMax").Validator;
        validator.TagValue("OnlyMin").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(2, null));
        validator.TagValue("OnlyMax").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(null, 100));
    }

    [DataTestMethod]
    [DataRow("onlyMin--; onlyMax--;")]
    [DataRow("--onlyMin; --onlyMax;")]
    public void IncrementOrDecrement_Range_Decrement(string expressions)
    {
        var code = $$"""
            if (onlyMin > 0 && onlyMax < 100)   // Scaffold ranges
            {
                {{expressions}}
                Tag("OnlyMin", onlyMin);
                Tag("OnlyMax", onlyMax);
            }
            """;
        var validator = SETestContext.CreateCS(code, "int onlyMin, int onlyMax").Validator;
        validator.TagValue("OnlyMin").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(0, null));
        validator.TagValue("OnlyMax").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(null, 98));
    }
}

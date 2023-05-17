/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn;

public partial class RoslynSymbolicExecutionTest
{
    [DataTestMethod]
    [DataRow("+=")]
    [DataRow("-=")]
    [DataRow("*=")]
    [DataRow("/=")]
    [DataRow("%=")]
    [DataRow("&=")]
    [DataRow("|=")]
    [DataRow("^=")]
    [DataRow("<<=")]
    [DataRow(">>=")]
    [DataRow(">>>=")]
    public void CompoundAssignment_Arithmetics(string op)
    {
        var code = $"""
            var value = 42;
            var result = value {op} 1;
            Tag("Result", result);
            Tag("Value", value);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTag("Result", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull));
        validator.ValidateTag("Value", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("&=")]
    [DataRow("|=")]
    [DataRow("^=")]
    public void CompoundAssignment_Logical(string op)
    {
        var code = $"""
            var value = true;
            var result = value {op} true;
            Tag("Result", result);
            Tag("Value", value);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTag("Result", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull));
        validator.ValidateTag("Value", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull));
    }

}

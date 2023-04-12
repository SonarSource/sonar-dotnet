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
using SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.Rules;

[TestClass]
public class ParameterReassignedConstraintTest
{
    [DataTestMethod]
    [DataRow(@"arg = Unknown<object>();", "object")]
    [DataRow(@"arg = Unknown<List<object>>();", "List<object>")]
    [DataRow(@"arg = Unknown<int>();", "int")]
    [DataRow(@"arg = Unknown<int?>();", "int?")]
    [DataRow(@"arg = Unknown<object>();", "ref object")]
    [DataRow(@"arg = Unknown<object>();", "out object")]
    [DataRow(@"arg = Unknown<object[]>();", "params object[]")]
    [DataRow(@"arg = Unknown<object>(); arg.ToString();", "object")]
    [DataRow(@"arg = Unknown<object>(); arg.ToString();", "object")]
    [DataRow(@"arg = Unknown<object>(); arg = Unknown<object>();", "object")]
    public void ParameterReassignedConstraint_AfterAssignment(string methodSnippet, string argumentType)
    {
        var methodBody = $@"{methodSnippet} Tag(""AfterAssignment"", arg);";
        var argumentSnippet = $", {argumentType} arg";
        var validator = SETestContext.CreateCS(methodBody, argumentSnippet, new PublicMethodArgumentsShouldBeCheckedForNull()).Validator;
        validator.ValidateTag("AfterAssignment", x => x.HasConstraint<ParameterReassignedConstraint>().Should().BeTrue());
    }

    [TestMethod]
    public void ParameterReassignedConstraint_InsideCondition()
    {
        const string snippet = """
            if (arg == null)
            {
                arg = Unknown<object>();
                Tag("AfterAssignment", arg);
            }
            else
            {
                Tag("NoAssignment", arg);
            }
            """;
        var validator = SETestContext.CreateCS(snippet, ", object arg", new PublicMethodArgumentsShouldBeCheckedForNull()).Validator;
        validator.ValidateTag("AfterAssignment", x => x.HasConstraint<ParameterReassignedConstraint>().Should().BeTrue());
        validator.ValidateTag("NoAssignment", x => x.HasConstraint<ParameterReassignedConstraint>().Should().BeFalse());
    }

    [TestMethod]
    public void ParameterReassignedConstraint_WithMultipleParameters()
    {
        const string snippet = """
            arg1 = Unknown<object>();
            Tag("AfterAssignmentArg1", arg1);
            Tag("AfterAssignmentArg2", arg2);
            """;
        var validator = SETestContext.CreateCS(snippet, ", object arg1, object arg2", new PublicMethodArgumentsShouldBeCheckedForNull()).Validator;
        validator.ValidateTag("AfterAssignmentArg1", x => x.HasConstraint<ParameterReassignedConstraint>().Should().BeTrue());
        validator.ValidateTag("AfterAssignmentArg2", x => x.Should().Match(c => c == null || !c.HasConstraint<ParameterReassignedConstraint>()));
    }

    [DataTestMethod]
    [DataRow(@"var local = Unknown<object>(); Tag(""AfterAssignment"", local);")]
    [DataRow(@"ObjectField = Unknown<object>(); Tag(""AfterAssignment"", ObjectField);")]
    [DataRow(@"FullProperty = Unknown<object>(); Tag(""AfterAssignment"", FullProperty);")]
    public void PublicMethodArgumentsShouldBeCheckedForNull_IgnoredAssignments_State_Roslyn_CS(string snippet)
    {
        var validator = SETestContext.CreateCS(snippet, new PublicMethodArgumentsShouldBeCheckedForNull()).Validator;
        validator.ValidateTag("AfterAssignment", x => x.Should().Match(c => c == null || !c.HasConstraint<ParameterReassignedConstraint>()));
    }
}

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

using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn;

public partial class RoslynSymbolicExecutionTest
{
    [DataTestMethod]
    [DataRow("""arg = Unknown<object>();""", "object")]
    [DataRow("""arg = Unknown<List<object>>();""", "List<object>")]
    [DataRow("""arg = Unknown<int?>();""", "int?")]
    [DataRow("""arg = Unknown<object>();""", "ref object")]
    [DataRow("""arg = Unknown<object>();""", "out object")]
    [DataRow("""arg = Unknown<object[]>();""", "params object[]")]
    public void ParameterReassignedConstraint_AfterAssignment(string methodSnippet, string argumentType)
    {
        var methodBody = $$"""
            {{methodSnippet}}
            Tag("End", arg);
            """;
        var argumentSnippet = $", {argumentType} arg";
        var validator = SETestContext.CreateCS(methodBody, argumentSnippet, new PublicMethodArgumentsShouldBeCheckedForNull()).Validator;
        validator.ValidateTag("End", x => x.Should().HaveOnlyConstraints(ParameterReassignedConstraint.Instance));
    }

    [TestMethod]
    public void ParameterReassignedConstraint_AfterAssignmentWithValueType()
    {
        var methodBody = $$"""
            arg = Unknown<int>();
            Tag("End", arg);
            """;
        var validator = SETestContext.CreateCS(methodBody, ", int arg", new PublicMethodArgumentsShouldBeCheckedForNull()).Validator;
        validator.ValidateTag("End", x => x.Should().HaveOnlyConstraints(ParameterReassignedConstraint.Instance, ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("""ObjectField = null;""")]
    [DataRow("""FullProperty = null;""")]
    [DataRow("""AutoProperty = null;""")]
    [DataRow("""arg = Unknown<object>();""")]
    [DataRow("""InstanceMethod(arg);""")]
    [DataRow("""ObjectField.ToString();""")]
    public void ParameterReassignedConstraint_AdditionalOperations(string snippet)
    {
        var methodBody = $$"""
            arg = Unknown<object>();
            {{snippet}}
            Tag("End", arg);
            """;
        var validator = SETestContext.CreateCS(methodBody, ", object arg", new PublicMethodArgumentsShouldBeCheckedForNull()).Validator;
        validator.ValidateTag("End", x => x.Should().HaveOnlyConstraints(ParameterReassignedConstraint.Instance));
    }

    [TestMethod]
    public void ParameterReassignedConstraint_AdditionalMethodInvocation()
    {
        var methodBody = """
            arg = Unknown<object>();
            arg.ToString();
            Tag("End", arg);
            """;
        var validator = SETestContext.CreateCS(methodBody, ", object arg", new PublicMethodArgumentsShouldBeCheckedForNull()).Validator;
        validator.ValidateTag("End", x => x.Should().HaveOnlyConstraints(ParameterReassignedConstraint.Instance, ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("""InstanceMethodWithRefParam(ref arg);""")]
    [DataRow("""InstanceMethodWithOutParam(out arg);""")]
    public void ParameterReassignedConstraint_PreservedAfterPassedAsRefOrOutParameter(string snippet)
    {
        var methodBody = $$"""
            arg = Unknown<object>();
            {{snippet}}
            Tag("End", arg);
            """;
        var validator = SETestContext.CreateCS(methodBody, ", object arg", new PublicMethodArgumentsShouldBeCheckedForNull()).Validator;
        validator.ValidateTag("End", x => x.Should().BeNull()); // FIXME: arg should have the ParameterReassignedConstraint
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
        validator.ValidateTag("AfterAssignment", x => x.Should().HaveOnlyConstraints(ParameterReassignedConstraint.Instance));
        validator.ValidateTag("NoAssignment", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull));
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
        validator.ValidateTag("AfterAssignmentArg1", x => x.Should().HaveOnlyConstraints(ParameterReassignedConstraint.Instance));
        validator.ValidateTag("AfterAssignmentArg2", x => x.Should().HaveNoConstraints());
    }

    [DataTestMethod]
    [DataRow("""var local = Unknown<object>(); Tag("AfterAssignment", local);""")]
    [DataRow("""ObjectField = Unknown<object>(); Tag("AfterAssignment", ObjectField);""")]
    [DataRow("""FullProperty = Unknown<object>(); Tag("AfterAssignment", FullProperty);""")]
    public void ParameterReassignedConstraint_IgnoreNonParameters(string snippet)
    {
        var validator = SETestContext.CreateCS(snippet, new PublicMethodArgumentsShouldBeCheckedForNull()).Validator;
        validator.ValidateTag("AfterAssignment", x => x.Should().HaveNoConstraints());
    }

    [TestMethod]
    public void ParameterReassignedConstraint_UnrestrictedGenericParameters() =>
    ValidateAssignmentConstraintForGenericParameter(string.Empty, ParameterReassignedConstraint.Instance);

    [TestMethod]
    public void ParameterReassignedConstraint_ReferenceTypeGenericParameters() =>
        ValidateAssignmentConstraintForGenericParameter("where T: class", ParameterReassignedConstraint.Instance, ObjectConstraint.Null);

    [TestMethod]
    public void ParameterReassignedConstraint_ValueTypeGenericParameters() =>
        ValidateAssignmentConstraintForGenericParameter("where T: struct", ParameterReassignedConstraint.Instance, ObjectConstraint.NotNull);

    private static void ValidateAssignmentConstraintForGenericParameter(string typeConstraint, params SymbolicConstraint[] expectedConstraints)
    {
        var methodCode = $$"""
            public void Main<T>(T arg) {{typeConstraint}}
            {
                arg = default;
                Tag("End", arg);
            }
            """;
        var validator = SETestContext.CreateCSMethod(methodCode, new PublicMethodArgumentsShouldBeCheckedForNull()).Validator;
        validator.ValidateTag("End", x => x.Should().HaveOnlyConstraints(expectedConstraints));
    }
}

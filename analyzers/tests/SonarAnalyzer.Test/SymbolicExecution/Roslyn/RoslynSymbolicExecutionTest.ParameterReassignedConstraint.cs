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

using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;
using SonarAnalyzer.Test.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

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
        var argumentSnippet = $"{argumentType} arg";
        var validator = SETestContext.CreateCS(methodBody, argumentSnippet, new PublicMethodArgumentsShouldBeCheckedForNull()).Validator;
        validator.TagValue("End").Should().HaveOnlyConstraints(ParameterReassignedConstraint.Instance);
    }

    [TestMethod]
    public void ParameterReassignedConstraint_AfterAssignmentWithValueType()
    {
        var methodBody = $$"""
            arg = Unknown<int>();
            Tag("End", arg);
            """;
        var validator = SETestContext.CreateCS(methodBody, "int arg", new PublicMethodArgumentsShouldBeCheckedForNull()).Validator;
        validator.TagValue("End").Should().HaveOnlyConstraints(ParameterReassignedConstraint.Instance, ObjectConstraint.NotNull);
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
        var validator = SETestContext.CreateCS(methodBody, "object arg", new PublicMethodArgumentsShouldBeCheckedForNull()).Validator;
        validator.TagValue("End").Should().HaveOnlyConstraints(ParameterReassignedConstraint.Instance);
    }

    [TestMethod]
    public void ParameterReassignedConstraint_AdditionalMethodInvocation()
    {
        var methodBody = """
            arg = Unknown<object>();
            arg.ToString();
            Tag("End", arg);
            """;
        var validator = SETestContext.CreateCS(methodBody, "object arg", new PublicMethodArgumentsShouldBeCheckedForNull()).Validator;
        validator.TagValue("End").Should().HaveOnlyConstraints(ParameterReassignedConstraint.Instance, ObjectConstraint.NotNull);
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
        var validator = SETestContext.CreateCS(methodBody, "object arg", new PublicMethodArgumentsShouldBeCheckedForNull()).Validator;
        validator.TagValue("End").Should().HaveNoConstraints(); // ToDo: arg should have the ParameterReassignedConstraint
    }

    [TestMethod]
    public void ParameterReassignedConstraint_ReassignedAfterNullCheck()
    {
        var validator = SETestContext.CreateCS("""
            if (arg == null)
            {
                Tag("BeforeReassignment");
                arg = Guid.NewGuid().ToString("N");
                Tag("AfterReassignment");
            }
            else
            {
                Tag("Else");
            }
            Tag("End");
            """, "string arg", new PublicMethodArgumentsShouldBeCheckedForNull(), new PreserveTestCheck("arg")).Validator;
        var arg = validator.Symbol("arg");
        validator.TagStates("BeforeReassignment").Should().ContainSingle().Which[arg].Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagStates("AfterReassignment").Should().ContainSingle().Which[arg].Should().HaveOnlyConstraint(ParameterReassignedConstraint.Instance);
        validator.TagStates("Else").Should().ContainSingle().Which[arg].Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagStates("End").Should().SatisfyRespectively(
            x => x[arg].Should().HaveOnlyConstraint(ObjectConstraint.NotNull),
            x => x[arg].Should().HaveOnlyConstraint(ParameterReassignedConstraint.Instance));
    }

    [TestMethod]
    public void ParameterReassignedConstraint_NullCoalescingAssignment_TakesObjectConstraintFromRightHandSide()
    {
        var validator = SETestContext.CreateCS("""
            arg ??= Guid.NewGuid().ToString("N");
            Tag("End");
            """, "string arg", new PublicMethodArgumentsShouldBeCheckedForNull(), new PreserveTestCheck("arg")).Validator;
        var arg = validator.Symbol("arg");
        validator.TagStates("End").Should().SatisfyRespectively(
            x => x[arg].Should().HaveOnlyConstraint(ObjectConstraint.NotNull),
            x => x[arg].Should().HaveNoConstraints()); // ToDo: Misses ParameterReassignedConstraint
    }

    [TestMethod]
    public void ParameterReassignedConstraint_NullCoalescingAssignment_ThrowOnNull()
    {
        var validator = SETestContext.CreateCS("""
            arg = arg ?? throw new ArgumentNullException();
            Tag("End", arg);
            """, "string arg", new PublicMethodArgumentsShouldBeCheckedForNull()).Validator;
        validator.TagValue("End").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void ParameterReassignedConstraint_NullCoalescingAssignment_Unknown()
    {
        var validator = SETestContext.CreateCS("""
            arg = arg ?? Unknown<string>();
            Tag("End");
            """, "string arg", new PublicMethodArgumentsShouldBeCheckedForNull(), new PreserveTestCheck("arg")).Validator;
        var arg = validator.Symbol("arg");
        validator.TagStates("End").Should().SatisfyRespectively(
            x => x[arg].Should().HaveNoConstraints(), // ToDo: Misses ParameterReassignedConstraint
            x => x[arg].Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
    }

    [TestMethod]
    public void ParameterReassignedConstraint_NullConditional_ThrowOnNull()
    {
        var validator = SETestContext.CreateCS("""
            arg = arg == null
                ? throw new ArgumentNullException()
                : arg;
            Tag("End", arg);
            """, "string arg", new PublicMethodArgumentsShouldBeCheckedForNull()).Validator;
        validator.TagValue("End").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void ParameterReassignedConstraint_NullConditional_ThrowOnNull_Unknown()
    {
        var validator = SETestContext.CreateCS("""
            arg = arg == null
                ? throw new ArgumentNullException()
                : Unknown<string>();
            Tag("End", arg);
            """, "string arg", new PublicMethodArgumentsShouldBeCheckedForNull()).Validator;
        validator.TagValue("End").Should().HaveNoConstraints(); // ToDo: Misses ParameterReassignedConstraint
    }

#if NET

    [DataTestMethod]
    [DataRow("ThrowIfNull")]
    [DataRow("ThrowIfNullOrEmpty")]
    public void ParameterReassignedConstraint_ArgumentNullException_ThrowIfNull(string throwIfNullMethod)
    {
        var validator = SETestContext.CreateCS($$"""
                ArgumentNullException.{{throwIfNullMethod}}(arg);
                Tag("End", arg);
                """, "string arg", new PublicMethodArgumentsShouldBeCheckedForNull()).Validator;
        validator.TagValue("End").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

#endif

    [TestMethod]
    public void ParameterReassignedConstraint_WithMultipleParameters()
    {
        const string snippet = """
            arg1 = Unknown<object>();
            Tag("AfterAssignmentArg1", arg1);
            Tag("AfterAssignmentArg2", arg2);
            """;
        var validator = SETestContext.CreateCS(snippet, "object arg1, object arg2", new PublicMethodArgumentsShouldBeCheckedForNull()).Validator;
        validator.TagValue("AfterAssignmentArg1").Should().HaveOnlyConstraints(ParameterReassignedConstraint.Instance);
        validator.TagValue("AfterAssignmentArg2").Should().HaveNoConstraints();
    }

    [DataTestMethod]
    [DataRow("""var local = Unknown<object>(); Tag("AfterAssignment", local);""")]
    [DataRow("""ObjectField = Unknown<object>(); Tag("AfterAssignment", ObjectField);""")]
    [DataRow("""FullProperty = Unknown<object>(); Tag("AfterAssignment", FullProperty);""")]
    public void ParameterReassignedConstraint_IgnoreNonParameters(string snippet)
    {
        var validator = SETestContext.CreateCS(snippet, new PublicMethodArgumentsShouldBeCheckedForNull()).Validator;
        validator.TagValue("AfterAssignment").Should().HaveNoConstraints();
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

    [TestMethod]
    public void ParameterReassignedConstraint_HasNoOpposite()
    {
        ParameterReassignedConstraint.Instance.Opposite.Should().BeNull();
    }

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
        validator.TagValue("End").Should().HaveOnlyConstraints(expectedConstraints);
    }
}

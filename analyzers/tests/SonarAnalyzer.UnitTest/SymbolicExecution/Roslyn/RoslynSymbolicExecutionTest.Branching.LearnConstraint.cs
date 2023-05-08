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

using System.Numerics;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn;

public partial class RoslynSymbolicExecutionTest
{
    [TestMethod]
    public void Branching_BoolSymbol_LearnsBoolConstraint()
    {
        const string code = @"
if (boolParameter)          // True constraint is learned
{
    Tag(""True"", boolParameter);
    if (boolParameter)      // True constraint is known
    {
        Tag(""TrueTrue"");
    }
    else
    {
        Tag(""TrueFalse Unreachable"");
    }
}
else                        // False constraint is learned
{
    Tag(""False"", boolParameter);
    if (boolParameter)      // False constraint is known
    {
        Tag(""FalseTrue Unreachable"");
    }
    else
    {
        Tag(""FalseFalse"");
    }
};
Tag(""End"");";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder(
            "True",
            "False",
            "TrueTrue",
            "FalseFalse",
            "End");
        validator.ValidateTag("True", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True));
        validator.ValidateTag("False", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False));
    }

    [TestMethod]
    public void Branching_ConversionAndBoolSymbol_LearnsBoolConstraint()
    {
        const string code = @"
if ((bool)(object)(bool)boolParameter)
{
    Tag(""True"", boolParameter);
}
else
{
    Tag(""False"", boolParameter);
};
Tag(""End"", boolParameter);";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTag("True", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True));
        validator.ValidateTag("False", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False));
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False));
    }

    [DataTestMethod]
    [DataRow("~")]
    [DataRow("+")]
    [DataRow("-")]
    public void Branching_ConversionAndOtherUnaryOperators_DoNotLearnConstraints(string unary)
    {
        var code = @$"
if ((bool)(object)({unary}arg))
{{
    Tag(""Arg"", arg);
}}";
        var validator = SETestContext.CreateCS(code, "int arg").Validator;
        validator.ValidateContainsOperation(OperationKind.Unary);
        validator.ValidateTag("Arg", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull));
    }

    [TestMethod]
    public void Branching_BoolOperation_LearnsBoolConstraint()
    {
        const string code = @"
if (collection.IsReadOnly)
{
    Tag(""If"", collection);
}
Tag(""End"", collection);";
        var check = new ConditionEvaluatedTestCheck(x => x.State[x.Operation].HasConstraint(BoolConstraint.True)
                                                             ? x.SetSymbolConstraint(x.Operation.Instance.AsPropertyReference().Value.Instance.TrackedSymbol(), DummyConstraint.Dummy)
                                                             : x.State);
        var validator = SETestContext.CreateCS(code, "ICollection<object> collection", check).Validator;
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraints(DummyConstraint.Dummy, ObjectConstraint.NotNull));
        validator.TagStates("End").Should().HaveCount(2);
    }

    [TestMethod]
    public void Branching_BoolExpression_LearnsBoolConstraint()
    {
        const string code = @"
if (boolParameter == true)
{
    Tag(""True"", boolParameter);
}
else
{
    Tag(""False"", boolParameter);
}
bool value;
if (value = boolParameter)
{
    Tag(""Value"", value);
}";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTag("True", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True));
        validator.ValidateTag("False", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False));
        validator.ValidateTag("Value", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True));    // Visited only for "true" condition
    }

    [DataTestMethod]
    [DataRow("arg == null")]
    [DataRow("arg == isNull")]
    [DataRow("null == arg")]
    [DataRow("isNull == arg")]
    [DataRow("(object)(object)arg == (object)(object)null")]
    [DataRow("(object)(object)arg == (object)(object)isNull")]
    [DataRow("!!!(arg != null)")]
    [DataRow("!!!(null != arg)")]
    [DataRow("!(bool)(object)!!(arg != null)")]
    [DataRow("!(bool)(object)!!(null != arg)")]
    [DataRow("!!!((object)arg != (object)null)")]
    [DataRow("!!!((object)null != (object)arg)")]
    public void Branching_LearnsObjectConstraint_CS(string expression)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.Binary);
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.Null));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg == null")]
    [DataRow("null == arg")]
    [DataRow("(object)(object)arg == (object)(object)null")]
    [DataRow("(object)(object)arg == (object)(object)isNull")]
    [DataRow("!!!(arg != null)")]
    [DataRow("!!!(null != arg)")]
    [DataRow("!(bool)(object)!!(arg != null)")]
    [DataRow("!(bool)(object)!!(null != arg)")]
    [DataRow("!!!((object)arg != (object)null)")]
    [DataRow("!!!((object)null != (object)arg)")]
    public void Branching_LearnsObjectConstraint_NullableInt_CS(string expression)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.Binary, "int?");
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.Null));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg == null")]
    [DataRow("null == arg")]
    [DataRow("(object)(object)arg == (object)(object)null")]
    [DataRow("(object)(object)arg == (object)(object)isNull")]
    [DataRow("!!!(arg != null)")]
    [DataRow("!!!(null != arg)")]
    [DataRow("!(bool)(object)!!(arg != null)")]
    [DataRow("!(bool)(object)!!(null != arg)")]
    [DataRow("!!!((object)arg != (object)null)")]
    [DataRow("!!!((object)null != (object)arg)")]
    public void Branching_LearnsObjectConstraint_NullableBool_CS(string expression)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.Binary, "bool?");
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.Null));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg != null")]
    [DataRow("arg != isNull")]
    [DataRow("null != arg")]
    [DataRow("isNull != arg")]
    [DataRow("(object)(object)arg != (object)(object)null")]
    [DataRow("(object)(object)arg != (object)(object)isNull")]
    [DataRow("!!!(arg == null)")]
    [DataRow("!!!(null == arg)")]
    [DataRow("!(bool)(object)!!(arg == null)")]
    [DataRow("!(bool)(object)!!(null == arg)")]
    public void Branching_LearnsObjectConstraint_Binary_Negated_CS(string expression)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.Binary);
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.Null));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg != null")]
    [DataRow("null != arg")]
    [DataRow("(object)(object)arg != (object)(object)null")]
    [DataRow("(object)(object)arg != (object)(object)isNull")]
    [DataRow("!!!(arg == null)")]
    [DataRow("!!!(null == arg)")]
    [DataRow("!(bool)(object)!!(arg == null)")]
    [DataRow("!(bool)(object)!!(null == arg)")]
    public void Branching_LearnsObjectConstraint_Binary_Negated_NullableInt_CS(string expression)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.Binary, "int?");
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.Null));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg != null")]
    [DataRow("null != arg")]
    [DataRow("(object)(object)arg != (object)(object)null")]
    [DataRow("(object)(object)arg != (object)(object)isNull")]
    [DataRow("!!!(arg == null)")]
    [DataRow("!!!(null == arg)")]
    [DataRow("!(bool)(object)!!(arg == null)")]
    [DataRow("!(bool)(object)!!(null == arg)")]
    public void Branching_LearnsObjectConstraint_Binary_Negated_NullableBool_CS(string expression)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.Binary, "bool?");
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.Null));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [TestMethod]
    public void Branching_LearnsObjectConstraint_NullableBool()
    {
        var validator = CreateIfElseEndValidatorCS("(bool)arg", OperationKind.Conversion, "bool?");
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False));
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False));
    }

    [TestMethod]
    public void Branching_LearnsObjectConstraint_Integer_VB()
    {
        var validator = CreateIfElseEndValidatorVB("arg", OperationKind.Binary, "Integer");
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull));
        validator.ValidateTag("End", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("Integer?")]
    [DataRow("Boolean?")]
    public void Branching_LearnsObjectConstraint_Nullable_VB(string parameterType)
    {
        var validator = CreateIfElseEndValidatorVB("arg", OperationKind.Binary, parameterType);
        validator.ValidateTag("If", x => x.Should().HaveNoConstraints("arg.GetValueOrDefault() is called in 'if arg' and arg is not found in RoslynSymbolicExecution.SetBranchingConstraints"));
        validator.ValidateTag("Else", x => x.Should().HaveNoConstraints());
        validator.ValidateTag("End", x => x.Should().HaveNoConstraints());
    }

    [DataTestMethod]
    [DataRow("True", true)]
    [DataRow("False", false)]
    public void Branching_LearnsObjectConstraint_NullableBool_VB(string branchValue, bool expectedPath)
    {
        var validator = SETestContext.CreateVB($$"""
            Dim b As Boolean? = {{branchValue}}
            if b Then
                Tag("True", b)
            else
                Tag("False", b)
            End if
            Tag("End", b)
            """).Validator;
        validator.ValidateTagOrder(branchValue, "End");
        validator.ValidateTag(branchValue, x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.From(expectedPath)));
        validator.TagValues("End").Should().SatisfyRespectively(x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.From(expectedPath)));
    }

    [DataTestMethod]
    [DataRow("arg == isObject", "object")]
    [DataRow("isObject == arg", "object")]
    [DataRow("arg == 42", "int?")]
    [DataRow("42 == arg", "int?")]
    public void Branching_LearnsObjectConstraint_Binary_UndefinedInOtherBranch_CS(string expression, string argType)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.Binary, argType);
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().BeNull("We can't tell if it is Null or NotNull in this branch"));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x == null)
            .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg == true", true)]
    [DataRow("arg == false", false)]
    [DataRow("true == arg", true)]
    [DataRow("false == arg", false)]
    public void Branching_LearnsObjectConstraint_Binary_UndefinedInOtherBranch_NullableBool_CS(string expression, bool expected)
    {
        var expectedBoolConstraint = BoolConstraint.From(expected);
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.Binary, "bool?");
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, expectedBoolConstraint));
        validator.ValidateTag("Else", x => x.Should().BeNull("We can't tell if it is Null or NotNull in this branch"));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x == null)
            .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg != isObject", "object")]
    [DataRow("isObject != arg", "object")]
    [DataRow("arg != 42", "int?")]
    [DataRow("42 != arg", "int?")]
    public void Branching_LearnsObjectConstraint_Binary_UndefinedInOtherBranch_Negated_CS(string expression, string argType)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.Binary, argType);
        validator.ValidateTag("If", x => x.Should().BeNull("We can't tell if it is Null or NotNull in this branch"));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x == null)
            .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg != true", true)]
    [DataRow("arg != false", false)]
    [DataRow("true != arg", true)]
    [DataRow("false != arg", false)]
    public void Branching_LearnsObjectConstraint_Binary_UndefinedInOtherBranch_NulalbleBool_Negated_CS(string expression, bool expected)
    {
        var expectedBoolConstraint = BoolConstraint.From(expected);
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.Binary, "bool?");
        validator.ValidateTag("If", x => x.Should().BeNull("We can't tell if it is Null or NotNull in this branch"));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, expectedBoolConstraint));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x == null)
            .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("Arg Is Nothing")]
    [DataRow("Arg = Nothing")]
    [DataRow("Nothing Is Arg")]
    [DataRow("Nothing = Arg")]
    [DataRow("Not Not Not Arg <> Nothing")]
    [DataRow("Not Not Not Nothing <> Arg")]
    public void Branching_LearnsObjectConstraint_Binary_VB(string expression)
    {
        var validator = CreateIfElseEndValidatorVB(expression, OperationKind.Binary);
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.Null));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("Arg <> Nothing")]
    [DataRow("Nothing <> Arg")]
    [DataRow("Not Not Not Arg Is Nothing")]
    [DataRow("Not Not Not Nothing Is Arg")]
    [DataRow("Not Not Not Arg = Nothing")]
    [DataRow("Not Not Not Nothing = Arg")]
    public void Branching_LearnsObjectConstraint_Binary_Negated_VB(string expression)
    {
        var validator = CreateIfElseEndValidatorVB(expression, OperationKind.Binary);
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.Null));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg is object")]
    [DataRow("arg is Exception")]
    [DataRow("!!(arg is object)")]
    [DataRow("!!(arg is Exception)")]
    public void Branching_LearnsObjectConstraint_IsType_CS(string expression)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.IsType);
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().BeNull("it could be null or any other type"));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x == null)
            .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("!(arg is object)")]
    [DataRow("!(arg is Exception)")]
    [DataRow("!!!(arg is object)")]
    [DataRow("!!!(arg is Exception)")]
    public void Branching_LearnsObjectConstraint_IsType_Negated_CS(string expression)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.IsType);
        validator.ValidateTag("If", x => x.Should().BeNull("it could be null or any other type"));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x == null)
            .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("(object)null is Exception")]
    [DataRow("(int?)null is object")]
    [DataRow("(int?)null is int")]
    [DataRow("(int?)null is int?")]
    public void Branching_LearnsObjectConstraint_IsType_NullValue(string expression)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.IsType);  // Check something that is known to be null
        validator.ValidateTagOrder("Else", "End");                  // Always true, else branch is not visited
        validator.ValidateTag("Else", x => x.Should().BeNull());
        validator.ValidateTag("End", x => x.Should().BeNull());
    }

    [TestMethod]
    public void Branching_LearnsObjectConstraint_IsType_NoSymbol_DoesNotChangeState()
    {
        var validator = CreateIfElseEndValidatorCS("(object)(40 + 2) is Exception", OperationKind.IsType); // Check something that doesn't have a tracked symbol
        validator.ValidateTag("If", x => x.Should().BeNull());
        validator.ValidateTag("Else", x => x.Should().BeNull());
        validator.ValidateTag("End", x => x.Should().BeNull());
    }

    [DataTestMethod]
    [DataRow("TypeOf Arg Is Object")]
    [DataRow("TypeOf Arg Is Exception")]
    [DataRow("Not Not TypeOf Arg Is Object")]
    [DataRow("Not Not TypeOf Arg Is Exception")]
    public void Branching_LearnsObjectConstraint_IsType_VB(string expression)
    {
        var validator = CreateIfElseEndValidatorVB(expression, OperationKind.IsType);
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().BeNull("it could be null or any other type"));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x == null)
            .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("TypeOf Arg IsNot Object")]
    [DataRow("TypeOf Arg IsNot Exception")]
    [DataRow("Not TypeOf Arg Is Object")]
    [DataRow("Not TypeOf Arg Is Exception")]
    public void Branching_LearnsObjectConstraint_IsType_Negated_VB(string expression)
    {
        var validator = CreateIfElseEndValidatorVB(expression, OperationKind.IsType);
        validator.ValidateTag("If", x => x.Should().BeNull("it could be null or any other type"));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x == null)
            .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg is null", "object")]
    [DataRow("arg is null", "int?")]
    [DataRow("!!(arg is null)", "object")]
    [DataRow("!!(arg is null)", "int?")]
    [DataRow("arg is not not null", "object")]
    [DataRow("arg is not not null", "int?")]
    public void Branching_LearnsObjectConstraint_ConstantPattern_Null(string expression, string argType)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.ConstantPattern, argType);
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.Null));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("!(arg is null)", "object")]
    [DataRow("!(arg is null)", "int?")]
    [DataRow("!!!(arg is null)", "object")]
    [DataRow("!!!(arg is null)", "int?")]
    [DataRow("arg is not null", "object")]
    [DataRow("arg is not null", "int?")]
    public void Branching_LearnsObjectConstraint_ConstantPattern_Null_Negated(string expression, string argType)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.ConstantPattern, argType);
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.Null));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg is true")]
    [DataRow("arg is true", "bool?")]
    [DataRow("!!(arg is true)")]
    [DataRow("!!(arg is true)", "bool?")]
    [DataRow("arg is not not true")]
    [DataRow("arg is not not true", "bool?")]
    public void Branching_LearnsObjectConstraint_ConstantPattern_True(string expression, string argType = "object")
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.ConstantPattern, argType);
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(BoolConstraint.True)); // Should be NotNull, True
        validator.ValidateTag("Else", x => x.Should().HaveNoConstraints("it could be False, null or any other type"));
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(BoolConstraint.True),
            x => x.Should().HaveNoConstraints());
    }

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Branching_LearnsObjectConstraint_ConstantPattern_ForBool(bool constantPattern)
    {
        var validator = CreateIfElseEndValidatorCS($"arg is {constantPattern.ToString().ToLower()}", OperationKind.ConstantPattern, "bool");
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.From(constantPattern)));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.From(constantPattern)),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg is not true", "object")]
    [DataRow("arg is not true", "bool?")]
    public void Branching_LearnsObjectConstraint_ConstantPattern_True_Negated(string expression, string argType)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.ConstantPattern, argType);
        validator.ValidateTag("If", x => x.Should().HaveNoConstraints("it could be False, null or any other type"));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(BoolConstraint.True));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x == null)
            .And.ContainSingle(x => x != null && x.HasConstraint(BoolConstraint.True));
    }

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Branching_LearnsObjectConstraint_ConstantPattern_True_Negated_ForBool(bool constantPattern)
    {
        var validator = CreateIfElseEndValidatorCS($"arg is not {constantPattern.ToString().ToLower()}", OperationKind.ConstantPattern, "bool");
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.From(constantPattern)));
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.From(constantPattern)));
    }

    [DataTestMethod]
    [DataRow("arg is false")]
    [DataRow("!!(arg is false)")]
    public void Branching_LearnsObjectConstraint_ConstantPattern_False(string expression)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.ConstantPattern);
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(BoolConstraint.False));
        validator.ValidateTag("Else", x => x.Should().BeNull("it could be True, null or any other type"));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x == null)
            .And.ContainSingle(x => x != null && x.HasConstraint(BoolConstraint.False));
    }

    [DataTestMethod]
    [DataRow("arg is 42", "int?")]
    [DataRow("arg is 42", "T")]
    public void Branching_LearnsObjectConstraint_ConstantPattern_ValueTypes_InputIsNotReferenceType(string expression, string argType)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.ConstantPattern, argType);
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().HaveNoConstraints());
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull),
            x => x.Should().HaveNoConstraints());
    }

    [DataTestMethod]
    [DataRow("arg is 42", "int")]
    [DataRow("arg is 42", "TStruct")]
    public void Branching_LearnsObjectConstraint_ConstantPattern_ValueTypes_InputNonNullableValueType(string expression, string argType)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.ConstantPattern, argType);
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.ValidateTag("End", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow(@"arg is ""some text""")]
    [DataRow(@"arg is """"")]
    [DataRow("arg is 42")]
    [DataRow("arg is 42", "int?")]
    [DataRow("arg is System.ConsoleKey.Enter")]     // Enum
    [DataRow("arg is 42", "TClass")]
    [DataRow("arg is 42", "IComparable")]           // arg is either a class implementing the interface or a boxed value type
    public void Branching_LearnsObjectConstraint_ConstantPattern_Literals(string expression, string argType = "object")
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.ConstantPattern, argType);
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().BeNull());
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x == null)
            .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg is not not object")]
    [DataRow("arg is not not Exception")]
    public void Branching_LearnsObjectConstraint_TypePattern(string expression)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.TypePattern);
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().BeNull("it could be null or any other type"));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x == null)
            .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg is not object")]
    [DataRow("arg is not Exception")]
    public void Branching_LearnsObjectConstraint_TypePattern_Negated(string expression)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.TypePattern);
        validator.ValidateTag("If", x => x.Should().BeNull("it could be null or any other type"));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x == null)
            .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg is { }")]
    [DataRow("arg is { }", "TClass")]
    [DataRow("arg is object { }")]
    [DataRow("arg is object { }")]
    [DataRow("arg is Exception { }", "ArgumentException")]
    [DataRow("arg is not not Exception { }", "ArgumentException")]
    [DataRow("arg is ICloneable { }", "string")] // string implements ICloneable
    [DataRow("arg is not not ICloneable { }", "string")]
    [DataRow("arg is { Length: var length }", "string")]
    [DataRow("arg is { Length: _ }", "string")]
    [DataRow("arg is not not { }")]
    [DataRow("!!(arg is { })")]
    [DataRow("arg is (A: var a, B: _)", "Deconstructable")]
    [DataRow("arg is { }", "T")]
    public void Branching_LearnsObjectConstraint_RecursivePattern_ElseIsNull(string expression, string argType = "object")
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.RecursivePattern, argType);
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.Null));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg is string { }")]
    [DataRow("arg is string { }", "TClass")]
    [DataRow("arg is string { Length: 0 }")]
    [DataRow("arg is string { Length: var length }")]
    [DataRow("arg is int { }")]
    [DataRow("arg is T { }")]
    [DataRow("arg is TClass { }")]
    [DataRow("arg is TStruct { }")]
    [DataRow("arg is DateTime { Ticks: 0 }")]
    [DataRow("arg is string { }", "T")]
    public void Branching_LearnsObjectConstraint_RecursivePattern_ElseIsUnknown(string expression, string argType = "object")
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.RecursivePattern, argType);
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().BeNull("it could be null or any other type"));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x == null)
            .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
    }

    [TestMethod]
    public void Branching_LearnsObjectConstraint_RecursivePattern_Tuple_NoConstraint()
    {
        // We don't support learning for tuples (yet). Should behave same as "arg is { }". Gets tricky when nesting (a, (b, c))
        var validator = CreateIfElseEndValidatorCS("(arg, Unknown<object>()) is ({ }, { })", OperationKind.RecursivePattern);
        validator.ValidateTag("If", x => x.Should().HaveNoConstraints());
        validator.ValidateTag("Else", x => x.Should().HaveNoConstraints());
        validator.ValidateTag("End", x => x.Should().HaveNoConstraints());
    }

    [DataTestMethod]
    [DataRow("arg is { }", "int")]
    [DataRow("arg is { }", "TStruct")]
    [DataRow("arg is TStruct { }", "TStruct")]
    public void Branching_LearnsObjectConstraint_RecursivePattern_ValueTypeConstraint(string expression, string argType)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.RecursivePattern, argType);
        validator.ValidateTagOrder("If", "End"); // Always true, else branch is not visited
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.ValidateTag("End", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("!(arg is { })")]
    [DataRow("arg is not { }")]
    public void Branching_LearnsObjectConstraint_RecursivePattern_Negated_IfIsNotNull(string expression)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.RecursivePattern);
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.Null));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg is not string { Length: var length }")]
    [DataRow("arg is not string { Length: 0 }")]
    public void Branching_LearnsObjectConstraint_RecursivePattern_Negated_IfIsUnknown(string expression)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.RecursivePattern);
        validator.ValidateTag("If", x => x.Should().BeNull("it could be null or any other type"));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x == null)
            .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg is string s")]
    [DataRow("arg is T t")]
    [DataRow("arg is TClass t")]
    [DataRow("arg is TStruct t")]
    [DataRow("arg is int i")]
    [DataRow("arg is not not string s")]
    [DataRow("arg is object o", "TClass")]  // We could infer that Else is null instead
    public void Branching_LearnsObjectConstraint_DeclarationPattern(string expression, string argType = "object")
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.DeclarationPattern, argType);
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().BeNull("it could be null or any other type"));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x == null)
            .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
    }

    [TestMethod]
    public void Branching_LearnsObjectConstraint_DeclarationPattern_ValueType()
    {
        var validator = CreateIfElseEndValidatorCS("arg is object o", OperationKind.DeclarationPattern, "TStruct");
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.ValidateTag("End", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg is object o")]
    [DataRow("arg is object o", "string")]
    [DataRow("arg is not not object o")]
    [DataRow("!(arg is not object o)")]
    public void Branching_LearnsObjectConstraint_DeclarationPattern_ElseIsNull(string expression, string argType = "object")
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.DeclarationPattern, argType);
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.Null));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg is var o")]
    [DataRow("arg is object o", "T")]
    public void Branching_LearnsObjectConstraint_DeclarationPattern_NoConstraints(string expression, string argType = "object")
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.DeclarationPattern, argType);
        validator.ValidateTag("If", x => x.Should().BeNull());
        validator.ValidateTag("Else", x => x.Should().BeNull());
        validator.ValidateTag("End", x => x.Should().BeNull());
    }

    [DataTestMethod]
    [DataRow("arg is not string s")]
    [DataRow("arg is not int i")]
    [DataRow("!(arg is string s)")]
    public void Branching_LearnsObjectConstraint_DeclarationPattern_Negated(string expression)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.DeclarationPattern);
        validator.ValidateTag("If", x => x.Should().BeNull("it could be null or any other type"));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x == null)
            .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg is not object o")]
    [DataRow("!(arg is object o)")]
    public void Branching_LearnsObjectConstraint_DeclarationPattern_Negated_ElseIsNull(string expression)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.DeclarationPattern);
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.Null));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [TestMethod]
    public void Branching_LearnsObjectConstraint_FlowCapture()
    {
        const string code = @"
switch (arg)
{
    case null:
        Tag(""Null"", arg);
        break;
    default:
        Tag(""Default"", arg);
        break;
}
Tag(""End"", arg);";
        var validator = SETestContext.CreateCS(code, "object arg").Validator;
        validator.ValidateContainsOperation(OperationKind.FlowCaptureReference);
        validator.ValidateTag("Null", x => x.Should().HaveOnlyConstraint(ObjectConstraint.Null));
        validator.ValidateTag("Default", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [TestMethod]
    public void Branching_LearnsObjectConstraint_IsNull()
    {
        var code = @"
arg?.ToString();
Tag(""End"", arg);";
        var validator = SETestContext.CreateCS(code, "object arg").Validator;
        validator.ValidateContainsOperation(OperationKind.IsNull);
        validator.Validate("Invocation: .ToString()", x => x.State.SymbolsWith(ObjectConstraint.NotNull).Should().ContainSingle());
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg.HasValue")]
    [DataRow("arg.HasValue is true")]
    [DataRow("arg.HasValue == true")]
    [DataRow("arg.HasValue != false")]
    [DataRow("!!arg.HasValue")]
    public void Branching_LearnsObjectConstraint_Nullable_HasValue_True(string expression)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.DeclarationPattern, "int?");
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.Null));
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull),
            x => x.Should().HaveOnlyConstraint(ObjectConstraint.Null));
    }

    [DataTestMethod]
    [DataRow("!arg.HasValue")]
    [DataRow("arg.HasValue != true")]
    [DataRow("arg.HasValue == false")]
    [DataRow("arg.HasValue is false")]
    [DataRow("!!!arg.HasValue")]
    public void Branching_LearnsObjectConstraint_Nullable_HasValue_Negated(string expression)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.DeclarationPattern, "int?");
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraint(ObjectConstraint.Null));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull),
            x => x.Should().HaveOnlyConstraint(ObjectConstraint.Null));
    }

    [TestMethod]
    public void Branching_IsNullOperation_WithIsNullOrEmpty()
    {
        var validator = CreateIfElseEndValidatorCS("string.IsNullOrEmpty(arg?.ToString())", OperationKind.IsNull);
        validator.TagValues("If").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg?.Length == 0")]
    [DataRow("0 == arg?.Length")]
    public void Branching_IsNullOperation_Equals(string expression)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.IsNull, "string");
        validator.TagValues("If").Should().ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
        validator.TagValues("Else").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [TestMethod]
    public void Branching_LearnIsNullSurvivesLambdaCapture()
    {
        var validator = SETestContext.CreateCS("""
                if (s == null)
                {
                    return;
                }

                Func<string> someFunc = () => s.ToString();

                Tag("End", s);
                """, "string s").Validator;
        validator.ValidateTag("End", x => x.Should().HaveNoConstraints()); // Should have NotNull constraint
    }

    [DataTestMethod]
    [DataRow("sbyte")]
    [DataRow("byte")]
    [DataRow("short")]
    [DataRow("ushort")]
    [DataRow("int")]
    [DataRow("uint")]
    [DataRow("nint")]
    [DataRow("nuint")]
    [DataRow("long")]
    [DataRow("ulong")]
    public void Branching_LearnsNumberConstraint(string argType)
    {
        var validator = CreateIfElseEndValidatorCS("arg == 42", OperationKind.Binary, argType);
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(42)));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));
        validator.TagValues("End").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(42)),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull));
    }

    [DataTestMethod]        // FIXME: Everything except one should go away
    [DataRow("arg >  42")]
    [DataRow("42 <  arg")]
    [DataRow("arg >= 42")]
    [DataRow("42 <= arg")]
    [DataRow("arg <  42")]
    [DataRow("42 >  arg")]
    [DataRow("arg <= 42")]
    [DataRow("42 >= arg")]
    [DataRow("arg >  42 && arg >    0")]
    [DataRow("arg >  42 && arg >   42")]
    [DataRow("arg >  42 && arg >  100")]
    [DataRow("arg >  42 && arg >= 100")]
    [DataRow("arg >  42 && arg >=   0")]
    [DataRow("arg >  42 && arg >=  41")]
    [DataRow("arg >  42 && arg >=  42")]
    [DataRow("arg >  42 && arg >=  43")]
    [DataRow("arg >  42 && arg <  100")]
    [DataRow("arg >  42 && arg <= 100")]
    [DataRow("arg >= 42 && arg >    0")]
    [DataRow("arg >= 42 && arg >   41")]
    [DataRow("arg >= 42 && arg >   42")]
    [DataRow("arg >= 42 && arg >   43")]
    [DataRow("arg >= 42 && arg >  100")]
    [DataRow("arg >= 42 && arg >=   0")]
    [DataRow("arg >= 42 && arg >=  41")]
    [DataRow("arg >= 42 && arg >=  42")]
    [DataRow("arg >= 42 && arg >=  43")]
    [DataRow("arg >= 42 && arg >= 100")]
    [DataRow("arg >= 42 && arg <  100")]
    [DataRow("arg >= 42 && arg <= 100")]
    [DataRow("arg <  42 && arg >    0")]
    [DataRow("arg <  42 && arg >=   0")]
    [DataRow("arg <  42 && arg <    0")]
    [DataRow("arg <  42 && arg <   41")]
    [DataRow("arg <  42 && arg <   42")]
    [DataRow("arg <  42 && arg <   43")]
    [DataRow("arg <  42 && arg <  100")]
    [DataRow("arg <  42 && arg <=   0")]
    [DataRow("arg <  42 && arg <=  41")]
    [DataRow("arg <  42 && arg <=  42")]
    [DataRow("arg <  42 && arg <=  43")]
    [DataRow("arg <  42 && arg <= 100")]
    [DataRow("arg <= 42 && arg >    0")]
    [DataRow("arg <= 42 && arg >=   0")]
    [DataRow("arg <= 42 && arg <    0")]
    [DataRow("arg <= 42 && arg <   41")]
    [DataRow("arg <= 42 && arg <   42")]
    [DataRow("arg <= 42 && arg <   43")]
    [DataRow("arg <= 42 && arg <  100")]
    [DataRow("arg <= 42 && arg <=   0")]
    [DataRow("arg <= 42 && arg <=  41")]
    [DataRow("arg <= 42 && arg <=  42")]
    [DataRow("arg <= 42 && arg <=  43")]
    [DataRow("arg <= 42 && arg <= 100")]
    public void Branching_LearnsNumberConstraint_NoConstraints(string expression)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.Binary, "int");
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("arg == 42", 42, 42)]
    [DataRow("42 == arg", 42, 42)]
    [DataRow("arg == 42 && arg ==  42", 42, 42)]
    [DataRow("arg == 42 && arg != 100", 42, 42)]
    [DataRow("arg == 42 && arg >    0", 42, 42)]
    [DataRow("arg == 42 && arg >=   0", 42, 42)]
    [DataRow("arg == 42 && arg <  100", 42, 42)]
    [DataRow("arg == 42 && arg <= 100", 42, 42)]
    [DataRow("arg >  42 && arg == 100", 100, 100)]
    [DataRow("arg >= 42 && arg == 100", 100, 100)]
    [DataRow("arg <  42 && arg ==   0", 0, 0)]
    [DataRow("arg <= 42 && arg ==  42", 42, 42)]
    public void Branching_LearnsNumberConstraint_OnlyIf(string expression, int? expectedIfMin, int? expectedIfMax)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.Binary, "int");
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, ExpectedNumber(expectedIfMin, expectedIfMax)));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    // FIXME: Add datarows
    public void Branching_LearnsNumberConstraint_IfElse(string expression, int? expectedIfMin, int? expectedIfMax, int? expectedElseMin, int? expectedElseMax)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.Binary, "int");
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, ExpectedNumber(expectedIfMin, expectedIfMax)));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, ExpectedNumber(expectedElseMin, expectedElseMax)));
    }

    [DataTestMethod]
    [DataRow("arg != 42", 42, 42)]
    [DataRow("42 != arg", 42, 42)]
    // FIXME: Produces more states for now because arg > 42 produces "no-number" value
    //[DataRow("arg != 42 && arg == 100", 42, 42)]
    //[DataRow("arg != 42 && arg != 100", 42, 42)]    // FIXME: Only this one should remain here, everything else shoudl go away
    //[DataRow("arg != 42 && arg >    0", 42, 42)]    // Actual value is 1-41, 43-oo
    //[DataRow("arg != 42 && arg >=   0", 42, 42)]    // Actual value is 0-41, 43-oo
    //[DataRow("arg != 42 && arg <  100", 42, 42)]    // Actual value is -oo-41, 43-99
    //[DataRow("arg != 42 && arg <= 100", 42, 42)]    // Actual value is -oo-41, 43-100
    //[DataRow("arg >  42 && arg !=  43", 43, 43)]
    //[DataRow("arg >  42 && arg != 100", 100, 100)]  // Actual value is 43-99, 101-oo
    //[DataRow("arg >= 42 && arg !=  42", 42, 42)]
    //[DataRow("arg >= 42 && arg != 100", 100, 100)]  // Actual value is 42-99, 101-oo
    //[DataRow("arg <  42 && arg !=  41", 41, 41)]
    //[DataRow("arg <  42 && arg !=   0", 0, 0)]      // Actual value is oo - -1, 1-41
    //[DataRow("arg <= 42 && arg !=   0", 0, 0)]      // Actual value is oo - -1, 1-42
    //[DataRow("arg <= 42 && arg !=  42", 42, 42)]
    public void Branching_LearnsNumberConstraint_OnlyElse(string expression, int? expectedElseMin, int? expectedElseMax)
    {
        var validator = CreateIfElseEndValidatorCS(expression, OperationKind.Binary, "int");
        validator.ValidateTag("If", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull));
        validator.ValidateTag("Else", x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, ExpectedNumber(expectedElseMin, expectedElseMax)));
    }

    [DataTestMethod]
    // FIXME: There are all wrong for the moment
    [DataRow("arg == 42 && arg == 100")]
    [DataRow("arg == 42 && arg !=  42")]
    [DataRow("arg == 42 && arg >  100")]
    [DataRow("arg == 42 && arg >= 100")]
    [DataRow("arg == 42 && arg <    0")]
    [DataRow("arg == 42 && arg <=   0")]
    [DataRow("arg != 42 && arg ==  42")]
    [DataRow("arg >  42 && arg ==   0")]
    [DataRow("arg >  42 && arg <    0")]
    [DataRow("arg >  42 && arg <=   0")]
    [DataRow("arg >= 42 && arg ==   0")]
    [DataRow("arg >= 42 && arg <    0")]
    [DataRow("arg >= 42 && arg <=   0")]
    [DataRow("arg <  42 && arg ==  42")]
    [DataRow("arg <  42 && arg >  100")]
    [DataRow("arg <  42 && arg >= 100")]
    [DataRow("arg <= 42 && arg == 100")]
    [DataRow("arg <= 42 && arg >  100")]
    [DataRow("arg <= 42 && arg >= 100")]
    public void Branching_LearnsNumberConstraint_Unreachable(string expression) =>
        CreateIfElseEndValidatorCS(expression, OperationKind.Binary, "int").ValidateTagOrder("Else", "End");

    private static ValidatorTestCheck CreateIfElseEndValidatorCS(string expression, OperationKind expectedOperation, string argType = "object")
    {
        var code = @$"
public void Main<T, TClass, TStruct>({argType} arg)
    where TClass : class
    where TStruct : struct
{{
    object isNull = null;
    var isObject = new object();
    if ({expression})
    {{
        Tag(""If"", arg);
    }}
    else
    {{
        Tag(""Else"", arg);
    }}
    Tag(""End"", arg);
}}";
        var validator = SETestContext.CreateCSMethod(code).Validator;
        validator.ValidateContainsOperation(expectedOperation);
        return validator;
    }

    private static ValidatorTestCheck CreateIfElseEndValidatorVB(string expression, OperationKind expectedOperation, string argType = "Object")
    {
        var code = @$"
If {expression} Then
    Tag(""If"", Arg)
Else
    Tag(""Else"", Arg)
End If
Tag(""End"", Arg)";
        var validator = SETestContext.CreateVB(code, $"Arg As {argType}").Validator;
        validator.ValidateContainsOperation(expectedOperation);
        return validator;
    }

    private static NumberConstraint ExpectedNumber(int? min, int? max) =>
        NumberConstraint.From(min.HasValue? new BigInteger(min.Value) : null, max.HasValue? new BigInteger(max.Value) : null);
}

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
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.Test.TestFramework.SymbolicExecution;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

public partial class RoslynSymbolicExecutionTest
{
    [TestMethod]
    public void IsPattern_TrueConstraint_SwitchExpressionFromSymbol_VisitsOnlyOneBranch()
    {
        const string code = @"
var isTrue = true;
var value = isTrue switch
{
    true => true,
    false => false
};
Tag(""Value"", value);";
        SETestContext.CreateCS(code).Validator.TagValue("Value").Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void IsPattern_TrueConstraint_SwitchExpressionFromOperation_VisitsOnlyOneBranch()
    {
        const string code = @"
var isTrue = true;
var value = (isTrue == true) switch
{
    true => true,
    false => false
};
Tag(""Value"", value);";
        SETestContext.CreateCS(code).Validator.TagValue("Value").Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void IsPattern_FalseConstraint_SwitchExpressionFromSymbol_VisitsOnlyOneBranch()
    {
        const string code = @"
var isFalse = false;
var value = isFalse switch
{
    true => true,
    false => false
};
Tag(""Value"", value);";
        SETestContext.CreateCS(code).Validator.TagValue("Value").Should().HaveOnlyConstraints(BoolConstraint.False, ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void IsPattern_NoConstraint_SwitchExpression_VisitsBothBranches()
    {
        const string code = @"
var value = boolParameter switch
{
    true => true,
    false => false
};
Tag(""Value"", value);";
        SETestContext.CreateCS(code).Validator.TagValues("Value").Should()
            .HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(BoolConstraint.True))
            .And.ContainSingle(x => x.HasConstraint(BoolConstraint.False));
    }

    [TestMethod]
    public void IsPattern_OtherConstraint_SwitchExpression_VisitsBothBranches()
    {
        const string code = @"
var value = boolParameter switch
{
    true => true,
    false => false
};
Tag(""Value"", value);";
        var check = new PostProcessTestCheck(x => x.Operation.Instance.Kind == OperationKind.ParameterReference ? x.SetOperationConstraint(DummyConstraint.Dummy) : x.State);
        SETestContext.CreateCS(code, check).Validator.TagValues("Value").Should()
            .HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(BoolConstraint.True))
            .And.ContainSingle(x => x.HasConstraint(BoolConstraint.False));
    }

    [TestMethod]
    public void IsPattern_OtherPattern_VisitsBothBranches()
    {
        const string code = @"
var value = arg switch
{
    string => true,
    null => true,
    _ => false
};
Tag(""Value"", value);";
        var check = new PostProcessTestCheck(x => x.Operation.Instance.Kind == OperationKind.ParameterReference ? x.SetOperationConstraint(DummyConstraint.Dummy) : x.State);
        SETestContext.CreateCS(code, "object arg", check).Validator.TagValues("Value").Should()
            .HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(BoolConstraint.True))
            .And.ContainSingle(x => x.HasConstraint(BoolConstraint.False));
    }

    [TestMethod]
    public void RecursivePattern_NoSymbol_DoesNothing()
    {
        const string code = @"
if (arg is { })
{
    Tag(""ArgNotNull"", arg);
}
Tag(""End"", arg);";
        var validator = SETestContext.CreateCS(code, "object arg").Validator;
        validator.ValidateContainsOperation(OperationKind.RecursivePattern);
        validator.TagValue("ArgNotNull").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [TestMethod]
    public void RecursivePattern_SetsNotNull_FirstLevel_NoPreviousConstraint()
    {
        const string code = @"
if (arg is { } value)
{
    Tag(""Value"", value);
    Tag(""ArgNotNull"", arg);
}
Tag(""End"", arg);";
        var validator = SETestContext.CreateCS(code, "object arg").Validator;
        validator.ValidateContainsOperation(OperationKind.RecursivePattern);
        validator.TagValue("Value").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("ArgNotNull").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [TestMethod]
    public void RecursivePattern_SetsNotNull_FirstLevel_PreservePreviousConstraint()
    {
        const string code = @"
if (arg is { } value)
{
    Tag(""Value"", value);
    Tag(""ArgNotNull"", arg);
}
Tag(""End"", arg);";
        var setter = new PreProcessTestCheck(OperationKind.ParameterReference, x => x.SetSymbolConstraint(x.Operation.Instance.TrackedSymbol(x.State), TestConstraint.First));
        var validator = SETestContext.CreateCS(code, "object arg", setter).Validator;
        validator.ValidateContainsOperation(OperationKind.RecursivePattern);
        validator.TagValue("Value").Should().HaveOnlyConstraints(TestConstraint.First, ObjectConstraint.NotNull);
        validator.TagValue("ArgNotNull").Should().HaveOnlyConstraints(TestConstraint.First, ObjectConstraint.NotNull);
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(TestConstraint.First) && x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(TestConstraint.First) && x.HasConstraint(ObjectConstraint.NotNull));
    }

    [TestMethod]
    public void RecursivePattern_SetsNotNull_Nested()
    {
        const string code = @"
if (arg is Exception { Message: { } msg } ex)
{
    Tag(""Msg"", msg);
    Tag(""Ex"", ex);
}
Tag(""End"", arg);";
        var setter = new PreProcessTestCheck(OperationKind.ParameterReference, x => x.SetSymbolConstraint(x.Operation.Instance.TrackedSymbol(x.State), TestConstraint.First));
        var validator = SETestContext.CreateCS(code, "object arg", setter).Validator;
        validator.ValidateContainsOperation(OperationKind.RecursivePattern);
        validator.TagValue("Msg").Should().HaveOnlyConstraint(ObjectConstraint.NotNull, "TestConstraint.First missing, because Constraint from source value should not be propagated to child property");
        validator.TagValue("Ex").Should().HaveOnlyConstraints(TestConstraint.First, ObjectConstraint.NotNull);
        validator.TagValues("End").Should().HaveCount(2).And.OnlyContain(x => x != null && x.HasConstraint(TestConstraint.First));   // 2x because value has different states
    }

    [TestMethod]
    public void DeclarationPattern_SetsNotNull_NoPreviousConstraint()
    {
        const string code = @"
if (arg is Exception value)
{
    Tag(""Value"", value);
    Tag(""ArgNotNull"", arg);
}
Tag(""End"", arg);";
        var validator = SETestContext.CreateCS(code, "object arg").Validator;
        validator.ValidateContainsOperation(OperationKind.DeclarationPattern);
        validator.TagValue("Value").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("ArgNotNull").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x == null)
            .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
    }

    [TestMethod]
    public void DeclarationPattern_SetsNotNull_PreservePreviousConstraint()
    {
        const string code = @"
if (arg is Exception value)
{
    Tag(""Value"", value);
    Tag(""ArgNotNull"", arg);
}
Tag(""End"", arg);";
        var setter = new PreProcessTestCheck(OperationKind.ParameterReference, x => x.SetSymbolConstraint(x.Operation.Instance.TrackedSymbol(x.State), TestConstraint.First));
        var validator = SETestContext.CreateCS(code, "object arg", setter).Validator;
        validator.ValidateContainsOperation(OperationKind.DeclarationPattern);
        validator.TagValue("Value").Should().HaveOnlyConstraints(TestConstraint.First, ObjectConstraint.NotNull);
        validator.TagValue("ArgNotNull").Should().HaveOnlyConstraints(TestConstraint.First, ObjectConstraint.NotNull);
        validator.TagValues("End").Should().HaveCount(2).And.OnlyContain(x => x != null && x.HasConstraint(TestConstraint.First));  // 2x because value has different states
    }

    [TestMethod]
    public void DeclarationPattern_Discard_DoesNotFail()
    {
        const string code = @"
if (arg is Exception _)
{
    Tag(""Arg"", arg);
}
Tag(""End"", arg);";
        var validator = SETestContext.CreateCS(code, "object arg").Validator;
        validator.ValidateContainsOperation(OperationKind.DeclarationPattern);
        validator.TagValue("Arg").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x == null)
            .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
    }

    [TestMethod]
    public void DeclarationPattern_Var_PreservePreviousConstraint_DoesNotSetNotNullConstraint()
    {
        const string code = """
            if (arg is var value)
            {
                Tag("Value", value);
                Tag("Arg", arg);
            }
            Tag("End", arg);
            """;
        var setter = new PreProcessTestCheck(OperationKind.ParameterReference, x => x.SetSymbolConstraint(x.Operation.Instance.TrackedSymbol(x.State), TestConstraint.First));
        var validator = SETestContext.CreateCS(code, "object arg", setter).Validator;
        validator.ValidateContainsOperation(OperationKind.DeclarationPattern);
        validator.TagValue("Value").Should().HaveOnlyConstraint(TestConstraint.First, "'var' only propagates existing constraints and ObjectConstraint is missing");
        validator.TagValue("Arg").Should().HaveOnlyConstraint(TestConstraint.First, "'var' only propagates existing constraints and ObjectConstraint is missing");
        validator.TagValue("End").Should().HaveOnlyConstraint(TestConstraint.First, "'var' only propagates existing constraints and ObjectConstraint is missing");
    }

    [TestMethod]
    public void DeclarationPattern_Var_Deconstruction_DoesNotSetConstraint()
    {
        const string code = @"
if (arg is var (a, b))
{
    Tag(""A"", a);
    Tag(""B"", b);
}
if (arg is (var c, var d))
{
    Tag(""C"", c);
    Tag(""D"", d);
}
Tag(""End"", arg);";
        var setter = new PreProcessTestCheck(OperationKind.ParameterReference, x => x.SetSymbolConstraint(x.Operation.Instance.TrackedSymbol(x.State), TestConstraint.First));
        var validator = SETestContext.CreateCS(code, "object arg", setter).Validator;
        validator.ValidateContainsOperation(OperationKind.DeclarationPattern);
        validator.TagValue("A").Should().BeNull();
        validator.TagValue("B").Should().BeNull();
        validator.TagValue("C").Should().BeNull();
        validator.TagValue("D").Should().BeNull();
        validator.TagValues("End").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(TestConstraint.First) && x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(TestConstraint.First) && x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("objectNotNull is null", false)]
    [DataRow("objectNotNull is 1", null)]
    [DataRow(@"objectNotNull is """"", null)]
    [DataRow("objectNull is null", true)]
    [DataRow("objectNull is true", false)]
    [DataRow("objectNull is false", false)]
    [DataRow("nullableBoolTrue is true", true)]
    [DataRow("nullableBoolTrue is false", false)]
    [DataRow("nullableBoolFalse is true", false)]
    [DataRow("nullableBoolFalse is false", true)]
    [DataRow("nullableBoolNull is true", false)]
    [DataRow("nullableBoolNull is false", false)]
    public void ConstantPatternSetBoolConstraint_SingleState(string isPattern, bool? expectedBoolConstraint) =>
        ValidateSetBoolConstraint(isPattern, OperationKindEx.ConstantPattern, expectedBoolConstraint);

    [DataTestMethod]
    [DataRow("objectNotNull", "is true", new[] { ConstraintKind.True, ConstraintKind.NotNull }, new[] { ConstraintKind.NotNull })]
    [DataRow("objectNotNull", "is false", new[] { ConstraintKind.False, ConstraintKind.NotNull }, new[] { ConstraintKind.NotNull })]
    [DataRow("objectNull", "is 1", new[] { ConstraintKind.NotNull }, new[] { ConstraintKind.Null })]          // Should recognize that objectNull doesn't match instead
    [DataRow("objectNull", @"is """"", new[] { ConstraintKind.NotNull }, new[] { ConstraintKind.Null })]      // Should recognize that objectNull doesn't match instead
    [DataRow("objectUnknown", "is null", new[] { ConstraintKind.Null }, new[] { ConstraintKind.NotNull })]
    [DataRow("objectUnknown", "is 1", new[] { ConstraintKind.NotNull }, null)]
    [DataRow("objectUnknown", @"is """"", new[] { ConstraintKind.NotNull }, null)]
    [DataRow("objectUnknown", "is true", new[] { ConstraintKind.True }, null)]
    [DataRow("objectUnknown", "is false", new[] { ConstraintKind.False }, null)]
    [DataRow("nullableBoolUnknown", "is true", new[] { ConstraintKind.True }, null)]
    [DataRow("nullableBoolUnknown", "is false", new[] { ConstraintKind.False }, null)]
    public void ConstantPatternSetBoolConstraint_TwoStates(string testedSymbol, string isPattern, ConstraintKind[] expectedForTrue, ConstraintKind[] expectedForFalse) =>
        ValidateSetBoolConstraint_TwoStates(testedSymbol, isPattern, OperationKindEx.ConstantPattern, expectedForTrue, expectedForFalse);

    [DataTestMethod]
    [DataRow("i == 42", 42, true)]
    [DataRow("i == 16", 42, false)]
    [DataRow("i >= 42", 42, null)]
    [DataRow("i <= 42", 42, null)]
    [DataRow("i >  42", 42, false)]
    [DataRow("i <  42", 42, false)]
    public void ConstantNumberPatternSetBoolConstraint(string rangeCondition, int constant, bool? expectedBoolConstraint)
    {
        var code = $$"""
            if ({{rangeCondition}})
            {
                var result = i is {{constant}};
                Tag("Result", result);
            }
            """;
        var expected = expectedBoolConstraint == null ? null : BoolConstraint.From(expectedBoolConstraint.Value);
        SETestContext.CreateCS(code, "int i").Validator.TagValue("Result").Should().HaveOnlyConstraints(expected, ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("objectNotNull is { }", true)]
    [DataRow("objectNotNull is object { }", true)]
    [DataRow("objectNotNull is string { }", null)]
    [DataRow("objectNull is { }", false)]
    [DataRow("objectNull is object { }", false)]
    [DataRow("stringNotNull is { }", true)]
    [DataRow("stringNotNull is object { }", true)]
    [DataRow("stringNotNull is { Length: 0 }", null)]
    [DataRow("stringNull is { Length: 0 }", false)]
    [DataRow("stringNotNull is { Length: var length }", true)] // only deconstruction
    public void RecursivePatternPropertySubPatternSetBoolConstraint_SingleState(string isPattern, bool? expectedBoolConstraint) =>
        ValidateSetBoolConstraint(isPattern, OperationKindEx.RecursivePattern, expectedBoolConstraint);

    [DataTestMethod]
    [DataRow("objectUnknown", "is { }", new[] { ConstraintKind.Null })]
    [DataRow("objectUnknown", "is object { }", new[] { ConstraintKind.Null })]
    [DataRow("objectUnknown", "is string { }", null)]
    public void RecursivePatternPropertySubPatternSetBoolConstraint_TwoStates(string testedSymbol, string isPattern, ConstraintKind[] expectedForFalse) =>
        ValidateSetBoolConstraint_TwoStates(testedSymbol, isPattern, OperationKindEx.RecursivePattern, new[] { ConstraintKind.NotNull }, expectedForFalse);

    [DataTestMethod]
    [DataRow("deconstructableNotNull is (A: 1, B: 2)", null)]
    [DataRow("deconstructableNotNull is (A: var a, B: _)", true)]
    [DataRow("deconstructableNull is (A: 1, B: 2)", false)]
    [DataRow("deconstructableNull is (A: var a, B: _)", false)]
    public void RecursivePatternDeconstructionSubpatternSetBoolConstraint_SingleState(string isPattern, bool? expectedBoolConstraint) =>
        ValidateSetBoolConstraint(isPattern, OperationKindEx.RecursivePattern, expectedBoolConstraint);

    [TestMethod]
    public void RecursivePatternDeconstructionSubpatternSetBoolConstraint_TwoStates() =>
        ValidateSetBoolConstraint_TwoStates("deconstructableUnknown", "is (A: var a, B: _)", OperationKindEx.RecursivePattern, new[] { ConstraintKind.NotNull }, new[] { ConstraintKind.Null });

    [DataTestMethod]
    [DataRow("objectNull is var a", true)]
    [DataRow("objectNotNull is var a", true)]
    [DataRow("objectUnknown is var a", true)]
    [DataRow("objectNull is object o", false)]
    [DataRow("objectNotNull is object o", true)]
    [DataRow("objectNull is int i", false)]
    [DataRow("objectNotNull is int i", null)]
    [DataRow("exceptionNull is object { }", false)]
    [DataRow("exceptionNull is Exception { }", false)]
    [DataRow("exceptionNull is FormatException { }", false)]
    [DataRow("exceptionNotNull is object { }", true)]
    [DataRow("exceptionNotNull is Exception { }", true)]
    [DataRow("exceptionNotNull is FormatException { }", null)]
    [DataRow("integer is object o", true)]
    public void DeclarationPatternSetBoolConstraint_SingleState(string isPattern, bool? expectedBoolConstraint) =>
        ValidateSetBoolConstraint(isPattern, OperationKindEx.DeclarationPattern, expectedBoolConstraint);

    [DataTestMethod]
    [DataRow("objectUnknown", "is object o", new[] { ConstraintKind.Null })]
    [DataRow("exceptionUnknown", "is object { }", new[] { ConstraintKind.Null })]
    [DataRow("exceptionUnknown", "is Exception { }", new[] { ConstraintKind.Null })]
    [DataRow("exceptionUnknown", "is FormatException { }", null)]
    public void DeclarationPatternSetBoolConstraint_TwoStates(string testedSymbol, string isPattern, ConstraintKind[] expectedForFalse) =>
        ValidateSetBoolConstraint_TwoStates(testedSymbol, isPattern, OperationKindEx.DeclarationPattern, new[] { ConstraintKind.NotNull }, expectedForFalse);

    [DataTestMethod]
    [DataRow("objectNull is not null", OperationKindEx.NegatedPattern, false)]
    [DataRow("objectNull is not { }", OperationKindEx.NegatedPattern, true)]
    [DataRow("objectNull is not object { }", OperationKindEx.NegatedPattern, true)]
    [DataRow("objectNull is not string { }", OperationKindEx.NegatedPattern, true)]
    [DataRow("objectNull is not not null", OperationKindEx.NegatedPattern, true)]
    [DataRow("objectNotNull is not null", OperationKindEx.NegatedPattern, true)]
    [DataRow("objectNotNull is not { }", OperationKindEx.NegatedPattern, false)]
    [DataRow("objectNotNull is not object { }", OperationKindEx.NegatedPattern, false)]
    [DataRow("objectNotNull is not string { }", OperationKindEx.NegatedPattern, null)]
    [DataRow("objectNotNull is not not null", OperationKindEx.NegatedPattern, false)]
    [DataRow("nullableBoolTrue is not true", OperationKindEx.NegatedPattern, null)]     // Should be false
    [DataRow("nullableBoolFalse is not false", OperationKindEx.NegatedPattern, null)]   // Should be false
    [DataRow("objectNull is not object", OperationKindEx.TypePattern, true)]
    [DataRow("objectNull is not not object", OperationKindEx.TypePattern, false)]
    [DataRow("objectNull is not Exception", OperationKindEx.TypePattern, true)]
    [DataRow("objectNull is not not Exception", OperationKindEx.TypePattern, false)]
    [DataRow("objectNotNull is not object", OperationKindEx.TypePattern, false)]
    [DataRow("objectNotNull is not not object", OperationKindEx.TypePattern, true)]
    [DataRow("exceptionNull is not object", OperationKindEx.TypePattern, true)]
    [DataRow("exceptionNull is not not object", OperationKindEx.TypePattern, false)]
    [DataRow("exceptionNotNull is not object", OperationKindEx.TypePattern, false)]
    [DataRow("exceptionNotNull is not not object", OperationKindEx.TypePattern, true)]
    [DataRow("objectNotNull is not Exception", OperationKindEx.TypePattern, null)]
    [DataRow("objectNotNull is not not Exception", OperationKindEx.TypePattern, null)]
    [DataRow("objectNull is not not _", OperationKindEx.DiscardPattern, true)]
    [DataRow("objectNotNull is not not _", OperationKindEx.DiscardPattern, true)]
    [DataRow("objectUnknown is not not _", OperationKindEx.DiscardPattern, null)] // Some patterns always match
    public void NegateTypeDiscardPatternsSetBoolConstraint_SingleState(string isPattern, OperationKind expectedOperation, bool? expectedBoolConstraint) =>
        ValidateSetBoolConstraint(isPattern, expectedOperation, expectedBoolConstraint);

    [DataTestMethod]
    [DataRow("objectUnknown", "is not null", OperationKindEx.NegatedPattern, new[] { ConstraintKind.NotNull }, new[] { ConstraintKind.Null })]
    [DataRow("objectUnknown", "is not { }", OperationKindEx.NegatedPattern, new[] { ConstraintKind.Null }, new[] { ConstraintKind.NotNull })]
    [DataRow("objectUnknown", "is not string { }", OperationKindEx.NegatedPattern, null, new[] { ConstraintKind.NotNull })]
    [DataRow("objectUnknown", "is not object { }", OperationKindEx.NegatedPattern, new[] { ConstraintKind.Null }, new[] { ConstraintKind.NotNull })]
    [DataRow("objectUnknown", "is not not null", OperationKindEx.NegatedPattern, new[] { ConstraintKind.Null }, new[] { ConstraintKind.NotNull })]
    [DataRow("nullableBoolTrue", "is not false", OperationKindEx.NegatedPattern, new[] { ConstraintKind.NotNull, ConstraintKind.True }, new[] { ConstraintKind.NotNull, ConstraintKind.False })]          // Should generate only single state with "true" result instead
    [DataRow("nullableBoolFalse", "is not true", OperationKindEx.NegatedPattern, new[] { ConstraintKind.NotNull, ConstraintKind.False }, new[] { ConstraintKind.True, ConstraintKind.NotNull })]          // Should generate only single state with "true" result instead
    [DataRow("nullableBoolNull", "is not true", OperationKindEx.NegatedPattern, new[] { ConstraintKind.Null }, new[] { ConstraintKind.True, ConstraintKind.Null })]      // Should generate only single state with "true" result instead
    [DataRow("nullableBoolNull", "is not false", OperationKindEx.NegatedPattern, new[] { ConstraintKind.Null }, new[] { ConstraintKind.False, ConstraintKind.Null })]    // Should generate only single state with "true" result instead
    [DataRow("nullableBoolUnknown", "is not true", OperationKindEx.NegatedPattern, null, new[] { ConstraintKind.True })]
    [DataRow("nullableBoolUnknown", "is not false", OperationKindEx.NegatedPattern, null, new[] { ConstraintKind.False })]
    [DataRow("objectUnknown", "is not object", OperationKindEx.TypePattern, null, new[] { ConstraintKind.NotNull })]
    [DataRow("objectUnknown", "is not not object", OperationKindEx.TypePattern, new[] { ConstraintKind.NotNull }, null)]
    [DataRow("exceptionUnknown", "is not object", OperationKindEx.TypePattern, null, new[] { ConstraintKind.NotNull })]
    [DataRow("exceptionUnknown", "is not not object", OperationKindEx.TypePattern, new[] { ConstraintKind.NotNull }, null)]
    [DataRow("objectUnknown", "is not Exception", OperationKindEx.TypePattern, null, new[] { ConstraintKind.NotNull })]
    [DataRow("objectUnknown", "is not not Exception", OperationKindEx.TypePattern, new[] { ConstraintKind.NotNull }, null)]
    public void NegateTypeDiscardPatternsSetBoolConstraint_TwoStates(string testedSymbol, string isPattern, OperationKind expectedOperation, ConstraintKind[] expectedForTrue, ConstraintKind[] expectedForFalse) =>
        ValidateSetBoolConstraint_TwoStates(testedSymbol, isPattern, expectedOperation, expectedForTrue, expectedForFalse);

    [DataTestMethod]
    [DataRow("objectNull is null and not { }", true)]
    [DataRow("objectNotNull is null and not { }", false)]
    [DataRow("objectUnknown is null and not { }", null)]
    [DataRow("stringNull is { Length: 0 } and not null", false)]
    [DataRow("stringNotNull is { Length: 0 } and not null", null)]
    [DataRow("stringUnknown is { Length: 0 } and not null", null)]
    [DataRow("stringNull is not null and { Length: 0 }", false)]
    [DataRow("stringNotNull is not null and { Length: 0 }", null)]
    [DataRow("stringUnknown is not null and { Length: 0 }", null)]
    [DataRow("stringNull is { Length: > 10 } and { Length: < 100 }", false)]
    [DataRow("stringNotNull is { Length: > 10 } and { Length: < 100 }", null)]
    [DataRow("stringUnknown is { Length: > 10 } and { Length: < 100 }", null)]
    [DataRow("objectNull is null or not { }", true)]
    [DataRow("objectNotNull is null or not { }", false)]
    [DataRow("objectUnknown is null or not { }", null)]
    [DataRow("objectNull is null or { }", true)]
    [DataRow("objectNotNull is null or { }", true)]
    [DataRow("objectUnknown is null or { }", null)]  // Should be true. Matches always.
    [DataRow("stringNull is { Length: 0 } or not null", false)]
    [DataRow("stringNotNull is { Length: 0 } or not null", true)]
    [DataRow("stringNotNull is { Length: 0 } or null", null)]
    [DataRow("stringUnknown is { Length: 0 } or not null", null)]
    [DataRow("stringNull is not null or { Length: 0 }", false)]
    [DataRow("stringNotNull is not null or { Length: 0 }", true)]
    [DataRow("stringUnknown is not null or { Length: 0 }", null)]
    [DataRow("stringNull is { Length: > 10 } or { Length: < 100 }", false)]
    [DataRow("stringNotNull is { Length: > 10 } or { Length: < 100 }", null)]
    [DataRow("stringUnknown is { Length: > 10 } or { Length: < 100 }", null)]
    public void AndOrPatternsSetBoolConstraint(string isPattern, bool? expectedBoolConstraint) =>
        ValidateSetBoolConstraint(isPattern, OperationKindEx.BinaryPattern, expectedBoolConstraint);

    [DataTestMethod]
    [DataRow("{ }", OperationKind.RecursivePattern)]
    [DataRow("Exception ex ", OperationKind.DeclarationPattern)]
    [DataRow("Exception", OperationKind.TypePattern)]
    [DataRow("42", OperationKind.ConstantPattern)]
    [DataRow("not 42", OperationKind.NegatedPattern)]
    [DataRow("_", OperationKind.Discard)]
    [DataRow("var _", OperationKind.Discard)]
    public void Pattern_ExistingConstraint_DoesNothing(string pattern, OperationKind expectedOperation)
    {
        var code = @$"
object value = arg switch
{{
    null => null,
    {pattern} when Condition => null, // Should not create arg=Null
    _ => Tag(""Arg"", arg)
}};

static object Tag(string name, object value) => null;";
        var validator = SETestContext.CreateCS(code, "object arg").Validator;
        validator.ValidateContainsOperation(expectedOperation);
        validator.TagValue("Arg").Should().HaveOnlyConstraint(ObjectConstraint.NotNull); // Should not have Null in any case
    }

    [TestMethod]
    public void RelationalPattern_SetsBoolConstraint_Enum()
    {
        const string code = """
            var value = DayOfWeek.Monday;
            var result = value is < DayOfWeek.Friday;
            Tag("Result", result);
            """;
        var validator = SETestContext.CreateCS(code, "int arg").Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True);
    }

    [DataTestMethod]
    [DataRow("is > 41")]
    [DataRow("is >= 41")]
    [DataRow("is >= 42")]
    [DataRow("is < 43")]
    [DataRow("is <= 42")]
    [DataRow("is <= 43")]
    public void RelationalPattern_SetsBoolConstraint_True(string expressionSuffix)
    {
        var code = $"""
            var value = 42;
            var result = value {expressionSuffix};
            Tag("Result", result);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True);
    }

    [DataTestMethod]
    [DataRow("is > 42")]
    [DataRow("is > 43")]
    [DataRow("is >= 43")]
    [DataRow("is < 41")]
    [DataRow("is < 42")]
    [DataRow("is <= 41")]
    public void RelationalPattern_SetsBoolConstraint_False(string expressionSuffix)
    {
        var code = $"""
            var value = 42;
            var result = value {expressionSuffix};
            Tag("Result", result);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False);
    }

    [DataTestMethod]
    [DataRow("43.1 is > 43")]
    [DataRow("'A' is > 'a'")]
    public void RelationalPattern_SetsBoolConstraint_NotSet(string expression)
    {
        var code = $"""
            var result = {expression};
            Tag("Result", result);
            """;
        var validator = SETestContext.CreateCS(code, "int arg").Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("arg is > 42")]
    public void RelationalPattern_SetsBoolConstraint_Branches(string expression)
    {
        var code = $"""
            var result = {expression};
            Tag("Result", result);
            """;
        var validator = SETestContext.CreateCS(code, "int arg").Validator;
        validator.TagValues("Result").Should().SatisfyRespectively(x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True),
                                                                   x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False));
    }

    [DataTestMethod]
    [DataRow("arg is > 42", 43, null, null, 42)]
    [DataRow("arg is >= 42", 42, null, null, 41)]
    [DataRow("arg is < 42", null, 41, 42, null)]
    [DataRow("arg is <= 42", null, 42, 43, null)]
    public void RelationalPattern_SetsNumberConstraint(string expression, int? ifMin, int? ifMax, int? elseMin, int? elseMax)
    {
        var code = $$"""
            if ({{expression}})
            {
                Tag("If", arg);
            }
            else
            {
                Tag("Else", arg);
            }
            """;
        var validator = SETestContext.CreateCS(code, "int arg").Validator;
        if (NumberConstraint.From(ifMin, ifMax) is { } expectedIf)
        {
            validator.TagValue("If").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, expectedIf);
        }
        else
        {
            validator.TagValue("If").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        }
        if (NumberConstraint.From(elseMin, elseMax) is { } expectedElse)
        {
            validator.TagValue("Else").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, expectedElse);
        }
        else
        {
            validator.TagValue("Else").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        }
    }

    [TestMethod]
    public void RelationalPattern_Double_DoesNotLearn() // We only track integer values because NumerConstraint uses BigInteger
    {
        const string code = """
            var result = arg is < 42.0;
            Tag("Result", result);
            Tag("Arg", arg);
            """;
        var validator = SETestContext.CreateCS(code, "double arg").Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
        validator.TagValue("Arg").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
    }

    private static void ValidateSetBoolConstraint(string isPattern, OperationKind expectedOperation, bool? expectedBoolConstraint)
    {
        var validator = CreateSetBoolConstraintValidator(isPattern);
        validator.ValidateContainsOperation(expectedOperation);
        var expectedConstraints = expectedBoolConstraint is bool expected
            ? new SymbolicConstraint[] { ObjectConstraint.NotNull, BoolConstraint.From(expected) }
            : new SymbolicConstraint[] { ObjectConstraint.NotNull };
        validator.TagValue("Result").Should().HaveOnlyConstraints(expectedConstraints);
    }

    private static void ValidateSetBoolConstraint_TwoStates(string testedSymbolName, string isPattern, OperationKind expectedOperation, ConstraintKind[] expectedForTrue, ConstraintKind[] expectedForFalse)
    {
        var validator = CreateSetBoolConstraintValidator($"{testedSymbolName} {isPattern}");
        validator.ValidateContainsOperation(expectedOperation);
        validator.TagValues("Result").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(BoolConstraint.True))
            .And.ContainSingle(x => x.HasConstraint(BoolConstraint.False));
        AssertSymbol(BoolConstraint.True, expectedForTrue);
        AssertSymbol(BoolConstraint.False, expectedForFalse);

        void AssertSymbol(BoolConstraint branchConstraint, ConstraintKind[] expected)
        {
            var result = validator.Symbol("result");
            var testedSymbol = validator.Symbol(testedSymbolName);
            var testedSymbolicValue = validator.TagStates("Result").Should().ContainSingle(x => x[result].HasConstraint(branchConstraint)).Which[testedSymbol];
            if (expected is null)
            {
                testedSymbolicValue.Should().BeNull("we should not learn about the tested symbol in {0} branch", branchConstraint);
            }
            else
            {
                var testedSymbolConstraints = testedSymbolicValue.AllConstraints.Select(x => x.Kind);
                testedSymbolConstraints.Should().BeEquivalentTo(expected, "we are in {0} branch", branchConstraint);
            }
        }
    }

    private static ValidatorTestCheck CreateSetBoolConstraintValidator(string isPattern)
    {
        var code = $$"""
            public void Main()
            {
                var objectNotNull = new object();
                var objectNull = (object)null;
                var objectUnknown = Unknown<object>();
                var exceptionNotNull = new Exception();
                var exceptionNull = (Exception)null;
                var exceptionUnknown = Unknown<Exception>();
                var nullableBoolTrue = (bool?)true;
                var nullableBoolFalse = (bool?)false;
                var nullableBoolNull = (bool?)null;
                var nullableBoolUnknown = Unknown<bool?>();
                var stringNotNull = new string('c', 1);  // Make sure, we learn 's is not null'
                var stringNull = (string)null;
                var stringUnknown = Unknown<string>();
                var integer = new int();
                var deconstructableNull = (Deconstructable)null;
                var deconstructableNotNull = new Deconstructable();
                var deconstructableUnknown = Unknown<Deconstructable>();

                var result = {{isPattern}};
                Tag("Result", result);
            }
            """;
        return SETestContext.CreateCSMethod(code).Validator;
    }
}

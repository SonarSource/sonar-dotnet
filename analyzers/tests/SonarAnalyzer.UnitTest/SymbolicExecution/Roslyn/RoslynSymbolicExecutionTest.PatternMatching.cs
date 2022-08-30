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
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
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
            SETestContext.CreateCS(code).Validator.ValidateTag("Value", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
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
            SETestContext.CreateCS(code).Validator.ValidateTag("Value", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
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
            SETestContext.CreateCS(code).Validator.ValidateTag("Value", x => x.HasConstraint(BoolConstraint.False).Should().BeTrue());
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
            SETestContext.CreateCS(code, ", object arg", check).Validator.TagValues("Value").Should()
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
            var validator = SETestContext.CreateCS(code, ", object arg").Validator;
            validator.ValidateContainsOperation(OperationKind.RecursivePattern);
            validator.ValidateTag("ArgNotNull", x => x.Should().BeNull());  // ToDo: Should learn that it is not null too
            validator.TagValues("End").Should().HaveCount(1).And.OnlyContain(x => x == null);
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
            var validator = SETestContext.CreateCS(code, ", object arg").Validator;
            validator.ValidateContainsOperation(OperationKind.RecursivePattern);
            validator.ValidateTag("Value", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("ArgNotNull", x => x.Should().BeNull());  // ToDo: MMF-2563 should have NotNull instead
            validator.TagValues("End").Should().HaveCount(2).And.OnlyContain(x => x == null);       // 2x because value has different states
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
            var setter = new PreProcessTestCheck(OperationKind.ParameterReference, x => x.SetSymbolConstraint(x.Operation.Instance.TrackedSymbol(), TestConstraint.First));
            var validator = SETestContext.CreateCS(code, ", object arg", setter).Validator;
            validator.ValidateContainsOperation(OperationKind.RecursivePattern);
            validator.ValidateTag("Value", x => x.HasConstraint(TestConstraint.First).Should().BeTrue());
            validator.ValidateTag("Value", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("ArgNotNull", x => x.HasConstraint(TestConstraint.First).Should().BeTrue());
            validator.ValidateTag("ArgNotNull", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeFalse());     // ToDo: MMF-2563 should BeTrue() instead
            validator.TagValues("End").Should().HaveCount(2).And.OnlyContain(x => x != null && x.HasConstraint(TestConstraint.First));  // 2x because value has different states
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
            var setter = new PreProcessTestCheck(OperationKind.ParameterReference, x => x.SetSymbolConstraint(x.Operation.Instance.TrackedSymbol(), TestConstraint.First));
            var validator = SETestContext.CreateCS(code, ", object arg", setter).Validator;
            validator.ValidateContainsOperation(OperationKind.RecursivePattern);
            validator.ValidateTag("Msg", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("Msg", x => x.HasConstraint(TestConstraint.First).Should().BeFalse("Constraint from source value should not be propagated to child property"));
            validator.ValidateTag("Ex", x => x.HasConstraint(TestConstraint.First).Should().BeTrue());
            validator.ValidateTag("Ex", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
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
            var validator = SETestContext.CreateCS(code, ", object arg").Validator;
            validator.ValidateContainsOperation(OperationKind.DeclarationPattern);
            validator.ValidateTag("Value", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("ArgNotNull", x => x.Should().BeNull());  // ToDo: MMF-2563 should have NotNull instead
            validator.TagValues("End").Should().HaveCount(2).And.OnlyContain(x => x == null);       // 2x because value has different states
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
            var setter = new PreProcessTestCheck(OperationKind.ParameterReference, x => x.SetSymbolConstraint(x.Operation.Instance.TrackedSymbol(), TestConstraint.First));
            var validator = SETestContext.CreateCS(code, ", object arg", setter).Validator;
            validator.ValidateContainsOperation(OperationKind.DeclarationPattern);
            validator.ValidateTag("Value", x => x.HasConstraint(TestConstraint.First).Should().BeTrue());
            validator.ValidateTag("Value", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("ArgNotNull", x => x.HasConstraint(TestConstraint.First).Should().BeTrue());
            validator.ValidateTag("ArgNotNull", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeFalse());     // ToDo: MMF-2563 should BeTrue() instead
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
            var validator = SETestContext.CreateCS(code, ", object arg").Validator;
            validator.ValidateContainsOperation(OperationKind.DeclarationPattern);
            validator.ValidateTag("Arg", x => x.Should().BeNull());  // ToDo: MMF-2563 should have NotNull instead
            validator.TagValues("End").Should().HaveCount(1).And.OnlyContain(x => x == null);
        }

        [TestMethod]
        public void DeclarationPattern_Var_PreservePreviousConstraint_DoesNotSetNotNullConstraint()
        {
            const string code = @"
if (arg is var value)
{
    Tag(""Value"", value);
    Tag(""Arg"", arg);
}
Tag(""End"", arg);";
            var setter = new PreProcessTestCheck(OperationKind.ParameterReference, x => x.SetSymbolConstraint(x.Operation.Instance.TrackedSymbol(), TestConstraint.First));
            var validator = SETestContext.CreateCS(code, ", object arg", setter).Validator;
            validator.ValidateContainsOperation(OperationKind.DeclarationPattern);
            validator.ValidateTag("Value", x => x.HasConstraint(TestConstraint.First).Should().BeTrue());
            validator.ValidateTag("Value", x => x.HasConstraint<ObjectConstraint>().Should().BeFalse("'var' only propagates existing constraints"));
            validator.ValidateTag("Arg", x => x.HasConstraint(TestConstraint.First).Should().BeTrue());
            validator.ValidateTag("Arg", x => x.HasConstraint<ObjectConstraint>().Should().BeFalse("'var' only propagates existing constraints"));
            validator.TagValues("End").Should().HaveCount(2).And.OnlyContain(x => x != null && x.HasConstraint(TestConstraint.First));     // 2x because value has different states
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
            var setter = new PreProcessTestCheck(OperationKind.ParameterReference, x => x.SetSymbolConstraint(x.Operation.Instance.TrackedSymbol(), TestConstraint.First));
            var validator = SETestContext.CreateCS(code, ", object arg", setter).Validator;
            validator.ValidateContainsOperation(OperationKind.DeclarationPattern);
            validator.ValidateTag("A", x => x.Should().BeNull());
            validator.ValidateTag("B", x => x.Should().BeNull());
            validator.ValidateTag("C", x => x.Should().BeNull());
            validator.ValidateTag("D", x => x.Should().BeNull());
            validator.ValidateTag("End", x => x.HasConstraint(TestConstraint.First).Should().BeTrue());
        }

        [DataTestMethod]
        [DataRow("objectNotNull is null", false)]
        [DataRow("objectNotNull is 1", null)]
        [DataRow(@"objectNotNull is """"", null)]
        [DataRow("objectNotNull is true", null)]
        [DataRow("objectNotNull is false", null)]
        [DataRow("objectNull is null", true)]
        [DataRow("objectNull is 1", null)]
        [DataRow(@"objectNull is """"", null)]
        [DataRow("objectNull is true", null)]
        [DataRow("objectNull is false", null)]
        [DataRow("objectUnknown is null", null)]
        [DataRow("objectUnknown is 1", null)]
        [DataRow(@"objectUnknown is """"", null)]
        [DataRow("objectUnknown is true", null)]
        [DataRow("objectUnknown is false", null)]
        [DataRow("nullableBoolTrue is true", true)]
        [DataRow("nullableBoolTrue is false", false)]
        [DataRow("nullableBoolFalse is true", false)]
        [DataRow("nullableBoolFalse is false", true)]
        [DataRow("nullableBoolNull is true", null)]  // Should be false.
        [DataRow("nullableBoolNull is false", null)] // Should be false.
        [DataRow("nullableBoolUnknown is true", null)]
        [DataRow("nullableBoolUnknown is false", null)]
        public void ConstantPatternSetBoolConstraint(string isPattern, bool? expectedBoolConstraint) =>
            ValidateSetBoolConstraint(isPattern, OperationKindEx.ConstantPattern, expectedBoolConstraint);

        [DataTestMethod]
        [DataRow("objectNotNull is { }", true)]
        [DataRow("objectNull is { }", false)]
        [DataRow("objectUnknown is { }", null)]
        [DataRow("stringNotNull is { }", true)]
        [DataRow("stringNotNull is { Length: 0 }", null)]
        [DataRow("stringNull is { Length: 0 }", false)]
        [DataRow("stringNotNull is { Length: var length }", true)] // only deconstruction
        public void RecursivePatternPropertySubPatternSetBoolConstraint(string isPattern, bool? expectedBoolConstraint) =>
            ValidateSetBoolConstraint(isPattern, OperationKindEx.RecursivePattern, expectedBoolConstraint);

#if NET

        [DataTestMethod]
        [DataRow("recordNotNull is (A: 1, B: 2)", null)]
        [DataRow("recordNotNull is (A: var a, B: _)", true)]
        [DataRow("recordNull is (A: 1, B: 2)", false)]
        [DataRow("recordNull is (A: var a, B: _)", false)]
        [DataRow("recordUnknown is (A: var a, B: _)", null)]
        public void RecursivePatternDeconstructionSubpatternSetBoolConstraint(string isPattern, bool? expectedBoolConstraint)
        {
            const string variableDeclarations = @"
var recordNotNull = new R(1, 2);
var recordNull = (R)null;
var recordUnknown = Unknown<R>();
";
            ValidateSetBoolConstraint(additionalTypes: "record R(int A, int B);", additionalVariables: variableDeclarations, isPattern, OperationKindEx.RecursivePattern, expectedBoolConstraint);
        }

#endif

        [DataTestMethod]
        [DataRow("objectNull is var a", true)]
        [DataRow("objectNotNull is var a", true)]
        [DataRow("objectUnknown is var a", null)] // Should be "true". Some patterns always match.
        [DataRow("objectNull is object o", false)]
        [DataRow("objectNotNull is object o", true)]
        [DataRow("objectUnknown is object o", null)]
        [DataRow("objectNull is int i", false)]
        [DataRow("objectNotNull is int i", null)]
        [DataRow("integer is object o", true)]
        public void DeclarationPatternSetBoolConstraint(string isPattern, bool? expectedBoolConstraint) =>
            ValidateSetBoolConstraint(isPattern, OperationKindEx.DeclarationPattern, expectedBoolConstraint);

        [DataTestMethod]
        [DataRow("objectNull is not null", OperationKindEx.NegatedPattern, false)]
        [DataRow("objectNull is not { }", OperationKindEx.NegatedPattern, true)]
        [DataRow("objectNull is not not null", OperationKindEx.NegatedPattern, true)]
        [DataRow("objectNotNull is not null", OperationKindEx.NegatedPattern, true)]
        [DataRow("objectNotNull is not { }", OperationKindEx.NegatedPattern, false)]
        [DataRow("objectNotNull is not not null", OperationKindEx.NegatedPattern, false)]
        [DataRow("objectUnknown is not null", OperationKindEx.NegatedPattern, null)]
        [DataRow("objectUnknown is not { }", OperationKindEx.NegatedPattern, null)]
        [DataRow("objectUnknown is not not null", OperationKindEx.NegatedPattern, null)]
        [DataRow("nullableBoolTrue is not true", OperationKindEx.NegatedPattern, null)]     // FN. Should be false
        [DataRow("nullableBoolTrue is not false", OperationKindEx.NegatedPattern, null)]    // FN. Should be true
        [DataRow("nullableBoolFalse is not true", OperationKindEx.NegatedPattern, null)]    // FN. Should be true
        [DataRow("nullableBoolFalse is not false", OperationKindEx.NegatedPattern, null)]   // FN. Should be false
        [DataRow("nullableBoolNull is not true", OperationKindEx.NegatedPattern, null)]     // FN. Should be true
        [DataRow("nullableBoolNull is not false", OperationKindEx.NegatedPattern, null)]    // FN. Should be true
        [DataRow("nullableBoolUnknown is not true", OperationKindEx.NegatedPattern, null)]
        [DataRow("nullableBoolUnknown is not false", OperationKindEx.NegatedPattern, null)]
        [DataRow("objectNull is not object", OperationKindEx.TypePattern, true)]
        [DataRow("objectNull is not not object", OperationKindEx.TypePattern, false)]
        [DataRow("objectNotNull is not object", OperationKindEx.TypePattern, false)]
        [DataRow("objectNotNull is not not object", OperationKindEx.TypePattern, true)]
        [DataRow("objectUnknown is not object", OperationKindEx.TypePattern, null)]
        [DataRow("objectUnknown is not not object", OperationKindEx.TypePattern, null)]
        [DataRow("exceptionNull is not object", OperationKindEx.TypePattern, true)]
        [DataRow("exceptionNull is not not object", OperationKindEx.TypePattern, false)]
        [DataRow("exceptionNotNull is not object", OperationKindEx.TypePattern, false)]
        [DataRow("exceptionNotNull is not not object", OperationKindEx.TypePattern, true)]
        [DataRow("exceptionUnknown is not object", OperationKindEx.TypePattern, null)]
        [DataRow("exceptionUnknown is not not object", OperationKindEx.TypePattern, null)]
        [DataRow("objectNull is not Exception", OperationKindEx.TypePattern, null)]
        [DataRow("objectNull is not not Exception", OperationKindEx.TypePattern, null)]
        [DataRow("objectNotNull is not Exception", OperationKindEx.TypePattern, null)]
        [DataRow("objectNotNull is not not Exception", OperationKindEx.TypePattern, null)]
        [DataRow("objectUnknown is not Exception", OperationKindEx.TypePattern, null)]
        [DataRow("objectUnknown is not not Exception", OperationKindEx.TypePattern, null)]
        [DataRow("objectNull is not not _", OperationKindEx.DiscardPattern, true)]
        [DataRow("objectNotNull is not not _", OperationKindEx.DiscardPattern, true)]
        [DataRow("objectUnknown is not not _", OperationKindEx.DiscardPattern, null)] // FN. Some patterns always match
        public void NegateTypeDiscardPatternsSetBoolConstraint(string isPattern, OperationKind expectedOperation, bool? expectedBoolConstraint) =>
            ValidateSetBoolConstraint(isPattern, expectedOperation, expectedBoolConstraint);

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

        private static void ValidateSetBoolConstraint(string isPattern, OperationKind expectedOperation, bool? expectedBoolConstraint) =>
            ValidateSetBoolConstraint(additionalTypes: string.Empty, additionalVariables: string.Empty, isPattern, expectedOperation, expectedBoolConstraint);

        private static void ValidateSetBoolConstraint(string additionalTypes, string additionalVariables, string isPattern, OperationKind expectedOperation, bool? expectedBoolConstraint)
        {
            var code = @$"
public void Main()
{{
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
    {additionalVariables}

    var result = {isPattern};
    Tag(""Result"", result);
}}

{additionalTypes}
";
            var validator = SETestContext.CreateCSMethod(code).Validator;
            validator.ValidateContainsOperation(expectedOperation);
            validator.ValidateTag("Result", x =>
            {
                if (expectedBoolConstraint is bool expected)
                {
                    x.Should().NotBeNull("we expect {expectedBoolConstraint} on the result");
                    x.HasConstraint(BoolConstraint.From(expected)).Should().BeTrue("we should have learned that result is {0}", expected);
                }
                else if (x != null)
                {
                    x.HasConstraint<BoolConstraint>().Should().BeFalse("we should not learn about the state of result");
                }
            });
        }
    }
}

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
        [DataRow("objectNotNull", "null", false)]
        [DataRow("objectNotNull", "1", null)]
        [DataRow("objectNotNull", @"""""", null)]
        [DataRow("objectNotNull", "true", null)]
        [DataRow("objectNotNull", "false", null)]
        [DataRow("objectNull", "null", true)]
        [DataRow("objectNull", "1", null)]
        [DataRow("objectNull", @"""""", null)]
        [DataRow("objectNull", "true", null)]
        [DataRow("objectNull", "false", null)]
        [DataRow("objectUnknown", "null", null)]
        [DataRow("objectUnknown", "1", null)]
        [DataRow("objectUnknown", @"""""", null)]
        [DataRow("objectUnknown", "true", null)]
        [DataRow("objectUnknown", "false", null)]
        [DataRow("nullableBoolTrue", "true", true)]
        [DataRow("nullableBoolTrue", "false", false)]
        [DataRow("nullableBoolFalse", "true", false)]
        [DataRow("nullableBoolFalse", "false", true)]
        [DataRow("nullableBoolNull", "true", null)]  // FN. Should be false.
        [DataRow("nullableBoolNull", "false", null)] // FN. Should be false.
        [DataRow("nullableBoolUnknown", "true", null)]
        [DataRow("nullableBoolUnknown", "false", null)]
        public void ConstantPatternSetBoolConstraint(string variableName, string isPattern, bool? expectedBoolConstraint)
        {
            const string variableDeclarations = @"
var objectNotNull = new object();
var objectNull = (object)null;
var objectUnknown = Unknown<object>();
var nullableBoolTrue = (bool?)true;
var nullableBoolFalse = (bool?)false;
var nullableBoolNull = (bool?)null;
var nullableBoolUnknown = Unknown<bool?>();
";
            ValidateSetBoolConstraint(variableDeclarations, variableName, isPattern, OperationKindEx.ConstantPattern, expectedBoolConstraint);
        }

        [DataTestMethod]
        [DataRow("objectNotNull", "{ }", true)]
        [DataRow("objectNull", "{ }", false)]
        [DataRow("objectUnknown", "{ }", null)]
        [DataRow("stringNotNull", "{ }", true)]
        [DataRow("stringNotNull", "{ Length: 0 }", null)]
        [DataRow("stringNull", "{ Length: 0 }", false)]
        [DataRow("stringNotNull", "{ Length: var length }", true)] // only deconstruction
        public void RecursivePatternPropertySubPatternSetBoolConstraint(string variableName, string isPattern, bool? expectedBoolConstraint)
        {
            const string variableDeclarations = @"
var objectNotNull = new object();
var objectNull = (object)null;
var objectUnknown = Unknown<object>();
var stringNotNull = new string('c', 1);  // Make sure, we learn 's is not null'
var stringNull = (string)null;
";
            ValidateSetBoolConstraint(variableDeclarations, variableName, isPattern, OperationKindEx.RecursivePattern, expectedBoolConstraint);
        }

#if NET

        [DataTestMethod]
        [DataRow("recordNotNull", "(A: 1, B: 2)", null)]
        [DataRow("recordNotNull", "(A: var a, B: _)", true)]
        [DataRow("recordNull", "(A: 1, B: 2)", false)]
        [DataRow("recordNull", "(A: var a, B: _)", false)]
        [DataRow("recordUnknown", "(A: var a, B: _)", null)]
        public void RecursivePatternDeconstructionSubpatternSetBoolConstraint(string variableName, string isPattern, bool? expectedBoolConstraint)
        {
            const string variableDeclarations = @"
var recordNotNull = new R(1, 2);
var recordNull = (R)null;
var recordUnknown = Unknown<R>();
";
            ValidateSetBoolConstraint(additionalTypes: "record R(int A, int B);", variableDeclarations, variableName, isPattern, OperationKindEx.RecursivePattern, expectedBoolConstraint);
        }

#endif

        [DataTestMethod]
        [DataRow("objectNull", "var a", true)]
        [DataRow("objectNotNull", "var a", true)]
        [DataRow("objectUnknown", "var a", null)] // FN. Should be "true". Some patterns always match.
        [DataRow("objectNull", "object o", false)]
        [DataRow("objectNotNull", "object o", true)]
        [DataRow("objectUnknown", "object o", null)]
        [DataRow("objectNull", "int i", false)]
        [DataRow("objectNotNull", "int i", null)]
        [DataRow("integer", "object o", true)]
        public void DeclarationPatternSetBoolConstraint(string variableName, string isPattern, bool? expectedBoolConstraint)
        {
            const string variableDeclarations = @"
var objectNotNull = new object();
var objectNull = (object)null;
var objectUnknown = Unknown<object>();
var integer = new int();
";
            ValidateSetBoolConstraint(variableDeclarations, variableName, isPattern, OperationKindEx.DeclarationPattern, expectedBoolConstraint);
        }

        [DataTestMethod]
        [DataRow("objectNull", "not null", OperationKindEx.NegatedPattern, false)]
        [DataRow("objectNull", "not { }", OperationKindEx.NegatedPattern, true)]
        [DataRow("objectNull", "not not null", OperationKindEx.NegatedPattern, true)]
        [DataRow("objectNotNull", "not null", OperationKindEx.NegatedPattern, true)]
        [DataRow("objectNotNull", "not { }", OperationKindEx.NegatedPattern, false)]
        [DataRow("objectNotNull", "not not null", OperationKindEx.NegatedPattern, false)]
        [DataRow("objectUnknown", "not null", OperationKindEx.NegatedPattern, null)]
        [DataRow("objectUnknown", "not { }", OperationKindEx.NegatedPattern, null)]
        [DataRow("objectUnknown", "not not null", OperationKindEx.NegatedPattern, null)]
        [DataRow("nullableBoolTrue", "not true", OperationKindEx.NegatedPattern, null)]     // FN. Should be false
        [DataRow("nullableBoolTrue", "not false", OperationKindEx.NegatedPattern, null)]    // FN. Should be true
        [DataRow("nullableBoolFalse", "not true", OperationKindEx.NegatedPattern, null)]    // FN. Should be true
        [DataRow("nullableBoolFalse", "not false", OperationKindEx.NegatedPattern, null)]   // FN. Should be false
        [DataRow("nullableBoolNull", "not true", OperationKindEx.NegatedPattern, null)]     // FN. Should be true
        [DataRow("nullableBoolNull", "not false", OperationKindEx.NegatedPattern, null)]    // FN. Should be true
        [DataRow("nullableBoolUnknown", "not true", OperationKindEx.NegatedPattern, null)]
        [DataRow("nullableBoolUnknown", "not false", OperationKindEx.NegatedPattern, null)]
        [DataRow("objectNull", "not object", OperationKindEx.TypePattern, true)]
        [DataRow("objectNull", "not not object", OperationKindEx.TypePattern, false)]
        [DataRow("objectNotNull", "not object", OperationKindEx.TypePattern, false)]
        [DataRow("objectNotNull", "not not object", OperationKindEx.TypePattern, true)]
        [DataRow("objectUnknown", "not object", OperationKindEx.TypePattern, null)]
        [DataRow("objectUnknown", "not not object", OperationKindEx.TypePattern, null)]
        [DataRow("exceptionNull", "not object", OperationKindEx.TypePattern, true)]
        [DataRow("exceptionNull", "not not object", OperationKindEx.TypePattern, false)]
        [DataRow("exceptionNotNull", "not object", OperationKindEx.TypePattern, false)]
        [DataRow("exceptionNotNull", "not not object", OperationKindEx.TypePattern, true)]
        [DataRow("exceptionUnknown", "not object", OperationKindEx.TypePattern, null)]
        [DataRow("exceptionUnknown", "not not object", OperationKindEx.TypePattern, null)]
        [DataRow("objectNull", "not Exception", OperationKindEx.TypePattern, null)]
        [DataRow("objectNull", "not not Exception", OperationKindEx.TypePattern, null)]
        [DataRow("objectNotNull", "not Exception", OperationKindEx.TypePattern, null)]
        [DataRow("objectNotNull", "not not Exception", OperationKindEx.TypePattern, null)]
        [DataRow("objectUnknown", "not Exception", OperationKindEx.TypePattern, null)]
        [DataRow("objectUnknown", "not not Exception", OperationKindEx.TypePattern, null)]
        [DataRow("objectNull", "not not _", OperationKindEx.DiscardPattern, true)]
        [DataRow("objectNotNull", "not not _", OperationKindEx.DiscardPattern, true)]
        [DataRow("objectUnknown", "not not _", OperationKindEx.DiscardPattern, null)] // FN. Some patterns always match
        public void NegateTypeDiscardPatternsSetBoolConstraint(string variableName, string isPattern, OperationKind expectedOperation, bool? expectedBoolConstraint)
        {
            const string variableDeclarations = @"
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
";
            ValidateSetBoolConstraint(variableDeclarations, variableName, isPattern, expectedOperation, expectedBoolConstraint);
        }

        [DataTestMethod]
        [DataRow("objectNull", "null and not { }", true)]
        [DataRow("objectNotNull", "null and not { }", false)]
        [DataRow("objectUnknown", "null and not { }", null)]
        [DataRow("stringNull", "{ Length: 0 } and not null", false)]
        [DataRow("stringNotNull", "{ Length: 0 } and not null", null)]
        [DataRow("stringUnknown", "{ Length: 0 } and not null", null)]
        [DataRow("stringNull", "not null and { Length: 0 }", false)]
        [DataRow("stringNotNull", "not null and { Length: 0 }", null)]
        [DataRow("stringUnknown", "not null and { Length: 0 }", null)]
        [DataRow("stringNull", "{ Length: > 10 } and { Length: < 100 }", false)]
        [DataRow("stringNotNull", "{ Length: > 10 } and { Length: < 100 }", null)]
        [DataRow("stringUnknown", "{ Length: > 10 } and { Length: < 100 }", null)]

        [DataRow("objectNull", "null or not { }", true)]
        [DataRow("objectNotNull", "null or not { }", false)]
        [DataRow("objectUnknown", "null or not { }", null)]
        [DataRow("objectNull", "null or { }", true)]
        [DataRow("objectNotNull", "null or { }", true)]
        [DataRow("objectUnknown", "null or { }", null)]  // FN. Matches always.
        [DataRow("stringNull", "{ Length: 0 } or not null", false)]
        [DataRow("stringNotNull", "{ Length: 0 } or not null", true)]
        [DataRow("stringNotNull", "{ Length: 0 } or null", null)]
        [DataRow("stringUnknown", "{ Length: 0 } or not null", null)]
        [DataRow("stringNull", "not null or { Length: 0 }", false)]
        [DataRow("stringNotNull", "not null or { Length: 0 }", true)]
        [DataRow("stringUnknown", "not null or { Length: 0 }", null)]
        [DataRow("stringNull", "{ Length: > 10 } or { Length: < 100 }", false)]
        [DataRow("stringNotNull", "{ Length: > 10 } or { Length: < 100 }", null)]
        [DataRow("stringUnknown", "{ Length: > 10 } or { Length: < 100 }", null)]
        public void AndOrPatternsSetBoolConstraint(string variableName, string isPattern, bool? expectedBoolConstraint)
        {
            const string variableDeclarations = @"
var objectNotNull = new object();
var objectNull = (object)null;
var objectUnknown = Unknown<object>();
var stringNotNull = new string('c', 1);  // Make sure, we learn 'stringNotNull is not null'
var stringNull = (string)null;
var stringUnknown = Unknown<string>();
var exceptionNotNull = new Exception();
";
            ValidateSetBoolConstraint(variableDeclarations, variableName, isPattern, OperationKindEx.BinaryPattern, expectedBoolConstraint);
        }

        private static void ValidateSetBoolConstraint(string variableDeclarations,
                                                      string variableName,
                                                      string isPattern,
                                                      OperationKind expectedOperation,
                                                      bool? expectedBoolConstraint)
            => ValidateSetBoolConstraint(additionalTypes: string.Empty, variableDeclarations, variableName, isPattern, expectedOperation, expectedBoolConstraint);

        private static void ValidateSetBoolConstraint(string additionalTypes,
                                                      string variableDeclarations,
                                                      string variableName,
                                                      string isPattern,
                                                      OperationKind expectedOperation,
                                                      bool? expectedBoolConstraint)
        {
            var code = @$"
{variableDeclarations}

var result = {variableName} is {isPattern};
Tag(""Result"", result);";
            var validator = SETestContext.CreateCSWithAddtitionalTypes(code, additionalTypes).Validator;
            validator.ValidateContainsOperation(expectedOperation);
            validator.ValidateTag("Result", x =>
            {
                if (expectedBoolConstraint is bool expected)
                {
                    x.Should().NotBeNull("we expect an constraint on the symbolValue");
                    x.HasConstraint(BoolConstraint.From(expected)).Should().BeTrue("we should have learned that result is {0}", expected);
                }
                else
                {
                    if (x != null)
                    {
                        x.HasConstraint<BoolConstraint>().Should().BeFalse("we should not learn about the state of result");
                    }
                }
            });
        }
    }
}

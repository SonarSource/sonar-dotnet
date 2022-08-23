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

        [TestMethod]
        public void LearnFromObjectContraint_IsNull()
        {
            const string code = @"
var notNull = new object();
var isNull = notNull is null;
Tag(""IsNull"", isNull);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsNull", x => x.HasConstraint(BoolConstraint.False).Should().BeTrue());
        }

        [TestMethod]
        public void LearnFromObjectContraint_IsConstant_Number()
        {
            const string code = @"
var notNull = new object();
var isNull = notNull is 1;
Tag(""IsNull"", isNull);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsNull", x => x.Should().BeNull());
        }

        [TestMethod]
        public void LearnFromObjectContraint_IsConstant_Boolean()
        {
            const string code = @"
var notNull = new object();
var isNull = notNull is true;
Tag(""IsNull"", isNull);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsNull", x => x.Should().BeNull());
        }

        [TestMethod]
        public void LearnFromObjectContraint_IsRecursivePattern_Empty()
        {
            const string code = @"
var notNull = new object();
var isNotNull = notNull is {};
Tag(""IsNotNull"", isNotNull);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsNotNull", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
        }

        [TestMethod]
        public void LearnFromObjectContraint_IsRecursivePattern_WithProps()
        {
            const string code = @"
var s = new string('c', 1);  // Make sure, we learn 's is not null'
var isEmpty = s is { Length: 0 };
Tag(""IsEmpty"", isEmpty);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsEmpty", x => x.Should().BeNull());
        }

        [TestMethod]
        public void LearnFromObjectContraint_IsRecursivePattern_WithDeconstruction()
        {
            const string code = @"
var r = new R(1,2);
var isPattern  = r is (A:1, B:2);
Tag(""IsPattern"", isPattern);";
            var validator = SETestContext.CreateCS(code, additionalTypes: "record R(int A, int B);").Validator;
            validator.ValidateTag("IsPattern", x => x.Should().BeNull());
        }

        [TestMethod]
        public void LearnFromObjectContraint_IsDeclarationPattern_MatchesNull_Null()
        {
            const string code = @"
var nullObject = (object)null;
var alwaysTrue = nullObject is var a;
Tag(""AlwaysTrue"", alwaysTrue);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("AlwaysTrue", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
        }

        [TestMethod]
        public void LearnFromObjectContraint_IsDeclarationPattern_MatchesNull_NotNull()
        {
            const string code = @"
var notNull = new object();
var alwaysTrue = notNull is var a;
Tag(""AlwaysTrue"", alwaysTrue);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("AlwaysTrue", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
        }

        [TestMethod]
        public void LearnFromObjectContraint_IsDeclarationPattern_NotMatchesNull_NotNull()
        {
            const string code = @"
var notNull = new object();
var isNotNull = notNull is object o;
Tag(""IsNotNull"", isNotNull);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsNotNull", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
        }

        [TestMethod]
        public void LearnFromObjectContraint_IsDeclarationPattern_NotMatchesNull_MatchTypeNotAssignable()
        {
            const string code = @"
var notNull = new object();
var isNotNull = notNull is int i;
Tag(""IsNotNull"", isNotNull);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsNotNull", x => x.Should().BeNull());
        }

        [TestMethod]
        public void LearnFromObjectContraint_IsDeclarationPattern_NotMatchesNull_MatchTypeAssignable()
        {
            const string code = @"
var i = 1;
var isNotNull = i is object o;
Tag(""IsNotNull"", isNotNull);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsNotNull", x => x.Should().BeNull()); // Should have BoolConstraint.True. We probably need Compilation.ClassifyConversion to fix this
        }

        [TestMethod]
        public void LearnFromObjectContraint_IsDeclarationPattern_NotMatchesNull_MatchTypeEqual()
        {
            const string code = @"
var nullObject = (object)null;
var isNotNull = nullObject is object o;
Tag(""IsNotNull"", isNotNull);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsNotNull", x => x.HasConstraint(BoolConstraint.False).Should().BeTrue());
        }

        [TestMethod]
        public void LearnFromObjectContraint_IsDeclarationPattern_NotMatchesNull_InputTypeNull()
        {
            const string code = @"
var nullObject = (object)null;
var isNotNull = nullObject is int i;
Tag(""IsNotNull"", isNotNull);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsNotNull", x => x.HasConstraint(BoolConstraint.False).Should().BeTrue());
        }

        [TestMethod]
        public void LearnFromObjectContraint_IsNegatePattern_NotNull()
        {
            const string code = @"
var nullObject = (object)null;
var isNotNull = nullObject is not null;
Tag(""IsNotNull"", isNotNull);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsNotNull", x => x.HasConstraint(BoolConstraint.False).Should().BeTrue());
        }

        [TestMethod]
        public void LearnFromObjectContraint_IsNegatePattern_NotNotNull()
        {
            const string code = @"
var nullObject = (object)null;
var isNotNull = nullObject is not { };
Tag(""IsNotNull"", isNotNull);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsNotNull", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
        }

        [TestMethod]
        public void LearnFromObjectContraint_IsNegatePattern_NotObject()
        {
            const string code = @"
var nullObject = (object)null;
var isNotNull = nullObject is not object;
Tag(""IsNotNull"", isNotNull);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsNotNull", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
        }

        [TestMethod]
        public void LearnFromObjectContraint_IsNegatePattern_NotNotObject()
        {
            const string code = @"
var nullObject = (object)null;
var isNotNull = nullObject is not not object;
Tag(""IsNotNull"", isNotNull);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsNotNull", x => x.HasConstraint(BoolConstraint.False).Should().BeTrue());
        }

        [TestMethod]
        public void LearnFromObjectContraint_IsDiscard()
        {
            const string code = @"
var nullObject = (object)null;
var isDiscard = nullObject is not not _;
Tag(""IsDiscard"", isDiscard);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsDiscard", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
        }

        [TestMethod]
        public void LearnFromObjectContraint_Binary_And_True()
        {
            const string code = @"
var nullObject = (object)null;
var isBinary = nullObject is null and not { };
Tag(""IsBinary"", isBinary);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsBinary", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
        }

        [TestMethod]
        public void LearnFromObjectContraint_Binary_And_False()
        {
            const string code = @"
var nullObject = (object)null;
var isBinary = nullObject is not null and { };
Tag(""IsBinary"", isBinary);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsBinary", x => x.HasConstraint(BoolConstraint.False).Should().BeTrue());
        }

        [TestMethod]
        public void LearnFromObjectContraint_Binary_And_LeftNotDeterministic()
        {
            const string code = @"
var s = new string('c', 1);
var isBinary = s is { Length: 0 } and not null;
Tag(""IsBinary"", isBinary);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsBinary", x => x.Should().BeNull());
        }

        [TestMethod]
        public void LearnFromObjectContraint_Binary_And_RightNotDeterministic()
        {
            const string code = @"
var s = new string('c', 1);
var isBinary = s is not null and { Length: 0 };
Tag(""IsBinary"", isBinary);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsBinary", x => x.Should().BeNull());
        }

        [TestMethod]
        public void LearnFromObjectContraint_Binary_And_BothNotDeterministic()
        {
            const string code = @"
var s = new string('c', 1);
var isBinary = s is { Length: > 10 } and { Length: < 100 };
Tag(""IsBinary"", isBinary);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsBinary", x => x.Should().BeNull());
        }

        [TestMethod]
        public void LearnFromObjectContraint_Binary_Or_True()
        {
            const string code = @"
var nullObject = (object)null;
var isBinary = nullObject is null or not { };
Tag(""IsBinary"", isBinary);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsBinary", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
        }

        [TestMethod]
        public void LearnFromObjectContraint_Binary_Or_False()
        {
            const string code = @"
var nullObject = (object)null;
var isBinary = nullObject is not null or { };
Tag(""IsBinary"", isBinary);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsBinary", x => x.HasConstraint(BoolConstraint.False).Should().BeTrue());
        }

        [TestMethod]
        public void LearnFromObjectContraint_Binary_Or_LeftNotDeterministic()
        {
            const string code = @"
var s = new string('c', 1);
var isBinary = s is { Length: 0 } or not null;
Tag(""IsBinary"", isBinary);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsBinary", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
        }

        [TestMethod]
        public void LearnFromObjectContraint_Binary_Or_RightNotDeterministic()
        {
            const string code = @"
var s = new string('c', 1);
var isBinary = s is not null or { Length: 0 };
Tag(""IsBinary"", isBinary);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsBinary", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
        }

        [TestMethod]
        public void LearnFromObjectContraint_Binary_Or_BothNotDeterministic()
        {
            const string code = @"
var s = new string('c', 1);
var isBinary = s is { Length: < 10 } or { Length: > 100 };
Tag(""IsBinary"", isBinary);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("IsBinary", x => x.Should().BeNull());
        }
    }
}

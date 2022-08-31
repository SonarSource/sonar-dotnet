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
            validator.ValidateTag("True", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
            validator.ValidateTag("False", x => x.HasConstraint(BoolConstraint.False).Should().BeTrue());
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
            validator.ValidateTag("True", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
            validator.ValidateTag("False", x => x.HasConstraint(BoolConstraint.False).Should().BeTrue());
            validator.TagValues("End").Should().HaveCount(2)
                .And.ContainSingle(x => x.HasConstraint(BoolConstraint.True))
                .And.ContainSingle(x => x.HasConstraint(BoolConstraint.False));
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
            var validator = SETestContext.CreateCS(code, ", int arg").Validator;
            validator.ValidateContainsOperation(OperationKind.Unary);
            validator.ValidateTag("Arg", x => x.Should().BeNull());
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
            var validator = SETestContext.CreateCS(code, ", ICollection<object> collection", check).Validator;
            validator.ValidateTag("If", x => x.HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
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
            validator.ValidateTag("True", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
            validator.ValidateTag("False", x => x.HasConstraint(BoolConstraint.False).Should().BeTrue());
            validator.ValidateTag("Value", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());    // Visited only for "true" condition
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
            validator.ValidateTag("If", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.ValidateTag("Else", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
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
            validator.ValidateTag("If", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("Else", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.TagValues("End").Should().HaveCount(2)
                .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
                .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
        }

        [DataTestMethod]
        [DataRow("arg == isObject")]
        [DataRow("isObject == arg")]
        public void Branching_LearnsObjectConstraint_Binary_UndefinedInOtherBranch_CS(string expression)
        {
            var validator = CreateIfElseEndValidatorCS(expression, OperationKind.Binary);
            validator.ValidateTag("If", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("Else", x => x.Should().BeNull("We can't tell if it is Null or NotNull in this branch"));
            validator.TagValues("End").Should().HaveCount(2)
                .And.ContainSingle(x => x == null)
                .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
        }

        [DataTestMethod]
        [DataRow("arg != isObject")]
        [DataRow("isObject != arg")]
        public void Branching_LearnsObjectConstraint_Binary_UndefinedInOtherBranch_Negated_CS(string expression)
        {
            var validator = CreateIfElseEndValidatorCS(expression, OperationKind.Binary);
            validator.ValidateTag("If", x => x.Should().BeNull("We can't tell if it is Null or NotNull in this branch"));
            validator.ValidateTag("Else", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
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
            validator.ValidateTag("If", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.ValidateTag("Else", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
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
            validator.ValidateTag("If", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("Else", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
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
            validator.ValidateTag("If", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
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
            validator.ValidateTag("Else", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.TagValues("End").Should().HaveCount(2)
                .And.ContainSingle(x => x == null)
                .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
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
            validator.ValidateTag("If", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
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
            validator.ValidateTag("Else", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.TagValues("End").Should().HaveCount(2)
                .And.ContainSingle(x => x == null)
                .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
        }

        [DataTestMethod]
        [DataRow("arg is null")]
        [DataRow("!!(arg is null)")]
        [DataRow("arg is not not null")]
        public void Branching_LearnsObjectConstraint_ConstantPattern_Null(string expression)
        {
            var validator = CreateIfElseEndValidatorCS(expression, OperationKind.ConstantPattern);
            validator.ValidateTag("If", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.ValidateTag("Else", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.TagValues("End").Should().HaveCount(2)
                .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
                .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
        }

        [DataTestMethod]
        [DataRow("!(arg is null)")]
        [DataRow("!!!(arg is null)")]
        [DataRow("arg is not null")]
        public void Branching_LearnsObjectConstraint_ConstantPattern_Null_Negated(string expression)
        {
            var validator = CreateIfElseEndValidatorCS(expression, OperationKind.ConstantPattern);
            validator.ValidateTag("If", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("Else", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.TagValues("End").Should().HaveCount(2)
                .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
                .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
        }

        [DataTestMethod]
        [DataRow(@"arg is ""some text""")]
        [DataRow(@"arg is """"")]
        public void Branching_LearnsObjectConstraint_ConstantPattern_String(string expression)
        {
            var validator = CreateIfElseEndValidatorCS(expression, OperationKind.ConstantPattern);
            validator.ValidateTag("If", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("Else", x => x.Should().BeNull());
            validator.TagValues("End").Should().HaveCount(2)
                .And.ContainSingle(x => x == null)
                .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
        }

        [DataTestMethod]
        [DataRow("arg is true")]
        [DataRow("!!(arg is true)")]
        [DataRow("arg is not not true")]
        public void Branching_LearnsObjectConstraint_ConstantPattern_True(string expression)
        {
            var validator = CreateIfElseEndValidatorCS(expression, OperationKind.ConstantPattern);
            validator.ValidateTag("If", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
            validator.ValidateTag("Else", x => x.Should().BeNull("it could be False, null or any other type"));
            validator.TagValues("End").Should().HaveCount(2)
                .And.ContainSingle(x => x == null)
                .And.ContainSingle(x => x != null && x.HasConstraint(BoolConstraint.True));
        }

        [DataTestMethod]
        [DataRow("arg is not true")]
        public void Branching_LearnsObjectConstraint_ConstantPattern_True_Negated(string expression)
        {
            var validator = CreateIfElseEndValidatorCS(expression, OperationKind.ConstantPattern);
            validator.ValidateTag("If", x => x.Should().BeNull("it could be False, null or any other type"));
            validator.ValidateTag("Else", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
            validator.TagValues("End").Should().HaveCount(2)
                .And.ContainSingle(x => x == null)
                .And.ContainSingle(x => x != null && x.HasConstraint(BoolConstraint.True));
        }

        [DataTestMethod]
        [DataRow("arg is false")]
        [DataRow("!!(arg is false)")]
        public void Branching_LearnsObjectConstraint_ConstantPattern_False(string expression)
        {
            var validator = CreateIfElseEndValidatorCS(expression, OperationKind.ConstantPattern);
            validator.ValidateTag("If", x => x.HasConstraint(BoolConstraint.False).Should().BeTrue());
            validator.ValidateTag("Else", x => x.Should().BeNull("it could be True, null or any other type"));
            validator.TagValues("End").Should().HaveCount(2)
                .And.ContainSingle(x => x == null)
                .And.ContainSingle(x => x != null && x.HasConstraint(BoolConstraint.False));
        }

        [DataTestMethod]
        [DataRow("arg is 42")]
        [DataRow("arg is System.ConsoleKey.Enter")]    // Enum
        public void Branching_LearnsObjectConstraint_ConstantPattern_Other(string expression)
        {
            var validator = CreateIfElseEndValidatorCS(expression, OperationKind.ConstantPattern);
            validator.ValidateTag("If", x => x.Should().BeNull());
            validator.ValidateTag("Else", x => x.Should().BeNull());
            validator.ValidateTag("End", x => x.Should().BeNull());
        }

        [DataTestMethod]
        [DataRow("arg is not not object")]
        [DataRow("arg is not not Exception")]
        public void Branching_LearnsObjectConstraint_TypePattern(string expression)
        {
            var validator = CreateIfElseEndValidatorCS(expression, OperationKind.TypePattern);
            validator.ValidateTag("If", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
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
            validator.ValidateTag("Else", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.TagValues("End").Should().HaveCount(2)
                .And.ContainSingle(x => x == null)
                .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));
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
            var validator = SETestContext.CreateCS(code, ", object arg").Validator;
            validator.ValidateContainsOperation(OperationKind.FlowCaptureReference);
            validator.ValidateTag("Null", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.ValidateTag("Default", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.TagValues("End").Should().HaveCount(2)
                .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
                .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
        }

        private static ValidatorTestCheck CreateIfElseEndValidatorCS(string expression, OperationKind expectedOperation)
        {
            var code = @$"
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
Tag(""End"", arg);";
            var validator = SETestContext.CreateCS(code, ", object arg").Validator;
            validator.ValidateContainsOperation(expectedOperation);
            return validator;
        }

        private static ValidatorTestCheck CreateIfElseEndValidatorVB(string expression, OperationKind expectedOperation)
        {
            var code = @$"
If {expression} Then
    Tag(""If"", Arg)
Else
    Tag(""Else"", Arg)
End If
Tag(""End"", Arg)";
            var validator = SETestContext.CreateVB(code, ", Arg As Object").Validator;
            validator.ValidateContainsOperation(expectedOperation);
            return validator;
        }
    }
}

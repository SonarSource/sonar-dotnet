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
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn;

public partial class RoslynSymbolicExecutionTest
{
    [DataTestMethod]
    [DataRow("isTrue == true", true)]
    [DataRow("isTrue == false", false)]
    [DataRow("isTrue == isTrue", true)]
    [DataRow("isTrue == isFalse", false)]
    [DataRow("isTrue == isNullableNull", false)]
    [DataRow("isTrue == isNullableTrue", true)]
    [DataRow("isTrue == isNullableFalse", false)]
    [DataRow("isFalse == true", false)]
    [DataRow("isFalse == false", true)]
    [DataRow("isFalse == isTrue", false)]
    [DataRow("isFalse == isFalse", true)]
    [DataRow("isFalse == isNullableNull", false)]
    [DataRow("isFalse == isNullableTrue", false)]
    [DataRow("isFalse == isNullableFalse", true)]
    [DataRow("isTrue != true", false)]
    [DataRow("isTrue != false", true)]
    [DataRow("isTrue != isTrue", false)]
    [DataRow("isTrue != isFalse", true)]
    [DataRow("isTrue != isNullableNull", true)]
    [DataRow("isTrue != isNullableTrue", false)]
    [DataRow("isTrue != isNullableFalse", true)]
    [DataRow("isFalse != true", true)]
    [DataRow("isFalse != false", false)]
    [DataRow("isFalse != isTrue", true)]
    [DataRow("isFalse != isFalse", false)]
    [DataRow("isFalse != isNullableNull", true)]
    [DataRow("isFalse != isNullableTrue", true)]
    [DataRow("isFalse != isNullableFalse", false)]
    [DataRow("true == isTrue", true)]
    [DataRow("false == isTrue", false)]
    [DataRow("isNullableNull == isTrue", false)]
    [DataRow("isNullableTrue == isTrue", true)]
    [DataRow("true != isTrue", false)]
    [DataRow("false != isTrue", true)]
    [DataRow("isNullableNull != isTrue", true)]
    [DataRow("isNullableTrue != isTrue", false)]
    public void Binary_BoolOperands_Equals_CS(string expression, bool expected)
    {
        var code = $"""
            bool isTrue = true;
            bool isFalse = false;
            bool? isNullableNull = null;
            bool? isNullableTrue = true;
            bool? isNullableFalse = false;
            var result = {expression};
            Tag("Result", result);
            """;
        SETestContext.CreateCS(code).Validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.From(expected));
    }

    [DataTestMethod]
    [DataRow("IsTrue = True", true)]
    [DataRow("IsTrue = False", false)]
    [DataRow("IsTrue = IsTrue", true)]
    [DataRow("IsTrue = IsFalse", false)]
    [DataRow("IsTrue = IsNullableNull", false)]
    [DataRow("IsTrue = IsNullableTrue", true)]
    [DataRow("IsTrue = IsNullableFalse", false)]
    [DataRow("IsFalse = True", false)]
    [DataRow("IsFalse = False", true)]
    [DataRow("IsFalse = IsTrue", false)]
    [DataRow("IsFalse = IsFalse", true)]
    [DataRow("IsFalse = IsNullableNull", false)]
    [DataRow("IsFalse = IsNullableTrue", false)]
    [DataRow("IsFalse = IsNullableFalse", true)]
    [DataRow("IsTrue <> True", false)]
    [DataRow("IsTrue <> False", true)]
    [DataRow("IsTrue <> IsTrue", false)]
    [DataRow("IsTrue <> IsFalse", true)]
    [DataRow("IsTrue <> IsNullableNull", true)]
    [DataRow("IsTrue <> IsNullableTrue", false)]
    [DataRow("IsTrue <> IsNullableFalse", true)]
    [DataRow("IsFalse <> True", true)]
    [DataRow("IsFalse <> False", false)]
    [DataRow("IsFalse <> IsTrue", true)]
    [DataRow("IsFalse <> IsFalse", false)]
    [DataRow("IsFalse <> IsNullableNull", true)]
    [DataRow("IsFalse <> IsNullableTrue", true)]
    [DataRow("IsFalse <> IsNullableFalse", false)]
    [DataRow("True = IsTrue", true)]
    [DataRow("False = IsTrue", false)]
    [DataRow("IsNullableNull = IsTrue", false)]
    [DataRow("IsNullableTrue = IsTrue", true)]
    [DataRow("True <> IsTrue", false)]
    [DataRow("False <> IsTrue", true)]
    [DataRow("IsNullableNull <> IsTrue", true)]
    [DataRow("IsNullableTrue <> IsTrue", false)]
    public void Binary_BoolOperands_Equals_VB(string expression, bool expected)
    {
        var code = $"""
            Dim IsTrue As Boolean= true
            Dim IsFalse As Boolean= false
            Dim IsNullableNull As Boolean? = Nothing
            Dim IsNullableTrue As Boolean? = True
            Dim IsNullableFalse As Boolean? = False
            Dim Result As Integer = {expression}
            Tag("Result", Result)
            """;
        SETestContext.CreateVB(code).Validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.From(expected));
    }

    [TestMethod]
    public void Binary_BoolOperands_And()
    {
        const string code = """
                var isTrue = true;
                var isFalse = false;

                if (isTrue & true)
                    Tag("True & True");
                else
                    Tag("True & True Unreachable");

                if (false & isTrue)
                    Tag("False & True Unreachable");
                else
                    Tag("False & True");

                if (false & isFalse)
                    Tag("False & False Unreachable");
                else
                    Tag("False & False");

                if (isFalse & arg)
                    Tag("isFalse & arg True Unreachable");
                else
                    Tag("isFalse & arg False");

                if (arg & isFalse)
                    Tag("arg & isFalse True Unreachable");
                else
                    Tag("arg & isFalse False");

                if (isTrue & arg)
                    Tag("isTrue & arg True");
                else
                    Tag("isTrue & arg False");

                if (arg & isTrue)
                    Tag("arg & isTrue True");
                else
                    Tag("arg & isTrue False");

                if (isTrue && true)
                    Tag("True && True");
                else
                    Tag("True && True Unreachable");

                if (isFalse && true)
                    Tag("False && True Unreachable");
                else
                    Tag("False && True");
                """;
        SETestContext.CreateCS(code, "bool arg").Validator.ValidateTagOrder(
            "True & True",
            "False & True",
            "False & False",
            "isFalse & arg False",
            "arg & isFalse False",
            "isTrue & arg True",
            "isTrue & arg False",
            "arg & isTrue True",
            "arg & isTrue False",
            "True && True",
            "False && True");
    }

    [TestMethod]
    public void Binary_BoolOperands_Or()
    {
        const string code = """
                var isTrue = true;
                var isFalse = false;

                if (isTrue | true)
                    Tag("True | True");
                else
                    Tag("True | True Unreachable");

                if (false | isTrue)
                    Tag("False | True");
                else
                    Tag("False | True Unreachable");

                if (false | isFalse)
                    Tag("False | False Unreachable");
                else
                    Tag("False | False");

                if (isTrue | arg)
                    Tag("isTrue | arg True");
                else
                    Tag("isTrue | arg False Unreachable");

                if (arg | isTrue)
                    Tag("arg | isTrue True");
                else
                    Tag("arg | isTrue False Unreachable");

                if (isFalse | arg)
                    Tag("isFalse | arg True");
                else
                    Tag("isFalse | arg False");

                if (arg | isFalse)
                    Tag("arg | isFalse True");
                else
                    Tag("arg | isFalse False");

                if (isTrue || true)
                    Tag("True || True");
                else
                    Tag("True || True Unreachable");

                if (isFalse || true)
                    Tag("False || True");
                else
                    Tag("False || True Unreachable");
                """;
        SETestContext.CreateCS(code, "bool arg").Validator.ValidateTagOrder(
            "True | True",
            "False | True",
            "False | False",
            "isTrue | arg True",
            "arg | isTrue True",
            "isFalse | arg True",
            "isFalse | arg False",
            "arg | isFalse True",
            "arg | isFalse False",
            "True || True",
            "False || True");
    }

    [TestMethod]
    public void Binary_BoolOperands_Xor()
    {
        const string code = @"
var isTrue = true;
var isFalse = false;

if (isTrue ^ true)
    Tag(""True ^ True Unreachable"");
else
    Tag(""True ^ True"");

if (false ^ isTrue)
    Tag(""False ^ True"");
else
    Tag(""False ^ True Unreachable"");

if (isTrue ^ false)
    Tag(""True ^ False"");
else
    Tag(""True ^ False Unreachable"");

if (false ^ isFalse)
    Tag(""False ^ False Unreachable"");
else
    Tag(""False ^ False"");";
        SETestContext.CreateCS(code).Validator.ValidateTagOrder("True ^ True", "False ^ True", "True ^ False", "False ^ False");
    }

    [DataTestMethod]
    [DataRow("boolParameter & isTrue")]
    [DataRow("isTrue & boolParameter")]
    public void Binary_NoConstraint_VisitsBothBranches(string condition)
    {
        var code = $@"
bool isTrue = true;
if ({condition})
{{
    Tag(""If"");
}}
else
{{
    Tag(""Else"");
}}
Tag(""End"");";
        SETestContext.CreateCS(code).Validator.ValidateTagOrder(
            "If",
            "Else",
            "End");
    }

    [DataTestMethod]
    [DataRow("boolParameter & isTrue")]
    [DataRow("isTrue & boolParameter")]
    public void Binary_OtherConstraint_VisitsBothBranches(string condition)
    {
        var code = $@"
bool isTrue = true;
if ({condition})
{{
    Tag(""If"");
}}
else
{{
    Tag(""Else"");
}}
Tag(""End"");";
        var check = new PostProcessTestCheck(OperationKind.ParameterReference, x => x.SetOperationConstraint(DummyConstraint.Dummy));
        SETestContext.CreateCS(code, check).Validator.ValidateTagOrder(
            "If",
            "Else",
            "End");
    }

    [TestMethod]
    public void Binary_UnexpectedOperator_VisitsBothBranches()
    {
        var code = $@"
if (a > b)      // Both, 'a' and 'b' have bool constraint (weird) and we do not produce bool constraint for '>' binary operator, because it doesn't make sense.
{{
    Tag(""If"");
}}
else
{{
    Tag(""Else"");
}}
Tag(""End"");";
        var check = new PostProcessTestCheck(OperationKind.ParameterReference, x => x.SetOperationConstraint(BoolConstraint.True));
        SETestContext.CreateCS(code, "int a, int b", check).Validator.ValidateTagOrder(
            "If",
            "Else",
            "End");
    }

    [TestMethod]
    public void BinaryEqualsNull_SetsBoolConstraint_KnownResult_CS()
    {
        const string code = @"
object nullValue = null;
object notNullValue = new object();
var isTrue = nullValue == null;
var isFalse = notNullValue == null;
var forNullNull = null == null;
var forNullSymbol = null == nullValue;
var forSymbolSymbolTrue = nullValue == nullValue;
var forSymbolSymbolFalse = notNullValue == nullValue;
var forSymbolSymbolNone = notNullValue == notNullValue;
Tag(""IsTrue"", isTrue);
Tag(""IsFalse"", isFalse);
Tag(""ForNullNull"", forNullNull);
Tag(""ForNullSymbol"", forNullSymbol);
Tag(""ForSymbolSymbolTrue"", forSymbolSymbolTrue);
Tag(""ForSymbolSymbolFalse"", forSymbolSymbolFalse);
Tag(""ForSymbolSymbolNone"", forSymbolSymbolNone);";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKind.Binary);
        validator.TagValue("IsTrue").Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull);
        validator.TagValue("IsFalse").Should().HaveOnlyConstraints(BoolConstraint.False, ObjectConstraint.NotNull);
        validator.TagValue("ForNullNull").Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull);  // null == null is Literal with constant value 'true'
        validator.TagValue("ForNullSymbol").Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull);
        validator.TagValue("ForSymbolSymbolTrue").Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull);
        validator.TagValue("ForSymbolSymbolFalse").Should().HaveOnlyConstraints(BoolConstraint.False, ObjectConstraint.NotNull);
        validator.ValidateTag("ForSymbolSymbolNone", x => x.HasConstraint<BoolConstraint>().Should().BeFalse("We can't tell if two instances are equivalent."));
    }

    [DataTestMethod]
    [DataRow("arg == null")]
    [DataRow("arg == nullValue")]
    public void BinaryEqualsNull_SetsBoolConstraint_Unknown_ComparedToNull_CS(string expression)
    {
        var code = @$"
object nullValue = null;
var value = {expression};
Tag(""End"");";
        var validator = SETestContext.CreateCS(code, "object arg").Validator;
        validator.ValidateContainsOperation(OperationKind.Binary);
        validator.TagStates("End").Should().HaveCount(2)
            .And.ContainSingle(state => state.SymbolsWith(BoolConstraint.True).Any(x => x.Name == "value") && state.SymbolsWith(ObjectConstraint.Null).Any(x => x.Name == "arg"))
            .And.ContainSingle(state => state.SymbolsWith(BoolConstraint.False).Any(x => x.Name == "value") && state.SymbolsWith(ObjectConstraint.NotNull).Any(x => x.Name == "arg"));
    }

    [TestMethod]
    public void BinaryEqualsNull_SetsBoolConstraint_Unknown_ComparedToNotNull_CS()
    {
        const string code = @"
object notNullValue = new object();
var value = arg == notNullValue;
Tag(""End"");";
        var validator = SETestContext.CreateCS(code, "object arg").Validator;
        validator.ValidateContainsOperation(OperationKind.Binary);
        validator.TagStates("End").Should().HaveCount(2)    // When False, we can't tell what constraints "args" have
            .And.ContainSingle(state => state.SymbolsWith(BoolConstraint.True).Any(x => x.Name == "value") && state.SymbolsWith(ObjectConstraint.NotNull).Any(x => x.Name == "arg"))
            .And.ContainSingle(state => state.SymbolsWith(BoolConstraint.False).Any(x => x.Name == "value") && state.SymbolsWith(ObjectConstraint.NotNull).All(x => x.Name != "arg"));
    }

    [TestMethod]
    public void BinaryEqualsNull_SetsBoolConstraint_KnownResult_VB()
    {
        const string code = @"
Dim NullValue As Object = Nothing
Dim NotNullValue As New Object()
Dim IsTrue As Boolean = NullValue Is Nothing
Dim IsFalse = NotNullValue Is Nothing
Tag(""IsTrue"", IsTrue)
Tag(""IsFalse"", IsFalse)";
        var validator = SETestContext.CreateVB(code).Validator;
        validator.ValidateContainsOperation(OperationKind.Binary);
        validator.TagValue("IsTrue").Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull);
        validator.TagValue("IsFalse").Should().HaveOnlyConstraints(BoolConstraint.False, ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("Arg Is Nothing")]
    [DataRow("Arg = Nothing")]
    [DataRow("Nothing = Arg ")]
    public void BinaryEqualsNull_SetsBoolConstraint_Unknown_ComparedToNull_VB(string expression)
    {
        var code = @$"
Dim Value = {expression}
Tag(""End"")";
        var setter = new PreProcessTestCheck(OperationKind.Literal, x => x.SetOperationConstraint(NumberConstraint.From(0))); // Coverage of Flip local function with ObjectValueEquals
        var validator = SETestContext.CreateVB(code, "Arg As Object", setter).Validator;
        validator.ValidateContainsOperation(OperationKind.Binary);
        validator.TagStates("End").Should().HaveCount(2)
            .And.ContainSingle(state => state.SymbolsWith(BoolConstraint.True).Any(x => x.Name == "Value") && state.SymbolsWith(ObjectConstraint.Null).Any(x => x.Name == "Arg"))
            .And.ContainSingle(state => state.SymbolsWith(BoolConstraint.False).Any(x => x.Name == "Value") && state.SymbolsWith(ObjectConstraint.NotNull).Any(x => x.Name == "Arg"));
    }

    [TestMethod]
    public void BinaryNotEqualsNull_SetsBoolConstraint_KnownResult_CS()
    {
        const string code = @"
object nullValue = null;
object notNullValue = new object();
var isTrue = notNullValue != null;
var isFalse = nullValue != null;
var forNullNull = null != null;
var forNullSymbol = null != nullValue;
var forSymbolSymbolTrue = notNullValue != nullValue;
var forSymbolSymbolFalse = nullValue != nullValue;
var forSymbolSymbolNone = notNullValue != notNullValue;
Tag(""IsTrue"", isTrue);
Tag(""IsFalse"", isFalse);
Tag(""ForNullNull"", forNullNull);
Tag(""ForNullSymbol"", forNullSymbol);
Tag(""ForSymbolSymbolTrue"", forSymbolSymbolTrue);
Tag(""ForSymbolSymbolFalse"", forSymbolSymbolFalse);
Tag(""ForSymbolSymbolNone"", forSymbolSymbolNone);";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKind.Binary);
        validator.TagValue("IsTrue").Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull);
        validator.TagValue("IsFalse").Should().HaveOnlyConstraints(BoolConstraint.False, ObjectConstraint.NotNull);
        validator.TagValue("ForNullNull").Should().HaveOnlyConstraints(BoolConstraint.False, ObjectConstraint.NotNull);  // null != null is Literal with constant value 'false'
        validator.TagValue("ForNullSymbol").Should().HaveOnlyConstraints(BoolConstraint.False, ObjectConstraint.NotNull);
        validator.TagValue("ForSymbolSymbolTrue").Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull);
        validator.TagValue("ForSymbolSymbolFalse").Should().HaveOnlyConstraints(BoolConstraint.False, ObjectConstraint.NotNull);
        validator.ValidateTag("ForSymbolSymbolNone", x => x.HasConstraint<BoolConstraint>().Should().BeFalse("We can't tell if two instances are equivalent."));
    }

    [DataTestMethod]
    [DataRow("arg != null")]
    [DataRow("arg != nullValue")]

    public void BinaryNotEqualsNull_SetsBoolConstraint_ComparedToNull_CS(string expression)
    {
        var code = @$"
object nullValue = null;
var value = {expression};
Tag(""End"");";
        var validator = SETestContext.CreateCS(code, "object arg").Validator;
        validator.ValidateContainsOperation(OperationKind.Binary);
        validator.TagStates("End").Should().HaveCount(2)
            .And.ContainSingle(state => state.SymbolsWith(BoolConstraint.True).Any(x => x.Name == "value") && state.SymbolsWith(ObjectConstraint.NotNull).Any(x => x.Name == "arg"))
            .And.ContainSingle(state => state.SymbolsWith(BoolConstraint.False).Any(x => x.Name == "value") && state.SymbolsWith(ObjectConstraint.Null).Any(x => x.Name == "arg"));
    }

    [TestMethod]
    public void BinaryNotEqualsNull_SetsBoolConstraint_ComparedToNotNull_CS()
    {
        const string code = @"
object notNullValue = new object();
var value = arg != notNullValue;
Tag(""End"");";
        var validator = SETestContext.CreateCS(code, "object arg").Validator;
        validator.ValidateContainsOperation(OperationKind.Binary);
        validator.TagStates("End").Should().HaveCount(2)    // When True, we can't tell what constraints "args" have
            .And.ContainSingle(state => state.SymbolsWith(BoolConstraint.True).Any(x => x.Name == "value") && state.SymbolsWith(ObjectConstraint.NotNull).All(x => x.Name != "arg"))
            .And.ContainSingle(state => state.SymbolsWith(BoolConstraint.False).Any(x => x.Name == "value") && state.SymbolsWith(ObjectConstraint.NotNull).Any(x => x.Name == "arg"));
    }

    [TestMethod]
    public void BinaryNotEqualsNull_SetsBoolConstraint_KnownResult_VB()
    {
        const string code = @"
Dim NullValue As Object = Nothing
Dim NotNullValue As New Object()
Dim IsTrue = NotNullValue IsNot Nothing
Dim IsFalse As Boolean = NullValue IsNot Nothing
Tag(""IsTrue"", IsTrue)
Tag(""IsFalse"", IsFalse)";
        var validator = SETestContext.CreateVB(code).Validator;
        validator.ValidateContainsOperation(OperationKind.Binary);
        validator.TagValue("IsTrue").Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull);
        validator.TagValue("IsFalse").Should().HaveOnlyConstraints(BoolConstraint.False, ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("Arg IsNot Nothing")]
    [DataRow("Arg <> Nothing")]
    [DataRow("Nothing <> Arg")]
    public void BinaryNotEqualsNull_SetsBoolConstraint_Unknown_ComparedToNull_VB(string expression)
    {
        var code = @$"
Dim Value = {expression}
Tag(""End"")";
        var validator = SETestContext.CreateVB(code, "Arg As Object").Validator;
        validator.ValidateContainsOperation(OperationKind.Binary);
        validator.TagStates("End").Should().HaveCount(2)
            .And.ContainSingle(state => state.SymbolsWith(BoolConstraint.True).Any(x => x.Name == "Value") && state.SymbolsWith(ObjectConstraint.NotNull).Any(x => x.Name == "Arg"))
            .And.ContainSingle(state => state.SymbolsWith(BoolConstraint.False).Any(x => x.Name == "Value") && state.SymbolsWith(ObjectConstraint.Null).Any(x => x.Name == "Arg"));
    }

    [DataTestMethod]
    [DataRow("arg >= null")]
    [DataRow("arg > null")]
    [DataRow("arg < null")]
    [DataRow("arg <= null")]
    [DataRow("null >= arg")]
    [DataRow("null > arg")]
    [DataRow("null < arg")]
    [DataRow("null <= arg")]
    [DataRow("arg > (int?)null")]
    [DataRow("arg > new Nullable<int>()")]
    [DataRow("arg > (null as int?)")]
    [DataRow("arg > nullValue")]
    [DataRow("nullValue > arg")]
    [DataRow("nullValue > 42")]
    [DataRow("nullValue > notNullValue")]
    [DataRow("notNullValue > nullValue")]
    public void Binary_NullableRelationalNull_SetsBoolConstraint_CS(string expression)
    {
        var code = $$"""
            int? notNullValue = 42;
            int? nullValue = null;
            var value = {{expression}};
            Tag("Value", value);
            if (value)
            {
                Tag("If_Unreachable");
            }
            else
            {
                Tag("Else");
            }
            Tag("End");
            """;
        var validator = SETestContext.CreateCS(code, "int? arg").Validator;
        validator.ValidateContainsOperation(OperationKind.Binary);
        validator.ValidateTagOrder("Value", "Else", "End");
    }

    [DataTestMethod]
    [DataRow("arg >= 42", 42, null)]
    [DataRow("arg > 42", 43, null)]
    [DataRow("arg < 42", null, 41)]
    [DataRow("arg <= 42",  null, 42)]
    [DataRow("42 >= arg", null, 42)]
    [DataRow("42 > arg", null, 41)]
    [DataRow("42 < arg", 43, null)]
    [DataRow("42 <= arg", 42, null)]
    [DataRow("arg > (int?)43", 44, null)]
    [DataRow("arg > new Nullable<int>(42)", 43, null)]
    [DataRow("arg > (42 as int?)", 43, null)]
    [DataRow("arg > notNullValue", 43, null)]
    public void Binary_NullableRelationalNonNull_SetsObjectConstraint_CS(string expression, int? min, int? max)
    {
        var code = $$"""
            int? notNullValue = 42;
            if ({{expression}})
            {
                Tag("If", arg);
            }
            else
            {
                Tag("Else", arg);
            }
            """;
        var validator = SETestContext.CreateCS(code, "int? arg").Validator;
        validator.ValidateContainsOperation(OperationKind.Binary);
        validator.TagValue("If").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(min, max));
        validator.TagValue("Else").Should().HaveNoConstraints("arg either null or comparison false");
    }

    [DataTestMethod]
    [DataRow("s = s + s;")]
    [DataRow("s = s + null;")]
    [DataRow("s += s;")]
    [DataRow("s += null;")]
    public void Binary_StringConcatenation_Binary_CS(string expression)
    {
        var code = $"""
            string s = null;
            {expression}
            Tag("S", s);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKind.Binary);
        validator.TagValue("S").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("s = s + s")]
    [DataRow("s = s + Nothing")]
    [DataRow("s = s & s")]
    [DataRow("s = s & Nothing")]
    [DataRow("s += s")]
    [DataRow("s += Nothing")]
    [DataRow("s &= s")]
    [DataRow("s &= Nothing")]
    public void Binary_StringConcatenation_Binary_VB(string expression)
    {
        var code = $"""
            Dim S As String = Nothing
            {expression}
            Tag("S", s)
            """;
        var validator = SETestContext.CreateVB(code).Validator;
        validator.ValidateContainsOperation(OperationKind.Binary);
        validator.TagValue("S").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("(byte)42", "== 42")]
    [DataRow("(short)42", "== 42")]
    [DataRow("(long)42", "== 42")]
    [DataRow("42", "== 42")]
    [DataRow("42", "!= 0")]
    [DataRow("42", "> 41")]
    [DataRow("42", ">= 41")]
    [DataRow("42", ">= 42")]
    [DataRow("42", "< 43")]
    [DataRow("42", "<= 42")]
    [DataRow("42", "<= 43")]
    public void Binary_NumberLiteral_SetsBoolConstraint_IsTrue(string value, string expressionSuffix)
    {
        var code = $"""
            var value = {value};
            var result = value {expressionSuffix};
            Tag("Result", result);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True);
    }

    [DataTestMethod]
    [DataRow("42", "== 0")]
    [DataRow("42", "!= 42")]
    [DataRow("42", "> 42")]
    [DataRow("42", "> 43")]
    [DataRow("42", ">= 43")]
    [DataRow("42", "< 41")]
    [DataRow("42", "< 42")]
    [DataRow("42", "<= 41")]
    public void Binary_NumberLiteral_SetsBoolConstraint_IsFalse(string value, string expressionSuffix)
    {
        var code = $"""
            var value = {value};
            var result = value {expressionSuffix};
            Tag("Result", result);
            """;
        var validator = SETestContext.CreateCS(code, "int arg").Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False);
    }

    [DataTestMethod]
    [DataRow("arg == 42")]
    [DataRow("arg != 42")]
    [DataRow("arg > 42")]
    [DataRow("arg >= 42")]
    [DataRow("arg < 42")]
    [DataRow("arg <= 42")]
    public void Binary_NumberLiteral_Unknown(string expression)
    {
        var code = $"""
            var result = {expression};
            Tag("Result", result);
            """;
        var validator = SETestContext.CreateCS(code, "int arg").Validator;
        validator.TagValues("Result").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False));
    }

    [DataTestMethod]
    [DataRow("i + j", 47)]
    [DataRow("i + 1", 43)]
    [DataRow("i + j + 1", 48)]
    [DataRow("(i + 1) + j", 48)]
    [DataRow("(i + j) + 1", 48)]
    [DataRow("i + (1 + j)", 48)]
    [DataRow("1 + (i + j)", 48)]
    [DataRow("i - j", 37)]
    [DataRow("j - i", -37)]
    [DataRow("i - 1", 41)]
    [DataRow("i - j - 1", 36)]
    [DataRow("(i - 1) - j", 36)]
    [DataRow("(i - j) - 1", 36)]
    [DataRow("i - (j - 1)", 38)]
    [DataRow("i - (1 - j)", 46)]
    [DataRow("1 - (i - j)", -36)]
    [DataRow("i + j - 1", 46)]
    [DataRow("(i + j) - 1", 46)]
    [DataRow("(i + 1) - j", 38)]
    [DataRow("i + (j - 1)", 46)]
    [DataRow("i + (1 - j)", 38)]
    [DataRow("1 + (i - j)", 38)]
    [DataRow("i - j + 1", 38)]
    [DataRow("(i - j) + 1", 38)]
    [DataRow("(i - 1) + j", 46)]
    [DataRow("i - (j + 1)", 36)]
    [DataRow("1 - (i + j)", -46)]
    public void Binary_PlusAndMinus(string expression, int expected)
    {
        var code = $"""
            var i = 42;
            var j = 5;
            var value = {expression};
            Tag("Value", value);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
    }

    [DataTestMethod]
    [DataRow(5, 0, 0)]
    [DataRow(5, 1, 5)]
    [DataRow(5, -1, -5)]
    [DataRow(5, 4, 20)]
    [DataRow(5, -4, -20)]
    [DataRow(-5, 0, 0)]
    [DataRow(-5, 1, -5)]
    [DataRow(-5, -1, 5)]
    [DataRow(-5, 4, -20)]
    [DataRow(-5, -4, 20)]
    public void Binary_Multiplication_SingleValue(int left, int right, int expected)
    {
        var code = $"""
            var left = {left};
            var right = {right};
            var value = left * right;
            Tag("Value", value);
            """;
        SETestContext.CreateCS(code).Validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
    }

    [DataTestMethod]
    [DataRow("i >=  3 && j >=  5", 15, null)]
    [DataRow("i >=  3 && j >= -5", null, null)]
    [DataRow("i >=  3 && j <=  5", null, null)]
    [DataRow("i >=  3 && j <= -5", null, -15)]
    [DataRow("i >= -3 && j >=  5", null, null)]
    [DataRow("i >= -3 && j >= -5", null, null)]
    [DataRow("i >= -3 && j <=  5", null, null)]
    [DataRow("i >= -3 && j <= -5", null, null)]
    [DataRow("i <=  3 && j >=  5", null, null)]
    [DataRow("i <=  3 && j >= -5", null, null)]
    [DataRow("i <=  3 && j <=  5", null, null)]
    [DataRow("i <=  3 && j <= -5", null, null)]
    [DataRow("i <= -3 && j >=  5", null, -15)]
    [DataRow("i <= -3 && j >= -5", null, null)]
    [DataRow("i <= -3 && j <=  5", null, null)]
    [DataRow("i <= -3 && j <= -5", 15, null)]
    [DataRow("i ==  3 && j >=  5", 15, null)]
    [DataRow("i ==  3 && j >= -5", -15, null)]
    [DataRow("i ==  3 && j <=  5", null, 15)]
    [DataRow("i ==  3 && j <= -5", null, -15)]
    [DataRow("i == -3 && j >=  5", null, -15)]
    [DataRow("i == -3 && j >= -5", null, 15)]
    [DataRow("i == -3 && j <=  5", -15, null)]
    [DataRow("i == -3 && j <= -5", 15, null)]
    public void Binary_Multiplication_Range(string expression, int? expectedMin, int? expectedMax)
    {
        var code = $$"""
            if ({{expression}})
            {
                var value = i * j;
                Tag("Value", value);
            }
            """;
        var validator = SETestContext.CreateCS(code, "int i, int j").Validator;
        validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
    }

    [DataTestMethod]
    [DataRow(21, 4, 5)]
    [DataRow(21, -4, -5)]
    [DataRow(-21, 4, -5)]
    [DataRow(-21, -4, 5)]
    [DataRow(4, 5, 0)]
    [DataRow(-4, 5, 0)]
    [DataRow(4, -5, 0)]
    [DataRow(-4, -5, 0)]
    [DataRow(5, 1, 5)]
    [DataRow(5, -1, -5)]
    [DataRow(-5, 1, -5)]
    [DataRow(-5, -1, 5)]
    public void Binary_Division_SingleValue(int left, int right, int expected)
    {
        var code = $"""
            var left = {left};
            var right = {right};
            var value = left / right;
            Tag("Value", value);
            """;
        SETestContext.CreateCS(code).Validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
    }

    [DataTestMethod]
    [DataRow("i >=  21 && j >=  5", 0, null)]
    [DataRow("i >=  21 && j >= -5", null, null)]
    [DataRow("i >=  21 && j ==  5", 4, null)]
    [DataRow("i >=  21 && j == -5", null, -4)]
    [DataRow("i >=  21 && j <=  5", null, null)]
    [DataRow("i >=  21 && j <= -5", null, 0)]
    [DataRow("i >= -21 && j >=  5", -4, null)]
    [DataRow("i >= -21 && j >= -5", null, null)]
    [DataRow("i >= -21 && j ==  5", -4, null)]
    [DataRow("i >= -21 && j == -5", null, 4)]
    [DataRow("i >= -21 && j <=  5", null, null)]
    [DataRow("i >= -21 && j <= -5", null, 4)]
    [DataRow("i ==  21 && j >=  5", 0, 4)]
    [DataRow("i ==  21 && j >= -5", -21, 21)]
    [DataRow("i ==  21 && j <=  5", -21, 21)]
    [DataRow("i ==  21 && j <= -5", -4, 0)]
    [DataRow("i == -21 && j >=  5", -4, 0)]
    [DataRow("i == -21 && j >= -5", -21, 21)]
    [DataRow("i == -21 && j <=  5", -21, 21)]
    [DataRow("i == -21 && j <= -5", 0, 4)]
    [DataRow("i <=  21 && j >=  5", null, 4)]
    [DataRow("i <=  21 && j >= -5", null, null)]
    [DataRow("i <=  21 && j ==  5", null, 4)]
    [DataRow("i <=  21 && j == -5", -4, null)]
    [DataRow("i <=  21 && j <=  5", null, null)]
    [DataRow("i <=  21 && j <= -5", -4, null)]
    [DataRow("i <= -21 && j >=  5", null, 0)]
    [DataRow("i <= -21 && j >= -5", null, null)]
    [DataRow("i <= -21 && j ==  5", null, -4)]
    [DataRow("i <= -21 && j == -5", 4, null)]
    [DataRow("i <= -21 && j <=  5", null, null)]
    [DataRow("i <= -21 && j <= -5", 0, null)]
    [DataRow("i >= -21 && i <= 15 && j >= -5", -21, 21)]
    [DataRow("i >= -15 && i <= 21 && j >= -5", -21, 21)]
    [DataRow("i >=  21 && j >= 0", 0, null)]
    [DataRow("i >=  21 && j == 0", null, null)]
    [DataRow("i >=  21 && j <= 0", null, 0)]
    [DataRow("i >= -21 && j >= 0", -21, null)]
    [DataRow("i >= -21 && j == 0", null, null)]
    [DataRow("i >= -21 && j <= 0", null, 21)]
    [DataRow("i <=  21 && j >= 0", null, 21)]
    [DataRow("i <=  21 && j == 0", null, null)]
    [DataRow("i <=  21 && j <= 0", -21, null)]
    [DataRow("i <= -21 && j >= 0", null, 0)]
    [DataRow("i <= -21 && j == 0", null, null)]
    [DataRow("i <= -21 && j <= 0", 0, null)]
    public void Binary_Division_Range(string expression, int? expectedMin, int? expectedMax)
    {
        var code = $$"""
            if ({{expression}})
            {
                var value = i / j;
                Tag("Value", value);
            }
            """;
        SETestContext.CreateCS(code, "int i, int j").Validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
    }

    [DataTestMethod]
    [DataRow(21, 4, 1)]
    [DataRow(21, -4, 1)]
    [DataRow(-21, 4, -1)]
    [DataRow(-21, -4, -1)]
    [DataRow(4, 5, 4)]
    [DataRow(-4, 5, -4)]
    [DataRow(4, -5, 4)]
    [DataRow(-4, -5, -4)]
    [DataRow(5, 5, 0)]
    [DataRow(5, -5, 0)]
    [DataRow(-5, 5, 0)]
    [DataRow(-5, -5, 0)]
    [DataRow(5, 1, 0)]
    [DataRow(5, -1, 0)]
    [DataRow(-5, 1, 0)]
    [DataRow(-5, -1, 0)]
    public void Binary_Remainder_SingleValue(int left, int right, int expected)
    {
        var code = $"""
            var left = {left};
            var right = {right};
            var value = left % right;
            Tag("Value", value);
            """;
        SETestContext.CreateCS(code).Validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
    }

    [DataTestMethod]
    [DataRow("i >=  21 && j >=  5", 0, null)]
    [DataRow("i >=  21 && j >= -5", 0, null)]
    [DataRow("i >=  21 && j ==  5", 0, 4)]
    [DataRow("i >=  21 && j == -5", 0, 4)]
    [DataRow("i >=  21 && j <=  5", 0, null)]
    [DataRow("i >=  21 && j <= -5", 0, null)]
    [DataRow("i >= -21 && j >=  5", -21, null)]
    [DataRow("i >= -21 && j >= -5", -21, null)]
    [DataRow("i >= -21 && j ==  5", -4, 4)]
    [DataRow("i >= -21 && j == -5", -4, 4)]
    [DataRow("i >= -21 && j <=  5", -21, null)]
    [DataRow("i >= -21 && j <= -5", -21, null)]
    [DataRow("i ==  21 && j >=  5", 0, 21)]
    [DataRow("i ==  21 && j >= -5", 0, 21)]
    [DataRow("i ==  21 && j <=  5", 0, 21)]
    [DataRow("i ==  21 && j <= -5", 0, 21)]
    [DataRow("i == -21 && j >=  5", -21, 0)]
    [DataRow("i == -21 && j >= -5", -21, 0)]
    [DataRow("i == -21 && j <=  5", -21, 0)]
    [DataRow("i == -21 && j <= -5", -21, 0)]
    [DataRow("i <=  21 && j >=  5", null, 21)]
    [DataRow("i <=  21 && j >= -5", null, 21)]
    [DataRow("i <=  21 && j ==  5", -4, 4)]
    [DataRow("i <=  21 && j == -5", -4, 4)]
    [DataRow("i <=  21 && j <=  5", null, 21)]
    [DataRow("i <=  21 && j <= -5", null, 21)]
    [DataRow("i <= -21 && j >=  5", null, 0)]
    [DataRow("i <= -21 && j >= -5", null, 0)]
    [DataRow("i <= -21 && j ==  5", -4, 0)]
    [DataRow("i <= -21 && j == -5", -4, 0)]
    [DataRow("i <= -21 && j <=  5", null, 0)]
    [DataRow("i <= -21 && j <= -5", null, 0)]
    [DataRow("i >=  21 && j >= 0", 0, null)]
    [DataRow("i >=  21 && j == 0", null, null)]
    [DataRow("i >=  21 && j <= 0", 0, null)]
    [DataRow("i >= -21 && j >= 0", -21, null)]
    [DataRow("i >= -21 && j == 0", null, null)]
    [DataRow("i >= -21 && j <= 0", -21, null)]
    [DataRow("i <=  21 && j >= 0", null, 21)]
    [DataRow("i <=  21 && j == 0", null, null)]
    [DataRow("i <=  21 && j <= 0", null, 21)]
    [DataRow("i <= -21 && j >= 0", null, 0)]
    [DataRow("i <= -21 && j == 0", null, null)]
    [DataRow("i <= -21 && j <= 0", null, 0)]
    [DataRow("i >=  21 && j == 1", 0, 0)]
    [DataRow("i >= -21 && j == 1", 0, 0)]
    [DataRow("i <=  21 && j == 1", 0, 0)]
    [DataRow("i <= -21 && j == 1", 0, 0)]
    [DataRow("i >=  21 && j >=   5 && j <= 10", 0, 9)]
    [DataRow("i >=  21 && j >=  -5 && j <= 10", 0, 9)]
    [DataRow("i >=  21 && j >= -10 && j <=  5", 0, 9)]
    [DataRow("i >=  21 && j >= -10 && j <= -5", 0, 9)]
    [DataRow("i >= -21 && j >=   5 && j <= 10", -9, 9)]
    [DataRow("i >= -21 && j >=  -5 && j <= 10", -9, 9)]
    [DataRow("i >= -21 && j >= -10 && j <=  5", -9, 9)]
    [DataRow("i >= -21 && j >= -10 && j <= -5", -9, 9)]
    [DataRow("i <=  21 && j >=   5 && j <= 10", -9, 9)]
    [DataRow("i <=  21 && j >=  -5 && j <= 10", -9, 9)]
    [DataRow("i <=  21 && j >= -10 && j <=  5", -9, 9)]
    [DataRow("i <=  21 && j >= -10 && j <= -5", -9, 9)]
    [DataRow("i <= -21 && j >=   5 && j <= 10", -9, 0)]
    [DataRow("i <= -21 && j >=  -5 && j <= 10", -9, 0)]
    [DataRow("i <= -21 && j >= -10 && j <=  5", -9, 0)]
    [DataRow("i <= -21 && j >= -10 && j <= -5", -9, 0)]
    [DataRow("i >=   5 && i <=  10 && j >=  21", 5, 10)]
    [DataRow("i >=   5 && i <=  10 && j <= -21", 5, 10)]
    [DataRow("i >=  -5 && i <=  10 && j >=  21", -5, 10)]
    [DataRow("i >=  -5 && i <=  10 && j <= -21", -5, 10)]
    [DataRow("i >= -10 && i <=   5 && j >=  21", -10, 5)]
    [DataRow("i >= -10 && i <=   5 && j <= -21", -10, 5)]
    [DataRow("i >= -10 && i <=  -5 && j >=  21", -10, -5)]
    [DataRow("i >= -10 && i <=  -5 && j <= -21", -10, -5)]
    [DataRow("i >=   4 && i <=   7 && j >=   5 && j <= 10", 0, 7)]
    [DataRow("i == 5 && j == 0", null, null)]
    public void Binary_Remainder_Range(string expression, int? expectedMin, int? expectedMax)
    {
        var code = $$"""
            if ({{expression}})
            {
                var value = i % j;
                Tag("Value", value);
            }
            """;
        SETestContext.CreateCS(code, "int i, int j").Validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
    }

    [DataTestMethod]
    [DataRow(0b0000, 0b0000, 0b0000)]
    [DataRow(0b0101, 0b0101, 0b0101)]
    [DataRow(0b0101, 0b0001, 0b0001)]
    [DataRow(0b1010, 0b0110, 0b0010)]
    [DataRow(0b1010, 0b0000, 0b0000)]
    [DataRow(0b1111, 0b1111, 0b1111)]
    [DataRow(5, -5, 1)]
    [DataRow(5, -4, 4)]
    [DataRow(-5, -5, -5)]
    [DataRow(-5, -4, -8)]
    public void Binary_BitAnd_SingleValue(int left, int right, int expected)
    {
        var code = $"""
            var left = {left};
            var right = {right};
            var value = left & right;
            Tag("Value", value);
            """;
        SETestContext.CreateCS(code).Validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
    }

    [DataTestMethod]
    [DataRow("i >=  3 && j >=  5", 0, null)]
    [DataRow("i >=  3 && j >= -5", 0, null)]
    [DataRow("i >=  3 && j <=  5", 0, null)]
    [DataRow("i >=  3 && j <= -5", 0, null)]
    [DataRow("i >= -3 && j >=  5", 0, null)]
    [DataRow("i >= -3 && j >= -5", -8, null)]
    [DataRow("i >= -3 && j <=  5", null, null)]
    [DataRow("i >= -3 && j <= -5", null, null)]
    [DataRow("i ==  3 && j >=  5", 0, 3)]
    [DataRow("i ==  3 && j >= -5", 0, 3)]
    [DataRow("i ==  3 && j <=  5", 0, 3)]
    [DataRow("i ==  3 && j <= -5", 0, 3)]
    [DataRow("i == -3 && j >=  5", 0, null)]
    [DataRow("i == -3 && j >= -5", -8, null)]
    [DataRow("i == -3 && j <=  5", null, 5)]
    [DataRow("i == -3 && j <= -5", null, -5)]
    [DataRow("i <=  3 && j >=  5", 0, null)]
    [DataRow("i <=  3 && j >= -5", null, null)]
    [DataRow("i <=  3 && j <=  5", null, 5)]
    [DataRow("i <=  3 && j <= -5", null, 3)]
    [DataRow("i <= -3 && j >=  5", 0, null)]
    [DataRow("i <= -3 && j >= -5", null, null)]
    [DataRow("i <= -3 && j <=  5", null, 5)]
    [DataRow("i <= -3 && j <= -5", null, -5)]
    public void Binary_BitAnd_Range(string expression, int? expectedMin, int? expectedMax)
    {
        var code = $$"""
            if ({{expression}})
            {
                var value = i & j;
                Tag("Value", value);
            }
            """;
        var validator = SETestContext.CreateCS(code, "int i, int j").Validator;
        validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
    }

    [DataTestMethod]
    [DataRow(0b0000, 0b0000, 0b0000)]
    [DataRow(0b0101, 0b0101, 0b0101)]
    [DataRow(0b0101, 0b0001, 0b0101)]
    [DataRow(0b1010, 0b0110, 0b1110)]
    [DataRow(0b1010, 0b0000, 0b1010)]
    [DataRow(0b1111, 0b1111, 0b1111)]
    [DataRow(5, -5, -1)]
    [DataRow(5, -4, -3)]
    [DataRow(-5, -5, -5)]
    [DataRow(-5, -4, -1)]
    public void Binary_BitOr_SingleValue(int left, int right, int expected)
    {
        var code = $"""
            var left = {left};
            var right = {right};
            var value = left | right;
            Tag("Value", value);
            """;
        SETestContext.CreateCS(code).Validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
    }

    [DataTestMethod]
    [DataRow("i >=  4 && j >=  6", 6, null)]
    [DataRow("i >=  4 && j >= -6", -6, null)]
    [DataRow("i >=  4 && j <=  6", null, null)]
    [DataRow("i >=  4 && j <= -6", null, -1)]
    [DataRow("i >= -4 && j >=  6", -4, null)]
    [DataRow("i >= -4 && j >= -6", -6, null)]
    [DataRow("i >= -4 && j <=  6", null, null)]
    [DataRow("i >= -4 && j <= -6", null, -1)]
    [DataRow("i ==  4 && j >=  6", 6, null)]
    [DataRow("i ==  4 && j >= -6", -6, null)] // (i | j) >= -4, we only get a lower bound of min
    [DataRow("i ==  4 && j <=  6", null, 7)]
    [DataRow("i ==  4 && j <= -6", null, -1)] // (i | j) <= -2, we only get an upper bound of max
    [DataRow("i == -4 && j >=  6", -4, -1)]
    [DataRow("i == -4 && j >= -6", -4, -1)]
    [DataRow("i == -4 && j <=  6", -4, -1)]
    [DataRow("i == -4 && j <= -6", -4, -1)]
    [DataRow("i <=  4 && j >=  6", null, null)]
    [DataRow("i <=  4 && j >= -6", null, null)]
    [DataRow("i <=  4 && j <=  6", null, 7)]
    [DataRow("i <=  4 && j <= -6", null, -1)]
    [DataRow("i <= -4 && j >=  6", null, -1)]
    [DataRow("i <= -4 && j >= -6", null, -1)]
    [DataRow("i <= -4 && j <=  6", null, -1)]
    [DataRow("i <= -4 && j <= -6", null, -1)]
    public void Binary_BitOr_Range(string expression, int? expectedMin, int? expectedMax)
    {
        var code = $$"""
            if ({{expression}})
            {
                var value = i | j;
                Tag("Value", value);
            }
            """;

        if (expectedMin is not null || expectedMax is not null)
        {
            SETestContext.CreateCS(code, "int i, int j").Validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
        }
        else
        {
            SETestContext.CreateCS(code, "int i, int j").Validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
        }
    }

    [DataTestMethod]
    [DataRow(0b0000, 0b0000, 0b0000)]
    [DataRow(0b0101, 0b0101, 0b0000)]
    [DataRow(0b0101, 0b0001, 0b0100)]
    [DataRow(0b1010, 0b0110, 0b1100)]
    [DataRow(0b1010, 0b0000, 0b1010)]
    [DataRow(0b1111, 0b1111, 0b0000)]
    [DataRow(5, -5, -2)]
    [DataRow(5, -4, -7)]
    [DataRow(-5, -5, 0)]
    [DataRow(-5, -4, 7)]
    public void Binary_BitXor_SingleValue(int left, int right, int expected)
    {
        var code = $"""
            var left = {left};
            var right = {right};
            var value = left ^ right;
            Tag("Value", value);
            """;
        SETestContext.CreateCS(code).Validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
    }

    [DataTestMethod]
    [DataRow("i >=  4 && j >=  6", 0, null)]
    [DataRow("i >=  4 && j >= -6", null, null)]
    [DataRow("i >=  4 && j <=  6", null, null)]
    [DataRow("i >=  4 && j <= -6", null, -1)]
    [DataRow("i >= -4 && j >=  6", null, null)]
    [DataRow("i >= -4 && j >= -6", null, null)]
    [DataRow("i >= -4 && j <=  6", null, null)]
    [DataRow("i >= -4 && j <= -6", null, null)]  // exact range: null, -1
    [DataRow("i ==  4 && j >=  6", 2, null)]
    [DataRow("i ==  4 && j >= -6", -8, null)]
    [DataRow("i ==  4 && j <=  6", null, 7)]
    [DataRow("i ==  4 && j <= -6", null, -1)]    // exact range: null, -2
    [DataRow("i == -4 && j >=  6", null, -1)]    // exact range: null, -5
    [DataRow("i == -4 && j >= -6", null, 7)]
    [DataRow("i == -4 && j <=  6", -8, null)]
    [DataRow("i == -4 && j <= -6", 2, null)]     // exact range: 4, null
    [DataRow("i <=  4 && j >=  6", null, null)]
    [DataRow("i <=  4 && j >= -6", null, null)]
    [DataRow("i <=  4 && j <=  6", null, null)]
    [DataRow("i <=  4 && j <= -6", null, null)]
    [DataRow("i <= -4 && j >=  6", null, -1)]
    [DataRow("i <= -4 && j >= -6", null, null)]
    [DataRow("i <= -4 && j <=  6", null, null)]
    [DataRow("i <= -4 && j <= -6", 0, null)]
    [DataRow("i >=  4 && j >=  6 && j <=  8", 0, null)]
    [DataRow("i >=  4 && j >=  1 && j <=  3", 1, null)]           // exact range: 4, null
    [DataRow("i >=  4 && i <=  6 && j >=  6", 0, null)]
    [DataRow("i >=  4 && i <=  6 && j >=  6 && j <= 8", 0, 15)]   // exact range: 0, 14
    [DataRow("i >=  4 && i <=  5 && j >=  6 && j <= 8", 1, 15)]   // exact range: 2, 13
    [DataRow("i >= -3 && i <= -1 && j >= -4 && j <=-2", 0, 7)]    // exact range: 0, 3
    [DataRow("i >= -4 && i <= -3 && j >= -2 && j <=-1", 1, 7)]    // exact range: 2, 3
    [DataRow("i >= -4 && i <=  6 && j >= -3 && j <= 8", -16, 15)] // exact range: -12, 14
    public void Binary_BitXor_Range(string expression, int? expectedMin, int? expectedMax)
    {
        var code = $$"""
            if ({{expression}})
            {
                var value = i ^ j;
                Tag("Value", value);
            }
            """;

        if (expectedMin is not null || expectedMax is not null)
        {
            SETestContext.CreateCS(code, "int i, int j").Validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
        }
        else
        {
            SETestContext.CreateCS(code, "int i, int j").Validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
        }
    }
}

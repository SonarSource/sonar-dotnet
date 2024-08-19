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

using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.Test.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

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
        validator.TagValue("ForSymbolSymbolNone").Should().HaveOnlyConstraint(ObjectConstraint.NotNull, "BoolContraint is missing, because we can't tell if two instances are equivalent.");
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
        validator.TagValue("ForSymbolSymbolNone").Should().HaveOnlyConstraint(ObjectConstraint.NotNull, "BoolConstraint is missing, because we can't tell if two instances are equivalent.");
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
    [DataRow("list.Count == 0", true, false)]
    [DataRow("list.Count == 5", false, null)]
    [DataRow("list.Count != 0", false, true)]
    [DataRow("list.Count != 5", null, false)]
    [DataRow("list.Count >  0", false, true)]
    [DataRow("list.Count >  1", false, null)]
    [DataRow("list.Count >  5", false, null)]
    [DataRow("list.Count >= 1", false, true)]
    [DataRow("list.Count >= 5", false, null)]
    [DataRow("list.Count <  1", true, false)]
    [DataRow("list.Count <  5", null, false)]
    [DataRow("list.Count <= 0", true, false)]
    [DataRow("list.Count <= 1", null, false)]
    [DataRow("list.Count <= 5", null, false)]
    [DataRow("0 == list.Count", true, false)]
    [DataRow("5 == list.Count", false, null)]
    [DataRow("0 != list.Count", false, true)]
    [DataRow("5 != list.Count", null, false)]
    [DataRow("1 >  list.Count", true, false)]
    [DataRow("5 >  list.Count", null, false)]
    [DataRow("0 >= list.Count", true, false)]
    [DataRow("1 >= list.Count", null, false)]
    [DataRow("5 >= list.Count", null, false)]
    [DataRow("0 <  list.Count", false, true)]
    [DataRow("1 <  list.Count", false, null)]
    [DataRow("5 <  list.Count", false, null)]
    [DataRow("1 <= list.Count", false, true)]
    [DataRow("5 <= list.Count", false, null)]
    public void Binary_Collections(string expression, bool? emptyInIf, bool? emptyInElse)
    {
        var code = $$"""
            string tag;
            if ({{expression}})
            {
                tag = "if";
            }
            else
            {
                tag = "else";
            }
            """;

        // Canot use Tag("If", list) because the Tag invocation will clear the CollectionConstraint from "list".
        var validator = SETestContext.CreateCS(code, "List<int> list", new PreserveTestCheck("list")).Validator;
        validator.TagValue("if", "list").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, Constraint(emptyInIf));
        validator.TagValue("else", "list").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, Constraint(emptyInElse));

        static CollectionConstraint Constraint(bool? empty) =>
            empty switch
            {
                true => CollectionConstraint.Empty,
                false => CollectionConstraint.NotEmpty,
                _ => null
            };
    }

    [DataTestMethod]
    [DataRow("list.Count == -5", true)]
    [DataRow("list.Count != -5", false)]
    [DataRow("list.Count <  -5", true)]
    [DataRow("list.Count >  -5", false)]
    [DataRow("list.Count <   0", true)]
    [DataRow("list.Count >=  0", false)]
    [DataRow("-5 == list.Count", true)]
    [DataRow("-5 != list.Count", false)]
    [DataRow("0 >   list.Count", true)]
    [DataRow("0 <=  list.Count", false)]
    public void Binary_Collections_UnreachableConditionalArm(string expression, bool unreachableIf)
    {
        var code = $$"""
            string tag;
            if ({{expression}})
            {
                tag = "if";
            }
            else
            {
                tag = "else";
            }
            """;

        var (reachable, unreachable) = unreachableIf
            ? ("else", "if")
            : ("if", "else");
        var validator = SETestContext.CreateCS(code, "List<int> list", new PreserveTestCheck("list")).Validator;
        validator.TagStates(unreachable).Should().BeEmpty();
        validator.TagValue(reachable, "list").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("3.14")]   // Double
    [DataRow("3.14M")]  // Decimal
    [DataRow("3.14F")]  // Float
    public void Binary_Relation_FloatingPoint_DoesNotLearnNumberConstraint(string value)
    {
        var code = $$"""
            var value = {{value}};
            if (value > 0)
            {
                Tag("If", value);
            }
            else
            {
                Tag("Else", value);
            }
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("If").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Else").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("3.14")]   // Double
    [DataRow("3.14M")]  // Decimal
    [DataRow("3.14F")]  // Float
    public void Binary_Equals_FloatingPoint_DoesNotLearnNumberConstraint(string value)
    {
        var code = $$"""
            var value = {{value}};
            if (value == 0)
            {
                Tag("If", value);
            }
            else
            {
                Tag("Else", value);
            }
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("If").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Else").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }
}

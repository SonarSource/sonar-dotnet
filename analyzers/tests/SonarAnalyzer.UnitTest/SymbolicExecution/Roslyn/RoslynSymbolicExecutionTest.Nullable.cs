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

using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn;

public partial class RoslynSymbolicExecutionTest
{
    [TestMethod]
    public void Nullable_Assignment_PropagatesConstrainsToValue()
    {
        const string code = """
            bool? value = null;
            Tag("Null", value);
            value = true;
            Tag("True", value);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Null").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagValue("True").Should().HaveOnlyConstraint(BoolConstraint.True);
    }

    [TestMethod]
    public void Nullable_Value_ReadsConstraintsFromInstance()
    {
        const string code = """
            var value = arg.Value;
            Tag("Unknown", arg);
            arg = true;
            value = arg.Value;
            Tag("True", value);
            arg = false;        // This will set additional constraint TestConstraint.First
            value = arg.Value;
            Tag("FalseFirst", value);
            """;
        var setter = new PreProcessTestCheck(OperationKind.Literal, x => x.Operation.Instance.ConstantValue.Value is false ? x.SetOperationConstraint(TestConstraint.First) : x.State);
        var validator = SETestContext.CreateCS(code, "bool? arg", setter).Validator;
        validator.ValidateTag("Unknown", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue("Accessing .Value would already throw, so it is NotNull by now"));
        validator.TagValue("True").Should().HaveOnlyConstraint(BoolConstraint.True);
        validator.TagValue("FalseFirst").Should().HaveOnlyConstraint(BoolConstraint.False);
        validator.TagValue("FalseFirst").Should().HaveOnlyConstraint(TestConstraint.First);
    }

    [TestMethod]
    public void Nullable_HasValue_ReadsBoolConstraintFromObjectConstraint_NullableInt()
    {
        const string code = """
            var hasValue = arg.HasValue;
            Tag("AfterUnknown");
            arg = 42;
            hasValue = arg.HasValue;
            Tag("HasValueAfter42", hasValue);
            Tag("SymbolAfter42", arg);
            arg = null;
            hasValue = arg.HasValue;
            Tag("HasValueAfterNull", hasValue);
            Tag("SymbolAfterNull", arg);
            """;
        var validator = SETestContext.CreateCS(code, "int? arg").Validator;
        var arg = validator.Symbol("arg");
        var hasValue = validator.Symbol("hasValue");
        validator.TagStates("AfterUnknown").Should().SatisfyRespectively(
            x =>
            {
                x[hasValue].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True);
                x[arg].Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
            },
            x =>
            {
                x[hasValue].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False);
                x[arg].Should().HaveOnlyConstraint(ObjectConstraint.Null);
            });
        validator.TagValue("HasValueAfter42").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True);
        validator.TagValue("SymbolAfter42").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(42));
        validator.TagValue("HasValueAfterNull").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False);
        validator.TagValue("SymbolAfterNull").Should().HaveOnlyConstraint(ObjectConstraint.Null);
    }

    [TestMethod]
    public void Nullable_HasValue_ReadsBoolConstraintFromObjectConstraint_NullableBool()
    {
        const string code = """
            var hasValue = arg.HasValue;
            Tag("AfterUnknown");
            arg = true;
            hasValue = arg.HasValue;
            Tag("HasValueAfterTrue", hasValue);
            Tag("SymbolAfterTrue", arg);
            arg = null;
            hasValue = arg.HasValue;
            Tag("HasValueAfterNull", hasValue);
            Tag("SymbolAfterNull", arg);
            """;
        var validator = SETestContext.CreateCS(code, "bool? arg").Validator;
        var arg = validator.Symbol("arg");
        var hasValue = validator.Symbol("hasValue");
        validator.TagStates("AfterUnknown").Should().SatisfyRespectively(
            x =>
            {
                x[hasValue].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True);
                x[arg].Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
            },
            x =>
            {
                x[hasValue].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False);
                x[arg].Should().HaveOnlyConstraint(ObjectConstraint.Null);
            });
        validator.TagValue("HasValueAfterTrue").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True);
        validator.TagValue("SymbolAfterTrue").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True);
        validator.TagValue("HasValueAfterNull").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False);
        validator.TagValue("SymbolAfterNull").Should().HaveOnlyConstraint(ObjectConstraint.Null);
    }

    [TestMethod]
    public void Nullable_Ctor_NoArguments_SetsNullConstraint()
    {
        const string code = """
            public void Main<T>() where T: struct
            {
                int? explicitType = new Nullable<int>();
                int? targetTyped = new();
                T? genericValue = new T();
                T? genericNull = new T?();
                Tag("ExplicitType", explicitType);
                Tag("TargetTyped", targetTyped);
                Tag("GenericValue", genericValue);
                Tag("GenericNull", genericNull);
            }
            """;
        var validator = SETestContext.CreateCSMethod(code).Validator;
        validator.TagValue("ExplicitType").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.ValidateTag("TargetTyped", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue("new() of int produces value 0"));
        validator.ValidateTag("GenericValue", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue("new() of T produces value T"));
        validator.TagValue("GenericNull").Should().HaveOnlyConstraint(ObjectConstraint.Null);
    }

    [TestMethod]
    public void Nullable_Ctor_Argument_PropagateConstraints()
    {
        const string code = """
            var falseValue = false;     // This will set additional constraint TestConstraint.First
            bool? isTrue = new Nullable<bool>(true);
            bool? isFalse = new Nullable<bool>(falseValue);
            int? isInt = new Nullable<int>(42);
            Tag("IsTrue", isTrue);
            Tag("IsFalse", isFalse);
            Tag("IsInt", isInt);
            """;
        var setter = new PreProcessTestCheck(OperationKind.Literal, x => x.Operation.Instance.ConstantValue.Value is false ? x.SetOperationConstraint(TestConstraint.First) : x.State);
        var validator = SETestContext.CreateCS(code, setter).Validator;
        validator.TagValue("IsTrue").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True);
        validator.TagValue("IsFalse").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False, TestConstraint.First);
        validator.TagValue("IsInt").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(42));
    }

    [TestMethod]
    public void Nullable_Conversion_PropagateConstraints()
    {
        const string code = """
            var isTrue = true;
            bool? toNullableImplicit = isTrue;
            bool? toNullableExplicit = (bool?)isTrue;
            bool? toNullableAs = isTrue as bool?;
            bool toBoolExplicit = (bool)toNullableImplicit;
            Tag("ToNullableImplicit", toNullableImplicit);
            Tag("ToNullableExplicit", toNullableExplicit);
            Tag("ToNullableAs", toNullableAs);
            Tag("ToBoolExplicit", toBoolExplicit);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("ToNullableImplicit").Should().HaveOnlyConstraint(BoolConstraint.True);
        validator.TagValue("ToNullableExplicit").Should().HaveOnlyConstraint(BoolConstraint.True);
        validator.TagValue("ToNullableAs").Should().HaveOnlyConstraint(BoolConstraint.True);
        validator.TagValue("ToBoolExplicit").Should().HaveOnlyConstraint(BoolConstraint.True);
    }

    [TestMethod]
    public void Nullable_GetValueOrDefault_Int()
    {
        const string code = """
            var value = arg.GetValueOrDefault();
            Tag("UnknownArg", arg);
            Tag("UnknownValue", value);

            arg = null;     // Adds DummyConstraint
            value = arg.GetValueOrDefault();
            Tag("NullArg", arg);
            Tag("NullValue", value);

            arg = 42;       // Adds DummyConstraint
            value = arg.GetValueOrDefault();
            Tag("NotNullArg", arg);
            Tag("NotNullValue", value);
            """;
        var validator = SETestContext.CreateCS(code, "int? arg", new LiteralDummyTestCheck()).Validator;
        validator.TagValue("UnknownArg").Should().HaveNoConstraints();
        validator.TagValue("UnknownValue").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("NullArg").Should().HaveOnlyConstraints(ObjectConstraint.Null, DummyConstraint.Dummy);
        validator.TagValue("NullValue").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(0));
        validator.TagValue("NotNullArg").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, DummyConstraint.Dummy, NumberConstraint.From(42));
        validator.TagValue("NotNullValue").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, DummyConstraint.Dummy, NumberConstraint.From(42));
    }

    [TestMethod]
    public void Nullable_GetValueOrDefault_Bool()
    {
        const string code = """
            var value = arg.GetValueOrDefault();
            Tag("UnknownArg", arg);
            Tag("UnknownValue", value);

            arg = null;       // Adds DummyConstraint
            value = arg.GetValueOrDefault();
            Tag("NullArg", arg);
            Tag("NullValue", value);

            arg = true;       // Adds DummyConstraint
            value = arg.GetValueOrDefault();
            Tag("NotNullArg", arg);
            Tag("NotNullValue", value);
            """;
        var validator = SETestContext.CreateCS(code, "bool? arg", new LiteralDummyTestCheck()).Validator;
        validator.TagValue("UnknownArg").Should().HaveNoConstraints();
        validator.TagValue("UnknownValue").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
        validator.TagValue("NullArg").Should().HaveOnlyConstraints(ObjectConstraint.Null, DummyConstraint.Dummy);
        validator.TagValue("NullValue").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False);
        validator.TagValue("NotNullArg").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True, DummyConstraint.Dummy);
        validator.TagValue("NotNullValue").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True, DummyConstraint.Dummy);
    }

    [TestMethod]
    public void Nullable_GetValueOrDefault_SubExpression()
    {
        const string code = """
            var value = (Condition ? null : (bool?)true).GetValueOrDefault();
            Tag("Value", value);
            """;
        SETestContext.CreateCS(code).Validator.TagValues("Value").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True));
    }

    [TestMethod]
    public void Nullable_GetValueOrDefault_SubExpression_Branching()
    {
        const string code = """
            bool? nullable;
            if (boolParameter)
                nullable = true;
            else
                nullable = null;
            var value = nullable.GetValueOrDefault();
            Tag("End");
            """;
        var validator = SETestContext.CreateCS(code, new PreserveTestCheck("boolParameter", "nullable", "value")).Validator;
        var boolParameter = validator.Symbol("boolParameter");
        var nullable = validator.Symbol("nullable");
        var value = validator.Symbol("value");
        validator.TagStates("End").Should().SatisfyRespectively(
            x =>
            {
                x[boolParameter].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True);
                x[nullable].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True);
                x[value].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True);
            }, x =>
            {
                x[boolParameter].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False);
                x[nullable].Should().HaveOnlyConstraint(ObjectConstraint.Null);
                x[value].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False);
            });
    }
}

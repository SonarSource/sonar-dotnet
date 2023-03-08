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
        validator.ValidateTag("Null", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
        validator.ValidateTag("True", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
    }

    [TestMethod]
    public void Nullable_Value_ReadsConstraintsFromInstance()
    {
        const string code = """
            var value = arg.Value;
            Tag("Unknown", value);
            arg = true;
            value = arg.Value;
            Tag("True", value);
            arg = false;        // This will set additional constraint TestConstraint.First
            value = arg.Value;
            Tag("FalseFirst", value);
            """;
        var setter = new PreProcessTestCheck(OperationKind.Literal, x => x.Operation.Instance.ConstantValue.Value is false ? x.SetOperationConstraint(TestConstraint.First) : x.State);
        var validator = SETestContext.CreateCS(code, ", bool? arg", setter).Validator;
        validator.ValidateTag("Unknown", x => x.Should().BeNull());
        validator.ValidateTag("True", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
        validator.ValidateTag("FalseFirst", x => x.HasConstraint(BoolConstraint.False).Should().BeTrue());
        validator.ValidateTag("FalseFirst", x => x.HasConstraint(TestConstraint.First).Should().BeTrue());
    }

    [TestMethod]
    public void Nullable_HasValue_ReadsBoolConstraintFromObjectConstraint()
    {
        const string code = """
            var hasValue = arg.HasValue;
            Tag("HasValueUnknown", hasValue);
            Tag("SymbolUnknown", arg);
            arg = true;
            hasValue = arg.HasValue;
            Tag("HasValueAfterTrue", hasValue);
            Tag("SymbolAfterTrue", arg);
            arg = null;
            hasValue = arg.HasValue;
            Tag("HasValueAfterNull", hasValue);
            Tag("SymbolAfterNull", arg);
            """;
        var validator = SETestContext.CreateCS(code, ", bool? arg").Validator;
        validator.ValidateTag("HasValueUnknown", x => x.Should().BeNull());
        validator.ValidateTag("SymbolUnknown", x => x.Should().BeNull());
        validator.ValidateTag("HasValueAfterTrue", x => x.Should().BeNull());   // ToDo: Should be x.HasConstraint(BoolConstraint.True).Should().BeTrue()); after we build NotNull for bool literal.
        validator.ValidateTag("SymbolAfterTrue", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
        validator.ValidateTag("SymbolAfterTrue", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeFalse());    // ToDo: Should be BeTrue() after we build NotNull for bool literal
        validator.ValidateTag("HasValueAfterNull", x => x.HasConstraint(BoolConstraint.False).Should().BeTrue());
        validator.ValidateTag("SymbolAfterNull", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
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
        validator.ValidateTag("ExplicitType", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
        validator.ValidateTag("TargetTyped", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue("new() of int produces value 0"));
        validator.ValidateTag("GenericValue", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue("new() of T produces value T"));
        validator.ValidateTag("GenericNull", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
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
        validator.ValidateTag("IsTrue", x => x.AllConstraints.Select(x => x.Kind).Should().BeEquivalentTo(new[] { ConstraintKind.ObjectNotNull, ConstraintKind.BoolTrue }));
        validator.ValidateTag("IsFalse", x => x.AllConstraints.Select(x => x.Kind).Should().BeEquivalentTo(new[] { ConstraintKind.ObjectNotNull, ConstraintKind.BoolFalse, (ConstraintKind)ConstraintKindTest.First }));
        validator.ValidateTag("IsInt", x => x.AllConstraints.Select(x => x.Kind).Should().BeEquivalentTo(new[] { ConstraintKind.ObjectNotNull }));
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
        validator.ValidateTag("ToNullableImplicit", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
        validator.ValidateTag("ToNullableExplicit", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
        validator.ValidateTag("ToNullableAs", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
        validator.ValidateTag("ToBoolExplicit", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
    }
}

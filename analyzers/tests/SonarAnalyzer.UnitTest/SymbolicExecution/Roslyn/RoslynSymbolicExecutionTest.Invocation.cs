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

using SonarAnalyzer.Common;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    public partial class RoslynSymbolicExecutionTest
    {
        [TestMethod]
        public void InstanceReference_SetsNotNull_VB()
        {
            const string code = @"
Dim FromMe As Sample = Me
Tag(""Me"", FromMe)";
            var validator = SETestContext.CreateVB(code).Validator;
            validator.ValidateContainsOperation(OperationKind.InstanceReference);
            validator.ValidateTag("Me", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
        }

        [TestMethod]
        public void Invocation_SetsNotNullOnInstance_CS()
        {
            const string code = @"
public class Sample
{
    public void Main(Sample instanceArg, Sample extensionArg)
    {
        var preserve = true;
        Sample extensionNull = null;
        Tag(""BeforeInstance"", instanceArg);
        Tag(""BeforeExtensionArg"", extensionArg);
        Tag(""BeforeExtensionNull"", extensionNull);
        Tag(""BeforePreserve"", preserve);

        instanceArg.InstanceMethod();
        extensionArg.ExtensionMethod();
        UntrackedSymbol().InstanceMethod(); // Is not invoked on any symbol, should not fail
        preserve.ExtensionMethod();
        preserve.ToString();

        Tag(""AfterInstance"", instanceArg);
        Tag(""AfterExtensionArg"", extensionArg);
        Tag(""AfterExtensionNull"", extensionNull);
        Tag(""AfterPreserve"", preserve);
    }

    private void InstanceMethod() { }
    private static void Tag(string name, object arg) { }
    private Sample UntrackedSymbol() => this;
}

public static class Extensions
{
    public static void ExtensionMethod(this Sample s) { }
    public static void ExtensionMethod(this bool b) { }
}";
            var validator = new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator;
            validator.ValidateContainsOperation(OperationKind.Invocation);
            validator.ValidateTag("BeforeInstance", x => x.Should().BeNull());
            validator.ValidateTag("BeforeExtensionArg", x => x.Should().BeNull());
            validator.ValidateTag("BeforeExtensionNull", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.ValidateTag("BeforePreserve", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
            validator.ValidateTag("AfterInstance", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue("Instance method should set NotNull constraint."));
            validator.ValidateTag("AfterExtensionArg", x => x.Should().BeNull("Extensions can run on null instances."));
            validator.ValidateTag("AfterExtensionNull", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue("Extensions can run on null instances."));
            validator.ValidateTag("AfterPreserve", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue("Other constraints should not be removed."));
        }

        [TestMethod]
        public void Invocation_SetsNotNullOnInstance_VB()
        {
            const string code = @"
Public Class Sample

    Public Sub Main(InstanceArg As Sample, StaticArg As Sample, ExtensionArg As Sample)
        Tag(""BeforeInstance"", InstanceArg)
        Tag(""BeforeStatic"", StaticArg)
        Tag(""BeforeExtension"", ExtensionArg)

        InstanceArg.InstanceMethod()
        StaticArg.StaticMethod()
        ExtensionArg.ExtensionMethod()

        Tag(""AfterInstance"", InstanceArg)
        Tag(""AfterStatic"", StaticArg)
        Tag(""AfterExtension"", ExtensionArg)
    End Sub

    Private Sub InstanceMethod()
    End Sub

    Private Shared Sub StaticMethod()
    End Sub

    Private Shared Sub Tag(Name As String, Arg As Object)
    End Sub

End Class

Public Module Extensions

    <Runtime.CompilerServices.Extension>
    Public Sub ExtensionMethod(S As Sample)
    End Sub

End Module";
            var validator = new SETestContext(code, AnalyzerLanguage.VisualBasic, Array.Empty<SymbolicCheck>()).Validator;
            validator.ValidateContainsOperation(OperationKind.ObjectCreation);
            validator.ValidateTag("BeforeInstance", x => x.Should().BeNull());
            validator.ValidateTag("BeforeStatic", x => x.Should().BeNull());
            validator.ValidateTag("BeforeExtension", x => x.Should().BeNull());
            validator.ValidateTag("AfterInstance", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue("Instance method should set NotNull constraint."));
            validator.ValidateTag("AfterStatic", x => x.Should().BeNull("Static method can execute from null instances."));
            validator.ValidateTag("AfterExtension", x => x.Should().BeNull("Extensions can run on null instances."));
        }

        [DataTestMethod]
        [DataRow("Initialize();")]
        [DataRow("this.Initialize();")]
        [DataRow("(this).Initialize();")]
        [DataRow("(((this))).Initialize();")]
        [DataRow("((IDisposable)this).Dispose();")]
        [DataRow("((IDisposable)(object)this).Dispose();")]
        [DataRow("this.SomeExtensionOnSample();")]
        [DataRow("Extensions.SomeExtensionOnSample(this);")]
        [DataRow("this.SomeExtensionOnObject();")]
        [DataRow("Extensions.SomeExtensionOnObject(this);")]
        [DataRow("Extensions.SomeExtensionOnObject((IDisposable)this);")]
        [DataRow("Extensions.SomeExtensionOnObject((object)(IDisposable)this);")]
        [DataRow("((object)(IDisposable)this).SomeExtensionOnObject();")]
        public void Invocation_InstanceMethodCallDoesClearFieldOnThis(string invocation)
        {
            var code = $@"
using System;
public class Sample: IDisposable
{{
    object field;
    static object staticField;

    void Main()
    {{
        field = null;
        staticField = null;
        Tag(""BeforeField"", field);
        Tag(""BeforeStaticField"", staticField);
        {invocation}
        Tag(""AfterField"", field);
        Tag(""AfterStaticField"", staticField);
    }}

    private void Initialize() {{ }}
    void IDisposable.Dispose() {{ }}
    private static void Tag(string name, object arg) {{ }}
}}

public static class Extensions
{{
    public static void SomeExtensionOnSample(this Sample sample) {{ }}
    public static void SomeExtensionOnObject(this object obj) {{ }}
}}";
            var validator = new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator;
            validator.ValidateContainsOperation(OperationKind.Invocation);
            validator.ValidateTag("BeforeField", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.ValidateTag("BeforeStaticField", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.ValidateTag("AfterField", x => x.Constraint<ObjectConstraint>().Should().BeNull());
            validator.ValidateTag("AfterStaticField", x => x.Constraint<ObjectConstraint>().Should().BeNull());
        }

        [DataTestMethod]
        [DataRow("this?.InstanceMethod();")]
        [DataRow("StaticMethod();")]
        [DataRow("Sample.StaticMethod();")]
        [DataRow("var dummy = Property;")]
        [DataRow("var dummy = this.Property;")]
        [DataRow("SampleProperty.InstanceMethod();")]
        [DataRow("this.SampleProperty.InstanceMethod();")]
        [DataRow("this.SampleProperty?.InstanceMethod();")]
        public void Invocation_InstanceMethodCallDoesNotClearFieldForOtherAccess(string invocation)
        {
            var code = $@"
ObjectField = null;
StaticObjectField = null;
Tag(""BeforeField"", ObjectField);
Tag(""BeforeStaticField"", StaticObjectField);
{invocation}
Tag(""AfterField"", ObjectField);
Tag(""AfterStaticField"", StaticObjectField);
";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateContainsOperation(OperationKind.Invocation);
            validator.ValidateTag("BeforeField", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.ValidateTag("BeforeStaticField", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.ValidateTag("AfterField", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.ValidateTag("AfterStaticField", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
        }

        [DataTestMethod]
        [DataRow("otherInstance.InstanceMethod();")]
        [DataRow("(otherInstance).InstanceMethod();")]
        [DataRow("(true ? this : otherInstance).InstanceMethod();")]
        public void Instance_InstanceMethodCallDoesNotClearFieldsOnOtherInstances(string invocation)
        {
            var code = $@"
ObjectField = null;
StaticObjectField = null;
var otherInstance = new Sample();
{invocation}
Tag(""Field"", ObjectField);
Tag(""StaticField"", StaticObjectField);
";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateContainsOperation(OperationKind.Invocation);
            validator.ValidateTag("Field", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.ValidateTag("StaticField", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
        }

        [TestMethod]
        public void Instance_InstanceMethodCallClearsFieldInConsistentManner()
        {
            var code = $@"
ObjectField = null;
Tag(""InitNull"", ObjectField);
InstanceMethod();
Tag(""AfterInvocationNull"", ObjectField);
ObjectField = new object();
Tag(""InitNotNull"", ObjectField);
InstanceMethod();
Tag(""AfterInvocationNotNull"", ObjectField);
if (ObjectField == null)
{{
    Tag(""IfBefore"", ObjectField);
    InstanceMethod();
    Tag(""IfAfter"", ObjectField);
}}
else
{{
    Tag(""ElseBefore"", ObjectField);
    InstanceMethod();
    Tag(""ElseAfter"", ObjectField);
}}
Tag(""AfterIfElse"", ObjectField);
";
            var invalidateConstraint = DummyConstraint.Dummy;
            var dontInvalidateConstraint = LockConstraint.Held;
            var check = new PostProcessTestCheck(x => x.Operation.Instance.Kind == OperationKindEx.SimpleAssignment
                && IFieldReferenceOperationWrapper.FromOperation(ISimpleAssignmentOperationWrapper.FromOperation(x.Operation.Instance).Target).Member is var field
                ? x.SetSymbolConstraint(field, invalidateConstraint).SetSymbolConstraint(field, dontInvalidateConstraint)
                : x.State);
            var validator = SETestContext.CreateCS(code, check).Validator;
            validator.ValidateContainsOperation(OperationKind.Invocation);
            validator.ValidateTag("InitNull", x =>
            {
                x.HasConstraint(ObjectConstraint.Null).Should().BeTrue();
                x.HasConstraint(invalidateConstraint).Should().BeTrue();
                x.HasConstraint(dontInvalidateConstraint).Should().BeTrue();
            });
            validator.ValidateTag("AfterInvocationNull", x =>
            {
                x.HasConstraint(ObjectConstraint.Null).Should().BeFalse();
                x.HasConstraint(invalidateConstraint).Should().BeFalse();
                x.HasConstraint(dontInvalidateConstraint).Should().BeTrue();
            });
            validator.ValidateTag("InitNotNull", x =>
            {
                x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
                x.HasConstraint(invalidateConstraint).Should().BeTrue();
                x.HasConstraint(dontInvalidateConstraint).Should().BeTrue();
            });
            validator.ValidateTag("AfterInvocationNotNull", x =>
            {
                x.HasConstraint(ObjectConstraint.Null).Should().BeFalse();
                x.HasConstraint(invalidateConstraint).Should().BeFalse();
                x.HasConstraint(dontInvalidateConstraint).Should().BeTrue();
            });
            validator.ValidateTag("IfBefore", x =>
            {
                x.HasConstraint(ObjectConstraint.Null).Should().BeTrue();
                x.HasConstraint(invalidateConstraint).Should().BeFalse();
                x.HasConstraint(dontInvalidateConstraint).Should().BeTrue();
            });
            validator.ValidateTag("IfAfter", x =>
            {
                x.HasConstraint(ObjectConstraint.Null).Should().BeFalse();
                x.HasConstraint(invalidateConstraint).Should().BeFalse();
                x.HasConstraint(dontInvalidateConstraint).Should().BeTrue();
            });
            validator.ValidateTag("ElseBefore", x =>
            {
                x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
                x.HasConstraint(invalidateConstraint).Should().BeFalse();
                x.HasConstraint(dontInvalidateConstraint).Should().BeTrue();
            });
            validator.ValidateTag("ElseAfter", x =>
            {
                x.HasConstraint(ObjectConstraint.NotNull).Should().BeFalse();
                x.HasConstraint(invalidateConstraint).Should().BeFalse();
                x.HasConstraint(dontInvalidateConstraint).Should().BeTrue();
            });
            validator.TagValues("AfterIfElse").Should().Equal(new[]
            {
                new SymbolicValue().WithConstraint(dontInvalidateConstraint),
            });
        }

        [TestMethod]
        public void Instance_InstanceMethodCallClearsField()
        {
            var code = $@"
if (this.ObjectField == null)
{{
    this.InstanceMethod(StaticObjectField == null ? 1 : 0);
}}
Tag(""After"", this.ObjectField);
";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.TagValues("After").Should().BeEquivalentTo(
                new SymbolicValue().WithConstraint(ObjectConstraint.Null), // Unexpected. this.InstanceMethod happens after the ternary and should clear any constraints on this.ObjectField
                new SymbolicValue().WithConstraint(ObjectConstraint.Null),
                new SymbolicValue().WithConstraint(ObjectConstraint.NotNull));
        }

        [TestMethod]
        public void Invocation_IsNullOrEmpty_ValidateOrder()
        {
            var validator = SETestContext.CreateCS(@"var isNullOrEmpy = string.IsNullOrEmpty(arg);", ", string arg").Validator;
            validator.ValidateOrder(
"LocalReference: isNullOrEmpy = string.IsNullOrEmpty(arg) (Implicit)",
"ParameterReference: arg",
"Argument: arg",
"Invocation: string.IsNullOrEmpty(arg)", // True/Null
"Invocation: string.IsNullOrEmpty(arg)", // True/NotNull
"Invocation: string.IsNullOrEmpty(arg)", // False/NotNull
"SimpleAssignment: isNullOrEmpy = string.IsNullOrEmpty(arg) (Implicit)",  // True/Null
"SimpleAssignment: isNullOrEmpy = string.IsNullOrEmpty(arg) (Implicit)",  // True/NotNull
"SimpleAssignment: isNullOrEmpy = string.IsNullOrEmpty(arg) (Implicit)"); // False/NotNull
        }

        [TestMethod]
        public void Invocation_IsNullOrEmpty_Tags()
        {
            const string code = @"
var isNullOrEmpy = string.IsNullOrEmpty(arg);
Tag(""IsNullOrEmpy"", isNullOrEmpy);
Tag(""Arg"", arg);";
            var validator = SETestContext.CreateCS(code, ", string arg").Validator;
            validator.TagValues("IsNullOrEmpy").Should().Equal(
                new SymbolicValue().WithConstraint(BoolConstraint.True),       // True/Null
                new SymbolicValue().WithConstraint(BoolConstraint.True),       // True/NotNull
                new SymbolicValue().WithConstraint(BoolConstraint.False));     // False/NotNull
            validator.TagValues("Arg").Should().Equal(
                new SymbolicValue().WithConstraint(ObjectConstraint.Null),     // True/Null
                new SymbolicValue().WithConstraint(ObjectConstraint.NotNull),  // True/NotNull
                new SymbolicValue().WithConstraint(ObjectConstraint.NotNull)); // False/NotNull
        }

        [TestMethod]
        public void Invocation_IsNullOrEmpty_NestedProperty()
        {
            const string code = @"
if (!string.IsNullOrEmpty(exception?.Message))
{
    Tag(""ExceptionChecked"", exception);
}
Tag(""ExceptionAfterCheck"", exception);";
            var validator = SETestContext.CreateCS(code, ", InvalidOperationException exception").Validator;
            validator.ValidateTag("ExceptionChecked", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.TagValues("ExceptionAfterCheck").Should().Equal(new[]
            {
                new SymbolicValue().WithConstraint(ObjectConstraint.Null),
                new SymbolicValue().WithConstraint(ObjectConstraint.NotNull)
            });
        }

        [TestMethod]
        public void Invocation_IsNullOrEmpty_TryFinally()
        {
            const string code = @"
try
{
    if (string.IsNullOrEmpty(arg)) return;
}
finally
{
    Tag(""ArgInFinally"", arg);
}";
            var validator = SETestContext.CreateCS(code, ", string arg").Validator;
            validator.TagValues("ArgInFinally").Should().Equal(new[]
            {
                null,
                new SymbolicValue().WithConstraint(ObjectConstraint.Null),    // Wrong. IsNullOrEmpty does not throw and "arg" is known to be not null.
                new SymbolicValue().WithConstraint(ObjectConstraint.NotNull)
            });
        }

        [DataTestMethod]
        [DataRow("arg.Append(arg)")]
        [DataRow("arg.AsEnumerable()")]
        [DataRow("arg.AsQueryable()")]
        [DataRow("arg.Cast<string>()")]
        [DataRow("arg.Concat(arg)")]
        [DataRow("arg.DefaultIfEmpty()")]   // Returns collection with a single default item in it in case the the source enumerable is empty
        [DataRow("arg.Distinct()")]
        [DataRow("Enumerable.Empty<string>()")]
        [DataRow("arg.Except(arg)")]
        [DataRow("arg.GroupBy(x => x);")]
        [DataRow("arg.GroupJoin(arg, x => x, x => x, (x, lst) => x);")]
        [DataRow("arg.Intersect(arg);")]
        [DataRow("arg.Join(arg, x => x, x => x, (x, lst) => x);")]
        [DataRow("arg.OfType<string>();")]
        [DataRow("arg.OrderBy(x => x);")]
        [DataRow("arg.OrderByDescending(x => x);")]
        [DataRow("arg.Prepend(null);")]
        [DataRow("Enumerable.Range(42, 42);")]
        [DataRow("Enumerable.Repeat(42, 42);")]
        [DataRow("arg.Reverse();")]
        [DataRow("arg.Select(x => x);")]
        [DataRow("arg.SelectMany(x => new[] { x });")]
        [DataRow("arg.Skip(42);")]
        [DataRow("arg.SkipWhile(x => x == null);")]
        [DataRow("arg.Take(42);")]
        [DataRow("arg.TakeWhile(x => x != null);")]
        [DataRow("arg.OrderBy(x => x).ThenBy(x => x);")]
        [DataRow("arg.OrderBy(x => x).ThenByDescending(x => x);")]
        [DataRow("arg.ToArray();")]
        [DataRow("arg.ToDictionary(x => x);")]
        [DataRow("arg.ToList();")]
        [DataRow("arg.ToLookup(x => x);")]
        [DataRow("arg.Union(arg);")]
        [DataRow("arg.Where(x => x != null);")]
        [DataRow("arg.Zip(arg, (x, y) => x);")]
#if NET
        [DataRow("arg.Chunk(42)")]
        [DataRow("arg.DistinctBy(x => x)")]
        [DataRow("arg.ExceptBy(arg, x => x)")]
        [DataRow("arg.IntersectBy(arg, x => x);")]
        [DataRow("arg.SkipLast(42);")]
        [DataRow("arg.UnionBy(arg, x => x);")]
        [DataRow("arg.TakeLast(42);")]
#endif
        public void Invocation_LinqEnumerableAndQueryable_NotNull(string expression)
        {
            var code = $@"
var value = {expression};
Tag(""Value"", value);";
            var enumerableValidator = SETestContext.CreateCS(code, ", IEnumerable<object> arg").Validator;
            enumerableValidator.ValidateTag("Value", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());

            var queryableValidator = SETestContext.CreateCS(code, ", IQueryable<object> arg").Validator;
            queryableValidator.ValidateTag("Value", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
        }

        [DataTestMethod]
        [DataRow("FirstOrDefault();")]
        [DataRow("LastOrDefault();")]
        [DataRow("SingleOrDefault();")]
        public void Invocation_LinqEnumerableAndQueryable_NullOrNotNull(string expression)
        {
            var code = $@"
var value = arg.{expression};
Tag(""Value"", value);";
            var enumerableValidator = SETestContext.CreateCS(code, $", IEnumerable<object> arg").Validator;
            enumerableValidator.TagValues("Value").Should().HaveCount(2)
                .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
                .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));

            var queryableValidator = SETestContext.CreateCS(code, $", IQueryable<object> arg").Validator;
            queryableValidator.TagValues("Value").Should().HaveCount(2)
                .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
                .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
        }

        [DataTestMethod]    // Just a few examples to demonstrate that we don't set it for all
        [DataRow("object", "First()")]
        [DataRow("int", "Min()")]
        [DataRow("int", "Min()")]
        [DataRow("int", "ElementAtOrDefault(42);")]
        [DataRow("int", "FirstOrDefault();")]
        [DataRow("int", "LastOrDefault();")]
        [DataRow("int", "SingleOrDefault();")]
        [DataRow("object", "ElementAtOrDefault(42);")]
        public void Invocation_LinqEnumerable_Unknown(string itemType, string expression)
        {
            var code = $@"
var value = arg.{expression};
Tag(""Value"", value);";
            var validator = SETestContext.CreateCS(code, $", IEnumerable<{itemType}> arg").Validator;
            validator.ValidateTag("Value", x => x.Should().BeNull());
        }

        [TestMethod]
        public void Invocation_Linq_VB()
        {
            const string code = @"
Dim Query = From Item In Items Where Item IsNot Nothing
If Query.Count <> 0 Then
    Dim Value = Query(0)
    Tag(""Value"", Value)
End If";
            var validator = SETestContext.CreateVB(code, ", Items() As Object").Validator;
            validator.ValidateTag("Value", x => x.Should().BeNull());
        }

        [DataTestMethod]
        [DataRow("arg != null")]
        [DataRow("arg is not null")]
        [DataRow("arg is { }")]
        public void Invocation_DebugAssert_LearnsNotNull_Simple(string expression) =>
            DebugAssertValues(expression).Should().HaveCount(1).And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));

        [TestMethod]
        public void Invocation_DebugAssert_LearnsNotNull_AndAlso() =>
            DebugAssertValues("arg != null && condition").Should().HaveCount(1).And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));

        [TestMethod]
        public void Invocation_DebugAssert_LearnsNotNullForAll_AndAlso()
        {
            var code = $@"
Debug.Assert(arg1 != null && arg2 != null);
Tag(""Arg1"", arg1);
Tag(""Arg2"", arg2);";
            var validator = SETestContext.CreateCS(code, $", object arg1, object arg2").Validator;
            validator.ValidateTag("Arg1", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("Arg2", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
        }

        [TestMethod]
        public void Invocation_DebugAssert_LearnsNotNull_OrElse() =>
            DebugAssertValues("arg != null || condition").Should().HaveCount(2)
                .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.Null))
                .And.ContainSingle(x => x != null && x.HasConstraint(ObjectConstraint.NotNull));

        [TestMethod]
        public void Invocation_DebugAssert_LearnsBoolConstraint_Simple() =>
            DebugAssertValues("arg", "bool").Should().HaveCount(1).And.ContainSingle(x => x.HasConstraint(BoolConstraint.True));

        [TestMethod]
        public void Invocation_DebugAssert_LearnsBoolConstraint_Binary() =>
            DebugAssertValues("arg == true", "bool").Should().HaveCount(1).And.ContainSingle(x => x.HasConstraint(BoolConstraint.True));

        [TestMethod]
        public void Invocation_DebugAssert_LearnsBoolConstraint_AlwaysEnds() =>
            DebugAssertValues("false", "bool").Should().BeEmpty();

        [DataTestMethod]
        [DataRow("!arg")]
        [DataRow("!!!arg")]
        public void Invocation_DebugAssert_LearnsBoolConstraint_Negated(string expression) =>
            DebugAssertValues(expression, "bool").Should().HaveCount(1).And.ContainSingle(x => x.HasConstraint(BoolConstraint.False));

        [TestMethod]
        public void Invocation_DebugAssert_CustomNoParameters_DoesNotFail()
        {
            const string code = @"
using System.Diagnostics;

public class Sample
{
    public void Main()
    {
        Debug.Assert();
        Tag(""End"");
    }

    private static void Tag(string name) { }
}

namespace System.Diagnostics
{
    public static class Debug
    {
        public static void Assert() { }
    }
}";
            new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator.ValidateTagOrder("End");
        }

        private static SymbolicValue[] DebugAssertValues(string expression, string argType = "object")
        {
            var code = $@"
Debug.Assert({expression});
Tag(""Arg"", arg);";
            return SETestContext.CreateCS(code, $", {argType} arg, bool condition").Validator.TagValues("Arg");
        }

        [DataTestMethod]
        [DynamicData(nameof(ThrowHelperCalls))]
        public void Invocation_ThrowHelper_StopProcessing(string throwHelperCall)
        {
            var code = $@"
Tag(""Before"");
{throwHelperCall}
Tag(""Unreachable"");
";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTagOrder("Before");
            validator.ValidateExitReachCount(0);
            validator.ValidateExecutionCompleted();
        }

        [DataTestMethod]
        [DynamicData(nameof(ThrowHelperCalls))]
        public void Invocation_ThrowHelper_OnlyInBranch(string throwHelperCall)
        {
            var code = @$"
if (condition)
{{
    Tag(""Before"");
    {throwHelperCall}
    Tag(""Unreachable"");
}}
Tag(""End"");
";
            var validator = SETestContext.CreateCS(code, ", bool condition").Validator;
            validator.ValidateTagOrder("Before", "End");
            validator.ValidateExitReachCount(1);
            validator.ValidateExecutionCompleted();
        }

        [DataTestMethod]
        [DynamicData(nameof(ThrowHelperCalls))]
        public void Invocation_ThrowHelper_TryCatchFinally(string throwHelperCall)
        {
            var code = @$"
try
{{
    {throwHelperCall}
    Tag(""Unreachable"");
}}
catch
{{
    Tag(""Catch"");
}}
finally
{{
    Tag(""Finally"");
}}
Tag(""End"");
";
            var validator = SETestContext.CreateCS(code, ", bool condition").Validator;
            validator.ValidateTagOrder("Catch", "Finally", "Finally", "End");
            validator.ValidateExitReachCount(2);
            validator.ValidateExecutionCompleted();
        }

        [DataTestMethod]
        [DataRow("System.Diagnostics.CodeAnalysis.DoesNotReturn")]
        [DataRow("System.Diagnostics.CodeAnalysis.DoesNotReturnAttribute")]
        [DataRow("OtherNamespace.DoesNotReturn")]
        [DataRow("OtherNamespace.DoesNotReturnAttribute")]
        [DataRow("JetBrains.Annotations.TerminatesProgram")]
        [DataRow("JetBrains.Annotations.TerminatesProgramAttribute")]
        [DataRow("OtherNamespace.TerminatesProgram")]
        [DataRow("OtherNamespace.TerminatesProgramAttribute")]
        public void Invocation_ThrowHelper_Attributes(string throwHelperAttribute)
        {
            var code = $@"
using System;
using System.Diagnostics;

public class Sample
{{
    public void Test()
    {{
        Tag(""Before"");
        ThrowHelper();
        Tag(""Unreachable"");
    }}

    [{throwHelperAttribute}]
    public void ThrowHelper()
    {{
        // No implementation. The attribute should drive the analysis.
    }}

    static void Tag(string name) {{ }}
}}

namespace JetBrains.Annotations
{{
    public sealed class TerminatesProgramAttribute : Attribute {{ }}
}}
namespace OtherNamespace
{{
    public sealed class TerminatesProgramAttribute : Attribute {{ }}
    public sealed class DoesNotReturnAttribute : Attribute {{ }}
}}
";
#if NETFRAMEWORK
            code += @"
namespace System.Diagnostics.CodeAnalysis
{
    public sealed class DoesNotReturnAttribute : Attribute { }
}
";
#endif
            var validator = new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator;
            validator.ValidateTagOrder("Before");
            validator.ValidateExitReachCount(0);
            validator.ValidateExecutionCompleted();
        }

        [DataTestMethod]
        [DataRow("DoesTerminatesProgramAttribute")]
        [DataRow("TerminatesMyProgramAttribute")]
        [DataRow("TerminatesProgramAttributeS")]
        [DataRow("DoesNotReturnEver")]
        [DataRow("ItDoesNotReturn")]
        public void Invocation_ThrowHelper_OtherAttributes_NotSupported(string attributeName)
        {
            var code = $@"
using System;
using System.Diagnostics;

public class Sample
{{
    public void Test()
    {{
        Tag(""Before"");
        ThrowHelper();
        Tag(""After"");
    }}

    [{attributeName}]
    public void ThrowHelper()
    {{
        // No implementation. The attribute should drive the analysis.
    }}

    static void Tag(string name) {{ }}
}}

public sealed class {attributeName}: Attribute {{ }}
";
            var validator = new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator;
            validator.ValidateTagOrder("Before", "After");
            validator.ValidateExitReachCount(1);
            validator.ValidateExecutionCompleted();
        }

        [TestMethod]
        public void Invocation_TargetMethodIsDelegateInvoke()
        {
            var code = @"
Func<Action> f = () => new Action(()=> { });
f()();
";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateContainsOperation(OperationKindEx.Invocation);
            validator.ValidateExitReachCount(1);
            validator.ValidateExecutionCompleted();
        }

        private static IEnumerable<object[]> ThrowHelperCalls =>
            new object[][]
            {
                new[] { @"System.Diagnostics.Debug.Fail(""Fail"");" },
                new[] { @"Environment.FailFast(""Fail"");" },
                new[] { @"Environment.Exit(-1);" },
            };
    }
}

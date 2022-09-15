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
            validator.TagValues("AfterIfElse").Should().Equal(new SymbolicValue[]
            {
                new SymbolicValue().WithConstraint(dontInvalidateConstraint),
            });
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
    }
}

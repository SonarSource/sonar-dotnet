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

using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.Test.TestFramework.SymbolicExecution;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

public partial class RoslynSymbolicExecutionTest
{
    private static IEnumerable<object[]> ThrowHelperCalls =>
        [
            ["""System.Diagnostics.Debug.Fail("Fail");"""],
            ["""Environment.FailFast("Fail");"""],
            ["""Environment.Exit(-1);"""],
        ];

    [TestMethod]
    public void InstanceReference_SetsNotNull_VB()
    {
        const string code = """
            Dim FromMe As Sample = Me
            Tag("Me", FromMe)
            """;
        var validator = SETestContext.CreateVB(code).Validator;
        validator.ValidateContainsOperation(OperationKind.InstanceReference);
        validator.TagValue("Me").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("Aggregate", "(x, y) => x")]
    [DataRow("Append", "1")]
    [DataRow("Average")]
    [DataRow("Concat", "[]")]
    [DataRow("Count")]
    [DataRow("ElementAt", "1")]
    [DataRow("ElementAtOrDefault", "1")]
    [DataRow("Except", "[]")]
    [DataRow("DefaultIfEmpty")]
    [DataRow("Distinct")]
    [DataRow("First")]
    [DataRow("FirstOrDefault")]
    [DataRow("GroupBy", "x => x")]
    [DataRow("GroupJoin", "[1], x => x, x => x, (x, y) => x")]
    [DataRow("Intersect", "[]")]
    [DataRow("Join", "[1], x => x, x => x, (x, y) => x")]
    [DataRow("Last")]
    [DataRow("LastOrDefault")]
    [DataRow("LongCount")]
    [DataRow("Max")]
    [DataRow("Min")]
    [DataRow("OrderBy", "x => x")]
    [DataRow("OrderByDescending", "x => x")]
    [DataRow("Prepend", "1")]
    [DataRow("Reverse")]
    [DataRow("Select", "x => x")]
    [DataRow("SelectMany", "x => new[] {1}")]
    [DataRow("SequenceEqual", "[]")]
    [DataRow("Single")]
    [DataRow("SingleOrDefault")]
    [DataRow("Skip", "1")]
    [DataRow("SkipWhile", "x => true")]
    [DataRow("Sum")]
    [DataRow("Take", "1")]
    [DataRow("TakeWhile", "x => true")]
    [DataRow("ToArray")]
    [DataRow("ToDictionary", "x => x, x => x")]
    [DataRow("ToList")]
    [DataRow("ToLookup", "x => x, x => x")]
    [DataRow("ToHashSet")]
    [DataRow("Union", "[]")]
    [DataRow("Where", "x => true")]
    [DataRow("Zip", "[1], (x, y) => x")]
    public void Invocation_SetsNotNull_OnLinqMethodsNullChecking(string method, string args = null)
    {
        var code = $$"""
            using System;
            using System.Linq;
            using System.Collections.Generic;
            public class Sample
            {
                public void Main(IEnumerable<int> collection)
                {
                    Tag("Before1", collection);
                    _ = collection.{{method}}({{args}});                                                 // Call as extension
                    Tag("After1", collection);

                    collection = Untracked();
                    Tag("Before2", collection);
                    _ = Enumerable.{{method}}(collection{{(args is null ? string.Empty : "," + args)}}); // Call as normal static method
                    Tag("After2", collection);
                }

                private IEnumerable<int> Untracked() => [ 1, 2, 3 ];

                private static void Tag(string name, object arg) { }
            }
            """;
        var validator = new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);
        validator.TagValue("Before1").Should().BeNull();
        validator.TagValue("After1").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Before2").Should().BeNull();
        validator.TagValue("After2").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("Cast<short>()")]
    [DataRow("OfType<short>()")]
    [DataRow("ThenBy(x => x)")]
    [DataRow("ThenByDescending(x => x)")]
    public void Invocation_SetsNotNull_OnLinqMethodsWithGenericParamNullChecking(string invocation)
    {
        var code = $$"""
            using System;
            using System.Linq;
            using System.Collections.Generic;
            public class Sample
            {
                public void Main(IOrderedEnumerable<int> collection)
                {
                    Tag("Before", collection);
                    _ = collection.{{invocation}};
                    Tag("After", collection);
                }

                private static void Tag(string name, object arg) { }
            }
            """;
        var validator = new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);
        validator.TagValue("Before").Should().BeNull();
        validator.TagValue("After").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("AsEnumerable")]
    public void Invocation_DoesNotSetNotNull_OnLinqMethodNonNullChecking(string method, string args = null)
    {
        var code = $$"""
            using System;
            using System.Linq;
            using System.Collections.Generic;
            public class Sample
            {
                public void Main(IEnumerable<int> collection)
                {
                    Tag("Before1", collection);
                    _ = collection.{{method}}({{args}});                                                 // Call as extension
                    Tag("After1", collection);

                    collection = Untracked();
                    Tag("Before2", collection);
                    _ = Enumerable.{{method}}(collection{{(args is null ? string.Empty : "," + args)}}); // Call as normal static method
                    Tag("After2", collection);
                }

                private IEnumerable<int> Untracked() => [ 1, 2, 3 ];

                private static void Tag(string name, object arg) { }
            }
            """;
        var validator = new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);
        validator.TagValue("Before1").Should().BeNull();
        validator.TagValue("After1").Should().BeNull();
        validator.TagValue("Before2").Should().BeNull();
        validator.TagValue("After2").Should().BeNull();
    }

    [TestMethod]
    public void Invocation_DoesntSetNotNull_OnExtensionsNonLinq()
    {
        var code = $$"""
            using System;
            using System.Linq;
            using System.Collections.Generic;
            public class Sample
            {
                public void Main(IEnumerable<int> collection)
                {
                    Tag("Before1", collection);
                    _ = collection.NonLinqExtensionNonNullChecking();
                    Tag("After1", collection);

                    collection = Untracked();
                    Tag("Before2", collection);
                    _ = collection.NonLinqExtensionNullChecking();
                    Tag("After2", collection);
                }

                private IEnumerable<int> Untracked() => [ 1, 2, 3 ];

                private static void Tag(string name, object arg) { }
            }

            public static class MyExtensions
            {
                public static int NonLinqExtensionNonNullChecking(this IEnumerable<int> source) => 42;
                public static int NonLinqExtensionNullChecking(this IEnumerable<int> source)
                {
                    _ = source ?? throw new ArgumentNullException(nameof(source));
                    return 42;
                }
            }
            """;
        var validator = new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);
        validator.TagValue("Before1").Should().BeNull();
        validator.TagValue("After1").Should().BeNull();
        validator.TagValue("Before2").Should().BeNull();
        validator.TagValue("After2").Should().BeNull();
    }

    [DataTestMethod]
    [DataRow("Count")]
    [DataRow("ToArray")]
    [DataRow("ToList")]
    public void Invocation_SetsNotNull_OnSourceOfLinqExtension_VB(string method)
    {
        var code = $$"""
            Imports System
            Imports System.Linq
            Imports System.Collections.Generic

            Public Class Sample
                Public Sub Main(collection As IEnumerable(Of Integer))
                    Tag("Before1", collection)
                    Dim i1 = collection.{{method}}()                     ' Call as extension
                    Tag("After1", collection)

                    collection = Untracked()
                    Tag("Before2", collection)
                    Dim i2 = collection.{{method.ToLower()}}()           ' Call as extension case insensitive
                    Tag("After2", collection)

                    collection = Untracked()
                    Tag("Before3", collection)
                    Dim i3 = collection.{{method}}                       ' Call as extension without parentheses
                    Tag("After3", collection)

                    collection = Untracked()
                    Tag("Before4", collection)
                    Dim i4 = Enumerable.{{method}}(collection)           ' Call as normal static method
                    Tag("After4", collection)

                    collection = Untracked()
                    Tag("Before5", collection)
                    Dim i5 = Enumerable.{{method.ToLower()}}(collection) ' Call as normal static method case insensitive
                    Tag("After5", collection)

                    collection = Untracked()
                    Tag("Before6", collection)
                    Dim i6 = collection.NonLinqExtensionNonNullChecking()
                    Tag("After6", collection)

                    collection = Untracked()
                    Tag("Before7", collection)
                    Dim i7 = collection.NonLinqExtensionNullChecking()
                    Tag("After7", collection)
                End Sub

                Private Function Untracked() As IEnumerable(Of Integer)
                    Return New List(Of Integer) From {1, 2, 3}
                End Function

                Private Shared Sub Tag(name As String, arg As Object)
                End Sub
            End Class

            Public Module MyExtensions
                <Runtime.CompilerServices.Extension>
                Public Function NonLinqExtensionNonNullChecking(source As IEnumerable(Of Integer)) As Integer
                    Return 42
                End Function
                <Runtime.CompilerServices.Extension>
                Public Function NonLinqExtensionNullChecking(source As IEnumerable(Of Integer)) As Integer
                    If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
                    Return 42
                End Function
            End Module
            """;
        var validator = new SETestContext(code, AnalyzerLanguage.VisualBasic, Array.Empty<SymbolicCheck>()).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);
        validator.TagValue("Before1").Should().BeNull();
        validator.TagValue("After1").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Before2").Should().BeNull();
        validator.TagValue("After2").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Before3").Should().BeNull();
        validator.TagValue("After3").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Before4").Should().BeNull();
        validator.TagValue("After4").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Before5").Should().BeNull();
        validator.TagValue("After5").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Before6").Should().BeNull();
        validator.TagValue("After6").Should().BeNull();
        validator.TagValue("Before7").Should().BeNull();
        validator.TagValue("After7").Should().BeNull();
    }

    [TestMethod]
    public void Invocation_SetNotNull_OnCountOfList_VB()
    {
        var code = $$"""
            Imports System
            Imports System.Linq
            Imports System.Collections.Generic

            Public Class Sample
                Public Sub Main(list As List(Of integer))
                    Tag("Before1", list)
                    Dim i1 = list.Count
                    Tag("After1", list)

                    list = Untracked()
                    Tag("Before2", list)
                    Dim i2 = list.Count()
                    Tag("After2", list)
                End Sub

                Private Function Untracked() As List(Of Integer)
                    Return New List(Of Integer) From {1, 2, 3}
                End Function

                Private Shared Sub Tag(name As String, arg As Object)
                End Sub
            End Class
            """;
        var validator = new SETestContext(code, AnalyzerLanguage.VisualBasic, []).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);
        validator.TagValue("Before1").Should().BeNull();
        validator.TagValue("After1").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Before2").Should().BeNull();
        validator.TagValue("After2").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("Count")]
    [DataRow("AsEnumerable")]
    public void Invocation_DoesntSetNotNull_OnSourceOfLinqLikeExtension(string method)
    {
        var code = $$"""
            using System.Linq;
            using System.Collections.Generic;
            public class Sample
            {
                public void Main(IEnumerable<int> collection)
                {
                    Tag("Before1", collection);
                    _ = collection.{{method}}();           // Call as extension
                    Tag("After1", collection);

                    collection = Untracked();
                    Tag("Before2", collection);
                    _ = Enumerable.{{method}}(collection); // Call as normal static method
                    Tag("After2", collection);
                }

                private IEnumerable<int> Untracked() => [ 1, 2, 3 ];

                private static void Tag(string name, object arg) { }
            }

            public static class Enumerable
            {
                public static int {{method}}(this IEnumerable<int> source) => 42;
            }
            """;
        var validator = new SETestContext(code, AnalyzerLanguage.CSharp, []).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);
        validator.TagValue("Before1").Should().BeNull();
        validator.TagValue("After1").Should().BeNull();
        validator.TagValue("Before2").Should().BeNull();
        validator.TagValue("After2").Should().BeNull();
    }

    [TestMethod]
    public void Invocation_SetsNotNullOnInstance_CS()
    {
        const string code = """
            public class Sample
            {
                public void Main(Sample instanceArg, Sample extensionArg)
                {
                    var preserve = true;
                    Sample extensionNull = null;
                    Tag("BeforeInstance", instanceArg);
                    Tag("BeforeExtensionArg", extensionArg);
                    Tag("BeforeExtensionNull", extensionNull);
                    Tag("BeforePreserve", preserve);

                    instanceArg.InstanceMethod();
                    extensionArg.ExtensionMethod();
                    UntrackedSymbol().InstanceMethod(); // Is not invoked on any symbol, should not fail
                    preserve.ExtensionMethod();
                    preserve.ToString();

                    Tag("AfterInstance", instanceArg);
                    Tag("AfterExtensionArg", extensionArg);
                    Tag("AfterExtensionNull", extensionNull);
                    Tag("AfterPreserve", preserve);
                }

                private void InstanceMethod() { }
                private static void Tag(string name, object arg) { }
                private Sample UntrackedSymbol() => this;
            }

            public static class Extensions
            {
                public static void ExtensionMethod(this Sample s) { }
                public static void ExtensionMethod(this bool b) { }
            }
        """;
        var validator = new SETestContext(code, AnalyzerLanguage.CSharp, []).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);
        validator.TagValue("BeforeInstance").Should().BeNull();
        validator.TagValue("BeforeExtensionArg").Should().BeNull();
        validator.TagValue("BeforeExtensionNull").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagValue("BeforePreserve").Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull);
        validator.TagValue("AfterInstance").Should().HaveOnlyConstraint(ObjectConstraint.NotNull, "Instance method should set NotNull constraint.");
        validator.TagValue("AfterExtensionArg").Should().BeNull("Extensions can run on null instances.");
        validator.TagValue("AfterExtensionNull").Should().HaveOnlyConstraint(ObjectConstraint.Null, "Extensions can run on null instances.");
        validator.TagValue("AfterPreserve").Should().HaveOnlyConstraints(new SymbolicConstraint[] { ObjectConstraint.NotNull, BoolConstraint.True }, "Other constraints should not be removed.");
    }

    [TestMethod]
    public void Invocation_SetsNotNullOnInstance_VB()
    {
        const string code = """
            Public Class Sample

                Public Sub Main(InstanceArg As Sample, StaticArg As Sample, ExtensionArg As Sample)
                    Tag("BeforeInstance", InstanceArg)
                    Tag("BeforeStatic", StaticArg)
                    Tag("BeforeExtension", ExtensionArg)

                    InstanceArg.InstanceMethod()
                    StaticArg.StaticMethod()
                    ExtensionArg.ExtensionMethod()

                    Tag("AfterInstance", InstanceArg)
                    Tag("AfterStatic", StaticArg)
                    Tag("AfterExtension", ExtensionArg)
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

            End Module
            """;
        var validator = new SETestContext(code, AnalyzerLanguage.VisualBasic, []).Validator;
        validator.ValidateContainsOperation(OperationKind.ObjectCreation);
        validator.TagValue("BeforeInstance").Should().BeNull();
        validator.TagValue("BeforeStatic").Should().BeNull();
        validator.TagValue("BeforeExtension").Should().BeNull();
        validator.TagValue("AfterInstance").Should().HaveOnlyConstraint(ObjectConstraint.NotNull, "Instance method should set NotNull constraint.");
        validator.TagValue("AfterStatic").Should().BeNull("Static method can execute from null instances.");
        validator.TagValue("AfterExtension").Should().BeNull("Extensions can run on null instances.");
    }

    [DataTestMethod]
    [DataRow("Initialize();")]
    [DataRow("this.Initialize();")]
    [DataRow("this?.Initialize();")]
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
    public void Invocation_InstanceMethodCall_DoesClearFieldOnThis(string invocation)
    {
        var code = $$"""
            using System;
            using static Extensions;
            public class Sample: IDisposable
            {
                object field;
                static object staticField;

                void Main()
                {
                    field = null;
                    staticField = null;
                    Tag("BeforeField", field);
                    Tag("BeforeStaticField", staticField);
                    {{invocation}}
                    Tag("AfterField", field);
                    Tag("AfterStaticField", staticField);
                }

                private void Initialize() { }
                void IDisposable.Dispose() { }
            }

            public static class Extensions
            {
                public static void SomeExtensionOnSample(this Sample sample) { }
                public static void SomeExtensionOnObject(this object obj) { }
                public static void Tag(string name, object arg) { }
            }
        """;
        var validator = new SETestContext(code, AnalyzerLanguage.CSharp, []).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);
        validator.TagValue("BeforeField").HasConstraint(ObjectConstraint.Null).Should().BeTrue();
        validator.TagValue("BeforeStaticField").HasConstraint(ObjectConstraint.Null).Should().BeTrue();
        validator.TagValue("AfterField").Should().BeNull();
        validator.TagValue("AfterStaticField").Should().BeNull();
    }

    [DataTestMethod]
    [DataRow("var dummy = Property;")]
    [DataRow("var dummy = this.Property;")]
    [DataRow("SampleProperty.InstanceMethod();")]
    [DataRow("this.SampleProperty.InstanceMethod();")]
    public void Invocation_InstanceMethodCall_DoesNotClearFieldForOtherAccess(string invocation)
    {
        var code = $$"""
            ObjectField = null;
            StaticObjectField = null;
            Tag("BeforeField", ObjectField);
            Tag("BeforeStaticField", StaticObjectField);
            {{invocation}}
            Tag("AfterField", ObjectField);
            Tag("AfterStaticField", StaticObjectField);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);
        validator.TagValue("BeforeField").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagValue("BeforeStaticField").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagValue("AfterField").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagValue("AfterStaticField").Should().HaveOnlyConstraint(ObjectConstraint.Null);
    }

    [DataTestMethod]
    [DataRow("otherInstance.InstanceMethod();")]
    [DataRow("(otherInstance).InstanceMethod();")]
    public void Instance_InstanceMethodCall_DoesNotClearFieldsOnOtherInstances(string invocation)
    {
        var code = $"""
            ObjectField = null;
            StaticObjectField = null;
            var otherInstance = new Sample();
            {invocation}
            Tag("Field", ObjectField);
            Tag("StaticField", StaticObjectField);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);
        validator.TagValue("Field").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagValue("StaticField").Should().HaveOnlyConstraint(ObjectConstraint.Null);
    }

    [DataTestMethod]
    [DataRow("(condition ? this : otherInstance).InstanceMethod();")]
    [DataRow("(condition ? this : otherInstance).ExtensionMethod();")] // invocation with flow-capture and conversion on the receiver
    public void Instance_InstanceMethodCall_ClearsFields_Ternary(string instanceCall)
    {
        var code = $$"""
            public class Sample
            {
                private object ObjectField;
                private static object StaticObjectField;

                public void Test(bool condition)
                {
                    ObjectField = null;
                    StaticObjectField = null;
                    var otherInstance = new Sample();
                    {{instanceCall}}
                    Extensions.Tag("End");
                }

                public void InstanceMethod() { }
            }
            public static class Extensions
            {
                public static void ExtensionMethod(this object o) { }
                public static void Tag(string name) { }
            }
            """;
        var validator = new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);
        validator.ValidateContainsOperation(OperationKind.FlowCapture);
        validator.ValidateContainsOperation(OperationKind.FlowCaptureReference);
        var field = validator.Symbol("ObjectField");
        var staticField = validator.Symbol("StaticObjectField");
        var condition = validator.Symbol("condition");
        validator.TagStates("End").Should().SatisfyRespectively(
            x => // Branch for "this"
            {
                x[condition].Should().BeNull(); // Should have BoolConstraint.True
                x[field].Should().BeNull();
                x[staticField].Should().BeNull();
            },
            x => // Branch for "otherInstance"
            {
                x[condition].Should().BeNull();  // Should have BoolConstraint.False
                x[field].HasConstraint(ObjectConstraint.Null).Should().BeTrue();
                x[staticField].HasConstraint(ObjectConstraint.Null).Should().BeTrue();
            });
    }

    [TestMethod]
    public void Instance_InstanceMethodCall_ClearsFieldInConsistentManner()
    {
        var code = """
            ObjectField = null;
            Tag("InitNull", ObjectField);
            InstanceMethod();
            Tag("AfterInvocationNull", ObjectField);
            ObjectField = new object();
            Tag("InitNotNull", ObjectField);
            InstanceMethod();
            Tag("AfterInvocationNotNull", ObjectField);
            if (ObjectField == null)
            {
                Tag("IfBefore", ObjectField);
                InstanceMethod();
                Tag("IfAfter", ObjectField);
            }
            else
            {
                Tag("ElseBefore", ObjectField);
                InstanceMethod();
                Tag("ElseAfter", ObjectField);
            }
            Tag("AfterIfElse", ObjectField);
            """;
        var invalidateConstraint = DummyConstraint.Dummy;
        var dontInvalidateConstraint = LockConstraint.Held;
        var check = new PostProcessTestCheck(x => x.Operation.Instance.Kind == OperationKindEx.SimpleAssignment
            && IFieldReferenceOperationWrapper.FromOperation(ISimpleAssignmentOperationWrapper.FromOperation(x.Operation.Instance).Target).Member is var field
            ? x.SetSymbolConstraint(field, invalidateConstraint).SetSymbolConstraint(field, dontInvalidateConstraint)
            : x.State);
        var validator = SETestContext.CreateCS(code, check).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);
        validator.TagValue("InitNull").Should().HaveOnlyConstraints(ObjectConstraint.Null, invalidateConstraint, dontInvalidateConstraint);
        validator.TagValue("AfterInvocationNull").Should().HaveOnlyConstraint(dontInvalidateConstraint, "ObjectConstraint.Null and invalidateConstraint are reset");
        validator.TagValue("InitNotNull").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, invalidateConstraint, dontInvalidateConstraint);
        validator.TagValue("AfterInvocationNotNull").Should().HaveOnlyConstraint(dontInvalidateConstraint, "ObjectConstraint.NotNull and invalidateConstraint are reset");
        validator.TagValue("IfBefore").Should().HaveOnlyConstraints(ObjectConstraint.Null, dontInvalidateConstraint);
        validator.TagValue("IfAfter").Should().HaveOnlyConstraint(dontInvalidateConstraint, "ObjectConstraint.Null is reset");
        validator.TagValue("ElseBefore").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, dontInvalidateConstraint);
        validator.TagValue("ElseAfter").Should().HaveOnlyConstraint(dontInvalidateConstraint, "ObjectConstraint.NotNull is reset");
        validator.TagValues("AfterIfElse").Should().Equal(
        [
            SymbolicValue.Empty.WithConstraint(dontInvalidateConstraint),
        ]);
    }

    [TestMethod]
    public void Instance_InstanceMethodCall_ClearsField()
    {
        var code = """
            if (this.ObjectField == null)
            {
                this.InstanceMethod(StaticObjectField == null ? 1 : 0);
            }
            Tag("After", this.ObjectField);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValues("After").Should().Equal(
            SymbolicValue.Empty.WithConstraint(ObjectConstraint.NotNull),
            null);
    }

    [TestMethod]
    public void Instance_InstanceMethodCall_ClearsFieldWithBranchInArgument()
    {
        var code = """
            this.ObjectField = null;
            this.InstanceMethod(boolParameter ? 1 : 0);
            Tag("After", this.ObjectField);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("After").Should().BeNull();
    }

    [TestMethod]
    public void Invocation_StaticMethodCall_ClearsField()
    {
        var code = """
            public class Sample: Base
            {
                public static object SampleField1;
                public static object SampleField2;

                public static void SampleMethod()
                {
                    Base.BaseField = null;
                    Other.OtherField = null;
                    Sample.SampleField1 = null;
                    Sample.SampleField2 = null;
                    Tagger.Tag("Start_Base_BaseField", BaseField);
                    Tagger.Tag("Start_Other_OtherField", Other.OtherField);
                    Tagger.Tag("Start_Sample_SampleField1", SampleField1);
                    Tagger.Tag("Start_Sample_SampleField2", SampleField2);

                    SampleMethod();
                    Tagger.Tag("SampleMethod_Base_BaseField", BaseField);
                    Tagger.Tag("SampleMethod_Other_OtherField", Other.OtherField);
                    Tagger.Tag("SampleMethod_Sample_SampleField1", SampleField1);
                    Tagger.Tag("SampleMethod_Sample_SampleField2", SampleField2);

                    Base.BaseField = null;
                    Other.OtherField = null;
                    Sample.SampleField1 = null;
                    Sample.SampleField2 = null;
                    Other.OtherMethod();
                    Tagger.Tag("OtherMethod_Base_BaseField", BaseField);
                    Tagger.Tag("OtherMethod_Other_OtherField", Other.OtherField);
                    Tagger.Tag("OtherMethod_Sample_SampleField1", SampleField1);
                    Tagger.Tag("OtherMethod_Sample_SampleField2", SampleField2);

                    Base.BaseField = null;
                    Other.OtherField = null;
                    Sample.SampleField1 = null;
                    Sample.SampleField2 = null;
                    BaseMethod();
                    Tagger.Tag("BaseMethod_Base_BaseField", BaseField);
                    Tagger.Tag("BaseMethod_Other_OtherField", Other.OtherField);
                    Tagger.Tag("BaseMethod_Sample_SampleField1", SampleField1);
                    Tagger.Tag("BaseMethod_Sample_SampleField2", SampleField2);
                }
            }

            public static class Tagger
            {
                public static void Tag<T>(string name, T value) { }
            }

            public class Base
            {
                protected static object BaseField;
                public static void BaseMethod() { }
            }
            public class Other
            {
                public static object OtherField;
                public static void OtherMethod() { }
            }
            """;
        var validator = new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator;
        validator.TagValue("Start_Base_BaseField").HasConstraint(ObjectConstraint.Null).Should().BeTrue();
        validator.TagValue("Start_Other_OtherField").HasConstraint(ObjectConstraint.Null).Should().BeTrue();
        validator.TagValue("Start_Sample_SampleField1").HasConstraint(ObjectConstraint.Null).Should().BeTrue();
        validator.TagValue("Start_Sample_SampleField2").HasConstraint(ObjectConstraint.Null).Should().BeTrue();

        // SampleMethod() resets own field and base class fields, but not other class fields
        validator.TagValue("SampleMethod_Base_BaseField").Should().BeNull();
        validator.TagValue("SampleMethod_Other_OtherField").HasConstraint(ObjectConstraint.Null).Should().BeTrue();
        validator.TagValue("SampleMethod_Sample_SampleField1").Should().BeNull();
        validator.TagValue("SampleMethod_Sample_SampleField2").Should().BeNull();

        // OtherMethod() resets only its own constraints
        validator.TagValue("OtherMethod_Base_BaseField").HasConstraint(ObjectConstraint.Null).Should().BeTrue();
        validator.TagValue("OtherMethod_Other_OtherField").Should().BeNull();
        validator.TagValue("OtherMethod_Sample_SampleField1").HasConstraint(ObjectConstraint.Null).Should().BeTrue();
        validator.TagValue("OtherMethod_Sample_SampleField2").HasConstraint(ObjectConstraint.Null).Should().BeTrue();

        // BaseMethod() called from Sample only resets Base field
        validator.TagValue("BaseMethod_Base_BaseField").Should().BeNull();
        validator.TagValue("BaseMethod_Other_OtherField").HasConstraint(ObjectConstraint.Null).Should().BeTrue();
        validator.TagValue("BaseMethod_Sample_SampleField1").HasConstraint(ObjectConstraint.Null).Should().BeTrue();
        validator.TagValue("BaseMethod_Sample_SampleField2").HasConstraint(ObjectConstraint.Null).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("StaticMethod();")]
    [DataRow("Sample.StaticMethod();")]
    public void Invocation_StaticMethodCall_DoesNotClearInstanceFields(string invocation)
    {
        var code = $"""
            ObjectField = null;
            StaticObjectField = null;
            Tag("BeforeField", ObjectField);
            Tag("BeforeStaticField", StaticObjectField);
            {invocation}
            Tag("AfterField", ObjectField);
            Tag("AfterStaticField", StaticObjectField);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);
        validator.TagValue("BeforeField").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagValue("BeforeStaticField").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagValue("AfterField").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagValue("AfterStaticField").Should().HaveNoConstraints();
    }

    [TestMethod]
    public void Invocation_IsNullOrEmpty_ValidateOrder()
    {
        var validator = SETestContext.CreateCS("var isNullOrEmpy = string.IsNullOrEmpty(arg);", "string arg").Validator;
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
        const string code = """
            var isNullOrEmpy = string.IsNullOrEmpty(arg);
            Tag("IsNullOrEmpy", isNullOrEmpy);
            Tag("Arg", arg);
            """;
        var validator = SETestContext.CreateCS(code, "string arg").Validator;
        validator.TagValues("IsNullOrEmpy").Should().Equal(
            SymbolicValue.NotNull.WithConstraint(BoolConstraint.False),      // False/NotNull
            SymbolicValue.NotNull.WithConstraint(BoolConstraint.True),       // True/Null
            SymbolicValue.NotNull.WithConstraint(BoolConstraint.True));      // True/NotNull
        validator.TagValues("Arg").Should().Equal(
            SymbolicValue.Empty.WithConstraint(ObjectConstraint.NotNull),  // False/NotNull
            SymbolicValue.Empty.WithConstraint(ObjectConstraint.Null),     // True/Null
            SymbolicValue.Empty.WithConstraint(ObjectConstraint.NotNull)); // True/NotNull
    }

    [TestMethod]
    public void Invocation_IsNullOrEmpty_NestedProperty()
    {
        const string code = """
            if (!string.IsNullOrEmpty(exception?.Message))
            {
                Tag("ExceptionChecked", exception);
            }
            Tag("ExceptionAfterCheck", exception);
            """;
        var validator = SETestContext.CreateCS(code, "InvalidOperationException exception").Validator;
        validator.TagValue("ExceptionChecked").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValues("ExceptionAfterCheck").Should().Equal(
            SymbolicValue.Empty.WithConstraint(ObjectConstraint.Null),
            SymbolicValue.Empty.WithConstraint(ObjectConstraint.NotNull));
    }

    [TestMethod]
    public void Invocation_IsNullOrEmpty_TryFinally()
    {
        const string code = """
            try
            {
                if (string.IsNullOrEmpty(arg)) return;
            }
            finally
            {
                Tag("ArgInFinally", arg);
            }
            """;
        var validator = SETestContext.CreateCS(code, "string arg").Validator;
        validator.TagValues("ArgInFinally").Should().Equal(
            null,
            SymbolicValue.Empty.WithConstraint(ObjectConstraint.NotNull),
            SymbolicValue.Empty.WithConstraint(ObjectConstraint.Null));   // Wrong. IsNullOrEmpty does not throw and "arg" is known to be not null.
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
        var code = $"""
            var value = {expression};
            Tag("Value", value);
            """;
        var enumerableValidator = SETestContext.CreateCS(code, "IEnumerable<object> arg").Validator;
        enumerableValidator.TagValue("Value").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);

        var queryableValidator = SETestContext.CreateCS(code, "IQueryable<object> arg").Validator;
        queryableValidator.TagValue("Value").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("FirstOrDefault();")]
    [DataRow("LastOrDefault();")]
    [DataRow("SingleOrDefault();")]
    public void Invocation_LinqEnumerableAndQueryable_NullOrNotNull(string expression)
    {
        var code = $"""
            var value = arg.{expression};
            Tag("Value", value);
            """;
        var enumerableValidator = SETestContext.CreateCS(code, $"IEnumerable<object> arg").Validator;
        enumerableValidator.TagValues("Value").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));

        var queryableValidator = SETestContext.CreateCS(code, $"IQueryable<object> arg").Validator;
        queryableValidator.TagValues("Value").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]    // Just a few examples to demonstrate that we don't set ObjectContraint for all
    [DataRow("Min()")]
    [DataRow("ElementAtOrDefault(42);")]
    [DataRow("FirstOrDefault();")]
    [DataRow("LastOrDefault();")]
    [DataRow("SingleOrDefault();")]
    public void Invocation_LinqEnumerable_Unknown_Int(string expression)
    {
        var code = $"""
            var value = arg.{expression};
            Tag("Value", value);
            """;
        var validator = SETestContext.CreateCS(code, $"IEnumerable<int> arg").Validator;
        validator.TagValue("Value").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]    // Just a few examples to demonstrate that we don't set ObjectContraint for all
    [DataRow("First()")]
    [DataRow("ElementAtOrDefault(42);")]
    public void Invocation_LinqEnumerable_Unknown_Object(string expression)
    {
        var code = $"""
            var value = arg.{expression};
            Tag("Value", value);
            """;
        var validator = SETestContext.CreateCS(code, $"IEnumerable<object> arg").Validator;
        validator.TagValue("Value").Should().BeNull();
    }

    [TestMethod]
    public void Invocation_ElementExistsCheckMethods_NoParameters_NoConstrains()
    {
        var code = """
            var value = collection.Any();
            Tag("Value");
            """;
        var enumerableValidator = SETestContext.CreateCS(code, $"IEnumerable<object> collection").Validator;
        var collectionSymbol = enumerableValidator.Symbol("collection");
        var valueSymbol = enumerableValidator.Symbol("value");
        enumerableValidator.TagStates("Value").Should().HaveCount(2)
            .And.ContainSingle(x => x[collectionSymbol].HasConstraint(CollectionConstraint.NotEmpty) && x[valueSymbol].HasConstraint(BoolConstraint.True))
            .And.ContainSingle(x => x[collectionSymbol].HasConstraint(CollectionConstraint.Empty) && x[valueSymbol].HasConstraint(BoolConstraint.False));
    }

    [TestMethod]
    public void Invocation_ElementExistsCheckMethods_NoParameters_CollectionNotEmpty()
    {
        var code = """
            collection.Add(1);
            var value = collection.Any();
            Tag("Value");
            """;
        var enumerableValidator = SETestContext.CreateCS(code, $"List<int> collection").Validator;
        var collectionSymbol = enumerableValidator.Symbol("collection");
        var valueSymbol = enumerableValidator.Symbol("value");
        enumerableValidator.TagStates("Value").Should().HaveCount(1)
            .And.ContainSingle(x => x[collectionSymbol].HasConstraint(CollectionConstraint.NotEmpty) && x[valueSymbol].HasConstraint(BoolConstraint.True));
    }

    [TestMethod]
    public void Invocation_ElementExistsCheckMethods_Parameters_CollectionNotEmpty()
    {
        var code = """
            collection.Add(1);
            var value = collection.Any(x => x is 1);
            Tag("Value");
            """;
        var enumerableValidator = SETestContext.CreateCS(code, $"List<int> collection").Validator;
        var collectionSymbol = enumerableValidator.Symbol("collection");
        var valueSymbol = enumerableValidator.Symbol("value");
        enumerableValidator.TagStates("Value").Should().HaveCount(1)
            .And.Contain(x => x[collectionSymbol].HasConstraint(CollectionConstraint.NotEmpty))
            .And.NotContain(x => x[valueSymbol].HasConstraint<BoolConstraint>());
    }

    [DataTestMethod]
    [DataRow("Any(x => x is 1)")]
    [DataRow("Exists(x => x is 1)")]
    [DataRow("Contains(1)")]
    public void Invocation_ElementExistsCheckMethods_NoPrexistingConstrains(string expression)
    {
        var code = $"""
            var value = collection.{expression};
            Tag("Value");
            """;
        var enumerableValidator = SETestContext.CreateCS(code, $"List<int> collection").Validator;
        var collectionSymbol = enumerableValidator.Symbol("collection");
        var valueSymbol = enumerableValidator.Symbol("value");
        enumerableValidator.TagStates("Value").Should().HaveCount(2)
            .And.ContainSingle(x => x[collectionSymbol].HasConstraint(CollectionConstraint.NotEmpty) && x[valueSymbol].HasConstraint(BoolConstraint.True));
    }

    [DataTestMethod]
    [DataRow("Any()")]
    [DataRow("Any(x => x is 1)")]
    [DataRow("Exists(x => x is 1)")]
    [DataRow("Contains(1)")]
    public void Invocation_ElementExistsCheckMethods_CollectionAlreadyHasContraint(string expression)
    {
        var code = $"""
            collection.Clear();
            var value = collection.{expression};
            Tag("Value");
            """;
        var enumerableValidator = SETestContext.CreateCS(code, $"List<int> collection").Validator;
        var collectionSymbol = enumerableValidator.Symbol("collection");
        var valueSymbol = enumerableValidator.Symbol("value");
        enumerableValidator.TagStates("Value").Should().HaveCount(1)
            .And.ContainSingle(x => x[collectionSymbol].HasConstraint(CollectionConstraint.Empty) && x[valueSymbol].HasConstraint(BoolConstraint.False));
    }

    [TestMethod]
    public void Invocation_Linq_VB()
    {
        const string code = """
            Dim Query = From Item In Items Where Item IsNot Nothing
            If Query.Count <> 0 Then
                Dim Value = Query(0)
                Tag("Value", Value)
            End If
            """;
        var validator = SETestContext.CreateVB(code, "Items() As Object").Validator;
        validator.TagValue("Value").Should().BeNull();
    }

    [DataTestMethod]
    [DataRow("arg.Clone()")]
    [DataRow("arg.Concat(\"foo\")")]
    [DataRow("string.Copy(arg)")]
    [DataRow("string.Format( \"\", arg)")]
    [DataRow("arg.GetEnumerator()")]
    [DataRow("arg.Insert(0, \"foo\")")]
    [DataRow("string.Intern(arg)")]
    [DataRow("string.Join(\"-\", arg, arg)")]
    [DataRow("arg.Normalize()")]
    [DataRow("arg.PadLeft(0)")]
    [DataRow("arg.PadRight(0)")]
    [DataRow("arg.Remove(0)")]
    [DataRow("arg.Replace(\"-\", \"\")")]
    [DataRow("arg.Split()")]
    [DataRow("arg.Substring(2)")]
    [DataRow("arg.ToCharArray()")]
    [DataRow("arg.ToLower()")]
    [DataRow("arg.ToLowerInvariant()")]
    [DataRow("arg.ToString()")]
    [DataRow("arg.ToUpper()")]
    [DataRow("arg.ToUpperInvariant()")]
    [DataRow("arg.Trim()")]
    [DataRow("arg.TrimEnd()")]
    [DataRow("arg.TrimStart()")]
#if NET
    [DataRow("string.Create(System.Globalization.CultureInfo.InvariantCulture, $\"{arg}\");")]
    [DataRow("arg.GetPinnableReference()")]
    [DataRow("arg.ReplaceLineEndings()")]
#endif
    public void Invocation_StringMethods_NotNull(string expression)
    {
        var code = $"""
            var value = {expression};
            Tag("Value", value);
            """;
        var stringMethodValidator = SETestContext.CreateCS(code, "string arg").Validator;
        stringMethodValidator.TagValue("Value").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void Invocation_StringMethods_NotNull_VB()
    {
        var code = $"""
            Dim Value = Arg.Trim()
            Tag("Value", Value)
            """;
        var stringMethodValidator = SETestContext.CreateVB(code, "Arg As String").Validator;
        stringMethodValidator.TagValue("Value").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("Object = Nothing", true)]
    [DataRow("Object = New Object()", false)]
    [DataRow("Integer = Nothing", false)]   // While it can be assigned Nothing, value 0 is stored. And that is not null
    [DataRow("TStruct = Nothing", false)]
    [DataRow("T = Nothing", false)]
    public void Invocation_InformationIsNothing_KnownSymbol(string declarationSuffix, bool expected)
    {
        var code = $"""
            Public Sub Main(Of T, TStruct As Structure)()
                Dim Value As {declarationSuffix}
                Dim Result As Boolean = IsNothing(CType(DirectCast(Value, Object), Object)) ' Some conversions in the way to detect the value type
                Tag("Result", Result)
            End Sub
            """;
        var validator = SETestContext.CreateVBMethod(code).Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.From(expected));
    }

    [DataTestMethod]
    [DataRow("Object")]
    [DataRow("Exception")]
    [DataRow("Integer?")]
    [DataRow("TClass")]
    public void Invocation_InformationIsNothing_UnknownSymbol(string type)
    {
        var code = $"""
            Public Sub Main(Of TClass As Class)(Arg As {type})
                Dim Result As Boolean = IsNothing(Arg)
                Tag("Result", Result)
            End Sub
            """;
        var validator = SETestContext.CreateVBMethod(code).Validator;
        var argSymbol = validator.Symbol("Arg");
        var resultSymbol = validator.Symbol("Result");
        validator.TagStates("Result").Should().HaveCount(2)
            .And.ContainSingle(x => x[argSymbol].HasConstraint(ObjectConstraint.Null) && x[resultSymbol].HasConstraint(BoolConstraint.True))
            .And.ContainSingle(x => x[argSymbol].HasConstraint(ObjectConstraint.NotNull) && x[resultSymbol].HasConstraint(BoolConstraint.False));
    }

    [TestMethod]
    public void Invocation_InformationIsNothing_NoTrackedSymbol()
    {
        var code = $"""
            Dim Result As Boolean = IsNothing(Arg.ToString())
            Tag("Result", Result)
            """;
        var validator = SETestContext.CreateVB(code, "Arg As Object").Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("arg is not true", "bool?")]
    [DataRow("arg is not false", "bool?")]
    [DataRow("arg is not 42", "object")]
    [DataRow("arg is not { Length: 0 }", "string")]
    [DataRow("arg.GetValueOrDefault()", "bool?")]
    public void Invocation_DebugAssert_DoesNotLearn(string expression, string argType) =>
        DebugAssertValues(expression, argType).Should().SatisfyRespectively(x => x.Should().HaveNoConstraints());

    [DataTestMethod]
    [DataRow("arg != null", "object")]
    [DataRow("arg != null", "int?")]
    [DataRow("arg != null", "bool?")]
    [DataRow("arg is not null", "object")]
    [DataRow("arg is not null", "int?")]
    [DataRow("arg is not null", "bool?")]
    [DataRow("arg is { }", "object")]
    [DataRow("arg is { }", "int?")]
    [DataRow("arg is { }", "bool?")]
    public void Invocation_DebugAssert_LearnsNotNull_Simple(string expression, string argType) =>
        DebugAssertValues(expression, argType).Should().SatisfyRespectively(x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));

    [TestMethod]
    public void Invocation_DebugAssert_LearnsNotNull_AndAlso() =>
        DebugAssertValues("arg != null && condition").Should().SatisfyRespectively(x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));

    [TestMethod]
    public void Invocation_DebugAssert_LearnsNotNullForAll_AndAlso()
    {
        var code = """
            Debug.Assert(arg1 != null && arg2 != null);
            Tag("Arg1", arg1);
            Tag("Arg2", arg2);
            """;
        var validator = SETestContext.CreateCS(code, $"object arg1, object arg2").Validator;
        validator.TagValue("Arg1").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Arg2").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void Invocation_DebugAssert_LearnsNotNull_OrElse() =>
        DebugAssertValues("arg != null || condition").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull),
            x => x.Should().HaveOnlyConstraint(ObjectConstraint.Null));

    [TestMethod]
    public void Invocation_DebugAssert_LearnsBoolConstraint_Simple() =>
        DebugAssertValues("arg", "bool").Should().SatisfyRespectively(x => x.Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull));

    [TestMethod]
    public void Invocation_DebugAssert_LearnsBoolConstraint_Binary() =>
        DebugAssertValues("arg == true", "bool").Should().SatisfyRespectively(x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True));

    [DataTestMethod]
    [DataRow("arg is true", true)]
    [DataRow("arg is false", false)]
    public void Invocation_DebugAssert_LearnsBoolConstraint_Nullable(string expression, bool expected) =>
        DebugAssertValues(expression, "bool?").Should().SatisfyRespectively(x => x.Should().HaveOnlyConstraints(BoolConstraint.From(expected)));

    [TestMethod]
    public void Invocation_DebugAssert_LearnsBoolConstraint_AlwaysEnds() =>
        DebugAssertValues("false", "bool").Should().BeEmpty();

    [DataTestMethod]
    [DataRow("!arg")]
    [DataRow("!!!arg")]
    public void Invocation_DebugAssert_LearnsBoolConstraint_Negated(string expression) =>
        DebugAssertValues(expression, "bool").Should().SatisfyRespectively(x => x.Should().HaveOnlyConstraints(BoolConstraint.False, ObjectConstraint.NotNull));

    [TestMethod]
    public void Invocation_DebugAssert_CustomNoParameters_DoesNotFail()
    {
        const string code = """
            using System.Diagnostics;

            public class Sample
            {
                public void Main()
                {
                    Debug.Assert();
                    Tag("End");
                }

                private static void Tag(string name) { }
            }

            namespace System.Diagnostics
            {
                public static class Debug
                {
                    public static void Assert() { }
                }
            }
            """;
        new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator.ValidateTagOrder("End");
    }

    [DataTestMethod]
    [DataRow("int?")]
    [DataRow("bool?")]
    public void Invocation_DebugAssert_NullableHasValue_Simple(string argType) =>
        DebugAssertValues("arg.HasValue", argType).Should().SatisfyRespectively(x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));

    [DataTestMethod]
    [DataRow("int?")]
    [DataRow("bool?")]
    public void Invocation_DebugAssert_NullableHasValue_Binary(string argType) =>
        DebugAssertValues("arg.HasValue == true", argType).Should().SatisfyRespectively(x => x.Should().HaveOnlyConstraint(ObjectConstraint.NotNull));

    [DataTestMethod]
    [DynamicData(nameof(ThrowHelperCalls))]
    public void Invocation_ThrowHelper_StopProcessing(string throwHelperCall)
    {
        var code = $$"""
            Tag("Before");
            {{throwHelperCall}}
            Tag("Unreachable");
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder("Before");
        validator.ValidateExitReachCount(0);
        validator.ValidateExecutionCompleted();
    }

    [DataTestMethod]
    [DynamicData(nameof(ThrowHelperCalls))]
    public void Invocation_ThrowHelper_OnlyInBranch(string throwHelperCall)
    {
        var code = $$"""
            if (condition)
            {
                Tag("Before");
                {{throwHelperCall}}
                Tag("Unreachable");
            }
            Tag("End");
            """;
        var validator = SETestContext.CreateCS(code, "bool condition").Validator;
        validator.ValidateTagOrder("Before", "End");
        validator.ValidateExitReachCount(1);
        validator.ValidateExecutionCompleted();
    }

    [DataTestMethod]
    [DynamicData(nameof(ThrowHelperCalls))]
    public void Invocation_ThrowHelper_TryCatchFinally(string throwHelperCall)
    {
        var code = $$"""
            try
            {
                {{throwHelperCall}}
                Tag("Unreachable");
            }
            catch
            {
                Tag("Catch");
            }
            finally
            {
                Tag("Finally");
            }
            Tag("End");
            """;
        var validator = SETestContext.CreateCS(code, "bool condition").Validator;
        validator.ValidateTagOrder("Catch", "Finally", "Finally", "End");
        validator.ValidateExitReachCount(2);
        validator.ValidateExecutionCompleted();
    }

    [TestMethod]
    public void Invocation_TargetMethodIsDelegateInvoke()
    {
        var code = """
            Func<Action> f = () => new Action(()=> { });
            f()();
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKindEx.Invocation);
        validator.ValidateExitReachCount(1);
        validator.ValidateExecutionCompleted();
    }

    [DataTestMethod]
    [DataRow("null", "null", true, ConstraintKind.Null, ConstraintKind.Null)]
    [DataRow("null", "new object()", false, ConstraintKind.Null, ConstraintKind.NotNull)]
    [DataRow("new object()", "null", false, ConstraintKind.NotNull, ConstraintKind.Null)]
    [DataRow("new int?()", "null", true, ConstraintKind.Null, ConstraintKind.Null)]
    [DataRow("(int?)null", "null", true, ConstraintKind.Null, ConstraintKind.Null)]
    [DataRow("new char?('x')", "null", false, ConstraintKind.NotNull, ConstraintKind.Null)]
    [DataRow("new char?()", "'x'", false, ConstraintKind.Null, ConstraintKind.NotNull)]
    public void Invocation_ObjectEquals_LearnResult(string left, string right, bool expectedResult, ConstraintKind expectedConstraintLeft, ConstraintKind expectedConstraintRight)
    {
        var code = $"""
            object left = {left};
            object right = {right};
            var result = object.Equals(left, right);
            Tag("Result", result);
            Tag("Left", left);
            Tag("Right", right);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.From(expectedResult));
        validator.TagValue("Left").AllConstraints.Should().ContainSingle().Which.Kind.Should().Be(expectedConstraintLeft);
        validator.TagValue("Right").AllConstraints.Should().ContainSingle().Which.Kind.Should().Be(expectedConstraintRight);
    }

    [DataTestMethod]
    [DataRow("true", "null", false)]
    [DataRow("null", "true", false)]
    [DataRow("null", "(bool?)true", false)]
    public void Invocation_ObjectEqualsMixedNullObjectArguments_LearnResult(string left, string right, bool expectedResult)
    {
        var code = $"""
            object left = {left};
            object right = {right};
            var result = object.Equals(left, right);
            Tag("Result", result);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.From(expectedResult));
    }

    [DataTestMethod]
    [DataRow("new object()", "new object()")]
    [DataRow("new object()", "Unknown<object>()")]
    [DataRow("Unknown<object>()", "new object()")]
    [DataRow("Unknown<object>()", "Unknown<object>()")]
    [DataRow("new int?(42)", "Unknown<int?>()")]
    [DataRow("true", "new object()")]
    [DataRow("(bool?)true", "new object()")]
    public void Invocation_ObjectEquals_DoesNotLearnResult(string left, string right)
    {
        var code = $"""
            object left = {left};
            object right = {right};
            var result = object.Equals(left, right);
            Tag("Result", result);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("null", "arg", "object")]
    [DataRow("null", "arg", "int?")]
    [DataRow("arg", "null", "object")]
    [DataRow("arg", "null", "int?")]
    public void Invocation_ObjectEquals_SplitsToBothResults(string left, string right, string argType)
    {
        var code = $"""
            var result = object.Equals({left}, {right});
            Tag("End");
            """;
        var validator = SETestContext.CreateCS(code, $"{argType} arg").Validator;
        var result = validator.Symbol("result");
        var arg = validator.Symbol("arg");
        validator.TagStates("End").Should().SatisfyRespectively(
            x =>
            {
                x[result].HasConstraint(BoolConstraint.True).Should().BeTrue();
                x[arg].HasConstraint(ObjectConstraint.Null).Should().BeTrue();
            },
            x =>
            {
                x[result].HasConstraint(BoolConstraint.False).Should().BeTrue();
                x[arg].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
            });
    }

    [DataTestMethod]
    [DataRow("null", "null", true)]
    [DataRow("null", "42", false)]
    [DataRow("42", "null", false)]
    public void Invocation_NullableEquals_LearnResult(string left, string right, bool expectedResult)
    {
        var code = $"""
            int? left = {left};
            int? right = {right};
            var result = left.Equals(right);
            Tag("Result", result);
            """;
        SETestContext.CreateCS(code).Validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.From(expectedResult));
    }

    [DataTestMethod]
    [DataRow("42", "Unknown<int?>()")]
    [DataRow("Unknown<int?>()", "42")]
    [DataRow("Unknown<int?>()", "Unknown<int?>()")]
    public void Invocation_NullableEquals_DoesNotLearnResult(string left, string right)
    {
        var code = $"""
            int? left = {left};
            int? right = {right};
            var result = left.Equals(right);
            Tag("Result", result);
            """;
        SETestContext.CreateCS(code).Validator.TagValue("Result").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("isNull", "arg")]
    [DataRow("arg", "null")]
    public void Invocation_NullableEquals_Null_SplitsToBothResults(string left, string right)
    {
        var code = $"""
            int? isNull = null;
            var result = {left}.Equals({right});
            Tag("End");
            """;
        var validator = SETestContext.CreateCS(code, "int? arg").Validator;
        var result = validator.Symbol("result");
        var arg = validator.Symbol("arg");
        validator.TagStates("End").Should().SatisfyRespectively(
            x =>
            {
                x[result].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True);
                x[arg].Should().HaveOnlyConstraint(ObjectConstraint.Null);
            },
            x =>
            {
                x[result].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False);
                x[arg].Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
            });
    }

    [TestMethod]
    public void Invocation_Equals_CustomSignatures_NotSupported()
    {
        const string code = """
            public void Main()
            {
                var instanceOne = Equals(null);
                var instanceTwo = Equals(null, null);
                var noArgs = Equals();
                var moreArgs = Equals(null, null, null);

                Tag("InstanceOne", instanceOne);
                Tag("InstanceTwo", instanceTwo);
                Tag("NoArgs", noArgs);
                Tag("MoreArgs", moreArgs);
            }

            private bool Equals(object a) => false;
            private bool Equals(object a, object b) => false;
            private static bool Equals() => false;
            private static bool Equals(object a, object b, object c) => false;
            """;
        var validator = SETestContext.CreateCSMethod(code).Validator;
        validator.TagValue("InstanceOne").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("InstanceTwo").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("NoArgs").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("MoreArgs").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("null", "null", true, ConstraintKind.Null, ConstraintKind.Null)]
    [DataRow("null", "new object()", false, ConstraintKind.Null, ConstraintKind.NotNull)]
    [DataRow("new object()", "null", false, ConstraintKind.NotNull, ConstraintKind.Null)]
    [DataRow("new int?()", "null", true, ConstraintKind.Null, ConstraintKind.Null)]
    [DataRow("(int?)null", "null", true, ConstraintKind.Null, ConstraintKind.Null)]
    [DataRow("new char?('x')", "null", false, ConstraintKind.NotNull, ConstraintKind.Null)]
    [DataRow("new char?()", "'x'", false, ConstraintKind.Null, ConstraintKind.NotNull)]
    public void Invocation_ReferenceEquals_LearnResult(string left, string right, bool expectedResult, ConstraintKind expectedConstraintLeft, ConstraintKind expectedConstraintRight)
    {
        var code = $"""
            object left = {left};
            object right = {right};
            var result = ReferenceEquals(left, right);
            Tag("Result", result);
            Tag("Left", left);
            Tag("Right", right);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.From(expectedResult));
        validator.TagValue("Left").AllConstraints.Should().ContainSingle().Which.Kind.Should().Be(expectedConstraintLeft);
        validator.TagValue("Right").AllConstraints.Should().ContainSingle().Which.Kind.Should().Be(expectedConstraintRight);
    }

    [DataTestMethod]
    [DataRow("new object()", "new object()")]
    [DataRow("new object()", "Unknown<object>()")]
    [DataRow("Unknown<object>()", "new object()")]
    [DataRow("Unknown<object>()", "Unknown<object>()")]
    [DataRow("new int?(42)", "Unknown<int?>()")]
    [DataRow("(int?)42", "(int?)42")]
    [DataRow("(int?)42", "(int?)0")]
    public void Invocation_ReferenceEquals_DoesNotLearnResult(string left, string right)
    {
        var code = $"""
            object left = {left};
            object right = {right};
            var result = ReferenceEquals(left, right);
            Tag("Result", result);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("null", "arg", "object")]
    [DataRow("null", "arg", "int?")]
    [DataRow("arg", "null", "object")]
    [DataRow("arg", "null", "int?")]
    public void Invocation_ReferenceEquals_SplitsToBothResults(string left, string right, string argType)
    {
        var code = $"""
            var result = ReferenceEquals({left}, {right});
            Tag("End");
            """;
        var validator = SETestContext.CreateCS(code, $"{argType} arg").Validator;
        var result = validator.Symbol("result");
        var arg = validator.Symbol("arg");
        validator.TagStates("End").Should().SatisfyRespectively(
            x =>
            {
                x[result].HasConstraint(BoolConstraint.True).Should().BeTrue();
                x[arg].HasConstraint(ObjectConstraint.Null).Should().BeTrue();
            },
            x =>
            {
                x[result].HasConstraint(BoolConstraint.False).Should().BeTrue();
                x[arg].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
            });
    }

    [TestMethod]
    public void Invocation_ReferenceEquals_CustomSignatures_NotSupported()
    {
        const string code = """
            public void Main()
            {
                var args0 = ReferenceEquals();
                var args1 = ReferenceEquals(null);
                var args2 = ReferenceEquals(null, null);
                var args3 = ReferenceEquals(null, null, null);

                Tag("Args0", args0);
                Tag("Args1", args1);
                Tag("Args2", args2);
                Tag("Args3", args3);
            }

            private bool ReferenceEquals() => false;
            private bool ReferenceEquals(object a) => false;
            private bool ReferenceEquals(object a, object b) => false;
            private bool ReferenceEquals(object a, object b, object c) => false;
            """;
        var validator = SETestContext.CreateCSMethod(code).Validator;
        validator.TagValue("Args0").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Args1").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Args2").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Args3").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("true", "true", true)]
    [DataRow("true", "false", false)]
    [DataRow("false", "true", false)]
    [DataRow("false", "false", true)]
    [DataRow("(bool?)true", "(bool?)true", true)]
    [DataRow("(bool?)true", "(bool?)false", false)]
    [DataRow("(bool?)true", "true", true)]
    [DataRow("(false ? true : false)", "false", true)]
    public void Invocation_BoolEquals_LearnsResult(string left, string right, bool expectedResult)
    {
        var code = $"""
            var result = object.Equals({left}, {right});
            Tag("Result", result);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.From(expectedResult));
    }

    [DataTestMethod]
    [DataRow("true")]
    [DataRow("false")]
    public void Invocation_BoolEquals_OnlyOneArgumentHasBoolConstraint_DoesNotLearn(string leftArgument)
    {
        var code = $"""
            bool left = {leftArgument};
            var result = object.Equals(left, right);
            Tag("Result", result);
            """;
        var validator = SETestContext.CreateCS(code, "bool right").Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    public void Invocation_BoolEquals_BothArgumentsHaveNoConstraint_DoesNotLearn()
    {
        var code = $"""
            var result = object.Equals(left, right);
            Tag("Result", result);
            """;
        var validator = SETestContext.CreateCS(code, "bool left, bool right").Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("42", "42", true)]
    [DataRow("0", "42", false)]
    [DataRow("((int?)42)", "((int?)42)", true)]
    [DataRow("((int?)42)", "((int?)0)", false)]
    public void Invocation_NumberEquals_SingleValues_LearnsResult(string left, string right, bool expected)
    {
        var code = $"""
            var result = {left}.Equals({right});
            Tag("Result", result);

            var resultStaticCall = object.Equals({left}, {right});
            Tag("ResultStaticCall", resultStaticCall);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.From(expected));
        validator.TagValue("ResultStaticCall").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.From(expected));
    }

    [TestMethod]
    public void Invocation_NumberEquals_ValueInRange_DoesNotLearn()
    {
        var code = """
            if (i > 1 && i < 10)
            {
                var result = object.Equals(i, 4);
                Tag("Result", result);
            }
            """;
        var validator = SETestContext.CreateCS(code, "int i").Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void Invocation_NumberEquals_RangesDoNotOverlap_LearnsResult()
    {
        var code = $$"""
            if (i < 0 && j > 0)
            {
                var result = i.Equals(j);
                Tag("Result", result);
            }
            """;
        var validator = SETestContext.CreateCS(code, "int i, int j").Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False);
    }

    [TestMethod]
    public void Invocation_StructEquals_Default_LearnsFromNull()
    {
        const string code = """
            public struct Sample
            {
                public void Method()
                {
                    var result = Equals(null);
                    Tag("Result", result);
                }

                private static void Tag(string name, object arg) { }
            }
            """;
        var validator = new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False);  // Calling underlaying ValueTuple.Equals recognizes null
    }

    [TestMethod]
    public void Invocation_StructEquals_Overriden_DoesNotLearn()
    {
        const string code = """
            private struct InnerStruct  // Nested inside outer Sample, that provides Tag(..)
            {
                public void Method()
                {
                    var fromOverride = Equals(null);
                    var fromOverload = Equals((string)null);
                    Tag("FromOverride", fromOverride);
                    Tag("FromOverload", fromOverload);
                }

                public bool Equals(string obj) => true;
                public override bool Equals(object obj) => true;
            }
            """;
        var validator = SETestContext.CreateCSMethod(code).Validator;
        validator.TagValue("FromOverride").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
        validator.TagValue("FromOverload").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("null", 0)]
    [DataRow("42", 42)]
    public void Invocation_NullableGetHasValue_LearnsBoolConstraint(string value, int expected)
    {
        var code = $"""
            int? value = {value};
            value ??= 0;    // Uses InvocationOperation int?.HasValue.get()
            Tag("Value", value);
            """;
        SETestContext.CreateCS(code).Validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
    }

    [TestMethod]
    public void Invocation_NullableGetHasValue_Unknown()
    {
        const string code = """
            arg ??= 0;      // Uses InvocationOperation int?.HasValue.get()
            Tag("Arg", arg);
            """;
        SETestContext.CreateCS(code, "int? arg").Validator.TagValues("Arg").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull),
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(0)));
    }

    [TestMethod]
    public void Invocation_NullableGetHasValue_UntrackedSymbol()
    {
        const string code = """
            var result = arg.NullableProperty ??= 0;      // Uses InvocationOperation int?.HasValue.get()
            Tag("Property", arg.NullableProperty);
            Tag("Result", result);
            """;
        var validator = SETestContext.CreateCS(code, "Sample arg").Validator;
        validator.TagValues("Property").Should().AllSatisfy(x => x.Should().HaveNoConstraints());
        validator.TagValues("Result").Should().SatisfyRespectively(
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull),  // Because it's int
            x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(0)));
    }

    [TestMethod]
    public void Invocation_EnumerableCount_SetsNumberConstraint()
    {
        const string code = """
            var x = list.Count();
            Tag("Before", x);

            list.Clear();
            x = list.Count();
            Tag("Clear", x);

            list.Add(42);
            x = list.Count();
            Tag("Add", x);

            x = list.Count(_ => true);
            Tag("Predicate", x);
            """;
        var validator = SETestContext.CreateCS(code, "List<int> list").Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);

        validator.TagValue("Before").Should().HaveOnlyConstraints(NumberConstraint.From(0, null), ObjectConstraint.NotNull);
        validator.TagValue("Clear").Should().HaveOnlyConstraints(NumberConstraint.From(0), ObjectConstraint.NotNull);
        validator.TagValue("Add").Should().HaveOnlyConstraints(NumberConstraint.From(1, null), ObjectConstraint.NotNull);
        validator.TagValue("Predicate").Should().HaveOnlyConstraints(NumberConstraint.From(0, null), ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void Invocation_CollectionMethods_SetCollectionConstraint()
    {
        const string code = """
            var tag = "before";

            list.Clear();
            tag = "clear";

            list.Add(42);
            tag = "add";

            list.RemoveAt(0);
            tag = "remove";

            void Invoke(Action<int> action) { }
            """;
        var validator = SETestContext.CreateCS(code, "List<int> list", new PreserveTestCheck("list")).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);

        validator.TagValue("before", "list").Should().HaveNoConstraints();
        Verify("clear", ObjectConstraint.NotNull, CollectionConstraint.Empty);
        Verify("add", ObjectConstraint.NotNull, CollectionConstraint.NotEmpty);
        Verify("remove", ObjectConstraint.NotNull);

        void Verify(string state, params SymbolicConstraint[] constraints) =>
            validator.TagValue(state, "list").Should().HaveOnlyConstraints(constraints);
    }

    [TestMethod]
    public void Invocation_CollectionArgument_RemovesCollectionConstraint()
    {
        const string code = """
            var tag = "before";

            list.Add(42);
            tag = "add";

            Use(list);
            tag = "argument";

            void Use(List<int> arg) { }
            """;
        var validator = SETestContext.CreateCS(code, "List<int> list", new PreserveTestCheck("list")).Validator;
        validator.ValidateContainsOperation(OperationKind.Argument);

        validator.TagValue("before", "list").Should().HaveNoConstraints();
        validator.TagValue("add", "list").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, CollectionConstraint.NotEmpty);
        validator.TagValue("argument", "list").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void MethodReference_RemovesCollectionConstraint()
    {
        const string code = """
                var tag = "before";

                list.Clear();
                tag = "clear";

                Invoke(list.Add);           // remove CollectionConstraint, we don't know what Invoke does
                tag = "methodReference1";

                list.Add(42);
                tag = "add";

                Action<int> action = list.RemoveAt;
                tag = "methodReference2";

                void Invoke(Action<int> action) { }
            """;
        var validator = SETestContext.CreateCS(code, "List<int> list", new PreserveTestCheck("list")).Validator;
        validator.ValidateContainsOperation(OperationKindEx.MethodReference);

        validator.TagValue("before", "list").Should().HaveNoConstraints();
        Verify("clear", ObjectConstraint.NotNull, CollectionConstraint.Empty);
        Verify("methodReference1", ObjectConstraint.NotNull);
        Verify("add", ObjectConstraint.NotNull, CollectionConstraint.NotEmpty);
        Verify("methodReference2", ObjectConstraint.NotNull);

        void Verify(string state, params SymbolicConstraint[] constraints) =>
            validator.TagValue(state, "list").Should().HaveOnlyConstraints(constraints);
    }

    private static SymbolicValue[] DebugAssertValues(string expression, string argType = "object")
    {
        var code = $"""
            Debug.Assert({expression});
            Tag("Arg", arg);
            """;
        return SETestContext.CreateCS(code, $"{argType} arg, bool condition").Validator.TagValues("Arg");
    }
}

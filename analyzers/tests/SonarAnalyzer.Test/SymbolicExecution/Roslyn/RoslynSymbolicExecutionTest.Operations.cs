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

using Microsoft.CodeAnalysis.Operations;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.Test.TestFramework.SymbolicExecution;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

public partial class RoslynSymbolicExecutionTest
{
    [TestMethod]
    public void SimpleAssignment_ToLocalVariable_FromLiteral()
    {
        var validator = SETestContext.CreateCS(@"var a = true; Tag(""a"", a);", new LiteralDummyTestCheck()).Validator;
        validator.Validate("Literal: true", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue("it's scaffolded"));
        validator.Validate("SimpleAssignment: a = true (Implicit)", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
        validator.Validate("SimpleAssignment: a = true (Implicit)", x => x.State[((ISimpleAssignmentOperation)x.Operation.Instance).Target].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
        validator.TagValue("a").Should().HaveOnlyConstraints(DummyConstraint.Dummy, ObjectConstraint.NotNull, BoolConstraint.True);
    }

    [TestMethod]
    public void SimpleAssignment_ToLocalVariable_FromTrackedSymbol()
    {
        var validator = SETestContext.CreateCS(@"bool a = true, b; b = a; Tag(""b"", b);", new LiteralDummyTestCheck()).Validator;
        validator.Validate("Literal: true", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue("it's scaffolded"));
        validator.Validate("SimpleAssignment: b = a", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
        validator.Validate("SimpleAssignment: b = a", x => x.State[((ISimpleAssignmentOperation)x.Operation.Instance).Target].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
        validator.TagValue("b").Should().HaveOnlyConstraints(DummyConstraint.Dummy, ObjectConstraint.NotNull, BoolConstraint.True);
    }

    [TestMethod]
    public void SimpleAssignment_ToLocalVariable_FromLocalConst()
    {
        var validator = SETestContext.CreateCS(@"const string a = null; var b = a; Tag(""b"", b);").Validator;
        validator.TagValue("b").Should().HaveOnlyConstraint(ObjectConstraint.Null);
    }

    [TestMethod]
    public void SimpleAssignment_ToLocalVariable_FromClassConst()
    {
        const string code = @"
private const string isNull = null;
private const string isNotNull = ""some text"";
public void Method()
{
    var value = isNull;
    Tag(""IsNull"", value);
    value = isNotNull;
    Tag(""IsNotNull"", value);
}";
        var validator = SETestContext.CreateCSMethod(code).Validator;
        validator.TagValue("IsNull").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagValue("IsNotNull").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void SimpleAssignment_ToLocalVariable_FromTrackedSymbol_Chained()
    {
        var validator = SETestContext.CreateCS(@"bool a = true, b, c; c = b = a; Tag(""c"", c);", new LiteralDummyTestCheck()).Validator;
        validator.Validate("Literal: true", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue("it's scaffolded"));
        validator.Validate("SimpleAssignment: c = b = a", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
        validator.Validate("SimpleAssignment: c = b = a", x => x.State[((ISimpleAssignmentOperation)x.Operation.Instance).Target].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
        validator.TagValue("c").Should().HaveOnlyConstraints(DummyConstraint.Dummy, ObjectConstraint.NotNull, BoolConstraint.True);
    }

    [TestMethod]
    public void SimpleAssignment_ToParameter_FromLiteral()
    {
        var validator = SETestContext.CreateCS(@"boolParameter = true; Tag(""boolParameter"", boolParameter);", new LiteralDummyTestCheck()).Validator;
        validator.Validate("Literal: true", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue("it's scaffolded"));
        validator.Validate("SimpleAssignment: boolParameter = true", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
        validator.Validate("SimpleAssignment: boolParameter = true", x => x.State[((ISimpleAssignmentOperation)x.Operation.Instance).Target].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
        validator.TagValue("boolParameter").Should().HaveOnlyConstraints(DummyConstraint.Dummy, ObjectConstraint.NotNull, BoolConstraint.True);
    }

    [TestMethod]
    public void SimpleAssignment_ToLocalVariable_FromTrackedSymbol_CS()
    {
        var setter = new PreProcessTestCheck(OperationKind.ParameterReference, x => x.SetOperationConstraint(DummyConstraint.Dummy));
        var validator = SETestContext.CreateCS(@"var b = boolParameter; Tag(""b"", b);", setter).Validator;
        validator.TagValue("b").Should().HaveOnlyConstraints(DummyConstraint.Dummy, ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void SimpleAssignment_ToLocalVariable_FromTrackedSymbol_VB()
    {
        var setter = new PreProcessTestCheck(OperationKind.ParameterReference, x => x.SetOperationConstraint(DummyConstraint.Dummy));
        var validator = SETestContext.CreateVB(@"Dim B As Boolean = BoolParameter : Tag(""B"", B)", setter).Validator;
        validator.TagValue("B").Should().HaveOnlyConstraints(DummyConstraint.Dummy, ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow(@"Sample.StaticField = 42; Tag(""Target"", Sample.StaticField);", "SimpleAssignment: Sample.StaticField = 42")]
    [DataRow(@"StaticField = 42; Tag(""Target"", StaticField);", "SimpleAssignment: StaticField = 42")]
    [DataRow(@"field = 42; Tag(""Target"", field);", "SimpleAssignment: field = 42")]
    [DataRow(@"this.field = 42; Tag(""Target"", this.field);", "SimpleAssignment: this.field = 42")]
    [DataRow(@"field = 42; var a = field; Tag(""Target"", field);", "SimpleAssignment: a = field (Implicit)")]
    public void SimpleAssignment_Fields(string snippet, string operation)
    {
        var validator = SETestContext.CreateCS(snippet, new LiteralDummyTestCheck()).Validator;
        validator.Validate("Literal: 42", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue("it's scaffolded"));
        validator.Validate(operation, x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
        validator.TagValue("Target").Should().HaveOnlyConstraints(DummyConstraint.Dummy, ObjectConstraint.NotNull, NumberConstraint.From(42));
    }

    [DataTestMethod]
    [DataRow("""this.AutoProperty = 42; Tag("Target", this.AutoProperty);""")]
    [DataRow("""AutoProperty = 42; Tag("Target", AutoProperty);""")]
    [DataRow("""Sample.StaticAutoProperty = 42; Tag("Target", Sample.StaticAutoProperty);""")]
    [DataRow("""StaticAutoProperty = 42; Tag("Target", StaticAutoProperty);""")]
    public void SimpleAssignment_FromLiteral(string snippet)
    {
        var validator = SETestContext.CreateCS(snippet, new LiteralDummyTestCheck()).Validator;
        validator.Validate("Literal: 42", x => x.State[x.Operation].Should().HaveOnlyConstraints(new SymbolicConstraint[] { ObjectConstraint.NotNull, NumberConstraint.From(42), DummyConstraint.Dummy }, "it's scaffolded"));
        validator.TagValue("Target").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(42), DummyConstraint.Dummy);
    }

    [DataTestMethod]
    [DataRow("""Sample.StaticFullProperty = 42; Tag("Target", Sample.StaticFullProperty);""")]
    [DataRow("""StaticFullProperty = 42; Tag("Target", StaticFullProperty);""")]
    [DataRow("""var dict = new Dictionary<string, int>(); dict["key"] = 42; Tag("Target", dict["key"]);""")]
    [DataRow("""var other = new Sample(); other.AutoProperty = 42; Tag("Target", other.AutoProperty);""")]
    [DataRow("""var other = new Sample(); other.FullProperty = 42; Tag("Target", other.FullProperty);""")]
    [DataRow("""this.FullProperty = 42; Tag("Target", this.FullProperty);""")]
    [DataRow("""FullProperty = 42; Tag("Target", FullProperty);""")]
    [DataRow("""var other = new Sample(); other.field = 42; Tag("Target", other.field);""")]
    public void SimpleAssignment_FromLiteral_ToUntracked(string snippet)
    {
        var validator = SETestContext.CreateCS(snippet, new LiteralDummyTestCheck()).Validator;
        validator.Validate("Literal: 42", x => x.State[x.Operation].Should().HaveOnlyConstraints(new SymbolicConstraint[] { ObjectConstraint.NotNull, NumberConstraint.From(42), DummyConstraint.Dummy }, "it's scaffolded"));
        validator.TagValue("Target").Should().HaveNoConstraints();
    }

    [TestMethod]
    public void SimpleAssignment_FromEnum()
    {
        const string code = """
            public enum E
            {
                None,
                Value = 42
            }
            public void Method()
            {
                var value = E.Value;
                Tag("Value", value);
            }
            """;
        var validator = SETestContext.CreateCSMethod(code).Validator;
        validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(42));
    }

    [TestMethod]
    public void ElementAccess_FromLiteral_ToUntracked()
    {
        var validator = SETestContext.CreateCS("""var arr = new object[] { 13 }; arr[0] = 42;""", new LiteralDummyTestCheck()).Validator;
        validator.Validate("Literal: 42", x => x.State[x.Operation].Should().HaveOnlyConstraints(new SymbolicConstraint[] { ObjectConstraint.NotNull, NumberConstraint.From(42), DummyConstraint.Dummy }, "it's scaffolded"));
        validator.Validate("ArrayElementReference: arr[0]", x => x.State[x.Operation].Should().HaveNoConstraints());
    }

    [TestMethod]
    public void Conversion_ToLocalVariable_FromTrackedSymbol_ExplicitCast()
    {
        var validator = SETestContext.CreateCS(@"int a = 42; byte b = (byte)a; var c = (byte)field; Tag(""b"", b); Tag(""c"", c);", new LiteralDummyTestCheck()).Validator;
        validator.ValidateOrder(
            "LocalReference: a = 42 (Implicit)",
            "Literal: 42",
            "SimpleAssignment: a = 42 (Implicit)",
            "LocalReference: b = (byte)a (Implicit)",
            "LocalReference: a",
            "Conversion: (byte)a",
            "SimpleAssignment: b = (byte)a (Implicit)",
            "LocalReference: c = (byte)field (Implicit)",
            "InstanceReference: field (Implicit)",
            "FieldReference: field",
            "Conversion: (byte)field",
            "SimpleAssignment: c = (byte)field (Implicit)",
            @"Literal: ""b""",
            @"Argument: ""b""",
            "LocalReference: b",
            "Conversion: b (Implicit)",
            "Argument: b",
            @"Invocation: Tag(""b"", b)",
            @"ExpressionStatement: Tag(""b"", b);",
            @"Literal: ""c""",
            @"Argument: ""c""",
            "LocalReference: c",
            "Conversion: c (Implicit)",
            "Argument: c",
            @"Invocation: Tag(""c"", c)",
            @"ExpressionStatement: Tag(""c"", c);");
        validator.TagValue("b").Should().HaveOnlyConstraints(DummyConstraint.Dummy, ObjectConstraint.NotNull, NumberConstraint.From(42));
        validator.TagValue("c").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void Conversion_ToLocalVariable_FromLiteral_ImplicitCast()
    {
        var validator = SETestContext.CreateCS(@"byte b = 42; Tag(""b"", b);", new LiteralDummyTestCheck()).Validator;
        validator.ValidateOrder(
            "LocalReference: b = 42 (Implicit)",
            "Literal: 42",
            "Conversion: 42 (Implicit)",
            "SimpleAssignment: b = 42 (Implicit)",
            @"Literal: ""b""",
            @"Argument: ""b""",
            "LocalReference: b",
            "Conversion: b (Implicit)",
            "Argument: b",
            @"Invocation: Tag(""b"", b)",
            @"ExpressionStatement: Tag(""b"", b);");
        validator.TagValue("b").Should().HaveOnlyConstraints(DummyConstraint.Dummy, ObjectConstraint.NotNull, NumberConstraint.From(42));
    }

    [DataTestMethod]
    [DataRow("Int16")]
    [DataRow("Int32")]
    [DataRow("Int64")]
    [DataRow("decimal")]
    [DataRow("float")]
    [DataRow("double")]
    public void Conversion_BuiltInConversion_PropagateState_CS(string type)
    {
        var code = $"""
            byte b = 42;
            {type} value = b;
            Tag("Value", value);
            """;
        SETestContext.CreateCS(code, new LiteralDummyTestCheck()).Validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, DummyConstraint.Dummy, NumberConstraint.From(42));
    }

    [DataTestMethod]
    [DataRow("Int16")]
    [DataRow("Int32")]
    [DataRow("Int64")]
    [DataRow("Decimal")]
    [DataRow("Single")]
    [DataRow("Double")]
    [DataRow("IntPtr")]
    [DataRow("UIntPtr")]
    public void Conversion_BuiltInConversion_PropagateState_VB(string type)
    {
        var code = $"""
            Dim B As Byte = 42
            Dim Value As {type} = B
            Tag("Value", Value)
            """;
        SETestContext.CreateVB(code, new LiteralDummyTestCheck()).Validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, DummyConstraint.Dummy, NumberConstraint.From(42));
    }

    [TestMethod]
    public void Conversion_CustomOperators_DoNotPropagateState()
    {
        const string code = """
            public void Main()
            {
                var isTrue = true;
                WithImplicit withImplicit = isTrue;
                WithExplicit withExplicit = (WithExplicit)isTrue;
                Tag("WithImplicit", withImplicit);
                Tag("WithExplicit", withExplicit);
            }

            public struct WithImplicit
            {
                public static implicit operator WithImplicit(bool b) => new();
            }
            public struct WithExplicit
            {
                public static explicit operator WithExplicit(bool b) => new();
            }
            """;
        var validator = SETestContext.CreateCSMethod(code, new LiteralDummyTestCheck()).Validator;
        validator.TagValue("WithImplicit").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("WithExplicit").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

#if NET

    [TestMethod]
    public void Conversion_CustomOperators_DoNotPropagateState_Half()
    {
        const string code = """
            byte b = 42;
            Half h = b;
            Tag("Half", h);
            """;
        var validator = SETestContext.CreateCS(code, new LiteralDummyTestCheck()).Validator;
        validator.TagValue("Half").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);    // While it would be better to propagate constraints here, Half has custom conversion operators
    }

#endif

    [TestMethod]
    public void Conversion_ToLocalNonNullableValueType_CS()
    {
        var validator = SETestContext.CreateCS("""
                object o = null; // Set ObjectConstraint.Null and Dummy (LiteralDummyTestCheck)
                int convert = System.Convert.ToInt32(o);
                int explicitCast = (int)o;
                object implicitBoxing = explicitCast;
                object asBoxing = explicitCast as object;
                object unboxingBoxing = (object)(int)o;

                Tag("Object", o);
                Tag("Convert", convert);
                Tag("Explicit", explicitCast);
                Tag("ImplicitBoxing", implicitBoxing);
                Tag("AsBoxing", asBoxing);
                Tag("UnboxingBoxing", unboxingBoxing);
                """, new LiteralDummyTestCheck()).Validator;
        validator.TagValue("Object").Should().HaveOnlyConstraints(ObjectConstraint.Null, DummyConstraint.Dummy);
        validator.TagValue("Convert").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
        validator.TagValue("Explicit").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, DummyConstraint.Dummy);
        validator.TagValue("ImplicitBoxing").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, DummyConstraint.Dummy);
        validator.TagValue("AsBoxing").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, DummyConstraint.Dummy);
        validator.TagValue("UnboxingBoxing").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, DummyConstraint.Dummy);
    }

    [TestMethod]
    public void Conversion_ToLocalNonNullableValueType_VB()
    {
        var validator = SETestContext.CreateVB("""
                Dim o As Object = Nothing ' Set ObjectConstraint.Null and Dummy (LiteralDummyTestCheck)
                Dim iCType As Integer = CType(o, Integer)
                Dim iCInt = CInt(o)
                Dim iDirectCast As Integer = DirectCast(o, Integer)
                Dim iImplicit As Integer = o
                Dim iTryCast As Integer = TryCast(o, Object) ' Two conversions: Implicit and TryCast
                Dim iTryCastBoxing As Object = TryCast(iTryCast, Object)

                Tag("Object", o)
                Tag("CType", iCType)
                Tag("CInt", iCInt)
                Tag("DirectCast", iDirectCast)
                Tag("Implicit", iImplicit)
                Tag("TryCast", iTryCast)
                Tag("TryCastBoxing", iTryCastBoxing)
                """, new LiteralDummyTestCheck()).Validator;
        validator.TagValue("Object").Should().HaveOnlyConstraints(ObjectConstraint.Null, DummyConstraint.Dummy);
        validator.TagValue("CType").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, DummyConstraint.Dummy);
        validator.TagValue("CInt").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, DummyConstraint.Dummy);
        validator.TagValue("DirectCast").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, DummyConstraint.Dummy);
        validator.TagValue("Implicit").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, DummyConstraint.Dummy);
        validator.TagValue("TryCast").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, DummyConstraint.Dummy);
        validator.TagValue("TryCastBoxing").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, DummyConstraint.Dummy);
    }

    [TestMethod]
    public void Conversion_TryDownCast_DoesNotBranch()
    {
        var validator = SETestContext.CreateCS("""
            var conversion = arg as Exception;
            Tag("Arg", arg);
            Tag("Conversion", conversion);
            """, "object arg").Validator;
        validator.TagValue("Arg").Should().BeNull();
        validator.TagValue("Conversion").Should().BeNull();
    }

    [TestMethod]
    public void Conversion_TryUpCast_DoesNotBranch()
    {
        var validator = SETestContext.CreateCS("""
            var conversion = arg as Exception;
            Tag("Arg", arg);
            Tag("Conversion", conversion);
            """, "ArgumentException arg").Validator;
        validator.TagValue("Arg").Should().BeNull();
        validator.TagValue("Conversion").Should().BeNull();
    }

    [TestMethod]
    public void Argument_Ref_ResetsConstraints_CS() =>
        SETestContext.CreateCS(@"var b = true; Main(boolParameter, ref b); Tag(""B"", b);", "ref bool outParam").Validator.TagValue("B").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);

    [TestMethod]
    public void Argument_Out_ResetsConstraints_CS() =>
        SETestContext.CreateCS(@"var b = true; Main(boolParameter, out b); Tag(""B"", b); outParam = false;", "out bool outParam").Validator.TagValue("B").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);

    [TestMethod]
    public void Argument_ByRef_ResetConstraints_VB() =>
        SETestContext.CreateVB(@"Dim B As Boolean = True : Main(BoolParameter, B) : Tag(""B"", B)", "ByRef ByRefParam As Boolean").Validator.TagValue("B").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);

    [TestMethod]
    public void Argument_PropagatesState_Existing()
    {
        var wasChecked = false;
        var checker = new PostProcessTestCheck(OperationKind.Argument, x =>
            {
                x.State[x.Operation].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True);
                wasChecked = true;
                return x.State;
            });
        SETestContext.CreateCS("var value = true; InstanceMethod(value);", checker);
        wasChecked.Should().BeTrue();
    }

    [TestMethod]
    public void Argument_PropagatesState_Null()
    {
        var wasChecked = false;
        var checker = new PostProcessTestCheck(OperationKind.Argument, x =>
            {
                x.State[x.Operation].Should().HaveNoConstraints();
                wasChecked = true;
                return x.State;
            });
        SETestContext.CreateCS("InstanceMethod(Tagger.Unknown<object>());", checker);
        wasChecked.Should().BeTrue();
    }

    [TestMethod]
    public void Argument_ArgList_DoesNotThrow()
    {
        const string code = @"
public void ArgListMethod(__arglist)
{
    ArgListMethod(__arglist(""""));
}";
        SETestContext.CreateCSMethod(code).Validator.ValidateExitReachCount(1);
    }

    [TestMethod]
    public void FlowCapture_SetsCapture()
    {
        var assertions = 0;
        var collector = new PostProcessTestCheck(x =>
        {
            if (x.Operation.Instance.Kind == OperationKind.FlowCaptureReference)
            {
                var capture = IFlowCaptureReferenceOperationWrapper.FromOperation(x.Operation.Instance);
                x.State.ResolveCapture(capture.WrappedOperation).Kind.Should().Be(OperationKind.LocalReference);
                assertions++;
            }
            return x.State;
        });
        SETestContext.CreateCS("string a = null; a ??= arg;", "string arg", collector);
        assertions.Should().Be(3);  // Block #3 transitive capture, Block #3 BranchValue, Block #4
    }

    [TestMethod]
    public void AnonymousObjectCreation_SetsNotNull()
    {
        const string code = @"
var anonymous = new { a = 42 };
Tag(""Anonymous"", anonymous);";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKind.AnonymousObjectCreation);
        validator.TagValue("Anonymous").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void ArrayCreation_SetsNotNull()
    {
        const string code = """
            string tag;
            var arr1 = new int[] { 42 };
            var arr2 = new int[0];
            int[] arr3 = { };

            int[,] arrMulti1 = new int[2, 3];
            int[,] arrMulti2 = new int[0, 3];
            int[,] arrMulti3 = new int[2, 0];

            int[][] arrJagged1 = new int[2][];
            int[][] arrJagged2 = new int[0][];

            tag = "tag";
            """;

        var preserved = new string[] { "arr1", "arr2", "arr3", "arrMulti1", "arrMulti2", "arrMulti3", "arrJagged1", "arrJagged2" };
        var validator = SETestContext.CreateCS(code, new PreserveTestCheck(preserved)).Validator;
        validator.ValidateContainsOperation(OperationKind.ArrayCreation);
        Verify("arr1", CollectionConstraint.NotEmpty);
        Verify("arr2", CollectionConstraint.Empty);
        Verify("arr3", CollectionConstraint.Empty);
        Verify("arrMulti1", CollectionConstraint.NotEmpty);
        Verify("arrMulti2", CollectionConstraint.Empty);
        Verify("arrMulti3", CollectionConstraint.Empty);
        Verify("arrJagged1", CollectionConstraint.NotEmpty);
        Verify("arrJagged2", CollectionConstraint.Empty);

        void Verify(string symbol, CollectionConstraint collectionConstraint) =>
            validator.TagValue("tag", symbol).Should().HaveOnlyConstraints(ObjectConstraint.NotNull, collectionConstraint);
    }

    [TestMethod]
    public void DelegateCreation_SetsNotNull()
    {
        const string code = @"
var pointer = Main; // Delegate creation to encapsulating method
var lambda = () => { };
var del = delegate() {};
Tag(""Pointer"", pointer);
Tag(""Lambda"", lambda);
Tag(""Delegate"", del);";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKind.DelegateCreation);
        validator.TagValue("Pointer").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Lambda").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Delegate").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void DynamicObjectCreation_SetsNotNull()
    {
        const string code = @"
var s = new Sample(dynamicArg);
Tag(""S"", s);";
        var validator = SETestContext.CreateCS(code, "dynamic dynamicArg").Validator;
        validator.ValidateContainsOperation(OperationKind.DynamicObjectCreation);
        validator.TagValue("S").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void ObjectCreation_SetsNotNull()
    {
        const string code = """
            string tag;

            object assigned;
            var obj = new Object();
            var valueType = new Guid();
            var declared = new Exception();
            assigned = new EventArgs();

            var collection1 = new List<int>();
            collection1.Add(42);
            var collection2 = new List<int>(collection1);
            var collection3 = new List<int>(list);

            tag = "tag";
            """;

        var preserved = new string[] { "obj", "valueType", "declared", "assigned", "collection1", "collection2", "collection3" };
        var validator = SETestContext.CreateCS(code, "List<int> list", new PreserveTestCheck(preserved)).Validator;
        validator.ValidateContainsOperation(OperationKind.ObjectCreation);

        Verify("declared", ObjectConstraint.NotNull);
        Verify("assigned", ObjectConstraint.NotNull);
        Verify("valueType", ObjectConstraint.NotNull); // This is questionable, value types should not have ObjectConstraint
        Verify("obj", ObjectConstraint.NotNull);
        Verify("collection1", ObjectConstraint.NotNull); // The CollectionConstraint here is deleted when collection1 is used as an argument
        Verify("collection2", ObjectConstraint.NotNull, CollectionConstraint.NotEmpty);
        Verify("collection3", ObjectConstraint.NotNull);

        void Verify(string symbol, params SymbolicConstraint[] constraints) =>
            validator.TagValue("tag", symbol).Should().HaveOnlyConstraints(constraints);
    }

    [TestMethod]
    public void TypeParameterObjectCreation_SetsNotNull()
    {
        const string code = @"
public void Main<T>() where T : new()
{
    var value = new T();
    Tag(""Value"", value);
}";
        var validator = SETestContext.CreateCSMethod(code).Validator;
        validator.ValidateContainsOperation(OperationKind.TypeParameterObjectCreation);
        validator.TagValue("Value").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

#if NET

    [TestMethod]
    public void Index_SetsNotNull()
    {
        const string code = """
            var index = ^0;
            Tag("Index", index);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKind.Unary);
        validator.TagValue("Index").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void Range_SetsNotNull()
    {
        const string code = """
            var range = 0..1;
            Tag("Range", range);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKind.Range);
        validator.TagValue("Range").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

#endif

    [TestMethod]
    public void Literal_NullAndDefault_SetsNull_CS()
    {
        const string code = @"
Tag(""BeforeObjNull"", argObjNull);
Tag(""BeforeObjDefault"", argObjDefault);
Tag(""BeforeInt"", argInt);
Tag(""BeforeNullableInt"", argNullableInt);
argObjNull = null;
argObjDefault = default;
argInt = default;
argNullableInt = default;
Tag(""AfterObjNull"", argObjNull);
Tag(""AfterObjDefault"", argObjDefault);
Tag(""AfterInt"", argInt);
Tag(""AfterNullableInt"", argNullableInt);";
        var validator = SETestContext.CreateCS(code, "object argObjNull, object argObjDefault, int argInt, int? argNullableInt").Validator;
        validator.ValidateContainsOperation(OperationKind.Literal);
        validator.TagValue("BeforeObjNull").Should().HaveNoConstraints();
        validator.TagValue("BeforeObjDefault").Should().HaveNoConstraints();
        validator.TagValue("BeforeInt").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("BeforeNullableInt").Should().HaveNoConstraints();
        validator.TagValue("AfterObjNull").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagValue("AfterObjDefault").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagValue("AfterInt").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(0));
        validator.TagValue("AfterNullableInt").Should().HaveOnlyConstraint(ObjectConstraint.Null);
    }

    [TestMethod]
    public void Literal_Null_SetsNull_VB()
    {
        const string code = @"
Tag(""BeforeObj"", ArgObj)
Tag(""BeforeInt"", ArgInt)
ArgObj = Nothing
ArgInt = Nothing
Tag(""AfterObj"", ArgObj)
Tag(""AfterInt"", ArgInt)";
        var validator = SETestContext.CreateVB(code, "ArgObj As Object, ArgInt As Integer").Validator;
        validator.ValidateContainsOperation(OperationKind.Literal);
        validator.TagValue("BeforeObj").Should().HaveNoConstraints();
        validator.TagValue("BeforeInt").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("AfterObj").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagValue("AfterInt").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(0));
    }

    [TestMethod]
    public void Literal_Default_ForGenericType()
    {
        const string code = """
                public void Main<TClass, TStruct, TUnknown, TType, TInterface, TUnmanaged, TEnum, TDelegate>
                    (TClass argClass, TStruct argStruct, TUnknown argUnknown, TType argType, TInterface argInterface, TUnmanaged argUnmanaged, TEnum argEnum, TDelegate argDelegate)
                    where TClass : class
                    where TStruct: struct
                    where TType : EventArgs
                    where TInterface : IDisposable
                    where TUnmanaged : unmanaged
                    where TEnum : Enum
                    where TDelegate : Delegate
                {
                    argClass = default;
                    argStruct = default;
                    argUnknown = default;
                    argType = default;
                    argInterface = default;
                    argUnmanaged = default;
                    argEnum = default;
                    argDelegate = default;
                    Tag("Class", argClass);
                    Tag("Struct", argStruct);
                    Tag("Unknown", argUnknown);
                    Tag("Type", argType);
                    Tag("Interface", argInterface);
                    Tag("Unmanaged", argUnmanaged);
                    Tag("Enum", argEnum);
                    Tag("Delegate", argDelegate);
                }
                """;
        var validator = SETestContext.CreateCSMethod(code).Validator;
        validator.ValidateContainsOperation(OperationKind.Literal);
        validator.TagValue("Class").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagValue("Struct").Should().HaveOnlyConstraint(ObjectConstraint.NotNull, "struct cannot be null.");
        validator.TagValue("Unknown").Should().HaveNoConstraints("it can be struct.");
        validator.TagValue("Type").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagValue("Interface").Should().HaveNoConstraints("interfaces can be implemented by a struct.");
        validator.TagValue("Unmanaged").Should().HaveOnlyConstraint(ObjectConstraint.NotNull, "unmanaged implies struct and cannot be null.");
        validator.TagValue("Enum").Should().HaveOnlyConstraint(ObjectConstraint.NotNull, "Enum cannot be null.");
        validator.TagValue("Delegate").Should().HaveOnlyConstraint(ObjectConstraint.Null);
    }

    [TestMethod]
    public void Literal_Default_ConversionsFromAnotherType()
    {
        const string code = @"
object o = default(Exception);
int i = default(byte);
Tag(""ObjectFromException"", o);
Tag(""IntegerFromByte"", i);";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("ObjectFromException").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagValue("IntegerFromByte").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(0));
    }

    [TestMethod]
    public void Literal_Default_StringLiteral()
    {
        const string code = @"
var stringLocal = ""someText"";
const string stringConst = ""someText"";
Tag(""StringLocal"", stringLocal);
Tag(""StringConst"", stringConst);";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("StringLocal").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("StringConst").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("true")]
    [DataRow("false")]
    public void Literal_Default_BoolLiterals(string literal)
    {
        var expected = bool.Parse(literal);
        var code = @$"
var value = {literal};
Tag(""Value"", value);";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.From(expected));
    }

    [DataTestMethod]
    [DataRow("'c'", null)]
    [DataRow("1", 1)]
    [DataRow("1u", 1u)]
    [DataRow("1l", 1L)]
    [DataRow("0xFF", 255)]
    [DataRow("1.0", 1.0)]
    [DataRow("1.0f", 1.0f)]
    public void Literal_Default_OtherLiterals(string literal, object expectedNumber)
    {
        var code = @$"
var value = {literal};
Tag(""Value"", value);";
        var validator = SETestContext.CreateCS(code).Validator;
        var expectedConstraints = expectedNumber is null
            ? new SymbolicConstraint[] { ObjectConstraint.NotNull }
            : new SymbolicConstraint[] { ObjectConstraint.NotNull, NumberConstraint.From(expectedNumber) };
        validator.TagValue("Value").Should().HaveOnlyConstraints(expectedConstraints);
    }

    [TestMethod]
    public void Literal_Default_InferredFromReturnType()
    {
        var validator = SETestContext.CreateCSMethod("public object Main() => default;").Validator;
        validator.ValidateContainsOperation(OperationKind.DefaultValue);    // And do not fail
    }

    [TestMethod]
    public void InstanceReference_SetsNotNull_CS()
    {
        const string code = @"
var fromThis = this;
var _ = field;
Tag(""This"", fromThis);";
        var implicitCheck = new PostProcessTestCheck(OperationKind.FieldReference, x =>
        {
            var reference = (IFieldReferenceOperation)x.Operation.Instance;
            reference.Instance.Kind.Should().Be(OperationKind.InstanceReference);
            reference.Instance.IsImplicit.Should().BeTrue();
            x.State[reference.Instance].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
            return x.State;
        });
        var validator = SETestContext.CreateCS(code, implicitCheck).Validator;
        validator.ValidateContainsOperation(OperationKind.InstanceReference);
        validator.ValidateContainsOperation(OperationKind.FieldReference);  // To execute implicitCheck
        validator.TagValue("This").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void SizeOf_SetNotNullAndNumberConstraint()
    {
        var code = """
                var size = sizeof(int);
                Tag("Size", size);
                """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Size").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(4));
    }

    [DataTestMethod]
    [DataRow("[ValidatedNotNull]")]
    [DataRow("[ValidatedNotNullAttribute]")]
    [DataRow("[NotNull]")]
    [DataRow("[NotNullAttribute]")]
    public void Invocation_NotNullAttribute_SetsNotNullOnArgumentsMarkedWithAttribute_CS(string notNullAttribute)
    {
        var code = $@"
using System;

public class Sample
{{
    public void Main(object o1, object o2, object o3, object o4, object o5, object o6, object o7, object o8, string s1, string s2)
    {{
        Guard.NotNullExt(o1);
        o2.NotNullExt();
        NotNullInst(o3);
        NotNullInst(o4, o5);
        NotNullInst(value2: o6, value1: o7, value3: o8);  // value2 is not annotated
        NotNullRefOut(ref s1, out s2);
        Tag(""AfterGuard_o1"", o1);
        Tag(""AfterGuard_o2"", o2);
        Tag(""AfterGuard_o3"", o3);
        Tag(""AfterGuard_o4"", o4);
        Tag(""AfterGuard_o5"", o5);
        Tag(""AfterGuard_o6"", o6);
        Tag(""AfterGuard_o7"", o7);
        Tag(""AfterGuard_o8"", o8);
        Tag(""AfterGuard_s1"", s1);
        Tag(""AfterGuard_s2"", s2);
    }}

    private void NotNullInst({notNullAttribute} object value)
    {{
        // Skip implementation to make sure, the attribute is driving the constraint
    }}

    private void NotNullInst({notNullAttribute} object value1, {notNullAttribute} object value2) {{ }}
    private void NotNullInst<T1, T2, T3>({notNullAttribute} T1 value1, T2 value2, {notNullAttribute} T3 value3) {{ }}
    private void NotNullRefOut<T1, T2>({notNullAttribute} ref T1 value1, {notNullAttribute} out T2 value2) {{ value1 = default; value2 = default; }}

    private static void Tag(string name, object arg) {{ }}
}}

public sealed class ValidatedNotNullAttribute : Attribute {{ }}

public sealed class NotNullAttribute : Attribute {{ }}

public static class Guard
{{
    public static void NotNullExt<T>({notNullAttribute} this T value) where T : class {{ }}
}}
";
        var validator = new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator;
        validator.TagValue("AfterGuard_o1").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("AfterGuard_o2").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("AfterGuard_o3").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("AfterGuard_o4").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("AfterGuard_o5").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("AfterGuard_o6").Should().BeNull("parameter is not annotated");
        validator.TagValue("AfterGuard_o7").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("AfterGuard_o8").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("AfterGuard_s1").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("AfterGuard_s2").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("<ValidatedNotNull>")]
    [DataRow("<ValidatedNotNullAttribute>")]
    [DataRow("<NotNull>")]
    [DataRow("<NotNullAttribute>")]
    public void Invocation_NotNullAttribute_SetsNotNullOnArgumentsMarkedWithAttribute_VB(string notNullAttribute)
    {
        var code = $"""
            Imports System.Runtime.CompilerServices

            Public Class Sample

                Public Sub Main(o1 As Object, o2 As Object, o3 As Object, o4 As Object, o5 As Object, o6 As Object, o7 As Object, o8 As Object, s1 As String, s2 As String, ex as Exception)
                    Dim SomeString As String = "Lorem"
                    Guard.NotNullExt(o1)
                    o2.NotNullExt()
                    NotNullInst(o3)
                    NotNullInst(o4, o5)
                    NotNullInst(value2:=o6, value1:=o7, value3:=o8)  ' value2 is not annotated
                    NotNullRef(s1)
                    SomeString.NotNullExtOtherArgument(s2)
                    ex.NotNullExt()
                    Tag("AfterGuard_o1", o1)
                    Tag("AfterGuard_o2", o2)
                    Tag("AfterGuard_o3", o3)
                    Tag("AfterGuard_o4", o4)
                    Tag("AfterGuard_o5", o5)
                    Tag("AfterGuard_o6", o6)
                    Tag("AfterGuard_o7", o7)
                    Tag("AfterGuard_o8", o8)
                    Tag("AfterGuard_s1", s1)
                    Tag("AfterGuard_s2", s2)
                    Tag("AfterGuard_ex", ex)
                End Sub

                Private Sub NotNullInst({notNullAttribute} value As Object)
                    ' Skip implementation to make sure, the attribute Is driving the constraint
                End Sub

                Private Sub NotNullInst({notNullAttribute} value1 As Object, {notNullAttribute} value2 As Object)
                End Sub

                Private Sub NotNullInst(Of T1, T2, T3)({notNullAttribute} value1 As T1, value2 As T2, {notNullAttribute} value3 As T3)
                End Sub

                Private Sub NotNullRef(Of T)({notNullAttribute} ByRef value As T)
                End Sub

                Private Shared Sub Tag(name As String, arg As Object)
                End Sub

            End Class

            Public NotInheritable Class ValidatedNotNullAttribute
                Inherits Attribute
            End Class

            Public NotInheritable Class NotNullAttribute
                Inherits Attribute
            End Class

            Public Module Guard

                <Extension>
                Public Sub NotNullExt(Of T As Class)({notNullAttribute} Value As T)
                End Sub

                <Extension>
                Public Sub NotNullExtOtherArgument(Of T As Class)(Value As String, {notNullAttribute} NotNull As T)
                End Sub

            End Module
            """;
        var validator = new SETestContext(code, AnalyzerLanguage.VisualBasic, Array.Empty<SymbolicCheck>()).Validator;
        validator.TagValue("AfterGuard_o1").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("AfterGuard_o2").Should().HaveNoConstraints("extension method invoked on Object type is DynamicInvocationOperation that we don't support.");
        validator.TagValue("AfterGuard_o3").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("AfterGuard_o4").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("AfterGuard_o5").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("AfterGuard_o6").Should().HaveNoConstraints("parameter is not annotated");
        validator.TagValue("AfterGuard_o7").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("AfterGuard_o8").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("AfterGuard_s1").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("AfterGuard_s2").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("AfterGuard_ex").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void Invocation_Read_SetsNotNull()
    {
        const string code = @"
_ = StaticField;            // Do not fail, do nothing
_ = Sample.StaticField;
_ = field;
_ = UntrackedSymbol().field;
Tag(""Before"", arg);
_ = arg.field;
Tag(""After"", arg);

Sample UntrackedSymbol() => this;";
        var validator = SETestContext.CreateCS(code, "Sample arg").Validator;
        validator.ValidateContainsOperation(OperationKind.FieldReference);
        validator.TagValue("Before").Should().BeNull();
        validator.TagValue("After").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void FieldReference_Write_SetsNotNull()
    {
        const string code = @"
StaticField = 42;            // Do not fail, do nothing
Sample.StaticField = 42;
field = 42;
UntrackedSymbol().field = 42;
Tag(""Before"", arg);
arg.field = 42;
Tag(""After"", arg);

Sample UntrackedSymbol() => this;";
        var validator = SETestContext.CreateCS(code, "Sample arg").Validator;
        validator.ValidateContainsOperation(OperationKind.FieldReference);
        validator.TagValue("Before").Should().BeNull();
        validator.TagValue("After").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void FieldReference_DoesNotMixInstances()
    {
        const string code = @"
this.fieldException = new NotImplementedException();
var argException = arg.fieldException;  // Should not propagate constraint from this.fieldException
Tag(""This"", fieldException);
Tag(""Arg"", argException);";
        var validator = SETestContext.CreateCS(code, "Sample arg").Validator;
        validator.TagValue("This").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Arg").Should().BeNull();
    }

    [TestMethod]
    public void PropertyReference_Read_SetsNotNull()
    {
        const string code = @"
_ = StaticProperty;            // Do not fail, do nothing
_ = Sample.StaticProperty;
_ = Property;
_ = UntrackedSymbol().Property;
Tag(""BeforeProperty"", arg);
Tag(""BeforeDictionary"", dictionary);
Tag(""BeforeIndexer"", indexer);
_ = arg.Property;
_ = dictionary[42];
_ = indexer[42];
Tag(""AfterProperty"", arg);
Tag(""AfterDictionary"", dictionary);
Tag(""AfterIndexer"", indexer);

Sample UntrackedSymbol() => this;";
        var validator = SETestContext.CreateCS(code, "Sample arg, Dictionary<int, int> dictionary, Sample indexer").Validator;
        validator.ValidateContainsOperation(OperationKind.PropertyReference);
        validator.TagValue("BeforeProperty").Should().BeNull();
        validator.TagValue("BeforeDictionary").Should().BeNull();
        validator.TagValue("BeforeIndexer").Should().BeNull();
        validator.TagValue("AfterProperty").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("AfterDictionary").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("AfterIndexer").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void PropertyReference_Write_SetsNotNull()
    {
        const string code = @"
StaticProperty = 42;            // Do not fail, do nothing
Sample.StaticProperty = 42;
Property = 42;
UntrackedSymbol().Property = 42;
Tag(""BeforeProperty"", arg);
Tag(""BeforeDictionary"", dictionary);
Tag(""BeforeIndexer"", indexer);
arg.Property = 42;
dictionary[42] = 42;
indexer[42] = 42;
Tag(""AfterProperty"", arg);
Tag(""AfterDictionary"", dictionary);
Tag(""AfterIndexer"", indexer);

Sample UntrackedSymbol() => this;";
        var validator = SETestContext.CreateCS(code, "Sample arg, Dictionary<int, int> dictionary, Sample indexer").Validator;
        validator.ValidateContainsOperation(OperationKind.PropertyReference);
        validator.TagValue("BeforeProperty").Should().BeNull();
        validator.TagValue("BeforeDictionary").Should().BeNull();
        validator.TagValue("BeforeIndexer").Should().BeNull();
        validator.TagValue("AfterProperty").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("AfterDictionary").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("AfterIndexer").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void PropertyReference_AutoProperty_IsTracked()
    {
        var code = """
            AutoProperty = null;
            var x = AutoProperty;
            Tag("AfterSetNull", AutoProperty);
            Tag("AfterSetNull_Operation", x);
            AutoProperty.ToString();
            x = AutoProperty;
            Tag("AfterReadReference", AutoProperty);
            Tag("AfterReadReference_Operation", x);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKind.PropertyReference);
        validator.TagValue("AfterSetNull").Should().HaveOnlyConstraints(ObjectConstraint.Null);
        validator.TagValue("AfterSetNull_Operation").Should().HaveOnlyConstraints(ObjectConstraint.Null);
        validator.TagValue("AfterReadReference").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
        validator.TagValue("AfterReadReference_Operation").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void PropertyReference_Count_HasNumericConstraint()
    {
        const string code = $"""
                var x = collection.Count;
                Tag("Before", x);

                collection.Add(42);
                x = collection.Count;
                Tag("AfterAdd", x);

                collection.Clear();
                x = collection.Count;
                Tag("AfterClear", x);
                """;

        var validator = SETestContext.CreateCS(code, $"List<int> collection").Validator;
        validator.ValidateContainsOperation(OperationKind.PropertyReference);
        validator.TagValue("Before").Should().HaveOnlyConstraints(NumberConstraint.From(0, null), ObjectConstraint.NotNull);
        validator.TagValue("AfterAdd").Should().HaveOnlyConstraints(NumberConstraint.From(1, null), ObjectConstraint.NotNull);
        validator.TagValue("AfterClear").Should().HaveOnlyConstraints(NumberConstraint.From(0), ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void PropertyReference_Indexer_SetsCollectionConstraint()
    {
        const string code = """
                var tag = "before";
                _ = list[42];
                tag = "after";
                """;

        var validator = SETestContext.CreateCS(code, "List<int> list").Validator;
        validator.ValidateContainsOperation(OperationKind.PropertyReference);
        validator.TagValue("before", "list").Should().HaveNoConstraints();
        validator.TagValue("after", "list").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, CollectionConstraint.NotEmpty);
    }

    [TestMethod]
    public void PropertyReference_FullProperties_NotTracked()
    {
        const string code = """
                FullProperty = null;
                Tag("AfterSetNull", FullProperty);
                FullProperty.ToString();
                Tag("AfterReadReference", FullProperty);
                """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKind.PropertyReference);
        validator.TagValue("AfterSetNull").Should().HaveNoConstraints();
        validator.TagValue("AfterReadReference").Should().HaveNoConstraints();
    }

    [TestMethod]
    public void ArrayElementReference_Read_SetsNotNull()
    {
        const string code = @"
_ = UntrackedSymbol()[42]; // Do not fail
Tag(""Before"", array);
_ = array[42];
Tag(""After"", array);

int[] UntrackedSymbol() => new[] { 42 };";
        var validator = SETestContext.CreateCS(code, "int[] array").Validator;
        validator.ValidateContainsOperation(OperationKind.ArrayElementReference);
        validator.TagValue("Before").Should().BeNull();
        validator.TagValue("After").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void ArrayElementReference_Write_SetsNotNull()
    {
        const string code = @"
UntrackedSymbol()[42] = 42; // Do not fail
Tag(""Before"", array);
array[42] = 42;
Tag(""After"", array);

int[] UntrackedSymbol() => new[] { 42 };";
        var validator = SETestContext.CreateCS(code, "int[] array").Validator;
        validator.ValidateContainsOperation(OperationKind.ArrayElementReference);
        validator.TagValue("Before").Should().BeNull();
        validator.TagValue("After").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void EventReference_SetsNotNull()
    {
        const string code = @"
Tag(""BeforeAdd"", add);
Tag(""BeforeRemove"", remove);
add.Event += (sender, e) => { };
remove.Event -= (sender, e) => { };
Tag(""AfterAdd"", add);
Tag(""AfterRemove"", remove);";
        var validator = SETestContext.CreateCS(code, "Sample add, Sample remove").Validator;
        validator.ValidateContainsOperation(OperationKind.ArrayElementReference);
        validator.TagValue("BeforeAdd").Should().BeNull();
        validator.TagValue("BeforeRemove").Should().BeNull();
        validator.TagValue("AfterAdd").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("AfterRemove").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void ReDim_SetsNotNull()
    {
        const string code = """
            Dim tag as String

            Dim First(), Second(), Third(4242) As Object
            Dim Fourth As Object()
            tag = "before"

            ReDim First(42), Second(1042), Third(4444), Fourth(4), Arg.FieldArray(42)
            tag = "after"
            """;

        var preserved = new string[] { "First", "Second", "Third", "Fourth" };
        var validator = SETestContext.CreateVB(code, "Arg As Sample", new PreserveTestCheck(preserved)).Validator;
        validator.ValidateContainsOperation(OperationKind.ReDim);

        VerifyBefore("First", ObjectConstraint.Null);
        VerifyBefore("Second", ObjectConstraint.Null);
        VerifyBefore("Third", ObjectConstraint.NotNull, CollectionConstraint.NotEmpty);
        VerifyBefore("Fourth", ObjectConstraint.Null);

        VerifyAfter("First", ObjectConstraint.NotNull);
        VerifyAfter("Second", ObjectConstraint.NotNull);
        VerifyAfter("Third", ObjectConstraint.NotNull, CollectionConstraint.NotEmpty);
        VerifyAfter("Fourth", ObjectConstraint.NotNull);

        void VerifyBefore(string symbol, params SymbolicConstraint[] constraints) =>
            validator.TagValue("before", symbol).Should().HaveOnlyConstraints(constraints);

        void VerifyAfter(string symbol, params SymbolicConstraint[] constraints) =>
            validator.TagValue("after", symbol).Should().HaveOnlyConstraints(constraints);
    }

    [TestMethod]
    public void ReDimPreserve_SetsNotNull()
    {
        const string code = """
            Dim tag as String
            Dim First(), Second(10) As Object
            tag = "before"
            ReDim Preserve First(42), Second(42)
            tag = "after"
            """;

        var preserved = new string[] { "First", "Second" };
        var validator = SETestContext.CreateVB(code, "Arg As Sample", new PreserveTestCheck(preserved)).Validator;
        validator.ValidateContainsOperation(OperationKind.ReDim);

        VerifyBefore("First", ObjectConstraint.Null);
        VerifyBefore("Second", ObjectConstraint.NotNull, CollectionConstraint.NotEmpty);

        VerifyAfter("First", ObjectConstraint.NotNull);
        VerifyAfter("Second", ObjectConstraint.NotNull, CollectionConstraint.NotEmpty);

        void VerifyBefore(string symbol, params SymbolicConstraint[] constraints) =>
            validator.TagValue("before", symbol).Should().HaveOnlyConstraints(constraints);

        void VerifyAfter(string symbol, params SymbolicConstraint[] constraints) =>
            validator.TagValue("after", symbol).Should().HaveOnlyConstraints(constraints);
    }

    [TestMethod]
    public void Await_ForgetsFieldStates()
    {
        const string code = @"
private object field;

public async System.Threading.Tasks.Task Main(System.Threading.Tasks.Task T)
{
    field = null;
    Tag(""Before"", field);
    await T;
    Tag(""After"", field);
}";
        var addConstraint = new PostProcessTestCheck(OperationKind.Literal, x => x.SetOperationConstraint(LockConstraint.Held));    // Persisted constraint
        var validator = SETestContext.CreateCSMethod(code, addConstraint).Validator;
        validator.TagValue("Before").Should().HaveOnlyConstraints(ObjectConstraint.Null, LockConstraint.Held);
        validator.TagValue("After").Should().HaveOnlyConstraint(LockConstraint.Held, "this constraint should be preserved on fields");
    }

    [TestMethod]
    public void TypeOf_SetsNotNull()
    {
        var validator = SETestContext.CreateCS(@"var value = typeof(object); Tag(""Value"", value);").Validator;
        validator.ValidateContainsOperation(OperationKind.TypeOf);
        validator.TagValue("Value").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("bool", "true", ConstraintKind.NotNull, ConstraintKind.False)]
    [DataRow("bool", "false", ConstraintKind.NotNull, ConstraintKind.True)]
    [DataRow("bool?", "default", ConstraintKind.Null)]
    [DataRow("bool?", "null", ConstraintKind.Null)]
    public void Unary_Not_SupportsBoolAndNull(string type, string defaultValue, params ConstraintKind[] expectedConstraints)
    {
        var code = $@"
{type} value = {defaultValue};
value = !value;
Tag(""Value"", value);";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKind.Unary);
        validator.TagValue("Value").AllConstraints.Select(y => y.Kind).Should().BeEquivalentTo(expectedConstraints);
    }

    [TestMethod]
    public void ParameterReference_NonNullableValue_DoesNotPropagateState()
    {
        const string code = """
                public class WithValue
                {
                    public object Value {get;}
                };

                public void Main()
                {
                    var instance = new WithValue();
                    var value = instance.Value;     // Same name as Nullable.Value
                    Tag("Value", value);
                }
                """;
        SETestContext.CreateCSMethod(code).Validator.TagValue("Value").Should().BeNull();
    }
}

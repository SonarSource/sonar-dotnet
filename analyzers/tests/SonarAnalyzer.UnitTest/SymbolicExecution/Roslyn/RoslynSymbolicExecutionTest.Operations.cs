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

using Microsoft.CodeAnalysis.Operations;
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
        public void SimpleAssignment_ToLocalVariable_FromLiteral()
        {
            var validator = SETestContext.CreateCS(@"var a = true; Tag(""a"", a);", new LiteralDummyTestCheck()).Validator;
            validator.Validate("Literal: true", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue("it's scaffolded"));
            validator.Validate("SimpleAssignment: a = true (Implicit)", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
            validator.Validate("SimpleAssignment: a = true (Implicit)", x => x.State[((ISimpleAssignmentOperation)x.Operation.Instance).Target].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
            validator.ValidateTag("a", x => x.HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
        }

        [TestMethod]
        public void SimpleAssignment_ToLocalVariable_FromTrackedSymbol()
        {
            var validator = SETestContext.CreateCS(@"bool a = true, b; b = a; Tag(""b"", b);", new LiteralDummyTestCheck()).Validator;
            validator.Validate("Literal: true", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue("it's scaffolded"));
            validator.Validate("SimpleAssignment: b = a", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
            validator.Validate("SimpleAssignment: b = a", x => x.State[((ISimpleAssignmentOperation)x.Operation.Instance).Target].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
            validator.ValidateTag("b", x => x.HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
        }

        [TestMethod]
        public void SimpleAssignment_ToLocalVariable_FromLocalConst()
        {
            var validator = SETestContext.CreateCS(@"const string a = null; var b = a; Tag(""b"", b);").Validator;
            validator.ValidateTag("b", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
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
            validator.ValidateTag("IsNull", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.ValidateTag("IsNotNull", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
        }

        [TestMethod]
        public void SimpleAssignment_ToLocalVariable_FromTrackedSymbol_Chained()
        {
            var validator = SETestContext.CreateCS(@"bool a = true, b, c; c = b = a; Tag(""c"", c);", new LiteralDummyTestCheck()).Validator;
            validator.Validate("Literal: true", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue("it's scaffolded"));
            validator.Validate("SimpleAssignment: c = b = a", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
            validator.Validate("SimpleAssignment: c = b = a", x => x.State[((ISimpleAssignmentOperation)x.Operation.Instance).Target].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
            validator.ValidateTag("c", x => x.HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
        }

        [TestMethod]
        public void SimpleAssignment_ToParameter_FromLiteral()
        {
            var validator = SETestContext.CreateCS(@"boolParameter = true; Tag(""boolParameter"", boolParameter);", new LiteralDummyTestCheck()).Validator;
            validator.Validate("Literal: true", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue("it's scaffolded"));
            validator.Validate("SimpleAssignment: boolParameter = true", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
            validator.Validate("SimpleAssignment: boolParameter = true", x => x.State[((ISimpleAssignmentOperation)x.Operation.Instance).Target].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
            validator.ValidateTag("boolParameter", x => x.HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
        }

        [TestMethod]
        public void SimpleAssignment_ToLocalVariable_FromTrackedSymbol_CS()
        {
            var setter = new PreProcessTestCheck(OperationKind.ParameterReference, x => x.SetOperationConstraint(DummyConstraint.Dummy));
            var validator = SETestContext.CreateCS(@"var b = boolParameter; Tag(""b"", b);", setter).Validator;
            validator.ValidateTag("b", x => x.HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
        }

        [TestMethod]
        public void SimpleAssignment_ToLocalVariable_FromTrackedSymbol_VB()
        {
            var setter = new PreProcessTestCheck(OperationKind.ParameterReference, x => x.SetOperationConstraint(DummyConstraint.Dummy));
            var validator = SETestContext.CreateVB(@"Dim B As Boolean = BoolParameter : Tag(""B"", B)", setter).Validator;
            validator.ValidateTag("B", x => x.HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
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
            validator.ValidateTag("Target", x => x.HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
        }

        [DataTestMethod]
        [DataRow(@"Sample.StaticProperty = 42; Tag(""Target"", Sample.StaticProperty);")]
        [DataRow(@"StaticProperty = 42; Tag(""Target"", StaticProperty);")]
        [DataRow(@"var arr = new byte[] { 13 }; arr[0] = 42; Tag(""Target"", arr[0]);")]
        [DataRow(@"var dict = new Dictionary<string, int>(); dict[""key""] = 42; Tag(""Target"", dict[""key""]);")]
        [DataRow(@"var other = new Sample(); other.Property = 42; Tag(""Target"", other.Property);")]
        [DataRow(@"this.Property = 42; Tag(""Target"", this.Property);")]
        [DataRow(@"Property = 42; Tag(""Target"", Property);")]
        [DataRow(@"var other = new Sample(); other.field = 42; Tag(""Target"", other.field);")]
        public void SimpleAssignment_ToUnsupported_FromLiteral(string snippet)
        {
            var validator = SETestContext.CreateCS(snippet, new LiteralDummyTestCheck()).Validator;
            validator.Validate("Literal: 42", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue("it's scaffolded"));
            validator.ValidateTag("Target", x => (x?.HasConstraint(DummyConstraint.Dummy) ?? false).Should().BeFalse());
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
                "InstanceReference: Tag (Implicit)",
                @"Literal: ""b""",
                @"Argument: ""b""",
                "LocalReference: b",
                "Conversion: b (Implicit)",
                "Argument: b",
                @"Invocation: Tag(""b"", b)",
                @"ExpressionStatement: Tag(""b"", b);",
                "InstanceReference: Tag (Implicit)",
                @"Literal: ""c""",
                @"Argument: ""c""",
                "LocalReference: c",
                "Conversion: c (Implicit)",
                "Argument: c",
                @"Invocation: Tag(""c"", c)",
                @"ExpressionStatement: Tag(""c"", c);");
            validator.ValidateTag("b", x => x.HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
            validator.ValidateTag("c", x => x.Should().BeNull());
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
                "InstanceReference: Tag (Implicit)",
                @"Literal: ""b""",
                @"Argument: ""b""",
                "LocalReference: b",
                "Conversion: b (Implicit)",
                "Argument: b",
                @"Invocation: Tag(""b"", b)",
                @"ExpressionStatement: Tag(""b"", b);");
            validator.ValidateTag("b", x => x.HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
        }

        [TestMethod]
        public void Argument_Ref_ResetsConstraints_CS() =>
            SETestContext.CreateCS(@"var b = true; Main(boolParameter, ref b); Tag(""B"", b);", ", ref bool outParam").Validator.ValidateTag("B", x => x.Should().BeNull());

        [TestMethod]
        public void Argument_Out_ResetsConstraints_CS() =>
            SETestContext.CreateCS(@"var b = true; Main(boolParameter, out b); Tag(""B"", b); outParam = false;", ", out bool outParam").Validator.ValidateTag("B", x => x.Should().BeNull());

        [TestMethod]
        public void Argument_ByRef_ResetConstraints_VB() =>
            SETestContext.CreateVB(@"Dim B As Boolean = True : Main(BoolParameter, B) : Tag(""B"", B)", ", ByRef ByRefParam As Boolean").Validator.ValidateTag("B", x => x.Should().BeNull());

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
            SETestContext.CreateCS("string a = null; a ??= arg;", ", string arg", collector);
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
            validator.ValidateTag("Anonymous", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
        }

        [TestMethod]
        public void ArrayCreation_SetsNotNull()
        {
            const string code = @"
var arr1 = new int[] { 42 };
var arr2 = new int[0];
int[] arr3 = { };
int[,] arrMulti = new int[2, 3];
int[][] arrJagged = new int[2][];

Tag(""Arr1"", arr1);
Tag(""Arr2"", arr2);
Tag(""Arr3"", arr3);
Tag(""ArrMulti"", arrMulti);
Tag(""ArrJagged"", arrJagged);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateContainsOperation(OperationKind.ArrayCreation);
            validator.ValidateTag("Arr1", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("Arr2", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("Arr3", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("ArrMulti", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("ArrJagged", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
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
            validator.ValidateTag("Pointer", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("Lambda", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("Delegate", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
        }

        [TestMethod]
        public void DynamicObjectCreation_SetsNotNull()
        {
            const string code = @"
var s = new Sample(dynamicArg);
Tag(""S"", s);";
            var validator = SETestContext.CreateCS(code, ", dynamic dynamicArg").Validator;
            validator.ValidateContainsOperation(OperationKind.DynamicObjectCreation);
            validator.ValidateTag("S", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
        }

        [TestMethod]
        public void ObjectCreation_SetsNotNull()
        {
            const string code = @"
object assigned;
var obj = new Object();
var valueType = new Guid();
var declared = new Exception();
assigned = new EventArgs();

Tag(""Declared"", declared);
Tag(""Assigned"", assigned);
Tag(""ValueType"", valueType);
Tag(""Object"", obj);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateContainsOperation(OperationKind.ObjectCreation);
            validator.ValidateTag("Declared", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("Assigned", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("ValueType", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeFalse());
            validator.ValidateTag("Object", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
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
            validator.ValidateTag("Value", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
        }

        [TestMethod]
        public void Literal_NullAndDefault_SetsNull_CS()
        {
            const string code = @"
Tag(""BeforeObjNull"", argObjNull);
Tag(""BeforeObjDefault"", argObjDefault);
Tag(""BeforeInt"", argInt);
argObjNull = null;
argObjDefault = default;
argInt = default;
Tag(""AfterObjNull"", argObjNull);
Tag(""AfterObjDefault"", argObjDefault);
Tag(""AfterInt"", argInt);";
            var validator = SETestContext.CreateCS(code, ", object argObjNull, object argObjDefault, int argInt").Validator;
            validator.ValidateContainsOperation(OperationKind.Literal);
            validator.ValidateTag("BeforeObjNull", x => x.Should().BeNull());
            validator.ValidateTag("BeforeObjDefault", x => x.Should().BeNull());
            validator.ValidateTag("BeforeInt", x => x.Should().BeNull());
            validator.ValidateTag("AfterObjNull", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.ValidateTag("AfterObjDefault", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.ValidateTag("AfterInt", x => x.Should().BeNull());
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
            var validator = SETestContext.CreateVB(code, ", ArgObj As Object, ArgInt As Integer").Validator;
            validator.ValidateContainsOperation(OperationKind.Literal);
            validator.ValidateTag("BeforeObj", x => x.Should().BeNull());
            validator.ValidateTag("BeforeInt", x => x.Should().BeNull());
            validator.ValidateTag("AfterObj", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.ValidateTag("AfterInt", x => x.HasConstraint(ObjectConstraint.Null).Should().BeFalse());
        }

        [TestMethod]
        public void Literal_Default_ForGenericType()
        {
            const string code = @"
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
    Tag(""Class"", argClass);
    Tag(""Struct"", argStruct);
    Tag(""Unknown"", argUnknown);
    Tag(""Type"", argType);
    Tag(""Interface"", argInterface);
    Tag(""Unmanaged"", argUnmanaged);
    Tag(""Enum"", argEnum);
    Tag(""Delegate"", argDelegate);
}";
            var validator = SETestContext.CreateCSMethod(code).Validator;
            validator.ValidateContainsOperation(OperationKind.Literal);
            validator.ValidateTag("Class", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.ValidateTag("Struct", x => x.Should().BeNull("struct cannot be null."));
            validator.ValidateTag("Unknown", x => x.Should().BeNull("it can be struct."));
            validator.ValidateTag("Type", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.ValidateTag("Interface", x => x.Should().BeNull("interfaces can be implemented by a struct."));
            validator.ValidateTag("Unmanaged", x => x.Should().BeNull("unmanaged implies struct and cannot be null."));
            validator.ValidateTag("Enum", x => x.Should().BeNull("Enum cannot be null."));
            validator.ValidateTag("Delegate", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
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
            validator.ValidateTag("ObjectFromException", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.ValidateTag("IntegerFromByte", x => x.Should().BeNull());
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
            validator.ValidateTag("StringLocal", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("StringConst", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
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
            validator.ValidateTag("Value", x => x.HasConstraint(BoolConstraint.From(expected)));
        }

        [DataTestMethod]
        [DataRow("'c'")]
        [DataRow("1")]
        [DataRow("1u")]
        [DataRow("1l")]
        [DataRow("0xFF")]
        [DataRow("1.0")]
        [DataRow("1.0f")]
        public void Literal_Default_OtherLiterals(string literal)
        {
            var code = @$"
var value = {literal};
Tag(""Value"", value);";
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateTag("Value", x => x.Should().BeNull());
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
            validator.ValidateTag("This", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
        }

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
    private void Tag(string name, object arg) { }
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

    Private Sub Tag(Name As String, Arg As Object)
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

        [TestMethod]
        public void FieldReference_Read_SetsNotNull()
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
            var validator = SETestContext.CreateCS(code, ", Sample arg").Validator;
            validator.ValidateContainsOperation(OperationKind.FieldReference);
            validator.ValidateTag("Before", x => x.Should().BeNull());
            validator.ValidateTag("After", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
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
            var validator = SETestContext.CreateCS(code, ", Sample arg").Validator;
            validator.ValidateContainsOperation(OperationKind.FieldReference);
            validator.ValidateTag("Before", x => x.Should().BeNull());
            validator.ValidateTag("After", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
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
            var validator = SETestContext.CreateCS(code, ", Sample arg, Dictionary<int, int> dictionary, Sample indexer").Validator;
            validator.ValidateContainsOperation(OperationKind.PropertyReference);
            validator.ValidateTag("BeforeProperty", x => x.Should().BeNull());
            validator.ValidateTag("BeforeDictionary", x => x.Should().BeNull());
            validator.ValidateTag("BeforeIndexer", x => x.Should().BeNull());
            validator.ValidateTag("AfterProperty", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("AfterDictionary", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("AfterIndexer", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
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
            var validator = SETestContext.CreateCS(code, ", Sample arg, Dictionary<int, int> dictionary, Sample indexer").Validator;
            validator.ValidateContainsOperation(OperationKind.PropertyReference);
            validator.ValidateTag("BeforeProperty", x => x.Should().BeNull());
            validator.ValidateTag("BeforeDictionary", x => x.Should().BeNull());
            validator.ValidateTag("BeforeIndexer", x => x.Should().BeNull());
            validator.ValidateTag("AfterProperty", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("AfterDictionary", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("AfterIndexer", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
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
            var validator = SETestContext.CreateCS(code, ", int[] array").Validator;
            validator.ValidateContainsOperation(OperationKind.ArrayElementReference);
            validator.ValidateTag("Before", x => x.Should().BeNull());
            validator.ValidateTag("After", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
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
            var validator = SETestContext.CreateCS(code, ", int[] array").Validator;
            validator.ValidateContainsOperation(OperationKind.ArrayElementReference);
            validator.ValidateTag("Before", x => x.Should().BeNull());
            validator.ValidateTag("After", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
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
            var validator = SETestContext.CreateCS(code, ", Sample add, Sample remove").Validator;
            validator.ValidateContainsOperation(OperationKind.ArrayElementReference);
            validator.ValidateTag("BeforeAdd", x => x.Should().BeNull());
            validator.ValidateTag("BeforeRemove", x => x.Should().BeNull());
            validator.ValidateTag("AfterAdd", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("AfterRemove", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
        }
    }
}

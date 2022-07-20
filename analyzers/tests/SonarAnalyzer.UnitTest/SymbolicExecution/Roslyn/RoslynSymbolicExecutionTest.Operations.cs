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
using SonarAnalyzer.SymbolicExecution.Constraints;
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
        public void Binary_BoolOperands_Equals_CS()
        {
            const string code = @"
var isTrue = true;
var isFalse = false;

if (isTrue == true)
    Tag(""True"");
else
    Tag(""True Unreachable"");

if (isFalse == false)
    Tag(""False"");
else
    Tag(""False Unreachable"");

if (isTrue == isFalse)
    Tag(""Variables Unreachable"");
else
    Tag(""Variables"");";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder("True", "False", "Variables");
        }

        [TestMethod]
        public void Binary_BoolOperands_Equals_VB()
        {
            const string code = @"
Dim IsTrue = True
Dim IsFalse = False

If IsTrue = True Then
    Tag(""True"")
Else
    Tag(""True Unreachable"")
End If

If IsFalse = False Then
    Tag(""False"")
Else
    Tag(""False Unreachable"")
End If

If IsTrue = IsFalse Then
    Tag(""Variables Unreachable"")
Else
    Tag(""Variables"")
End If";
            SETestContext.CreateVB(code).Validator.ValidateTagOrder("True", "False", "Variables");
        }

        [TestMethod]
        public void Binary_BoolOperands_NotEquals_CS()
        {
            const string code = @"
var isTrue = true;
var isFalse = false;

if (isTrue != true)
    Tag(""True Unreachable"");
else
    Tag(""True"");

if (isFalse != false)
    Tag(""False Unreachable"");
else
    Tag(""False"");

if (isTrue != isFalse)
    Tag(""Variables"");
else
    Tag(""Variables Unreachable"");";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder("True", "False", "Variables");
        }

        [TestMethod]
        public void Binary_BoolOperands_NotEquals_VB()
        {
            const string code = @"
Dim IsTrue = True
Dim IsFalse = False

If IsTrue <> True Then
    Tag(""True Unreachable"")
Else
    Tag(""True"")
End If

If IsFalse <> False Then
    Tag(""False Unreachable"")
Else
    Tag(""False"")
End If

If IsTrue <> IsFalse Then
    Tag(""Variables"")
Else
    Tag(""Variables Unreachable"")
End If";
            SETestContext.CreateVB(code).Validator.ValidateTagOrder(
                "True",
                "False",
                "Variables");
        }

        [TestMethod]
        public void Binary_BoolOperands_And()
        {
            const string code = @"
var isTrue = true;
var isFalse = false;

if (isTrue & true)
    Tag(""True & True"");
else
    Tag(""True & True Unreachable"");

if (false & isTrue)
    Tag(""False & True Unreachable"");
else
    Tag(""False & True"");

if (false & isFalse)
    Tag(""False & False Unreachable"");
else
    Tag(""False & False"");

if (isTrue && true)
    Tag(""True && True"");
else
    Tag(""True && True Unreachable"");

if (isFalse && true)
    Tag(""False && True Unreachable"");
else
    Tag(""False && True"");";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder("True & True", "False & True", "False & False", "True && True", "False && True");
        }

        [TestMethod]
        public void Binary_BoolOperands_Or()
        {
            const string code = @"
var isTrue = true;
var isFalse = false;

if (isTrue | true)
    Tag(""True | True"");
else
    Tag(""True | True Unreachable"");

if (false | isTrue)
    Tag(""False | True"");
else
    Tag(""False | True Unreachable"");

if (false | isFalse)
    Tag(""False | False Unreachable"");
else
    Tag(""False | False"");

if (isTrue || true)
    Tag(""True || True"");
else
    Tag(""True || True Unreachable"");

if (isFalse || true)
    Tag(""False || True"");
else
    Tag(""False || True Unreachable"");";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder("True | True", "False | True", "False | False", "True || True", "False || True");
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
            SETestContext.CreateCS(code, ", int a, int b", check).Validator.ValidateTagOrder(
                "If",
                "Else",
                "End");
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
            validator.ValidateTag("ValueType", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
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
}

private void Tag(string name, object arg) { }";
            var validator = SETestContext.CreateCSMethod(code).Validator;
            validator.ValidateContainsOperation(OperationKind.TypeParameterObjectCreation);
            validator.ValidateTag("Value", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
        }
    }
}

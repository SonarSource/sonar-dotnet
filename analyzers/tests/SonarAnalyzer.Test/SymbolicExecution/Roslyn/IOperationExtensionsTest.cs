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

using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using StyleCop.Analyzers.Lightup;
using Operations = Microsoft.CodeAnalysis.Operations;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

[TestClass]
public class IOperationExtensionsTest
{
    [TestMethod]
    public void TrackedSymbol_LocalReference_IsVariableSymbol()
    {
        var localReference = ((Operations.ISimpleAssignmentOperation)TestHelper.CompileCfgBodyCS("var a = true;").Blocks[1].Operations[0]).Target;
        var symbol = localReference.ToLocalReference().Local;
        localReference.TrackedSymbol(ProgramState.Empty).Should().Be(symbol);
    }

    [TestMethod]
    public void TrackedSymbol_ParameterReference_IsParameterSymbol()
    {
        var expressionStatement = (Operations.IExpressionStatementOperation)TestHelper.CompileCfgBodyCS("parameter = true;", "bool parameter").Blocks[1].Operations[0];
        var parameterReference = ((Operations.ISimpleAssignmentOperation)expressionStatement.Operation).Target;
        var symbol = IParameterReferenceOperationWrapper.FromOperation(parameterReference).Parameter;
        parameterReference.TrackedSymbol(ProgramState.Empty).Should().Be(symbol);
    }

    [DataTestMethod]
    [DataRow(@"field = 1")]
    [DataRow(@"this.field = 1")]
    [DataRow(@"StaticField = 1")]
    [DataRow(@"C.StaticField = 1")]
    public void TrackedSymbol_FieldReference_IsFieldSymbol(string assignment)
    {
        var code = $"public class C {{ int field; static int StaticField; void Method() {{ {assignment}; }} }}";
        var graph = TestHelper.CompileCfgCS(code);
        var expressionStatement = (Operations.IExpressionStatementOperation)graph.Blocks[1].Operations[0];
        var assignmentTarget = ((Operations.ISimpleAssignmentOperation)expressionStatement.Operation).Target;
        var fieldReferenceSymbol = IFieldReferenceOperationWrapper.FromOperation(assignmentTarget).Field;
        assignmentTarget.TrackedSymbol(ProgramState.Empty).Should().Be(fieldReferenceSymbol);
    }

    [TestMethod]
    public void TrackedSymbol_FieldReference_Enum()
    {
        var code = """
            public enum E
            {
                None
            }

            public class Sample
            {
                public void Method()
                {
                    var none = E.None;
                }
            }
            """;
        var cfg = TestHelper.CompileCfgCS(code);
        var assignment = (Operations.ISimpleAssignmentOperation)cfg.Blocks[1].Operations.Single();
        var enumReference = (Operations.IFieldReferenceOperation)assignment.Value;
        enumReference.TrackedSymbol(ProgramState.Empty).Should().BeNull();
    }

    [DataTestMethod]
    [DataRow("Full", false)]
    [DataRow("Auto", true)]
    [DataRow("StaticFull", false)]
    [DataRow("StaticAuto", true)]
    [DataRow("this.Full", false)]
    [DataRow("this.Auto", true)]
    [DataRow("Test.StaticFull", false)]
    [DataRow("Test.StaticAuto", true)]
    [DataRow("(new TestImpl()).Full", false)]
    [DataRow("(new TestImpl()).Auto", false)]
    [DataRow("OverridenAuto", true)]
    [DataRow("ShadowedAuto", true)]
    [DataRow("OverridenFull", false)]
    [DataRow("ShadowedFull", false)]
    [DataRow("AbstractAuto", false)]
    [DataRow("VirtualAuto", false)]
    public void TrackedSymbol_AutoProperty_IsPropertySymbol(string property, bool tracked)
    {
        var code = $$"""
            class BaseClass
                {
                    public virtual object OverridenAuto { get => null; set => _ = value; }
                    public object ShadowedAuto { get => null; set => _ = value; }
                    public virtual object OverridenFull { get; set; }
                    public object ShadowedFull { get; set; }
                }

                abstract class Test : BaseClass
                {
                    object Full { get => null; set => _ = value; }
                    object Auto { get; set; }
                    static object StaticFull { get => null; set => _ = value; }
                    static object StaticAuto { get; set; }
                    public override object OverridenAuto { get; set; }
                    public new object ShadowedAuto { get; set; }
                    public override object OverridenFull { get => null; set => _ = value; }
                    public new object ShadowedFull { get => null; set => _ = value; }
                    public abstract object AbstractAuto { get; set; }
                    public virtual object VirtualAuto { get; set; }

                    public void Method()
                    {
                        {{property}} = null;
                    }
                }

                class TestImpl : Test
                {
                    public override object AbstractAuto { get; set; }
                }
            """;
        var graph = TestHelper.CompileCfgCS(code);
        var expressionStatement = (Operations.IExpressionStatementOperation)graph.Blocks[1].Operations[0];
        var assignmentTarget = ((Operations.ISimpleAssignmentOperation)expressionStatement.Operation).Target;
        var propertyReferenceSymbol = IPropertyReferenceOperationWrapper.FromOperation(assignmentTarget).Property;
        assignmentTarget.TrackedSymbol(ProgramState.Empty).Should().Be(tracked ? propertyReferenceSymbol : null);
    }

    [DataTestMethod]
    [DataRow(@"(int i, int j) = (1, 1)")]
    [DataRow(@"(var i, var j) = (1, 1)")]
    [DataRow(@"int.TryParse(string.Empty, out int value)")]
    [DataRow(@"int.TryParse(string.Empty, out var value)")]
    public void TrackedSymbol_DeclarationExpression(string assignment)
    {
        var code = $"public class C {{ void Method() {{ {assignment}; }} }}";
        var graph = TestHelper.CompileCfgCS(code);
        var allDeclarations = graph.Blocks[1].Operations
            .SelectMany(x => x.DescendantsAndSelf()).Where(x => x.Kind == OperationKindEx.DeclarationExpression).Select(IDeclarationExpressionOperationWrapper.FromOperation).ToArray();
        allDeclarations.Should().NotBeEmpty();
        allDeclarations.Should().AllSatisfy(x =>
            x.WrappedOperation.TrackedSymbol(ProgramState.Empty).Should().NotBeNull().And.BeAssignableTo<ISymbol>()
            .Which.GetSymbolType().Should().BeAssignableTo<ITypeSymbol>()
            .Which.SpecialType.Should().Be(SpecialType.System_Int32));
    }

    [TestMethod]
    public void TrackedSymbol_CaptureReference()
    {
        var cfg = TestHelper.CompileCfgBodyCS("a ??= b;", "object a, object b");
        var capture = IFlowCaptureOperationWrapper.FromOperation(cfg.Blocks[1].Operations[0]);
        var captureReference = cfg.Blocks[3].Operations[0].ChildOperations.First();
        var state = ProgramState.Empty.SetCapture(capture.Id, capture.Value);
        captureReference.TrackedSymbol(state).Should().BeAssignableTo<ISymbol>().Which.Name.Should().Be("a");
    }

    [TestMethod]
    public void TrackedSymbol_CaptureReferenceInConversion()
    {
        var cfg = TestHelper.CompileCfgBodyCS("_ = (string)(condition ? a : null);", "object a, bool condition");
        var capture = IFlowCaptureOperationWrapper.FromOperation(cfg.Blocks[2].Operations[0]);
        var conversion = cfg.Blocks[4].Operations[0].ChildOperations.First().ChildOperations.Skip(1).First();
        var state = ProgramState.Empty.SetCapture(capture.Id, capture.Value);
        conversion.TrackedSymbol(state).Should().BeAssignableTo<ISymbol>().Which.Name.Should().Be("a");
    }

    [TestMethod]
    public void TrackedSymbol_UnknownCaptureReference_ReturnsNull()
    {
        var cfg = TestHelper.CompileCfgBodyCS("a ??= b;", "object a, object b");
        var captureReference = cfg.Blocks[3].Operations[0].ChildOperations.First();
        var state = ProgramState.Empty;
        captureReference.TrackedSymbol(state).Should().BeNull();
    }

    [TestMethod]
    public void TrackedSymbol_NullSafe() =>
        SonarAnalyzer.SymbolicExecution.Roslyn.IOperationExtensions.TrackedSymbol(null, ProgramState.Empty).Should().BeNull();

    [TestMethod]
    public void TrackedSymbol_DeclarationExpression_Tuple()
    {
        var code = $"public class C {{ void Method() {{ var (i, j) = (1, 1); }} }}";
        var graph = TestHelper.CompileCfgCS(code);
        var allDeclarations = graph.Blocks[1].Operations
            .SelectMany(x => x.DescendantsAndSelf()).Where(x => x.Kind == OperationKindEx.DeclarationExpression).Select(IDeclarationExpressionOperationWrapper.FromOperation).ToArray();
        var declaration = allDeclarations.Should().ContainSingle().Which.WrappedOperation;
        declaration.Kind.Should().Be(OperationKindEx.DeclarationExpression);
        var declarationExpression = IDeclarationExpressionOperationWrapper.FromOperation(declaration).Expression;
        declarationExpression.Kind.Should().Be(OperationKindEx.Tuple);
        declaration.TrackedSymbol(ProgramState.Empty).Should().BeNull();
    }

    [TestMethod]
    public void TrackedSymbol_SimpleAssignment_IsNull()
    {
        var simpleAssignment = TestHelper.CompileCfgBodyCS("var a = true; bool b; b = a;").Blocks[1].Operations[0];
        simpleAssignment.TrackedSymbol(ProgramState.Empty).Should().BeNull();
    }

    [DataTestMethod]
    [DataRow("""1, "Test" """, "intParam", 1)]
    [DataRow("""1, "Test" """, "stringParam", "Test")]
    [DataRow("""1, "Test" """, "optionalBoolParam", true)]
    [DataRow("""1, "Test" """, "optionalIntParam", 1)]
    [DataRow("""1, "Test", true """, "optionalBoolParam", true)]
    [DataRow("""stringParam: "Test", intParam: 1 """, "stringParam", "Test")]
    [DataRow("""stringParam: "Test", intParam: 1, optionalIntParam: 2 """, "optionalIntParam", 2)]
    public void ArgumentValue_ObjectCreation(string objectCreationArguments, string parameterName, object expected)
    {
        var testClass = $$"""
            class Test
            {
                Test(int intParam, string stringParam, bool optionalBoolParam = true, int optionalIntParam = 1) { }

                static void Create() =>
                    new Test({{objectCreationArguments}});
            }
            """;
        var (tree, model) = TestHelper.CompileCS(testClass);
        var objectCreation = tree.GetRoot().DescendantNodesAndSelf().OfType<ObjectCreationExpressionSyntax>().Single();
        var operation = IObjectCreationOperationWrapper.FromOperation(model.GetOperation(objectCreation));
        operation.ArgumentValue(parameterName).Should().NotBeNull().And.BeAssignableTo<IOperation>().Which.ConstantValue.Value.Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("""  """)]
    [DataRow(""" "param1", "param2" """, "param1", "param2")]
    [DataRow(""" null, null """, null, null)]
    [DataRow(""" new[] {"param1", "param2"} """, "param1", "param2")]
    public void ArgumentValue_ObjectCreation_Params(string arguments, params string[] expected)
    {
        var testClass = $$"""
            class Test
            {
                Test(params string[] stringParams) { }

                static void Create() =>
                    new Test({{arguments}});
            }
            """;
        var (tree, model) = TestHelper.CompileCS(testClass);
        var objectCreation = tree.GetRoot().DescendantNodesAndSelf().OfType<ObjectCreationExpressionSyntax>().Single();
        var operation = IObjectCreationOperationWrapper.FromOperation(model.GetOperation(objectCreation));
        var argument = operation.ArgumentValue("stringParams").Should().NotBeNull().And.BeAssignableTo<IOperation>().Subject;
        var argumentArray = IArrayCreationOperationWrapper.FromOperation(argument);
        var result = argumentArray.Initializer.ElementValues.Select(x => x.ConstantValue.Value).ToArray();
        result.Should().BeEquivalentTo(expected);
    }

    [TestMethod]
    public void ArgumentValue_Invocation()
    {
        const string testClass = """
            class Test
            {
                static void M(string stringParam) => M("param");
            }
            """;
        var (tree, model) = TestHelper.CompileCS(testClass);
        var invocation = tree.GetRoot().DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().Single();
        var operation = IInvocationOperationWrapper.FromOperation(model.GetOperation(invocation));
        operation.ArgumentValue("stringParam").Should().NotBeNull().And.BeAssignableTo<IOperation>().Which.ConstantValue.Value.Should().Be("param");
    }

    [TestMethod]
    public void ArgumentValue_PropertyReference()
    {
        const string testClass = """
            class Test
            {
                int this[int index] => this[1];
            }
            """;
        var (tree, model) = TestHelper.CompileCS(testClass);
        var elementAccess = tree.GetRoot().DescendantNodesAndSelf().OfType<ElementAccessExpressionSyntax>().Single();
        var operation = IPropertyReferenceOperationWrapper.FromOperation(model.GetOperation(elementAccess));
        operation.ArgumentValue("index").Should().NotBeNull().And.BeAssignableTo<IOperation>().Which.ConstantValue.Value.Should().Be(1);
    }

    [TestMethod]
    public void ArgumentValue_RaiseEvent()
    {
        const string testClass = """
            Imports System
            Public Class C
                Event SomeEvent As EventHandler
                Public Sub M()
                    RaiseEvent SomeEvent(Nothing, EventArgs.Empty)
                End Sub
            End Class
            """;
        var (tree, model) = TestHelper.CompileVB(testClass);
        var raiseEvent = tree.GetRoot().DescendantNodesAndSelf().OfType<Microsoft.CodeAnalysis.VisualBasic.Syntax.RaiseEventStatementSyntax>().Single();
        var operation = IRaiseEventOperationWrapper.FromOperation(model.GetOperation(raiseEvent));
        operation.ArgumentValue("sender").Should().NotBeNull().And.BeAssignableTo<IOperation>().Which.ConstantValue.Value.Should().BeNull();
        operation.ArgumentValue("e").Should().NotBeNull().And.BeAssignableTo<IOperation>().Which.Kind.Should().Be(OperationKindEx.FieldReference);
    }
}

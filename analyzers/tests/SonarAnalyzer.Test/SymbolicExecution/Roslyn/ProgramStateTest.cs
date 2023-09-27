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
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.Test.TestFramework.SymbolicExecution;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

[TestClass]
public partial class ProgramStateTest
{
    [TestMethod]
    public void Equals_ReturnsTrueForEquivalent()
    {
        var reusedValue = SymbolicValue.Empty.WithConstraint(TestConstraint.First);
        var anotherValue = SymbolicValue.Empty.WithConstraint(TestConstraint.Second);
        var cfg = TestHelper.CompileCfgBodyCS("var x = 42; var y = 42;");
        var operations = cfg.Blocks[1].Operations.ToExecutionOrder().ToArray();
        var symbols = operations.Select(x => x.Instance.TrackedSymbol(ProgramState.Empty)).Where(x => x is not null).ToArray();
        var exception = new ExceptionState(cfg.OriginalOperation.SemanticModel.Compilation.GetTypeByMetadataName("System.Exception"));

        var empty = ProgramState.Empty;
        var withOperationOrig = empty.SetOperationValue(operations[0], reusedValue);
        var withOperationSame = empty.SetOperationValue(operations[0], reusedValue);
        var withOperationDiff = empty.SetOperationValue(operations[0], anotherValue);
        var withSymbolOrig = empty.SetSymbolValue(symbols[0], reusedValue);
        var withSymbolSame = empty.SetSymbolValue(symbols[0], reusedValue);
        var withSymbolDiff = empty.SetSymbolValue(symbols[0], anotherValue);
        var withCaptureOrig = empty.SetCapture(new CaptureId(0), operations[0].Instance);
        var withCaptureSame = empty.SetCapture(new CaptureId(0), operations[0].Instance);
        var withCaptureDiff = empty.SetCapture(new CaptureId(0), operations[1].Instance);
        var withPreservedSymbolOrig = empty.Preserve(symbols[0]);
        var withPreservedSymbolSame = empty.Preserve(symbols[0]);
        var withPreservedSymbolDiff = empty.Preserve(symbols[1]);
        var withExceptionOrig = empty.PushException(exception);
        var withExceptionSame = empty.PushException(exception);
        var withExceptionMore = empty.PushException(exception).PushException(exception);
        var withExceptionDiff = empty.PushException(ExceptionState.UnknownException);
        var mixedOrig = withOperationOrig.SetSymbolValue(symbols[0], reusedValue);
        var mixedSame = withOperationSame.SetSymbolValue(symbols[0], reusedValue);
        var mixedDiff = withOperationDiff.SetSymbolValue(symbols[0], anotherValue);

        empty.Equals((object)empty).Should().BeTrue();
        empty.Equals((object)withOperationOrig).Should().BeFalse();
        empty.Equals("Other type").Should().BeFalse();
        empty.Equals((object)null).Should().BeFalse();
        empty.Equals((ProgramState)null).Should().BeFalse();    // Explicit cast to ensure correct overload

        withOperationOrig.Equals(withOperationOrig).Should().BeTrue();
        withOperationOrig.Equals(withOperationSame).Should().BeTrue();
        withOperationOrig.Equals(withOperationDiff).Should().BeFalse();
        withOperationOrig.Equals(empty).Should().BeFalse();
        withOperationOrig.Equals(withSymbolDiff).Should().BeFalse();
        withOperationOrig.Equals(withCaptureDiff).Should().BeFalse();
        withOperationOrig.Equals(withPreservedSymbolDiff).Should().BeFalse();
        withOperationOrig.Equals(withExceptionMore).Should().BeFalse();
        withOperationOrig.Equals(withExceptionDiff).Should().BeFalse();

        withSymbolOrig.Equals(withSymbolOrig).Should().BeTrue();
        withSymbolOrig.Equals(withSymbolSame).Should().BeTrue();
        withSymbolOrig.Equals(withSymbolDiff).Should().BeFalse();
        withSymbolOrig.Equals(empty).Should().BeFalse();
        withSymbolOrig.Equals(withOperationDiff).Should().BeFalse();
        withSymbolOrig.Equals(withCaptureDiff).Should().BeFalse();
        withSymbolOrig.Equals(withPreservedSymbolDiff).Should().BeFalse();
        withSymbolOrig.Equals(withExceptionMore).Should().BeFalse();
        withSymbolOrig.Equals(withExceptionDiff).Should().BeFalse();

        withCaptureOrig.Equals(withCaptureOrig).Should().BeTrue();
        withCaptureOrig.Equals(withCaptureSame).Should().BeTrue();
        withCaptureOrig.Equals(withCaptureDiff).Should().BeFalse();
        withCaptureOrig.Equals(empty).Should().BeFalse();
        withCaptureOrig.Equals(withOperationDiff).Should().BeFalse();
        withCaptureOrig.Equals(withSymbolDiff).Should().BeFalse();

        withPreservedSymbolOrig.Equals(withPreservedSymbolOrig).Should().BeTrue();
        withPreservedSymbolOrig.Equals(withPreservedSymbolSame).Should().BeTrue();
        withPreservedSymbolOrig.Equals(withPreservedSymbolDiff).Should().BeFalse();
        withPreservedSymbolOrig.Equals(empty).Should().BeFalse();
        withPreservedSymbolOrig.Equals(withOperationDiff).Should().BeFalse();
        withPreservedSymbolOrig.Equals(withSymbolDiff).Should().BeFalse();
        withPreservedSymbolOrig.Equals(withExceptionMore).Should().BeFalse();
        withPreservedSymbolOrig.Equals(withExceptionDiff).Should().BeFalse();

        withExceptionOrig.Equals(withExceptionOrig).Should().BeTrue();
        withExceptionOrig.Equals(withExceptionSame).Should().BeTrue();
        withExceptionOrig.Equals(withExceptionMore).Should().BeFalse();
        withExceptionOrig.Equals(withExceptionDiff).Should().BeFalse();
        withExceptionOrig.Equals(empty).Should().BeFalse();
        withExceptionOrig.Equals(withOperationDiff).Should().BeFalse();
        withExceptionOrig.Equals(withSymbolDiff).Should().BeFalse();

        mixedOrig.Equals(mixedOrig).Should().BeTrue();
        mixedOrig.Equals(mixedSame).Should().BeTrue();
        mixedOrig.Equals(mixedDiff).Should().BeFalse();
        mixedOrig.Equals(empty).Should().BeFalse();
    }

    [TestMethod]
    public void GetHashCode_ReturnsSameForEquivalent()
    {
        var reusedValue = SymbolicValue.Empty.WithConstraint(TestConstraint.First);
        var anotherValue = SymbolicValue.Empty.WithConstraint(TestConstraint.Second);
        var cfg = TestHelper.CompileCfgBodyCS("var x = 42; var y = 42;");
        var operations = cfg.Blocks[1].Operations.ToExecutionOrder().ToArray();
        var symbols = operations.Select(x => x.Instance.TrackedSymbol(ProgramState.Empty)).Where(x => x is not null).ToArray();
        var exception = new ExceptionState(cfg.OriginalOperation.SemanticModel.Compilation.GetTypeByMetadataName("System.Exception"));

        var empty = ProgramState.Empty;
        var withOperationOrig = empty.SetOperationValue(operations[0], reusedValue);
        var withOperationSame = empty.SetOperationValue(operations[0], reusedValue);
        var withOperationDiff = empty.SetOperationValue(operations[0], anotherValue);
        var withSymbolOrig = empty.SetSymbolValue(symbols[0], reusedValue);
        var withSymbolSame = empty.SetSymbolValue(symbols[0], reusedValue);
        var withSymbolDiff = empty.SetSymbolValue(symbols[0], anotherValue);
        var withCaptureOrig = empty.SetCapture(new CaptureId(0), operations[0].Instance);
        var withCaptureSame = empty.SetCapture(new CaptureId(0), operations[0].Instance);
        var withCaptureDiff = empty.SetCapture(new CaptureId(0), operations[1].Instance);
        var withPreservedSymbolOrig = empty.Preserve(symbols[0]);
        var withPreservedSymbolSame = empty.Preserve(symbols[0]);
        var withPreservedSymbolDiff = empty.Preserve(symbols[1]);
        var withExceptionOrig = empty.PushException(exception);
        var withExceptionSame = empty.PushException(exception);
        var withExceptionMore = empty.PushException(exception).PushException(exception);
        var withExceptionDiff = empty.PushException(ExceptionState.UnknownException);
        var mixedOrig = withOperationOrig.SetSymbolValue(symbols[0], reusedValue);
        var mixedSame = withOperationSame.SetSymbolValue(symbols[0], reusedValue);
        var mixedDiff = withOperationDiff.SetSymbolValue(symbols[0], anotherValue);

        empty.GetHashCode().Should().Be(empty.GetHashCode());

        withOperationOrig.GetHashCode().Should().Be(withOperationSame.GetHashCode());
        withOperationOrig.GetHashCode().Should().NotBe(withOperationDiff.GetHashCode());
        withOperationOrig.GetHashCode().Should().NotBe(withSymbolSame.GetHashCode());
        withOperationOrig.GetHashCode().Should().NotBe(mixedSame.GetHashCode());

        withSymbolOrig.GetHashCode().Should().Be(withSymbolSame.GetHashCode());
        withSymbolOrig.GetHashCode().Should().NotBe(withSymbolDiff.GetHashCode());
        withSymbolOrig.GetHashCode().Should().NotBe(mixedSame.GetHashCode());

        withCaptureOrig.GetHashCode().Should().Be(withCaptureSame.GetHashCode());
        withCaptureOrig.GetHashCode().Should().NotBe(withCaptureDiff.GetHashCode());

        withPreservedSymbolOrig.GetHashCode().Should().Be(withPreservedSymbolSame.GetHashCode());
        withPreservedSymbolOrig.GetHashCode().Should().NotBe(withPreservedSymbolDiff.GetHashCode());
        withPreservedSymbolOrig.GetHashCode().Should().NotBe(mixedSame.GetHashCode());

        withExceptionOrig.GetHashCode().Should().Be(withExceptionSame.GetHashCode());
        withExceptionOrig.GetHashCode().Should().NotBe(withExceptionMore.GetHashCode());
        withExceptionOrig.GetHashCode().Should().NotBe(withExceptionDiff.GetHashCode());
        withExceptionOrig.GetHashCode().Should().NotBe(mixedSame.GetHashCode());

        mixedOrig.GetHashCode().Should().Be(mixedSame.GetHashCode());
        mixedOrig.GetHashCode().Should().NotBe(mixedDiff.GetHashCode());
    }

    [TestMethod]
    public void ToString_Empty() =>
        ProgramState.Empty.ToString().Should().BeIgnoringLineEndings(
@"Empty
");

    [TestMethod]
    public void ToString_WithSymbols()
    {
        var assignment = TestHelper.CompileCfgBodyCS("var a = arg;", "bool arg").Blocks[1].Operations[0];
        var variableSymbol = assignment.ChildOperations.First().TrackedSymbol(ProgramState.Empty);
        var parameterSymbol = assignment.ChildOperations.Last().TrackedSymbol(ProgramState.Empty);
        var sut = ProgramState.Empty.SetSymbolValue(variableSymbol, null);
        sut.ToString().Should().BeIgnoringLineEndings(
@"Empty
");

        sut = ProgramState.Empty.SetSymbolValue(variableSymbol, SymbolicValue.Empty);
        sut.ToString().Should().BeIgnoringLineEndings(
@"Symbols:
a: No constraints
");

        var valueWithConstraint = SymbolicValue.Empty.WithConstraint(TestConstraint.Second);
        sut = sut.SetSymbolValue(variableSymbol.ContainingSymbol, valueWithConstraint).SetSymbolValue(parameterSymbol, valueWithConstraint);
        sut.ToString().Should().BeIgnoringLineEndings(
@"Symbols:
a: No constraints
arg: Second
Main: Second
");
    }

    [TestMethod]
    public void ToString_WithOperations()
    {
        var assignment = TestHelper.CompileCfgBodyCS("var a = true;").Blocks[1].Operations[0];
        var sut = ProgramState.Empty.SetOperationValue(assignment, null);
        sut.ToString().Should().BeIgnoringLineEndings(
@"Empty
");

        sut = ProgramState.Empty.SetOperationValue(assignment, SymbolicValue.Empty);
        sut.ToString().Should().BeIgnoringLineEndings(
@"Operations:
SimpleAssignmentOperation: a = true: No constraints
");
        var valueWithConstraint = SymbolicValue.Empty.WithConstraint(TestConstraint.Second);
        sut = sut.SetOperationValue(assignment.ChildOperations.First(), valueWithConstraint);
        sut.ToString().Should().BeIgnoringLineEndings(
@"Operations:
LocalReferenceOperation: a = true: Second
SimpleAssignmentOperation: a = true: No constraints
");
    }

    [TestMethod]
    public void ToString_WithCaptures()
    {
        var assignment = TestHelper.CompileCfgBodyCS("var a = true;").Blocks[1].Operations[0];
        var sut = ProgramState.Empty.SetCapture(new CaptureId(42), assignment);
        sut.ToString().Should().BeIgnoringLineEndings(
@"Captures:
#42: SimpleAssignmentOperation: a = true
");
        sut.SetCapture(new CaptureId(24), assignment).ToString().Should().BeIgnoringLineEndings(
@"Captures:
#24: SimpleAssignmentOperation: a = true
#42: SimpleAssignmentOperation: a = true
");
    }

    [TestMethod]
    public void ToString_Exception()
    {
        var sut = ProgramState.Empty.PushException(ExceptionState.UnknownException);
        sut.ToString().Should().BeIgnoringLineEndings(
@"Exception: Unknown
");

        sut = sut.PushException(ExceptionState.UnknownException);   // There are two exceptions in the stack now
        sut.ToString().Should().BeIgnoringLineEndings(
@"Exception: Unknown
Exception: Unknown
");
    }

    [TestMethod]
    public void ToString_WithAll()
    {
        var assignment = TestHelper.CompileCfgBodyCS("var a = true;").Blocks[1].Operations[0];
        var variableSymbol = assignment.ChildOperations.First().TrackedSymbol(ProgramState.Empty);
        var valueWithConstraint = SymbolicValue.Empty.WithConstraint(TestConstraint.First);
        var sut = ProgramState.Empty
            .SetSymbolValue(variableSymbol, SymbolicValue.Empty)
            .SetSymbolValue(variableSymbol.ContainingSymbol, valueWithConstraint)
            .SetOperationValue(assignment, SymbolicValue.Empty)
            .SetOperationValue(assignment.ChildOperations.First(), valueWithConstraint).Preserve(variableSymbol)
            .SetCapture(new CaptureId(0), assignment)
            .SetCapture(new CaptureId(1), assignment.ChildOperations.First())
            .PushException(ExceptionState.UnknownException);

        sut.ToString().Should().BeIgnoringLineEndings(
@"Exception: Unknown
Symbols:
a: No constraints
Main: First
Operations:
LocalReferenceOperation: a = true: First
SimpleAssignmentOperation: a = true: No constraints
Captures:
#0: SimpleAssignmentOperation: a = true
#1: LocalReferenceOperation: a = true
");
    }

    [TestMethod]
    public void ResetFieldConstraints_NotPreservesField() =>
        ResetFieldConstraintTests(ObjectConstraint.Null, false);

    [TestMethod]
    public void ResetFieldConstraints_PreservesField() =>
        ResetFieldConstraintTests(LockConstraint.Held, true);

    [TestMethod]
    public void ResetFieldConstraints_ResetFieldsAsDefined()
    {
        var instanceField = CreateFieldSymbol("object field;");
        var staticField = CreateFieldSymbol("static object field;");

        var preserveAll = LockConstraint.Held;
        var preserveNone = ObjectConstraint.Null;

        var sut = ProgramState.Empty;
        sut = sut.SetSymbolValue(instanceField, SymbolicValue.Empty.WithConstraint(preserveAll).WithConstraint(preserveNone))
                 .SetSymbolValue(staticField, SymbolicValue.Empty.WithConstraint(preserveAll).WithConstraint(preserveNone));
        sut = sut.ResetFieldConstraints();
        var instanceFieldSymbolValue = sut[instanceField];
        var staticFieldSymbolValue = sut[staticField];
        instanceFieldSymbolValue.HasConstraint(preserveAll).Should().BeTrue();
        instanceFieldSymbolValue.HasConstraint(preserveNone).Should().BeFalse();
        staticFieldSymbolValue.HasConstraint(preserveAll).Should().BeTrue();
        staticFieldSymbolValue.HasConstraint(preserveNone).Should().BeFalse();
    }

    private static IFieldSymbol CreateFieldSymbol(string fieldDefinition)
    {
        var compiler = new SnippetCompiler($@"class C {{ {fieldDefinition} }}");
        var fieldSymbol = compiler.SemanticModel.GetDeclaredSymbol(compiler.GetNodes<VariableDeclaratorSyntax>().Single());
        return (IFieldSymbol)fieldSymbol;
    }

    private static void ResetFieldConstraintTests(SymbolicConstraint constraint, bool expectIsPreserved)
    {
        var field = CreateFieldSymbol("object field;");
        var sut = ProgramState.Empty;
        sut = sut.SetSymbolValue(field, SymbolicValue.Empty.WithConstraint(constraint));
        var symbolValue = sut[field];
        symbolValue.HasConstraint(constraint).Should().BeTrue();
        sut = sut.ResetFieldConstraints();
        symbolValue = sut[field];
        if (expectIsPreserved)
        {
            symbolValue.HasConstraint(constraint).Should().BeTrue();
        }
        else
        {
            symbolValue.Should().BeNull();
        }
    }

    private static ISymbol[] CreateSymbols()
    {
        const string code = @"public class Sample { public void Main() { var variable = 0; } }";
        var (_, model) = TestHelper.CompileCS(code);
        var ret = model.LookupSymbols(code.IndexOf("variable")).Where(x => x.Name == "Sample" || x.Name == "Main" || x.Name == "variable").ToArray();
        ret.Should().HaveCount(3);
        return ret;
    }
}

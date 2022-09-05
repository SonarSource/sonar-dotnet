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

using SonarAnalyzer.Extensions;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.Helpers;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    [TestClass]
    public partial class ProgramStateTest
    {
        [TestMethod]
        public void Equals_ReturnsTrueForEquivalent()
        {
            var reusedValue = new SymbolicValue().WithConstraint(TestConstraint.First);
            var anotherValue = new SymbolicValue().WithConstraint(TestConstraint.Second);
            var cfg = TestHelper.CompileCfgBodyCS("var x = 42; var y = 42;");
            var operations = cfg.Blocks[1].Operations.ToExecutionOrder().ToArray();
            var symbols = operations.Select(x => x.Instance.TrackedSymbol()).Where(x => x is not null).ToArray();
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
            var reusedValue = new SymbolicValue().WithConstraint(TestConstraint.First);
            var anotherValue = new SymbolicValue().WithConstraint(TestConstraint.Second);
            var cfg = TestHelper.CompileCfgBodyCS("var x = 42; var y = 42;");
            var operations = cfg.Blocks[1].Operations.ToExecutionOrder().ToArray();
            var symbols = operations.Select(x => x.Instance.TrackedSymbol()).Where(x => x is not null).ToArray();
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
            var variableSymbol = assignment.ChildOperations.First().TrackedSymbol();
            var parameterSymbol = assignment.ChildOperations.Last().TrackedSymbol();
            var sut = ProgramState.Empty.SetSymbolValue(variableSymbol, null);
            sut.ToString().Should().BeIgnoringLineEndings(
@"Empty
");

            sut = ProgramState.Empty.SetSymbolValue(variableSymbol, new());
            sut.ToString().Should().BeIgnoringLineEndings(
@"Symbols:
a: No constraints
");

            var valueWithConstraint = new SymbolicValue().WithConstraint(TestConstraint.Second);
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

            sut = ProgramState.Empty.SetOperationValue(assignment, new());
            sut.ToString().Should().BeIgnoringLineEndings(
@"Operations:
SimpleAssignmentOperation / VariableDeclaratorSyntax: a = true: No constraints
");
            var valueWithConstraint = new SymbolicValue().WithConstraint(TestConstraint.Second);
            sut = sut.SetOperationValue(assignment.ChildOperations.First(), valueWithConstraint);
            sut.ToString().Should().BeIgnoringLineEndings(
@"Operations:
LocalReferenceOperation / VariableDeclaratorSyntax: a = true: Second
SimpleAssignmentOperation / VariableDeclaratorSyntax: a = true: No constraints
");
        }

        [TestMethod]
        public void ToString_WithCaptures()
        {
            var assignment = TestHelper.CompileCfgBodyCS("var a = true;").Blocks[1].Operations[0];
            var sut = ProgramState.Empty.SetCapture(new CaptureId(42), assignment);
            sut.ToString().Should().BeIgnoringLineEndings(
@"Captures:
#42: SimpleAssignmentOperation / VariableDeclaratorSyntax: a = true
");
            sut.SetCapture(new CaptureId(24), assignment).ToString().Should().BeIgnoringLineEndings(
@"Captures:
#24: SimpleAssignmentOperation / VariableDeclaratorSyntax: a = true
#42: SimpleAssignmentOperation / VariableDeclaratorSyntax: a = true
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
            var variableSymbol = assignment.ChildOperations.First().TrackedSymbol();
            var valueWithConstraint = new SymbolicValue().WithConstraint(TestConstraint.First);
            var sut = ProgramState.Empty
                .SetSymbolValue(variableSymbol, new())
                .SetSymbolValue(variableSymbol.ContainingSymbol, valueWithConstraint)
                .SetOperationValue(assignment, new())
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
LocalReferenceOperation / VariableDeclaratorSyntax: a = true: First
SimpleAssignmentOperation / VariableDeclaratorSyntax: a = true: No constraints
Captures:
#0: SimpleAssignmentOperation / VariableDeclaratorSyntax: a = true
#1: LocalReferenceOperation / VariableDeclaratorSyntax: a = true
");
        }

        [TestMethod]
        public void ProgramStateToArrayEncapsulatesSingleElement()
        {
            var programState = ProgramState.Empty;
            ProgramState[] target = programState;
            target.Should().NotBeNull().And.HaveCount(1).And.Equal(new[] { ProgramState.Empty });
        }

        [TestMethod]
        public void ProgramStateToArrayConversionHandlesNull()
        {
            ProgramState[] target = (ProgramState)null;
            target.Should().NotBeNull().And.HaveCount(0).And.BeOfType<ProgramState[]>();
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
}

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

using Microsoft.CodeAnalysis;
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
            var counter = new SymbolicValueCounter();
            var reusedValue = new SymbolicValue(counter).WithConstraint(TestConstraint.First);
            var anotherValue = new SymbolicValue(counter).WithConstraint(TestConstraint.Second);
            var operations = TestHelper.CompileCfgBodyCS("var x = 42; var y = 42;").Blocks[1].Operations.ToExecutionOrder().ToArray();
            var symbols = operations.Select(x => x.Instance.TrackedSymbol()).Where(x => x is not null).ToArray();
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

            withSymbolOrig.Equals(withSymbolOrig).Should().BeTrue();
            withSymbolOrig.Equals(withSymbolSame).Should().BeTrue();
            withSymbolOrig.Equals(withSymbolDiff).Should().BeFalse();
            withSymbolOrig.Equals(empty).Should().BeFalse();
            withSymbolOrig.Equals(withOperationDiff).Should().BeFalse();
            withSymbolOrig.Equals(withCaptureDiff).Should().BeFalse();
            withSymbolOrig.Equals(withPreservedSymbolDiff).Should().BeFalse();

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

            mixedOrig.Equals(mixedOrig).Should().BeTrue();
            mixedOrig.Equals(mixedSame).Should().BeTrue();
            mixedOrig.Equals(mixedDiff).Should().BeFalse();
            mixedOrig.Equals(empty).Should().BeFalse();
        }

        [TestMethod]
        public void GetHashCode_ReturnsSameForEquivalent()
        {
            var counter = new SymbolicValueCounter();
            var reusedValue = new SymbolicValue(counter).WithConstraint(TestConstraint.First);
            var anotherValue = new SymbolicValue(counter).WithConstraint(TestConstraint.Second);
            var operations = TestHelper.CompileCfgBodyCS("var x = 42; var y = 42;").Blocks[1].Operations.ToExecutionOrder().ToArray();
            var symbols = operations.Select(x => x.Instance.TrackedSymbol()).Where(x => x is not null).ToArray();
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

            mixedOrig.GetHashCode().Should().Be(mixedSame.GetHashCode());
            mixedOrig.GetHashCode().Should().NotBe(mixedDiff.GetHashCode());
        }

        [TestMethod]
        public void ToString_Empty() =>
            ProgramState.Empty.ToString().Should().Be("Empty");

        [TestMethod]
        public void ToString_WithSymbols()
        {
            var assignment = TestHelper.CompileCfgBodyCS("var a = true;").Blocks[1].Operations[0];
            var counter = new SymbolicValueCounter();
            var variableSymbol = assignment.Children.First().TrackedSymbol();
            var sut = ProgramState.Empty.SetSymbolValue(variableSymbol, null);
            sut.ToString().Should().Be("Empty");

            sut = ProgramState.Empty.SetSymbolValue(variableSymbol, new SymbolicValue(counter));
            sut.ToString().Should().BeIgnoringLineEndings(
@"Symbols:
a: SV_1
");

            var valueWithConstraint = new SymbolicValue(counter).WithConstraint(TestConstraint.Second);
            sut = sut.SetSymbolValue(variableSymbol.ContainingSymbol, valueWithConstraint);
            sut.ToString().Should().BeIgnoringLineEndings(
@"Symbols:
a: SV_1
Sample.Main(): SV_3: Second
");
        }

        [TestMethod]
        public void ToString_WithOperations()
        {
            var assignment = TestHelper.CompileCfgBodyCS("var a = true;").Blocks[1].Operations[0];
            var counter = new SymbolicValueCounter();
            var sut = ProgramState.Empty.SetOperationValue(assignment, null);
            sut.ToString().Should().Be("Empty");

            sut = ProgramState.Empty.SetOperationValue(assignment, new SymbolicValue(counter));
            sut.ToString().Should().BeIgnoringLineEndings(
@"Operations:
SimpleAssignmentOperation / VariableDeclaratorSyntax: a = true: SV_1
");
            var valueWithConstraint = new SymbolicValue(counter).WithConstraint(TestConstraint.Second);
            sut = sut.SetOperationValue(assignment.Children.First(), valueWithConstraint);
            sut.ToString().Should().BeIgnoringLineEndings(
@"Operations:
LocalReferenceOperation / VariableDeclaratorSyntax: a = true: SV_3: Second
SimpleAssignmentOperation / VariableDeclaratorSyntax: a = true: SV_1
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
        public void ToString_WithAll()
        {
            var assignment = TestHelper.CompileCfgBodyCS("var a = true;").Blocks[1].Operations[0];
            var counter = new SymbolicValueCounter();
            var variableSymbol = assignment.Children.First().TrackedSymbol();
            var valueWithConstraint = new SymbolicValue(counter).WithConstraint(TestConstraint.First);
            var sut = ProgramState.Empty
                .SetSymbolValue(variableSymbol, new SymbolicValue(counter))
                .SetSymbolValue(variableSymbol.ContainingSymbol, valueWithConstraint)
                .SetOperationValue(assignment, new SymbolicValue(counter))
                .SetOperationValue(assignment.Children.First(), valueWithConstraint).Preserve(variableSymbol)
                .SetCapture(new CaptureId(0), assignment)
                .SetCapture(new CaptureId(1), assignment.Children.First());

            sut.ToString().Should().BeIgnoringLineEndings(
@"Symbols:
a: SV_3
Sample.Main(): SV_2: First
Operations:
LocalReferenceOperation / VariableDeclaratorSyntax: a = true: SV_2: First
SimpleAssignmentOperation / VariableDeclaratorSyntax: a = true: SV_4
Captures:
#0: SimpleAssignmentOperation / VariableDeclaratorSyntax: a = true
#1: LocalReferenceOperation / VariableDeclaratorSyntax: a = true
");
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

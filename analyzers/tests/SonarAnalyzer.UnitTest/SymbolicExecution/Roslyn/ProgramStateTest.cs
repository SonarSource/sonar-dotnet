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

using System;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.Helpers;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    [TestClass]
    public class ProgramStateTest
    {
        [TestMethod]
        public void SetOperationValue_WithWrapper_ReturnsValues()
        {
            var counter = new SymbolicValueCounter();
            var value1 = new SymbolicValue(counter);
            var value2 = new SymbolicValue(counter);
            var operations = TestHelper.CompileCfgBodyCS("var x = 0; x = 1; x = 42;").Blocks[1].Operations;
            var op1 = new IOperationWrapperSonar(operations[0]);
            var op2 = new IOperationWrapperSonar(operations[1]);
            var op3 = new IOperationWrapperSonar(operations[2]);
            var sut = ProgramState.Empty;

            sut[op1].Should().BeNull();
            sut[op2].Should().BeNull();
            sut[op3].Should().BeNull();

            sut = sut.SetOperationValue(op1, value1);
            sut = sut.SetOperationValue(op2, value2);

            sut[op1].Should().Be(value1);
            sut[op2].Should().Be(value2);
            sut[op3].Should().BeNull();     // Value was not set
        }

        [TestMethod]
        public void SetOperationValue_ReturnsValues()
        {
            var counter = new SymbolicValueCounter();
            var value1 = new SymbolicValue(counter);
            var value2 = new SymbolicValue(counter);
            var operations = TestHelper.CompileCfgBodyCS("var x = 0; x = 1; x = 42;").Blocks[1].Operations;
            var sut = ProgramState.Empty;

            sut[operations[0]].Should().BeNull();
            sut[operations[1]].Should().BeNull();
            sut[operations[2]].Should().BeNull();

            sut = sut.SetOperationValue(operations[0], value1);
            sut = sut.SetOperationValue(operations[1], value2);

            sut[operations[0]].Should().Be(value1);
            sut[operations[1]].Should().Be(value2);
            sut[operations[2]].Should().BeNull();     // Value was not set
        }

        [TestMethod]
        public void SetOperationValue_WithWrapper_IsImmutable()
        {
            var operation = new IOperationWrapperSonar(TestHelper.CompileCfgBodyCS("var x = 42;").Blocks[1].Operations[0]);
            var sut = ProgramState.Empty;

            sut[operation].Should().BeNull();
            sut.SetOperationValue(operation, new SymbolicValue(new SymbolicValueCounter()));
            sut[operation].Should().BeNull(nameof(sut.SetOperationValue) + " returned new ProgramState instance.");
        }

        [TestMethod]
        public void SetOperationValue_IsImmutable()
        {
            var operation = TestHelper.CompileCfgBodyCS("var x = 42;").Blocks[1].Operations[0];
            var sut = ProgramState.Empty;

            sut[operation].Should().BeNull();
            sut.SetOperationValue(operation, new SymbolicValue(new SymbolicValueCounter()));
            sut[operation].Should().BeNull(nameof(sut.SetOperationValue) + " returned new ProgramState instance.");
        }

        [TestMethod]
        public void SetOperationValue_WithWrapper_UsesUnderlyingOperation()
        {
            var operation = new IOperationWrapperSonar(TestHelper.CompileCfgBodyCS("var x = 42;").Blocks[1].Operations[0]);
            var another = new IOperationWrapperSonar(operation.Instance);
            var value = new SymbolicValue(new SymbolicValueCounter());
            var sut = ProgramState.Empty;

            sut[operation].Should().BeNull();
            sut = sut.SetOperationValue(operation, value);
            sut[another].Should().Be(value, "GetHashCode and Equals are based on underlying IOperation instance.");
            sut[another.Instance].Should().Be(value, "GetHashCode and Equals are based on underlying IOperation instance.");
        }

        [TestMethod]
        public void SetOperationValue_WithWrapper_Overrides()
        {
            var counter = new SymbolicValueCounter();
            var value1 = new SymbolicValue(counter);
            var value2 = new SymbolicValue(counter);
            var operation = new IOperationWrapperSonar(TestHelper.CompileCfgBodyCS("var x = 42;").Blocks[1].Operations[0]);
            var sut = ProgramState.Empty;

            sut[operation].Should().BeNull();
            sut = sut.SetOperationValue(operation, value1);
            sut = sut.SetOperationValue(operation, value2);
            sut[operation].Should().Be(value2);
        }

        [TestMethod]
        public void SetOperationValue_Overrides()
        {
            var counter = new SymbolicValueCounter();
            var value1 = new SymbolicValue(counter);
            var value2 = new SymbolicValue(counter);
            var operation = TestHelper.CompileCfgBodyCS("var x = 42;").Blocks[1].Operations[0];
            var sut = ProgramState.Empty;

            sut[operation].Should().BeNull();
            sut = sut.SetOperationValue(operation, value1);
            sut = sut.SetOperationValue(operation, value2);
            sut[operation].Should().Be(value2);
        }

        [TestMethod]
        public void SetOperationValue_NullValue_ReturnsNull()
        {
            var operation = TestHelper.CompileCfgBodyCS("var x = 42;").Blocks[1].Operations[0];
            var sut = ProgramState.Empty;

            sut[operation].Should().BeNull();
            sut = sut.SetOperationValue(operation, null);
            sut[operation].Should().BeNull();
        }

        [TestMethod]
        public void SetOperationValue_NullOperation_Throws() =>
            ProgramState.Empty.Invoking(x => x.SetOperationValue((IOperation)null, new SymbolicValue(new SymbolicValueCounter()))).Should().Throw<ArgumentNullException>();

        [TestMethod]
        public void SetOperationValue_WithWrapper_NullOperation_Throws() =>
            ProgramState.Empty.Invoking(x => x.SetOperationValue((IOperationWrapperSonar)null, new SymbolicValue(new SymbolicValueCounter()))).Should().Throw<NullReferenceException>();

        [TestMethod]
        public void ResetOperations_IsImmutable()
        {
            var operation = TestHelper.CompileCfgBodyCS("var x = 42;").Blocks[1].Operations[0];
            var beforeReset = ProgramState.Empty.SetOperationValue(operation, new(new()));
            beforeReset[operation].Should().NotBeNull();
            var afterReset = beforeReset.ResetOperations();
            beforeReset[operation].Should().NotBeNull();
            afterReset[operation].Should().BeNull();
        }

        [TestMethod]
        public void SetSymbolValue_ReturnsValues()
        {
            var counter = new SymbolicValueCounter();
            var value1 = new SymbolicValue(counter);
            var value2 = new SymbolicValue(counter);
            var symbols = CreateSymbols();
            var sut = ProgramState.Empty;

            sut[symbols[0]].Should().BeNull();
            sut[symbols[1]].Should().BeNull();
            sut[symbols[2]].Should().BeNull();

            sut = sut.SetSymbolValue(symbols[0], value1);
            sut = sut.SetSymbolValue(symbols[1], value2);

            sut[symbols[0]].Should().Be(value1);
            sut[symbols[1]].Should().Be(value2);
            sut[symbols[2]].Should().BeNull();     // Value was not set
        }

        [TestMethod]
        public void SetSymbolValue_IsImmutable()
        {
            var symbol = CreateSymbols().First();
            var sut = ProgramState.Empty;

            sut[symbol].Should().BeNull();
            sut.SetSymbolValue(symbol, new SymbolicValue(new SymbolicValueCounter()));
            sut[symbol].Should().BeNull(nameof(sut.SetSymbolValue) + " returned new ProgramState instance.");
        }

        [TestMethod]
        public void SetSymbolValue_Overrides()
        {
            var counter = new SymbolicValueCounter();
            var value1 = new SymbolicValue(counter);
            var value2 = new SymbolicValue(counter);
            var symbol = CreateSymbols().First();
            var sut = ProgramState.Empty;

            sut[symbol].Should().BeNull();
            sut = sut.SetSymbolValue(symbol, value1);
            sut = sut.SetSymbolValue(symbol, value2);
            sut[symbol].Should().Be(value2);
        }

        [TestMethod]
        public void SetSymbolValue_NullSymbol_Throws() =>
            ProgramState.Empty.Invoking(x => x.SetSymbolValue(null, new SymbolicValue(new SymbolicValueCounter()))).Should().Throw<ArgumentNullException>();

        [TestMethod]
        public void SymbolsWith_ReturnCorrectSymbols()
        {
            var counter = new SymbolicValueCounter();
            var value0 = new SymbolicValue(counter);
            var value1 = new SymbolicValue(counter);
            var value2 = new SymbolicValue(counter);
            value0.SetConstraint(DummyConstraint.Dummy);
            value1.SetConstraint(TestConstraint.First);
            value2.SetConstraint(TestConstraint.First);
            var symbols = CreateSymbols();
            var sut = ProgramState.Empty
                .SetSymbolValue(symbols[0], value0)
                .SetSymbolValue(symbols[1], value1)
                .SetSymbolValue(symbols[2], value2);

            sut.SymbolsWith(DummyConstraint.Dummy).Should().ContainSingle().Which.Should().Be(symbols[0]);
            sut.SymbolsWith(TestConstraint.First).Should().HaveCount(2);
            sut.SymbolsWith(TestConstraint.Second).Should().BeEmpty();
        }

        [TestMethod]
        public void SymbolsWith_IgnoreNullValue()
        {
            var symbol = CreateSymbols().First();
            var sut = ProgramState.Empty.SetSymbolValue(symbol, null);
            sut.SymbolsWith(DummyConstraint.Dummy).Should().BeEmpty();
        }

        [TestMethod]
        public void Equals_ReturnsTrueForEquivalent()
        {
            var counter = new SymbolicValueCounter();
            var reusedValue = new SymbolicValue(counter);
            reusedValue.SetConstraint(TestConstraint.First);
            var anotherValue = new SymbolicValue(counter);
            anotherValue.SetConstraint(TestConstraint.Second);
            var operations = TestHelper.CompileCfgBodyCS("var x = 42;").Blocks[1].Operations.ToExecutionOrder().ToArray();
            var symbol = operations.Select(x => x.Instance.TrackedSymbol()).First(x => x is not null);
            var empty = ProgramState.Empty;
            var withOperationOrig = empty.SetOperationValue(operations[0], reusedValue);
            var withOperationSame = empty.SetOperationValue(operations[0], reusedValue);
            var withOperationDiff = empty.SetOperationValue(operations[0], anotherValue);
            var withSymbolOrig = empty.SetSymbolValue(symbol, reusedValue);
            var withSymbolSame = empty.SetSymbolValue(symbol, reusedValue);
            var withSymbolDiff = empty.SetSymbolValue(symbol, anotherValue);
            var mixedOrig = withOperationOrig.SetSymbolValue(symbol, reusedValue);
            var mixedSame = withOperationSame.SetSymbolValue(symbol, reusedValue);
            var mixedDiff = withOperationDiff.SetSymbolValue(symbol, anotherValue);

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

            withSymbolOrig.Equals(withSymbolOrig).Should().BeTrue();
            withSymbolOrig.Equals(withSymbolSame).Should().BeTrue();
            withSymbolOrig.Equals(withSymbolDiff).Should().BeFalse();
            withSymbolOrig.Equals(empty).Should().BeFalse();
            withSymbolOrig.Equals(withOperationDiff).Should().BeFalse();

            mixedOrig.Equals(mixedOrig).Should().BeTrue();
            mixedOrig.Equals(mixedSame).Should().BeTrue();
            mixedOrig.Equals(mixedDiff).Should().BeFalse();
            mixedOrig.Equals(empty).Should().BeFalse();
        }

        [TestMethod]
        public void GetHashCode_ReturnsSameForEquivalent()
        {
            var counter = new SymbolicValueCounter();
            var reusedValue = new SymbolicValue(counter);
            reusedValue.SetConstraint(TestConstraint.First);
            var anotherValue = new SymbolicValue(counter);
            anotherValue.SetConstraint(TestConstraint.Second);
            var operations = TestHelper.CompileCfgBodyCS("var x = 42;").Blocks[1].Operations.ToExecutionOrder().ToArray();
            var symbol = operations.Select(x => x.Instance.TrackedSymbol()).First(x => x is not null);
            var empty = ProgramState.Empty;
            var withOperationOrig = empty.SetOperationValue(operations[0], reusedValue);
            var withOperationSame = empty.SetOperationValue(operations[0], reusedValue);
            var withOperationDiff = empty.SetOperationValue(operations[0], anotherValue);
            var withSymbolOrig = empty.SetSymbolValue(symbol, reusedValue);
            var withSymbolSame = empty.SetSymbolValue(symbol, reusedValue);
            var withSymbolDiff = empty.SetSymbolValue(symbol, anotherValue);
            var mixedOrig = withOperationOrig.SetSymbolValue(symbol, reusedValue);
            var mixedSame = withOperationSame.SetSymbolValue(symbol, reusedValue);
            var mixedDiff = withOperationDiff.SetSymbolValue(symbol, anotherValue);

            empty.GetHashCode().Should().Be(empty.GetHashCode());

            withOperationOrig.GetHashCode().Should().Be(withOperationSame.GetHashCode());
            withOperationOrig.GetHashCode().Should().Be(withOperationDiff.GetHashCode(), "SymbolicValue produces constant hash code");
            withOperationOrig.GetHashCode().Should().NotBe(withSymbolSame.GetHashCode());
            withOperationOrig.GetHashCode().Should().NotBe(mixedSame.GetHashCode());

            withSymbolOrig.GetHashCode().Should().Be(withSymbolSame.GetHashCode());
            withSymbolOrig.GetHashCode().Should().Be(withSymbolDiff.GetHashCode(), "SymbolicValue produces constant hash code");
            withSymbolOrig.GetHashCode().Should().NotBe(mixedSame.GetHashCode());

            mixedOrig.GetHashCode().Should().Be(mixedSame.GetHashCode());
            mixedOrig.GetHashCode().Should().Be(mixedDiff.GetHashCode(), "SymbolicValue produces constant hash code");
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
            sut.ToString().Should().BeIgnoringLineEndings(
@"Symbols:
a: <null>
");

            sut = ProgramState.Empty.SetSymbolValue(variableSymbol, new SymbolicValue(counter));
            sut.ToString().Should().BeIgnoringLineEndings(
@"Symbols:
a: SV_1
");

            var valueWithConstraint = new SymbolicValue(counter);
            valueWithConstraint.SetConstraint(TestConstraint.Second);
            sut = sut.SetSymbolValue(variableSymbol.ContainingSymbol, valueWithConstraint);
            sut.ToString().Should().BeIgnoringLineEndings(
@"Symbols:
a: SV_1
Sample.Main(): SV_2: Second
");
        }

        [TestMethod]
        public void ToString_WithOperations()
        {
            var assignment = TestHelper.CompileCfgBodyCS("var a = true;").Blocks[1].Operations[0];
            var counter = new SymbolicValueCounter();
            var sut = ProgramState.Empty.SetOperationValue(assignment, null);
            sut.ToString().Should().BeIgnoringLineEndings(
@"Operations:
SimpleAssignmentOperation / VariableDeclaratorSyntax: a = true: <null>
");

            sut = ProgramState.Empty.SetOperationValue(assignment, new SymbolicValue(counter));
            sut.ToString().Should().BeIgnoringLineEndings(
@"Operations:
SimpleAssignmentOperation / VariableDeclaratorSyntax: a = true: SV_1
");
            var valueWithConstraint = new SymbolicValue(counter);
            valueWithConstraint.SetConstraint(TestConstraint.Second);
            sut = sut.SetOperationValue(assignment.Children.First(), valueWithConstraint);
            sut.ToString().Should().BeIgnoringLineEndings(
@"Operations:
LocalReferenceOperation / VariableDeclaratorSyntax: a = true: SV_2: Second
SimpleAssignmentOperation / VariableDeclaratorSyntax: a = true: SV_1
");
        }

        [TestMethod]
        public void ToString_WithAll()
        {
            var assignment = TestHelper.CompileCfgBodyCS("var a = true;").Blocks[1].Operations[0];
            var counter = new SymbolicValueCounter();
            var variableSymbol = assignment.Children.First().TrackedSymbol();
            var valueWithConstraint = new SymbolicValue(counter);
            valueWithConstraint.SetConstraint(TestConstraint.First);
            var sut = ProgramState.Empty
                .SetSymbolValue(variableSymbol, new SymbolicValue(counter))
                .SetSymbolValue(variableSymbol.ContainingSymbol, valueWithConstraint)
                .SetOperationValue(assignment, new SymbolicValue(counter))
                .SetOperationValue(assignment.Children.First(), valueWithConstraint);

            sut.ToString().Should().BeIgnoringLineEndings(
@"Symbols:
a: SV_2
Sample.Main(): SV_1: First
Operations:
LocalReferenceOperation / VariableDeclaratorSyntax: a = true: SV_1: First
SimpleAssignmentOperation / VariableDeclaratorSyntax: a = true: SV_3
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

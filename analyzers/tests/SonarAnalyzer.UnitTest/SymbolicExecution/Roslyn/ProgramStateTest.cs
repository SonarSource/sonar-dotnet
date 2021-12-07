/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    [TestClass]
    public class ProgramStateTest
    {
        [TestMethod]
        public void SetOperationValue_ReturnsValues()
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
        public void SetOperationValue_IsImmutable()
        {
            var operation = new IOperationWrapperSonar(TestHelper.CompileCfgBodyCS("var x = 42;").Blocks[1].Operations[0]);
            var sut = ProgramState.Empty;

            sut[operation].Should().BeNull();
            sut.SetOperationValue(operation, new SymbolicValue(new SymbolicValueCounter()));
            sut[operation].Should().BeNull(nameof(sut.SetOperationValue) + " returned new ProgramState instance.");
        }

        [TestMethod]
        public void SetOperationValue_UsesUnderlyingOperation()
        {
            var operation = new IOperationWrapperSonar(TestHelper.CompileCfgBodyCS("var x = 42;").Blocks[1].Operations[0]);
            var another = new IOperationWrapperSonar(operation.Instance);
            var value = new SymbolicValue(new SymbolicValueCounter());
            var sut = ProgramState.Empty;

            sut[operation].Should().BeNull();
            sut = sut.SetOperationValue(operation, value);
            sut[another].Should().Be(value, "GetHashCode and Equals are based on underlying IOperation instance.");
        }

        [TestMethod]
        public void SetOperationValue_Overrides()
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

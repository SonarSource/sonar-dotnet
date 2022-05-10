﻿/*
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

using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    public partial class ProgramStateTest
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
            var operation = new IOperationWrapperSonar(CreateOperation());
            var sut = ProgramState.Empty;

            sut[operation].Should().BeNull();
            sut.SetOperationValue(operation, new SymbolicValue(new SymbolicValueCounter()));
            sut[operation].Should().BeNull(nameof(sut.SetOperationValue) + " returned new ProgramState instance.");
        }

        [TestMethod]
        public void SetOperationValue_IsImmutable()
        {
            var operation = CreateOperation();
            var sut = ProgramState.Empty;

            sut[operation].Should().BeNull();
            sut.SetOperationValue(operation, new SymbolicValue(new SymbolicValueCounter()));
            sut[operation].Should().BeNull(nameof(sut.SetOperationValue) + " returned new ProgramState instance.");
        }

        [TestMethod]
        public void SetOperationValue_WithWrapper_UsesUnderlyingOperation()
        {
            var operation = new IOperationWrapperSonar(CreateOperation());
            var another = new IOperationWrapperSonar(operation.Instance);
            var value = new SymbolicValue(new SymbolicValueCounter());
            var sut = ProgramState.Empty;

            sut[operation].Should().BeNull();
            sut = sut.SetOperationValue(operation, value);
            sut[another].Should().Be(value, "GetHashCode and Equals are based on underlying IOperation instance.");
            sut[another.Instance].Should().Be(value, "GetHashCode and Equals are based on underlying IOperation instance.");
        }

        [TestMethod]
        public void SetOperationValue_WithWrapper_Overwrites()
        {
            var counter = new SymbolicValueCounter();
            var value1 = new SymbolicValue(counter);
            var value2 = new SymbolicValue(counter);
            var operation = new IOperationWrapperSonar(CreateOperation());
            var sut = ProgramState.Empty;

            sut[operation].Should().BeNull();
            sut = sut.SetOperationValue(operation, value1);
            sut = sut.SetOperationValue(operation, value2);
            sut[operation].Should().Be(value2);
        }

        [TestMethod]
        public void SetOperationValue_NullValue_RemovesSymbol()
        {
            var value = new SymbolicValue(new());
            var operation = new IOperationWrapperSonar(CreateOperation());
            var sut = ProgramState.Empty;

            sut = sut.SetOperationValue(operation, value);
            sut[operation].Should().NotBeNull();
            sut = sut.SetOperationValue(operation, null);
            sut[operation].Should().BeNull();
        }

        [TestMethod]
        public void SetOperationValue_Overwrites()
        {
            var counter = new SymbolicValueCounter();
            var value1 = new SymbolicValue(counter);
            var value2 = new SymbolicValue(counter);
            var operation = CreateOperation();
            var sut = ProgramState.Empty;

            sut[operation].Should().BeNull();
            sut = sut.SetOperationValue(operation, value1);
            sut = sut.SetOperationValue(operation, value2);
            sut[operation].Should().Be(value2);
        }

        [TestMethod]
        public void SetOperationValue_NullValue_ReturnsNull()
        {
            var operation = CreateOperation();
            var sut = ProgramState.Empty;

            sut[operation].Should().BeNull();
            sut = sut.SetOperationValue(operation, null);
            sut[operation].Should().BeNull();
        }

        [TestMethod]
        public void SetOperationValue_NullOperation_Throws() =>
            ProgramState.Empty.Invoking(x => x.SetOperationValue((IOperation)null, new(new()))).Should().Throw<NullReferenceException>();

        [TestMethod]
        public void SetOperationValue_WithWrapper_NullOperation_Throws() =>
            ProgramState.Empty.Invoking(x => x.SetOperationValue((IOperationWrapperSonar)null, new(new()))).Should().Throw<NullReferenceException>();

        [TestMethod]
        public void SetOperationValue_OnCaptureReference_SetsValueToCapturedOperation()
        {
            var value = new SymbolicValue(new());
            var cfg = TestHelper.CompileCfgBodyCS("a ??= b;", "object a, object b");
            var capture = IFlowCaptureOperationWrapper.FromOperation(cfg.Blocks[1].Operations[0]);
            var captureReference = IFlowCaptureReferenceOperationWrapper.FromOperation(cfg.Blocks[3].Operations[0].Children.First());
            captureReference.Id.Should().Be(capture.Id);
            var sut = ProgramState.Empty
                .SetCapture(capture.Id, capture.Value)
                .SetOperationValue(captureReference.WrappedOperation, value);
            sut[capture.Value].Should().Be(value);
        }

        [TestMethod]
        public void ResetOperations_IsImmutable()
        {
            var operation = CreateOperation();
            var beforeReset = ProgramState.Empty.SetOperationValue(operation, new SymbolicValue(new SymbolicValueCounter()));
            beforeReset[operation].Should().NotBeNull();

            var afterReset = beforeReset.ResetOperations();
            beforeReset[operation].Should().NotBeNull();
            afterReset[operation].Should().BeNull();
        }

        [TestMethod]
        public void ResetOperations_PreservesCaptured()
        {
            var counter = new SymbolicValueCounter();
            var operations = TestHelper.CompileCfgBodyCS("var x = 0; x = 1; x = 42; x = 100;").Blocks[1].Operations;
            var beforeReset = ProgramState.Empty
                .SetOperationValue(operations[0], new(counter))
                .SetOperationValue(operations[1], new(counter))
                .SetOperationValue(operations[2], new(counter))
                .SetOperationValue(operations[3], new(counter))
                .SetCapture(new CaptureId(1), operations[1])
                .SetCapture(new CaptureId(2), operations[2]);
            beforeReset[operations[0]].Should().NotBeNull();
            beforeReset[operations[1]].Should().NotBeNull();
            beforeReset[operations[2]].Should().NotBeNull();
            beforeReset[operations[3]].Should().NotBeNull();

            var afterReset = beforeReset.ResetOperations();
            afterReset[operations[0]].Should().BeNull();
            afterReset[operations[1]].Should().NotBeNull();
            afterReset[operations[2]].Should().NotBeNull();
            afterReset[operations[3]].Should().BeNull();
        }

        [TestMethod]
        public void SetOperationConstraint_NoValue_CreatesNewValue()
        {
            var operation = new IOperationWrapperSonar(CreateOperation());
            var sut = ProgramState.Empty.SetOperationConstraint(operation, new(), DummyConstraint.Dummy);
            sut[operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue();
        }

        [TestMethod]
        public void SetOperationConstraint_ExistingValue_PreservesOtherConstraints()
        {
            var operation = new IOperationWrapperSonar(CreateOperation());
            var counter = new SymbolicValueCounter();
            var sut = ProgramState.Empty
                .SetOperationValue(operation, new SymbolicValue(counter).WithConstraint(TestConstraint.First))
                .SetOperationConstraint(operation, counter, DummyConstraint.Dummy);
            sut[operation].HasConstraint(TestConstraint.First).Should().BeTrue("original constraints of different type should be preserved");
            sut[operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue();
        }

        private static IOperation CreateOperation() =>
            TestHelper.CompileCfgBodyCS("var x = 42;").Blocks[1].Operations[0];
    }
}

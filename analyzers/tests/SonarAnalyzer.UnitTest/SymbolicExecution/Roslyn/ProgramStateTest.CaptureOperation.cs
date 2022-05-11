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
using Moq;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    public partial class ProgramStateTest
    {
        [TestMethod]
        public void SetCapture_ReturnsValues()
        {
            var operation1 = new Mock<IOperation>().Object;
            var operation2 = new Mock<IOperation>().Object;
            var captures = new[] { new CaptureId(0), new CaptureId(1), new CaptureId(2) };
            var sut = ProgramState.Empty;

            sut[captures[0]].Should().BeNull();
            sut[captures[1]].Should().BeNull();
            sut[captures[2]].Should().BeNull();

            sut = sut.SetCapture(captures[0], operation1);
            sut = sut.SetCapture(captures[1], operation2);

            sut[captures[0]].Should().Be(operation1);
            sut[captures[1]].Should().Be(operation2);
            sut[captures[2]].Should().BeNull();     // Value was not set
        }

        [TestMethod]
        public void SetCapture_IsImmutable()
        {
            var capture = new CaptureId(42);
            var sut = ProgramState.Empty;

            sut[capture].Should().BeNull();
            sut.SetCapture(capture, new Mock<IOperation>().Object);
            sut[capture].Should().BeNull(nameof(sut.SetCapture) + " returned new ProgramState instance.");
        }

        [TestMethod]
        public void SetCapture_Overwrites()
        {
            var operation1 = new Mock<IOperation>().Object;
            var operation2 = new Mock<IOperation>().Object;
            var capture = new CaptureId(42);
            var sut = ProgramState.Empty;

            sut[capture].Should().BeNull();
            sut = sut.SetCapture(capture, operation1);
            sut = sut.SetCapture(capture, operation2);
            sut[capture].Should().Be(operation2);
        }

        [TestMethod]
        public void RemoveCapture_RemovesOnlyRequested()
        {
            var operation = new Mock<IOperation>().Object;
            var captures = new[] { new CaptureId(0), new CaptureId(1), new CaptureId(2) };
            var sut = ProgramState.Empty
                .SetCapture(captures[0], operation)
                .SetCapture(captures[1], operation)
                .SetCapture(captures[2], operation)
                .RemoveCapture(captures[1]);

            sut[captures[0]].Should().Be(operation);
            sut[captures[1]].Should().BeNull();
            sut[captures[2]].Should().Be(operation);
        }

        [TestMethod]
        public void ResolveCapture_CaptureReference_ReturnsCapturedOperation()
        {
            var cfg = TestHelper.CompileCfgBodyCS("a ??= b;", "object a, object b");
            var capture = IFlowCaptureOperationWrapper.FromOperation(cfg.Blocks[1].Operations[0]);
            var captureReference = IFlowCaptureReferenceOperationWrapper.FromOperation(cfg.Blocks[3].Operations[0].Children.First());
            captureReference.Id.Should().Be(capture.Id);
            var sut = ProgramState.Empty.SetCapture(capture.Id, capture.Value);
            sut.ResolveCapture(captureReference.WrappedOperation).Should().Be(capture.Value);
        }

        [TestMethod]
        public void ResolveCapture_OtherOperation_ReturnsArgument()
        {
            var cfg = TestHelper.CompileCfgBodyCS("a ??= b;", "object a, object b");
            var capture = IFlowCaptureOperationWrapper.FromOperation(cfg.Blocks[1].Operations[0]);
            var sut = ProgramState.Empty.SetCapture(capture.Id, capture.Value);
            sut.ResolveCapture(capture.Value).Should().Be(capture.Value);   // Any other operation
        }
    }
}

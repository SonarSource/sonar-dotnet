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

using NSubstitute;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

public partial class ProgramStateTest
{
    [TestMethod]
    public void SetCapture_ReturnsValues()
    {
        var operation1 = Substitute.For<IOperation>();
        var operation2 = Substitute.For<IOperation>();
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
        sut.SetCapture(capture, Substitute.For<IOperation>());
        sut[capture].Should().BeNull(nameof(sut.SetCapture) + " returned new ProgramState instance.");
    }

    [TestMethod]
    public void SetCapture_Overwrites()
    {
        var operation1 = Substitute.For<IOperation>();
        var operation2 = Substitute.For<IOperation>();
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
        var operation = Substitute.For<IOperation>();
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
        var captureReference = cfg.Blocks[3].Operations[0].ChildOperations.First().ToFlowCaptureReference();
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

    [TestMethod]
    public void ResolveCaptureNull_ReturnsNull() =>
        ProgramState.Empty.ResolveCapture(null).Should().BeNull();

    [TestMethod]
    public void ResolveCaptureAndUnwrapConversion_CaptureReference_ReturnsCapturedOperation()
    {
        var cfg = TestHelper.CompileCfgBodyCS("a ??= b;", "object a, object b");
        var capture = IFlowCaptureOperationWrapper.FromOperation(cfg.Blocks[1].Operations[0]);
        var captureReference = cfg.Blocks[3].Operations[0].ChildOperations.First();
        var sut = ProgramState.Empty.SetCapture(capture.Id, capture.Value);
        sut.ResolveCaptureAndUnwrapConversion(captureReference).Should().Be(capture.Value);
    }

    [TestMethod]
    public void ResolveCaptureAndUnwrapConversion_CaptureReferenceInConversion_ReturnsCapturedOperation()
    {
        var cfg = TestHelper.CompileCfgBodyCS("_ = (condition ? a : null) as string;", "object a, bool condition");
        var capture = IFlowCaptureOperationWrapper.FromOperation(cfg.Blocks[2].Operations[0]);
        var conversion = cfg.Blocks[4].Operations[0].ChildOperations.First().ChildOperations.Skip(1).First();
        var sut = ProgramState.Empty.SetCapture(capture.Id, capture.Value);
        sut.ResolveCaptureAndUnwrapConversion(conversion).Should().Be(capture.Value);
    }

    [TestMethod]
    public void ResolveCaptureAndUnwrapConversion_ChainOfConversions_InnerConversionOperand()
    {
        var cfg = TestHelper.CompileCfgBodyCS("_ = (string)(object)(int)a;", "object a");
        var firstConversion = cfg.Blocks[1].Operations[0].ChildOperations.First().ChildOperations.Skip(1).First();
        var parameterReference = firstConversion.ChildOperations.First().ChildOperations.First().ChildOperations.First();
        var sut = ProgramState.Empty;
        sut.ResolveCaptureAndUnwrapConversion(firstConversion).Should().Be(parameterReference);
    }

    [TestMethod]
    public void ResolveCaptureAndUnwrapConversion_CaptureNotFound_ReturnsFlowCaptureReference()
    {
        var cfg = TestHelper.CompileCfgBodyCS("_ = (condition ? a : null) as string;", "object a, bool condition");
        var conversion = cfg.Blocks[4].Operations[0].ChildOperations.First().ChildOperations.Skip(1).First();
        var flowCaptureReference = conversion.ChildOperations.First();
        var sut = ProgramState.Empty;
        sut.ResolveCaptureAndUnwrapConversion(conversion).Should().Be(flowCaptureReference);
    }

    [TestMethod]
    public void ResolveCaptureAndUnwrapConversion_Null_ReturnsNull() =>
        ProgramState.Empty.ResolveCaptureAndUnwrapConversion(null).Should().BeNull();
}

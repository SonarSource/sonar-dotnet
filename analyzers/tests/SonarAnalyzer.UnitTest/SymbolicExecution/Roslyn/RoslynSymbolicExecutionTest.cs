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
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    [TestClass]
    public partial class RoslynSymbolicExecutionTest
    {
        [TestMethod]
        public void Constructor_Throws()
        {
            var cfg = TestHelper.CompileCfgCS("public class Sample { public void Main() { } }");
            var check = new Mock<SymbolicCheck>().Object;
            ((Action)(() => new RoslynSymbolicExecution(null, new[] { check }))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("cfg");
            ((Action)(() => new RoslynSymbolicExecution(cfg, null))).Should().Throw<ArgumentException>().WithMessage("At least one check is expected*");
            ((Action)(() => new RoslynSymbolicExecution(cfg, Array.Empty<SymbolicCheck>()))).Should().Throw<ArgumentException>().WithMessage("At least one check is expected*");
        }

        [TestMethod]
        public void Execute_SecondRun_Throws()
        {
            var cfg = TestHelper.CompileCfgBodyCS();
            var se = new RoslynSymbolicExecution(cfg, new[] { new ValidatorTestCheck() });
            se.Execute();
            se.Invoking(x => x.Execute()).Should().Throw<InvalidOperationException>().WithMessage("Engine can be executed only once.");
        }

        [TestMethod]
        public void SequentialInput_CS()
        {
            var context = SETestContext.CreateCS("var a = true; var b = false; b = !b; a = (b);");
            context.Validator.ValidateOrder(
                "LocalReference: a = true (Implicit)",
                "Literal: true",
                "SimpleAssignment: a = true (Implicit)",
                "LocalReference: b = false (Implicit)",
                "Literal: false",
                "SimpleAssignment: b = false (Implicit)",
                "LocalReference: b",
                "LocalReference: b",
                "UnaryOperator: !b",
                "SimpleAssignment: b = !b",
                "ExpressionStatement: b = !b;",
                "LocalReference: a",
                "LocalReference: b",
                "SimpleAssignment: a = (b)",
                "ExpressionStatement: a = (b);");
        }

        [TestMethod]
        public void SequentialInput_VB()
        {
            var context = SETestContext.CreateVB("Dim A As Boolean = True, B As Boolean = False : B = Not B : A = (B)");
            context.Validator.ValidateOrder(
                "LocalReference: A (Implicit)",
                "Literal: True",
                "SimpleAssignment: A As Boolean = True (Implicit)",
                "LocalReference: B (Implicit)",
                "Literal: False",
                "SimpleAssignment: B As Boolean = False (Implicit)",
                "LocalReference: B",
                "LocalReference: B",
                "UnaryOperator: Not B",
                "SimpleAssignment: B = Not B (Implicit)",
                "ExpressionStatement: B = Not B",
                "LocalReference: A",
                "LocalReference: B",
                "Parenthesized: (B)",
                "SimpleAssignment: A = (B) (Implicit)",
                "ExpressionStatement: A = (B)");
        }

        [TestMethod]
        public void PreProcess_Null_StopsExecution()
        {
            var stopper = new PreProcessTestCheck(OperationKind.Unary, x => null);
            var context = SETestContext.CreateCS("var a = true; var b = false; b = !b; a = (b);", stopper);
            context.Validator.ValidateOrder(
                "LocalReference: a = true (Implicit)",
                "Literal: true",
                "SimpleAssignment: a = true (Implicit)",
                "LocalReference: b = false (Implicit)",
                "Literal: false",
                "SimpleAssignment: b = false (Implicit)",
                "LocalReference: b",
                "LocalReference: b");
        }

        [TestMethod]
        public void PostProcess_Null_StopsExecution()
        {
            var stopper = new PostProcessTestCheck(OperationKind.Unary, x => null);
            var context = SETestContext.CreateCS("var a = true; var b = false; b = !b; a = (b);", stopper);
            context.Validator.ValidateOrder(
                "LocalReference: a = true (Implicit)",
                "Literal: true",
                "SimpleAssignment: a = true (Implicit)",
                "LocalReference: b = false (Implicit)",
                "Literal: false",
                "SimpleAssignment: b = false (Implicit)",
                "LocalReference: b",
                "LocalReference: b");
        }

        [TestMethod]
        public void PostProcess_OperationDoesNotHaveValuesByDefault()
        {
            var validator = SETestContext.CreateCS("int x = 0;").Validator;
            validator.ValidatePostProcessCount(3);
            validator.ValidateOperationValuesAreNull();
        }

        [TestMethod]
        public void Execute_PersistConstraints()
        {
            var validator = SETestContext.CreateCS("var a = true;").Validator;
            validator.ValidateOrder(    // Visualize operations
                "LocalReference: a = true (Implicit)",
                "Literal: true",
                "SimpleAssignment: a = true (Implicit)");
            validator.Validate("Literal: true", x => x.State[x.Operation].HasConstraint(BoolConstraint.True).Should().BeTrue());
            validator.Validate("SimpleAssignment: a = true (Implicit)", x => x.State[x.Operation].HasConstraint(BoolConstraint.True).Should().BeTrue());
        }

        [TestMethod]
        public void Execute_PersistSymbols_InsideBlock()
        {
            var validator = SETestContext.CreateCS("var first = true; var second = false; first = second;").Validator;
            validator.ValidateOrder(    // Visualize operations
                   "LocalReference: first = true (Implicit)",
                   "Literal: true",
                   "SimpleAssignment: first = true (Implicit)",
                   "LocalReference: second = false (Implicit)",
                   "Literal: false",
                   "SimpleAssignment: second = false (Implicit)",
                   "LocalReference: first",
                   "LocalReference: second",
                   "SimpleAssignment: first = second",
                   "ExpressionStatement: first = second;");
            validator.Validate("LocalReference: first", x => x.State[LocalReferenceOperationSymbol(x.Operation)].HasConstraint(BoolConstraint.True).Should().BeTrue());
            validator.Validate("LocalReference: second", x => x.State[LocalReferenceOperationSymbol(x.Operation)].HasConstraint(BoolConstraint.False).Should().BeTrue());

            static ISymbol LocalReferenceOperationSymbol(IOperationWrapperSonar operation) =>
                ((ILocalReferenceOperation)operation.Instance).Local;
        }

        [TestMethod]
        public void Execute_TooManyBlocks_NotSupported()
        {
            var validator = SETestContext.CreateCS($"var a = true{Enumerable.Repeat(" && true", 1020).JoinStr(null)};").Validator;
            validator.ValidateExitReachCount(0);
            validator.ValidateExecutionNotCompleted();
        }

        [TestMethod]
        public void Execute_CheckProducesMoreStates_PreProcess()
        {
            var check = new PreProcessTestCheck(x => DecorateIntLiteral(x, TestConstraint.First, TestConstraint.Second));
            SETestContext.CreateCS(@"var i = 42; Tag(""I"", i);", check).Validator.TagValues("I").Should()
                .HaveCount(2)
                .And.ContainSingle(x => x.HasConstraint(TestConstraint.First))
                .And.ContainSingle(x => x.HasConstraint(TestConstraint.Second));
        }

        [TestMethod]
        public void Execute_CheckProducesMoreStates_PostProcess()
        {
            var check = new PostProcessTestCheck(x => DecorateIntLiteral(x, TestConstraint.First, TestConstraint.Second));
            SETestContext.CreateCS(@"var i = 42; Tag(""I"", i);", check).Validator.TagValues("I").Should()
                .HaveCount(2)
                .And.ContainSingle(x => x.HasConstraint(TestConstraint.First))
                .And.ContainSingle(x => x.HasConstraint(TestConstraint.Second));
        }

        [TestMethod]
        public void Execute_CheckProducesMoreStates_Both()
        {
            var preProcess = new PreProcessTestCheck(x => DecorateIntLiteral(x, TestConstraint.First, TestConstraint.Second));
            var postProcess = new PostProcessTestCheck(x => DecorateIntLiteral(x, BoolConstraint.True, BoolConstraint.False));
            SETestContext.CreateCS(@"var i = 42; Tag(""I"", i);", preProcess, postProcess).Validator.TagValues("I").Should()
                .HaveCount(4)
                .And.ContainSingle(x => x.HasConstraint(TestConstraint.First) && x.HasConstraint(BoolConstraint.True))
                .And.ContainSingle(x => x.HasConstraint(TestConstraint.First) && x.HasConstraint(BoolConstraint.False))
                .And.ContainSingle(x => x.HasConstraint(TestConstraint.Second) && x.HasConstraint(BoolConstraint.True))
                .And.ContainSingle(x => x.HasConstraint(TestConstraint.Second) && x.HasConstraint(BoolConstraint.False));
        }

        [TestMethod]
        public void Execute_ClearsCapturesAfterBranching()
        {
            const string code = @"
string a = null;
a ??= arg;
Tag(""End"");";
            var allReferences = new List<IOperation>();
            var collector = new PostProcessTestCheck(x =>
            {
                if (x.Operation.Instance.Kind == OperationKind.FlowCaptureReference)
                {
                    allReferences.Add(x.Operation.Instance);
                }
                return x.State;
            });
            var validator = SETestContext.CreateCS(code, ", string arg", collector).Validator;
            allReferences.Should().HaveCount(3);
            var state = validator.TagStates("End").Single();
            foreach (var reference in allReferences)
            {
                state.ResolveCapture(reference).Should().Be(reference); // Not resolved, because the captures were cleared before entering the block with Tag("End")
            }
        }

        private static ProgramState[] DecorateIntLiteral(SymbolicContext context, SymbolicConstraint first, SymbolicConstraint second) =>
            context.Operation.Instance.Kind == OperationKind.Literal && context.Operation.Instance.ConstantValue.Value is int
                ? new[] { context.SetOperationConstraint(first), context.SetOperationConstraint(second) }
                : new[] { context.State };

        [TestMethod]
        public void Execute_LocalScopeRegion_AssignDefaultBoolConstraint() =>
            SETestContext.CreateVB(@"Dim B As Boolean : Tag(""B"", B)").Validator.ValidateTag("B", x => x.HasConstraint(BoolConstraint.False).Should().BeTrue());
    }
}

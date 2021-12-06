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

using System;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    [TestClass]
    public class RoslynSymbolicExecutionTest
    {
        [TestMethod]
        public void Constructor_Throws()
        {
            var cfg = TestHelper.CompileCfgCS("public class Sample { public void Main() { } }");
            var check = new Mock<SymbolicExecutionCheck>().Object;
            ((Action)(() => new RoslynSymbolicExecution(null, new[] { check }))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("cfg");
            ((Action)(() => new RoslynSymbolicExecution(cfg, null))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("checks");
            ((Action)(() => new RoslynSymbolicExecution(cfg, Array.Empty<SymbolicExecutionCheck>()))).Should().Throw<ArgumentException>().WithMessage("At least one check is expected*");
        }

        [TestMethod]
        public void SequentialInput_CS()
        {
            var context = SETestContext.CreateCS("var a = true; var b = false; b = !b; a = (b);");
            context.Collector.ValidateOrder(
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
            context.Collector.ValidateOrder(
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
            var stopper = new PreProcessTestCheck((state, operation) => operation.Instance.Kind == OperationKind.Unary ? null : state);
            var context = SETestContext.CreateCS("var a = true; var b = false; b = !b; a = (b);", stopper);
            context.Collector.ValidateOrder(
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
            var stopper = new PostProcessTestCheck((state, operation) => operation.Instance.Kind == OperationKind.Unary ? null : state);
            var context = SETestContext.CreateCS("var a = true; var b = false; b = !b; a = (b);", stopper);
            context.Collector.ValidateOrder(
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
        public void PostProcess_OperationHasValue()
        {
            var collector = SETestContext.CreateCS("var a = true;").Collector;
            collector.PostProcessed.Should().HaveCount(3);
            foreach (var (state, operation) in collector.PostProcessed)
            {
                state[operation].Should().NotBeNull();
            }
        }

        [TestMethod]
        public void Execute_PersistConstraints()
        {
            var setter = new PreProcessTestCheck((state, operation) =>
                {
                    if (operation.Instance.Kind == OperationKind.Literal)
                    {
                        state[operation].SetConstraint(DummyConstraint.Dummy);
                    }
                    return state;
                });
            var collector = SETestContext.CreateCS("var a = true;", setter).Collector;
            collector.ValidateOrder(    // Visualize operations
                "LocalReference: a = true (Implicit)",
                "Literal: true",
                "SimpleAssignment: a = true (Implicit)");
            collector.Validate("Literal: true", (state, operation) => state[operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
            collector.Validate("SimpleAssignment: a = true (Implicit)", (state, operation) => state[operation].HasConstraint(DummyConstraint.Dummy).Should().BeFalse());
        }

        [TestMethod]
        public void Execute_PersistSymbols()
        {
            var setter = new PreProcessTestCheck((state, operation) =>
            {
                // Set constraint to local symbol declarations. To assert them when they are used later.
                if (operation.Instance is ILocalReferenceOperation local && operation.IsImplicit)
                {
                    var sv = new SymbolicValue(new SymbolicValueCounter()); // ToDo: Improve check design
                    sv.SetConstraint(local.Local.Name switch
                    {
                        "first" => TestConstraint.First,
                        "second" => TestConstraint.Second,
                        _ => throw new InvalidOperationException("Unexpected local variable name: " + local.Local.Name)
                    });
                    return state.SetSymbolValue(local.Local, sv);
                }
                else
                {
                    return state;
                }
            });
            var collector = SETestContext.CreateCS("var first = true; var second = false; first = second;", setter).Collector;
            collector.ValidateOrder(    // Visualize operations
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
            collector.Validate("LocalReference: first", (state, operation) => state[LocalReferenceOperationSymbol(operation)].HasConstraint(TestConstraint.First).Should().BeTrue());
            collector.Validate("LocalReference: second", (state, operation) => state[LocalReferenceOperationSymbol(operation)].HasConstraint(TestConstraint.Second).Should().BeTrue());

            static ISymbol LocalReferenceOperationSymbol(IOperationWrapperSonar operation) =>
                ((ILocalReferenceOperation)operation.Instance).Local;
        }
    }
}

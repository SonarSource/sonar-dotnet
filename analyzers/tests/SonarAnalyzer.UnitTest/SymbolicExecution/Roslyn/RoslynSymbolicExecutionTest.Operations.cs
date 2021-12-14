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

using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    public partial class RoslynSymbolicExecutionTest
    {
        [TestMethod]
        public void SimpleAssignment_ToLocalVariable_FromLiteral()
        {
            var setter = new LiteralDummyTestCheck();
            var collector = SETestContext.CreateCS(@"var a = true; Tag(""a"", a);", setter).Collector;
            collector.Validate("Literal: true", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
            collector.Validate("SimpleAssignment: a = true (Implicit)", x => x.State[((ISimpleAssignmentOperation)x.Operation.Instance).Target].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
            collector.ValidateTag("a", x => x.HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
        }

        [TestMethod] // SimpleAssignment_ToLocalVariable_FromLocalVariable
        public void SimpleAssignment_ToLocalVariable_FromTrackedSymbol()
        {
            var setter = new LiteralDummyTestCheck();
            var collector = SETestContext.CreateCS(@"var a = true; bool b; b = a; Tag(""b"", b);", setter).Collector;
            collector.Validate("Literal: true", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
            collector.Validate("SimpleAssignment: a = true (Implicit)", x => x.State[((ISimpleAssignmentOperation)x.Operation.Instance).Target].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
            collector.ValidateTag("b", x => x.HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
        }

        [TestMethod]
        public void SimpleAssignment_ToLocalVariable_FromTrackedSymbol_Chained()
        {
            var setter = new LiteralDummyTestCheck();
            var collector = SETestContext.CreateCS(@"var a = true; bool b,c; c = b = a; Tag(""c"", c);", setter).Collector;
            collector.Validate("Literal: true", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
            collector.Validate("SimpleAssignment: a = true (Implicit)", x => x.State[((ISimpleAssignmentOperation)x.Operation.Instance).Target].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
            collector.ValidateTag("c", x => x.HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
        }

        [TestMethod]
        public void SimpleAssignment_ToParameter_FromLiteral()
        {
            var setter = new LiteralDummyTestCheck();
            var collector = SETestContext.CreateCS(@"boolParameter = true; Tag(""boolParameter"", boolParameter);", setter).Collector;
            collector.Validate("Literal: true", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
            collector.Validate("SimpleAssignment: boolParameter = true", x => x.State[((ISimpleAssignmentOperation)x.Operation.Instance).Target].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
            collector.ValidateTag("boolParameter", x => x.HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
        }

        [TestMethod]
        public void SimpleAssignment_ToLocalVariable_FromTrackedSymbol_CS()
        {
            var setter = new PreProcessTestCheck(x =>
            {
                if (x.Operation.Instance.Kind == OperationKind.ParameterReference)
                {
                    x.State[x.Operation].SetConstraint(DummyConstraint.Dummy);
                    return x.State.SetSymbolValue(x.Operation.Instance.TrackedSymbol(), x.State[x.Operation]);
                }
                return x.State;
            });
            var collector = SETestContext.CreateCS(@"var b = boolParameter; Tag(""b"", b);", setter).Collector;
            collector.ValidateTag("b", x => x.HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
        }

        [TestMethod]
        public void SimpleAssignment_ToLocalVariable_FromTrackedSymbol_VB()
        {
            var setter = new PreProcessTestCheck(x =>
            {
                if (x.Operation.Instance.Kind == OperationKind.ParameterReference)
                {
                    x.State[x.Operation].SetConstraint(DummyConstraint.Dummy);
                    return x.State.SetSymbolValue(x.Operation.Instance.TrackedSymbol(), x.State[x.Operation]);
                }
                return x.State;
            });
            var collector = SETestContext.CreateVB(
@"BoolParameter = True
Dim B As Boolean = BoolParameter
Tag(""B"", B)",
setter).Collector;
            collector.ValidateTag("B", x => x.HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
        }

        [DataTestMethod]
        [DataRow("Sample.StaticField", "StaticField")]
        [DataRow("Sample.StaticProperty", "StaticProperty")]
        [DataRow("arr[0]", "array")]
        [DataRow(@"dict[""key""]", "dictionary")]
        [DataRow(@"other.Property", "other")]
        [DataRow(@"this.Property", "this")]
        public void SimpleAssignment_ToUnsupported_FromLiteral(string assignmentTarget, string tagName)
        {
            var setter = new LiteralDummyTestCheck();
            var collector = SETestContext.CreateCS(
                $@"{assignmentTarget} = 42; Tag(""{tagName}"", {assignmentTarget});",
                ", byte[] arr, Dictionary<string, int> dict, Sample other",
                setter).Collector;
            collector.Validate("Literal: 42", x => x.State[x.Operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue());
            collector.ValidateTag(tagName, x => x.Should().BeNull());
        }
    }
}

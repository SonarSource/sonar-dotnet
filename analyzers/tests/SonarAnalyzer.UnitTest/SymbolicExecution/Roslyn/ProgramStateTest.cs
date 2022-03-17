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

using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.Helpers;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

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
            var reusedValue = new SymbolicValue(counter).WithConstraint(TestConstraint.First);
            var anotherValue = new SymbolicValue(counter).WithConstraint(TestConstraint.Second);
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
            withOperationOrig.GetHashCode().Should().NotBe(withOperationDiff.GetHashCode());
            withOperationOrig.GetHashCode().Should().NotBe(withSymbolSame.GetHashCode());
            withOperationOrig.GetHashCode().Should().NotBe(mixedSame.GetHashCode());

            withSymbolOrig.GetHashCode().Should().Be(withSymbolSame.GetHashCode());
            withSymbolOrig.GetHashCode().Should().NotBe(withSymbolDiff.GetHashCode());
            withSymbolOrig.GetHashCode().Should().NotBe(mixedSame.GetHashCode());

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
                .SetOperationValue(assignment.Children.First(), valueWithConstraint);

            sut.ToString().Should().BeIgnoringLineEndings(
@"Symbols:
a: SV_3
Sample.Main(): SV_2: First
Operations:
LocalReferenceOperation / VariableDeclaratorSyntax: a = true: SV_2: First
SimpleAssignmentOperation / VariableDeclaratorSyntax: a = true: SV_4
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

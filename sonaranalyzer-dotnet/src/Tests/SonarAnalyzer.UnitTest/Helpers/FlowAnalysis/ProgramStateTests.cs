/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class ProgramStateTests
    {
        private class FakeConstraint : SymbolicValueConstraint
        {
            public override SymbolicValueConstraint OppositeForLogicalNot => null;
        }

        private static ISymbol GetSymbol()
        {
            string testInput = "var a = true; var b = false; b = !b; a = (b);";
            SemanticModel semanticModel;
            var method = ControlFlowGraphTest.CompileWithMethodBody(string.Format(ControlFlowGraphTest.TestInput, testInput), "Bar", out semanticModel);
            return semanticModel.GetDeclaredSymbol(method);
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ProgramState_Equivalence()
        {
            var ps1 = new ProgramState();
            var ps2 = new ProgramState();

            var sv = new SymbolicValue();
            var constraint = new FakeConstraint();
            var symbol = GetSymbol();
            ps1 = ps1.StoreSymbolicValue(symbol, sv);
            ps1 = sv.SetConstraint(constraint, ps1);
            ps2 = ps2.StoreSymbolicValue(symbol, sv);
            ps2 = sv.SetConstraint(constraint, ps2);

            ps2.Should().Be(ps1);
            ps2.GetHashCode().Should().Be(ps1.GetHashCode());
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ProgramState_Diff_SymbolicValue()
        {
            var ps1 = new ProgramState();
            var ps2 = new ProgramState();

            var symbol = GetSymbol();
            ps1 = ps1.StoreSymbolicValue(symbol, new SymbolicValue());
            ps2 = ps2.StoreSymbolicValue(symbol, new SymbolicValue());

            ps2.Should().NotBe(ps1);
            ps2.GetHashCode().Should().NotBe(ps1.GetHashCode());
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ProgramState_Diff_Constraint()
        {
            var ps1 = new ProgramState();
            var ps2 = new ProgramState();

            var symbol = GetSymbol();
            var sv = new SymbolicValue();
            ps1 = ps1.StoreSymbolicValue(symbol, sv);
            ps1 = sv.SetConstraint(new FakeConstraint(), ps1);
            ps2 = ps2.StoreSymbolicValue(symbol, sv);
            ps2 = sv.SetConstraint(new FakeConstraint(), ps2);

            ps2.Should().NotBe(ps1);
            ps2.GetHashCode().Should().NotBe(ps1.GetHashCode());
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ProgramState_Diff_Symbol()
        {
            var ps1 = new ProgramState();
            var ps2 = new ProgramState();

            var sv = new SymbolicValue();
            ps1 = ps1.StoreSymbolicValue(GetSymbol(), sv);
            ps2 = ps2.StoreSymbolicValue(GetSymbol(), sv);

            ps2.Should().NotBe(ps1);
            ps2.GetHashCode().Should().NotBe(ps1.GetHashCode());
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ProgramState_Constraint()
        {
            var ps = new ProgramState();
            var sv = new SymbolicValue();
            var symbol = GetSymbol();
            var constraint = new FakeConstraint();

            ps = ps.StoreSymbolicValue(symbol, sv);
            ps = sv.SetConstraint(constraint, ps);
            symbol.HasConstraint(constraint, ps).Should().BeTrue();
            symbol.HasConstraint(new FakeConstraint(), ps).Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ProgramState_NotNull_Bool_Constraint()
        {
            var ps = new ProgramState();
            var sv = new SymbolicValue();
            var symbol = GetSymbol();

            ps = ps.StoreSymbolicValue(symbol, sv);
            ps = sv.SetConstraint(BoolConstraint.True, ps);
            symbol.HasConstraint(BoolConstraint.True, ps).Should().BeTrue();
            symbol.HasConstraint(ObjectConstraint.NotNull, ps).Should().BeTrue();

            ps = ps.StoreSymbolicValue(symbol, sv);
            ps = sv.SetConstraint(BoolConstraint.False, ps);
            symbol.HasConstraint(BoolConstraint.False, ps).Should().BeTrue();
            symbol.HasConstraint(ObjectConstraint.NotNull, ps).Should().BeTrue();

            ps = ps.StoreSymbolicValue(symbol, sv);
            ps = sv.SetConstraint(ObjectConstraint.NotNull, ps);
            symbol.HasConstraint(BoolConstraint.False, ps).Should().BeTrue();
            symbol.HasConstraint(ObjectConstraint.NotNull, ps).Should().BeTrue();
        }
    }
}

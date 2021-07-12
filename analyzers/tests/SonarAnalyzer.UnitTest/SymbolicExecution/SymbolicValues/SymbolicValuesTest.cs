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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.SymbolicValues;
using ComparisonKind = SonarAnalyzer.SymbolicExecution.SymbolicValues.ComparisonKind;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.SymbolicValues
{
    [TestClass]
    public class SymbolicValuesTest
    {
        [TestMethod]
        public void Prebuilt_StringRepresentation()
        {
            SymbolicValue.This.ToString().Should().Be("SV_THIS");
            SymbolicValue.Base.ToString().Should().Be("SV_BASE");
            SymbolicValue.Null.ToString().Should().Be("SV_NULL");
        }

        [TestMethod]
        public void Custom_StringRepresentation()
        {
            var sv = new SymbolicValue();
            sv.ToString().Should().StartWith("SV_");
        }

        [TestMethod]
        public void AndConstraint_ToString() =>
            new AndSymbolicValue(SymbolicValue.True, SymbolicValue.False).ToString().Should().Be("SV_True & SV_False");

        [TestMethod]
        public void AndConstraint_ForNull_ToString() =>
            new AndSymbolicValue(null, null).ToString().Should().Be(" & ");

        [TestMethod]
        public void OrConstraint_ToString() =>
            new OrSymbolicValue(SymbolicValue.True, SymbolicValue.False).ToString().Should().Be("SV_True | SV_False");

        [TestMethod]
        public void OrConstraint_ForNull_ToString() =>
            new OrSymbolicValue(null, null).ToString().Should().Be(" | ");

        [TestMethod]
        public void XorConstraint_ToString() =>
            new XorSymbolicValue(SymbolicValue.True, SymbolicValue.False).ToString().Should().Be("SV_True ^ SV_False");

        [TestMethod]
        public void XorConstraint_ForNull_ToString() =>
            new XorSymbolicValue(null, null).ToString().Should().Be(" ^ ");

        [TestMethod]
        public void ComparisonConstraint_ToString_Less() =>
            new ComparisonSymbolicValue(ComparisonKind.Less, SymbolicValue.True, SymbolicValue.False).ToString().Should().Be("<(SV_True, SV_False)");

        [TestMethod]
        public void ComparisonConstraint_ToString_LessOrEqual() =>
            new ComparisonSymbolicValue(ComparisonKind.LessOrEqual, SymbolicValue.True, SymbolicValue.False).ToString().Should().Be("<=(SV_True, SV_False)");
    }
}

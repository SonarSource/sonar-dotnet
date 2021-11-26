﻿/*
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    [TestClass]
    public class RoslynSymbolicExecutionTest
    {
        [TestMethod]
        public void Constructor_Throws()
        {
            var cfg = TestHelper.CompileCfg("public class Sample { public void Main() { } }");
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
                "Literal: true",
                "Literal: false",
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
                "Literal: True",
                "Literal: False",
                "LocalReference: B",
                "LocalReference: B",
                "UnaryOperator: Not B",
                "ExpressionStatement: B = Not B",
                "LocalReference: A",
                "LocalReference: B",
                "Parenthesized: (B)",
                "ExpressionStatement: A = (B)");
        }
    }
}

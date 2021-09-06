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

using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.UnitTest.CFG.Roslyn
{
    [TestClass]
    public class CfgAllPathValidatorTest
    {
        [TestMethod]
        public void ValidateCfgPaths()
        {
            const string code = @"
public class Sample
{
    public int Method2(bool condition)
    {
        if (condition)
            return 42;
        else
            return 1;
    }
}";
            var cfg = TestHelper.CompileCfg(code);
            var validator = new CfgValidate(cfg, new List<int> { 3, 4 });
            validator.CheckAllPaths().Should().BeTrue();
            var emptyValidator = new EmptyValidator(cfg);
            emptyValidator.CheckAllPaths().Should().BeFalse();
        }

        private class EmptyValidator : CfgAllPathValidator
        {
            public EmptyValidator(ControlFlowGraph cfg) : base(cfg)
            {
            }
        }

        private class CfgValidate : CfgAllPathValidator
        {
            private readonly List<int> invalidBlocks;

            public CfgValidate(ControlFlowGraph cfg, List<int> invalidBlocks) : base(cfg)
            {
                this.invalidBlocks = invalidBlocks;
            }

            protected override bool IsBlockValid(BasicBlock block) =>
                !invalidBlocks.Contains(block.Ordinal);

            protected override bool IsBlockInvalid(BasicBlock block) =>
                invalidBlocks.Contains(block.Ordinal);
        }
    }
}

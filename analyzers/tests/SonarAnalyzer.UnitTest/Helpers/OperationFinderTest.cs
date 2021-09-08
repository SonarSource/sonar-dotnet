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
using Microsoft.CodeAnalysis.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.CFG.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class OperationFinderTest
    {
        [TestMethod]
        public void ValidateFinder()
        {
            const string code = @"
public class Sample
{
    int field;

    public void Method(bool condition)
    {
        if (condition)
            field = 42 + 43;
    }
}";
            var cfg = TestHelper.CompileCfg(code);
            var assign = cfg.Blocks[2];
            var finder = new FirstNumericLiteralFinder();
            finder.TryFind(cfg.EntryBlock, out var result).Should().BeFalse();
            result.Should().Be(default);
            finder.TryFind(assign, out result).Should().BeTrue();
            result.Should().Be(42);
        }

        private class FirstNumericLiteralFinder : OperationFinder<int>
        {
            protected override bool TryFindOperation(IOperationWrapperSonar operation, out int result)
            {
                if (operation.Instance is ILiteralOperation)
                {
                    result = (int)operation.Instance.ConstantValue.Value;
                    return true;
                }
                result = default;
                return false;
            }
        }
    }
}

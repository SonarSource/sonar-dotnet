/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.Operations;
using SonarAnalyzer.CFG.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Test.Helpers;

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
        var cfg = TestHelper.CompileCfgCS(code);
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

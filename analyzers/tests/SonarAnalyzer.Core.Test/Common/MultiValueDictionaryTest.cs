/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Test.Common;

[TestClass]
public class MultiValueDictionaryTest
{
    [TestMethod]
    public void MultiValueDictionary_Add()
    {
        var mvd = new MultiValueDictionary<int, int>
        {
            { 5, 42 },
            { 5, 42 },
            { 42, 42 }
        };

        mvd.Keys.Should().HaveCount(2);
        mvd[5].Should().HaveCount(2);
    }

    [TestMethod]
    public void MultiValueDictionary_Add_Set()
    {
        var mvd = MultiValueDictionary<int, int>.Create<HashSet<int>>();
        mvd.Add(5, 42);
        mvd.Add(5, 42);
        mvd.Add(42, 42);

        mvd.Keys.Should().HaveCount(2);
        mvd[5].Should().ContainSingle();
    }

    [TestMethod]
    public void MultiValueDictionary_AddRange()
    {
        var mvd = new MultiValueDictionary<int, int>();
        mvd.AddRangeWithKey(5, new[] { 42, 42 });

        mvd[5].Should().HaveCount(2);
    }
}

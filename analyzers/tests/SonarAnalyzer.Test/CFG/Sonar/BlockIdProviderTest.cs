/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using SonarAnalyzer.CFG.Sonar;

namespace SonarAnalyzer.Test.CFG.Sonar;

[TestClass]
public class BlockIdProviderTest
{
    private BlockIdProvider blockId;

    [TestInitialize]
    public void TestInitialize()
    {
        blockId = new BlockIdProvider();
    }

    [TestMethod]
    public void Get_Returns_Same_Id_For_Same_Block()
    {
        var block = new TemporaryBlock();

        blockId.Get(block).Should().Be("0");
        blockId.Get(block).Should().Be("0");
        blockId.Get(block).Should().Be("0");
    }

    [TestMethod]
    public void Get_Returns_Different_Id_For_Different_Block()
    {
        var id1 = blockId.Get(new TemporaryBlock());
        var id2 = blockId.Get(new TemporaryBlock());
        var id3 = blockId.Get(new TemporaryBlock());

        id1.Should().Be("0");
        id2.Should().Be("1");
        id3.Should().Be("2");
    }
}

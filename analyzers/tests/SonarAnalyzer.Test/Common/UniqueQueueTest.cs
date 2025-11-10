/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using System.Collections;
using SonarAnalyzer.CFG.Common;

namespace SonarAnalyzer.Test.Common;

[TestClass]
public class UniqueQueueTest
{
    [TestMethod]
    public void Enqueue_UniqueItems()
    {
        var sut = new UniqueQueue<int>();
        sut.Enqueue(42);
        sut.Dequeue().Should().Be(42);
        sut.Enqueue(42);
        sut.Dequeue().Should().Be(42, "second enqueue of dequeued item should not prevent it from being enqueued again");
    }

    [TestMethod]
    public void Dequeue_UniqueItems()
    {
        var sut = new UniqueQueue<int>();
        sut.Enqueue(41);
        sut.Enqueue(42);
        sut.Enqueue(42);
        sut.Enqueue(43);
        sut.Enqueue(42);
        sut.Dequeue().Should().Be(41);
        sut.Enqueue(42);
        sut.Dequeue().Should().Be(42);
        sut.Dequeue().Should().Be(43);
        sut.Invoking(x => x.Dequeue()).Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public void GetEnumerator()
    {
        var sut = new UniqueQueue<int>();
        ((IEnumerable)sut).GetEnumerator().Should().NotBeNull();    // For coverage
    }
}

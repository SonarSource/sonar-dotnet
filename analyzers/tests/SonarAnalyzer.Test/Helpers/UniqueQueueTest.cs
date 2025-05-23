﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using SonarAnalyzer.CFG.Helpers;

namespace SonarAnalyzer.Test.Helpers
{
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
    }
}

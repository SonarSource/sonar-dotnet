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

using System.Collections;

namespace SonarAnalyzer.CFG.Helpers;

internal class UniqueQueue<T> : IEnumerable<T>
{
    private readonly Queue<T> queue = new Queue<T>();
    private readonly ISet<T> unique = new HashSet<T>();

    public void Enqueue(T item)
    {
        if (!unique.Contains(item))
        {
            queue.Enqueue(item);
            unique.Add(item);
        }
    }

    public T Dequeue()
    {
        var ret = queue.Dequeue();
        unique.Remove(ret);
        return ret;
    }

    public IEnumerator<T> GetEnumerator() =>
        queue.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}

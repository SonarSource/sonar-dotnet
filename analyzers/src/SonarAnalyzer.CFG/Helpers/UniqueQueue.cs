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

using System.Collections;
using System.Collections.Generic;

namespace SonarAnalyzer.CFG.Helpers
{
    public class UniqueQueue<T> : IEnumerable<T>
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
}

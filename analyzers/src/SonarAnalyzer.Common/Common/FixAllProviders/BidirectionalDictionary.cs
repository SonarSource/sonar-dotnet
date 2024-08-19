/*
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

namespace SonarAnalyzer.Common
{
    internal class BidirectionalDictionary<TA, TB>
    {
        private readonly IDictionary<TA, TB> aToB = new Dictionary<TA, TB>();
        private readonly IDictionary<TB, TA> bToA = new Dictionary<TB, TA>();

        public ICollection<TA> AKeys => aToB.Keys;

        public ICollection<TB> BKeys => bToA.Keys;

        public void Add(TA a, TB b)
        {
            if (aToB.ContainsKey(a) || bToA.ContainsKey(b))
            {
                throw new ArgumentException("An element with the same key already exists in the BidirectionalDictionary.");
            }

            aToB.Add(a, b);
            bToA.Add(b, a);
        }

        public TB GetByA(TA a) => aToB[a];

        public TA GetByB(TB b) => bToA[b];

        public bool ContainsKeyByA(TA a) =>
            aToB.ContainsKey(a);

        public bool ContainsKeyByB(TB b) =>
            bToA.ContainsKey(b);
    }
}

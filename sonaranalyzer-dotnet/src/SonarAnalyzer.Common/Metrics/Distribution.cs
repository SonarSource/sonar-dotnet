/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace SonarAnalyzer.Common
{
    public class Distribution
    {
        internal static readonly IEnumerable<int> FileComplexityRange = ImmutableArray.Create(0, 5, 10, 20, 30, 60, 90);
        internal static readonly IEnumerable<int> FunctionComplexityRange = ImmutableArray.Create(1, 2, 4, 6, 8, 10, 12);

        internal ImmutableArray<int> Ranges { private set; get; }

        public IList<int> Values { private set; get; }

        public Distribution(IEnumerable<int> ranges)
        {
            if (ranges == null)
            {
                throw new ArgumentNullException(nameof(ranges));
            }

            Ranges = ranges.OrderBy(i => i).ToImmutableArray();
            Values = new int[Ranges.Length];
        }

        public Distribution Add(int value)
        {
            var i = Ranges.Length - 1;

            while (i > 0 && value < Ranges[i])
            {
                i--;
            }

            Values[i]++;

            return this;
        }

        public override string ToString()
        {
            return string.Join(";",
                Ranges.Zip(Values, (r, v) => string.Format(CultureInfo.InvariantCulture, "{0}={1}", r, v)));
        }
    }
}

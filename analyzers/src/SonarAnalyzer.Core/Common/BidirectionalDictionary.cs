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

namespace SonarAnalyzer.Core.Common;

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

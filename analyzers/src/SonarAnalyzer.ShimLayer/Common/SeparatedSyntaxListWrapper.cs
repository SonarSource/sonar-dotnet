/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

namespace SonarAnalyzer.ShimLayer.Common;

public sealed class SeparatedSyntaxListWrapper<TItem> : IReadOnlyList<TItem>
{
    private readonly TItem[] items;
    private readonly SyntaxToken[] separators;

    public int Count => items.Length;
    public int SeparatorCount => separators.Length;

    public TItem this[int index] => items[index];

    public SeparatedSyntaxListWrapper(IEnumerable<TItem> items, IEnumerable<SyntaxToken> separators)
    {
        this.items = items.ToArray();
        this.separators = separators.ToArray();
    }

    public int IndexOf(TItem item) =>
        Array.IndexOf(items, item);

    public SyntaxToken Separator(int index) =>
            separators[index];

    public IEnumerator<TItem> GetEnumerator() =>
        items.AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}

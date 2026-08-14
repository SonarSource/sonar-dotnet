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

namespace SonarAnalyzer.ShimLayer.Generator.Extensions;

public static class IEnumerableExtensions
{
    extension<T>(IEnumerable<T> enumerable)
    {
        /// <summary>
        /// This is <see cref="string.Join"/> as extension. It concatenates the members of the collection using the specified <paramref name="separator"/> between each member.
        /// <paramref name="selector"/> is used to convert <typeparamref name="T"/> to <see cref="string"/> for concatenation.
        /// </summary>
        public string JoinStr(string separator, Func<T, string> selector) =>
            string.Join(separator, enumerable.Select(x => selector(x)));
    }

    extension(IEnumerable<string> enumerable)
    {
        /// <summary>
        /// This is <see cref="string.Join"/> as extension. It concatenates the members of the collection using the specified <paramref name="separator"/> between each member.
        /// </summary>
        public string JoinStr(string separator) =>
            JoinStr(enumerable, separator, x => x);
    }
}

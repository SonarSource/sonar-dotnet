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

using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Primitives;
using System;

namespace SonarAnalyzer.UnitTest.Helpers
{
    internal static class FluentTestHelper
    {
        private const string WindowsLineEnding = "\r\n";
        private const string UnixLineEnding = "\n";

        public static void OnlyContain<T, TAssertions>(this SelfReferencingCollectionAssertions<T, TAssertions> self, params T[] expected)
             where TAssertions : SelfReferencingCollectionAssertions<T, TAssertions>
        {
            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            self.Subject
                .Should().HaveSameCount(expected)
                .And.Contain(expected);
        }

        public static void OnlyContainInOrder<T, TAssertions>(this SelfReferencingCollectionAssertions<T, TAssertions> self, params T[] expected)
             where TAssertions : SelfReferencingCollectionAssertions<T, TAssertions>
        {
            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            self.Subject
                .Should().HaveSameCount(expected)
                .And.ContainInOrder(expected);
        }

        public static void BeIgnoringLineEndings(this StringAssertions stringAssertions, string expected)
        {
            stringAssertions.Subject.ToLinuxLineEndings().Should().Be(expected.ToLinuxLineEndings());
        }

        // This allows to deal with multiple line endings
        private static string ToLinuxLineEndings(this string str) =>
            str.Replace(WindowsLineEnding, UnixLineEnding);
    }
}

/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using FluentAssertions.Collections;
using FluentAssertions.Primitives;

namespace SonarAnalyzer.UnitTest.Helpers
{
    internal static class FluentTestHelper
    {
        public static void OnlyContain<T, TAssertions>(this SelfReferencingCollectionAssertions<T, TAssertions> self, params T[] expected)
             where TAssertions : SelfReferencingCollectionAssertions<T, TAssertions>
        {
            _ = expected ?? throw new ArgumentNullException(nameof(expected));
            self.Subject.Should().Contain(expected).And.HaveSameCount(expected);
        }

        public static void OnlyContainInOrder<T, TAssertions>(this SelfReferencingCollectionAssertions<T, TAssertions> self, params T[] expected)
             where TAssertions : SelfReferencingCollectionAssertions<T, TAssertions>
        {
            _ = expected ?? throw new ArgumentNullException(nameof(expected));
            self.Subject.Should().ContainInOrder(expected).And.BeEquivalentTo(expected);    // BeEquivalentTo to have better collection message
        }

        public static void BeIgnoringLineEndings(this StringAssertions stringAssertions, string expected) =>
            stringAssertions.Subject.ToUnixLineEndings().Should().Be(expected.ToUnixLineEndings());
    }
}

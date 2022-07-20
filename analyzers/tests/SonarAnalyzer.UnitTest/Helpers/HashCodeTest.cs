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

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class HashCodeTest
    {
        [DataTestMethod]
        [DataRow(null)]
        [DataRow("Lorem Ipsum")]
        public void Combine_ProducesDifferentResults(string input)
        {
            var hash2 = SonarAnalyzer.Helpers.HashCode.Combine(input, input);
            var hash3 = SonarAnalyzer.Helpers.HashCode.Combine(input, input, input);
            var hash4 = SonarAnalyzer.Helpers.HashCode.Combine(input, input, input, input);
            var hash5 = SonarAnalyzer.Helpers.HashCode.Combine(input, input, input, input, input);

            hash2.Should().NotBe(0);
            hash3.Should().NotBe(0).And.NotBe(hash2);
            hash4.Should().NotBe(0).And.NotBe(hash2).And.NotBe(hash3);
            hash5.Should().NotBe(0).And.NotBe(hash2).And.NotBe(hash3).And.NotBe(hash4);
        }
    }
}

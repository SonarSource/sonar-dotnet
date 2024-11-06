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

using SonarAnalyzer.Core.Common;

namespace SonarAnalyzer.Core.Test.Common;

[TestClass]
public class ShannonEntropyTest
{
    [DataTestMethod]
    [DataRow(null, 0)]
    [DataRow("", 0)]
    [DataRow("a", 0)]
    [DataRow("aa", 0)]
    [DataRow("aA", 1)]
    [DataRow("abc", 1.58)]
    [DataRow("aabc", 1.5)]
    [DataRow("0000000000000000000000000000000000000000", 0)]
    [DataRow("0000000000000000000011111111111111111111", 1)]
    [DataRow("0000011111222223333344444555556666677777", 3)]
    [DataRow("squ_764ba4e9ccba193c84369792691b5500d999aadd", 3.964)]
    [DataRow("qAhEMdXy/MPwEuDlhh7O0AFBuzGvNy7AxpL3sX3q", 4.684183)]

    public void Calculate_Entropy(string input, double expectedEntropy) =>
        ShannonEntropy.Calculate(input).Should().BeApproximately(expectedEntropy, 0.01);
}

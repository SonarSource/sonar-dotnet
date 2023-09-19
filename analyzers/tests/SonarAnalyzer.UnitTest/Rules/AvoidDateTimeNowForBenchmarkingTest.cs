/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules;

[TestClass]
public class AvoidDateTimeNowForBenchmarkingTest
{
    [TestMethod]
    public void AvoidDateTimeNowForBenchmarking_CS() =>
        new VerifierBuilder<CS.AvoidDateTimeNowForBenchmarking>().AddPaths("AvoidDateTimeNowForBenchmarking.cs").Verify();

    [TestMethod]
    public void AvoidDateTimeNowForBenchmarking_VB() =>
        new VerifierBuilder<VB.AvoidDateTimeNowForBenchmarking>().AddPaths("AvoidDateTimeNowForBenchmarking.vb").Verify();

    [TestMethod]
    public void TestToBeDeleted() =>
        new VerifierBuilder<CS.AvoidDateTimeNowForBenchmarking>()
            .AddSnippet("""
int[] row0 = [1, 2, 3];
int[] row1 = [4, 5, 6];
int[] row2 = [7, 8, 9];
int[][] twoDFromVariables = [row0, row1, row2];
""").WithTopLevelStatements().WithOptions(ParseOptionsHelper.FromCSharp12).Verify();
}

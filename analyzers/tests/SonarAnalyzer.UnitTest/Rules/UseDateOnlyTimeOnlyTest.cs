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
public class UseDateOnlyTimeOnlyTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.UseDateOnlyTimeOnly>();

    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.UseDateOnlyTimeOnly>();

    [TestMethod]
    public void UseDateOnlyTimeOnly_CS() =>
        builderCS.AddPaths("UseDateOnlyTimeOnly.cs").Verify();

/*
#if NET

    [TestMethod]
    public void UseDateOnlyTimeOnly_CSharp9(string expression, bool compliant)
    {
        var code = $$"""
            using System;

            DateTime date1 = new(1993, 1, 6); // Noncompliant
            DateTime date2 = new(1, 1, 1, 12, 13, 14); // Noncompliant
            DateTime date3 = new(1993, 1, 6, 1, 1, 1); // Compliant
            """;
        var builder = builderCS.AddSnippet(code).WithTopLevelStatements();
        if (compliant)
        {
            builder.VerifyNoIssueReported();
        }
        else
        {
            builder.Verify();
        }
    }

#endif
*/

    [TestMethod]
    public void UseDateOnlyTimeOnly_VB() =>
        builderVB.AddPaths("UseDateOnlyTimeOnly.vb").Verify();
}

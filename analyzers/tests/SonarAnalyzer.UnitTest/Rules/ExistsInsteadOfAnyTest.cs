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
public class ExistsInsteadOfAnyTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ExistsInsteadOfAny>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.ExistsInsteadOfAny>();

    [TestMethod]
    public void ExistsInsteadOfAny_CS() =>
        builderCS.AddPaths("ExistsInsteadOfAny.cs").Verify();

#if NET

    [DataTestMethod]
    [DataRow("data.Any(x => x > 0);", false)]
    [DataRow("data.Append(1).Any(x => x > 0);", false)]
    [DataRow("data.Any();", true)]
    [DataRow("data.Exists(x => x > 0);", true)]
    public void ExistsInsteadOfAny_TopLevelStatements(string expression, bool compliant)
    {
        var code = $$"""
            using System.Collections.Generic;
            using System.Linq;

            var data =  new List<int>();
            {{expression}} // {{(compliant ? "Compliant" : "Noncompliant")}}
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

    [TestMethod]
    public void ExistsInsteadOfAny_VB() =>
        builderVB.AddPaths("ExistsInsteadOfAny.vb").Verify();
}

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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class UnchangedLocalVariablesShouldBeConstTest
{
    private readonly VerifierBuilder verifier = new VerifierBuilder<UnchangedLocalVariablesShouldBeConst>();

    [TestMethod]
    public void UnchangedLocalVariablesShouldBeConst() =>
        verifier.AddPaths("UnchangedLocalVariablesShouldBeConst.cs").Verify();

    [TestMethod]
    public void UnchangedLocalVariablesShouldBeConst_CSharp7() =>
        verifier.AddPaths("UnchangedLocalVariablesShouldBeConst.CSharp7.cs").WithOptions(ParseOptionsHelper.OnlyCSharp7).Verify();

#if NET

    [TestMethod]
    public void UnchangedLocalVariablesShouldBeConst_CSharp9() =>
        verifier.AddPaths("UnchangedLocalVariablesShouldBeConst.CSharp9.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void UnchangedLocalVariablesShouldBeConst_CSharp10() =>
        verifier.AddPaths("UnchangedLocalVariablesShouldBeConst.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).WithTopLevelStatements().Verify();

    [TestMethod]
    public void UnchangedLocalVariablesShouldBeConst_CSharp11() =>
        verifier.AddPaths("UnchangedLocalVariablesShouldBeConst.CSharp11.cs").WithOptions(ParseOptionsHelper.FromCSharp11).WithTopLevelStatements().Verify();

    [TestMethod]
    public void UnchangedLocalVariablesShouldBeConst_CSharp12() =>
        verifier.AddPaths("UnchangedLocalVariablesShouldBeConst.CSharp12.cs").WithOptions(ParseOptionsHelper.FromCSharp12).Verify();

#endif

    [TestMethod]
    public void UnchangedLocalVariablesShouldBeConst_InvalidCode() =>
        verifier.AddSnippet("""
            // invalid code
            public void Test_TypeThatCannotBeConst(int arg)
            {
                System.Random random = 1;
            }

            // invalid code
            public void (int arg)
            {
                int intVar = 1;
            }
            """).VerifyNoIssuesIgnoreErrors();

    [TestMethod]
    public void UnchangedLocalVariablesShouldBeConst_Fix() =>
        verifier
            .AddPaths("UnchangedLocalVariablesShouldBeConst.ToFix.cs")
            .WithCodeFixedPaths("UnchangedLocalVariablesShouldBeConst.Fixed.cs")
            .WithCodeFix<UnchangedLocalVariablesShouldBeConstCodeFix>()
            .VerifyCodeFix();
}

﻿/*
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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class ArrayPassedAsParamsTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ArrayPassedAsParams>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.ArrayPassedAsParams>();

    [TestMethod]
    public void ArrayPassedAsParams_CS() =>
        builderCS.AddPaths("ArrayPassedAsParams.cs").Verify();

#if NET

    [TestMethod]
    public void ArrayPassedAsParams_CSharp9() =>
        builderCS.AddPaths("ArrayPassedAsParams.CSharp9.cs")
            .WithTopLevelStatements()
            .Verify();

    [TestMethod]
    public void ArrayPassedAsParams_CSharp12() =>
        builderCS.AddPaths("ArrayPassedAsParams.CSharp12.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp12)
            .WithTopLevelStatements()
            .Verify();

#endif

    [TestMethod]
    public void ArrayPassedAsParams_VB() =>
        builderVB.AddPaths("ArrayPassedAsParams.vb").Verify();

    [DataTestMethod]
    [DataRow("{ }", false)]
    [DataRow("""{ "s", "s", "s", "s" }""", false)]
    [DataRow("New String(2) { }", true)]
    [DataRow("""New String(2) { "s", "s", "s" }""", false)]
    [DataRow("New String() { }", false)]
    [DataRow("""New String() { "s" }""", false)]
    public void ArrayPassedAsParams_VBCollectionInitializerSyntaxTests(string arrayCreation, bool compliant)
    {
        var code = $$"""
            Public Class C
                Public Sub M()
                    ParamsMethod({{arrayCreation}}) ' {{(compliant ? "Compliant" : "Noncompliant")}}
                End Sub

                Public Sub ParamsMethod(ParamArray p As String())
                End Sub
            End Class
            """;
        var builder = builderVB.AddSnippet(code);
        if (compliant)
        {
            builder.VerifyNoIssues();
        }
        else
        {
            builder.Verify();
        }
    }
}

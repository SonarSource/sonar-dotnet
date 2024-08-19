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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class UnusedStringBuilderTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.UnusedStringBuilder>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.UnusedStringBuilder>();

    [TestMethod]
    public void UnusedStringBuilder_CS() =>
        builderCS.AddPaths("UnusedStringBuilder.cs").Verify();

    [TestMethod]
    public void UnusedStringBuilder_VB() =>
        builderVB.AddPaths("UnusedStringBuilder.vb").WithOptions(ParseOptionsHelper.FromVisualBasic14).Verify();

#if NET

    [TestMethod]
    public void UnusedStringBuilder_CSharp9() =>
        builderCS.AddPaths("UnusedStringBuilder.CSharp9.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp9)
            .Verify();

    [DataTestMethod]
    [DataRow("", false)]
    [DataRow("sb.ToString();", true)]
    [DataRow("""var a = sb.Append("").Append("").Append("").Append("").ToString().ToLower();""", true)]
    [DataRow("sb.CopyTo(0, new char[1], 0, 1);", true)]
    [DataRow("sb.GetChunks();", true)]
    [DataRow("var a = sb[0];", true)]
    [DataRow("""sb?.Append("").ToString().ToLower();""", false)] // FP see https://github.com/SonarSource/sonar-dotnet/issues/6743
    [DataRow("sb?.ToString().ToLower();", false)] // FP
    [DataRow("""@sb.Append("").ToString();""", true)]
    [DataRow("sb.Remove(sb.Length - 1, 1);", true)]
    [DataRow("var a = sb.Length;", true)]
    [DataRow("var a = sb.Capacity;", false)]
    [DataRow("var a = sb.MaxCapacity;", false)]
    [DataRow("""var a = $"{sb} is ToStringed here";""", true)]
    [DataRow("var a = sb;", true)]
    public void UnusedStringBuilder_TopLevelStatements(string expression, bool compliant)
    {
        var code = $$"""
            using System.Text;

            var sb = new StringBuilder(); // {{(compliant ? "Compliant" : "Noncompliant")}}
            {{expression}}
            """;
        var builder = builderCS.AddSnippet(code).WithTopLevelStatements();
        if (compliant)
        {
            builder.VerifyNoIssues();
        }
        else
        {
            builder.Verify();
        }
    }

#endif

    [DataTestMethod]
    [DataRow("", false)]
    [DataRow("sb.ToString();", true)]
    [DataRow("""var a = sb.Append("").Append("").Append("").Append("").ToString().ToLower();""", true)]
    [DataRow("sb.CopyTo(0, new char[1], 0, 1);", true)]
    [DataRow("var a = sb[0];", true)]
    [DataRow("""sb?.Append("").ToString().ToLower();""", false)] // FP see https://github.com/SonarSource/sonar-dotnet/issues/6743
    [DataRow("sb?.ToString();", false)] // FP
    [DataRow("""@sb.Append("").ToString();""", true)]
    [DataRow("sb.Remove(sb.Length - 1, 1);", true)]
    [DataRow("var a = sb.Length;", true)]
    [DataRow("var a = sb.Capacity;", false)]
    [DataRow("var a = sb.MaxCapacity;", false)]
    [DataRow("""var a = $"{sb} is ToStringed here";""", true)]
    [DataRow("var a = sb;", true)]

#if NET

    [DataRow("sb.GetChunks();", true)]

#endif

    public void UnusedStringBuilder_CSExpressionsTest(string expression, bool compliant)
    {
        var code = $$"""
            using System.Text;

            public class MyClass
            {
                public void MyMethod()
                {
                    var sb = new StringBuilder(); // {{(compliant ? "Compliant" : "Noncompliant")}}
                    {{expression}}
                }
            }
            """;
        var builder = builderCS.AddSnippet(code);
        if (compliant)
        {
            builder.VerifyNoIssues();
        }
        else
        {
            builder.Verify();
        }
    }

    [DataTestMethod]
    [DataRow("", false)]
    [DataRow("sb.ToString()", true)]
    [DataRow("""Dim a = sb.Append("").Append("").Append("").Append("").ToString().ToLower()""", true)]
    [DataRow("sb.CopyTo(0, New Char(0) {}, 0, 1)", true)]
    [DataRow("Dim a = sb(0)", true)]
    [DataRow("""sb?.Append("").ToString().ToLower()""", false)] // FP see https://github.com/SonarSource/sonar-dotnet/issues/6743
    [DataRow("sb?.ToString().ToLower()", false)] // FP
    [DataRow("""sb.Append("").ToString()""", true)]
    [DataRow("sb.Remove(sb.Length - 1, 1)", true)]
    [DataRow("""Dim a = $"{sb} is ToStringed here" """, true)]
    [DataRow("Dim a = sb.Length", true)]
    [DataRow("Dim a = sb.Capacity", false)]
    [DataRow("Dim a = sb.MaxCapacity", false)]
    [DataRow("Dim a = SB.ToString()", true)]
    [DataRow("Dim a = sb.TOSTRING()", true)]
    [DataRow("Dim a = sb.LENGTH", true)]
    [DataRow("Dim a = sb", true)]

#if NET

    [DataRow("sb.GetChunks()", true)]

#endif

    public void UnusedStringBuilder_VBExpressionsTest(string expression, bool compliant)
    {
        var code = $$"""
            Imports System.Text

            Public Class [MyClass]
                Public Sub MyMethod()
                    Dim sb = New StringBuilder() ' {{(compliant ? "Compliant" : "Noncompliant")}}
                    {{expression}}
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

/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
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
    public void ArrayPassedAsParams_CS_Latest() =>
        builderCS.AddPaths("ArrayPassedAsParams.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
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

/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class ParametersCorrectOrderTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ParametersCorrectOrder>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.ParametersCorrectOrder>();

    [TestMethod]
    public void ParametersCorrectOrder_CSharp8() =>
        builderCS.AddPaths("ParametersCorrectOrder.cs").WithOptions(LanguageOptions.FromCSharp8).Verify();

#if NET

    [TestMethod]
    public void ParametersCorrectOrder_CSharp11() =>
        builderCS.AddPaths("ParametersCorrectOrder.CSharp11.cs").WithOptions(LanguageOptions.FromCSharp11).Verify();

    [TestMethod]
    public void ParametersCorrectOrder_CSharp12() =>
        builderCS.AddPaths("ParametersCorrectOrder.CSharp12.cs").WithOptions(LanguageOptions.FromCSharp12).Verify();

#endif

    [TestMethod]
    public void ParametersCorrectOrder_InvalidCode_CS() =>
        builderCS.AddSnippet("""
            public class Foo
            {
                public void Bar()
                {
                    new Foo
                    new ()
                    new System. ()
                }
            }
            """).VerifyNoIssuesIgnoreErrors();

    [TestMethod]
    public void ParametersCorrectOrder_VB() =>
        builderVB.AddPaths("ParametersCorrectOrder.vb").Verify();

    [TestMethod]
    public void ParametersCorrectOrder_InvalidCode_VB() =>
        builderVB.AddSnippet("""
            Public Class Foo
                Public Sub Bar()
                    Dim x = New ()
                    Dim y = New System. ()
                End Sub
            End Class
            """).VerifyNoIssuesIgnoreErrors();

    [TestMethod]
    public async Task ParametersCorrectOrder_SecondaryLocationsOutsideCurrentCompilation()
    {
        var library = TestCompiler.CompileCS("""
            public static class Library
            {
                public static void Method(int a, int b) { }
            }
            """).Model.Compilation;
        var usage = TestCompiler.CompileCS("""
            public class Usage
            {
                public void Method()
                {
                    int a = 4, b = 2;
                    Library.Method(b, a);
                }
            }
            """, library.ToMetadataReference()).Model.Compilation;
        var diagnostics = await usage.WithAnalyzers([new CS.ParametersCorrectOrder()]).GetAnalyzerDiagnosticsAsync();
        diagnostics.Should().ContainSingle().Which.Id.Should().Be("S2234", "we don't want AD0001 here");
    }
}

/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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
public class MethodParameterUnusedTest
{
    private readonly VerifierBuilder sonarCS = new VerifierBuilder().AddAnalyzer(() => new CS.MethodParameterUnused(AnalyzerConfiguration.AlwaysEnabledWithSonarCfg));
    private readonly VerifierBuilder roslynCS = new VerifierBuilder<CS.MethodParameterUnused>();   // Default constructor uses Roslyn CFG

    [TestMethod]
    public void MethodParameterUnused_CS_SonarCfg() =>
        sonarCS.AddPaths("MethodParameterUnused.SonarCfg.cs").Verify();

    [TestMethod]
    public void MethodParameterUnused_CS_RoslynCfg() =>
        roslynCS.AddPaths("MethodParameterUnused.RoslynCfg.cs").Verify();

#if NETFRAMEWORK

    [TestMethod]
    public void MethodParameterUnused_CS_RoslynCfg_NetFx() =>
        roslynCS.AddPaths("MethodParameterUnused.RoslynCfg.NetFx.cs").Verify();

#endif

    [TestMethod]
    public void MethodParameterUnused_CodeFix_CS() =>
        roslynCS.AddPaths("MethodParameterUnused.RoslynCfg.cs")
            .WithCodeFix<CS.MethodParameterUnusedCodeFix>()
            .WithCodeFixedPaths("MethodParameterUnused.RoslynCfg.Fixed.cs")
            .VerifyCodeFix();

    [TestMethod]
    public void MethodParameterUnused_DoubleCompilation_CS()
    {
        // https://github.com/SonarSource/sonar-dotnet/issues/5491
        const string code = @"
public class Sample
{
    private void Method(int arg) =>
        arg.ToString();
}";
        var compilation1 = roslynCS.AddSnippet(code).WithLanguageVersion(Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp7).Compile().Single();
        var compilation2 = compilation1.WithAssemblyName("Different-Compilation-Reusing-Same-Nodes");
        // Modified compilation should not reuse cached CFG, because symbols from method would not be equal to symbols from the other CFG.
        Analyze(compilation1).Should().BeEmpty();
        Analyze(compilation2).Should().BeEmpty();

        ImmutableArray<Diagnostic> Analyze(Compilation compilation) =>
            compilation.WithAnalyzers(roslynCS.Analyzers.Select(x => x()).ToImmutableArray()).GetAllDiagnosticsAsync(default).Result;
    }

    [TestMethod]
    public void MethodParameterUnused_VB() =>
        new VerifierBuilder<VB.MethodParameterUnused>().AddPaths("MethodParameterUnused.vb").WithOptions(ParseOptionsHelper.FromVisualBasic14).Verify();

#if NET

    [TestMethod]
    public void MethodParameterUnused_CS_RoslynCfg_Latest() =>
        roslynCS
            .AddPaths("MethodParameterUnused.Latest.cs")
            .WithOptions(ParseOptionsHelper.CSharpLatest)
            .Verify();

#endif

    [TestMethod]
    // https://github.com/SonarSource/sonar-dotnet/issues/8988
    public void MethodParameterUnused_GeneratedCode_CS() =>
        roslynCS
            .AddSnippet("""
                using System.CodeDom.Compiler;

                [GeneratedCode("TestTool", "Version")]
                public partial class Generated
                {
                    private partial void M(int a, int unused);
                }
                """)
            .AddSnippet("""
                using System;

                public partial class Generated
                {
                    private partial void M(int a, int unused) // Compliant
                    {
                        Console.WriteLine(a);
                    }
                }
                """)
            .WithOptions(ParseOptionsHelper.FromCSharp9)
            .VerifyNoIssues();
}

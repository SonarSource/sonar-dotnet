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
        verifier.AddSnippet("""
                            public class Test{

                                public void Message()
                                {
                                    var s1 = "Test";                              // Noncompliant {{Add the 'const' modifier to 's1', and replace 'var' with 'string'.}}
                                    string s2 = $"This is a {nameof(Message)}";   // Compliant - constant string interpolation is only valid in C# 10 and above
                                    var s3 = $"This is a {nameof(Message)}";      // Compliant - constant string interpolation is only valid in C# 10 and above
                                    var s4 = "This is a" + $" {nameof(Message)}"; // Compliant - constant string interpolation is only valid in C# 10 and above
                                    var s5 = $@"This is a {nameof(Message)}";     // Compliant - constant string interpolation is only valid in C# 10 and above
                                }
                            }
                            """)
        .WithOptions(LanguageOptions.OnlyCSharp7).Verify();

#if NET

    [TestMethod]
    public void UnchangedLocalVariablesShouldBeConst_TopLevelStatements() =>
        verifier.AddPaths("UnchangedLocalVariablesShouldBeConst.TopLevelStatements.cs")
        .WithOptions(LanguageOptions.CSharpLatest)
        .WithTopLevelStatements()
        .Verify();

    [TestMethod]
    public void UnchangedLocalVariablesShouldBeConst_Latest() =>
        verifier.AddPaths("UnchangedLocalVariablesShouldBeConst.Latest.cs")
        .WithOptions(LanguageOptions.CSharpLatest)
        .Verify();

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

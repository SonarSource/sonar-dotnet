/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class LocalFunctionShouldBeStaticTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<LocalFunctionShouldBeStatic>();

    [TestMethod]
    public void LocalFunctionShouldBeStatic() =>
        builder.AddPaths("LocalFunctionShouldBeStatic.cs").WithOptions(LanguageOptions.FromCSharp9).Verify();

    [TestMethod]
    public void LocalFunctionShouldBeStatic_CSharp7() =>
        builder.AddSnippet("""
            class Sample
            {
                void M()
                {
                    void Local() { }
                }
            }
            """).WithOptions(LanguageOptions.OnlyCSharp7).VerifyNoIssues();

    [TestMethod]
    public void LocalFunctionShouldBeStatic_ExtendedPropertyPattern() =>
        builder.AddSnippet("""
            class Sample
            {
                private const int Constant = 1;

                private Sample Other => this;
                private int Prop => 42;

                bool M(Sample sample)
                {
                    bool Check(Sample value) => value is { Other.Prop: 1 };                 // Noncompliant
                    bool Nested(Sample value) => value is { Other.Other.Prop: 1 };          // Noncompliant
                    bool ConstantField(Sample value) => value is { Other.Prop: Constant };  // Noncompliant
                    bool Capture(Sample value) => value is { Other.Prop: 1 } && Prop == 1;  // Compliant: uses an instance property
                    bool CaptureParameter() => sample is { Other.Prop: 1 };                 // Compliant: captures the enclosing parameter
                    return Check(sample) && Nested(sample) && ConstantField(sample) && Capture(sample) && CaptureParameter();
                }
            }
            """).WithOptions(LanguageOptions.FromCSharp10).Verify();

    [TestMethod]
    public void LocalFunctionShouldBeStatic_TopLevel() =>
        builder.AddSnippet("""
            int Local(int value) => value + 1; // Noncompliant {{Make this local function static.}}
            System.Console.WriteLine(Local(1));
            """).WithOptions(LanguageOptions.CSharpLatest).WithTopLevelStatements().Verify();

    [TestMethod]
    public void LocalFunctionShouldBeStatic_InvalidCode() =>
        builder.AddSnippet("""
            class Sample
            {
                void M()
                {
                    void Local( { missing
                }
            }
            """).VerifyNoAD0001();
}

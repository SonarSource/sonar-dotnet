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
public class ImplementSerializationMethodsCorrectlyTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ImplementSerializationMethodsCorrectly>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.ImplementSerializationMethodsCorrectly>();

    [TestMethod]
    public void ImplementSerializationMethodsCorrectly_CS() =>
        builderCS.AddPaths("ImplementSerializationMethodsCorrectly.cs").Verify();

#if NET

    [TestMethod]
    public void ImplementSerializationMethodsCorrectly_CSharp8() =>
        builderCS.AddPaths("ImplementSerializationMethodsCorrectly.CSharp8.cs")
            .WithOptions(LanguageOptions.FromCSharp8)
            .VerifyNoIssues();

    [TestMethod]
    public void ImplementSerializationMethodsCorrectly_CSharp9() =>
        builderCS.AddPaths("ImplementSerializationMethodsCorrectly.CSharp9.cs")
            .WithTopLevelStatements()
            .WithOptions(LanguageOptions.FromCSharp9)
            .Verify();

    [TestMethod]
    public void ImplementSerializationMethodsCorrectly_CSharp10() =>
        builderCS.AddPaths("ImplementSerializationMethodsCorrectly.CSharp10.cs")
            .WithOptions(LanguageOptions.FromCSharp10)
            .Verify();

    [TestMethod]
    public void ImplementSerializationMethodsCorrectly_CSharp11() =>
        builderCS.AddPaths("ImplementSerializationMethodsCorrectly.CSharp11.cs")
            .WithOptions(LanguageOptions.FromCSharp11)
            .Verify();

#endif

    [TestMethod]
    public void ImplementSerializationMethodsCorrectly_CS_InvalidCode() =>
        builderCS.AddSnippet("""
            [Serializable]
            public class Foo
            {
                [OnDeserializing]
                public int  { throw new NotImplementedException(); }
            }
            """)
            .VerifyNoIssuesIgnoreErrors();

    [TestMethod]
    public void ImplementSerializationMethodsCorrectly_VB() =>
        builderVB.AddPaths("ImplementSerializationMethodsCorrectly.vb").Verify();
}

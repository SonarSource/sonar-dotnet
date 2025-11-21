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
public class ImplementSerializationMethodsCorrectlyTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ImplementSerializationMethodsCorrectly>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.ImplementSerializationMethodsCorrectly>();

    [TestMethod]
    public void ImplementSerializationMethodsCorrectly_CS() =>
        builderCS.AddPaths("ImplementSerializationMethodsCorrectly.cs").Verify();

    [TestMethod]
    public void ImplementSerializationMethodsCorrectly_CSharpLatest() =>
        builderCS.AddPaths("ImplementSerializationMethodsCorrectly.Latest.cs")
            .WithTopLevelStatements()
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

    [TestMethod]
    public void ImplementSerializationMethodsCorrectly_CS_InvalidCode() =>
        builderCS.AddSnippet("""
            [System.Serializable]
            public class Foo
            {
                [System.OnDeserializing]
                // Error@+4 [CS1519] Invalid token in a member declaration
                // Error@+3 [CS1519] Invalid token in a member declaration
                // Error@+2 [CS0106] The modifier 'new' is not valid for this item
                // Error@+1 [CS1520] Method must have a return type
                public int  { throw new NotImplementedException(); }
            }   // Error [CS1022] Type or namespace definition, or end-of-file expected
            """)
            .Verify();  // This one is difficult to do in the source file due to concurrent file generation

    [TestMethod]
    public void ImplementSerializationMethodsCorrectly_VB() =>
        builderVB.AddPaths("ImplementSerializationMethodsCorrectly.vb").Verify();
}

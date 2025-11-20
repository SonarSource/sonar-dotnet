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

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class UnsafeCodeBlocksTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder().WithBasePath("Hotspots").AddAnalyzer(() => new UnsafeCodeBlocks(AnalyzerConfiguration.AlwaysEnabled));

    [TestMethod]
    public void UnsafeCodeBlocks() =>
        builder.AddPaths("UnsafeCodeBlocks.cs").Verify();

    [TestMethod]
    public void UnsafeRecord() =>
        builder.AddSnippet("""
            unsafe record MyRecord(byte* Pointer);  // Noncompliant
            // Error@-1 [CS8908]
            """)
            .WithOptions(LanguageOptions.FromCSharp9)
            .Verify();

    [TestMethod]
    public void UnsafeRecordStruct() =>
        builder.AddSnippet("""
            unsafe record struct MyRecord(byte* Pointer); // Noncompliant
            // Error@-1 [CS8908]
            """)
            .WithOptions(LanguageOptions.FromCSharp10)
            .Verify();
}

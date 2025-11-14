/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Styling.Rules.Test;

[TestClass]
public class NamespaceNameTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<NamespaceName>()
        .WithOptions(LanguageOptions.CSharpLatest)
        .AddTestReference() // For now, we raise only in UTs
        .AddSnippet("namespace SonarAnalyzer.Project.Folder.Something;", "ProductionCode.cs");   // Production code to import via using statement

    [TestMethod]
    [DataRow("SonarAnalyzer.Project.Folder.Something")]
    [DataRow("SonarAnalyzer.Project.Folder.Something.Test")]
    [DataRow("SonarAnalyzer.Project.Test.Folder.Something")]
    public void NamespaceName_Compliant(string name) =>
        builder
            .AddSnippet($"namespace {name};")   // No using to remove, no issue raised here
            .VerifyNoIssues();

    [TestMethod]
    [DataRow("SonarAnalyzer.Test.Project.Folder.Something")]    // Anywhere the .Test is
    [DataRow("SonarAnalyzer.Project.Test.Folder.Something")]
    [DataRow("SonarAnalyzer.Project.Folder.Test.Something")]
    public void NamespaceName_Noncompliant(string name) =>
        builder
            .AddSnippet($$$"""
                using SonarAnalyzer.Project.Folder.Something;
                namespace {{{name}}};   // Noncompliant {{Use SonarAnalyzer.Project.Folder.Something.Test namespace.}}
                """)
            .Verify();

    [TestMethod]
    public void NamespaceName_NoncompliantLocation() =>
        builder
            .AddSnippet("""
                using SonarAnalyzer.Project.Folder.Something;
                namespace SonarAnalyzer.Project.Test.Folder.Something;   // Noncompliant {{Use SonarAnalyzer.Project.Folder.Something.Test namespace.}}
                //        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                """)
            .Verify();

    [TestMethod]
    public void NamespaceName_ExpectedWithUsing() =>
        builder
            .AddSnippet($"""
                using SonarAnalyzer.Project.Folder.Something;           // This is useless now
                namespace SonarAnalyzer.Project.Folder.Something.Test;  // Compliant
                """)
            .VerifyNoIssues();
}

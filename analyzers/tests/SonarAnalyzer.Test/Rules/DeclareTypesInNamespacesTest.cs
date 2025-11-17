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
public class DeclareTypesInNamespacesTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<CS.DeclareTypesInNamespaces>().WithAutogenerateConcurrentFiles(false);

    [TestMethod]
    public void DeclareTypesInNamespaces_CS() =>
        builder.AddPaths("DeclareTypesInNamespaces.cs", "DeclareTypesInNamespaces2.cs").Verify();

#if NET

    [TestMethod]
    public void DeclareTypesInNamespaces_CS_Latest() =>
        builder
            .AddPaths("DeclareTypesInNamespaces.Latest.cs", "DeclareTypesInNamespaces.FileScopedNamespace.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

    [TestMethod]
    public void DeclareTypesInNamespaces_CS_TopLevelStatements() =>
        builder
            .AddPaths("DeclareTypesInNamespaces.TopLevelStatements.cs", "DeclareTypesInNamespaces.TopLevelStatements.Partial.cs")
            .WithTopLevelStatements()
            .Verify();

#endif

    [TestMethod]
    public void DeclareTypesInNamespaces_VB() =>
        new VerifierBuilder<VB.DeclareTypesInNamespaces>()
            .AddPaths("DeclareTypesInNamespaces.vb", "DeclareTypesInNamespaces2.vb")
            .WithAutogenerateConcurrentFiles(false)
            .Verify();
}

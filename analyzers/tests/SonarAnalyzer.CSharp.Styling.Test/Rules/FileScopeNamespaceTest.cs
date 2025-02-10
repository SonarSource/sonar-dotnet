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

using SonarAnalyzer.TestFramework.Common;

namespace SonarAnalyzer.CSharp.Styling.Rules.Test;

[TestClass]
public class FileScopeNamespaceTest
{
    private readonly VerifierBuilder builder = StylingVerifierBuilder.Create<FileScopeNamespace>().WithConcurrentAnalysis(false);

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void FileScopeNamespace() =>
        builder.AddPaths("FileScopeNamespace.cs").Verify();

    [TestMethod]
    public void FileScopeNamespace_TestCode() =>
        builder.AddPaths("FileScopeNamespace.cs")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Test))
            .Verify();

    [TestMethod]
    public void FileScopeNamespace_Compliant() =>
        builder.AddPaths("FileScopeNamespace.Compliant.cs").VerifyNoIssues();
}

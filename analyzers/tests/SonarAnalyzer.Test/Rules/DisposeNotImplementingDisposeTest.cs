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
public class DisposeNotImplementingDisposeTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<DisposeNotImplementingDispose>();

    [TestMethod]
    public void DisposeNotImplementingDispose() =>
        builder.AddPaths("DisposeNotImplementingDispose.cs").Verify();

    [TestMethod]
    public void DisposeNotImplementingDispose_TopLevelStatements() =>
        builder.AddPaths("DisposeNotImplementingDispose.TopLevelStatements.cs")
            .WithTopLevelStatements()
            .VerifyNoIssues();

    [TestMethod]
    public void DisposeNotImplementingDispose_Latest() =>
        builder.AddPaths("DisposeNotImplementingDispose.Latest.cs", "DisposeNotImplementingDispose.Latest.Partial.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();
}

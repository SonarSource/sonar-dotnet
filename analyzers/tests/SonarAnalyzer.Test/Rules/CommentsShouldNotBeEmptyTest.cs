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
public class CommentsShouldNotBeEmptyTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.CommentsShouldNotBeEmpty>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.CommentsShouldNotBeEmpty>();

    [TestMethod]
    public void CommentsShouldNotBeEmpty_CS() =>
        builderCS.AddPaths("CommentsShouldNotBeEmpty.cs").Verify();

    [TestMethod]
    public void CommentsShouldNotBeEmpty_CS1() =>
        builderCS
            .AddTestReference()
            .AddPaths("CommentsShouldNotBeEmpty.cs").VerifyNoIssues();

    [TestMethod]
    public void CommentsShouldNotBeEmpty_VB() =>
        builderVB.AddPaths("CommentsShouldNotBeEmpty.vb").Verify();
}

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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class MarkAssemblyWithComVisibleAttributeTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.MarkAssemblyWithComVisibleAttribute>().WithConcurrentAnalysis(false);
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.MarkAssemblyWithComVisibleAttribute>().WithConcurrentAnalysis(false);

    [TestMethod]
    public void MarkAssemblyWithComVisibleAttribute_CS() =>
        builderCS.AddPaths(@"MarkAssemblyWithComVisibleAttribute.cs").VerifyNoIssues();

    [TestMethod]
    public void MarkAssemblyWithComVisibleAttribute_VB() =>
        builderVB.AddPaths(@"MarkAssemblyWithComVisibleAttribute.vb").VerifyNoIssues();

    [TestMethod]
    public void MarkAssemblyWithComVisibleAttributeNoncompliant_CS() =>
        builderCS.AddPaths(@"MarkAssemblyWithComVisibleAttributeNoncompliant.cs").Verify();

    [TestMethod]
    public void MarkAssemblyWithComVisibleAttributeNoncompliant_VB() =>
        builderVB.AddPaths(@"MarkAssemblyWithComVisibleAttributeNoncompliant.vb").Verify();
}

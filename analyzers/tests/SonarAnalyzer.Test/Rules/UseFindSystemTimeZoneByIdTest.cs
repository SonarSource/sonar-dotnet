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

using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class UseFindSystemTimeZoneByIdTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.UseFindSystemTimeZoneById>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.UseFindSystemTimeZoneById>();

#if NET

    [TestMethod]
    public void UseFindSystemTimeZoneById_Net_CS() =>
        builderCS.AddReferences(NuGetMetadataReference.TimeZoneConverter())
            .AddPaths("UseFindSystemTimeZoneById.Net.cs")
            .Verify();

    [TestMethod]
    public void UseFindSystemTimeZoneById_Net_VB() =>
        builderVB.AddReferences(NuGetMetadataReference.TimeZoneConverter())
            .AddPaths("UseFindSystemTimeZoneById.Net.vb")
            .Verify();

    [TestMethod]
    public void UseFindSystemTimeZoneById_Net_WithoutReference_DoesNotRaise_CS() =>
        builderCS.AddPaths("UseFindSystemTimeZoneById.Net.cs")
            .WithErrorBehavior(CompilationErrorBehavior.Ignore)
            .VerifyNoIssues();

    [TestMethod]
    public void UseFindSystemTimeZoneById_Net_WithoutReference_DoesNotRaise_VB() =>
        builderVB.AddPaths("UseFindSystemTimeZoneById.Net.vb")
            .WithErrorBehavior(CompilationErrorBehavior.Ignore)
            .VerifyNoIssues();

#else

    [TestMethod]
    public void UseFindSystemTimeZoneById_CS() =>
        builderCS.AddReferences(NuGetMetadataReference.TimeZoneConverter())
            .AddPaths("UseFindSystemTimeZoneById.cs").VerifyNoIssues();

    [TestMethod]
    public void UseFindSystemTimeZoneById_VB() =>
        builderVB.AddReferences(NuGetMetadataReference.TimeZoneConverter())
            .AddPaths("UseFindSystemTimeZoneById.vb").VerifyNoIssues();

#endif

}

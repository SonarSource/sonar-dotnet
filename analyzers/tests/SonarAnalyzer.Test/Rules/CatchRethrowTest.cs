/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
public class CatchRethrowTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.CatchRethrow>();

    [TestMethod]
    public void CatchRethrow() =>
        builderCS.AddPaths("CatchRethrow.cs").Verify();

    [TestMethod]
    public void CatchRethrow_CodeFix() =>
        builderCS.AddPaths("CatchRethrow.cs")
            .WithCodeFix<CS.CatchRethrowCodeFix>()
            .WithCodeFixedPaths("CatchRethrow.Fixed.cs")
            .VerifyCodeFix();

    /// <summary>Verifies the execution-context temporary boundary.</summary>
    [TestMethod]
    public void CatchRethrow_ExecutionContext() =>
        builderCS.AddPaths("CatchRethrow.ExecutionContext.cs").Verify();

    /// <summary>Verifies that unrecognized temporary-context shapes are reported.</summary>
    [TestMethod]
    public void CatchRethrow_TemporaryContextControls() =>
        builderCS.AddPaths("CatchRethrow.TemporaryContextControls.cs").Verify();

    /// <summary>Verifies the Windows identity temporary boundary.</summary>
    [TestMethod]
    public void CatchRethrow_WindowsIdentity()
    {
        var verifier = builderCS.AddPaths("CatchRethrow.WindowsIdentity.cs").WithNetOnly();
#if NET
        verifier = verifier.AddReferences([CoreMetadataReference.SystemSecurityClaims]);
#endif
        verifier.Verify();
    }

    /// <summary>Verifies legacy temporary-context boundaries.</summary>
    [TestMethod]
    public void CatchRethrow_LegacyContexts() =>
        builderCS.AddPaths("CatchRethrow.LegacyContexts.cs")
            .WithNetFrameworkOnly()
            .Verify();

    [TestMethod]
    public void CatchRethrow_VB() =>
        new VerifierBuilder<VB.CatchRethrow>().AddPaths("CatchRethrow.vb").Verify();
}

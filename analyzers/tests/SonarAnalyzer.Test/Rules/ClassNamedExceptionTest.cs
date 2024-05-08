/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class ClassNamedExceptionTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ClassNamedException>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.ClassNamedException>();

    [TestMethod]
    public void ClassNamedException_CS() =>
        builderCS
            .AddPaths("ClassNamedException.cs")
            .Verify();

    [TestMethod]
    public void ClassNamedException_FromCSharp9() =>
        builderCS
            .AddPaths("ClassNamedException.CSharp9.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp9)
            .VerifyNoIssues();   // records are compliant

    [TestMethod]
    public void ClassNamedException_FromCSharp10() =>
        builderCS
            .AddPaths("ClassNamedException.CSharp10.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp10)
            .VerifyNoIssues();   // records are compliant

    [TestMethod]
    public void ClassNamedException_VB() =>
        builderVB
            .AddPaths("ClassNamedException.vb")
            .Verify();

#if NETFRAMEWORK

    [TestMethod]
    public void ClassNamedException_Interop_CS() =>
        builderCS
            .AddPaths("ClassNamedException.Interop.cs")
            .VerifyNoIssues();

    [TestMethod]
    public void ClassNamedException_Interop_VB() =>
        builderVB
            .AddPaths("ClassNamedException.Interop.vb")
            .VerifyNoIssues();

#endif
}

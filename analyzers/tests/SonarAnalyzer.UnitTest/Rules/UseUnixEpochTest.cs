/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.UnitTest.Rules;

[TestClass]
public class UseUnixEpochTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.UseUnixEpoch>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.UseUnixEpoch>();

#if NETFRAMEWORK

    [TestMethod]
    public void UseUnixEpoch_Framework_CS() =>
        builderCS.AddPaths("UseUnixEpoch.Framework.cs").VerifyNoIssueReported();

    [TestMethod]
    public void UseUnixEpoch_Framework_VB() =>
        builderVB.AddPaths("UseUnixEpoch.Framework.vb").VerifyNoIssueReported();

#else

    [TestMethod]
    public void UseUnixEpoch_CS() =>
        builderCS.AddPaths("UseUnixEpoch.cs").Verify();

    [TestMethod]
    public void UseUnixEpoch_CSharp9() =>
        builderCS.AddSnippet(
            """
            using System;

            DateTime dateTime = new(1970, 1, 1); // Noncompliant
            DateTimeOffset dateTimeOffset = new(1970, 1, 1, 0, 0, 0, TimeSpan.Zero); // Noncompliant
            """)
                 .WithTopLevelStatements()
                 .Verify();

    [TestMethod]
    public void UseUnixEpoch_VB() =>
        builderVB.AddPaths("UseUnixEpoch.vb").Verify();

#endif

}

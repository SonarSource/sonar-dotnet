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
public class AlwaysSetDateTimeKindTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.AlwaysSetDateTimeKind>();

    [TestMethod]
    public void AlwaysSetDateTimeKind_CS() =>
        builderCS.AddPaths("AlwaysSetDateTimeKind.cs").Verify();

    [TestMethod]
    public void AlwaysSetDateTimeKind_VB() =>
        new VerifierBuilder<VB.AlwaysSetDateTimeKind>().AddPaths("AlwaysSetDateTimeKind.vb").Verify();

#if NET

    [TestMethod]
    public void AlwaysSetDateTimeKind_CSharp9() =>
        builderCS.AddSnippet(
            """
                using System;

                DateTime dateTime = new(1994, 07, 05, 16, 23, 00, 42, 42); // Noncompliant
                //                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                dateTime = new(1623, DateTimeKind.Unspecified); // Compliant
                """)
            .WithTopLevelStatements()
            .Verify();

#endif

}

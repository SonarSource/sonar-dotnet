/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

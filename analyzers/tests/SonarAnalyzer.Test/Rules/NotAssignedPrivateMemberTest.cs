/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class NotAssignedPrivateMemberTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<NotAssignedPrivateMember>();

    [TestMethod]
    public void NotAssignedPrivateMember() =>
        builder.AddPaths("NotAssignedPrivateMember.cs").Verify();

#if NET
    [TestMethod]
    public void NotAssignedPrivateMember_Latest() =>
        builder
            .AddPaths("NotAssignedPrivateMember.Latest.cs")
            .AddPaths("NotAssignedPrivateMember.Latest.Partial.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

    [TestMethod]
    public void NotAssignedPrivateMember_Razor() =>
        builder.AddPaths("NotAssignedPrivateMember.razor", "NotAssignedPrivateMember.razor.cs").VerifyNoIssues();
#endif

    [TestMethod]
    public void NotAssignedPrivateMember_IndexingMovableFixedBuffer() =>
        builder.AddSnippet("""
            unsafe struct FixedArray
            {
                private fixed int a[42]; // Compliant, because of the fixed modifier

                private int[] b; // Noncompliant

                void M()
                {
                    a[0] = 42;
                    b[0] = 42;
                }
            }
            """).WithLanguageVersion(LanguageVersion.CSharp7_3).Verify();
}

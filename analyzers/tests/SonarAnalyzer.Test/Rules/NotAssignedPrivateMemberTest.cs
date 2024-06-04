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

using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class NotAssignedPrivateMemberTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<NotAssignedPrivateMember>();

        [TestMethod]
        public void NotAssignedPrivateMember() =>
            builder.AddPaths("NotAssignedPrivateMember.cs").Verify();

#if NET
        [TestMethod]
        public void NotAssignedPrivateMember_CSharp9() =>
            builder.AddPaths("NotAssignedPrivateMember.CSharp9.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [TestMethod]
        public void NotAssignedPrivateMember_Razor() =>
            builder.AddPaths("NotAssignedPrivateMember.Partial.razor", "NotAssignedPrivateMember.Partial.razor.cs").Verify();
#endif

        [TestMethod]
        public void NotAssignedPrivateMember_IndexingMovableFixedBuffer() =>
            builder.AddSnippet(@"
unsafe struct FixedArray
{
    private fixed int a[42]; // Compliant, because of the fixed modifier

    private int[] b; // Noncompliant

    void M()
    {
        a[0] = 42;
        b[0] = 42;
    }
}").WithLanguageVersion(LanguageVersion.CSharp7_3).Verify();
    }
}

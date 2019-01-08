/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

extern alias csharp;
using csharp::SonarAnalyzer.Rules.CSharp;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class NotAssignedPrivateMemberTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void NotAssignedPrivateMember()
        {
            Verifier.VerifyAnalyzer(@"TestCases\NotAssignedPrivateMember.cs",
                new NotAssignedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void NotAssignedPrivateMember_IndexingMovableFixedBuffer()
        {
            Verifier.VerifyCSharpAnalyzer(@"
unsafe struct FixedArray
{
    private fixed int a[42]; // Compliant, because of the fixed modifier

    private int[] b; // Noncompliant

    void M()
    {
        a[0] = 42;
        b[0] = 42;
    }
}", new NotAssignedPrivateMember(), new[] { new CSharpParseOptions(LanguageVersion.CSharp7_3) });
        }
    }
}

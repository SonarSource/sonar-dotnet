/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class DeclareTypesInNamespacesTest
    {
        [TestMethod]
        public void DeclareTypesInNamespaces_CS() =>
            Verifier.VerifyAnalyzer(new[] { @"TestCases\DeclareTypesInNamespaces.cs", @"TestCases\DeclareTypesInNamespaces2.cs", },
                new CS.DeclareTypesInNamespaces());

        [TestMethod]
        public void DeclareTypesInNamespaces_CS_Before8() =>
            Verifier.VerifyNonConcurrentAnalyzer(@"TestCases\DeclareTypesInNamespaces.BeforeCSharp8.cs",
                new CS.DeclareTypesInNamespaces(),
                ParseOptionsHelper.BeforeCSharp8);

        [TestMethod]
        public void DeclareTypesInNamespaces_CS_After8() =>
            Verifier.VerifyNonConcurrentAnalyzer(@"TestCases\DeclareTypesInNamespaces.AfterCSharp8.cs",
                new CS.DeclareTypesInNamespaces(),
                ParseOptionsHelper.FromCSharp8);

#if NET
        [TestMethod]
        public void DeclareTypesInNamespaces_CS_AfterCSharp9() =>
            Verifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\DeclareTypesInNamespaces.AfterCSharp9.cs", new CS.DeclareTypesInNamespaces());

        [TestMethod]
        public void DeclareTypesInNamespaces_CS_AfterCSharp10() =>
            Verifier.VerifyAnalyzerFromCSharp10Library(
                new[]
                {
                    @"TestCases\DeclareTypesInNamespaces.AfterCSharp10.FileScopedNamespace.cs",
                    @"TestCases\DeclareTypesInNamespaces.AfterCSharp10.RecordStruct.cs"
                },
                new CS.DeclareTypesInNamespaces());

#endif

        [TestMethod]
        public void DeclareTypesInNamespaces_VB() =>
            Verifier.VerifyAnalyzer(new[] { @"TestCases\DeclareTypesInNamespaces.vb", @"TestCases\DeclareTypesInNamespaces2.vb", },
                new VB.DeclareTypesInNamespaces());
    }
}

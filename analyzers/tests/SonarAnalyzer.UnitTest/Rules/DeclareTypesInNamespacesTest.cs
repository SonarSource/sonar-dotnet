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
        private readonly VerifierBuilder builder = new VerifierBuilder<CS.DeclareTypesInNamespaces>();
        private readonly VerifierBuilder nonconcurrent = new VerifierBuilder<CS.DeclareTypesInNamespaces>().WithConcurrentAnalysis(false);

        [TestMethod]
        public void DeclareTypesInNamespaces_CS() =>
            builder.AddPaths("DeclareTypesInNamespaces.cs", "DeclareTypesInNamespaces2.cs").WithAutogenerateConcurrentFiles(false).Verify();

        [TestMethod]
        public void DeclareTypesInNamespaces_CS_Before8() =>
            nonconcurrent.AddPaths("DeclareTypesInNamespaces.BeforeCSharp8.cs").WithOptions(ParseOptionsHelper.BeforeCSharp8).Verify();

        [TestMethod]
        public void DeclareTypesInNamespaces_CS_After8() =>
            nonconcurrent.AddPaths("DeclareTypesInNamespaces.AfterCSharp8.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

#if NET

        [TestMethod]
        public void DeclareTypesInNamespaces_CS_AfterCSharp9() =>
            builder.AddPaths("DeclareTypesInNamespaces.AfterCSharp9.cs").WithTopLevelStatements().Verify();

        [TestMethod]
        public void DeclareTypesInNamespaces_CS_AfterCSharp10() =>
            nonconcurrent
                .AddPaths("DeclareTypesInNamespaces.AfterCSharp10.FileScopedNamespace.cs", "DeclareTypesInNamespaces.AfterCSharp10.RecordStruct.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .Verify();

#endif

        [TestMethod]
        public void DeclareTypesInNamespaces_VB() =>
            new VerifierBuilder<VB.DeclareTypesInNamespaces>()
                .AddPaths("DeclareTypesInNamespaces.vb", "DeclareTypesInNamespaces2.vb")
                .WithAutogenerateConcurrentFiles(false)
                .Verify();
    }
}

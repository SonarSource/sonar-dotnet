/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class AsyncVoidMethodTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<AsyncVoidMethod>();

        [TestMethod]
        public void AsyncVoidMethod() =>
            builder.AddPaths("AsyncVoidMethod.cs")
                .Verify();

#if NET
        [TestMethod]
        public void AsyncVoidMethod_CSharp9() =>
            builder.AddPaths("AsyncVoidMethod.CSharp9.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .Verify();

        [TestMethod]
        public void AsyncVoidMethod_CSharpPreview() =>
            builder.AddPaths("AsyncVoidMethod.CSharpPreview.cs")
                .WithLanguageVersion(Microsoft.CodeAnalysis.CSharp.LanguageVersion.Preview)
                .Verify();

        [TestMethod]
        public void AsyncVoidMethod_CSharp10() =>
            builder.AddPaths("AsyncVoidMethod.CSharp10.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .AddReferences(NuGetMetadataReference.MicrosoftVisualStudioQualityToolsUnitTestFramework)
                .Verify();

        [DataTestMethod]
        [DataRow("1.1.11")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void AsyncVoidMethod_MsTestV2_CSharpPreview(string testFwkVersion) =>
            builder.AddPaths("AsyncVoidMethod_MsTestV2_CSharp.Preview.cs")
                // The first version of the framework is not compatible with Net 6 so we need to test only v2 with preview features
                .WithLanguageVersion(Microsoft.CodeAnalysis.CSharp.LanguageVersion.Preview)
                .AddReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion))
                .WithConcurrentAnalysis(false)
                .Verify();

#endif

        [DataTestMethod]
        [DataRow("1.1.11")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void AsyncVoidMethod_MsTestV2(string testFwkVersion) =>
            builder.AddPaths("AsyncVoidMethod_MsTestV2.cs")
                .AddReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion))
                .Verify();

        [TestMethod]
        public void AsyncVoidMethod_MsTestV1() =>
            builder.AddPaths("AsyncVoidMethod_MsTestV1.cs")
                .AddReferences(NuGetMetadataReference.MicrosoftVisualStudioQualityToolsUnitTestFramework)
                .Verify();
    }
}

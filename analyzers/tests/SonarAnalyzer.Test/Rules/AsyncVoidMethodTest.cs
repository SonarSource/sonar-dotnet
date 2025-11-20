/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using SonarAnalyzer.CSharp.Rules;

using static SonarAnalyzer.TestFramework.MetadataReferences.NugetPackageVersions;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class AsyncVoidMethodTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<AsyncVoidMethod>();

        [TestMethod]
        public void AsyncVoidMethod() =>
            builder.AddPaths("AsyncVoidMethod.cs")
                .Verify();

        [TestMethod]
        public void AsyncVoidMethod_CSharp9() =>
            builder.AddPaths("AsyncVoidMethod.CSharp9.cs")
                .WithOptions(LanguageOptions.FromCSharp9)
                .Verify();

        [TestMethod]
        public void AsyncVoidMethod_CSharp10() =>
            builder.AddPaths("AsyncVoidMethod.CSharp10.cs")
                .WithOptions(LanguageOptions.FromCSharp10)
                .AddReferences(NuGetMetadataReference.MicrosoftVisualStudioQualityToolsUnitTestFramework)
                .Verify();

        [TestMethod]
        public void AsyncVoidMethod_CSharp11() =>
            builder.AddPaths("AsyncVoidMethod.CSharp11.cs")
                .WithOptions(LanguageOptions.FromCSharp11)
                .Verify();

        [TestMethod]
        [DataRow(MsTest.Ver1_1)]
        [DataRow(MsTest.Ver3)]
        [DataRow(Latest)]
        public void AsyncVoidMethod_MsTestV2_CSharp11(string testFwkVersion) =>
            builder.AddPaths("AsyncVoidMethod_MsTestV2_CSharp11.cs")
                // The first version of the framework is not compatible with Net 7 so we need to test only v2 with C#11 features
                .WithOptions(LanguageOptions.FromCSharp11)
                .AddReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion))
                .WithConcurrentAnalysis(false)
                .Verify();

        [TestMethod]
        [DataRow("1.1.11")]
        [DataRow(TestConstants.NuGetLatestVersion)]
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

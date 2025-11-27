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

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class AsyncVoidMethodTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<AsyncVoidMethod>();

    [TestMethod]
    public void AsyncVoidMethod() =>
        builder.AddPaths("AsyncVoidMethod.cs")
            .Verify();

    [TestMethod]
    public void AsyncVoidMethod_CSharpLatest() =>
        builder.AddPaths("AsyncVoidMethod.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .AddReferences(NuGetMetadataReference.MicrosoftVisualStudioQualityToolsUnitTestFramework)
            .Verify();

    [TestMethod]
    [DataRow(MsTest.Ver1_1)]
    [DataRow(MsTest.Ver1_2)]
    [DataRow(MsTest.Ver3)]
    [DataRow(Latest)]
    public void AsyncVoidMethod_MsTestTestFramework(string testFwkVersion) =>
        builder.AddPaths("AsyncVoidMethod.MsTestTestFramework.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .AddReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion))
            .WithConcurrentAnalysis(false)
            .Verify();

    [TestMethod]
    public void AsyncVoidMethod_VsUtFramework() =>
        builder.AddPaths("AsyncVoidMethod.VsUtFramework.cs")
            // MicrosoftVisualStudioQualityToolsUnitTestFramework is not compatible with Net7/C#11 so max is C#10
            .WithOptions(LanguageOptions.FromCSharp10)
            .AddReferences(NuGetMetadataReference.MicrosoftVisualStudioQualityToolsUnitTestFramework)
            .Verify();
}

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

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class DisposableNotDisposedTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<DisposableNotDisposed>();

    [TestMethod]
    public void DisposableNotDisposed() =>
        builder.AddPaths("DisposableNotDisposed.cs")
            .AddReferences(MetadataReferenceFacade.SystemNetHttp)
            .Verify();

    [TestMethod]
    public void DisposableNotDisposed_ILogger() =>
        builder.AddPaths("DisposableNotDisposed.ILogger.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingPackages(TestConstants.NuGetLatestVersion).ToArray())
            .VerifyNoIssues();

    [TestMethod]
    public void DisposableNotDisposed_TopLevelStatements() =>
        builder.AddPaths("DisposableNotDisposed.TopLevelStatements.cs")
            .WithTopLevelStatements()
            .Verify();

#if NET

    [TestMethod]
    public void DisposableNotDisposed_Latest() =>
        builder.AddPaths("DisposableNotDisposed.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .AddReferences(NuGetMetadataReference.FluentAssertions(NugetPackageVersions.FluentAssertionsVersions.Ver5))
            .AddReferences(MetadataReferenceFacade.SystemNetHttp)
            .Verify();

#endif

}

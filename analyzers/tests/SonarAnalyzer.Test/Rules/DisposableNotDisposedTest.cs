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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class DisposableNotDisposedTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<DisposableNotDisposed>();

    [TestMethod]
    public void DisposableNotDisposed() =>
        builder.AddPaths("DisposableNotDisposed.cs")
            .WithOptions(LanguageOptions.FromCSharp7)
            .AddReferences(MetadataReferenceFacade.SystemNetHttp)
            .Verify();

    [TestMethod]
    public void DisposableNotDisposed_ILogger() =>
        builder.AddPaths("DisposableNotDisposed.ILogger.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingPackages(Constants.NuGetLatestVersion).ToArray())
            .VerifyNoIssues();

#if NET

    [TestMethod]
    public void DisposableNotDisposed_CSharp8() =>
        builder.AddPaths("DisposableNotDisposed.CSharp8.cs")
            .WithOptions(LanguageOptions.FromCSharp8)
            .AddReferences(NuGetMetadataReference.FluentAssertions("5.9.0"))
            .Verify();

    [TestMethod]
    public void DisposableNotDisposed_CSharp9() =>
        builder.AddPaths("DisposableNotDisposed.CSharp9.cs")
            .WithTopLevelStatements()
            .AddReferences(MetadataReferenceFacade.SystemNetHttp)
            .Verify();

    [TestMethod]
    public void DisposableNotDisposed_CSharp10() =>
        builder.AddPaths("DisposableNotDisposed.CSharp10.cs")
            .WithOptions(LanguageOptions.FromCSharp10)
            .Verify();

#endif

}

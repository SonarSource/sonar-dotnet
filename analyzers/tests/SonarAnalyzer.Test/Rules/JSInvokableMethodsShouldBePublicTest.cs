/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

#if NET

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class JSInvokableMethodsShouldBePublicTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<JSInvokableMethodsShouldBePublic>()
        .AddReferences(NuGetMetadataReference.MicrosoftJSInterop("7.0.14"));

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void JSInvokableMethodsShouldBePublic_CS() =>
        builder.AddPaths("JSInvokableMethodsShouldBePublic.cs").Verify();

    [TestMethod]
    public void JSInvokableMethodsShouldBePublic_Razor() =>
        builder
            .AddPaths("JSInvokableMethodsShouldBePublic.razor", "JSInvokableMethodsShouldBePublic.razor.cs")
            .WithOptions(LanguageOptions.FromCSharp9)
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .Verify();

    [TestMethod]
    public void JSInvokableMethodsShouldBePublic_CSharp8() =>
        builder.AddPaths("JSInvokableMethodsShouldBePublic.CSharp8.cs")
            .WithOptions(LanguageOptions.FromCSharp8)
            .Verify();

    [TestMethod]
    public void JSInvokableMethodsShouldBePublic_CSharp9() =>
        builder.AddPaths("JSInvokableMethodsShouldBePublic.CSharp9.cs")
            .WithOptions(LanguageOptions.FromCSharp9)
            .VerifyNoIssues();
}

#endif

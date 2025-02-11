/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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
public class MemberShouldBeStaticTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<MemberShouldBeStatic>();

    [DataTestMethod]
    [DataRow("1.0.0", "3.0.20105.1")]
    [DataRow(TestConstants.NuGetLatestVersion, TestConstants.NuGetLatestVersion)]
    public void MemberShouldBeStatic(string aspnetCoreVersion, string aspnetVersion) =>
        builder.AddPaths("MemberShouldBeStatic.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftAspNetCoreMvcWebApiCompatShim(aspnetCoreVersion)
                                    .Concat(NuGetMetadataReference.MicrosoftAspNetMvc(aspnetVersion))
                                    .Concat(NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(aspnetCoreVersion))
                                    .Concat(NuGetMetadataReference.MicrosoftAspNetCoreMvcViewFeatures(aspnetCoreVersion))
                                    .Concat(NuGetMetadataReference.MicrosoftAspNetCoreRoutingAbstractions(aspnetCoreVersion)))
            .Verify();

    [TestMethod]
    public void MemberShouldBeStatic_WinForms() =>
        builder.AddPaths("MemberShouldBeStatic.WinForms.cs")
            .AddReferences(MetadataReferenceFacade.SystemWindowsForms)
            .Verify();

    [TestMethod]
    public void MemberShouldBeStatic_Xaml() =>
        builder.AddPaths("MemberShouldBeStatic.Xaml.cs")
            .AddReferences(MetadataReferenceFacade.PresentationFramework)
            .Verify();

#if NET

    [TestMethod]
    public void MemberShouldBeStatic_Latest() =>
        builder
            .AddPaths("MemberShouldBeStatic.Latest.cs")
            .AddPaths("MemberShouldBeStatic.Latest.Partial.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .WithTopLevelStatements().Verify();
#endif

#if NETFRAMEWORK // HttpApplication is available only on .Net Framework
    [TestMethod]
    public void MemberShouldBeStatic_HttpApplication() =>
        builder.AddSnippet(@"
public class HttpApplication1 : System.Web.HttpApplication
{
public int Foo() => 0;

protected int FooFoo() => 0; // Noncompliant
}").WithErrorBehavior(CompilationErrorBehavior.Ignore).Verify();

#endif

    [TestMethod]
    public void MemberShouldBeStatic_InvalidCode() =>
        // Handle invalid code causing NullReferenceException: https://github.com/SonarSource/sonar-dotnet/issues/819
        builder.AddSnippet("""
                           public class Class7
                           {
                               public async Task<Result<T> Function<T>(Func<Task<Result<T>>> f)
                               {
                                   Result<T> result;
                                   result = await f();
                                   return result;
                               }
                           }
                           """)
                .VerifyNoAD0001();
}

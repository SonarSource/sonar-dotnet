/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

    [TestMethod]
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
        builder.AddPaths("MemberShouldBeStatic.WinForms.cs").AddReferences(MetadataReferenceFacade.SystemWindowsForms).Verify();

    [TestMethod]
    public void MemberShouldBeStatic_Xaml() =>
        builder.AddPaths("MemberShouldBeStatic.Xaml.cs").AddReferences(MetadataReferenceFacade.PresentationFramework).Verify();

    [TestMethod]
    public void MemberShouldBeStatic_LambdaStartup() =>
        builder.AddSnippet("""
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            namespace Amazon.Lambda.Annotations
            {
                [System.AttributeUsage(System.AttributeTargets.Class)]
                public sealed class LambdaStartupAttribute : System.Attribute { }
            }

            namespace Other
            {
                public sealed class LambdaStartupAttribute : System.Attribute { }
            }

            namespace Microsoft.Extensions.DependencyInjection
            {
                public interface IServiceCollection { }
            }

            namespace Microsoft.Extensions.Hosting
            {
                public interface IHostApplicationBuilder { }
                public sealed class HostApplicationBuilder : IHostApplicationBuilder { }
            }

            [Amazon.Lambda.Annotations.LambdaStartup]
            public class Startup
            {
                public IHostApplicationBuilder ConfigureHostBuilder() => new HostApplicationBuilder();      // Compliant: invoked by the Lambda source generator
                public void ConfigureServices(IServiceCollection services) => System.Console.WriteLine();   // Compliant: invoked by the Lambda source generator
                public void OtherMethod() => System.Console.WriteLine(); // Noncompliant
            }

            [Amazon.Lambda.Annotations.LambdaStartup]
            public class StartupWithProperty
            {
                public int ConfigureServices => 42; // Noncompliant
            }

            public class UnannotatedStartup
            {
                public object ConfigureHostBuilder() => new object();           // Noncompliant
                public void ConfigureServices() => System.Console.WriteLine();  // Noncompliant
            }

            [Other.LambdaStartup]
            public class StartupWithDifferentAttribute
            {
                public void ConfigureServices() => System.Console.WriteLine(); // Noncompliant
            }
            """).Verify();

    [TestMethod]
    public void MemberShouldBeStatic_Latest() =>
        builder.AddPaths("MemberShouldBeStatic.Latest.cs")
            .AddPaths("MemberShouldBeStatic.Latest.Partial.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .WithTopLevelStatements()
            .Verify();

    [TestMethod]
    public void MemberShouldBeStatic_HttpApplication() =>
        builder.AddSnippet("""

            public class HttpApplication1 : System.Web.HttpApplication // Error [CS0234]
            {
            public int Foo() => 0;

            protected int FooFoo() => 0; // Noncompliant
            }
            """).Verify();

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

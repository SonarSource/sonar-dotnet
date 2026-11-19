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

using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

#if NET

[TestClass]
public class ControllersHaveMixedResponsibilitiesTest
{
    private readonly VerifierBuilder builder =
        new VerifierBuilder<ControllersHaveMixedResponsibilities>().AddReferences(References).WithBasePath("AspNet");

    private static IEnumerable<MetadataReference> References =>
    [
        AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcAbstractions,
        AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcCore,
        AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcViewFeatures,
        CoreMetadataReference.SystemComponentModel, // For IServiceProvider
        .. NuGetMetadataReference.MicrosoftExtensionsDependencyInjectionAbstractions("8.0.1"), // For IServiceProvider extensions
    ];

    [TestMethod]
    public void ControllersHaveMixedResponsibilities_CS() =>
        builder
            .AddPaths("ControllersHaveMixedResponsibilities.Latest.cs", "ControllersHaveMixedResponsibilities.Latest.Partial.cs")
            .WithLanguageVersion(LanguageVersion.Latest)
            .Verify();

    [TestMethod]
    public void ControllersHaveMixedResponsibilities_CustomExcludedServices() =>
        new VerifierBuilder()
            .AddAnalyzer(() => new ControllersHaveMixedResponsibilities { ExcludedServices = "IS1, IS2" })
            .AddReferences(References)
            .AddSnippet("""
                using Microsoft.AspNetCore.Mvc;

                public interface IS1 { void Use(); }
                public interface IS2 { void Use(); }
                public interface IS3 { void Use(); }
                public interface IS4 { void Use(); }
                public interface ILogger<T> { void Use(); }

                // Compliant: IS1 and IS2 are excluded via rule parameter; remaining services form one responsibility
                [ApiController]
                public class OnlyExcludedAndSharedService(IS1 s1, IS2 s2, IS3 s3) : ControllerBase
                {
                    public IActionResult A1() { s1.Use(); s3.Use(); return Ok(); }
                    public IActionResult A2() { s2.Use(); s3.Use(); return Ok(); }
                }

                // Noncompliant@+2 {{This controller has multiple responsibilities and could be split into 2 smaller controllers.}}
                [ApiController]
                public class CustomExclusionStillFlagsOtherServices(IS3 s3, IS4 s4) : ControllerBase
                {
                    public IActionResult A1() { s3.Use(); return Ok(); } // Secondary {{May belong to responsibility #1.}}
                    public IActionResult A2() { s4.Use(); return Ok(); } // Secondary {{May belong to responsibility #2.}}
                }

                // Noncompliant@+2: default well-known ILogger is not excluded when the parameter overrides the default list
                [ApiController]
                public class LoggerIsNotExcludedWhenOverridden(ILogger<LoggerIsNotExcludedWhenOverridden> logger, IS3 s3) : ControllerBase
                {
                    public IActionResult A1() { logger.Use(); return Ok(); } // Secondary {{May belong to responsibility #1.}}
                    public IActionResult A2() { s3.Use(); return Ok(); }     // Secondary {{May belong to responsibility #2.}}
                }
                """)
            .WithLanguageVersion(LanguageVersion.Latest)
            .WithAutogenerateConcurrentFiles(false)
            .Verify();

    [TestMethod]
    public void ExcludedServices_ByDefault_ContainsWellKnownServices() =>
        new ControllersHaveMixedResponsibilities().ExcludedServices.Should().Be(
            "ILogger, IMediator, IMapper, IConfiguration, IBus, IMessageBus, IHttpClientFactory");
}

#endif

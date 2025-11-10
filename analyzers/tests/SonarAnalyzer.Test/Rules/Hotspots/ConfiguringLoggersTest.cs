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

using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class ConfiguringLoggersTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder().WithBasePath("Hotspots").AddAnalyzer(() => new CS.ConfiguringLoggers(AnalyzerConfiguration.AlwaysEnabled));
        private readonly VerifierBuilder builderVB = new VerifierBuilder().WithBasePath("Hotspots").AddAnalyzer(() => new VB.ConfiguringLoggers(AnalyzerConfiguration.AlwaysEnabled));

        [TestMethod]
        public void ConfiguringLoggers_Log4Net_CS() =>
            builderCS.AddPaths("ConfiguringLoggers_Log4Net.cs")
                .AddReferences(Log4NetReferences)
                .Verify();

        [TestMethod]
        public void ConfiguringLoggers_Log4Net_VB() =>
            builderVB.AddPaths("ConfiguringLoggers_Log4Net.vb")
                .AddReferences(Log4NetReferences)
                .Verify();

        [TestMethod]
        public void ConfiguringLoggers_NLog_CS() =>
            builderCS.AddPaths("ConfiguringLoggers_NLog.cs")
                .AddReferences(NLogReferences)
                .Verify();

        [TestMethod]
        public void ConfiguringLoggers_NLog_VB() =>
            builderVB.AddPaths("ConfiguringLoggers_NLog.vb")
                .AddReferences(NLogReferences)
                .Verify();

        [TestMethod]
        public void ConfiguringLoggers_Serilog_CS() =>
            builderCS.AddPaths("ConfiguringLoggers_Serilog.cs")
                .AddReferences(SeriLogReferences)
                .Verify();

        [TestMethod]
        public void ConfiguringLoggers_Serilog_VB() =>
            builderVB.AddPaths("ConfiguringLoggers_Serilog.vb")
                .AddReferences(SeriLogReferences)
                .Verify();

#if NET
        [TestMethod]
        public void ConfiguringLoggers_AspNetCore2_CS() =>
            builderCS.AddPaths("ConfiguringLoggers_AspNetCore.cs")
                .AddReferences(AspNetCore2LoggingReferences)
                .WithConcurrentAnalysis(false)
                .Verify();

        [TestMethod]
        public void ConfiguringLoggers_AspNetCoreLatest_CS() =>
            builderCS.AddPaths("ConfiguringLoggers_AspNetCore6.cs")
                .AddReferences(AspNetCoreLoggingReferences("10.0.0-rc.2.25502.107"))    // Change back to TestConstants.NuGetLatestVersion when .NET 10 is in GA - NET-2572
                .Verify();

        [TestMethod]
        public void ConfiguringLoggers_AspNetCore_VB() =>
            builderVB.AddPaths("ConfiguringLoggers_AspNetCore.vb")
                .AddReferences(AspNetCore2LoggingReferences)
                .Verify();
#endif

        internal static IEnumerable<MetadataReference> Log4NetReferences =>
            // See: https://github.com/SonarSource/sonar-dotnet/issues/3548
            NuGetMetadataReference.Log4Net("2.0.8", "net45-full").Concat(MetadataReferenceFacade.SystemXml);

        private static IEnumerable<MetadataReference> NLogReferences =>
            NuGetMetadataReference.NLog();

        private static IEnumerable<MetadataReference> SeriLogReferences =>
            Enumerable.Empty<MetadataReference>()
                .Concat(NuGetMetadataReference.Serilog("2.11.0"))
                .Concat(NuGetMetadataReference.SerilogSinksConsole("4.1.0"));

#if NET
        private static IEnumerable<MetadataReference> AspNetCore2LoggingReferences =>
            Enumerable.Empty<MetadataReference>()
                .Concat(NuGetMetadataReference.MicrosoftAspNetCore(TestConstants.DotNetCore220Version))
                .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHosting(TestConstants.DotNetCore220Version))
                .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHostingAbstractions(TestConstants.DotNetCore220Version))
                .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHttpAbstractions(TestConstants.DotNetCore220Version))
                .Concat(NuGetMetadataReference.MicrosoftExtensionsConfigurationAbstractions(TestConstants.DotNetCore220Version))
                .Concat(NuGetMetadataReference.MicrosoftExtensionsOptions(TestConstants.DotNetCore220Version))
                .Concat(NuGetMetadataReference.MicrosoftExtensionsLoggingPackages(TestConstants.DotNetCore220Version))
                .Concat(new[] { AspNetCoreMetadataReference.MicrosoftExtensionsDependencyInjectionAbstractions });

        private static IEnumerable<MetadataReference> AspNetCoreLoggingReferences(string version) =>
            new[]
                {
                    AspNetCoreMetadataReference.MicrosoftAspNetCore,
                    AspNetCoreMetadataReference.MicrosoftAspNetCoreHosting,
                    AspNetCoreMetadataReference.MicrosoftAspNetCoreHostingAbstractions,
                    AspNetCoreMetadataReference.MicrosoftAspNetCoreHttpAbstractions,
                    AspNetCoreMetadataReference.MicrosoftExtensionsLoggingEventSource
                }
                .Concat(NuGetMetadataReference.MicrosoftExtensionsConfigurationAbstractions(version))
                .Concat(NuGetMetadataReference.MicrosoftExtensionsOptions(version))
                .Concat(NuGetMetadataReference.MicrosoftExtensionsLoggingPackages(version))
                .Concat(NuGetMetadataReference.MicrosoftExtensionsDependencyInjectionAbstractions(version));

#endif

    }
}

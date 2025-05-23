﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

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
                .AddReferences(AspNetCoreLoggingReferences(Constants.NuGetLatestVersion))
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
                .Concat(NuGetMetadataReference.MicrosoftAspNetCore(Constants.DotNetCore220Version))
                .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHosting(Constants.DotNetCore220Version))
                .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHostingAbstractions(Constants.DotNetCore220Version))
                .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHttpAbstractions(Constants.DotNetCore220Version))
                .Concat(NuGetMetadataReference.MicrosoftExtensionsConfigurationAbstractions(Constants.DotNetCore220Version))
                .Concat(NuGetMetadataReference.MicrosoftExtensionsOptions(Constants.DotNetCore220Version))
                .Concat(NuGetMetadataReference.MicrosoftExtensionsLoggingPackages(Constants.DotNetCore220Version))
                .Concat(new[] { AspNetCoreMetadataReference.MicrosoftExtensionsDependencyInjectionAbstractions });

        private static IEnumerable<MetadataReference> AspNetCoreLoggingReferences(string version) =>
            new[]
                {
                    AspNetCoreMetadataReference.MicrosoftAspNetCore,
                    AspNetCoreMetadataReference.MicrosoftAspNetCoreDiagnostics,
                    AspNetCoreMetadataReference.MicrosoftAspNetCoreHosting,
                    AspNetCoreMetadataReference.MicrosoftAspNetCoreHostingAbstractions,
                    AspNetCoreMetadataReference.MicrosoftAspNetCoreHttpAbstractions,
                    AspNetCoreMetadataReference.MicrosoftExtensionsHostingAbstractions,
                    AspNetCoreMetadataReference.MicrosoftExtensionsLoggingEventSource
                }
                .Concat(NuGetMetadataReference.MicrosoftExtensionsConfigurationAbstractions(version))
                .Concat(NuGetMetadataReference.MicrosoftExtensionsOptions(version))
                .Concat(NuGetMetadataReference.MicrosoftExtensionsLoggingPackages(version))
                .Concat(NuGetMetadataReference.MicrosoftExtensionsDependencyInjectionAbstractions(version));

#endif

    }
}

/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

extern alias csharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using csharp::SonarAnalyzer.Rules.CSharp;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class ConfiguringLoggersTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void ConfiguringLoggers_AspNetCore_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\ConfiguringLoggers_AspNetCore.cs",
                new ConfiguringLoggers(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: AspNetCoreLoggingReferences);

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void ConfiguringLoggers_AspNetCore_VB() =>
            Verifier.VerifyAnalyzer(@"TestCases\ConfiguringLoggers_AspNetCore.vb",
                new SonarAnalyzer.Rules.VisualBasic.ConfiguringLoggers(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: AspNetCoreLoggingReferences);

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void ConfiguringLoggers_Log4Net_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\ConfiguringLoggers_Log4Net.cs",
                new ConfiguringLoggers(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: Log4NetReferences);

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void ConfiguringLoggers_Log4Net_VB() =>
            Verifier.VerifyAnalyzer(@"TestCases\ConfiguringLoggers_Log4Net.vb",
                new SonarAnalyzer.Rules.VisualBasic.ConfiguringLoggers(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: Log4NetReferences);

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void ConfiguringLoggers_NLog_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\ConfiguringLoggers_NLog.cs",
                new ConfiguringLoggers(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: NLogReferences);

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void ConfiguringLoggers_NLog_VB() =>
            Verifier.VerifyAnalyzer(@"TestCases\ConfiguringLoggers_NLog.vb",
                new SonarAnalyzer.Rules.VisualBasic.ConfiguringLoggers(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: NLogReferences);

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void ConfiguringLoggers_Serilog_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\ConfiguringLoggers_Serilog.cs",
                new ConfiguringLoggers(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: SeriLogReferences);

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void ConfiguringLoggers_Serilog_VB() =>
            Verifier.VerifyAnalyzer(@"TestCases\ConfiguringLoggers_Serilog.vb",
                new SonarAnalyzer.Rules.VisualBasic.ConfiguringLoggers(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: SeriLogReferences);

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void ConfiguringLoggers_CS_RuleDisabled() =>
            Verifier.VerifyNoIssueReported(@"TestCases\ConfiguringLoggers_AspNetCore.cs",
                new ConfiguringLoggers(),
                additionalReferences: AspNetCoreLoggingReferences);

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void ConfiguringLoggers_VB_RuleDisabled() =>
            Verifier.VerifyNoIssueReported(@"TestCases\ConfiguringLoggers_AspNetCore.vb",
                new SonarAnalyzer.Rules.VisualBasic.ConfiguringLoggers(),
                additionalReferences: AspNetCoreLoggingReferences);

        private static IEnumerable<MetadataReference> AspNetCoreLoggingReferences =>
            NetStandardMetadataReference.Netstandard
            .Concat(NuGetMetadataReference.MicrosoftAspNetCore(Constants.DotNetCore220Version))
            .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHosting(Constants.DotNetCore220Version))
            .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHostingAbstractions(Constants.DotNetCore220Version))
            .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHttpAbstractions(Constants.DotNetCore220Version))
            .Concat(NuGetMetadataReference.MicrosoftExtensionsConfigurationAbstractions(Constants.DotNetCore220Version))
            .Concat(NuGetMetadataReference.MicrosoftExtensionsDependencyInjectionAbstractions(Constants.DotNetCore220Version))
            .Concat(NuGetMetadataReference.MicrosoftExtensionsOptions(Constants.DotNetCore220Version))
            .Concat(NuGetMetadataReference.MicrosoftExtensionsLoggingPackages(Constants.DotNetCore220Version));

        private static IEnumerable<MetadataReference> Log4NetReferences =>
            NuGetMetadataReference.Log4Net(Constants.NuGetLatestVersion, "net45-full")
            .Concat(MetadataReferenceFacade.GetSystemXml());

        private static IEnumerable<MetadataReference> NLogReferences =>
            NuGetMetadataReference.NLog(Constants.NuGetLatestVersion);

        private static IEnumerable<MetadataReference> SeriLogReferences =>
            NuGetMetadataReference.SerilogPackages(Constants.NuGetLatestVersion);

    }
}


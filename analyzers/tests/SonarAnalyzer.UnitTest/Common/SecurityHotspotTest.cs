﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.Rules;
using SonarAnalyzer.UnitTest.TestFramework;
using SonarAnalyzer.Utilities;

namespace SonarAnalyzer.UnitTest.Common
{
    [TestClass]
    public class SecurityHotspotTest
    {
        [TestMethod]
        public void SecurityHotspotRules_DoNotRaiseIssues_CS() =>
            VerifyNoIssueReported(AnalyzerLanguage.CSharp);

        [TestMethod]
        public void SecurityHotspotRules_DoNotRaiseIssues_VB() =>
            VerifyNoIssueReported(AnalyzerLanguage.VisualBasic);

        private static void VerifyNoIssueReported(AnalyzerLanguage language)
        {
            foreach (var analyzer in GetHotspotAnalyzers(language).Where(IsTestValid))
            {
                Verifier.VerifyNoIssueReported(@$"TestCases\Hotspots\{GetTestCaseFileName(analyzer)}.{language.FileExtension}", analyzer, GetAdditionalReferences());
            }
        }

        private static IEnumerable<SonarDiagnosticAnalyzer> GetHotspotAnalyzers(AnalyzerLanguage language) =>
            new RuleFinder().GetAnalyzerTypes(language)
                .Where(type => typeof(SonarDiagnosticAnalyzer).IsAssignableFrom(type))   // Avoid IRuleFactory and SE rules
                .Select(type => (SonarDiagnosticAnalyzer)Activator.CreateInstance(type))
                .Where(IsSecurityHotspot);

        private static bool IsSecurityHotspot(DiagnosticAnalyzer analyzer) =>
            analyzer.SupportedDiagnostics.Any(TestHelper.IsSecurityHotspot);

        private static string GetTestCaseFileName(DiagnosticAnalyzer analyzer)
        {
            var typeName = analyzer.GetType().Name;

            return typeName switch
            {
                "ConfiguringLoggers" => "ConfiguringLoggers_Log4Net",
                "CookieShouldBeHttpOnly" => "CookieShouldBeHttpOnly_Nancy",
                "CookieShouldBeSecure" => "CookieShouldBeSecure_Nancy",
                "DoNotHardcodeCredentials" => "DoNotHardcodeCredentials_DefaultValues",
                "DeliveringDebugFeaturesInProduction" => "DeliveringDebugFeaturesInProduction.NetCore2",
#if NETFRAMEWORK
                "ExecutingSqlQueries" => "ExecutingSqlQueries_Net46",
                "UsingCookies" => "UsingCookies_Net46",
                "LooseFilePermissions" => "LooseFilePermissions.Windows",
#else
                "ExecutingSqlQueries" => "ExecutingSqlQueries_NetCore",
                "UsingCookies" => "UsingCookies_NetCore",
                "LooseFilePermissions" => "LooseFilePermissions.Unix",
#endif
                _ => typeName
            };
        }

        private static bool IsTestValid(DiagnosticAnalyzer analyzer)
        {
#if NETFRAMEWORK
            return analyzer.GetType().Name != "DisablingCSRFProtection";
#else
            // IdentityModel is not available on .Net Core
            return analyzer.GetType().Name != "ControllingPermissions";
#endif
        }

        private static IEnumerable<MetadataReference> GetAdditionalReferences() =>
            DeliveringDebugFeaturesInProductionTest.AdditionalReferencesNetCore2
#if NETFRAMEWORK
                                  .Concat(ControllingPermissionsTest.AdditionalReferences)
                                  .Concat(ExecutingSqlQueriesTest.GetReferencesNet46(Constants.NuGetLatestVersion))
                                  .Concat(UsingCookies.GetAdditionalReferencesForNet46())
                                  .Concat(NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(Constants.NuGetLatestVersion)) // Needed by RequestsWithExcessiveLength
                                  .Concat(NuGetMetadataReference.MicrosoftAspNetCoreMvcViewFeatures(Constants.NuGetLatestVersion)) // Needed by RequestsWithExcessiveLength
#else
                                  .Concat(ExecutingSqlQueriesTest.GetReferencesNetCore(Constants.DotNetCore220Version))
                                  .Concat(UsingCookies.GetAdditionalReferencesForNetCore(Constants.DotNetCore220Version))
                                  .Concat(NuGetMetadataReference.MonoPosixNetStandard()) // Needed by LooseFilePermissions
                                  .Concat(DisablingCSRFProtectionTest.AdditionalReferences())
#endif
                                  .Concat(ConfiguringLoggersTest.Log4NetReferences)
                                  .Concat(DeliveringDebugFeaturesInProductionTest.AdditionalReferencesNetCore2)
                                  .Concat(ExpandingArchivesTest.AdditionalReferences)
                                  .Concat(DoNotHardcodeCredentialsTest.AdditionalReferences)
                                  .Concat(MetadataReferenceFacade.SystemDiagnosticsProcess)
                                  .Concat(MetadataReferenceFacade.RegularExpressions) // Needed by UsingRegularExpressions
                                  .Concat(MetadataReferenceFacade.SystemSecurityCryptography) // Needed by DoNotUseRandom
                                  .Concat(NuGetMetadataReference.MicrosoftAspNetMvc(Constants.NuGetLatestVersion)) // Needed by DisablingRequestValidation
                                  .Concat(NuGetMetadataReference.Nancy()); // Needed by CookieShouldBeHttpOnly, CookiesShouldBeSecure
    }
}

/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.Rules;
using SonarAnalyzer.Utilities;
using UsingCookies = SonarAnalyzer.UnitTest.Rules.UsingCookies;

namespace SonarAnalyzer.UnitTest.Common
{
    [TestClass]
    public class SecurityHotspotTest
    {
        [TestMethod]
        public void SecurityHotspotRules_DoNotRaiseIssues_CS() =>
            VerifyNoIssueReported(AnalyzerLanguage.CSharp, ParseOptionsHelper.FromCSharp9);

        [TestMethod]
        public void SecurityHotspotRules_DoNotRaiseIssues_VB() =>
            VerifyNoIssueReported(AnalyzerLanguage.VisualBasic, ParseOptionsHelper.FromVisualBasic12);

        private static void VerifyNoIssueReported(AnalyzerLanguage language, ImmutableArray<ParseOptions> parseOptions)
        {
            foreach (var analyzer in GetHotspotAnalyzers(language))
            {
                var analyzerName = analyzer.GetType().Name;

                if (IsTestValid(analyzerName))
                {
                    new VerifierBuilder()
                        .AddPaths(@$"Hotspots\{GetTestCaseFileName(analyzerName)}{language.FileExtension}")
                        .AddAnalyzer(() => analyzer)
                        .WithOptions(parseOptions)
                        .AddReferences(GetAdditionalReferences(analyzerName, Constants.NuGetLatestVersion))
                        .VerifyNoIssueReported();
                }
            }
        }

        private static IEnumerable<SonarDiagnosticAnalyzer> GetHotspotAnalyzers(AnalyzerLanguage language) =>
            RuleFinder.GetAnalyzerTypes(language)
                .Where(type => typeof(SonarDiagnosticAnalyzer).IsAssignableFrom(type))   // Avoid IRuleFactory and SE rules
                .Select(type => (SonarDiagnosticAnalyzer)Activator.CreateInstance(type))
                .Where(IsSecurityHotspot);

        private static bool IsSecurityHotspot(DiagnosticAnalyzer analyzer) =>
            analyzer.SupportedDiagnostics.Any(TestHelper.IsSecurityHotspot);

        private static string GetTestCaseFileName(string analyzerName) =>
            analyzerName switch
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
                "ExecutingSqlQueries" => "ExecutingSqlQueries_EntityFrameworkCoreLatest",
                "UsingCookies" => "UsingCookies_NetCore",
                "LooseFilePermissions" => "LooseFilePermissions.Unix",
                "PermissiveCors" => "PermissiveCors.Net",
#endif
                _ => analyzerName
            };

        private static bool IsTestValid(string analyzerName) =>

#if NETFRAMEWORK
            analyzerName != nameof(DisablingCsrfProtection)
            && analyzerName != nameof(PermissiveCors);
#else
            // IdentityModel is not available on .Net Core
            analyzerName != nameof(ControllingPermissions);
#endif

        private static IEnumerable<MetadataReference> GetAdditionalReferences(string analyzerName, string version) =>
            analyzerName switch
            {
                nameof(ClearTextProtocolsAreSensitive) => ClearTextProtocolsAreSensitiveTest.AdditionalReferences,
                nameof(CookieShouldBeHttpOnly) => CookieShouldBeHttpOnlyTest.AdditionalReferences,
                nameof(CookieShouldBeSecure) => CookieShouldBeSecureTest.AdditionalReferences,
                nameof(ConfiguringLoggers) => ConfiguringLoggersTest.Log4NetReferences,
                nameof(DeliveringDebugFeaturesInProduction) => DeliveringDebugFeaturesInProductionTest.AdditionalReferencesForAspNetCore2,
                nameof(DisablingRequestValidation) => NuGetMetadataReference.MicrosoftAspNetMvc(version),
                nameof(DoNotHardcodeCredentials) => DoNotHardcodeCredentialsTest.AdditionalReferences,
                nameof(DoNotUseRandom) => MetadataReferenceFacade.SystemSecurityCryptography,
                nameof(ExpandingArchives) => ExpandingArchivesTest.AdditionalReferences,
                nameof(RequestsWithExcessiveLength) => RequestsWithExcessiveLengthTest.GetAdditionalReferences(),
                nameof(UsingRegularExpressions) => MetadataReferenceFacade.RegularExpressions,
#if NET
                nameof(DisablingCsrfProtection) => DisablingCsrfProtectionTest.AdditionalReferences(),
                nameof(ExecutingSqlQueries) => ExecutingSqlQueriesTest.GetReferencesEntityFrameworkNetCore(version),
                nameof(LooseFilePermissions) => NuGetMetadataReference.MonoPosixNetStandard(),
                nameof(PermissiveCors) => PermissiveCorsTest.AdditionalReferences,
                nameof(UsingCookies) => UsingCookies.GetAspNetCoreReferences(version),
#else
                nameof(ControllingPermissions) => ControllingPermissionsTest.AdditionalReferences,
                nameof(ExecutingSqlQueries) => ExecutingSqlQueriesTest.GetReferencesNet46(version),
                nameof(UsingCookies) => UsingCookies.GetAdditionalReferencesForNet46(),
#endif
                _ => MetadataReferenceFacade.SystemNetHttp
                                            .Concat(MetadataReferenceFacade.SystemDiagnosticsProcess)
                                            .Concat(MetadataReferenceFacade.SystemSecurityCryptography)
            };
    }
}

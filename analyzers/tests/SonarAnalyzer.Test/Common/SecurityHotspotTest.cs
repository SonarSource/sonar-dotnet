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

using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.Test.PackagingTests;
using SonarAnalyzer.Test.Rules;
using SonarAnalyzer.Test.TestFramework;

namespace SonarAnalyzer.Test.Common;

[TestClass]
public class SecurityHotspotTest
{
    [TestMethod]
    public void SecurityHotspotRules_DoNotRaiseIssues_CS() =>
        VerifyNoIssues(AnalyzerLanguage.CSharp, ParseOptionsHelper.FromCSharp9);

    [TestMethod]
    public void SecurityHotspotRules_DoNotRaiseIssues_VB() =>
        VerifyNoIssues(AnalyzerLanguage.VisualBasic, ParseOptionsHelper.FromVisualBasic12);

    private static void VerifyNoIssues(AnalyzerLanguage language, ImmutableArray<ParseOptions> parseOptions)
    {
        foreach (var analyzer in GetHotspotAnalyzers(language))
        {
            var analyzerName = analyzer.GetType().Name;

#if NETFRAMEWORK

            if (analyzerName is nameof(DisablingCsrfProtection) || analyzerName is nameof(PermissiveCors))
            {
                continue;
            }
#endif

            new VerifierBuilder()
                .AddPaths(@$"Hotspots\{GetTestCaseFileName(analyzerName)}{language.FileExtension}")
                .AddAnalyzer(() => analyzer)
                .WithOptions(parseOptions)
                .AddReferences(GetAdditionalReferences(analyzerName))
                .WithConcurrentAnalysis(analyzerName is not nameof(ClearTextProtocolsAreSensitive))
                .VerifyNoIssuesIgnoreErrors();
        }
    }

    private static IEnumerable<SonarDiagnosticAnalyzer> GetHotspotAnalyzers(AnalyzerLanguage language) =>
        RuleFinder.GetAnalyzerTypes(language)
            .Where(type => typeof(SonarDiagnosticAnalyzer).IsAssignableFrom(type))   // Avoid IRuleFactory and SE rules
            .Select(type => (SonarDiagnosticAnalyzer)Activator.CreateInstance(type))
            .Where(IsSecurityHotspot);

    private static bool IsSecurityHotspot(DiagnosticAnalyzer analyzer) =>
        analyzer.SupportedDiagnostics.Any(IsSecurityHotspot);

    private static bool IsSecurityHotspot(DiagnosticDescriptor diagnostic)
    {
        var type = RuleTypeMappingCS.Rules.GetValueOrDefault(diagnostic.Id) ?? RuleTypeMappingVB.Rules.GetValueOrDefault(diagnostic.Id);
        return type == "SECURITY_HOTSPOT";
    }

    private static string GetTestCaseFileName(string analyzerName) =>
        analyzerName switch
        {
            "ConfiguringLoggers" => "ConfiguringLoggers_Log4Net",
            "CookieShouldBeHttpOnly" => "CookieShouldBeHttpOnly_Nancy",
            "CookieShouldBeSecure" => "CookieShouldBeSecure_Nancy",
            "DoNotHardcodeCredentials" => "DoNotHardcodeCredentials.DefaultValues",
            "DeliveringDebugFeaturesInProduction" => "DeliveringDebugFeaturesInProduction.NetCore2",
#if NETFRAMEWORK
            "ExecutingSqlQueries" => "ExecutingSqlQueries.Net46",
            "UsingCookies" => "UsingCookies_Net46",
            "LooseFilePermissions" => "LooseFilePermissions.Windows",
#else
            "ExecutingSqlQueries" => "ExecutingSqlQueries.EntityFrameworkCoreLatest",
            "UsingCookies" => "UsingCookies_NetCore",
            "LooseFilePermissions" => "LooseFilePermissions.Unix",
            "PermissiveCors" => "PermissiveCors.Net",
#endif
            _ => analyzerName
        };

    private static IEnumerable<MetadataReference> GetAdditionalReferences(string analyzerName) =>
        analyzerName switch
        {
            nameof(ClearTextProtocolsAreSensitive) => ClearTextProtocolsAreSensitiveTest.AdditionalReferences,
            nameof(CookieShouldBeHttpOnly) => CookieShouldBeHttpOnlyTest.AdditionalReferences,
            nameof(CookieShouldBeSecure) => CookieShouldBeSecureTest.AdditionalReferences,
            nameof(ConfiguringLoggers) => ConfiguringLoggersTest.Log4NetReferences,
            nameof(DeliveringDebugFeaturesInProduction) => DeliveringDebugFeaturesInProductionTest.AdditionalReferencesForAspNetCore2,
            nameof(DisablingRequestValidation) => NuGetMetadataReference.MicrosoftAspNetMvc(Constants.NuGetLatestVersion),
            nameof(DoNotHardcodeCredentials) => DoNotHardcodeCredentialsTest.AdditionalReferences,
            nameof(DoNotUseRandom) => MetadataReferenceFacade.SystemSecurityCryptography,
            nameof(ExpandingArchives) => ExpandingArchivesTest.AdditionalReferences,
            nameof(RequestsWithExcessiveLength) => RequestsWithExcessiveLengthTest.GetAdditionalReferences(),
            nameof(SpecifyTimeoutOnRegex) => MetadataReferenceFacade.RegularExpressions
                .Concat(NuGetMetadataReference.SystemComponentModelAnnotations()),

#if NET
            nameof(DisablingCsrfProtection) => DisablingCsrfProtectionTest.AdditionalReferences(),
            nameof(ExecutingSqlQueries) => ExecutingSqlQueriesTest.GetReferencesEntityFrameworkNetCore("7.0.14"),
            nameof(LooseFilePermissions) => NuGetMetadataReference.MonoPosixNetStandard(),
            nameof(PermissiveCors) => PermissiveCorsTest.AdditionalReferences,
#else
            nameof(ExecutingSqlQueries) => ExecutingSqlQueriesTest.GetReferencesNet46(Constants.NuGetLatestVersion),
#endif
            _ => MetadataReferenceFacade.SystemNetHttp
                                        .Concat(MetadataReferenceFacade.SystemDiagnosticsProcess)
                                        .Concat(MetadataReferenceFacade.SystemSecurityCryptography)
        };
}

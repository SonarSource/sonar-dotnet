/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using System.Reflection;
using SonarAnalyzer.Rules;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.Test.Rules;

namespace SonarAnalyzer.Test.Common;

[TestClass]
public class SecurityHotspotTest
{
    [TestMethod]
    public void SecurityHotspotRules_DoNotRaiseIssues_CS() =>
        VerifyNoIssues(AnalyzerLanguage.CSharp, LanguageOptions.FromCSharp9);

    [TestMethod]
    public void SecurityHotspotRules_DoNotRaiseIssues_VB() =>
        VerifyNoIssues(AnalyzerLanguage.VisualBasic, LanguageOptions.FromVisualBasic12);

    private static void VerifyNoIssues(AnalyzerLanguage language, ImmutableArray<ParseOptions> parseOptions)
    {
        foreach (var analyzer in CreateHotspotAnalyzers(language))
        {
            var analyzerName = analyzer.GetType().Name;

#if NETFRAMEWORK

            if (analyzerName is nameof(DisablingCsrfProtection) || analyzerName is nameof(PermissiveCors))
            {
                continue;
            }
#endif

            new VerifierBuilder()
                .AddPaths(@$"Hotspots\{TestCaseFileName(analyzerName)}{language.FileExtension}")
                .AddAnalyzer(() => analyzer)
                .WithOptions(parseOptions)
                .AddReferences(AdditionalReferences(analyzerName))
                .WithConcurrentAnalysis(analyzerName is not nameof(ClearTextProtocolsAreSensitive))
                .VerifyNoIssuesIgnoreErrors();
        }
    }

    private static IEnumerable<SonarDiagnosticAnalyzer> CreateHotspotAnalyzers(AnalyzerLanguage language) =>
        new[] { typeof(CSharp.Metrics.CSharpMetrics), typeof(VisualBasic.Metrics.VisualBasicMetrics) }  // Any type from those assemblies
            .SelectMany(x => x.Assembly.GetExportedTypes())
            .Where(x => x.IsSubclassOf(typeof(DiagnosticAnalyzer))
                        && !typeof(UtilityAnalyzerBase).IsAssignableFrom(x)
                        && x.GetCustomAttributes<DiagnosticAnalyzerAttribute>().Any())
            .Where(x => typeof(SonarDiagnosticAnalyzer).IsAssignableFrom(x) && x.AnalyzerTargetLanguage() == language)   // Avoid IRuleFactory and SE rules
            .Select(x => (SonarDiagnosticAnalyzer)Activator.CreateInstance(x))
            .Where(IsSecurityHotspot);

    private static bool IsSecurityHotspot(DiagnosticAnalyzer analyzer) =>
        analyzer.SupportedDiagnostics.Any(x => x.IsSecurityHotspot());

    private static string TestCaseFileName(string analyzerName) =>
        analyzerName switch
        {
            "ConfiguringLoggers" => "ConfiguringLoggers_Log4Net",
            "CookieShouldBeHttpOnly" => "CookieShouldBeHttpOnly_Nancy",
            "CookieShouldBeSecure" => "CookieShouldBeSecure_Nancy",
            "DeliveringDebugFeaturesInProduction" => "DeliveringDebugFeaturesInProduction.NetCore2",
            "DoNotHardcodeCredentials" => "DoNotHardcodeCredentials.DefaultValues",
#if NETFRAMEWORK
            "ExecutingSqlQueries" => "ExecutingSqlQueries.Net46",
            "LooseFilePermissions" => "LooseFilePermissions.Windows",
            "UsingCookies" => "UsingCookies_Net46",
#else
            "DisablingCsrfProtection" => "DisablingCsrfProtection.Latest",
            "ExecutingSqlQueries" => "ExecutingSqlQueries.EntityFrameworkCoreLatest",
            "LooseFilePermissions" => "LooseFilePermissions.Unix",
            "PermissiveCors" => "PermissiveCors.Latest",
            "UsingCookies" => "UsingCookies_NetCore",
#endif
            _ => analyzerName
        };

    private static IEnumerable<MetadataReference> AdditionalReferences(string analyzerName) =>
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

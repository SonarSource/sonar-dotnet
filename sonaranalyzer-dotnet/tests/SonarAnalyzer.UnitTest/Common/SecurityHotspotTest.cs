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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.PackagingTests;
using SonarAnalyzer.UnitTest.Rules;
using SonarAnalyzer.UnitTest.TestFramework;
using SonarAnalyzer.Utilities;

namespace SonarAnalyzer.UnitTest.Common
{
    [TestClass]
    public class SecurityHotspotTest
    {
        [TestMethod]
        public void SecurityHotspotRules_AreDisabledByDefault()
        {
            VerifyNoIssueReported(AnalyzerLanguage.CSharp);
            VerifyNoIssueReported(AnalyzerLanguage.VisualBasic);
        }

        private static void VerifyNoIssueReported(AnalyzerLanguage language)
        {
            foreach (var analyzer in GetHotspotAnalyzers(language).Where(IsValid))
            {
                Verifier.VerifyNoIssueReported($"TestCases\\{GetTestCaseFileName(analyzer)}.{language.FileExtension}",
                                               analyzer,
                                               additionalReferences: GetAdditionalReferences());
            }
        }

        private static string GetTestCaseFileName(DiagnosticAnalyzer analyzer)
        {
            var typeName = analyzer.GetType().Name;

            return typeName switch
            {
                "ConfiguringLoggers" => "ConfiguringLoggers_AspNetCore",
                "CookieShouldBeHttpOnly" => "CookieShouldBeHttpOnly_Nancy",
                "CookieShouldBeSecure" => "CookiesShouldBeSecure_Nancy",
                "DoNotHardcodeCredentials" => "DoNotHardcodeCredentials_DefaultValues",
#if NETFRAMEWORK
                "ExecutingSqlQueries" => "ExecutingSqlQueries_Net46",
                "UsingCookies" => "UsingCookies_Net46",
#else
                "ExecutingSqlQueries" => "ExecutingSqlQueries_NetCore",
                "UsingCookies" => "UsingCookies_NetCore",
#endif
                _ => typeName
            };
        }

        private static bool IsValid(DiagnosticAnalyzer analyzer)
        {
#if NETFRAMEWORK
            return true;
#else
            // IdentityModel is not available on .Net Core
            return analyzer.GetType().Name != "ControllingPermissions";
#endif
        }

        private static IEnumerable<SonarDiagnosticAnalyzer> GetHotspotAnalyzers(AnalyzerLanguage language) =>
            new RuleFinder().GetAnalyzerTypes(language)
                            .Where(type => type.Implements(typeof(DiagnosticAnalyzer)))
                            .Select(type => (SonarDiagnosticAnalyzer)Activator.CreateInstance(type))
                            .Where(IsHotSpot);

        private static bool IsHotSpot(DiagnosticAnalyzer analyzer) =>
            analyzer.SupportedDiagnostics
                    .Any(diagnostic => CsRuleTypeMapping.RuleTypesCs.GetValueOrDefault(diagnostic.Id.TrimStart('S')) == "SECURITY_HOTSPOT");

        private static IEnumerable<MetadataReference> GetAdditionalReferences() =>
            ConfiguringLoggersTest.AspNetCoreLoggingReferences
#if NETFRAMEWORK
                                  .Concat(ControllingPermissionsTest.AdditionalReferences)
                                  .Concat(ExecutingSqlQueriesTest.GetReferencesNet46(Constants.NuGetLatestVersion))
                                  .Concat(UsingCookies.GetAdditionalReferencesForNet46())
#else
                                  .Concat(ExecutingSqlQueriesTest.GetReferencesNetCore(Constants.DotNetCore220Version))
                                  .Concat(UsingCookies.GetAdditionalReferencesForNetCore(Constants.DotNetCore220Version))
#endif
                                  .Concat(DeliveringDebugFeaturesInProductionTest.AdditionalReferences)
                                  .Concat(ExpandingArchivesTest.AdditionalReferences)
                                  .Concat(NuGetMetadataReference.Nancy());// Needed by CookieShouldBeHttpOnly, CookiesShouldBeSecure
    }
}

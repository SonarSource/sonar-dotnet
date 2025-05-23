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

#if NETFRAMEWORK
using System.IO;
#endif

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class CookieShouldBeHttpOnlyTest
    {

#if NETFRAMEWORK

        private const string WebConfig = "Web.config";

#endif

        private readonly VerifierBuilder builder = new VerifierBuilder().WithBasePath("Hotspots").AddAnalyzer(() => new CookieShouldBeHttpOnly(AnalyzerConfiguration.AlwaysEnabled));

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CookiesShouldBeHttpOnly_Nancy() =>
            builder.AddPaths("CookieShouldBeHttpOnly_Nancy.cs")
                .AddReferences(AdditionalReferences)
                .Verify();

#if NETFRAMEWORK // The analyzed code is valid only for .Net Framework

        [TestMethod]
        public void CookiesShouldBeHttpOnly() =>
            builder.AddPaths("CookieShouldBeHttpOnly.cs")
                .AddReferences(MetadataReferenceFacade.SystemWeb)
                .Verify();

        [DataTestMethod]
        [DataRow(@"TestCases\WebConfig\CookieShouldBeHttpOnly\HttpOnlyCookiesConfig")]
        [DataRow(@"TestCases\WebConfig\CookieShouldBeHttpOnly\Formatting")]
        public void CookiesShouldBeHttpOnly_WithWebConfigValueSetToTrue(string root)
        {
            var webConfigPath = Path.Combine(root, WebConfig);
            builder.AddPaths("CookieShouldBeHttpOnly_WithWebConfig.cs")
                .AddReferences(MetadataReferenceFacade.SystemWeb)
                .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, webConfigPath))
                .Verify();
        }

        [DataTestMethod]
        [DataRow(@"TestCases\WebConfig\CookieShouldBeHttpOnly\NonHttpOnlyCookiesConfig")]
        [DataRow(@"TestCases\WebConfig\CookieShouldBeHttpOnly\UnrelatedConfig")]
        [DataRow(@"TestCases\WebConfig\CookieShouldBeHttpOnly\ConfigWithoutAttribute")]
        public void CookiesShouldBeHttpOnly_WithWebConfigValueSetToFalse(string root)
        {
            var webConfigPath = Path.Combine(root, WebConfig);
            builder.AddPaths("CookieShouldBeHttpOnly.cs")
                .AddReferences(MetadataReferenceFacade.SystemWeb)
                .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, webConfigPath))
                .Verify();
        }

#else

        [TestMethod]
        public void CookiesShouldBeHttpOnly_NetCore() =>
            builder.AddPaths("CookieShouldBeHttpOnly_NetCore.cs")
                .AddReferences(GetAdditionalReferences_NetCore())
                .Verify();

        [TestMethod]
        public void CookiesShouldBeHttpOnly_CSharp9() =>
            builder.AddPaths("CookieShouldBeHttpOnly.CSharp9.cs")
                .WithTopLevelStatements()
                .AddReferences(GetAdditionalReferences_NetCore())
                .AddReferences(NuGetMetadataReference.Nancy())
                .Verify();

        [TestMethod]
        public void CookiesShouldBeHttpOnly_CSharp10() =>
            builder.AddPaths("CookieShouldBeHttpOnly.CSharp10.cs")
                .WithTopLevelStatements()
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .AddReferences(GetAdditionalReferences_NetCore())
                .AddReferences(NuGetMetadataReference.Nancy())
                .Verify();

        private static IEnumerable<MetadataReference> GetAdditionalReferences_NetCore() =>
            NuGetMetadataReference.MicrosoftAspNetCoreHttpFeatures(Constants.NuGetLatestVersion);

#endif

        internal static IEnumerable<MetadataReference> AdditionalReferences =>
            NuGetMetadataReference.Nancy();
    }
}

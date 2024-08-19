/*
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
    public class CookieShouldBeSecureTest
    {

#if NETFRAMEWORK

        private const string WebConfig = "Web.config";

#endif

        private readonly VerifierBuilder builder = new VerifierBuilder().WithBasePath("Hotspots").AddAnalyzer(() => new CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled));

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CookiesShouldBeSecure_Nancy() =>
            builder.AddPaths("CookieShouldBeSecure_Nancy.cs")
                .AddReferences(AdditionalReferences)
                .Verify();

#if NETFRAMEWORK // HttpCookie is not available on .Net Core

        [TestMethod]
        public void CookiesShouldBeSecure() =>
             builder.AddPaths("CookieShouldBeSecure.cs")
                .AddReferences(MetadataReferenceFacade.SystemWeb)
                .Verify();

        [DataTestMethod]
        [DataRow(@"TestCases\WebConfig\CookieShouldBeSecure\SecureCookieConfig")]
        [DataRow(@"TestCases\WebConfig\CookieShouldBeSecure\Formatting")]
        public void CookiesShouldBeSecure_WithWebConfigValueSetToTrue(string root)
        {
            var webConfigPath = Path.Combine(root, WebConfig);

            builder.AddPaths("CookieShouldBeSecure_WithWebConfig.cs")
                .AddReferences(MetadataReferenceFacade.SystemWeb)
                .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, webConfigPath))
                .Verify();
        }

        [DataTestMethod]
        [DataRow(@"TestCases\WebConfig\CookieShouldBeSecure\NonSecureCookieConfig")]
        [DataRow(@"TestCases\WebConfig\CookieShouldBeSecure\UnrelatedConfig")]
        [DataRow(@"TestCases\WebConfig\CookieShouldBeSecure\ConfigWithoutAttribute")]
        public void CookiesShouldBeSecure_WithWebConfigValueSetToFalse(string root)
        {
            var webConfigPath = Path.Combine(root, WebConfig);
            builder.AddPaths("CookieShouldBeSecure.cs")
                .AddReferences(MetadataReferenceFacade.SystemWeb)
                .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, webConfigPath))
                .Verify();
        }

#else

        [TestMethod]
        public void CookiesShouldBeSecure_NetCore() =>
             builder.AddPaths("CookieShouldBeSecure_NetCore.cs")
                .AddReferences(GetAdditionalReferences_NetCore())
                .Verify();

        [TestMethod]
        public void CookiesShouldBeSecure_CSharp9() =>
            builder.AddPaths("CookieShouldBeSecure.CSharp9.cs")
                .WithTopLevelStatements()
                .AddReferences(GetAdditionalReferences_NetCore())
                .AddReferences(NuGetMetadataReference.Nancy())
                .Verify();

        [TestMethod]
        public void CookiesShouldBeSecure_CSharp10() =>
            builder.AddPaths("CookieShouldBeSecure.CSharp10.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .WithTopLevelStatements()
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

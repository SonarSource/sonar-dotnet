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

#if NET

using System.IO;
using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class DatabasePasswordsShouldBeSecureTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<DatabasePasswordsShouldBeSecure>();

        public TestContext TestContext { get; set; }

        [DataTestMethod]
        [DataRow("3.1.11", "3.19.80")]
        [DataRow("5.0.2", "5.21.1")]
        public void DatabasePasswordsShouldBeSecure_CS(string entityFrameworkCoreVersion, string oracleVersion) =>
            builder.AddPaths("DatabasePasswordsShouldBeSecure.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .AddReferences(GetReferences(entityFrameworkCoreVersion, oracleVersion))
                .Verify();

        [DataTestMethod]
        [DataRow("3.1.11", "3.19.80")]
        [DataRow("5.0.2", "5.21.1")]
        public void DatabasePasswordsShouldBeSecure_CSharp11_CS(string entityFrameworkCoreVersion, string oracleVersion) =>
            builder.AddPaths("DatabasePasswordsShouldBeSecure.CSharp11.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .AddReferences(GetReferences(entityFrameworkCoreVersion, oracleVersion))
                .Verify();

        [TestMethod]
        public void DatabasePasswordsShouldBeSecure_Net5_CS() =>
            builder.AddPaths("DatabasePasswordsShouldBeSecure.Net5.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .AddReferences(GetReferences("5.0.2", "5.21.1"))
                .Verify();

        [TestMethod]
        public void DatabasePasswordsShouldBeSecure_NetCore3_CS() =>
            builder.AddPaths("DatabasePasswordsShouldBeSecure.NetCore31.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .AddReferences(GetReferences("3.1.11", "3.19.80"))
                .Verify();

        [DataTestMethod]
        [DataRow(@"TestCases\WebConfig\DatabasePasswordsShouldBeSecure\Values")]
        [DataRow(@"TestCases\WebConfig\DatabasePasswordsShouldBeSecure\UnexpectedContent")]
        public void DatabasePasswordsShouldBeSecure_CS_WebConfig(string root)
        {
            var webConfigPath = GetWebConfigPath(root);
            DiagnosticVerifier.Verify(
                CreateCompilation(),
                new DatabasePasswordsShouldBeSecure(),
                AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, webConfigPath),
                null,
                [webConfigPath]);
        }

        [TestMethod]
        public void DatabasePasswordsShouldBeSecure_CS_ExternalConnection()
        {
            var root = @"TestCases\WebConfig\DatabasePasswordsShouldBeSecure\ExternalConfig";
            var webConfigPath = GetWebConfigPath(root);
            var externalConfigPath = Path.Combine(root, "external.config");
            DiagnosticVerifier.Verify(
                CreateCompilation(),
                new DatabasePasswordsShouldBeSecure(),
                AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, webConfigPath, externalConfigPath),
                null,
                [webConfigPath]);
        }

        [TestMethod]
        public void DatabasePasswordsShouldBeSecure_CS_CorruptAndNonExistingWebConfigs_ShouldNotFail()
        {
            var root = @"TestCases\WebConfig\DatabasePasswordsShouldBeSecure\Corrupt";
            var missingDirectory = @"TestCases\WebConfig\DatabasePasswordsShouldBeSecure\NonExistingDirectory";
            var corruptFilePath = GetWebConfigPath(root);
            var nonExistingFilePath = GetWebConfigPath(missingDirectory);
            DiagnosticVerifier.Verify(
                CreateCompilation(),
                new DatabasePasswordsShouldBeSecure(),
                AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, corruptFilePath, nonExistingFilePath),
                null,
                [corruptFilePath]);
        }

        [DataTestMethod]
        [DataRow(@"TestCases\AppSettings\DatabasePasswordsShouldBeSecure\Values")]
        [DataRow(@"TestCases\AppSettings\DatabasePasswordsShouldBeSecure\UnexpectedContent\ArrayInside")]
        [DataRow(@"TestCases\AppSettings\DatabasePasswordsShouldBeSecure\UnexpectedContent\EmptyArray")]
        [DataRow(@"TestCases\AppSettings\DatabasePasswordsShouldBeSecure\UnexpectedContent\EmptyFile")]
        [DataRow(@"TestCases\AppSettings\DatabasePasswordsShouldBeSecure\UnexpectedContent\WrongStructure")]
        [DataRow(@"TestCases\AppSettings\DatabasePasswordsShouldBeSecure\UnexpectedContent\ConnectionStringComment")]
        [DataRow(@"TestCases\AppSettings\DatabasePasswordsShouldBeSecure\UnexpectedContent\ValueKind")]
        [DataRow(@"TestCases\AppSettings\DatabasePasswordsShouldBeSecure\UnexpectedContent\PropertyKinds")]
        [DataRow(@"TestCases\AppSettings\DatabasePasswordsShouldBeSecure\UnexpectedContent\Null")]
        public void DatabasePasswordsShouldBeSecure_CS_AppSettings(string root)
        {
            var appSettingsPath = GetAppSettingsPath(root);
            DiagnosticVerifier.Verify(
                CreateCompilation(),
                new DatabasePasswordsShouldBeSecure(),
                AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, appSettingsPath),
                null,
                [appSettingsPath]);
        }

        [TestMethod]
        public void DatabasePasswordsShouldBeSecure_CS_CorruptAndNonExistingAppSettings_ShouldNotFail()
        {
            var root = @"TestCases\AppSettings\DatabasePasswordsShouldBeSecure\Corrupt";
            var missingDirectory = @"TestCases\AppSettings\DatabasePasswordsShouldBeSecure\NonExistingDirectory";
            var corruptFilePath = GetAppSettingsPath(root);
            var nonExistingFilePath = GetAppSettingsPath(missingDirectory);
            DiagnosticVerifier.Verify(
                CreateCompilation(),
                new DatabasePasswordsShouldBeSecure(),
                AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, corruptFilePath, nonExistingFilePath),
                null,
                [corruptFilePath]);
        }

        private static string GetWebConfigPath(string rootFolder) => Path.Combine(rootFolder, "Web.config");

        private static string GetAppSettingsPath(string rootFolder) => Path.Combine(rootFolder, "appsettings.json");

        private static Compilation CreateCompilation() => SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).GetCompilation();

        private static IEnumerable<MetadataReference> GetReferences(string entityFrameworkCoreVersion, string oracleVersion) =>
            Enumerable.Empty<MetadataReference>()
                      .Concat(MetadataReferenceFacade.SystemData)
                      .Concat(MetadataReferenceFacade.SystemComponentModelPrimitives)
                      .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCore(entityFrameworkCoreVersion))
                      .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCoreSqliteCore(entityFrameworkCoreVersion))
                      .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCoreSqlServer(entityFrameworkCoreVersion))
                      .Concat(NuGetMetadataReference.OracleEntityFrameworkCore(oracleVersion))
                      .Concat(NuGetMetadataReference.MySqlDataEntityFrameworkCore())
                      .Concat(NuGetMetadataReference.NpgsqlEntityFrameworkCorePostgreSQL(entityFrameworkCoreVersion));
    }
}

#endif

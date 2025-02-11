/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

#if NET

using System.IO;
using SonarAnalyzer.CSharp.Rules;

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
                .WithOptions(LanguageOptions.FromCSharp8)
                .AddReferences(GetReferences(entityFrameworkCoreVersion, oracleVersion))
                .Verify();

        [DataTestMethod]
        [DataRow("3.1.11", "3.19.80")]
        [DataRow("5.0.2", "5.21.1")]
        public void DatabasePasswordsShouldBeSecure_CS_Latest(string entityFrameworkCoreVersion, string oracleVersion) =>
            builder.AddPaths("DatabasePasswordsShouldBeSecure.Latest.cs")
                .WithOptions(LanguageOptions.CSharpLatest)
                .AddReferences(GetReferences(entityFrameworkCoreVersion, oracleVersion))
                .Verify();

        [TestMethod]
        public void DatabasePasswordsShouldBeSecure_Net5_CS() =>
            builder.AddPaths("DatabasePasswordsShouldBeSecure.Net5.cs")
                .WithOptions(LanguageOptions.FromCSharp8)
                .AddReferences(GetReferences("5.0.2", "5.21.1"))
                .Verify();

        [TestMethod]
        public void DatabasePasswordsShouldBeSecure_NetCore3_CS() =>
            builder.AddPaths("DatabasePasswordsShouldBeSecure.NetCore31.cs")
                .WithOptions(LanguageOptions.FromCSharp8)
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

/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class SqlKeywordsDelimitedBySpaceTest
{
    private static readonly VerifierBuilder Builder = new VerifierBuilder<SqlKeywordsDelimitedBySpace>()
        .AddReferences(NuGetMetadataReference.SystemDataSqlClient());

    [TestMethod]
    public void SqlKeywordsDelimitedBySpace_Csharp8() =>
        Builder.AddPaths("SqlKeywordsDelimitedBySpace.cs")
            .WithOptions(LanguageOptions.FromCSharp8)
            .Verify();

    [TestMethod]
    public void SqlKeywordsDelimitedBySpace_UsingInsideNamespace() =>
        Builder.AddPaths("SqlKeywordsDelimitedBySpace_InsideNamespace.cs")
            .WithConcurrentAnalysis(false)
            .Verify();

    [TestMethod]
    public void SqlKeywordsDelimitedBySpace_DefaultNamespace() =>
        Builder.AddPaths("SqlKeywordsDelimitedBySpace_DefaultNamespace.cs")
            .AddTestReference()
            .VerifyNoIssues();

#if NET

    [TestMethod]
    public void SqlKeywordsDelimitedBySpace_CSharp10_GlobalUsings() =>
        Builder.AddPaths("SqlKeywordsDelimitedBySpace.CSharp10.GlobalUsing.cs", "SqlKeywordsDelimitedBySpace.CSharp10.GlobalUsingConsumer.cs")
            .WithOptions(LanguageOptions.FromCSharp10)
            .WithConcurrentAnalysis(false)
            .VerifyNoIssues();

    [TestMethod]
    public void SqlKeywordsDelimitedBySpace_CSharp10_FileScopesNamespace() =>
        Builder.AddPaths("SqlKeywordsDelimitedBySpace.CSharp10.FileScopedNamespaceDeclaration.cs")
            .WithOptions(LanguageOptions.FromCSharp10)
            .WithConcurrentAnalysis(false)
            .Verify();

    [TestMethod]
    public void SqlKeywordsDelimitedBySpace_Latest() =>
        Builder.AddPaths("SqlKeywordsDelimitedBySpace.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .WithConcurrentAnalysis(false)
            .Verify();

#endif

    [DataRow("System.Data")]
    [DataRow("System.Data.SqlClient")]
    [DataRow("System.Data.SQLite")]
    [DataRow("System.Data.SqlServerCe")]
    [DataRow("System.Data.Entity")]
    [DataRow("System.Data.Odbc")]
    [DataRow("Dapper")]
    [DataRow("Microsoft.Data.SqlClient")]
    [DataRow("Microsoft.Data.Sqlite")]
    [DataRow("NHibernate")]
    [DataRow("PetaPoco")]
    [TestMethod]
    public void SqlKeywordsDelimitedBySpace_DotnetFramework(string sqlNamespace) =>
        Builder
            .AddReferences(MetadataReferenceFacade.SystemData)
            .AddReferences(NuGetMetadataReference.Dapper())
            .AddReferences(NuGetMetadataReference.EntityFramework())
            .AddReferences(NuGetMetadataReference.MicrosoftDataSqlClient())
            .AddReferences(NuGetMetadataReference.MicrosoftDataSqliteCore())
            .AddReferences(NuGetMetadataReference.MicrosoftSqlServerCompact())
            .AddReferences(NuGetMetadataReference.NHibernate())
            .AddReferences(NuGetMetadataReference.PetaPocoCompiled())
            .AddReferences(NuGetMetadataReference.SystemDataOdbc())
            .AddReferences(NuGetMetadataReference.SystemDataSQLiteCore())
            .AddSnippet($@"
using {sqlNamespace};
namespace TestNamespace
{{
    public class Test
    {{
        private string field = ""SELECT * FROM table"" +
            ""WHERE col ="" + // Noncompliant
            ""val"";
    }}
}}").Verify();

    [DataRow("System.Data.SqlClient")]
    [DataRow("System.Data.OracleClient")]
    [DataRow("Microsoft.EntityFrameworkCore")]
    [DataRow("ServiceStack.OrmLite")]
    [TestMethod]
    public void SqlKeywordsDelimitedBySpace_DotnetCore(string sqlNamespace) =>
        Builder
            .AddReferences(MetadataReferenceFacade.SystemData)
            .AddReferences(NuGetMetadataReference.MicrosoftEntityFrameworkCore("2.2.0"))
            .AddReferences(NuGetMetadataReference.ServiceStackOrmLite())
            .AddReferences(NuGetMetadataReference.SystemDataOracleClient())
            .AddSnippet($@"
using {sqlNamespace};
namespace TestNamespace
{{
    public class Test
    {{
        private string field = ""SELECT * FROM table"" +
            ""WHERE col ="" + // Noncompliant
            ""val"";
    }}
}}").Verify();
}

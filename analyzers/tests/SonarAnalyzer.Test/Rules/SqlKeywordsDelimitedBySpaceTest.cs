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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class SqlKeywordsDelimitedBySpaceTest
{
    private static readonly VerifierBuilder Builder = new VerifierBuilder<SqlKeywordsDelimitedBySpace>()
        .AddReferences(NuGetMetadataReference.SystemDataSqlClient());

    [TestMethod]
    public void SqlKeywordsDelimitedBySpace_Csharp8() =>
        Builder.AddPaths("SqlKeywordsDelimitedBySpace.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp8)
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
            .WithOptions(ParseOptionsHelper.FromCSharp10)
            .WithConcurrentAnalysis(false)
            .VerifyNoIssues();

    [TestMethod]
    public void SqlKeywordsDelimitedBySpace_CSharp10_FileScopesNamespace() =>
        Builder.AddPaths("SqlKeywordsDelimitedBySpace.CSharp10.FileScopedNamespaceDeclaration.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp10)
            .WithConcurrentAnalysis(false)
            .Verify();

    [TestMethod]
    public void SqlKeywordsDelimitedBySpace_Latest() =>
        Builder.AddPaths("SqlKeywordsDelimitedBySpace.Latest.cs")
            .WithOptions(ParseOptionsHelper.CSharpLatest)
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
    [DataTestMethod]
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
    [DataTestMethod]
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

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

using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class ExecutingSqlQueriesTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder().AddAnalyzer(() => new CS.ExecutingSqlQueries(AnalyzerConfiguration.AlwaysEnabled)).WithBasePath("Hotspots");
    private readonly VerifierBuilder builderVB = new VerifierBuilder().AddAnalyzer(() => new VB.ExecutingSqlQueries(AnalyzerConfiguration.AlwaysEnabled)).WithBasePath("Hotspots");

    [TestMethod]
    public void ExecutingSqlQueries_CS_Dapper() =>
        builderCS
            .AddPaths("ExecutingSqlQueries.Dapper.cs")
            .AddReferences(MetadataReferenceFacade.SystemData)
            .AddReferences(NuGetMetadataReference.Dapper())
            .Verify();

    [TestMethod]
    public void ExecutingSqlQueries_CS_MySqlData() =>
        builderCS
            .AddPaths("ExecutingSqlQueries.MySqlData.cs")
            .AddReferences(MetadataReferenceFacade.SystemData)
            .AddReferences(MetadataReferenceFacade.SystemComponentModelPrimitives)
            .AddReferences(NuGetMetadataReference.Dapper("2.1.35"))
            .AddReferences(NuGetMetadataReference.MySqlData("9.0.0"))
            .Verify();

    [TestMethod]
    public void ExecutingSqlQueries_CS_EF6() =>
        builderCS.AddPaths("ExecutingSqlQueries.EF6.cs")
            .AddReferences(NuGetMetadataReference.EntityFramework())
            .Verify();

    [TestMethod]
    public void ExecutingSqlQueries_OrmLite_CS() =>
        builderCS
            .AddPaths(@"ExecutingSqlQueries.OrmLite.cs")
            .AddReferences(MetadataReferenceFacade.SystemData)
            .AddReferences(NuGetMetadataReference.ServiceStackOrmLite(TestConstants.NuGetLatestVersion))
            .Verify();

    [TestMethod]
    public void ExecutingSqlQueries_NHibernate_CS() =>
        builderCS
            .AddPaths("ExecutingSqlQueries.NHibernate.cs")
            .AddReferences(NuGetMetadataReference.NHibernate(TestConstants.NuGetLatestVersion))
            .Verify();

#if NETFRAMEWORK // System.Data.OracleClient.dll is not available on .Net Core

    [TestMethod]
    public void ExecutingSqlQueries_CS_Net46() =>
        builderCS
            .AddPaths("ExecutingSqlQueries.Net46.cs")
            .AddReferences(GetReferencesNet46(TestConstants.NuGetLatestVersion))
            .Verify();

    [TestMethod]
    public void ExecutingSqlQueries_VB_Net46() =>
        builderVB
            .AddPaths("ExecutingSqlQueries.Net46.vb")
            .WithOptions(LanguageOptions.FromVisualBasic15)
            .AddReferences(GetReferencesNet46(TestConstants.NuGetLatestVersion))
            .Verify();

    [TestMethod]
    public void ExecutingSqlQueries_MonoSqlLite_Net46_CS() =>
        builderCS
            .AddPaths("ExecutingSqlQueries.Net46.MonoSqlLite.cs")
            .AddReferences(FrameworkMetadataReference.SystemData)
            .AddReferences(NuGetMetadataReference.MonoDataSqlite())
            .Verify();

    [TestMethod]
    public void ExecutingSqlQueries_MonoSqlLite_Net46_VB() =>
        builderVB
            .AddPaths("ExecutingSqlQueries.Net46.MonoSqlLite.vb")
            .AddReferences(FrameworkMetadataReference.SystemData)
            .AddReferences(NuGetMetadataReference.MonoDataSqlite())
            .WithOptions(LanguageOptions.FromVisualBasic14)
            .Verify();

    internal static IEnumerable<MetadataReference> GetReferencesNet46(string sqlServerCeVersion) =>
        MetadataReferenceFacade.NetStandard
            .Concat(FrameworkMetadataReference.SystemData)
            .Concat(FrameworkMetadataReference.SystemDataOracleClient)
            .Concat(NuGetMetadataReference.SystemDataSqlServerCe(sqlServerCeVersion))
            .Concat(NuGetMetadataReference.MySqlData("8.0.22"))
            .Concat(NuGetMetadataReference.MicrosoftDataSqliteCore())
            .Concat(NuGetMetadataReference.SystemDataSQLiteCore());

#else

    [TestMethod]
    public void ExecutingSqlQueries_CS_EntityFrameworkCore2() =>
        builderCS
            .AddPaths("ExecutingSqlQueries.EntityFrameworkCore2.cs")
            .WithOptions(LanguageOptions.FromCSharp8)
            .AddReferences(GetReferencesEntityFrameworkNetCore("2.2.6").Concat(NuGetMetadataReference.SystemComponentModelTypeConverter()))
            .Verify();

    [TestMethod]
    public void ExecutingSqlQueries_CS_EntityFrameworkCore7() =>
        builderCS
            .AddPaths("ExecutingSqlQueries.EntityFrameworkCoreLatest.cs")
            .WithOptions(LanguageOptions.FromCSharp8)
            .AddReferences(GetReferencesEntityFrameworkNetCore("7.0.14"))
            .Verify();

    [TestMethod]
    public void ExecutingSqlQueries_CS_Latest() =>
        builderCS.AddPaths("ExecutingSqlQueries.Latest.cs")
            .WithTopLevelStatements()
            .WithOptions(LanguageOptions.CSharpLatest)
            .AddReferences(GetReferencesEntityFrameworkNetCore(TestConstants.DotNetCore220Version).Concat(NuGetMetadataReference.MicrosoftDataSqliteCore()))
            .Verify();

    [TestMethod]
    public void ExecutingSqlQueries_VB_EntityFrameworkCore2() =>
        builderVB
            .AddPaths(@"ExecutingSqlQueries.EntityFrameworkCore2.vb")
            .WithOptions(LanguageOptions.FromVisualBasic15)
            .AddReferences(GetReferencesEntityFrameworkNetCore(TestConstants.DotNetCore220Version))
            .Verify();

    [TestMethod]
    public void ExecutingSqlQueries_VB_EntityFrameworkCore7() =>
        builderVB
            .AddPaths(@"ExecutingSqlQueries.EntityFrameworkCoreLatest.vb")
            .WithOptions(LanguageOptions.FromVisualBasic15)
            .AddReferences(GetReferencesEntityFrameworkNetCore("7.0.14"))
            .Verify();

    internal static IEnumerable<MetadataReference> GetReferencesEntityFrameworkNetCore(string entityFrameworkVersion) =>
        Enumerable.Empty<MetadataReference>()
            .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCore(entityFrameworkVersion))
            .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCoreRelational(entityFrameworkVersion));

#endif
}

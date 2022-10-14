/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using SonarAnalyzer.Common;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class ExecutingSqlQueriesTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder().AddAnalyzer(() => new CS.ExecutingSqlQueries(AnalyzerConfiguration.AlwaysEnabled)).WithBasePath("Hotspots");
        private readonly VerifierBuilder builderVB = new VerifierBuilder().AddAnalyzer(() => new VB.ExecutingSqlQueries(AnalyzerConfiguration.AlwaysEnabled)).WithBasePath("Hotspots");

#if NETFRAMEWORK // System.Data.OracleClient.dll is not available on .Net Core

        [TestMethod]
        public void ExecutingSqlQueries_CS_Net46() =>
            builderCS
                .AddPaths(@"ExecutingSqlQueries.Net46.cs")
                .AddReferences(GetReferencesNet46(Constants.NuGetLatestVersion))
                .Verify();

        [TestMethod]
        public void ExecutingSqlQueries_VB_Net46() =>
            builderVB
                .AddPaths(@"ExecutingSqlQueries.Net46.vb")
                .WithOptions(ParseOptionsHelper.FromVisualBasic15)
                .AddReferences(GetReferencesNet46(Constants.NuGetLatestVersion))
                .Verify();

        internal static IEnumerable<MetadataReference> GetReferencesNet46(string sqlServerCeVersion) =>
            NetStandardMetadataReference.Netstandard
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
                .AddPaths(@"ExecutingSqlQueries.EntityFrameworkCore2.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .AddReferences(GetReferencesEntityFrameworkNetCore("2.2.6").Concat(NuGetMetadataReference.SystemComponentModelTypeConverter()))
                .Verify();

        [TestMethod]
        public void ExecutingSqlQueries_CS_EntityFrameworkCoreLatest() =>
            builderCS
                .AddPaths(@"ExecutingSqlQueries.EntityFrameworkCoreLatest.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .AddReferences(GetReferencesEntityFrameworkNetCore(Constants.NuGetLatestVersion))
                .Verify();

        [TestMethod]
        public void ExecutingSqlQueries_CSharp9() =>
            builderCS
                .AddPaths(@"ExecutingSqlQueries.CSharp9.cs")
                .WithTopLevelStatements()
                .AddReferences(GetReferencesEntityFrameworkNetCore(Constants.DotNetCore220Version).Concat(NuGetMetadataReference.MicrosoftDataSqliteCore()))
                .Verify();

        [TestMethod]
        public void ExecutingSqlQueries_CSharp10() =>
            builderCS
                .AddPaths(@"ExecutingSqlQueries.CSharp10.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .WithTopLevelStatements()
                .AddReferences(GetReferencesEntityFrameworkNetCore(Constants.DotNetCore220Version).Concat(NuGetMetadataReference.MicrosoftDataSqliteCore()))
                .Verify();

        [TestMethod]
        public void ExecutingSqlQueries_CSharp11() =>
            builderCS
                .AddPaths(@"ExecutingSqlQueries.CSharp11.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .WithTopLevelStatements()
                .AddReferences(GetReferencesEntityFrameworkNetCore(Constants.DotNetCore220Version).Concat(NuGetMetadataReference.MicrosoftDataSqliteCore()))
                .Verify();

        [TestMethod]
        public void ExecutingSqlQueries_VB_EntityFrameworkCore2() =>
            builderVB
                .AddPaths(@"ExecutingSqlQueries.EntityFrameworkCore2.vb")
                .WithOptions(ParseOptionsHelper.FromVisualBasic15)
                .AddReferences(GetReferencesEntityFrameworkNetCore(Constants.DotNetCore220Version))
                .Verify();

        [TestMethod]
        public void ExecutingSqlQueries_VB_EntityFrameworkCoreLatest() =>
            builderVB
                .AddPaths(@"ExecutingSqlQueries.EntityFrameworkCoreLatest.vb")
                .WithOptions(ParseOptionsHelper.FromVisualBasic15)
                .AddReferences(GetReferencesEntityFrameworkNetCore(Constants.NuGetLatestVersion))
                .Verify();

        internal static IEnumerable<MetadataReference> GetReferencesEntityFrameworkNetCore(string entityFrameworkVersion) =>
            Enumerable.Empty<MetadataReference>()
                      .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCore(entityFrameworkVersion))
                      .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCoreRelational(entityFrameworkVersion));

#endif

        [TestMethod]
        public void ExecutingSqlQueries_CS_Dapper() =>
            builderCS
                .AddPaths("ExecutingSqlQueries.Dapper.cs")
                .AddReferences(MetadataReferenceFacade.SystemData)
                .AddReferences(NuGetMetadataReference.Dapper())
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
                .AddReferences(NuGetMetadataReference.ServiceStackOrmLite(Constants.NuGetLatestVersion))
                .Verify();

        [TestMethod]
        public void ExecutingSqlQueries_NHibernate_CS() =>
            builderCS
                .AddPaths("ExecutingSqlQueries.NHibernate.cs")
                .AddReferences(NuGetMetadataReference.NHibernate(Constants.NuGetLatestVersion))
                .Verify();
    }
}

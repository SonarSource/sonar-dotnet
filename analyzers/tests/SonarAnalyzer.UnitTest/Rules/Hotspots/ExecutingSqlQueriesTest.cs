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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class ExecutingSqlQueriesTest
    {
        private readonly VerifierBuilder builderCs = new VerifierBuilder().AddAnalyzer(() => new CS.ExecutingSqlQueries(AnalyzerConfiguration.AlwaysEnabled));
        private readonly VerifierBuilder builderVb = new VerifierBuilder().AddAnalyzer(() => new VB.ExecutingSqlQueries(AnalyzerConfiguration.AlwaysEnabled));

#if NETFRAMEWORK // System.Data.OracleClient.dll is not available on .Net Core

        [TestMethod]
        public void ExecutingSqlQueries_CS_Net46() =>
            builderCs.AddPaths(@"Hotspots\ExecutingSqlQueries_Net46.cs")
                .AddReferences(GetReferencesNet46(Constants.NuGetLatestVersion))
                .Verify();

        [TestMethod]
        public void ExecutingSqlQueries_VB_Net46() =>
            builderVb.AddPaths(@"Hotspots\ExecutingSqlQueries_Net46.vb")
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
        public void ExecutingSqlQueries_CS_NetCore() =>
            builderCs.AddPaths(@"Hotspots\ExecutingSqlQueries_NetCore.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .AddReferences(GetReferencesNetCore(Constants.DotNetCore220Version))
                .Verify();

        [TestMethod]
        public void ExecutingSqlQueries_CSharp9() =>
            builderCs.AddPaths(@"Hotspots\ExecutingSqlQueries.CSharp9.cs")
                .WithTopLevelStatements()
                .AddReferences(GetReferencesNetCore(Constants.DotNetCore220Version).Concat(NuGetMetadataReference.MicrosoftDataSqliteCore()))
                .Verify();

        [TestMethod]
        public void ExecutingSqlQueries_CSharp10() =>
            builderCs.AddPaths(@"Hotspots\ExecutingSqlQueries.CSharp10.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .WithTopLevelStatements()
                .AddReferences(GetReferencesNetCore(Constants.DotNetCore220Version).Concat(NuGetMetadataReference.MicrosoftDataSqliteCore()))
                .Verify();

        [TestMethod]
        public void ExecutingSqlQueries_VB_NetCore() =>
            builderVb.AddPaths(@"Hotspots\ExecutingSqlQueries_NetCore.vb")
                .WithOptions(ParseOptionsHelper.FromVisualBasic15)
                .AddReferences(GetReferencesNetCore(Constants.DotNetCore220Version))
                .Verify();

        internal static IEnumerable<MetadataReference> GetReferencesNetCore(string entityFrameworkVersion) =>
            Enumerable.Empty<MetadataReference>()
                      .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCore(entityFrameworkVersion))
                      .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCoreRelational(entityFrameworkVersion));
#endif
    }
}

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

extern alias csharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using csharp::SonarAnalyzer.Rules.CSharp;
using System.Linq;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class SqlKeywordsDelimitedBySpaceTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void SqlKeywordsDelimitedBySpace()
        {
            Verifier.VerifyAnalyzer(@"TestCases\SqlKeywordsDelimitedBySpace.cs",
                new SqlKeywordsDelimitedBySpace(),
                additionalReferences: FrameworkMetadataReference.SystemData,
                options: ParseOptionsHelper.FromCSharp8);
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void SqlKeywordsDelimitedBySpace_UsingInsideNamespace()
        {
            Verifier.VerifyAnalyzer(@"TestCases\SqlKeywordsDelimitedBySpace_InsideNamespace.cs",
                new SqlKeywordsDelimitedBySpace(),
                additionalReferences: FrameworkMetadataReference.SystemData);
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void SqlKeywordsDelimitedBySpace_DefaultNamespace()
        {
            Verifier.VerifyNoIssueReportedInTest(@"TestCases\SqlKeywordsDelimitedBySpace_DefaultNamespace.cs",
                new SqlKeywordsDelimitedBySpace(),
                additionalReferences: FrameworkMetadataReference.SystemData);
        }

        [DataRow("System.Data")]
        [DataRow("System.Data.SqlClient")]
        [DataRow("System.Data.SQLite")]
        [DataRow("System.Data.SqlServerCe")]
        [DataRow("System.Data.Entity")]
        [DataRow("System.Data.Odbc")]
        [DataRow("Dapper")]
        [DataRow("Microsoft.Data.Sqlite")]
        [DataRow("NHibernate")]
        [DataRow("PetaPoco")]
        [DataTestMethod]
        [TestCategory("Rule")]
        public void SqlKeywordsDelimitedBySpace_DotnetFramework(string sqlNamespace)
        {
            var references = FrameworkMetadataReference.SystemData
                .Concat(NuGetMetadataReference.Dapper())
                .Concat(NuGetMetadataReference.EntityFramework())
                .Concat(NuGetMetadataReference.MicrosoftDataSqliteCore())
                .Concat(NuGetMetadataReference.MicrosoftSqlServerCompact())
                .Concat(NuGetMetadataReference.NHibernate())
                .Concat(NuGetMetadataReference.PetaPocoCompiled())
                .Concat(NuGetMetadataReference.SystemDataOdbc())
                .Concat(NuGetMetadataReference.SystemDataSqlClient())
                .Concat(NuGetMetadataReference.SystemDataSQLiteCore());

            Verifier.VerifyCSharpAnalyzer($@"
using {sqlNamespace};
namespace TestNamespace
{{
    public class Test
    {{
        private string field = ""SELECT * FROM table"" +
            ""WHERE col ="" + // Noncompliant
            ""val"";
    }}
}}
",
                new SqlKeywordsDelimitedBySpace(),
                additionalReferences: references.ToArray());
        }

        [DataRow("System.Data.SqlClient")]
        [DataRow("System.Data.OracleClient")]
        [DataRow("Microsoft.EntityFrameworkCore")]
        [DataRow("ServiceStack.OrmLite")]
        [DataTestMethod]
        [TestCategory("Rule")]
        public void SqlKeywordsDelimitedBySpace_DotnetCore(string sqlNamespace)
        {
            var references = FrameworkMetadataReference.SystemData
                .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCore("2.2.0"))
                .Concat(NuGetMetadataReference.ServiceStackOrmLite())
                .Concat(NuGetMetadataReference.SystemDataSqlClient())
                .Concat(NuGetMetadataReference.SystemDataOracleClient());

            Verifier.VerifyCSharpAnalyzer($@"
using {sqlNamespace};
namespace TestNamespace
{{
    public class Test
    {{
        private string field = ""SELECT * FROM table"" +
            ""WHERE col ="" + // Noncompliant
            ""val"";
    }}
}}
",
                new SqlKeywordsDelimitedBySpace(),
                additionalReferences: references.ToArray());
        }
    }
}

/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class DatabasePasswordsShouldBeSecureTest
    {
        [DataTestMethod]
        [DataRow("3.1.11", "3.19.80")]
        [DataRow("5.0.2", "5.21.1")]
        [TestCategory("Rule")]
        public void DatabasePasswordsShouldBeSecure_CS(string dotnetVersion, string oracleVersion) =>
            Verifier.VerifyAnalyzer(@"TestCases\DatabasePasswordsShouldBeSecure.cs", new CS.DatabasePasswordsShouldBeSecure(), ParseOptionsHelper.FromCSharp8,
                Enumerable.Empty<MetadataReference>()
                .Concat(MetadataReferenceFacade.SystemData)
                .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCore(dotnetVersion))
                .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCoreSqliteCore(dotnetVersion))
                .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCoreSqlServer(dotnetVersion))
                .Concat(NuGetMetadataReference.OracleEntityFrameworkCore(oracleVersion))
                .Concat(NuGetMetadataReference.MySqlDataEntityFrameworkCore())
                .Concat(NuGetMetadataReference.NpgsqlEntityFrameworkCorePostgreSQL(dotnetVersion)));
    }
}

#endif

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

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.Rules.SymbolicExecution;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class PublicMethodArgumentsShouldBeCheckedForNullTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder().AddAnalyzer(() => new SymbolicExecutionRunner())
                                                       .WithOnlyDiagnostics(PublicMethodArgumentsShouldBeCheckedForNull.S3900);

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void PublicMethodArgumentsShouldBeCheckedForNull_CS(ProjectType projectType) =>
            builder.AddReferences(TestHelper.ProjectTypeReference(projectType).Concat(MetadataReferenceFacade.NETStandard21))
                .AddPaths(@"SymbolicExecution\Sonar\PublicMethodArgumentsShouldBeCheckedForNull.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

#if NET
        [TestMethod]
        public void PublicMethodArgumentsShouldBeCheckedForNull_CSharp9() =>
            builder.AddPaths(@"SymbolicExecution\Sonar\PublicMethodArgumentsShouldBeCheckedForNull.CSharp9.cs")
                .AddReferences(NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(Constants.NuGetLatestVersion))
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .Verify();
#endif
    }
}

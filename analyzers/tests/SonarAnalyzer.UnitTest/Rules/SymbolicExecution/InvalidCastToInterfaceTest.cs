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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.Rules.SymbolicExecution;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules.SymbolicExecution
{
    [TestClass]
    public class InvalidCastToInterfaceTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<SymbolicExecutionRunner>()
            .AddAnalyzer(() => new InvalidCastToInterface())
            .WithBasePath(@"SymbolicExecution\Sonar")
            .WithOnlyDiagnostics(InvalidCastToInterfaceSymbolicExecution.S1944);

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void InvalidCastToInterface(ProjectType projectType) =>
            builder.AddPaths("InvalidCastToInterface.cs")
                .AddReferences(TestHelper.ProjectTypeReference(projectType))
                .AddReferences(MetadataReferenceFacade.NETStandard21)
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

#if NET

        [TestMethod]
        public void InvalidCastToInterface_CSharp9() =>
            builder.AddPaths("InvalidCastToInterface.CSharp9.cs").WithTopLevelStatements().Verify();

        [TestMethod]
        public void InvalidCastToInterface_CSharp10() =>
            builder.AddPaths("InvalidCastToInterface.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

#endif

    }
}

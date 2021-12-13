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

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.Rules.SymbolicExecution;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules.SymbolicExecution
{
    [TestClass]
    public class EmptyCollectionsShouldNotBeEnumeratedTest
    {
        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void EmptyCollectionsShouldNotBeEnumerated(ProjectType projectType) =>
            Verifier.VerifyAnalyzer(
                @"TestCases\SymbolicExecution\Sonar\EmptyCollectionsShouldNotBeEnumerated.cs",
                new SymbolicExecutionRunner(new EmptyCollectionsShouldNotBeEnumerated()),
                ParseOptionsHelper.FromCSharp8,
                TestHelper.ProjectTypeReference(projectType).Concat(MetadataReferenceFacade.NETStandard21));

#if NET
        [TestMethod]
        public void EmptyCollectionsShouldNotBeEnumerated_CSharp9() =>
            Verifier.VerifyAnalyzerFromCSharp9Console(
                @"TestCases\SymbolicExecution\Sonar\EmptyCollectionsShouldNotBeEnumerated.CSharp9.cs",
                new SymbolicExecutionRunner(new EmptyCollectionsShouldNotBeEnumerated()));

        [TestMethod]
        public void EmptyCollectionsShouldNotBeEnumerated_CSharp10() =>
            Verifier.VerifyAnalyzerFromCSharp10Library(
                @"TestCases\SymbolicExecution\Sonar\EmptyCollectionsShouldNotBeEnumerated.CSharp10.cs",
                new SymbolicExecutionRunner(new EmptyCollectionsShouldNotBeEnumerated()));
#endif
    }
}

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

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Protobuf;
using SonarAnalyzer.Rules;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class SymbolReferenceAnalyzerTest
    {
        private const string Root = @"TestCases\Utilities\SymbolReferenceAnalyzer\";

        public TestContext TestContext { get; set; } // Set automatically by MsTest

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void Verify_Method_PreciseLocation_CS(ProjectType projectType) =>
            Verify("Method.cs", projectType, references =>
            {
                references.Select(x => x.Declaration.StartLine).Should().BeEquivalentTo(1, 3, 5);   // class 'Sample' on line 1, method 'Method' on line 3, method 'Go' on line 5
                var methodDeclaration = references.Single(x => x.Declaration.StartLine == 3);
                methodDeclaration.Declaration.Should().BeEquivalentTo(new TextRange { StartLine = 3, EndLine = 3, StartOffset = 16, EndOffset = 22 });
                methodDeclaration.Reference.Should().HaveCount(1);
                methodDeclaration.Reference.Single().Should().BeEquivalentTo(new TextRange { StartLine = 6, EndLine = 6, StartOffset = 8, EndOffset = 14 });
            });

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void Verify_Method_PreciseLocation_VB(ProjectType projectType) =>
            Verify("Method.vb", projectType, references =>
            {
                references.Select(x => x.Declaration.StartLine).Should().BeEquivalentTo(1, 3, 6);   // class 'Sample' on line 1, method 'Method' on line 3, method 'Go' on line 6
                var methodDeclaration = references.Single(x => x.Declaration.StartLine == 3);
                methodDeclaration.Declaration.Should().BeEquivalentTo(new TextRange { StartLine = 3, EndLine = 3, StartOffset = 15, EndOffset = 21 });
                methodDeclaration.Reference.Should().HaveCount(1);
                methodDeclaration.Reference.Single().Should().BeEquivalentTo(new TextRange { StartLine = 7, EndLine = 7, StartOffset = 8, EndOffset = 14 });
            });

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void Verify_Event_CS(ProjectType projectType) =>
            Verify("Event.cs", projectType, 6, 5, 9, 10);

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void Verify_Field_CS(ProjectType projectType) =>
            Verify("Field.cs", projectType, 4, 3, 7, 8);

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void Verify_LocalFunction_CS(ProjectType projectType) =>
            Verify("LocalFunction.cs", projectType, 4, 7, 5);

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void Verify_Method_CS(ProjectType projectType) =>
            Verify("Method.cs", projectType, 3, 3, 6);

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void Verify_NamedType_CS(ProjectType projectType) =>
            Verify("NamedType.cs", projectType, 4, 3, 7, 7); // 'var' and type name on the same line

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void Verify_Parameter_CS(ProjectType projectType) =>
            Verify("Parameter.cs", projectType, 4, 4, 6, 7);

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void Verify_Property_CS(ProjectType projectType) =>
            Verify("Property.cs", projectType, 4, 3, 7, 8);

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void Verify_Setter_CS(ProjectType projectType) =>
            Verify("Setter.cs", projectType, 4, 6, 8);

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void Verify_TypeParameter_CS(ProjectType projectType) =>
            Verify("TypeParameter.cs", projectType, 5, 2, 4, 6);

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void GetSetKeyword_ReturnsNull_VB(bool isTestProject) =>
            // This path is unreachable for VB code
            new TestSymbolReferenceAnalyzer_VB(null, isTestProject).TestGetSetKeyword(null).Should().BeNull();

        [TestMethod]
        public void Verify_TokenThreshold() =>
            // In TokenThreshold.cs there are 40009 tokens which is more than the current limit of 40000
            Verify("TokenThreshold.cs", ProjectType.Product, _ => { }, false);

        private void Verify(string fileName, ProjectType projectType, int expectedDeclarationCount, int assertedDeclarationLine, params int[] assertedDeclarationLineReferences) =>
            Verify(fileName, projectType, references =>
                {
                    references.Where(x => x.Declaration != null).Should().HaveCount(expectedDeclarationCount);
                    var declarationReferences = references.Single(x => x.Declaration.StartLine == assertedDeclarationLine).Reference;
                    declarationReferences.Select(x => x.StartLine).Should().BeEquivalentTo(assertedDeclarationLineReferences);
                });

        private void Verify(string fileName,
                            ProjectType projectType,
                            Action<IReadOnlyList<SymbolReferenceInfo.Types.SymbolReference>> verifyReference,
                            bool isMessageExpected = true)
        {
            var testRoot = Root + TestContext.TestName;
            UtilityAnalyzerBase analyzer = fileName.EndsWith(".cs")
                ? new TestSymbolReferenceAnalyzer_CS(testRoot, projectType == ProjectType.Test)
                : new TestSymbolReferenceAnalyzer_VB(testRoot, projectType == ProjectType.Test);

            Verifier.VerifyNonConcurrentUtilityAnalyzer<SymbolReferenceInfo>(
                new[] { Root + fileName },
                analyzer,
                @$"{testRoot}\symrefs.pb",
                TestHelper.CreateSonarProjectConfig(testRoot, projectType),
                messages =>
                {
                    messages.Should().HaveCount(isMessageExpected ? 1 : 0);

                    if (isMessageExpected)
                    {
                        var info = messages.Single();
                        info.FilePath.Should().Be(fileName);
                        verifyReference(info.Reference);
                    }
                });
        }

        // We need to set protected properties and this class exists just to enable the analyzer without bothering with additional files with parameters
        private class TestSymbolReferenceAnalyzer_CS : CS.SymbolReferenceAnalyzer
        {
            public TestSymbolReferenceAnalyzer_CS(string outPath, bool isTestProject)
            {
                IsAnalyzerEnabled = true;
                OutPath = outPath;
                IsTestProject = isTestProject;
            }
        }

        private class TestSymbolReferenceAnalyzer_VB : VB.SymbolReferenceAnalyzer
        {
            public TestSymbolReferenceAnalyzer_VB(string outPath, bool isTestProject)
            {
                IsAnalyzerEnabled = true;
                OutPath = outPath;
                IsTestProject = isTestProject;
            }

            public object TestGetSetKeyword(ISymbol valuePropertySymbol) =>
                GetSetKeyword(valuePropertySymbol);
        }
    }
}

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
        [DataRow(true)]
        [DataRow(false)]
        [TestCategory("Rule")]
        public void Verify_Method_PreciseLocation_CS(bool isTestProject) =>
            Verify("Method.cs", isTestProject, references =>
            {
                references.Select(x => x.Declaration.StartLine).Should().BeEquivalentTo(1, 3, 5);   // class 'Sample' on line 1, method 'Method' on line 3, method 'Go' on line 5
                var methodDeclaration = references.Single(x => x.Declaration.StartLine == 3);
                methodDeclaration.Declaration.Should().BeEquivalentTo(new TextRange { StartLine = 3, EndLine = 3, StartOffset = 16, EndOffset = 22 });
                methodDeclaration.Reference.Should().HaveCount(1);
                methodDeclaration.Reference.Single().Should().BeEquivalentTo(new TextRange { StartLine = 6, EndLine = 6, StartOffset = 8, EndOffset = 14 });
            });

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        [TestCategory("Rule")]
        public void Verify_Method_PreciseLocation_VB(bool isTestProject) =>
            Verify("Method.vb", isTestProject, references =>
            {
                references.Select(x => x.Declaration.StartLine).Should().BeEquivalentTo(1, 3, 6);   // class 'Sample' on line 1, method 'Method' on line 3, method 'Go' on line 6
                var methodDeclaration = references.Single(x => x.Declaration.StartLine == 3);
                methodDeclaration.Declaration.Should().BeEquivalentTo(new TextRange { StartLine = 3, EndLine = 3, StartOffset = 15, EndOffset = 21 });
                methodDeclaration.Reference.Should().HaveCount(1);
                methodDeclaration.Reference.Single().Should().BeEquivalentTo(new TextRange { StartLine = 7, EndLine = 7, StartOffset = 8, EndOffset = 14 });
            });

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        [TestCategory("Rule")]
        public void Verify_Event_CS(bool isTestProject) =>
            Verify("Event.cs", isTestProject, 6, 5, 9, 10);

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        [TestCategory("Rule")]
        public void Verify_Field_CS(bool isTestProject) =>
            Verify("Field.cs", isTestProject, 4, 3, 7, 8);

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        [TestCategory("Rule")]
        public void Verify_LocalFunction_CS(bool isTestProject) =>
            Verify("LocalFunction.cs", isTestProject, 4, 7, 5);

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        [TestCategory("Rule")]
        public void Verify_Method_CS(bool isTestProject) =>
            Verify("Method.cs", isTestProject, 3, 3, 6);

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        [TestCategory("Rule")]
        public void Verify_NamedType_CS(bool isTestProject) =>
            Verify("NamedType.cs", isTestProject, 4, 3, 7, 7); // 'var' and type name on the same line

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        [TestCategory("Rule")]
        public void Verify_Parameter_CS(bool isTestProject) =>
            Verify("Parameter.cs", isTestProject, 4, 4, 6, 7);

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        [TestCategory("Rule")]
        public void Verify_Property_CS(bool isTestProject) =>
            Verify("Property.cs", isTestProject, 4, 3, 7, 8);

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        [TestCategory("Rule")]
        public void Verify_Setter_CS(bool isTestProject) =>
            Verify("Setter.cs", isTestProject, 4, 6, 8);

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        [TestCategory("Rule")]
        public void Verify_TypeParameter_CS(bool isTestProject) =>
            Verify("TypeParameter.cs", isTestProject, 5, 2, 4, 6);

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        [TestCategory("Rule")]
        public void GetSetKeyword_ReturnsNull_VB(bool isTestProject) =>
            // This path is unreachable for VB code
            new TestSymbolReferenceAnalyzer_VB(null, isTestProject).TestGetSetKeyword(null).Should().BeNull();

        private void Verify(string fileName, bool isTestProject, int expectedDeclarationCount, int assertedDeclarationLine, params int[] assertedDeclarationLineReferences) =>
            Verify(fileName, isTestProject, references =>
                {
                    references.Where(x => x.Declaration != null).Should().HaveCount(expectedDeclarationCount);
                    var declarationReferences = references.Single(x => x.Declaration.StartLine == assertedDeclarationLine).Reference;
                    declarationReferences.Select(x => x.StartLine).Should().BeEquivalentTo(assertedDeclarationLineReferences);
                });

        private void Verify(string fileName, bool isTestProject, Action<IReadOnlyList<SymbolReferenceInfo.Types.SymbolReference>> verifyReference)
        {
            var testRoot = Root + TestContext.TestName;
            UtilityAnalyzerBase analyzer = fileName.EndsWith(".cs")
                ? new TestSymbolReferenceAnalyzer_CS(testRoot, isTestProject)
                : new TestSymbolReferenceAnalyzer_VB(testRoot, isTestProject);

            Verifier.VerifyUtilityAnalyzer<SymbolReferenceInfo>(
                new[] { Root + fileName },
                analyzer,
                @$"{testRoot}\symrefs.pb",
                messages =>
                {
                    messages.Should().HaveCount(1);
                    var info = messages.Single();
                    info.FilePath.Should().Be(fileName);
                    verifyReference(info.Reference);
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

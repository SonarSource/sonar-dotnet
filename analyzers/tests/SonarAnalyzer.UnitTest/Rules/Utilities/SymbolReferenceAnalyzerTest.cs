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

        [TestMethod]
        [TestCategory("Rule")]
        public void Verify_Method_PreciseLocation_CS() =>
            Verify("Method.cs", references =>
            {
                references.Select(x => x.Declaration.StartLine).Should().BeEquivalentTo(1, 3, 5);   // class 'Sample' on line 1, method 'Method' on line 3, method 'Go' on line 5
                var methodDeclaration = references.Single(x => x.Declaration.StartLine == 3);
                methodDeclaration.Declaration.Should().BeEquivalentTo(new TextRange { StartLine = 3, EndLine = 3, StartOffset = 16, EndOffset = 22 });
                methodDeclaration.Reference.Should().HaveCount(1);
                methodDeclaration.Reference.Single().Should().BeEquivalentTo(new TextRange { StartLine = 6, EndLine = 6, StartOffset = 8, EndOffset = 14 });
            });

        [TestMethod]
        [TestCategory("Rule")]
        public void Verify_Method_PreciseLocation_VB() =>
            Verify("Method.vb", references =>
            {
                references.Select(x => x.Declaration.StartLine).Should().BeEquivalentTo(1, 3, 6);   // class 'Sample' on line 1, method 'Method' on line 3, method 'Go' on line 6
                var methodDeclaration = references.Single(x => x.Declaration.StartLine == 3);
                methodDeclaration.Declaration.Should().BeEquivalentTo(new TextRange { StartLine = 3, EndLine = 3, StartOffset = 15, EndOffset = 21 });
                methodDeclaration.Reference.Should().HaveCount(1);
                methodDeclaration.Reference.Single().Should().BeEquivalentTo(new TextRange { StartLine = 7, EndLine = 7, StartOffset = 8, EndOffset = 14 });
            });

        [TestMethod]
        [TestCategory("Rule")]
        public void Verify_Event_CS() =>
            Verify("Event.cs", 6, 5, 9, 10);

        [TestMethod]
        [TestCategory("Rule")]
        public void Verify_Field_CS() =>
            Verify("Field.cs", 4, 3, 7, 8);

        [TestMethod]
        [TestCategory("Rule")]
        public void Verify_LocalFunction_CS() =>
            Verify("LocalFunction.cs", 4, 7, 5);

        [TestMethod]
        [TestCategory("Rule")]
        public void Verify_Method_CS() =>
            Verify("Method.cs", 3, 3, 6);

        [TestMethod]
        [TestCategory("Rule")]
        public void Verify_NamedType_CS() =>
            Verify("NamedType.cs", 4, 3, 7, 7); // 'var' and type name on the same line

        [TestMethod]
        [TestCategory("Rule")]
        public void Verify_Parameter_CS() =>
            Verify("Parameter.cs", 4, 4, 6, 7);

        [TestMethod]
        [TestCategory("Rule")]
        public void Verify_Property_CS() =>
            Verify("Property.cs", 4, 3, 7, 8);

        [TestMethod]
        [TestCategory("Rule")]
        public void Verify_Setter_CS() =>
            Verify("Setter.cs", 4, 6, 8);

        [TestMethod]
        [TestCategory("Rule")]
        public void Verify_TypeParameter_CS() =>
            Verify("TypeParameter.cs", 5, 2, 4, 6);

        [TestMethod]
        [TestCategory("Rule")]
        public void GetSetKeyword_ReturnsNull_VB() =>
            // This path is unreachable for VB code
            new TestSymbolReferenceAnalyzer_VB(null, false).TestGetSetKeyword(null).Should().BeNull();

        public void Verify(string fileName, int expectedDeclarationCount, int assertedDeclarationLine, params int[] assertedDeclarationLineReferences) =>
            Verify(fileName, references =>
                {
                    references.Where(x => x.Declaration != null).Should().HaveCount(expectedDeclarationCount);
                    var declarationReferences = references.Single(x => x.Declaration.StartLine == assertedDeclarationLine).Reference;
                    declarationReferences.Select(x => x.StartLine).Should().BeEquivalentTo(assertedDeclarationLineReferences);
                });

        public void Verify(string fileName, Action<IReadOnlyList<SymbolReferenceInfo.Types.SymbolReference>> verifyReference)
        {
            var testRoot = Root + TestContext.TestName;
            UtilityAnalyzerBase mainAnalyzer = fileName.EndsWith(".cs")
                ? new TestSymbolReferenceAnalyzer_CS(testRoot, true)
                : new TestSymbolReferenceAnalyzer_VB(testRoot, true);

            UtilityAnalyzerBase testAnalyzer = fileName.EndsWith(".cs")
                ? new TestSymbolReferenceAnalyzer_CS(testRoot, true)
                : new TestSymbolReferenceAnalyzer_VB(testRoot, true);

            Verifier.VerifyUtilityAnalyzer<SymbolReferenceInfo>(
                new[] { Root + fileName },
                mainAnalyzer,
                @$"{testRoot}\symrefs.pb",
                VerifyProtoBuf);

            Verifier.VerifyUtilityAnalyzer<SymbolReferenceInfo>(
                new[] { Root + fileName },
                testAnalyzer,
                @$"{testRoot}\symrefs.pb",
                VerifyProtoBuf);

            void VerifyProtoBuf(IReadOnlyList<SymbolReferenceInfo> messages)
            {
                messages.Should().HaveCount(1);
                var info = messages.Single();
                info.FilePath.Should().Be(fileName);
                verifyReference(info.Reference);
            }
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

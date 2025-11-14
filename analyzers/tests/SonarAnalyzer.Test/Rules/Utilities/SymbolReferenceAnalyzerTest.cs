/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.IO;
using SonarAnalyzer.Core.AnalysisContext;
using SonarAnalyzer.Core.Rules;
using SonarAnalyzer.Protobuf;
using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class SymbolReferenceAnalyzerTest
{
    private const string BasePath = @"Utilities\SymbolReferenceAnalyzer\";

    public TestContext TestContext { get; set; }

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_Method_PreciseLocation_CS(ProjectType projectType) =>
        Verify("Method.cs", projectType, x =>
        {
            x.Select(x => x.Declaration.StartLine).Should().BeEquivalentTo([1, 3, 5, 7]);   // class 'Sample' on line 1, method 'Method' on line 3, method 'method' on line 5 and method 'Go' on line 7
            var methodDeclaration = x.Single(x => x.Declaration.StartLine == 3);
            methodDeclaration.Declaration.Should().BeEquivalentTo(new TextRange { StartLine = 3, EndLine = 3, StartOffset = 16, EndOffset = 22 });
            methodDeclaration.Reference.Should().Equal(new TextRange { StartLine = 9, EndLine = 9, StartOffset = 8, EndOffset = 14 });
        });

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_Method_PreciseLocation_VB(ProjectType projectType) =>
        Verify("Method.vb", projectType, x =>
        {
            x.Select(x => x.Declaration.StartLine).Should().BeEquivalentTo([1, 3, 6, 10]);

            var procedureDeclaration = x.Single(x => x.Declaration.StartLine == 3);
            procedureDeclaration.Declaration.Should().BeEquivalentTo(new TextRange { StartLine = 3, EndLine = 3, StartOffset = 15, EndOffset = 21 });
            procedureDeclaration.Reference.Should().BeEquivalentTo(
                [
                    new TextRange { StartLine = 11, EndLine = 11, StartOffset = 8, EndOffset = 14 },
                    new TextRange { StartLine = 13, EndLine = 13, StartOffset = 8, EndOffset = 14 }
                ]);

            var functionDeclaration = x.Single(x => x.Declaration.StartLine == 6);
            functionDeclaration.Declaration.Should().BeEquivalentTo(new TextRange { StartLine = 6, EndLine = 6, StartOffset = 13, EndOffset = 23 });
            functionDeclaration.Reference.Should().Equal(new TextRange { StartLine = 12, EndLine = 12, StartOffset = 8, EndOffset = 18 });
        });

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_Event_CS(ProjectType projectType) =>
        Verify("Event.cs", projectType, 6, 5, 9, 10);

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_Event_VB(ProjectType projectType) =>
        Verify("Event.vb", projectType, 4, 3, 6, 8, 11);

    [TestMethod]
    [DataRow("Field.cs", ProjectType.Product)]
    [DataRow("Field.cs", ProjectType.Test)]
    [DataRow("Field.ReservedKeyword.cs", ProjectType.Product)]
    [DataRow("Field.ReservedKeyword.cs", ProjectType.Test)]
    [DataRow("Field.EscapedNonKeyword.cs", ProjectType.Product)]
    [DataRow("Field.EscapedNonKeyword.cs", ProjectType.Test)]
    public void Verify_Field_CS(string fileName, ProjectType projectType) =>
        Verify(fileName, projectType, 4, 3, 7, 8);

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_Field_EscapedSequences_CS(ProjectType projectType) =>
        Verify("Field.EscapedSequences.cs", projectType, 3, 3, 7, 8, 9, 10, 11, 12);

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_MissingDeclaration_CS(ProjectType projectType) =>
        Verify("MissingDeclaration.cs", projectType, 1, 3);

    [TestMethod]
    [DataRow("Field.vb", ProjectType.Product)]
    [DataRow("Field.vb", ProjectType.Test)]
    [DataRow("Field.ReservedKeyword.vb", ProjectType.Product)]
    [DataRow("Field.ReservedKeyword.vb", ProjectType.Test)]
    [DataRow("Field.EscapedNonKeyword.vb", ProjectType.Product)]
    [DataRow("Field.EscapedNonKeyword.vb", ProjectType.Test)]
    public void Verify_Field_VB(string fileName, ProjectType projectType) =>
        Verify(fileName, projectType, 4, 3, 6, 7);

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_Tuples_CS(ProjectType projectType) =>
        Verify("Tuples.cs", projectType, 4, 7, 8);

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_Tuples_VB(ProjectType projectType) =>
        Verify("Tuples.vb", projectType, 4, 4, 8);

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_LocalFunction_CS(ProjectType projectType) =>
        Verify("LocalFunction.cs", projectType, 4, 7, 5);

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_Method_CS(ProjectType projectType) =>
        Verify("Method.cs", projectType, 4, 3, 9);

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_Method_Partial_CS(ProjectType projectType)
    {
        var builder = CreateBuilder(projectType, "Method_Partial1.cs", "Method_Partial2.cs");
        builder.VerifyUtilityAnalyzer<SymbolReferenceInfo>(x => x.OrderBy(x => x.FilePath).Should().SatisfyRespectively(
            p1 =>
            {
                p1.FilePath.Should().Be(@"Utilities\SymbolReferenceAnalyzer\Method_Partial1.cs");
                p1.Reference.Should().HaveCount(7, because: "a class, 5 partial methods, and a referencing method are declared");

                var unimplementedReference = p1.Reference[1];
                unimplementedReference.Declaration.StartLine.Should().Be(5, because: "the Unimplemented() method is declared here");
                unimplementedReference.Reference.Should().ContainSingle(because: "the method is referenced inside Reference1() once").Subject.StartLine.Should().Be(14, because: "The reference to Unimplemented() happens here");

                var implemented1Reference = p1.Reference[2];
                implemented1Reference.Declaration.StartLine.Should().Be(6, because: "the Implemented1() method is declared here");
                implemented1Reference.Reference.Should().ContainSingle(because: "the method is referenced inside Reference1() once").Subject.StartLine.Should().Be(15, because: "The reference to Implemented1() happens here");

                var implemented2Reference = p1.Reference[3];
                implemented2Reference.Declaration.StartLine.Should().Be(7, because: "the Implemented2() method is declared here");
                implemented2Reference.Reference.Should().ContainSingle(because: "the method is referenced inside Reference1() once").Subject.StartLine.Should().Be(16, because: "The reference to Implemented2() happens here");

                var declaredAndImplementedReference = p1.Reference[4];
                declaredAndImplementedReference.Declaration.StartLine.Should().Be(9, because: "the DeclaredAndImplemented() method is declared here");
                declaredAndImplementedReference.Reference.Should().ContainSingle(because: "the method is referenced inside Reference1() once").Subject.StartLine.Should().Be(18, because: "The reference to DeclaredAndImplemented() happens here");

                var declaredAndImplementedReference2 = p1.Reference[5];
                declaredAndImplementedReference2.Declaration.StartLine.Should().Be(10, because: "the DeclaredAndImplemented() method is implemented here");
                declaredAndImplementedReference2.Reference.Should().ContainSingle(because: "the method is referenced inside Reference1() once").Subject.StartLine.Should().Be(18, because: "The reference to DeclaredAndImplemented() happens here");
            },
            p2 =>
            {
                p2.FilePath.Should().Be(@"Utilities\SymbolReferenceAnalyzer\Method_Partial2.cs");
                p2.Reference.Should().HaveCount(4, because: "a class, 2 partial methods, and a referencing method are declared");

                var implemented1Reference = p2.Reference[1];
                implemented1Reference.Declaration.StartLine.Should().Be(5, because: "the Implemented1() method is declared here");
                implemented1Reference.Reference.Should().ContainSingle(because: "the method is referenced inside Reference2() once").Subject.StartLine.Should().Be(11, because: "The reference to Implemented1() happens here");

                var implemented2Reference = p2.Reference[2];
                implemented2Reference.Declaration.StartLine.Should().Be(6, because: "the Implemented2() method is declared here");
                implemented2Reference.Reference.Should().ContainSingle(because: "the method is referenced inside Reference2() once").Subject.StartLine.Should().Be(12, because: "The reference to Implemented2() happens here");
            }));
    }

    [TestMethod]
    [DataRow("NamedType.cs", ProjectType.Product)]
    [DataRow("NamedType.cs", ProjectType.Test)]
    [DataRow("NamedType.ReservedKeyword.cs", ProjectType.Product)]
    [DataRow("NamedType.ReservedKeyword.cs", ProjectType.Test)]
    public void Verify_NamedType_CS(string fileName, ProjectType projectType) =>
        Verify(fileName, projectType, 4, 3, 7);

    [TestMethod]
    [DataRow("NamedType.vb", ProjectType.Product)]
    [DataRow("NamedType.vb", ProjectType.Test)]
    [DataRow("NamedType.ReservedKeyword.vb", ProjectType.Product)]
    [DataRow("NamedType.ReservedKeyword.vb", ProjectType.Test)]
    public void Verify_NamedType_VB(string fileName, ProjectType projectType) =>
        Verify(fileName, projectType, 5, 1, 4, 4, 5);

    [TestMethod]
    [DataRow("Parameter.cs", ProjectType.Product)]
    [DataRow("Parameter.cs", ProjectType.Test)]
    [DataRow("Parameter.ReservedKeyword.cs", ProjectType.Product)]
    [DataRow("Parameter.ReservedKeyword.cs", ProjectType.Test)]
    public void Verify_Parameter_CS(string fileName, ProjectType projectType) =>
        Verify(fileName, projectType, 4, 4, 6, 7);

    [TestMethod]
    [DataRow("Parameter.vb", ProjectType.Product)]
    [DataRow("Parameter.vb", ProjectType.Test)]
    [DataRow("Parameter.ReservedKeyword.vb", ProjectType.Product)]
    [DataRow("Parameter.ReservedKeyword.vb", ProjectType.Test)]
    public void Verify_Parameter_VB(string fileName, ProjectType projectType) =>
        Verify(fileName, projectType, 4, 4, 5, 6);

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_Property_CS(ProjectType projectType) =>
        Verify("Property.cs", projectType, 5, 3, 9, 10);

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_Property_Partial_CS(ProjectType projectType)
    {
        var builder = CreateBuilder(projectType, "Property_Partial1.cs", "Property_Partial2.cs");
        builder.VerifyUtilityAnalyzer<SymbolReferenceInfo>(x => x.OrderBy(x => x.FilePath).Should().SatisfyRespectively(
            p1 =>
            {
                p1.FilePath.Should().Be(@"Utilities\SymbolReferenceAnalyzer\Property_Partial1.cs");
                p1.Reference.Should().HaveCount(5, because: "a class, three properties, and a method are declared");
                var secondReference = p1.Reference[1];
                secondReference.Declaration.StartLine.Should().Be(5, because: "the property is declared here");
                secondReference.Reference.Should().ContainSingle(because: "the property is referenced inside Reference1() once").Subject.StartLine.Should().Be(12, because: "The reference to Property happens here");

                var declaredAndImplementedReference = p1.Reference[2];
                declaredAndImplementedReference.Declaration.StartLine.Should().Be(7, because: "the DeclaredAndImplemented property is declared here");
                declaredAndImplementedReference.Reference.Should().ContainSingle(because: "the method is referenced inside Reference1() once").Subject.StartLine.Should().Be(13, because: "The reference to DeclaredAndImplemented happens here");

                var declaredAndImplementedReference2 = p1.Reference[3];
                declaredAndImplementedReference2.Declaration.StartLine.Should().Be(8, because: "the DeclaredAndImplemented property is implemented here");
                declaredAndImplementedReference2.Reference.Should().ContainSingle(because: "the method is referenced inside Reference1() once").Subject.StartLine.Should().Be(13, because: "The reference to DeclaredAndImplemented happens here");
            },
            p2 =>
            {
                p2.FilePath.Should().Be(@"Utilities\SymbolReferenceAnalyzer\Property_Partial2.cs");
                p2.Reference.Should().HaveCount(3, because: "a class, a property, and a method are declared");
                var secondReference = p2.Reference[1];
                secondReference.Declaration.StartLine.Should().Be(5, because: "the property is declared here");
                secondReference.Reference.Should().ContainSingle(because: "the property is referenced inside Reference2() once").Subject.StartLine.Should().Be(9, because: "The reference to Property happens here");
            }));
    }

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_Property_VB(ProjectType projectType) =>
        Verify("Property.vb", projectType, 5, 3, 6, 7, 8);

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_TypeParameter_CS(ProjectType projectType) =>
        Verify("TypeParameter.cs", projectType, 5, 2, 4, 6);

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_TypeParameter_VB(ProjectType projectType) =>
        Verify("TypeParameter.vb", projectType, 5, 2, 4, 5);

    [TestMethod]
    public void Verify_TokenThreshold() =>
        // In TokenThreshold.cs there are 40009 tokens which is more than the current limit of 40000
        Verify("TokenThreshold.cs", ProjectType.Product, _ => { }, false);

    [TestMethod]
    [DataRow("Method.cs", true)]
    [DataRow("SomethingElse.cs", false)]
    public void Verify_UnchangedFiles(string unchangedFileName, bool expectedProtobufIsEmpty)
    {
        var builder = CreateBuilder(ProjectType.Product, "Method.cs").WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithUnchangedFiles(TestContext, BasePath + unchangedFileName));
        if (expectedProtobufIsEmpty)
        {
            builder.VerifyUtilityAnalyzerProducesEmptyProtobuf();
        }
        else
        {
            builder.VerifyUtilityAnalyzer<SymbolReferenceInfo>(x => x.Should().NotBeEmpty());
        }
    }

#if NET

    [TestMethod]
    public void Verify_Razor() =>
        CreateBuilder(ProjectType.Product, "Razor.razor", "Razor.razor.cs", "RazorComponent.razor", "ToDo.cs", "Razor.cshtml")
            .WithConcurrentAnalysis(false)
            .VerifyUtilityAnalyzer<SymbolReferenceInfo>(x =>
                {
                    var orderedSymbols = x.OrderBy(x => x.FilePath, StringComparer.InvariantCulture).ToArray();
                    orderedSymbols.Select(x => Path.GetFileName(x.FilePath)).Should().BeEquivalentTo("Razor.razor", "Razor.razor.cs", "RazorComponent.razor", "ToDo.cs");
                    orderedSymbols[0].FilePath.Should().EndWith("Razor.razor");

                    VerifyReferences(orderedSymbols[0].Reference, 10, 14, 5, 7, 21, 49);        // currentCount
                    VerifyReferences(orderedSymbols[0].Reference, 10, 17, 11, 21, 22);          // IncrementAmount
                    VerifyReferences(orderedSymbols[0].Reference, 10, 19, 9, 53);               // IncrementCount
                    VerifyReferences(orderedSymbols[0].Reference, 10, 35, 35);                  // x
                    VerifyReferences(orderedSymbols[0].Reference, 10, 38, 29, 35);              // todos
                    VerifyReferences(orderedSymbols[0].Reference, 10, 40, 26);                  // AddTodo
                    VerifyReferences(orderedSymbols[0].Reference, 10, 42);                      // x
                    VerifyReferences(orderedSymbols[0].Reference, 10, 43);                      // y
                    VerifyReferences(orderedSymbols[0].Reference, 10, 45, 42);                  // LocalMethod
                    VerifyReferences(orderedSymbols[0].Reference, 10, 58, 52);                  // AdditionalAttributes

                    VerifyReferencesColumns(orderedSymbols[0].Reference, 14, 5, 5, 19, 31);     // currentCount: line 5
                    VerifyReferencesColumns(orderedSymbols[0].Reference, 14, 7, 7, 1, 13);      // currentCount: line 7
                    VerifyReferencesColumns(orderedSymbols[0].Reference, 14, 21, 21, 8, 20);    // currentCount: line 21
                    VerifyReferencesColumns(orderedSymbols[0].Reference, 14, 49, 49, 26, 38);   // currentCount: line 49

                    orderedSymbols[1].FilePath.Should().EndWith("RazorComponent.razor");        // RazorComponent.razor
                    // https://github.com/SonarSource/sonar-dotnet/issues/8417
                    // before dotnet 8.0.5  SDK: Declaration (1,0) - (1,17) Reference (1,6) - (1,23) <- Overlapping
                    // Declaration of TSomeVeryLongName is placed starting at index 0 (ignoring "@typeparam ")
                    // Reference "where TSomeVeryLongName" is placed starting at index 6 (ignoring "@typeparam TSomeVeryLongName ")
                    VerifyReferences(orderedSymbols[1].Reference, 1, 1, 1);
                    orderedSymbols[1].Reference.Single().Reference.Single().Should().BeEquivalentTo(new TextRange { StartLine = 1, EndLine = 1, StartOffset = 35, EndOffset = 52 });
                });

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_PrimaryConstructor_PreciseLocation_CSharp12(ProjectType projectType) =>
        Verify("PrimaryConstructor.cs", projectType, x =>
        {
            x.Select(x => x.Declaration.StartLine).Should().BeEquivalentTo([1, 3, 6, 6, 8, 8, 10, 11, 12, 14, 17, 17, 19, 20, 21, 21, 23, 23, 25]);

            var primaryCtorParameter = x.Single(x => x.Declaration.StartLine == 8 && x.Declaration.StartOffset == 19); // b1, primary ctor
            primaryCtorParameter.Declaration.Should().BeEquivalentTo(new TextRange { StartLine = 8, EndLine = 8, StartOffset = 19, EndOffset = 21 });
            primaryCtorParameter.Reference.Should().BeEquivalentTo(
                [
                    new TextRange { StartLine = 10, EndLine = 10, StartOffset = 24, EndOffset = 26 }, // Field
                    new TextRange { StartLine = 11, EndLine = 11, StartOffset = 41, EndOffset = 43 }, // Property
                    new TextRange { StartLine = 12, EndLine = 12, StartOffset = 21, EndOffset = 23 }  // b1
                ]);

            var ctorDeclaration = x.Single(x => x.Declaration.StartLine == 17 && x.Declaration.StartOffset == 6); // B
            ctorDeclaration.Reference.Should().BeEmpty(); // FN, not reporting constructor 'B' and 'this' (line 21)

            var fieldNameEqualToParameter = x.Single(x => x.Declaration.StartLine == 12 && x.Declaration.StartOffset == 16); // b1, field
            fieldNameEqualToParameter.Reference.Should().Equal(
                new TextRange { StartLine = 14, EndLine = 14, StartOffset = 20, EndOffset = 22 }); // b1, returned by Method

            var ctorParameterDeclaration = x.Single(x => x.Declaration.StartLine == 21 && x.Declaration.StartOffset == 17); // b1, internal ctor
            ctorParameterDeclaration.Reference.Should().Equal(
                new TextRange { StartLine = 21, EndLine = 21, StartOffset = 36, EndOffset = 38 }); // b1, this parameter

            var primaryCtorParameterB = x.Single(x => x.Declaration.StartLine == 17 && x.Declaration.StartOffset == 12); // b1, primary ctor B
            primaryCtorParameterB.Reference.Should().BeEquivalentTo(
                [
                    new TextRange { StartLine = 19, EndLine = 19, StartOffset = 24, EndOffset = 26 }, // Field
                    new TextRange { StartLine = 20, EndLine = 20, StartOffset = 41, EndOffset = 43 }, // Property
                    new TextRange { StartLine = 25, EndLine = 25, StartOffset = 20, EndOffset = 22 }  // returned by Method
                ]);

            var classADeclaration = x.Single(x => x.Declaration.StartLine == 1 && x.Declaration.StartOffset == 13); // A
            classADeclaration.Reference.Should().BeEquivalentTo(
                [
                    new TextRange { StartLine = 6, EndLine = 6, StartOffset = 34, EndOffset = 35 },  // primary ctor default parameter
                    new TextRange { StartLine = 23, EndLine = 23, StartOffset = 25, EndOffset = 26 } // lambda default parameter
                ]);

            var constFieldDeclaration = x.Single(x => x.Declaration.StartLine == 3 && x.Declaration.StartOffset == 21); // I
            constFieldDeclaration.Reference.Should().BeEquivalentTo(
                [
                    new TextRange { StartLine = 6, EndLine = 6, StartOffset = 36, EndOffset = 37 },  // primary ctor default parameter
                    new TextRange { StartLine = 23, EndLine = 23, StartOffset = 27, EndOffset = 28 } // lambda default parameter
                ]);
        });

#endif

    private void Verify(string fileName, ProjectType projectType, int expectedDeclarationCount, int assertedDeclarationLine, params int[] assertedDeclarationLineReferences) =>
        Verify(fileName, projectType, x => VerifyReferences(x, expectedDeclarationCount, assertedDeclarationLine, assertedDeclarationLineReferences));

    private void Verify(string fileName,
                        ProjectType projectType,
                        Action<IReadOnlyList<SymbolReferenceInfo.Types.SymbolReference>> verifyReference,
                        bool isMessageExpected = true) =>
        CreateBuilder(projectType, fileName)
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, projectType))
            .VerifyUtilityAnalyzer<SymbolReferenceInfo>(x =>
                {
                    x.Should().HaveCount(isMessageExpected ? 1 : 0);

                    if (isMessageExpected)
                    {
                        var info = x.Single();
                        info.FilePath.Should().Be(Path.Combine(BasePath, fileName));
                        verifyReference(info.Reference);
                    }
                });

    private VerifierBuilder CreateBuilder(ProjectType projectType, params string[] fileNames)
    {
        var testRoot = BasePath + TestContext.TestName;
        var language = AnalyzerLanguage.FromPath(fileNames[0]);
        UtilityAnalyzerBase analyzer = language.LanguageName switch
        {
            LanguageNames.CSharp => new TestSymbolReferenceAnalyzer_CS(testRoot, projectType == ProjectType.Test),
            LanguageNames.VisualBasic => new TestSymbolReferenceAnalyzer_VB(testRoot, projectType == ProjectType.Test),
            _ => throw new UnexpectedLanguageException(language)
        };
        return new VerifierBuilder()
            .AddAnalyzer(() => analyzer)
            .AddPaths(fileNames)
            .WithBasePath(BasePath)
            .WithOptions(LanguageOptions.Latest(language))
            .WithProtobufPath(@$"{testRoot}\symrefs.pb");
    }

    private static void VerifyReferences(IReadOnlyList<SymbolReferenceInfo.Types.SymbolReference> references,
                                         int expectedDeclarationCount,
                                         int assertedDeclarationLine,
                                         params int[] assertedDeclarationLineReferences)
    {
        references.Where(x => x.Declaration is not null).Should().HaveCount(expectedDeclarationCount);
        references.Should().ContainSingle(x => x.Declaration.StartLine == assertedDeclarationLine).Subject.Reference.Select(x => x.StartLine)
                  .Should().BeEquivalentTo(assertedDeclarationLineReferences);
    }

    private static void VerifyReferencesColumns(IReadOnlyList<SymbolReferenceInfo.Types.SymbolReference> symbolReference,
                                                int declarationLine,
                                                int startLine,
                                                int endLine,
                                                int startOffset,
                                                int endOffset) =>
        symbolReference.Should().ContainSingle(x => x.Declaration.StartLine == declarationLine).Subject.Reference
            .Should().ContainSingle(x => x.StartLine == startLine && x.EndLine == endLine && x.StartOffset == startOffset && x.EndOffset == endOffset);

    // We need to set protected properties and this class exists just to enable the analyzer without bothering with additional files with parameters
    private sealed class TestSymbolReferenceAnalyzer_CS : CS.SymbolReferenceAnalyzer
    {
        private readonly string outPath;
        private readonly bool isTestProject;

        public TestSymbolReferenceAnalyzer_CS(string outPath, bool isTestProject)
        {
            this.outPath = outPath;
            this.isTestProject = isTestProject;
        }

        protected override UtilityAnalyzerParameters ReadParameters(IAnalysisContext context) =>
            base.ReadParameters(context) with { IsAnalyzerEnabled = true, OutPath = outPath, IsTestProject = isTestProject };
    }

    private sealed class TestSymbolReferenceAnalyzer_VB : VB.SymbolReferenceAnalyzer
    {
        private readonly string outPath;
        private readonly bool isTestProject;

        public TestSymbolReferenceAnalyzer_VB(string outPath, bool isTestProject)
        {
            this.outPath = outPath;
            this.isTestProject = isTestProject;
        }

        protected override UtilityAnalyzerParameters ReadParameters(IAnalysisContext context) =>
            base.ReadParameters(context) with { IsAnalyzerEnabled = true, OutPath = outPath, IsTestProject = isTestProject };
    }
}

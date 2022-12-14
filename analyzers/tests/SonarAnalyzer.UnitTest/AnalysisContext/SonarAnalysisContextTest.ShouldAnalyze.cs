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

using SonarAnalyzer.Common;

namespace SonarAnalyzer.UnitTest;

public partial class SonarAnalysisContextTest
{
    [TestMethod]
    public void ShouldAnalyze_SonarLint()
    {
        var options = new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty);    // No SonarProjectConfig.xml

        ShouldAnalyze(options).Should().BeTrue();
    }

    [TestMethod]
    public void ShouldAnalyze_Scanner_UnchangedFiles_NotAvailable()
    {
        var sonarProjectConfig = TestHelper.CreateSonarProjectConfig(TestContext, ProjectType.Product); // SonarProjectConfig.xml without UnchangedFiles.txt
        var additionalFile = new AnalyzerAdditionalFile(sonarProjectConfig);
        var options = new AnalyzerOptions(ImmutableArray.Create<AdditionalText>(additionalFile));

        ShouldAnalyze(options).Should().BeTrue();
    }

    [TestMethod]
    public void ShouldAnalyze_Scanner_UnchangedFiles_Empty()
    {
        var options = CreateOptions(Array.Empty<string>());

        ShouldAnalyze(options).Should().BeTrue();
    }

    [TestMethod]
    public void ShouldAnalyze_Scanner_UnchangedFiles_ContainsTreeFile()
    {
        var options = CreateOptions(new[] { OtherFileName + "cs" });

        ShouldAnalyze(options).Should().BeFalse("File is known to be Unchanged in Incremental PR analysis");
    }

    [TestMethod]
    public void ShouldAnalyze_Scanner_UnchangedFiles_ContainsOtherFile()
    {
        var options = CreateOptions(new[] { "ThisIsNotInCompilation.cs", "SomethingElse.cs" });

        ShouldAnalyze(options).Should().BeTrue();
    }

    private static bool ShouldAnalyze(AnalyzerOptions options)
    {
        var compilation = CreateDummyCompilation(AnalyzerLanguage.CSharp);
        return CreateSut().ShouldAnalyze(CSharpGeneratedCodeRecognizer.Instance, compilation.SyntaxTrees.Single(x => x.FilePath.Contains(OtherFileName)), compilation, options);
    }

    private AnalyzerOptions CreateOptions(string[] unchangedFiles)
    {
        var sonarProjectConfig = TestHelper.CreateSonarProjectConfig(TestContext, unchangedFiles);
        var additionalFile = new AnalyzerAdditionalFile(sonarProjectConfig);
        return new(ImmutableArray.Create<AdditionalText>(additionalFile));
    }
}

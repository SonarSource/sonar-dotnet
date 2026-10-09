/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.CSharp;

namespace SonarAnalyzer.TestFramework.Test.Build;

[TestClass]
public class RazorAnalyzerConfigOptionsProviderTest
{
    private const string RootPath = "C:/Users/Johnny/source/repos/WebApplication";
    private const string TargetPathKey = "build_metadata.AdditionalFiles.TargetPath";

    [TestMethod]
    public void Constructor_NullRootPath() =>
        FluentActions.Invoking(() => new RazorAnalyzerConfigOptionsProvider(null)).Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("rootPath");

    [TestMethod]
    [DataRow("C:/Users/Johnny/source/repos/WebApplication/Component.razor", "Q29tcG9uZW50LnJhem9y")]
    [DataRow("C:/Users/Johnny/source/repos/WebApplication/Folder/Child.razor", "Rm9sZGVyXENoaWxkLnJhem9y")]
    [DataRow("C:/Users/Johnny/source/repos/Parent.razor", "Li5cUGFyZW50LnJhem9y")]
    public void GetOptions_AdditionalFile(string razorFile, string expectedTargetPath)
    {
        new RazorAnalyzerConfigOptionsProvider(RootPath).GetOptions(new AnalyzerAdditionalFile(razorFile)).TryGetValue(TargetPathKey, out var value).Should().BeTrue();
        value.Should().Be(expectedTargetPath);
    }

    [TestMethod]
    public void GetOptions_AdditionalFile_UnknownKey()
    {
        new RazorAnalyzerConfigOptionsProvider(RootPath).GetOptions(new AnalyzerAdditionalFile($"{RootPath}/Component.razor")).TryGetValue("build_property.RootNamespace", out var value).Should().BeFalse();
        value.Should().BeNull();
    }

    [TestMethod]
    public void GlobalOptions()
    {
        new RazorAnalyzerConfigOptionsProvider(RootPath).GlobalOptions.TryGetValue(TargetPathKey, out var value).Should().BeFalse();
        value.Should().BeNull();
    }

    [TestMethod]
    public void GetOptions_SyntaxTree()
    {
        new RazorAnalyzerConfigOptionsProvider(RootPath).GetOptions(CSharpSyntaxTree.ParseText("class Sample { }")).TryGetValue(TargetPathKey, out var value).Should().BeFalse();
        value.Should().BeNull();
    }
}

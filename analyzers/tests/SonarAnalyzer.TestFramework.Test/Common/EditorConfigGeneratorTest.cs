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

namespace SonarAnalyzer.TestFramework.Test.Common;

/*
 * This class is copy-pasted from EditorConfigGeneratorTest in Autoscan .Net.
 * See https://github.com/SonarSource/sonar-dotnet-autoscan/blob/master/AutoScan.NET.Test/Build/EditorConfigGeneratorTest.cs
*/
[TestClass]
public class EditorConfigGeneratorTest
{
    [TestMethod]
    public void EditorConfigGenerator_NullRootPath_Throws()
    {
        var createWithNull = () => _ = new EditorConfigGenerator(null);
        createWithNull.Should().Throw<ArgumentException>().And.ParamName.Should().Be("rootPath");
    }

    [TestMethod]
    public void EditorConfigGenerator_EmptyOrWhiteSpaceRootPath_Throws()
    {
        var createWithEmpty = () => _ = new EditorConfigGenerator(string.Empty);
        createWithEmpty.Should().Throw<ArgumentException>().And.ParamName.Should().Be("rootPath");

        var createWithWhiteSpace = () => _ = new EditorConfigGenerator("     ");
        createWithWhiteSpace.Should().Throw<ArgumentException>().And.ParamName.Should().Be("rootPath");
    }

    [TestMethod]
    public void GenerateEditorConfig_NullRazorFiles_Throws()
    {
        var generateWithNull = () => _ = new EditorConfigGenerator("C:/Users").Generate(null);
        generateWithNull.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("source");
    }

    [TestMethod]
    public void GenerateEditorConfig_EmptyCollection_ValidEditorConfig()
    {
        var rootPath = "C:/Users/Johnny/source/repos/WebApplication";
        var editorConfig = new EditorConfigGenerator(rootPath).Generate([]);
        editorConfig.Should().Be("is_global = true");
    }

    [TestMethod]
    public void GenerateEditorConfig_SingleFile_ValidEditorConfig()
    {
        var rootPath = "C:/Users/Johnny/source/repos/WebApplication";
        var razorFile = "C:/Users/Johnny/source/repos/WebApplication/Component.razor";
        var editorConfig = new EditorConfigGenerator(rootPath).Generate([razorFile]);
        editorConfig.Should().BeIgnoringLineEndings("""
            is_global = true
            [C:/Users/Johnny/source/repos/WebApplication/Component.razor]
            build_metadata.AdditionalFiles.TargetPath = Q29tcG9uZW50LnJhem9y
            """);
    }

    [TestMethod]
    public void GenerateEditorConfig_MultipleFiles_ValidEditorConfig()
    {
        var rootPath = "C:/Users/Johnny/source/repos/WebApplication";
        var razorFiles = new List<string>()
        {
            "C:/Users/Johnny/source/repos/WebApplication/Component.razor",
            "C:/Users/Johnny/source/repos/WebApplication/Folder/Child.razor",
            "C:/Users/Johnny/source/repos/Parent.razor"
        };
        var editorConfig = new EditorConfigGenerator(rootPath).Generate(razorFiles);
        editorConfig.Should().BeIgnoringLineEndings("""
            is_global = true
            [C:/Users/Johnny/source/repos/WebApplication/Component.razor]
            build_metadata.AdditionalFiles.TargetPath = Q29tcG9uZW50LnJhem9y
            [C:/Users/Johnny/source/repos/WebApplication/Folder/Child.razor]
            build_metadata.AdditionalFiles.TargetPath = Rm9sZGVyXENoaWxkLnJhem9y
            [C:/Users/Johnny/source/repos/Parent.razor]
            build_metadata.AdditionalFiles.TargetPath = Li5cUGFyZW50LnJhem9y
            """);
    }

    [TestMethod]
    public void GenerateEditorConfig_NullFile_Throws()
    {
        var rootPath = "C:/Users/Johnny/source/repos/WebApplication";
        var generateWithNullElement = () => _ = new EditorConfigGenerator(rootPath).Generate([null]);
        generateWithNullElement.Should().Throw<NullReferenceException>();
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("          ")]
    public void GenerateEditorConfig_EmptyFile_Throws(string element)
    {
        var rootPath = "C:/Users/Johnny/source/repos/WebApplication";
        var generateWithNullElement = () => _ = new EditorConfigGenerator(rootPath).Generate([element]);
        generateWithNullElement.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void GenerateEditorConfig_MixedFiles_Throws()
    {
        var rootPath = "C:/Users/Johnny/source/repos/WebApplication";
        var generateMixedWithNullElement = () => _ = new EditorConfigGenerator(rootPath).Generate([null, "C:/Users/Johnny/source/repos/WebApplication/Component.razor", string.Empty]);
        generateMixedWithNullElement.Should().Throw<NullReferenceException>();
    }
}

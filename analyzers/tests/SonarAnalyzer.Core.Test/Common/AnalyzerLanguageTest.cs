/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Core.Test.Common;

[TestClass]
public class AnalyzerLanguageTest
{
    [TestMethod]
    public void ToString_ReturnsLanguageName()
    {
        AnalyzerLanguage.CSharp.ToString().Should().Be("C#");
        AnalyzerLanguage.VisualBasic.ToString().Should().Be("Visual Basic");
    }

    [DataTestMethod]
    [DataRow("File.cs")]
    [DataRow("File.Cs")]
    [DataRow("File.CS")]
    [DataRow("File.razor")]
    [DataRow(@"C:\Project\File.cs")]
    [DataRow(@"/c/Project/File.cs")]
    [DataRow(@"/c/Project/File.razor")]
    public void FromPath_CS(string path) =>
        AnalyzerLanguage.FromPath(path).Should().Be(AnalyzerLanguage.CSharp);

    [DataTestMethod]
    [DataRow("File.vb")]
    [DataRow("File.Vb")]
    [DataRow("File.VB")]
    [DataRow(@"C:\Project\File.vb")]
    [DataRow(@"/c/Project/File.vb")]
    public void FromPath_VB(string path) =>
        AnalyzerLanguage.FromPath(path).Should().Be(AnalyzerLanguage.VisualBasic);

    [DataTestMethod]
    [DataRow("File.txt", ".txt")]
    [DataRow("", "")]
    [DataRow(null, "")]
    public void FromPath_UnexpectedThrows(string path, string message)
    {
        var action = () => AnalyzerLanguage.FromPath(path);
        action.Should().Throw<NotSupportedException>().WithMessage($"Unsupported file extension: {message}");
    }

    [TestMethod]
    public void FromName()
    {
        AnalyzerLanguage.FromName(LanguageNames.CSharp).Should().Be(AnalyzerLanguage.CSharp);
        AnalyzerLanguage.FromName(LanguageNames.VisualBasic).Should().Be(AnalyzerLanguage.VisualBasic);
    }

    [TestMethod]
    public void FromName_UnexpectedThrows() =>
        ((Func<AnalyzerLanguage>)(() => AnalyzerLanguage.FromName(LanguageNames.FSharp))).Should().Throw<NotSupportedException>().WithMessage("Unsupported language name: F#");

    [TestMethod]
    public void HelpLink_ReturnsUrl()
    {
        AnalyzerLanguage.CSharp.HelpLink("S2222").Should().Be("https://rules.sonarsource.com/csharp/RSPEC-2222");
        AnalyzerLanguage.VisualBasic.HelpLink("S2222").Should().Be("https://rules.sonarsource.com/vbnet/RSPEC-2222");
    }

    [TestMethod]
    public void HelpLink_StylingRules_IsEmpty()
    {
        AnalyzerLanguage.CSharp.HelpLink("T2222").Should().BeNull();
        AnalyzerLanguage.VisualBasic.HelpLink("T2222").Should().BeNull();
    }
}

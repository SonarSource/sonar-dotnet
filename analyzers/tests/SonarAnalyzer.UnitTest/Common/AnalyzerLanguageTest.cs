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

namespace SonarAnalyzer.UnitTest.Common
{
    [TestClass]
    public class AnalyzerLanguageTest
    {
        [TestMethod]
        public void Parse()
        {
            AnalyzerLanguage.Parse("cs").Should().Be(AnalyzerLanguage.CSharp);
            AnalyzerLanguage.Parse("vbnet").Should().Be(AnalyzerLanguage.VisualBasic);
        }

        [TestMethod]
        public void Parse_Fail() =>
            Assert.ThrowsException<NotSupportedException>(() => AnalyzerLanguage.Parse("csharp"));

        [TestMethod]
        public void ToString_ReturnsValue()
        {
            AnalyzerLanguage.CSharp.ToString().Should().Be("cs");
            AnalyzerLanguage.VisualBasic.ToString().Should().Be("vbnet");
        }

        [DataTestMethod]
        [DataRow("File.cs")]
        [DataRow("File.Cs")]
        [DataRow("File.CS")]
        [DataRow(@"C:\Project\File.cs")]
        [DataRow(@"/c/Project/File.cs")]
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

        [TestMethod]
        public void FromPath_UnexpectedThrows() =>
            ((Func<AnalyzerLanguage>)(() => AnalyzerLanguage.FromPath("File.txt"))).Should().Throw<NotSupportedException>().WithMessage("Unsupported file extension: .txt");

        [TestMethod]
        public void FromName()
        {
            AnalyzerLanguage.FromName(LanguageNames.CSharp).Should().Be(AnalyzerLanguage.CSharp);
            AnalyzerLanguage.FromName(LanguageNames.VisualBasic).Should().Be(AnalyzerLanguage.VisualBasic);
        }

        [TestMethod]
        public void FromName_UnexpectedThrows() =>
            ((Func<AnalyzerLanguage>)(() => AnalyzerLanguage.FromName(LanguageNames.FSharp))).Should().Throw<NotSupportedException>().WithMessage("Unsupported language name: F#");
    }
}

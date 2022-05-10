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

using Microsoft.CodeAnalysis;
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
        public void GetDirectory()
        {
            AnalyzerLanguage.CSharp.DirectoryName.Should().Be("CSharp");
            AnalyzerLanguage.VisualBasic.DirectoryName.Should().Be("VisualBasic");
        }

        [TestMethod]
        public void GetQualityProfileRepositoryKey()
        {
            AnalyzerLanguage.CSharp.RepositoryKey.Should().Be("csharpsquid");
            AnalyzerLanguage.VisualBasic.RepositoryKey.Should().Be("vbnet");
        }

        [TestMethod]
        public void AddLanguage()
        {
            AnalyzerLanguage.CSharp.AddLanguage(AnalyzerLanguage.VisualBasic).Should().Be(AnalyzerLanguage.Both);
            AnalyzerLanguage.CSharp.AddLanguage(AnalyzerLanguage.CSharp).Should().Be(AnalyzerLanguage.CSharp);
            AnalyzerLanguage.CSharp.AddLanguage(AnalyzerLanguage.Both).Should().Be(AnalyzerLanguage.Both);
            AnalyzerLanguage.CSharp.Invoking(x => x.AddLanguage(null)).Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void IsAlso()
        {
            AnalyzerLanguage.CSharp.IsAlso(AnalyzerLanguage.CSharp).Should().BeTrue();
            AnalyzerLanguage.CSharp.IsAlso(AnalyzerLanguage.VisualBasic).Should().BeFalse();
            AnalyzerLanguage.Both.IsAlso(AnalyzerLanguage.VisualBasic).Should().BeTrue();
            AnalyzerLanguage.CSharp.Invoking<AnalyzerLanguage>(x => x.IsAlso(null)).Should().Throw<ArgumentNullException>();
            AnalyzerLanguage.CSharp.Invoking<AnalyzerLanguage>(x => x.IsAlso(AnalyzerLanguage.None)).Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void LanguageProperties_None_Throws()
        {
            AnalyzerLanguage.None.Invoking(x => x.LanguageName).Should().Throw<NotSupportedException>();
            AnalyzerLanguage.None.Invoking(x => x.RepositoryKey).Should().Throw<NotSupportedException>();
            AnalyzerLanguage.None.Invoking(x => x.DirectoryName).Should().Throw<NotSupportedException>();
            AnalyzerLanguage.None.Invoking(x => x.FileExtension).Should().Throw<NotSupportedException>();
            AnalyzerLanguage.None.Invoking(x => x.ResourceSuffix).Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void LanguageProperties_Both_Throws()
        {
            AnalyzerLanguage.Both.Invoking(x => x.LanguageName).Should().Throw<NotSupportedException>();
            AnalyzerLanguage.Both.Invoking(x => x.RepositoryKey).Should().Throw<NotSupportedException>();
            AnalyzerLanguage.Both.Invoking(x => x.DirectoryName).Should().Throw<NotSupportedException>();
            AnalyzerLanguage.Both.Invoking(x => x.FileExtension).Should().Throw<NotSupportedException>();
            AnalyzerLanguage.Both.Invoking(x => x.ResourceSuffix).Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void ToString_ReturnsValue()
        {
            AnalyzerLanguage.CSharp.ToString().Should().Be("cs");
            AnalyzerLanguage.VisualBasic.ToString().Should().Be("vbnet");
            AnalyzerLanguage.None.ToString().Should().Be("none");
            AnalyzerLanguage.Both.ToString().Should().Be("both");
        }

        [TestMethod]
        public void FromPath_Unexpected_ReturnsNone() =>
            AnalyzerLanguage.FromPath("File.txt").Should().Be(AnalyzerLanguage.None);

        [TestMethod]
        public void FromName()
        {
            AnalyzerLanguage.FromName(LanguageNames.CSharp).Should().Be(AnalyzerLanguage.CSharp);
            AnalyzerLanguage.FromName(LanguageNames.VisualBasic).Should().Be(AnalyzerLanguage.VisualBasic);
            AnalyzerLanguage.FromName(LanguageNames.FSharp).Should().Be(AnalyzerLanguage.None);
            AnalyzerLanguage.FromName("Random").Should().Be(AnalyzerLanguage.None);
        }
    }
}

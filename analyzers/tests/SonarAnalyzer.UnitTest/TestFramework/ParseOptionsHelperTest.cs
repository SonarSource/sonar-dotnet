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

using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.Helpers;

using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    [TestClass]
    public class ParseOptionsHelperTest
    {
        [TestMethod]
        public void ExpectedLanguageVersion()
        {
            if (TestContextHelper.IsNotPullRequestBuild)
            {
                CS.LanguageVersion[] csVersions =
                {
                    CS.LanguageVersion.CSharp6,
                    CS.LanguageVersion.CSharp7,
                    CS.LanguageVersion.CSharp7_1,
                    CS.LanguageVersion.CSharp7_2,
                    CS.LanguageVersion.CSharp7_3,
                    CS.LanguageVersion.CSharp8,
                    CS.LanguageVersion.CSharp9
                };
                // the following assertion is expected to fail when we add the support of C#10
                ParseOptionsHelper.FromCSharp6.Should().Contain(csVersions.Select(x => new CS.CSharpParseOptions(x)));

                VB.LanguageVersion[] vbVersions =
                {
                    VB.LanguageVersion.VisualBasic12,
                    VB.LanguageVersion.VisualBasic14,
                    VB.LanguageVersion.VisualBasic15,
                    VB.LanguageVersion.VisualBasic15_3,
                    VB.LanguageVersion.VisualBasic15_5,
                    VB.LanguageVersion.VisualBasic16
                };
                ParseOptionsHelper.FromVisualBasic12.Should().Contain(vbVersions.Select(x => new VB.VisualBasicParseOptions(x)));
            }
            else
            {
                ParseOptionsHelper.FromCSharp6.Should().Contain(new CS.CSharpParseOptions(CS.LanguageVersion.CSharp6));
                ParseOptionsHelper.FromVisualBasic12.Should().Contain(new VB.VisualBasicParseOptions(VB.LanguageVersion.VisualBasic12));
            }
        }
    }
}

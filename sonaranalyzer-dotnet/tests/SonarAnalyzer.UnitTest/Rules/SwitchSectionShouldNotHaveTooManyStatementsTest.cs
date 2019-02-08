/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

extern alias csharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CS=SonarAnalyzer.Rules.CSharp;
using VB=SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class SwitchSectionShouldNotHaveTooManyStatementsTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void SwitchSectionShouldNotHaveTooManyStatements_DefaultValue_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\SwitchSectionShouldNotHaveTooManyStatements_DefaultValue.cs",
                new CS.SwitchSectionShouldNotHaveTooManyStatements());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void SwitchSectionShouldNotHaveTooManyStatements_CustomValue_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\SwitchSectionShouldNotHaveTooManyStatements_CustomValue.cs",
                new CS.SwitchSectionShouldNotHaveTooManyStatements { Threshold = 1 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void SwitchSectionShouldNotHaveTooManyStatements_DefaultValue_VB()
        {
            Verifier.VerifyAnalyzer(@"TestCases\SwitchSectionShouldNotHaveTooManyStatements_DefaultValue.vb",
                new VB.SwitchSectionShouldNotHaveTooManyStatements());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void SwitchSectionShouldNotHaveTooManyStatements_CustomValue_VB()
        {
            Verifier.VerifyAnalyzer(@"TestCases\SwitchSectionShouldNotHaveTooManyStatements_CustomValue.vb",
                new VB.SwitchSectionShouldNotHaveTooManyStatements() { Threshold = 1 });
        }
    }
}


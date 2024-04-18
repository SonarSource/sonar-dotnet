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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class TooManyParametersTest
    {
        private readonly VerifierBuilder builderCSMax3 = new VerifierBuilder().AddAnalyzer(() => new CS.TooManyParameters { Maximum = 3 });
        private readonly VerifierBuilder builderVBMax3 = new VerifierBuilder().AddAnalyzer(() => new VB.TooManyParameters { Maximum = 3 });

        [TestMethod]
        public void TooManyParameters_CS_CustomValues() =>
            builderCSMax3.AddPaths("TooManyParameters_CustomValues.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

#if NET

        [TestMethod]
        public void TooManyParameters_CS_CustomValues_CSharp9() =>
             builderCSMax3.AddPaths("TooManyParameters_CustomValues.CSharp9.cs")
                .WithTopLevelStatements()
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .Verify();

        [TestMethod]
        public void TooManyParameters_CS_CustomValues_CSharp11() =>
            builderCSMax3.AddPaths("TooManyParameters_CustomValues.CSharp11.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .Verify();

        [TestMethod]
        public void TooManyParameters_CS_CustomValues_CSharp12() =>
            builderCSMax3.AddPaths("TooManyParameters_CustomValues.CSharp12.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp12)
                .Verify();

#endif

        [TestMethod]
        public void TooManyParameters_VB_CustomValues() =>
            builderVBMax3.AddPaths("TooManyParameters_CustomValues.vb").Verify();

        [TestMethod]
        public void TooManyParameters_CS_DefaultValues() =>
            new VerifierBuilder<CS.TooManyParameters>().AddPaths("TooManyParameters_DefaultValues.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

        [TestMethod]
        public void TooManyParameters_VB_DefaultValues() =>
            new VerifierBuilder<VB.TooManyParameters>().AddPaths("TooManyParameters_DefaultValues.vb").Verify();
    }
}

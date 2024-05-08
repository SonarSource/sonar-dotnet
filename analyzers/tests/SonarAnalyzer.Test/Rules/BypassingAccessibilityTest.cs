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
    public class BypassingAccessibilityTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.BypassingAccessibility>();
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.BypassingAccessibility>();

        [TestMethod]
        public void BypassingAccessibility_CS() =>
            builderCS.AddPaths("BypassingAccessibility.cs").Verify();

#if NET

        [TestMethod]
        public void BypassingAccessibility_CSharp9() =>
            builderCS.AddPaths("BypassingAccessibility.CSharp9.cs").WithTopLevelStatements().Verify();

        [TestMethod]
        public void BypassingAccessibility_CSharp11() =>
            builderCS.AddPaths("BypassingAccessibility.CSharp11.cs").WithOptions(ParseOptionsHelper.FromCSharp11).Verify();

        [TestMethod]
        public void BypassingAccessibility_CSharp12() =>
            builderCS.AddPaths("BypassingAccessibility.CSharp12.cs").WithOptions(ParseOptionsHelper.FromCSharp12).VerifyNoIssues();  // zero-overhead member access is not supported yet

#endif

        [TestMethod]
        public void BypassingAccessibility_VB() =>
            builderVB.AddPaths("BypassingAccessibility.vb").Verify();
    }
}

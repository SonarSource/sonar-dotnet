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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class ShouldImplementExportedInterfacesTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ShouldImplementExportedInterfaces>().AddReferences(MetadataReferenceFacade.SystemComponentModelComposition);
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.ShouldImplementExportedInterfaces>().AddReferences(MetadataReferenceFacade.SystemComponentModelComposition);

        [TestMethod]
        public void ShouldImplementExportedInterfaces_CS() =>
            builderCS.AddPaths("ShouldImplementExportedInterfaces.cs").Verify();

#if NET

        [TestMethod]
        public void ShouldImplementExportedInterfaces_CSharp9() =>
            builderCS.AddPaths("ShouldImplementExportedInterfaces.CSharp9.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

#endif

        [TestMethod]
        public void ShouldImplementExportedInterfaces_VB() =>
            builderVB.AddPaths("ShouldImplementExportedInterfaces.vb").Verify();

        [TestMethod]
        public void ShouldImplementExportedInterfaces_Partial() =>
            builderCS.AddPaths("ShouldImplementExportedInterfaces_Part1.cs", "ShouldImplementExportedInterfaces_Part2.cs").Verify();
    }
}

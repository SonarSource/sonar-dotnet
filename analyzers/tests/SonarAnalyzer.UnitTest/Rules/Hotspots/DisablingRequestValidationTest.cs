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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class DisablingRequestValidationTest
    {
        private const string AspNetMvcVersion = "5.2.7";

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void DisablingRequestValidation_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\Hotspots\DisablingRequestValidation.cs",
                new CS.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: NuGetMetadataReference.MicrosoftAspNetMvc(AspNetMvcVersion));

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void DisablingRequestValidation_VB() =>
            Verifier.VerifyAnalyzer(@"TestCases\Hotspots\DisablingRequestValidation.vb",
                new VB.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: NuGetMetadataReference.MicrosoftAspNetMvc(AspNetMvcVersion));
    }
}

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

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class SerializationConstructorsShouldBeSecuredTest
    {
        [TestMethod]
        public void SerializationConstructorsShouldBeSecured() =>
            OldVerifier.VerifyNonConcurrentAnalyzer(@"TestCases\SerializationConstructorsShouldBeSecured.cs",
                                    new SerializationConstructorsShouldBeSecured(),
                                    GetAdditionalReferences());

#if NET
        [TestMethod]
        public void SerializationConstructorsShouldBeSecured_CSharp9() =>
            OldVerifier.VerifyAnalyzerFromCSharp9Library(@"TestCases\SerializationConstructorsShouldBeSecured.CSharp9.cs",
                                    new SerializationConstructorsShouldBeSecured(),
                                    GetAdditionalReferences());
#endif

        [TestMethod]
        public void SerializationConstructorsShouldBeSecured_InvalidCode() =>
            OldVerifier.VerifyCSharpAnalyzer(@"
[Serializable]
    public partial class InvalidCode : ISerializable
    {
        [FileIOPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        [ZoneIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
        public InvalidCode() { }

        protected (SerializationInfo info, StreamingContext context) { }

        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }", new SerializationConstructorsShouldBeSecured(), CompilationErrorBehavior.Ignore);

        [TestMethod]
        public void SerializationConstructorsShouldBeSecured_NoAssemblyAttribute() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\SerializationConstructorsShouldBeSecured_NoAssemblyAttribute.cs",
                                    new SerializationConstructorsShouldBeSecured(),
                                    GetAdditionalReferences());

        [TestMethod]
        public void SerializationConstructorsShouldBeSecured_PartialClasses() =>
            OldVerifier.VerifyNonConcurrentAnalyzer(
                                    new[]
                                    {
                                        @"TestCases\SerializationConstructorsShouldBeSecured_Part1.cs",
                                        @"TestCases\SerializationConstructorsShouldBeSecured_Part2.cs",
                                    },
                                    new SerializationConstructorsShouldBeSecured(),
                                    GetAdditionalReferences());

        private static IEnumerable<MetadataReference> GetAdditionalReferences() =>
            MetadataReferenceFacade.SystemSecurityPermissions;
    }
}

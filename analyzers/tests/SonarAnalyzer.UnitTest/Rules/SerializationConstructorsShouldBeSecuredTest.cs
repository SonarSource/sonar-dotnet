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
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class SerializationConstructorsShouldBeSecuredTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<SerializationConstructorsShouldBeSecured>().AddReferences(MetadataReferenceFacade.SystemSecurityPermissions);

        [TestMethod]
        public void SerializationConstructorsShouldBeSecured() =>
            builder.AddPaths("SerializationConstructorsShouldBeSecured.cs").WithConcurrentAnalysis(false).Verify();

#if NET

        [TestMethod]
        public void SerializationConstructorsShouldBeSecured_CSharp9() =>
            builder.AddPaths("SerializationConstructorsShouldBeSecured.CSharp9.cs").WithConcurrentAnalysis(false).WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

#endif

        [TestMethod]
        public void SerializationConstructorsShouldBeSecured_InvalidCode() =>
            builder.AddSnippet(@"
[Serializable]
    public partial class InvalidCode : ISerializable
    {
        [FileIOPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        [ZoneIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
        public InvalidCode() { }

        protected (SerializationInfo info, StreamingContext context) { }

        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }").WithErrorBehavior(CompilationErrorBehavior.Ignore).Verify();

        [TestMethod]
        public void SerializationConstructorsShouldBeSecured_NoAssemblyAttribute() =>
            builder.AddPaths("SerializationConstructorsShouldBeSecured_NoAssemblyAttribute.cs").Verify();

        [TestMethod]
        public void SerializationConstructorsShouldBeSecured_PartialClasses() =>
            builder.AddPaths("SerializationConstructorsShouldBeSecured_Part1.cs", "SerializationConstructorsShouldBeSecured_Part2.cs").WithConcurrentAnalysis(false).Verify();
    }
}

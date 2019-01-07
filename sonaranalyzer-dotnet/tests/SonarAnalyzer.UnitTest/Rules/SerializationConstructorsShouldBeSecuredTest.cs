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
using csharp::SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class SerializationConstructorsShouldBeSecuredTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void SerializationConstructorsShouldBeSecured()
        {
            Verifier.VerifyAnalyzer(@"TestCases\SerializationConstructorsShouldBeSecured.cs",
                new SerializationConstructorsShouldBeSecured());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void SerializationConstructorsShouldBeSecured_InvalidCode()
        {
            Verifier.VerifyCSharpAnalyzer(@"
[Serializable]
    public partial class InvalidCode : ISerializable
    {
        [FileIOPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        [ZoneIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
        public InvalidCode() { }

        protected (SerializationInfo info, StreamingContext context) { }

        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }", new SerializationConstructorsShouldBeSecured(), checkMode: CompilationErrorBehavior.Ignore);
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void SerializationConstructorsShouldBeSecured_NoAssemblyAttribute()
        {
            Verifier.VerifyAnalyzer(@"TestCases\SerializationConstructorsShouldBeSecured_NoAssemblyAttribute.cs",
                new SerializationConstructorsShouldBeSecured());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void SerializationConstructorsShouldBeSecured_PartialClasses()
        {
            Verifier.VerifyAnalyzer(
                new[]
                {
                    @"TestCases\SerializationConstructorsShouldBeSecured_Part1.cs",
                    @"TestCases\SerializationConstructorsShouldBeSecured_Part2.cs",
                },
                new SerializationConstructorsShouldBeSecured());
        }
    }
}

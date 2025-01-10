/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

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
        builder.AddPaths("SerializationConstructorsShouldBeSecured.CSharp9.cs").WithConcurrentAnalysis(false).WithOptions(LanguageOptions.FromCSharp9).Verify();

#endif

    [TestMethod]
    public void SerializationConstructorsShouldBeSecured_InvalidCode() =>
        builder.AddSnippet("""
            [Serializable]
                public partial class InvalidCode : ISerializable
                {
                    [FileIOPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
                    [ZoneIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
                    public InvalidCode() { }

                    protected (SerializationInfo info, StreamingContext context) { }

                    public void GetObjectData(SerializationInfo info, StreamingContext context) { }
                }
            """).VerifyNoIssuesIgnoreErrors();

    [TestMethod]
    public void SerializationConstructorsShouldBeSecured_NoAssemblyAttribute() =>
        builder.AddPaths("SerializationConstructorsShouldBeSecured_NoAssemblyAttribute.cs").VerifyNoIssues();

    [TestMethod]
    public void SerializationConstructorsShouldBeSecured_PartialClasses() =>
        builder.AddPaths("SerializationConstructorsShouldBeSecured_Part1.cs", "SerializationConstructorsShouldBeSecured_Part2.cs").WithConcurrentAnalysis(false).Verify();
}

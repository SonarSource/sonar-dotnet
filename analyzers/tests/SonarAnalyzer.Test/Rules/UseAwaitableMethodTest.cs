/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class UseAwaitableMethodTest
{
    private const string EntityFrameworkVersion = "7.0.18";

    private readonly VerifierBuilder builder = new VerifierBuilder<UseAwaitableMethod>();

    [TestMethod]
    public void UseAwaitableMethod_CS() =>
        builder
            .AddReferences(MetadataReferenceFacade.SystemXml)
            .AddPaths("UseAwaitableMethod.cs")
            .Verify();

    [TestMethod]
    public void UseAwaitableMethod_Moq() =>
        builder.AddReferences(NuGetMetadataReference.Moq(TestConstants.NuGetLatestVersion)).AddPaths("UseAwaitableMethod.Moq.cs").Verify();

    [TestMethod]
    public void UseAwaitableMethod_Sockets() =>
        builder
            .AddReferences(MetadataReferenceFacade.SystemNetPrimitives)
            .AddReferences(MetadataReferenceFacade.SystemNetSockets)
            .AddPaths("UseAwaitableMethod_Sockets.cs")
            .Verify();

    // Starting from FluentValidation 12, the library support only net8 and newer
    [TestMethod]
    public void UseAwaitableMethod_FluentValidation11() =>
        builder
            .AddReferences(NuGetMetadataReference.FluentValidation("11.11.0"))
            .AddPaths("UseAwaitableMethod_FluentValidation.cs")
            .Verify();

#if NET
    [TestMethod]
    public void UseAwaitableMethod_CSharp9() =>
        builder
            .WithTopLevelStatements()
            .AddPaths("UseAwaitableMethod_CSharp9.cs")
            .Verify();

    [TestMethod]
    public void UseAwaitableMethod_CSharp8() =>
        builder
            .WithOptions(LanguageOptions.FromCSharp8)
            .AddPaths("UseAwaitableMethod_CSharp8.cs")
            .Verify();

    [TestMethod]
    public void UseAwaitableMethod_EF() =>
        builder
            .WithOptions(LanguageOptions.FromCSharp11)
            .AddReferences([CoreMetadataReference.SystemComponentModelTypeConverter, CoreMetadataReference.SystemDataCommon])
            .AddReferences(NuGetMetadataReference.MicrosoftEntityFrameworkCore(EntityFrameworkVersion))
            .AddReferences(NuGetMetadataReference.MicrosoftEntityFrameworkCoreRelational(EntityFrameworkVersion))
            .AddReferences(NuGetMetadataReference.MicrosoftEntityFrameworkCoreSqlServer(EntityFrameworkVersion))
            .AddPaths("UseAwaitableMethod_EF.cs")
            .Verify();

    [TestMethod]
    public void UseAwaitableMethod_MongoDb() =>
        builder
            .WithOptions(LanguageOptions.FromCSharp11)
            .AddReferences(NuGetMetadataReference.MongoDBDriver())
            .AddPaths("UseAwaitableMethod_MongoDBDriver.cs")
            .VerifyNoIssues();

    [TestMethod]
    public void UseAwaitableMethod_FluentValidationLatest() =>
        builder
            .AddReferences(NuGetMetadataReference.FluentValidation())
            .AddPaths("UseAwaitableMethod_FluentValidation.cs")
            .Verify();
#endif
}

/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class UseAwaitableMethodTest
{
    private const string EntityFrameworkVersion = "7.0.18";

    private readonly VerifierBuilder builder = new VerifierBuilder<UseAwaitableMethod>();

    [TestMethod]
    public void UseAwaitableMethod_CS() =>
        builder.AddPaths("UseAwaitableMethod.cs").AddReferences(MetadataReferenceFacade.SystemXml).Verify();

    [TestMethod]
    public void UseAwaitableMethod_Moq() =>
        builder.AddPaths("UseAwaitableMethod.Moq.cs").AddReferences(NuGetMetadataReference.Moq(TestConstants.NuGetLatestVersion)).Verify();

    [TestMethod]
    public void UseAwaitableMethod_Sockets() =>
        builder.AddPaths("UseAwaitableMethod_Sockets.cs").AddReferences(MetadataReferenceFacade.SystemNetPrimitives).AddReferences(MetadataReferenceFacade.SystemNetSockets).Verify();

    [TestMethod]
    public void UseAwaitableMethod_CS_TopLevelStatements() =>
        builder.AddPaths("UseAwaitableMethod.TopLevelStatements.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void UseAwaitableMethod_CS_Latest() =>
        builder.AddPaths("UseAwaitableMethod.Latest.cs").WithOptions(LanguageOptions.CSharpLatest).Verify();

#if NET
    [TestMethod]
    public void UseAwaitableMethod_EF() =>
        builder
            .WithOptions(LanguageOptions.CSharpLatest)
            .AddReferences([CoreMetadataReference.SystemComponentModelTypeConverter, CoreMetadataReference.SystemDataCommon])
            .AddReferences(NuGetMetadataReference.MicrosoftEntityFrameworkCore(EntityFrameworkVersion))
            .AddReferences(NuGetMetadataReference.MicrosoftEntityFrameworkCoreRelational(EntityFrameworkVersion))
            .AddReferences(NuGetMetadataReference.MicrosoftEntityFrameworkCoreSqlServer(EntityFrameworkVersion))
            .AddPaths("UseAwaitableMethod_EF.cs")
            .Verify();
#endif

    [TestMethod]
    public void UseAwaitableMethod_MongoDb() =>
        builder.AddPaths("UseAwaitableMethod_MongoDBDriver.cs").AddReferences(NuGetMetadataReference.MongoDBDriver()).WithOptions(LanguageOptions.CSharpLatest).VerifyNoIssues();

    // Starting from FluentValidation 12, the library support only net8 and newer
    [TestMethod]
    public void UseAwaitableMethod_FluentValidation11() =>
        builder.AddPaths("UseAwaitableMethod_FluentValidation.cs").AddReferences(NuGetMetadataReference.FluentValidation("11.11.0")).Verify();

    [TestMethod]
    public void UseAwaitableMethod_FluentValidationLatest() =>
        builder.AddPaths("UseAwaitableMethod_FluentValidation.cs").AddReferences(NuGetMetadataReference.FluentValidation()).WithNetOnly().Verify();
}

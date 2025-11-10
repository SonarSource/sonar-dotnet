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

namespace SonarAnalyzer.Test.Rules.CloudNative
{
    [TestClass]
    public class AzureFunctionsReuseClientsTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<AzureFunctionsReuseClients>()
            .WithBasePath("CloudNative")
            .AddReferences(NuGetMetadataReference.MicrosoftNetSdkFunctions())
            .AddReferences(MetadataReferenceFacade.SystemThreadingTasks)
            .AddReferences(NuGetMetadataReference.SystemNetHttp())
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsHttp());

        [TestMethod]
        public void AzureFunctionsReuseClients_HttpClient_CS() =>
            builder.AddPaths("AzureFunctionsReuseClients.HttpClient.cs").Verify();

        [TestMethod]
        public void AzureFunctionsReuseClients_HttpClient_CSharp9() =>
            builder.AddPaths("AzureFunctionsReuseClients.HttpClient.CSharp9.cs").WithOptions(LanguageOptions.FromCSharp9).Verify();

        [TestMethod]
        public void AzureFunctionsReuseClients_DocumentClient_CS() =>
            builder.AddPaths("AzureFunctionsReuseClients.DocumentClient.cs")
                .AddReferences(NuGetMetadataReference.MicrosoftAzureDocumentDB())
                .Verify();

        [TestMethod]
        public void AzureFunctionsReuseClients_CosmosClient_CS() =>
            builder.AddPaths("AzureFunctionsReuseClients.CosmosClient.cs")
                .AddReferences(NuGetMetadataReference.MicrosoftAzureCosmos())
                .Verify();

        [TestMethod]
        public void AzureFunctionsReuseClients_ServiceBusV5_CS() =>
            builder.AddPaths("AzureFunctionsReuseClients.ServiceBusV5.cs")
                .AddReferences(NuGetMetadataReference.MicrosoftAzureServiceBus())
                .Verify();

        [TestMethod]
        public void AzureFunctionsReuseClients_ServiceBusV7_CS() =>
            builder.AddPaths("AzureFunctionsReuseClients.ServiceBusV7.cs")
                .AddReferences(NuGetMetadataReference.AzureMessagingServiceBus())
                .Verify();

        [TestMethod]
        public void AzureFunctionsReuseClients_Storage_CS() =>
            builder.AddPaths("AzureFunctionsReuseClients.Storage.cs")
                .AddReferences(NuGetMetadataReference.AzureCore())
                .AddReferences(NuGetMetadataReference.AzureStorageCommon())
                .AddReferences(NuGetMetadataReference.AzureStorageBlobs())
                .AddReferences(NuGetMetadataReference.AzureStorageQueues())
                .AddReferences(NuGetMetadataReference.AzureStorageFilesShares())
                .AddReferences(NuGetMetadataReference.AzureStorageFilesDataLake())
                .Verify();

        [TestMethod]
        public void AzureFunctionsReuseClients_ArmClient_CS() =>
            builder.AddPaths("AzureFunctionsReuseClients.ArmClient.cs")
                .AddReferences(NuGetMetadataReference.AzureCore())
                .AddReferences(NuGetMetadataReference.AzureIdentity())
                .AddReferences(NuGetMetadataReference.AzureResourceManager())
                .Verify();
    }
}

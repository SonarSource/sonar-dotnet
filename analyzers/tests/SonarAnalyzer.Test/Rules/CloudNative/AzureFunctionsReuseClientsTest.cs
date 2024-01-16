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

using SonarAnalyzer.Rules.CSharp;

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
            builder.AddPaths("AzureFunctionsReuseClients.HttpClient.CSharp9.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

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

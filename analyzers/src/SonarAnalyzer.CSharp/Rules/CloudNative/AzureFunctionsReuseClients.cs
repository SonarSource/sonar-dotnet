﻿/*
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AzureFunctionsReuseClients : ReuseClientBase
    {
        private const string DiagnosticId = "S6420";
        private const string MessageFormat = "Reuse client instances rather than creating new ones with each function invocation.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override ImmutableArray<KnownType> ReusableClients => ImmutableArray.Create(
                KnownType.System_Net_Http_HttpClient,
                // ComosDb (DocumentClient is superseded by CosmosClient)
                KnownType.Microsoft_Azure_Documents_Client_DocumentClient,
                KnownType.Microsoft_Azure_Cosmos_CosmosClient,
                // Servicebus V5
                KnownType.Microsoft_Azure_ServiceBus_Management_ManagementClient,
                KnownType.Microsoft_Azure_ServiceBus_QueueClient,
                KnownType.Microsoft_Azure_ServiceBus_SessionClient,
                KnownType.Microsoft_Azure_ServiceBus_SubscriptionClient,
                KnownType.Microsoft_Azure_ServiceBus_TopicClient,
                // Servicebus V7
                KnownType.Azure_Messaging_ServiceBus_ServiceBusClient,
                KnownType.Azure_Messaging_ServiceBus_Administration_ServiceBusAdministrationClient,
                // Storage
                KnownType.Azure_Storage_Blobs_BlobServiceClient,
                KnownType.Azure_Storage_Queues_QueueServiceClient,
                KnownType.Azure_Storage_Files_Shares_ShareServiceClient,
                KnownType.Azure_Storage_Files_DataLake_DataLakeServiceClient,
                // Resource manager
                KnownType.Azure_ResourceManager_ArmClient);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
            {
                if (c.AzureFunctionMethod() is not null
                    && IsReusableClient(c)
                    && !IsAssignedForReuse(c))
                {
                    c.ReportIssue(Rule, c.Node);
                }
            },
            SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression);
    }
}

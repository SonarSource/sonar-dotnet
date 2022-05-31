﻿/*
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

using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Wrappers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AzureFunctionsReuseClients : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S6420";
        private const string MessageFormat = "Reuse client instances rather than creating new ones with each function invocation.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        private static readonly ImmutableArray<KnownType> Clients = ImmutableArray.Create(
            KnownType.System_Net_Http_HttpClient,
            KnownType.Microsoft_Azure_Documents_Client_DocumentClient,
            KnownType.Microsoft_Azure_Cosmos_CosmosClient,
            KnownType.Microsoft_Azure_ServiceBus_Management_ManagementClient,
            KnownType.Microsoft_Azure_ServiceBus_QueueClient,
            KnownType.Microsoft_Azure_ServiceBus_SessionClient,
            KnownType.Microsoft_Azure_ServiceBus_SubscriptionClient,
            KnownType.Microsoft_Azure_ServiceBus_TopicClient,
            KnownType.Azure_Messaging_ServiceBus_ServiceBusClient,
            KnownType.Azure_Messaging_ServiceBus_Administration_ServiceBusAdministrationClient,
            KnownType.Azure_Storage_Blobs_BlobServiceClient,
            KnownType.Azure_Storage_Queues_QueueServiceClient,
            KnownType.Azure_Storage_Files_Shares_ShareServiceClient,
            KnownType.Azure_Storage_Files_DataLake_DataLakeServiceClient,
            KnownType.Azure_ResourceManager_ArmClient);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                if (c.AzureFunctionMethod() is not null)
                {
                    var node = c.Node;
                    var semanticModel = c.SemanticModel;
                    if (CreatedResuableClient(semanticModel, node) is { } knownType
                        && !IsAssignedForReuse(semanticModel, node, c.CancellationToken))
                    {
                        c.ReportIssue(Diagnostic.Create(Rule, node.GetLocation()));
                    }
                }
            },
            SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression);

        private static bool IsAssignedForReuse(SemanticModel model, SyntaxNode node, CancellationToken cancellationToken)
        {
            if (IsAssignedToLocal(node))
            {
                return false;
            }
            if (IsInPropertyInitializer(node)
                || IsInFieldInitializer(node)
                || IsAssignedToFieldOrProperty(model, node, cancellationToken))
            {
                return true;
            }

            return false;
        }
        private static bool IsAssignedToLocal(SyntaxNode node) =>
            node.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: LocalDeclarationStatementSyntax or UsingStatementSyntax } } };

        private static bool IsInFieldInitializer(SyntaxNode node) =>
            node.Ancestors().Any(x => x.IsKind(SyntaxKind.FieldDeclaration));

        private static bool IsInPropertyInitializer(SyntaxNode node) =>
            node.Parent is EqualsValueClauseSyntax { Parent: PropertyDeclarationSyntax };

        private static bool IsAssignedToFieldOrProperty(SemanticModel model, SyntaxNode node, CancellationToken cancellationToken) =>
            node.Parent is AssignmentExpressionSyntax assignment
                && assignment.Left is { } identifier
                && model.GetSymbolInfo(identifier, cancellationToken).Symbol is { } symbol
                && symbol.Kind is SymbolKind.Field or SymbolKind.Property;

        private static KnownType CreatedResuableClient(SemanticModel model, SyntaxNode node)
        {
            var objectCreation = ObjectCreationFactory.Create(node);
            return Clients.FirstOrDefault(x => objectCreation.IsKnownType(x, model));
        }
    }
}

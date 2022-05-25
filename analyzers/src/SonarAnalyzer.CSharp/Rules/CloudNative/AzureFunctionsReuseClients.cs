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

using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;
using SonarAnalyzer.Helpers;
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
            KnownType.Microsoft_Azure_ServiceBus_QueueClient,
            KnownType.StackExchange_Redis_ConnectionMultiplexer);
        // ToDo: Append the list of (usefull) Azure Storage clients (there are dozens).

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var node = c.Node;
                    if (CreatedResuableClient(c.SemanticModel, node, c.CancellationToken) is { } knownType)
                    {
                        if (IsNotAssignedForReuse(c.SemanticModel, node, c.CancellationToken))
                        {
                            c.ReportIssue(Diagnostic.Create(Rule, node.GetLocation()));
                        }
                    }
                },
                SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression);

        private static bool IsNotAssignedForReuse(SemanticModel model, SyntaxNode node, CancellationToken cancellationToken)
        {
            if (IsAssignedToLocal(node)
                || IsUnAssigned(node))
            {
                return true;
            }

            return false;
        }

        private static bool IsUnAssigned(SyntaxNode node) =>
            node.Ancestors().Any(x => x.IsKind(SyntaxKind.ExpressionStatement));

        private static bool IsAssignedToLocal(SyntaxNode node) =>
            node.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: LocalDeclarationStatementSyntax or UsingStatementSyntax } } };

        private static ITypeSymbol CreatedResuableClient(SemanticModel model, SyntaxNode node, CancellationToken cancellationToken) =>
            node is ObjectCreationExpressionSyntax objectCreationExpression
            && objectCreationExpression.Type is NameSyntax name
            && name.GetIdentifier()?.Identifier.Text is { } typeName
            && Clients.FirstOrDefault(x => x.ShortName == typeName) is { } knownResuableClient
            && model.GetSymbolInfo(name, cancellationToken).Symbol is ITypeSymbol typeSymbol
            && typeSymbol.Is(knownResuableClient)
                ? typeSymbol
                : null;
    }
}

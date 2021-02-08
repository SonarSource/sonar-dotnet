/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class WeakSslTlsProtocolsBase<TSyntaxKind, TIdentifierNameSyntax> : SonarDiagnosticAnalyzer
        where TIdentifierNameSyntax : SyntaxNode
        where TSyntaxKind : struct
    {
        private static bool IsSecurityProtocolType(ITypeSymbol typeSymbol) =>
            typeSymbol.IsAny(KnownType.System_Net_SecurityProtocolType, KnownType.System_Security_Authentication_SslProtocols);

        protected const string DiagnosticId = "S4423";

        protected const string MessageFormat = "Change this code to use a stronger protocol.";

        private readonly HashSet<string> weakProtocols = new HashSet<string>
        {
            "Ssl2",
            "Ssl3",
            "Tls",
            "Tls11",
            "Default",
        };

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }

        protected abstract TSyntaxKind SyntaxKind { get; }

        protected abstract DiagnosticDescriptor Rule { get; }

        protected abstract string GetIdentifierText(TIdentifierNameSyntax identifierNameSyntax);

        protected abstract bool IsPartOfBinaryNegationOrCondition(SyntaxNode node);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(GeneratedCodeRecognizer, AnalyzeSyntax, SyntaxKind);

        private void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            var node = context.Node;
            if (!IsPartOfBinaryNegationOrCondition(node) && IsWeakProtocolUsed((TIdentifierNameSyntax)node, context.SemanticModel))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, node.GetLocation()));
            }
        }

        private bool IsWeakProtocolUsed(TIdentifierNameSyntax identifierNameSyntax, SemanticModel semanticModel) =>
            weakProtocols.Contains(GetIdentifierText(identifierNameSyntax))
            && IsSecurityProtocolType(semanticModel.GetTypeInfo(identifierNameSyntax).Type);
    }
}

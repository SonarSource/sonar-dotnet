/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class WeakSslTlsProtocols : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4423";
        private const string MessageFormat = "Change this code to use a stronger protocol.";

        private static readonly DiagnosticDescriptor Rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private readonly HashSet<string> weakProtocols = new HashSet<string>
        {
            "Ssl2",
            "Ssl3",
            "Tls",
            "Tls11",
            "Default",
        };

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var node = c.Node;
                var semanticModel = c.SemanticModel;

                if (node is IdentifierNameSyntax identifierNameSyntax
                && IsWeakProtocol(identifierNameSyntax, semanticModel))
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, identifierNameSyntax.GetLocation()));
                }
            },
            SyntaxKind.IdentifierName);

        private bool IsWeakProtocol(IdentifierNameSyntax identifierNameSyntax, SemanticModel semanticModel) =>
                    weakProtocols.Contains(identifierNameSyntax.Identifier.Text)
                    && (semanticModel.GetTypeInfo(identifierNameSyntax).Type.Is(KnownType.System_Net_SecurityProtocolType)
                    || semanticModel.GetTypeInfo(identifierNameSyntax).Type.Is(KnownType.System_Security_Authentication_SslProtocols));
    }
}

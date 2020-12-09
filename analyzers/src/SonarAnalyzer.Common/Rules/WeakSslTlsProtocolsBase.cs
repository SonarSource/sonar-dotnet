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

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class WeakSslTlsProtocolsBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S4423";

        protected const string MessageFormat = "Change this code to use a stronger protocol.";

        protected HashSet<string> WeakProtocols { get; } = new HashSet<string>
        {
            "Ssl2",
            "Ssl3",
            "Tls",
            "Tls11",
            "Default",
        };

        protected Action<SyntaxNodeAnalysisContext> GetAnalysisAction(DiagnosticDescriptor rule) =>
            c =>
            {
                var node = c.Node;

                if (IsWeakProtocolUsed(node, c.SemanticModel))
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, node.GetLocation()));
                }
            };

        protected abstract bool IsWeakProtocolUsed(SyntaxNode node, SemanticModel semanticModel);

        protected bool IsSecurityProtocolType(ITypeSymbol typeSymbol) =>
            typeSymbol.IsAny(KnownType.System_Net_SecurityProtocolType, KnownType.System_Security_Authentication_SslProtocols);
    }
}

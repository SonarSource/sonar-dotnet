/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules
{
    public abstract class WeakSslTlsProtocolsBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        private const string DiagnosticId = "S4423";

        private readonly HashSet<string> weakProtocols = new()
        {
            "Ssl2",
            "Ssl3",
            "Tls",
            "Tls11",
            "Default",
        };

        protected abstract bool IsPartOfBinaryNegationOrCondition(SyntaxNode node);

        protected override string MessageFormat => "Change this code to use a stronger protocol.";

        protected WeakSslTlsProtocolsBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    var node = c.Node;
                    if (!IsPartOfBinaryNegationOrCondition(node) && IsWeakProtocol(node, c.SemanticModel))
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, node.GetLocation()));
                    }
                },
                Language.SyntaxKind.IdentifierName);

        private bool IsWeakProtocol(SyntaxNode identifierName, SemanticModel semanticModel) =>
            weakProtocols.Contains(Language.Syntax.NodeIdentifier(identifierName).Value.ValueText)
            && semanticModel.GetTypeInfo(identifierName).Type.IsAny(KnownType.System_Net_SecurityProtocolType, KnownType.System_Security_Authentication_SslProtocols);
    }
}

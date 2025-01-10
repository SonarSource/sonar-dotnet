/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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
                        c.ReportIssue(Rule, node);
                    }
                },
                Language.SyntaxKind.IdentifierName);

        private bool IsWeakProtocol(SyntaxNode identifierName, SemanticModel semanticModel) =>
            weakProtocols.Contains(Language.Syntax.NodeIdentifier(identifierName).Value.ValueText)
            && semanticModel.GetTypeInfo(identifierName).Type.IsAny(KnownType.System_Net_SecurityProtocolType, KnownType.System_Security_Authentication_SslProtocols);
    }
}

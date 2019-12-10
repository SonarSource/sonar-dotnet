/*
 * Copyright (C) 2018-2019 SonarSource SA
 * All rights reserved
 * mailto:info AT sonarsource DOT com
 */

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    public class UsingEndBlock : SimpleBlock
    {
        public UsingStatementSyntax UsingStatement { get; }
        public IEnumerable<SyntaxToken> Identifiers { get; }

        public UsingEndBlock(UsingStatementSyntax usingStatement, Block successor)
            : base(successor)
        {
            UsingStatement = usingStatement;

            Identifiers = usingStatement.Declaration != null
                ? GetIdentifiers(usingStatement.Declaration)
                : GetIdentifiers(usingStatement.Expression);
        }

        private static IEnumerable<SyntaxToken> GetIdentifiers(VariableDeclarationSyntax declaration)
        {
            return declaration.Variables
                .Select(v => v.Identifier)
                .ToImmutableArray();
        }

        private static IEnumerable<SyntaxToken> GetIdentifiers(ExpressionSyntax expression)
        {
            return expression.RemoveParentheses() is IdentifierNameSyntax identifier ? ImmutableArray.Create(identifier.Identifier)
                : expression.DescendantNodesAndSelf()
                    .OfType<AssignmentExpressionSyntax>()
                    .Select(a => a.Left)
                    .OfType<IdentifierNameSyntax>()
                    .Select(i => i.Identifier)
                    .ToImmutableArray();
        }
    }
}

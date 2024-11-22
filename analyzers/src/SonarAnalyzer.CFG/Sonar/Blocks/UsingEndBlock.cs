/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.CFG.Helpers;

namespace SonarAnalyzer.CFG.Sonar
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

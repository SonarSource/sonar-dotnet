/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class UnaryPrefixOperatorRepeatedBase<TSyntaxKindEnum, TSyntaxNode> : SonarDiagnosticAnalyzer
        where TSyntaxNode : SyntaxNode
        where TSyntaxKindEnum : struct
    {
        internal const string DiagnosticId = "S2761";
        protected const string MessageFormat = "Use the '{0}' operator just once or not at all.";

        protected abstract DiagnosticDescriptor Rule { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected abstract ISet<TSyntaxKindEnum> SyntaxKinds { get; }

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                GeneratedCodeRecognizer,
                c =>
                {
                    var topLevelUnary = (TSyntaxNode)c.Node;

                    if (!TopLevelUnaryInChain(topLevelUnary))
                    {
                        return;
                    }

                    var repeatedCount = 0U;
                    var currentUnary = topLevelUnary;
                    var lastUnary = currentUnary;
                    while (currentUnary != null &&
                           SameOperators(currentUnary, topLevelUnary))
                    {
                        lastUnary = currentUnary;
                        repeatedCount++;
                        currentUnary = GetOperand(currentUnary) as TSyntaxNode;
                    }

                    if (repeatedCount < 2)
                    {
                        return;
                    }

                    c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, topLevelUnary.CreateLocation(GetOperatorToken(lastUnary)),
                        GetOperatorToken(topLevelUnary).ToString()));
                }, SyntaxKinds.ToArray());
        }

        private bool TopLevelUnaryInChain(TSyntaxNode unary) =>
            !(unary.Parent is TSyntaxNode parent) || !SameOperators(parent, unary);

        protected abstract SyntaxNode GetOperand(TSyntaxNode unarySyntax);

        protected abstract SyntaxToken GetOperatorToken(TSyntaxNode unarySyntax);

        protected abstract bool SameOperators(TSyntaxNode expression1, TSyntaxNode expression2);
    }
}

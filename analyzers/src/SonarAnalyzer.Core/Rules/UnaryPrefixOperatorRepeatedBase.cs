/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Rules
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
            context.RegisterNodeAction(
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

                    c.ReportIssue(Rule, topLevelUnary.CreateLocation(GetOperatorToken(lastUnary)), GetOperatorToken(topLevelUnary).ValueText);
                }, SyntaxKinds.ToArray());
        }

        private bool TopLevelUnaryInChain(TSyntaxNode unary) =>
            !(unary.Parent is TSyntaxNode parent) || !SameOperators(parent, unary);

        protected abstract SyntaxNode GetOperand(TSyntaxNode unarySyntax);

        protected abstract SyntaxToken GetOperatorToken(TSyntaxNode unarySyntax);

        protected abstract bool SameOperators(TSyntaxNode expression1, TSyntaxNode expression2);
    }
}

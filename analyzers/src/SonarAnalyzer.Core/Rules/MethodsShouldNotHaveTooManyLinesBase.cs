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

namespace SonarAnalyzer.Core.Rules
{
    public abstract class MethodsShouldNotHaveTooManyLinesBase<TSyntaxKind, TBaseMethodSyntax>
        : ParametrizedDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TBaseMethodSyntax : SyntaxNode
    {
        internal const string DiagnosticId = "S138";
        protected const string MessageFormat = "This {0} has {1} lines, which is greater than the {2} lines authorized. Split it into smaller {3}.";

        private const int DefaultMaxMethodLines = 80;
        [RuleParameter("max", PropertyType.Integer, "Maximum authorized lines of code in a method", DefaultMaxMethodLines)]
        public int Max { get; set; } = DefaultMaxMethodLines;

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
        protected abstract TSyntaxKind[] SyntaxKinds { get; }
        protected abstract string MethodKeyword { get; }

        protected abstract IEnumerable<SyntaxToken> GetMethodTokens(TBaseMethodSyntax baseMethodDeclaration);
        protected abstract SyntaxToken? GetMethodIdentifierToken(TBaseMethodSyntax baseMethodDeclaration);
        protected abstract string GetMethodKindAndName(SyntaxToken identifierToken);

        protected override void Initialize(SonarParametrizedAnalysisContext context) =>
            context.RegisterNodeAction(
                GeneratedCodeRecognizer,
                c =>
                {
                    if (Max < 2)
                    {
                        throw new InvalidOperationException($"Invalid rule parameter: maximum number of lines = {Max}. Must be at least 2.");
                    }

                    var baseMethod = (TBaseMethodSyntax)c.Node;
                    var linesCount = GetMethodTokens(baseMethod).SelectMany(token => token.LineNumbers())
                                                                .Distinct()
                                                                .LongCount();

                    if (linesCount > Max)
                    {
                        var identifierToken = GetMethodIdentifierToken(baseMethod);

                        if (!string.IsNullOrEmpty(identifierToken?.ValueText))
                        {
                            c.ReportIssue(
                                SupportedDiagnostics[0],
                                identifierToken.Value,
                                GetMethodKindAndName(identifierToken.Value),
                                linesCount.ToString(),
                                Max.ToString(),
                                MethodKeyword);
                        }
                    }
                },
                SyntaxKinds);
    }
}

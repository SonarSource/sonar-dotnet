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

using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonarAnalyzer.Rules
{
    public abstract class MethodsShouldNotHaveTooManyLinesBase<TSyntaxKind, TBaseMethodSyntax>
        : ParameterLoadingDiagnosticAnalyzer
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

        protected sealed override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                GeneratedCodeRecognizer,
                c =>
                {
                    if (Max < 2)
                    {
                        throw new InvalidOperationException(
                            $"Invalid rule parameter: maximum number of lines = {Max}. Must be at least 2.");
                    }

                    var baseMethod = (TBaseMethodSyntax)c.Node;

                    var linesCount = GetMethodTokens(baseMethod)
                        .SelectMany(token => token.GetLineNumbers())
                        .Distinct()
                        .LongCount();

                    if (linesCount > Max)
                    {
                        var identifierToken = GetMethodIdentifierToken(baseMethod);

                        if (!string.IsNullOrEmpty(identifierToken?.ValueText))
                        {
                            c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], identifierToken.Value.GetLocation(),
                                GetMethodKindAndName(identifierToken.Value), linesCount, Max, MethodKeyword));
                        }
                    }
                },
                SyntaxKinds);
        }
    }
}


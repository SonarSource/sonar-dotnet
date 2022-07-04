/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class StreamReadStatement : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2674";
        private const string MessageFormat = "Check the return value of the '{0}' call to see how many bytes were read.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        private static readonly ISet<string> ReadMethodNames = new HashSet<string> { nameof(Stream.Read), nameof(Stream.ReadAsync) };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var statement = (ExpressionStatementSyntax)c.Node;
                    var expression = statement is { Expression: AwaitExpressionSyntax awaitExpression }
                        ? awaitExpression.Expression
                        : statement.Expression;

                    if (expression is InvocationExpressionSyntax invocation
                        && invocation.GetMethodCallIdentifier() is { } methodIdentifier
                        && ReadMethodNames.Contains(methodIdentifier.Text, StringComparer.Ordinal)
                        && c.SemanticModel.GetSymbolInfo(expression).Symbol is IMethodSymbol method
                        && (method.ContainingType.Is(KnownType.System_IO_Stream)
                            || (method.IsOverride && method.ContainingType.DerivesOrImplements(KnownType.System_IO_Stream))))
                    {
                        c.ReportIssue(Diagnostic.Create(Rule, expression.GetLocation(), method.Name));
                    }
                },
                SyntaxKind.ExpressionStatement);
    }
}

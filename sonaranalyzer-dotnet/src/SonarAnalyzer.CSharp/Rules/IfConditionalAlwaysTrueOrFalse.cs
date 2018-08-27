/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class IfConditionalAlwaysTrueOrFalse : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1145";
        private const string MessageFormat = "Remove this useless {0}.";
        private const string ifStatementLiteral = "'if' statement";
        private const string elseClauseLiteral = "'else' clause";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var ifNode = (IfStatementSyntax)c.Node;

                    var isTrue = ifNode.Condition.IsKind(SyntaxKind.TrueLiteralExpression);
                    var isFalse = ifNode.Condition.IsKind(SyntaxKind.FalseLiteralExpression);

                    if (!isTrue && !isFalse)
                    {
                        return;
                    }

                    if (isTrue)
                    {
                        ReportIfTrue(ifNode, c);
                    }
                    else
                    {
                        ReportIfFalse(ifNode, c);
                    }
                },
                SyntaxKind.IfStatement);
        }

        private static void ReportIfFalse(IfStatementSyntax ifStatement, SyntaxNodeAnalysisContext context)
        {
            var location = ifStatement.Else == null
                ? ifStatement.GetLocation()
                : Location.Create(
                    ifStatement.SyntaxTree,
                    new TextSpan(ifStatement.IfKeyword.SpanStart, ifStatement.Else.ElseKeyword.Span.End - ifStatement.IfKeyword.SpanStart));

            context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location, ifStatementLiteral));
        }

        private static void ReportIfTrue(IfStatementSyntax ifStatement, SyntaxNodeAnalysisContext context)
        {
            var location = Location.Create(
                ifStatement.SyntaxTree,
                new TextSpan(ifStatement.IfKeyword.SpanStart, ifStatement.CloseParenToken.Span.End - ifStatement.IfKeyword.SpanStart));

            context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location, ifStatementLiteral));

            if (ifStatement.Else != null)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, ifStatement.Else.GetLocation(), elseClauseLiteral));
            }
        }
    }
}

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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class IfConditionalAlwaysTrueOrFalse : IfConditionalAlwaysTrueOrFalseBase<IfStatementSyntax>
    {
        private const string ifStatementLiteral = "'if' statement";
        private const string elseClauseLiteral = "'else' clause";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(TreatNode, SyntaxKind.IfStatement);

        protected override bool ConditionIsTrueLiteral(IfStatementSyntax ifSyntax) =>
            ifSyntax.Condition.IsKind(SyntaxKind.TrueLiteralExpression);

        protected override bool ConditionIsFalseLiteral(IfStatementSyntax ifSyntax) =>
            ifSyntax.Condition.IsKind(SyntaxKind.FalseLiteralExpression);

        protected override void ReportIfFalse(IfStatementSyntax ifSyntax, SyntaxNodeAnalysisContext context)
        {
            var location = ifSyntax.Else == null
                ? ifSyntax.GetLocation()
                : ifSyntax.IfKeyword.CreateLocation(ifSyntax.Else.ElseKeyword);

            context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location, ifStatementLiteral));
        }

        protected override void ReportIfTrue(IfStatementSyntax ifSyntax, SyntaxNodeAnalysisContext context)
        {
            var location = ifSyntax.IfKeyword.CreateLocation(ifSyntax.CloseParenToken);

            context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location, ifStatementLiteral));

            if (ifSyntax.Else != null)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, ifSyntax.Else.GetLocation(), elseClauseLiteral));
            }
        }
    }
}

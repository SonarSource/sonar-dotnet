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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class IfConditionalAlwaysTrueOrFalse : IfConditionalAlwaysTrueOrFalseBase<MultiLineIfBlockSyntax>
    {
        private const string ifStatementLiteral = "'If' statement";
        private const string elseIfClauseLiteral = "'ElseIf' clause";
        private const string elseClauseLiteral = "'Else' clause";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(TreatNode, SyntaxKind.MultiLineIfBlock);

        protected override bool ConditionIsTrueLiteral(MultiLineIfBlockSyntax ifSyntax) =>
            ifSyntax.IfStatement != null &&
            ifSyntax.IfStatement.Condition.IsKind(SyntaxKind.TrueLiteralExpression);

        protected override bool ConditionIsFalseLiteral(MultiLineIfBlockSyntax ifSyntax) =>
            ifSyntax.IfStatement != null &&
            ifSyntax.IfStatement.Condition.IsKind(SyntaxKind.FalseLiteralExpression);

        protected override void ReportIfFalse(MultiLineIfBlockSyntax ifSyntax, SyntaxNodeAnalysisContext context)
        {
            var location = ifSyntax.ElseBlock == null
                ? ifSyntax.IfStatement.GetLocation()
                : ifSyntax.IfStatement.IfKeyword.CreateLocation(ifSyntax.ElseBlock.ElseStatement.ElseKeyword);

            context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location, ifStatementLiteral));
        }

        protected override void ReportIfTrue(MultiLineIfBlockSyntax ifSyntax, SyntaxNodeAnalysisContext context)
        {
            var location = ifSyntax.IfStatement.IfKeyword.CreateLocation(ifSyntax.IfStatement);

            context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location, ifStatementLiteral));

            ifSyntax.ElseIfBlocks.ToList().ForEach(elseIfBlock =>
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, elseIfBlock.ElseIfStatement.GetLocation(), elseIfClauseLiteral))
            );

            if (ifSyntax.ElseBlock != null)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, ifSyntax.ElseBlock.ElseStatement.GetLocation(), elseClauseLiteral));
            }
        }
    }
}

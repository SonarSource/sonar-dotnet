/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DoNotCatchSystemException : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2221";
        private const string MessageFormat = "Catch a list of specific exception subtype or use exception filters instead.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var catchClause = (CatchClauseSyntax)c.Node;

                    if (IsSystemException(catchClause.Declaration, c.SemanticModel)
                        && IsCatchClauseEmptyOrNotPattern(catchClause)
                        && !IsThrowTheLastStatementInTheBlock(catchClause?.Block))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, GetLocation(catchClause)));
                    }
                },
                SyntaxKind.CatchClause);

        private static bool IsCatchClauseEmptyOrNotPattern(CatchClauseSyntax catchClause) =>
            catchClause.Filter?.FilterExpression == null
             || (catchClause.Filter.FilterExpression.IsKind(SyntaxKindEx.IsPatternExpression)
                 && (IsPatternExpressionSyntaxWrapper)catchClause.Filter.FilterExpression is var patternExpression
                 && patternExpression.SyntaxNode.DescendantNodes().AnyOfKind(SyntaxKindEx.NotPattern));

        private static bool IsSystemException(CatchDeclarationSyntax catchDeclaration, SemanticModel semanticModel)
        {
            var caughtTypeSyntax = catchDeclaration?.Type;
            return caughtTypeSyntax == null || semanticModel.GetTypeInfo(caughtTypeSyntax).Type.Is(KnownType.System_Exception);
        }

        private static bool IsThrowTheLastStatementInTheBlock(BlockSyntax block)
        {
            var lastStatement = block?.DescendantNodes()?.LastOrDefault();

            while (!Equals(lastStatement, block) && lastStatement != null)
            {
                if (lastStatement is ThrowStatementSyntax)
                {
                    return true;
                }

                lastStatement = lastStatement.Parent;
            }

            return false;
        }

        private static Location GetLocation(CatchClauseSyntax catchClause) =>
            catchClause.Declaration?.Type != null
                ? catchClause.Declaration.Type.GetLocation()
                : catchClause.CatchKeyword.GetLocation();
    }
}

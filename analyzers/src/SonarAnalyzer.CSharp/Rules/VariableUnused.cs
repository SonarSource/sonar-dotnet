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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class VariableUnused : VariableUnusedBase
    {
        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCodeBlockStartActionInNonGenerated<SyntaxKind>(cbc =>
            {
                var collector = new UnusedLocalsCollector();

                cbc.RegisterSyntaxNodeAction(collector.CollectDeclarations, SyntaxKind.LocalDeclarationStatement, SyntaxKind.SimpleAssignmentExpression);
                cbc.RegisterSyntaxNodeAction(collector.CollectUsages, SyntaxKind.IdentifierName);
                cbc.RegisterCodeBlockEndAction(collector.GetReportUnusedVariablesAction(Rule));
            });

        private sealed class UnusedLocalsCollector : UnusedLocalsCollectorBase<SyntaxNode>
        {
            protected override IEnumerable<SyntaxNode> GetDeclaredVariables(SyntaxNode variableDeclaration) =>
                variableDeclaration switch
                {
                    LocalDeclarationStatementSyntax localDeclaration when !localDeclaration.UsingKeyword().IsKind(SyntaxKind.UsingKeyword) =>
                        localDeclaration.Declaration.Variables,
                    AssignmentExpressionSyntax assignmentExpression =>
                        assignmentExpression.AssignmentTargets().Where(x => DeclarationExpressionSyntaxWrapper.IsInstance(x) || SingleVariableDesignationSyntaxWrapper.IsInstance(x)),
                    _ => Enumerable.Empty<SyntaxNode>(),
                };
        }
    }
}

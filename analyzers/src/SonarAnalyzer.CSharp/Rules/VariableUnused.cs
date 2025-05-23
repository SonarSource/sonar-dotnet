﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class VariableUnused : VariableUnusedBase
    {
        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCodeBlockStartAction(cbc =>
            {
                var collector = new UnusedLocalsCollector();

                cbc.RegisterNodeAction(collector.CollectDeclarations,
                    SyntaxKind.LocalDeclarationStatement,
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxKindEx.VarPattern,
                    SyntaxKindEx.RecursivePattern,
                    SyntaxKindEx.DeclarationPattern,
                    SyntaxKindEx.ListPattern);
                cbc.RegisterNodeAction(collector.CollectUsages, SyntaxKind.IdentifierName);
                cbc.RegisterCodeBlockEndAction(c => collector.ReportUnusedVariables(c, Rule));
            });

        private sealed class UnusedLocalsCollector : UnusedLocalsCollectorBase<SyntaxNode>
        {
            protected override IEnumerable<SyntaxNode> GetDeclaredVariables(SyntaxNode variableDeclaration) =>
                variableDeclaration switch
                {
                    LocalDeclarationStatementSyntax localDeclaration when !localDeclaration.UsingKeyword().IsKind(SyntaxKind.UsingKeyword) => localDeclaration.Declaration.Variables,
                    AssignmentExpressionSyntax assignmentExpression =>
                        assignmentExpression.AssignmentTargets().Where(x => DeclarationExpressionSyntaxWrapper.IsInstance(x) || SingleVariableDesignationSyntaxWrapper.IsInstance(x)),
                    { RawKind: (int)SyntaxKindEx.VarPattern } pattern when ((VarPatternSyntaxWrapper)pattern).Designation is { } designation => Variables(designation),
                    { RawKind: (int)SyntaxKindEx.RecursivePattern } pattern when ((RecursivePatternSyntaxWrapper)pattern).Designation is { } designation => Variables(designation),
                    { RawKind: (int)SyntaxKindEx.DeclarationPattern } pattern when ((DeclarationPatternSyntaxWrapper)pattern).Designation is { } designation => Variables(designation),
                    { RawKind: (int)SyntaxKindEx.ListPattern } pattern when ((ListPatternSyntaxWrapper)pattern).Designation is { } designation => Variables(designation),
                    _ => Enumerable.Empty<SyntaxNode>(),
                };

            private static IEnumerable<SyntaxNode> Variables(VariableDesignationSyntaxWrapper designation) =>
                designation.AllVariables().Select(v => v.SyntaxNode);
        }
    }
}

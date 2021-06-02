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

using System;
using System.Collections.Immutable;
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
    public sealed class UseReturnStatement : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S5944";
        private const string MessageFormat = "{0}";
        private const string UseReturnStatementMessage = "Use a 'Return' statement; assigning returned values to function names is obsolete.";
        private const string DontUseImplicitMessage = "Do not make use of the implicit return value.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var method = (MethodStatementSyntax)((MethodBlockSyntax)c.Node).BlockStatement;
                new IdentifierWalker(c, method.Identifier.ValueText).SafeVisit(c.Node);
            },
            SyntaxKind.FunctionBlock);

        private class IdentifierWalker : VisualBasicSyntaxWalker
        {
            private readonly SyntaxNodeAnalysisContext context;
            private readonly string name;

            public IdentifierWalker(SyntaxNodeAnalysisContext context, string name)
            {
                this.context = context;
                this.name = name;
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                if (IsImplicitReturnValue(node))
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, node.GetLocation(),
                        IsAssignmentStatement(node)
                        ? UseReturnStatementMessage
                        : DontUseImplicitMessage));
                }
            }

            public override void VisitImplementsClause(ImplementsClauseSyntax node) { /* Skip */ }

            public override void VisitAttributeList(AttributeListSyntax node) { /* Skip */ }

            private bool IsImplicitReturnValue(IdentifierNameSyntax node) =>
                name.Equals(node.Identifier.ValueText, StringComparison.InvariantCultureIgnoreCase)
                && !IsExcluded(node);

            private static bool IsExcluded(SyntaxNode node) =>
                node.Parent is InvocationExpressionSyntax
                || node.Parent is MemberAccessExpressionSyntax
                || node.Parent is QualifiedNameSyntax
                || node.Parent is NamedFieldInitializerSyntax
                || node.Parent is NameOfExpressionSyntax
                || node.Parent is AsClauseSyntax
                || node.Parent is ObjectCreationExpressionSyntax
                || (node.Parent is NameColonEqualsSyntax nameColon && nameColon.Name == node);

            private static bool IsAssignmentStatement(SyntaxNode node) =>
                node.Parent is AssignmentStatementSyntax assignement
                && assignement.Left == node;
        }
    }
}

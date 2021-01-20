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
using System.Collections.Generic;
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
        private const string MessageFormat = "Use a 'Return' statement; assigning returned values to function names is obsolete.";
        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var method = (MethodStatementSyntax)((MethodBlockSyntax)c.Node).BlockStatement;
                new Identifierswalker(c, method.Identifier.ValueText).SafeVisit(c.Node);
            },
            SyntaxKind.FunctionBlock);

        private class Identifierswalker : VisualBasicSyntaxWalker
        {
            private SyntaxNodeAnalysisContext Context { get; }
            private string Name { get; }

            public Identifierswalker(SyntaxNodeAnalysisContext context, string name)
            {
                Context = context;
                Name = name;
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                if (IsImplicitReturnValue(node))
                {
                    if (NoAssignment(node))
                    {
                        // TODO: find the best way to do this.
                        var rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, "Do not make use of the implicit return value.", RspecStrings.ResourceManager);
                        Context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, node.GetLocation()));
                    }
                    else
                    {
                        Context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, node.GetLocation()));
                    }
                }
            }

            private static bool IsExcluded(SyntaxNode node) =>
               node.Parent is InvocationExpressionSyntax
               || node.Parent is MemberAccessExpressionSyntax
               || node.Parent is NamedFieldInitializerSyntax;

            private bool IsImplicitReturnValue(IdentifierNameSyntax node) =>
                Name.Equals(node.Identifier.ValueText, StringComparison.InvariantCultureIgnoreCase)
                && !IsExcluded(node);

            private bool NoAssignment(SyntaxNode node) =>
                node.Parent is ReturnStatementSyntax
                || node.Parent is EqualsValueSyntax;
        }
    }
}

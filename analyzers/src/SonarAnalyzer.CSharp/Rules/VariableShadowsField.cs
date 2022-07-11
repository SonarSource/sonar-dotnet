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
    public sealed class VariableShadowsField : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S1117";
        private const string MessageFormat = "Rename '{0}' which hides the {1} with the same name.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var declaration = (ForEachStatementSyntax)c.Node;

                    var variableSymbol = c.SemanticModel.GetDeclaredSymbol(declaration);
                    if (variableSymbol == null)
                    {
                        return;
                    }

                    var members = GetMembers(variableSymbol.ContainingType);

                    ReportOnVariableMatchingField(c, members, declaration.Identifier);
                },
                SyntaxKind.ForEachStatement);
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var declaration = (ForEachVariableStatementSyntaxWrapper)c.Node;
                    var variable = declaration.Variable;
                    if (TupleElementSyntaxWrapper.IsInstance(variable))
                    {
                        ((TupleElementSyntaxWrapper)variable).AllElements();
                    }
                },
                SyntaxKindEx.ForEachVariableStatement);

            context.RegisterSyntaxNodeActionInNonGenerated(c => ProcessVariableDeclaration(c, ((LocalDeclarationStatementSyntax)c.Node).Declaration), SyntaxKind.LocalDeclarationStatement);
            context.RegisterSyntaxNodeActionInNonGenerated(c => ProcessVariableDeclaration(c, ((ForStatementSyntax)c.Node).Declaration), SyntaxKind.ForStatement);
            context.RegisterSyntaxNodeActionInNonGenerated(c => ProcessVariableDeclaration(c, ((UsingStatementSyntax)c.Node).Declaration), SyntaxKind.UsingStatement);
            context.RegisterSyntaxNodeActionInNonGenerated(c => ProcessVariableDeclaration(c, ((FixedStatementSyntax)c.Node).Declaration), SyntaxKind.FixedStatement);

            context.RegisterSyntaxNodeActionInNonGenerated(c => ProcessVariableDesignation(c, ((DeclarationPatternSyntaxWrapper)c.Node).Designation), SyntaxKindEx.DeclarationPattern);
            context.RegisterSyntaxNodeActionInNonGenerated(c => ProcessVariableDesignation(c, ((DeclarationExpressionSyntaxWrapper)c.Node).Designation), SyntaxKindEx.DeclarationExpression);
        }

        private static void ProcessVariableDesignation(SyntaxNodeAnalysisContext context, VariableDesignationSyntaxWrapper variableDesignation)
        {
            if (variableDesignation.AllVariables() is { Length: > 0 } variables
                && context.ContainingSymbol.ContainingType is { } containingType
                && GetMembers(containingType) is var members)
            {
                foreach (var variable in variables)
                {
                    ReportOnVariableMatchingField(context, members, variable.Identifier);
                }
            }
        }

        private static void ProcessVariableDeclaration(SyntaxNodeAnalysisContext context, VariableDeclarationSyntax variableDeclaration)
        {
            if (variableDeclaration is { Variables: { Count: > 0 } variables }
                && context.ContainingSymbol.ContainingType is { } containingType
                && GetMembers(containingType) is var members)
            {
                foreach (var variable in variables)
                {
                    ReportOnVariableMatchingField(context, members, variable.Identifier);
                }
            }
        }

        private static void ReportOnVariableMatchingField(SyntaxNodeAnalysisContext context, IEnumerable<ISymbol> members, SyntaxToken identifier)
        {
            if (members.FirstOrDefault(m => m.Name == identifier.ValueText) is { } matchingMember)
            {
                context.ReportIssue(Diagnostic.Create(Rule, identifier.GetLocation(), identifier.Text, matchingMember is IFieldSymbol ? "field" : "property"));
            }
        }

        private static List<ISymbol> GetMembers(INamespaceOrTypeSymbol classSymbol) =>
            classSymbol.GetMembers()
                       .Where(member => member is IFieldSymbol or IPropertySymbol)
                       .ToList();
    }
}

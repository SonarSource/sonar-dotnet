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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SetLocaleForDataTypes : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4057";
        private const string MessageFormat = "Set the locale for this '{0}'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        private static readonly ImmutableArray<KnownType> CheckedTypes = ImmutableArray.Create(
            KnownType.System_Data_DataTable,
            KnownType.System_Data_DataSet);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);
        protected override bool EnableConcurrentExecution => false;

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCompilationStartAction(
                compilationStartContext =>
                {
                    var symbolsWhereTypeIsCreated = new HashSet<NodeAndSymbol>();
                    var symbolsWhereLocaleIsSet = new HashSet<ISymbol>();

                    compilationStartContext.RegisterSyntaxNodeActionInNonGenerated(
                        ProcessObjectCreations(symbolsWhereTypeIsCreated),
                        SyntaxKind.ObjectCreationExpression,
                        SyntaxKindEx.ImplicitObjectCreationExpression);
                    compilationStartContext.RegisterSyntaxNodeActionInNonGenerated(ProcessSimpleAssignments(symbolsWhereLocaleIsSet), SyntaxKind.SimpleAssignmentExpression);
                    compilationStartContext.RegisterCompilationEndAction(ProcessCollectedSymbols(symbolsWhereTypeIsCreated, symbolsWhereLocaleIsSet));
                });

        private static Action<SyntaxNodeAnalysisContext> ProcessObjectCreations(ISet<NodeAndSymbol> symbolsWhereTypeIsCreated) =>
            c =>
            {
                if (GetSymbolFromConstructorInvocation(c.Node, c.SemanticModel) is ITypeSymbol objectType
                    && objectType.IsAny(CheckedTypes)
                    && GetAssignmentTargetVariable(c.Node) is { } variableSyntaxArray)
                {
                    var variableSymbols = variableSyntaxArray.Select(x => FindSymbol(x))
                        .OfType<ISymbol>()
                        .Where(x => x.GetSymbolType().IsAny(CheckedTypes))
                        .Select(x => new NodeAndSymbol(c.Node, x));
                    symbolsWhereTypeIsCreated.UnionWith(variableSymbols);
                }
                ISymbol FindSymbol(SyntaxNode node) =>
                node is IdentifierNameSyntax || DeclarationExpressionSyntaxWrapper.IsInstance(node)
                ? c.SemanticModel.GetSymbolInfo(node).Symbol
                : c.SemanticModel.GetDeclaredSymbol(node);
            };

        private static Action<SyntaxNodeAnalysisContext> ProcessSimpleAssignments(ISet<ISymbol> symbolsWhereLocaleIsSet) =>
            c =>
            {
                var assignmentExpression = (AssignmentExpressionSyntax)c.Node;

                foreach (var argument in assignmentExpression.AssignmentTargets())
                {
                    if (GetPropertySymbol(argument, c.SemanticModel) is { } propertySymbol
                        && propertySymbol.Name == "Locale"
                        && propertySymbol.ContainingType.IsAny(CheckedTypes))
                    {
                        var variableSymbol = GetAccessedVariable(argument, c.SemanticModel);
                        if (variableSymbol != null)
                        {
                            symbolsWhereLocaleIsSet.Add(variableSymbol);
                        }
                    }
                }
            };

        private static Action<CompilationAnalysisContext> ProcessCollectedSymbols(ICollection<NodeAndSymbol> symbolsWhereTypeIsCreated, ICollection<ISymbol> symbolsWhereLocaleIsSet) =>
            c =>
            {
                foreach (var invalidCreation in symbolsWhereTypeIsCreated.Where(x => !symbolsWhereLocaleIsSet.Contains(x.Symbol)))
                {
                    if (invalidCreation.Symbol.GetSymbolType()?.Name is { } typeName)
                    {
                        c.ReportIssue(Diagnostic.Create(Rule, invalidCreation.Node.GetLocation(), typeName));
                    }
                }
            };

        private static ISymbol GetSymbolFromConstructorInvocation(SyntaxNode constructorCall, SemanticModel semanticModel) =>
            constructorCall is ObjectCreationExpressionSyntax objectCreation
                ? semanticModel.GetSymbolInfo(objectCreation.Type).Symbol
                : semanticModel.GetSymbolInfo(constructorCall).Symbol?.ContainingType;

        private static ImmutableArray<SyntaxNode> GetAssignmentTargetVariable(SyntaxNode objectCreation) =>
            objectCreation.GetFirstNonParenthesizedParent() switch
            {
                AssignmentExpressionSyntax assignment => assignment.AssignmentTargets(),
                EqualsValueClauseSyntax equalsClause when equalsClause.Parent.Parent is VariableDeclarationSyntax variableDeclaration => ImmutableArray.Create((SyntaxNode)variableDeclaration.Variables.Last()),
                ArgumentSyntax argument when argument.Parent.Parent is AssignmentExpressionSyntax expressionSyntax => expressionSyntax.AssignmentTargets(),
                _ => ImmutableArray<SyntaxNode>.Empty
            };

        private static IPropertySymbol GetPropertySymbol(SyntaxNode node, SemanticModel model) =>
            model.GetSymbolInfo(node).Symbol as IPropertySymbol;

        private static ISymbol GetAccessedVariable(SyntaxNode node, SemanticModel model)
        {
            var variable = node.RemoveParentheses();

            if (variable is IdentifierNameSyntax identifier)
            {
                var leftSideOfParentAssignment = identifier
                    .FirstAncestorOrSelf((SyntaxNode x) => x.IsAnyKind(SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression))
                    ?.FirstAncestorOrSelf<AssignmentExpressionSyntax>()
                    ?.Left;
                if (leftSideOfParentAssignment != null)
                {
                    return model.GetSymbolInfo(leftSideOfParentAssignment).Symbol;
                }

                var lastVariable = identifier.FirstAncestorOrSelf<VariableDeclarationSyntax>()?.Variables.LastOrDefault();
                return lastVariable != null
                    ? model.GetDeclaredSymbol(lastVariable)
                    : null;
            }

            var memberAccess = variable as MemberAccessExpressionSyntax;

            return memberAccess?.Expression != null
                ? model.GetSymbolInfo(memberAccess.Expression).Symbol
                : null;
        }
    }
}

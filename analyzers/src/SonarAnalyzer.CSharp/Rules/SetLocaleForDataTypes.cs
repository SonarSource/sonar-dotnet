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
using System.Collections.Concurrent;
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
    [Rule(DiagnosticId)]
    public sealed class SetLocaleForDataTypes : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4057";
        private const string MessageFormat = "Set the locale for this '{0}'.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        private static readonly ImmutableArray<KnownType> CheckedTypes = ImmutableArray.Create(
            KnownType.System_Data_DataTable,
            KnownType.System_Data_DataSet);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCompilationStartAction(
                compilationStartContext =>
                {
                    var symbolsWhereTypeIsCreated = new ConcurrentBag<NodeAndSymbol>();
                    var symbolsWhereLocaleIsSet = new ConcurrentBag<ISymbol>();

                    compilationStartContext.RegisterSyntaxNodeActionInNonGenerated(ProcessObjectCreations(symbolsWhereTypeIsCreated),
                                                                                   SyntaxKind.ObjectCreationExpression,
                                                                                   SyntaxKindEx.ImplicitObjectCreationExpression);
                    compilationStartContext.RegisterSyntaxNodeActionInNonGenerated(ProcessSimpleAssignments(symbolsWhereLocaleIsSet), SyntaxKind.SimpleAssignmentExpression);
                    compilationStartContext.RegisterCompilationEndAction(ProcessCollectedSymbols(symbolsWhereTypeIsCreated, symbolsWhereLocaleIsSet));
                });

        private static Action<SyntaxNodeAnalysisContext> ProcessObjectCreations(ConcurrentBag<NodeAndSymbol> symbolsWhereTypeIsCreated) =>
            c =>
            {
                if (GetSymbolFromConstructorInvocation(c.Node, c.SemanticModel) is ITypeSymbol objectType
                    && objectType.IsAny(CheckedTypes)
                    && GetAssignmentTargetVariable(c.Node) is { } variableSyntax)
                {
                    var variableSymbol = variableSyntax is IdentifierNameSyntax
                        ? c.SemanticModel.GetSymbolInfo(variableSyntax).Symbol
                        : c.SemanticModel.GetDeclaredSymbol(variableSyntax);
                    if (variableSymbol != null)
                    {
                        symbolsWhereTypeIsCreated.Add(new NodeAndSymbol(c.Node, variableSymbol));
                    }
                }
            };

        private static Action<SyntaxNodeAnalysisContext> ProcessSimpleAssignments(ConcurrentBag<ISymbol> symbolsWhereLocaleIsSet) =>
            c =>
            {
                var assignmentExpression = (AssignmentExpressionSyntax)c.Node;

                if (GetPropertySymbol(assignmentExpression, c.SemanticModel) is { } propertySymbol
                    && propertySymbol.ContainingType.IsAny(CheckedTypes)
                    && propertySymbol.Name == "Locale")
                {
                    var variableSymbol = GetAccessedVariable(assignmentExpression, c.SemanticModel);
                    if (variableSymbol != null)
                    {
                        symbolsWhereLocaleIsSet.Add(variableSymbol);
                    }
                }
            };

        private static Action<CompilationAnalysisContext> ProcessCollectedSymbols(ConcurrentBag<NodeAndSymbol> symbolsWhereTypeIsCreated,
                                                                                  ConcurrentBag<ISymbol> symbolsWhereLocaleIsSet) =>
            c =>
            {
                foreach (var invalidCreation in symbolsWhereTypeIsCreated.Where(x => !symbolsWhereLocaleIsSet.Contains(x.Symbol)))
                {
                    if (invalidCreation.Symbol.GetSymbolType()?.Name is { }  typeName)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, invalidCreation.Node.GetLocation(), typeName));
                    }
                }
            };

        private static ISymbol GetSymbolFromConstructorInvocation(SyntaxNode constructorCall, SemanticModel semanticModel) =>
            constructorCall is ObjectCreationExpressionSyntax objectCreation
                ? semanticModel.GetSymbolInfo(objectCreation.Type).Symbol
                : semanticModel.GetSymbolInfo(constructorCall).Symbol?.ContainingType;

        private static SyntaxNode GetAssignmentTargetVariable(SyntaxNode objectCreation) =>
            objectCreation.GetFirstNonParenthesizedParent() switch
            {
                AssignmentExpressionSyntax assignment => assignment.Left,
                EqualsValueClauseSyntax equalsClause when equalsClause.Parent.Parent is VariableDeclarationSyntax variableDeclaration => variableDeclaration.Variables.Last(),
                _ => null
            };

        private static IPropertySymbol GetPropertySymbol(AssignmentExpressionSyntax assignment, SemanticModel model) =>
            model.GetSymbolInfo(assignment.Left).Symbol as IPropertySymbol;

        private static ISymbol GetAccessedVariable(AssignmentExpressionSyntax assignment, SemanticModel model)
        {
            var variable = assignment.Left.RemoveParentheses();

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

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
                    && GetAssignmentTargetVariable(c.Node) is { } variableSyntax)
                {
                    var variableSymbol = variableSyntax is IdentifierNameSyntax
                                         || DeclarationExpressionSyntaxWrapper.IsInstance(variableSyntax)
                        ? c.SemanticModel.GetSymbolInfo(variableSyntax).Symbol
                        : c.SemanticModel.GetDeclaredSymbol(variableSyntax);
                    if (variableSymbol != null)
                    {
                        symbolsWhereTypeIsCreated.Add(new NodeAndSymbol(c.Node, variableSymbol));
                    }
                }
            };

        private static Action<SyntaxNodeAnalysisContext> ProcessSimpleAssignments(ISet<ISymbol> symbolsWhereLocaleIsSet) =>
            c =>
            {
                var assignmentExpression = (AssignmentExpressionSyntax)c.Node;
                ProcessExpression(assignmentExpression.Left, c.SemanticModel, symbolsWhereLocaleIsSet);
            };

        private static void ProcessExpression(ExpressionSyntax expression, SemanticModel semanticModel, ISet<ISymbol> symbolsWhereLocaleIsSet)
        {
            expression = expression.RemoveParentheses();
            if (TupleExpressionSyntaxWrapper.IsInstance(expression)
                && ((TupleExpressionSyntaxWrapper)expression) is var tupleExpression)
            {
                foreach (var argument in tupleExpression.Arguments)
                {
                    ProcessExpression(argument.Expression, semanticModel, symbolsWhereLocaleIsSet);
                }
            }
            else
            {
                if (GetPropertySymbol(expression, semanticModel) is { } propertySymbol
                            && propertySymbol.Name == "Locale"
                            && propertySymbol.ContainingType.IsAny(CheckedTypes))
                {
                    var variableSymbol = GetAccessedVariable(expression, semanticModel);
                    if (variableSymbol != null)
                    {
                        symbolsWhereLocaleIsSet.Add(variableSymbol);
                    }
                }
            }
        }

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

        private static SyntaxNode GetAssignmentTargetVariable(SyntaxNode objectCreation) =>
            objectCreation.GetFirstNonParenthesizedParent() switch
            {
                AssignmentExpressionSyntax assignment => assignment.Left,
                EqualsValueClauseSyntax equalsClause when equalsClause.Parent.Parent is VariableDeclarationSyntax variableDeclaration => variableDeclaration.Variables.Last(),
                ArgumentSyntax argument => HandleArgumentSyntax(argument),
                _ => null
            };

        private static SyntaxNode HandleArgumentSyntax(ArgumentSyntax argument)
        {
            if (TupleExpressionSyntaxWrapper.IsInstance(argument.Parent)
                && ((TupleExpressionSyntaxWrapper)argument.Parent) is var rightTuple)
            {
                var currentNode = rightTuple.SyntaxNode.Parent;
                AssignmentExpressionSyntax assignment = null;
                while (currentNode != null && assignment == null)
                {
                    if (currentNode is AssignmentExpressionSyntax syntax)
                    {
                        assignment = syntax;
                    }
                    else
                    {
                        currentNode = currentNode.Parent;
                    }
                }
                return FindAssignedSyntaxNodeInAssignment(argument, assignment);
            }
            return null;
        }

        private static SyntaxNode FindAssignedSyntaxNodeInAssignment(ArgumentSyntax argument, AssignmentExpressionSyntax assignment)
        {
            if (assignment == null)
            {
                return null;
            }

            var mappedAssignments = MapAssignmentArguments(assignment);
            foreach (var mappedAssignment in mappedAssignments)
            {
                if (mappedAssignment.Value == argument.Expression)
                {
                    return mappedAssignment.Key;
                }
            }
            return null;
        }

        /// <summary>
        /// Maps the left and the right side arguments of an <paramref name="assignment"/>. If both sides are tuples, the tuple elements are mapped.
        /// <code>
        /// (var x, var y) = (1, 2);                 // [x→1, y→2]
        /// (var x, (var y, var z)) = (1, (2, 3));   // [x→1, y→2, z→3]
        /// var x = 1;                               // [x→1]
        /// (var x, var y) = M();                    // [(x,y)→M()]
        /// </code>
        /// </summary>
        /// <param name="assignment">The <paramref name="assignment"/> expression.</param>
        /// <returns>A mapping from expressions on the left side of the <paramref name="assignment"/> to the right side.</returns>
        public static ImmutableArray<KeyValuePair<ExpressionSyntax, ExpressionSyntax>> MapAssignmentArguments(AssignmentExpressionSyntax assignment)
        {
            if (TupleExpressionSyntaxWrapper.IsInstance(assignment.Left)
                && TupleExpressionSyntaxWrapper.IsInstance(assignment.Right))
            {
                var builder = ImmutableArray.CreateBuilder<KeyValuePair<ExpressionSyntax, ExpressionSyntax>>();
                AssignTupleElements(builder, (TupleExpressionSyntaxWrapper)assignment.Left, (TupleExpressionSyntaxWrapper)assignment.Right);
                return builder.ToImmutableArray();
            }
            else
            {
                return ImmutableArray.Create(new KeyValuePair<ExpressionSyntax, ExpressionSyntax>(assignment.Left, assignment.Right));
            }

            static void AssignTupleElements(ImmutableArray<KeyValuePair<ExpressionSyntax, ExpressionSyntax>>.Builder builder,
                                            TupleExpressionSyntaxWrapper left,
                                            TupleExpressionSyntaxWrapper right)
            {
                var leftEnum = left.Arguments.GetEnumerator();
                var rightEnum = right.Arguments.GetEnumerator();
                while (leftEnum.MoveNext() && rightEnum.MoveNext())
                {
                    var leftArg = leftEnum.Current;
                    var rightArg = rightEnum.Current;
                    if (leftArg is ArgumentSyntax { Expression: { } leftExpression } && TupleExpressionSyntaxWrapper.IsInstance(leftExpression)
                        && rightArg is ArgumentSyntax { Expression: { } rightExpression } && TupleExpressionSyntaxWrapper.IsInstance(rightExpression))
                    {
                        AssignTupleElements(builder, (TupleExpressionSyntaxWrapper)leftExpression, (TupleExpressionSyntaxWrapper)rightExpression);
                    }
                    else
                    {
                        builder.Add(new KeyValuePair<ExpressionSyntax, ExpressionSyntax>(leftArg.Expression, rightArg.Expression));
                    }
                }
            }
        }

        private static IPropertySymbol GetPropertySymbol(ExpressionSyntax expression, SemanticModel model) =>
            model.GetSymbolInfo(expression).Symbol as IPropertySymbol;

        private static ISymbol GetAccessedVariable(ExpressionSyntax expression, SemanticModel model)
        {
            var variable = expression.RemoveParentheses();

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

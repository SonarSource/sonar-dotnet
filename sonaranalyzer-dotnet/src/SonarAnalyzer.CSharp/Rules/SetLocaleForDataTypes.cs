/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class SetLocaleForDataTypes : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4057";
        private const string MessageFormat = "Set the locale for this '{0}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<KnownType> invalidTypes = new HashSet<KnownType>
        {
            KnownType.System_Data_DataTable,
            KnownType.System_Data_DataSet
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCompilationStartAction(
                compilationStartContext =>
                {
                    var symbolsWhereTypeIsCreated =
                        new ConcurrentBag<SyntaxNodeWithSymbol<ObjectCreationExpressionSyntax, ISymbol>>();
                    var symbolsWhereLocaleIsSet = new ConcurrentBag<ISymbol>();

                    compilationStartContext.RegisterSyntaxNodeActionInNonGenerated(
                        c =>
                        {
                            var objectCreation = (ObjectCreationExpressionSyntax)c.Node;
                            var objectType = c.SemanticModel.GetSymbolInfo(objectCreation.Type).Symbol as ITypeSymbol;

                            if (objectType == null ||
                                !objectType.IsAny(invalidTypes))
                            {
                                return;
                            }

                            var variableSyntax = GetAssignedToSyntax(objectCreation);
                            if (variableSyntax == null)
                            {
                                return;
                            }

                            var variableSymbol = variableSyntax is IdentifierNameSyntax
                                ? c.SemanticModel.GetSymbolInfo(variableSyntax).Symbol
                                : c.SemanticModel.GetDeclaredSymbol(variableSyntax);
                            if (variableSymbol != null)
                            {
                                symbolsWhereTypeIsCreated.Add(variableSymbol.ToSymbolWithSyntax(objectCreation));
                            }
                        }, SyntaxKind.ObjectCreationExpression);

                    compilationStartContext.RegisterSyntaxNodeActionInNonGenerated(
                        c =>
                        {
                            var assignmentExpression = (AssignmentExpressionSyntax)c.Node;
                            var propertySymbol = GetPropertySymbol(assignmentExpression, c.SemanticModel);

                            if (propertySymbol == null ||
                                !propertySymbol.ContainingType.IsAny(invalidTypes) ||
                                propertySymbol.Name != "Locale")
                            {
                                return;
                            }

                            var variableSymbol = GetAccessedVariable(assignmentExpression, c.SemanticModel);
                            if (variableSymbol != null)
                            {
                                symbolsWhereLocaleIsSet.Add(variableSymbol);
                            }
                        }, SyntaxKind.SimpleAssignmentExpression);

                    compilationStartContext.RegisterCompilationEndAction(
                        c =>
                        {
                            var invalidDataTypeCreation = symbolsWhereTypeIsCreated
                                .Where(x => !symbolsWhereLocaleIsSet.Contains(x.Symbol));

                            foreach (var invalidCreation in invalidDataTypeCreation)
                            {
                                var typeName = GetConstructedTypeName(invalidCreation.Symbol);
                                if (typeName == null)
                                {
                                    continue;
                                }

                                c.ReportDiagnosticIfNonGenerated(Diagnostic.Create(rule,
                                    invalidCreation.Syntax.GetLocation(), typeName), c.Compilation);
                            }
                        });
                });
        }

        private static SyntaxNode GetAssignedToSyntax(ObjectCreationExpressionSyntax objectCreation)
        {
            var parent = objectCreation.GetSelfOrTopParenthesizedExpression().Parent;

            var assignment = parent as AssignmentExpressionSyntax;
            if (assignment != null)
            {
                return assignment.Left;
            }

            var equalsClause = parent as EqualsValueClauseSyntax;
            if (equalsClause != null)
            {
                var variableDeclaration = equalsClause.Parent.Parent as VariableDeclarationSyntax;
                if (variableDeclaration != null)
                {
                    return variableDeclaration.Variables.Last();
                }
            }

            return null;
        }

        private static IPropertySymbol GetPropertySymbol(AssignmentExpressionSyntax assignment, SemanticModel model)
        {
            var syntax = assignment.Left;

            var conditionalAccess = assignment.Left as ConditionalAccessExpressionSyntax;
            if (conditionalAccess != null)
            {
                syntax = conditionalAccess.WhenNotNull;
            }

            return model.GetSymbolInfo(syntax).Symbol as IPropertySymbol;
        }

        private static ISymbol GetAccessedVariable(AssignmentExpressionSyntax assignment, SemanticModel model)
        {
            var variable = assignment.Left.RemoveParentheses();

            var identifier = variable as IdentifierNameSyntax;
            if (identifier != null)
            {
                var leftSideOfParentAssignment = identifier
                    .FirstAncestorOrSelf<ObjectCreationExpressionSyntax>()
                    ?.FirstAncestorOrSelf<AssignmentExpressionSyntax>()
                    ?.Left;
                if (leftSideOfParentAssignment != null)
                {
                    return model.GetSymbolInfo(leftSideOfParentAssignment).Symbol;
                }

                var lastVariable = identifier.FirstAncestorOrSelf<VariableDeclarationSyntax>()?.Variables.LastOrDefault();
                if (lastVariable == null)
                {
                    return null;
                }

                return model.GetDeclaredSymbol(lastVariable);
            }

            var memberAccess = variable as MemberAccessExpressionSyntax;
            if (memberAccess != null)
            {
                if (memberAccess.Expression == null)
                {
                    return null;
                }

                return model.GetSymbolInfo(memberAccess.Expression).Symbol;
            }

            var conditionalAccess = variable as ConditionalAccessExpressionSyntax;
            if (conditionalAccess != null)
            {
                if (conditionalAccess.Expression == null)
                {
                    return null;
                }

                return model.GetSymbolInfo(conditionalAccess.Expression).Symbol;
            }

            return null;
        }

        private static string GetConstructedTypeName(ISymbol symbol)
        {
            var localSymbol = symbol as ILocalSymbol;
            if (localSymbol != null)
            {
                return localSymbol.Type.Name;
            }

            var fieldSymbol = symbol as IFieldSymbol;
            if (fieldSymbol != null)
            {
                return fieldSymbol.Type.Name;
            }

            return null;
        }
    }
}
